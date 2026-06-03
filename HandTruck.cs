using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandTruck : VehichleInteractable
{
	[SerializeField]
	private PlayerMove playerMove;

	[SerializeField]
	private PlayerLook playerLook;

	[SerializeField]
	private Outline outline;

	[SerializeField]
	private Transform boxStartTransform;

	[SerializeField]
	private Transform playerSitTransform;

	[SerializeField]
	private Transform handTruckParent;

	[SerializeField]
	private Transform wheels;

	private const float MAXIMUM_HEIGHT = 1.6f;

	private const string HANDTRUCK_POSITION_KEY = "HTRotation";

	private const string HANDTRUCK_ROTATION_KEY = "HTPosition";

	private const string HANDTRUCK_BOX_LIST_KEY = "HTBoxList";

	private List<int> containedBoxIDs = new List<int>();

	private Vector3 pickedHandtruckRotation = new Vector3(-30f, 0f, 0f);

	private float currentHeight;

	private int placeableLayer = 6;

	private int animatingVehicleLayer = 22;

	private bool unloading;

	private bool loadingBoxes;

	private Tweener leaveTween;

	public void DeleteAllBoxes()
	{
		containedBoxIDs.Clear();
		currentHeight = 0f;
		SaveContent();
	}

	protected override void Awake()
	{
		base.Awake();
		EventManager.AddListener(StartupEvents.BOXES_INITIALIZED, Initialize);
		EventManager.AddListener(PlaceableEvents.PLACE_PRODUCT, OnPlaceProductByHand);
		EventManager.AddListener(GameEvents.BOX_PICK_STARTED, OnBoxPickStarted);
		EventManager.AddListener<Box>(GameEvents.VEHICLE_BOX_PICK_STARTED, OnBoxPickStartedVehicle);
		EventManager.AddListener<int>(GameEvents.BOX_DELETED, OnBoxDeleted);
	}

	private void Start()
	{
		if (!SingletonBehaviour<VehicleManager>.Instance.IsVehiclePurchased(base.VehicleType, base.VehicleID))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnBoxDeleted(int boxId)
	{
		if (containedBoxIDs.Contains(boxId))
		{
			RemoveBox(SingletonBehaviour<BoxManager>.Instance.GetBox(boxId));
		}
	}

	public void Initialize()
	{
		containedBoxIDs = GenericDataSerializer.Load("HTBoxList", new List<int>());
		Vector3 position = GenericDataSerializer.Load(GetPositionKey(), base.transform.position);
		if (position.z > 0f)
		{
			base.transform.position = position;
		}
		base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, GenericDataSerializer.LoadFloat(GetRotationKey(), base.transform.localEulerAngles.y), base.transform.localEulerAngles.z);
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] != -1)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
				if (box != null)
				{
					Vector3 localPosition = boxStartTransform.localPosition + currentHeight * Vector3.up;
					localPosition += box.GetWidth() / 2f * Vector3.forward;
					currentHeight += box.GetHeight();
					box.transform.SetParent(base.transform);
					box.transform.localEulerAngles = boxStartTransform.localEulerAngles;
					box.transform.localPosition = localPosition;
					box.UnregisterFromRestockZone();
				}
			}
		}
	}

	private void OnClicked()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked || unloading || loadingBoxes || DOTween.IsTweening(base.transform))
		{
			return;
		}
		EventManager.NotifyEvent(GameEvents.VEHICLE_TAKEN, (VehichleInteractable)this);
		playerLook.RotationLocked = true;
		playerMove.MovementLocked = true;
		playerMove.transform.DOMove(new Vector3(playerSitTransform.position.x, playerSitTransform.position.y, playerSitTransform.position.z), 0.2f).OnComplete(delegate
		{
			isDriving = true;
			playerLook.RotationLocked = false;
			playerMove.MovementLocked = false;
			base.transform.SetParent(handTruckParent);
			base.transform.DOLocalMove(Vector3.zero, 0.2f);
			base.transform.DOLocalRotate(pickedHandtruckRotation, 0.2f).OnComplete(delegate
			{
				RefreshButtons();
			});
			if (outline.enabled)
			{
				outline.enabled = false;
			}
			if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
			{
				SingletonBehaviour<TooltipUI>.Instance.Close();
			}
		});
		playerLook.CameraParent.DORotate(Vector3.up * base.transform.localEulerAngles.y, 0.2f);
		playerLook.transform.DOLocalRotate(Vector3.zero, 0.2f).OnComplete(delegate
		{
			playerLook.UpdateClamp(0f);
		});
		for (int num = 0; num < containedBoxIDs.Count; num++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null)
			{
				box.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
			}
		}
	}

	private void Leave()
	{
		if (unloading || (leaveTween != null && leaveTween.IsActive()))
		{
			return;
		}
		isDriving = false;
		base.gameObject.layer = animatingVehicleLayer;
		EventManager.NotifyEvent(GameEvents.VEHICLE_RELEASED);
		base.transform.SetParent(null);
		leaveTween = base.transform.DORotate(new Vector3(0f, base.transform.localEulerAngles.y, base.transform.localEulerAngles.z), 0.2f).OnComplete(delegate
		{
			base.gameObject.layer = placeableLayer;
			playerLook.UpdateClamp(0f);
		});
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		StopAnimation();
		for (int num = 0; num < containedBoxIDs.Count; num++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null)
			{
				box.Collider.gameObject.layer = BoxManager.BOX_LAYER;
			}
		}
		SaveLocation();
	}

	private void UnloadBoxes()
	{
		if (unloading || DOTween.IsTweening(base.transform))
		{
			return;
		}
		unloading = true;
		playerMove.MovementLocked = true;
		playerLook.RotationLocked = true;
		base.transform.DOLocalMove(base.transform.localPosition + Vector3.forward * 0.15f, 0.4f).SetLoops(2, LoopType.Yoyo);
		base.transform.DOLocalRotate(new Vector3(-5f, base.transform.localEulerAngles.y, base.transform.localEulerAngles.z), 0.4f).OnComplete(delegate
		{
			base.transform.DOLocalRotate(pickedHandtruckRotation, 0.4f).SetDelay(0.35f).OnComplete(delegate
			{
				playerMove.MovementLocked = false;
				playerLook.RotationLocked = false;
				unloading = false;
			});
			for (int num = 0; num < containedBoxIDs.Count; num++)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
				if (box != null)
				{
					box.RigidBody.constraints = RigidbodyConstraints.None;
					box.gameObject.layer = BoxManager.BOX_LAYER;
					box.transform.SetParent(null);
					box.OnBoxRemovedFromVehicle();
				}
			}
			containedBoxIDs.Clear();
			RefreshButtons();
			currentHeight = 0f;
			SaveContent();
		});
	}

	public override void RefreshButtons()
	{
		if (containedBoxIDs.Count == 0)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					Leave();
				})
			} });
			return;
		}
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
		{
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					Leave();
				})
			},
			{
				KeyCode.Mouse1,
				("unload", delegate
				{
					UnloadBoxes();
				})
			}
		});
	}

	public override void RefreshButtonsBoxHovered()
	{
		base.RefreshButtonsBoxHovered();
		if (containedBoxIDs.Count == 0)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("take", null)
				},
				{
					SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
					("leave", delegate
					{
						Leave();
					})
				}
			});
			return;
		}
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
		{
			{
				KeyCode.Mouse0,
				("take", null)
			},
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					Leave();
				})
			},
			{
				KeyCode.Mouse1,
				("unload", delegate
				{
					UnloadBoxes();
				})
			}
		});
	}

	public override void RefreshButtonsPalletHovered(Action palletTakeAction)
	{
		base.RefreshButtonsBoxHovered();
		if (containedBoxIDs.Count == 0)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("take", palletTakeAction)
				},
				{
					SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
					("leave", delegate
					{
						Leave();
					})
				}
			});
			return;
		}
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
		{
			{
				KeyCode.Mouse0,
				("take", palletTakeAction)
			},
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					Leave();
				})
			},
			{
				KeyCode.Mouse1,
				("unload", delegate
				{
					UnloadBoxes();
				})
			}
		});
	}

	public int GetProductCount()
	{
		return containedBoxIDs.Count;
	}

	private void RemoveBox(Box box)
	{
		currentHeight -= box.GetHeight();
		int num = containedBoxIDs.IndexOf(box.BoxID);
		containedBoxIDs.Remove(box.BoxID);
		SaveContent();
		float height = box.GetHeight();
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (i >= num)
			{
				Box box2 = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
				if (box2 != null)
				{
					box2.transform.DOLocalMove(box2.transform.localPosition - Vector3.up * height, 0.2f);
				}
			}
		}
	}

	private void OnPlaceProductByHand()
	{
		if (!outline.enabled)
		{
			return;
		}
		Box box = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (unloading || containedBoxIDs.Contains(box.BoxID) || SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			return;
		}
		box.GetContainedProduct();
		int productCount = GetProductCount();
		if (currentHeight + box.GetHeight() < 1.6f)
		{
			if (productCount == 0)
			{
				containedBoxIDs = new List<int>();
			}
			Vector3 to = boxStartTransform.localPosition + currentHeight * Vector3.up;
			to += box.GetWidth() / 2f * Vector3.forward;
			currentHeight += box.GetHeight();
			SingletonBehaviour<BoxManager>.Instance.OnBoxPutWithoutThrow();
			if (box.IsOpen())
			{
				box.Close();
			}
			box.transform.DOKill();
			box.gameObject.layer = BoxManager.NOT_COLLIDING_BOX_LAYER;
			loadingBoxes = true;
			box.transform.SetParent(base.transform);
			box.transform.DOLocalRotate(boxStartTransform.localEulerAngles, 0.3f);
			box.transform.DoCurvedLocalMove(to, 0.3f, 2f).OnComplete(delegate
			{
				box.OnBoxPut();
				loadingBoxes = false;
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
			});
			HapticController.Vibrate(PresetType.LightImpact);
			containedBoxIDs.Add(box.BoxID);
			SaveContent();
			CloseInteractionElements();
			SingletonBehaviour<HotKeyManager>.Instance.RefreshEnablity();
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_handtruck_full", base.transform);
		}
	}

	public override bool PlaceBoxByClick(Box box)
	{
		if (unloading || containedBoxIDs.Contains(box.BoxID))
		{
			return false;
		}
		int productCount = GetProductCount();
		if (currentHeight + box.GetHeight() < 1.6f)
		{
			if (productCount == 0)
			{
				containedBoxIDs = new List<int>();
			}
			Vector3 to = boxStartTransform.localPosition + currentHeight * Vector3.up;
			to += box.GetWidth() / 2f * Vector3.forward;
			currentHeight += box.GetHeight();
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_THROW);
			if (box.IsOpen())
			{
				box.Close();
			}
			EventManager.NotifyEvent(GameEvents.VEHICLE_BOX_PICK_STARTED, box);
			box.transform.SetParent(base.transform);
			box.transform.DOLocalRotate(boxStartTransform.localEulerAngles, 0.3f);
			box.transform.DoCurvedLocalMove(to, 0.3f, 2f).OnComplete(delegate
			{
				if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
				{
					box.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
				}
				box.SetBoxStored();
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
				EventManager.NotifyEvent(GameEvents.VEHICLE_BOX_PICKED_UP, box);
			});
			box.gameObject.layer = BoxManager.NOT_COLLIDING_BOX_LAYER;
			SingletonBehaviour<BoxManager>.Instance.WakeUpBoxesAbove(box);
			HapticController.Vibrate(PresetType.LightImpact);
			box.OnMouseHoverEnded();
			containedBoxIDs.Add(box.BoxID);
			RefreshButtons();
			SaveContent();
			CloseInteractionElements();
			return true;
		}
		SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_handtruck_full", base.transform);
		return false;
	}

	private void CloseInteractionElements()
	{
		if (outline.enabled)
		{
			outline.enabled = false;
		}
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		FireExtinguisher.Instance.UpdateMenu();
		GenericBox.Instance.UpdateMenu();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void OnBoxPickStarted()
	{
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (containedBoxIDs.Contains(pickedBox.BoxID))
		{
			RemoveBox(pickedBox);
		}
	}

	private void OnBoxPickStartedVehicle(Box box)
	{
		if (containedBoxIDs.Contains(box.BoxID))
		{
			box.transform.DOKill();
			RemoveBox(box);
		}
	}

	private void SaveContent()
	{
		GenericDataSerializer.Save("HTBoxList", containedBoxIDs);
	}

	public override void OnMouseHoverStarted()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			outline.enabled = true;
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuForStorageShelf();
		}
		else if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !FireExtinguisher.Instance.IsPicked && !SingletonBehaviour<TrayManager>.Instance.IsPicked && !GenericBox.Instance.IsPicked)
		{
			outline.enabled = true;
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("use", delegate
				{
					OnClicked();
				})
			} }, base.transform);
		}
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (!SingletonBehaviour<TrayManager>.Instance.IsPicked && !SingletonBehaviour<VehicleManager>.Instance.IsOnHandTruck)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		CloseInteractionElements();
	}

	public override void StartAnimation()
	{
		if (!DOTween.IsTweening(wheels))
		{
			wheels.DOLocalRotate(new Vector3(360f, 0f, 0f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
		}
	}

	public override void StopAnimation()
	{
		base.StopAnimation();
		wheels.DOKill();
		wheels.localEulerAngles = Vector3.zero;
	}

	public override bool IsAnimating()
	{
		return DOTween.IsTweening(wheels);
	}

	public override bool ContainsBox(int id)
	{
		return containedBoxIDs.Contains(id);
	}

	public override LayerMask GetInteractableLayers()
	{
		int num = 1 << RayShooter.PICKABLE_LAYER;
		int num2 = 1 << RayShooter.VEHICLE_CLICKABLE_LAYER;
		int num3 = 1 << RayShooter.PALLET_LAYER;
		return num | num2 | num3;
	}

	public override bool CanSell()
	{
		bool num = containedBoxIDs.Count == 0;
		if (!num)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("cant_sell_handtruck", base.transform);
		}
		return num;
	}
}
