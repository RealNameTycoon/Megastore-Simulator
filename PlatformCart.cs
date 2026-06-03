using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlatformCart : VehichleInteractable
{
	private enum BoxSlot
	{
		FirstDualSlot,
		SecondDualSlot,
		CloserLeft,
		CloserRight,
		FartherLeft,
		FartherRight
	}

	[SerializeField]
	private Outline outline;

	[SerializeField]
	private Transform boxStartTransformCloserLeft;

	[SerializeField]
	private Transform boxStartTransformCloserRight;

	[SerializeField]
	private Transform boxStartTransformFartherLeft;

	[SerializeField]
	private Transform boxStartTransformFartherRight;

	[SerializeField]
	private Transform boxStartTransformFirstDualSlot;

	[SerializeField]
	private Transform boxStartTransformSecondDualSlot;

	[SerializeField]
	private Transform playerSitTransform;

	[SerializeField]
	private Transform platformCartParent;

	[SerializeField]
	private BoxCollider platformCartTrigger;

	private const float MAXIMUM_HEIGHT = 1.6f;

	private const string PLATFORM_CART_BOX_LIST_KEY = "PLATFORM_CART_BOX_LIST_KEY";

	[SerializeField]
	private List<int> containedBoxIDsCloserLeft = new List<int>();

	[SerializeField]
	private List<int> containedBoxIDsCloserRight = new List<int>();

	[SerializeField]
	private List<int> containedBoxIDsFartherLeft = new List<int>();

	[SerializeField]
	private List<int> containedBoxIDsFartherRight = new List<int>();

	[SerializeField]
	private List<int> firstDualSlotBoxIDs = new List<int>();

	[SerializeField]
	private List<int> secondDualSlotBoxIDs = new List<int>();

	private Vector3 pickedHandtruckRotation = new Vector3(0f, 0f, 0f);

	private float currentHeightCloserLeft;

	private float currentHeightCloserRight;

	private float currentHeightFartherLeft;

	private float currentHeightFartherRight;

	private float currentHeightFirstDualSlot;

	private float currentHeightSecondDualSlot;

	private int placeableLayer = 6;

	private int animatingVehicleLayer = 22;

	private bool unloading;

	private bool loadingBoxes;

	private Tweener leaveTween;

	private List<int> GetContainedBoxes(BoxSlot slot)
	{
		return slot switch
		{
			BoxSlot.CloserLeft => containedBoxIDsCloserLeft, 
			BoxSlot.CloserRight => containedBoxIDsCloserRight, 
			BoxSlot.FartherLeft => containedBoxIDsFartherLeft, 
			BoxSlot.FartherRight => containedBoxIDsFartherRight, 
			BoxSlot.FirstDualSlot => firstDualSlotBoxIDs, 
			BoxSlot.SecondDualSlot => secondDualSlotBoxIDs, 
			_ => new List<int>(), 
		};
	}

	private float GetHeight(BoxSlot slot)
	{
		return slot switch
		{
			BoxSlot.CloserLeft => currentHeightCloserLeft, 
			BoxSlot.CloserRight => currentHeightCloserRight, 
			BoxSlot.FartherLeft => currentHeightFartherLeft, 
			BoxSlot.FartherRight => currentHeightFartherRight, 
			BoxSlot.FirstDualSlot => currentHeightFirstDualSlot, 
			BoxSlot.SecondDualSlot => currentHeightSecondDualSlot, 
			_ => 0f, 
		};
	}

	private void SetHeight(BoxSlot slot, float height)
	{
		switch (slot)
		{
		case BoxSlot.CloserLeft:
			currentHeightCloserLeft = height;
			break;
		case BoxSlot.CloserRight:
			currentHeightCloserRight = height;
			break;
		case BoxSlot.FartherLeft:
			currentHeightFartherLeft = height;
			break;
		case BoxSlot.FartherRight:
			currentHeightFartherRight = height;
			break;
		case BoxSlot.FirstDualSlot:
			currentHeightFirstDualSlot = height;
			break;
		case BoxSlot.SecondDualSlot:
			currentHeightSecondDualSlot = height;
			break;
		}
	}

	private Transform GetBoxStartTransform(BoxSlot slot)
	{
		return slot switch
		{
			BoxSlot.CloserLeft => boxStartTransformCloserLeft, 
			BoxSlot.CloserRight => boxStartTransformCloserRight, 
			BoxSlot.FartherLeft => boxStartTransformFartherLeft, 
			BoxSlot.FartherRight => boxStartTransformFartherRight, 
			BoxSlot.FirstDualSlot => boxStartTransformFirstDualSlot, 
			BoxSlot.SecondDualSlot => boxStartTransformSecondDualSlot, 
			_ => null, 
		};
	}

	public void DeleteAllBoxes()
	{
		containedBoxIDsCloserLeft.Clear();
		containedBoxIDsCloserRight.Clear();
		containedBoxIDsFartherLeft.Clear();
		containedBoxIDsFartherRight.Clear();
		firstDualSlotBoxIDs.Clear();
		secondDualSlotBoxIDs.Clear();
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
		if (IsBoxContained(boxId))
		{
			RemoveBox(SingletonBehaviour<BoxManager>.Instance.GetBox(boxId));
		}
	}

	public void Initialize()
	{
		containedBoxIDsCloserLeft = GenericDataSerializer.Load("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.CloserLeft, new List<int>());
		containedBoxIDsCloserRight = GenericDataSerializer.Load("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.CloserRight, new List<int>());
		containedBoxIDsFartherLeft = GenericDataSerializer.Load("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.FartherLeft, new List<int>());
		containedBoxIDsFartherRight = GenericDataSerializer.Load("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.FartherRight, new List<int>());
		firstDualSlotBoxIDs = GenericDataSerializer.Load("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.FirstDualSlot, new List<int>());
		secondDualSlotBoxIDs = GenericDataSerializer.Load("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.SecondDualSlot, new List<int>());
		Vector3 position = GenericDataSerializer.Load(GetPositionKey(), base.transform.position);
		if (position.z > 0f)
		{
			base.transform.position = position;
		}
		base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, GenericDataSerializer.LoadFloat(GetRotationKey(), base.transform.localEulerAngles.y), base.transform.localEulerAngles.z);
		for (int i = 0; i < Enum.GetValues(typeof(BoxSlot)).Length; i++)
		{
			for (int j = 0; j < GetContainedBoxes((BoxSlot)i).Count; j++)
			{
				BoxSlot boxSlot = (BoxSlot)i;
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(GetContainedBoxes(boxSlot)[j]);
				if (box != null)
				{
					box.transform.SetParent(base.transform);
					Transform boxStartTransform = GetBoxStartTransform(boxSlot);
					float num = GetHeight(boxSlot);
					switch (boxSlot)
					{
					case BoxSlot.CloserLeft:
					case BoxSlot.CloserRight:
						num += GetHeight(BoxSlot.FirstDualSlot);
						break;
					case BoxSlot.FartherLeft:
					case BoxSlot.FartherRight:
						num += GetHeight(BoxSlot.SecondDualSlot);
						break;
					}
					Vector3 localPosition = boxStartTransform.localPosition + num * Vector3.up;
					SetHeight(boxSlot, GetHeight(boxSlot) + box.GetHeight());
					box.transform.localPosition = localPosition;
					box.transform.localEulerAngles = GetBoxRotation(box.Type);
					box.UnregisterFromRestockZone();
				}
			}
		}
	}

	private Vector3 GetBoxRotation(BoxType boxType)
	{
		if (boxType == BoxType.WIDE || boxType == BoxType.XL_BOX)
		{
			return new Vector3(0f, 90f, 0f);
		}
		return Vector3.zero;
	}

	private void OnClicked()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked || unloading || loadingBoxes || DOTween.IsTweening(base.transform))
		{
			return;
		}
		EventManager.NotifyEvent(GameEvents.VEHICLE_TAKEN, (VehichleInteractable)this);
		SingletonBehaviour<PlayerLook>.Instance.RotationLocked = true;
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
		SingletonBehaviour<PlayerMove>.Instance.transform.DOMove(new Vector3(playerSitTransform.position.x, playerSitTransform.position.y, playerSitTransform.position.z), 0.2f).OnComplete(delegate
		{
			isDriving = true;
			SingletonBehaviour<PlayerLook>.Instance.RotationLocked = false;
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
			base.transform.SetParent(platformCartParent);
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
		SingletonBehaviour<PlayerLook>.Instance.CameraParent.DORotate(Vector3.up * base.transform.localEulerAngles.y, 0.2f);
		SingletonBehaviour<PlayerLook>.Instance.transform.DOLocalRotate(Vector3.zero, 0.2f).OnComplete(delegate
		{
			SingletonBehaviour<PlayerLook>.Instance.UpdateClamp(0f);
		});
		for (int num = 0; num < Enum.GetValues(typeof(BoxSlot)).Length; num++)
		{
			List<int> containedBoxes = GetContainedBoxes((BoxSlot)num);
			for (int num2 = 0; num2 < containedBoxes.Count; num2++)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxes[num2]);
				if (box != null)
				{
					box.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
				}
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
			SingletonBehaviour<PlayerLook>.Instance.UpdateClamp(0f);
		});
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		StopAnimation();
		for (int num = 0; num < Enum.GetValues(typeof(BoxSlot)).Length; num++)
		{
			List<int> containedBoxes = GetContainedBoxes((BoxSlot)num);
			for (int num2 = 0; num2 < containedBoxes.Count; num2++)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxes[num2]);
				if (box != null)
				{
					box.Collider.gameObject.layer = BoxManager.BOX_LAYER;
				}
			}
		}
		SaveLocation();
	}

	private void UnloadBoxes()
	{
		if (unloading || DOTween.IsTweening(base.transform) || !CanUnload())
		{
			return;
		}
		unloading = true;
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
		SingletonBehaviour<PlayerLook>.Instance.RotationLocked = true;
		base.transform.DOLocalMove(base.transform.localPosition + Vector3.forward * 0.15f, 0.4f).SetLoops(2, LoopType.Yoyo);
		base.transform.DOLocalRotate(new Vector3(5f, base.transform.localEulerAngles.y, base.transform.localEulerAngles.z), 0.4f).OnComplete(delegate
		{
			base.transform.DOLocalRotate(pickedHandtruckRotation, 0.4f).SetDelay(0.35f).OnComplete(delegate
			{
				SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
				SingletonBehaviour<PlayerLook>.Instance.RotationLocked = false;
				unloading = false;
			});
			for (int num = 0; num < Enum.GetValues(typeof(BoxSlot)).Length; num++)
			{
				List<int> containedBoxes = GetContainedBoxes((BoxSlot)num);
				for (int num2 = 0; num2 < containedBoxes.Count; num2++)
				{
					Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxes[num2]);
					if (box != null)
					{
						box.RigidBody.constraints = RigidbodyConstraints.None;
						box.gameObject.layer = BoxManager.BOX_LAYER;
						box.transform.SetParent(null);
						box.OnBoxRemovedFromVehicle();
					}
				}
			}
			containedBoxIDsCloserLeft.Clear();
			containedBoxIDsCloserRight.Clear();
			containedBoxIDsFartherLeft.Clear();
			containedBoxIDsFartherRight.Clear();
			firstDualSlotBoxIDs.Clear();
			secondDualSlotBoxIDs.Clear();
			RefreshButtons();
			currentHeightCloserLeft = 0f;
			currentHeightCloserRight = 0f;
			currentHeightFartherLeft = 0f;
			currentHeightFartherRight = 0f;
			currentHeightFirstDualSlot = 0f;
			currentHeightSecondDualSlot = 0f;
			SaveContent();
		});
	}

	public override void RefreshButtons()
	{
		if (GetBoxCount() == 0)
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
		if (GetBoxCount() == 0)
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

	private int GetBoxCount()
	{
		return containedBoxIDsCloserLeft.Count + containedBoxIDsCloserRight.Count + containedBoxIDsFartherLeft.Count + containedBoxIDsFartherRight.Count + firstDualSlotBoxIDs.Count + secondDualSlotBoxIDs.Count;
	}

	public override void RefreshButtonsPalletHovered(Action palletTakeAction)
	{
		base.RefreshButtonsBoxHovered();
		if (GetBoxCount() == 0)
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

	private void RemoveBox(Box box)
	{
		BoxSlot boxSlot = GetBoxSlot(box);
		SetHeight(boxSlot, GetHeight(boxSlot) - box.GetHeight());
		List<int> containedBoxes = GetContainedBoxes(boxSlot);
		int num = containedBoxes.IndexOf(box.BoxID);
		containedBoxes.Remove(box.BoxID);
		SaveContent();
		float height = box.GetHeight();
		for (int i = 0; i < containedBoxes.Count; i++)
		{
			if (i >= num)
			{
				Box box2 = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxes[i]);
				if (box2 != null)
				{
					box2.transform.DOLocalMove(box2.transform.localPosition - Vector3.up * height, 0.2f);
				}
			}
		}
		switch (boxSlot)
		{
		case BoxSlot.FirstDualSlot:
			MoveBoxesDownBy(BoxSlot.CloserLeft, height);
			MoveBoxesDownBy(BoxSlot.CloserRight, height);
			break;
		case BoxSlot.SecondDualSlot:
			MoveBoxesDownBy(BoxSlot.FartherLeft, height);
			MoveBoxesDownBy(BoxSlot.FartherRight, height);
			break;
		}
	}

	private void MoveBoxesDownBy(BoxSlot boxSlot, float height)
	{
		List<int> containedBoxes = GetContainedBoxes(boxSlot);
		for (int i = 0; i < containedBoxes.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxes[i]);
			if (box != null)
			{
				box.transform.DOKill();
				box.transform.DOLocalMove(box.transform.localPosition - Vector3.up * height, 0.2f);
			}
		}
	}

	private void MoveBoxesUpBy(BoxSlot boxSlot, float height)
	{
		List<int> containedBoxes = GetContainedBoxes(boxSlot);
		for (int i = 0; i < containedBoxes.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxes[i]);
			if (box != null)
			{
				box.transform.DOKill();
				box.transform.DOLocalMove(box.transform.localPosition + Vector3.up * height, 0.2f);
			}
		}
	}

	private BoxSlot GetBoxSlot(Box box)
	{
		if (containedBoxIDsCloserLeft.Contains(box.BoxID))
		{
			return BoxSlot.CloserLeft;
		}
		if (containedBoxIDsCloserRight.Contains(box.BoxID))
		{
			return BoxSlot.CloserRight;
		}
		if (containedBoxIDsFartherLeft.Contains(box.BoxID))
		{
			return BoxSlot.FartherLeft;
		}
		if (containedBoxIDsFartherRight.Contains(box.BoxID))
		{
			return BoxSlot.FartherRight;
		}
		if (firstDualSlotBoxIDs.Contains(box.BoxID))
		{
			return BoxSlot.FirstDualSlot;
		}
		if (secondDualSlotBoxIDs.Contains(box.BoxID))
		{
			return BoxSlot.SecondDualSlot;
		}
		return BoxSlot.CloserLeft;
	}

	private void OnPlaceProductByHand()
	{
		if (!outline.enabled)
		{
			return;
		}
		Box box = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (unloading || IsBoxContained(box.BoxID) || SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			return;
		}
		box.GetContainedProduct();
		float num = 0f;
		float num2 = 0f;
		BoxSlot boxSlot = BoxSlot.CloserLeft;
		if (box.Type == BoxType.WIDE || box.Type == BoxType.XL_BOX)
		{
			boxSlot = GetMostAvailableDualSlot();
			num = GetHeight(boxSlot);
			num2 = num;
			switch (boxSlot)
			{
			case BoxSlot.FirstDualSlot:
				num2 += Mathf.Max(GetHeight(BoxSlot.CloserLeft), GetHeight(BoxSlot.CloserRight));
				break;
			case BoxSlot.SecondDualSlot:
				num2 += Mathf.Max(GetHeight(BoxSlot.FartherLeft), GetHeight(BoxSlot.FartherRight));
				break;
			}
		}
		else
		{
			boxSlot = GetMostAvailableSlot();
			float num3 = 0f;
			switch (boxSlot)
			{
			case BoxSlot.CloserLeft:
			case BoxSlot.CloserRight:
				num3 = GetHeight(BoxSlot.FirstDualSlot);
				break;
			case BoxSlot.FartherLeft:
			case BoxSlot.FartherRight:
				num3 = GetHeight(BoxSlot.SecondDualSlot);
				break;
			}
			num = GetHeight(boxSlot) + num3;
			num2 = num;
		}
		int count = GetContainedBoxes(boxSlot).Count;
		if (num2 + box.GetHeight() < 1.6f)
		{
			if (count == 0)
			{
				GetContainedBoxes(boxSlot).Clear();
			}
			Vector3 boxPosition = GetBoxStartTransform(boxSlot).localPosition + num * Vector3.up;
			SetHeight(boxSlot, GetHeight(boxSlot) + box.GetHeight());
			SingletonBehaviour<BoxManager>.Instance.OnBoxPutWithoutThrow();
			if (box.IsOpen())
			{
				box.Close();
			}
			box.transform.DOKill();
			box.gameObject.layer = BoxManager.NOT_COLLIDING_BOX_LAYER;
			loadingBoxes = true;
			box.transform.SetParent(base.transform);
			box.transform.DOLocalRotate(GetBoxRotation(box.Type), 0.3f);
			box.transform.DoCurvedLocalMove(boxPosition, 0.3f, 2f).OnComplete(delegate
			{
				box.OnBoxPut();
				loadingBoxes = false;
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
			}).OnKill(delegate
			{
				box.transform.localPosition = boxPosition;
				box.transform.localEulerAngles = GetBoxRotation(box.Type);
				box.OnBoxPut();
				loadingBoxes = false;
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
			});
			switch (boxSlot)
			{
			case BoxSlot.FirstDualSlot:
				MoveBoxesUpBy(BoxSlot.CloserLeft, box.GetHeight());
				MoveBoxesUpBy(BoxSlot.CloserRight, box.GetHeight());
				break;
			case BoxSlot.SecondDualSlot:
				MoveBoxesUpBy(BoxSlot.FartherLeft, box.GetHeight());
				MoveBoxesUpBy(BoxSlot.FartherRight, box.GetHeight());
				break;
			}
			HapticController.Vibrate(PresetType.LightImpact);
			GetContainedBoxes(boxSlot).Add(box.BoxID);
			SaveContent();
			CloseInteractionElements();
			SingletonBehaviour<HotKeyManager>.Instance.RefreshEnablity();
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_handtruck_full", base.transform);
		}
	}

	private BoxSlot GetMostAvailableSlot()
	{
		float height = GetHeight(BoxSlot.SecondDualSlot);
		float height2 = GetHeight(BoxSlot.FirstDualSlot);
		float num = float.PositiveInfinity;
		BoxSlot result = BoxSlot.CloserLeft;
		for (int i = 0; i < Enum.GetValues(typeof(BoxSlot)).Length; i++)
		{
			BoxSlot boxSlot = (BoxSlot)i;
			if (i != 0 && i != 1)
			{
				float num2 = GetHeight(boxSlot);
				switch (boxSlot)
				{
				case BoxSlot.CloserLeft:
				case BoxSlot.CloserRight:
					num2 += height2;
					break;
				case BoxSlot.FartherLeft:
				case BoxSlot.FartherRight:
					num2 += height;
					break;
				}
				if (num2 < num)
				{
					num = num2;
					result = boxSlot;
				}
			}
		}
		return result;
	}

	private BoxSlot GetMostAvailableDualSlot()
	{
		float height = GetHeight(BoxSlot.SecondDualSlot);
		float height2 = GetHeight(BoxSlot.FirstDualSlot);
		float num = float.PositiveInfinity;
		BoxSlot result = BoxSlot.CloserLeft;
		for (int i = 0; i < Enum.GetValues(typeof(BoxSlot)).Length; i++)
		{
			BoxSlot boxSlot = (BoxSlot)i;
			if (i == 0 || i == 1)
			{
				continue;
			}
			float height3 = GetHeight(boxSlot);
			switch (boxSlot)
			{
			case BoxSlot.CloserLeft:
			case BoxSlot.CloserRight:
				height3 += height2;
				if (height3 < num)
				{
					num = height3;
					result = BoxSlot.FirstDualSlot;
				}
				break;
			case BoxSlot.FartherLeft:
			case BoxSlot.FartherRight:
				height3 += height;
				if (height3 < num)
				{
					num = height3;
					result = BoxSlot.SecondDualSlot;
				}
				break;
			}
		}
		return result;
	}

	public override bool PlaceBoxByClick(Box box)
	{
		if (unloading || IsBoxContained(box.BoxID))
		{
			return false;
		}
		float num = 0f;
		float num2 = 0f;
		BoxSlot boxSlot = BoxSlot.CloserLeft;
		if (box.Type == BoxType.WIDE || box.Type == BoxType.XL_BOX)
		{
			boxSlot = GetMostAvailableDualSlot();
			num = GetHeight(boxSlot);
			num2 = num;
			switch (boxSlot)
			{
			case BoxSlot.FirstDualSlot:
				num2 += Mathf.Max(GetHeight(BoxSlot.CloserLeft), GetHeight(BoxSlot.CloserRight));
				break;
			case BoxSlot.SecondDualSlot:
				num2 += Mathf.Max(GetHeight(BoxSlot.FartherLeft), GetHeight(BoxSlot.FartherRight));
				break;
			}
		}
		else
		{
			boxSlot = GetMostAvailableSlot();
			float num3 = 0f;
			switch (boxSlot)
			{
			case BoxSlot.CloserLeft:
			case BoxSlot.CloserRight:
				num3 = GetHeight(BoxSlot.FirstDualSlot);
				break;
			case BoxSlot.FartherLeft:
			case BoxSlot.FartherRight:
				num3 = GetHeight(BoxSlot.SecondDualSlot);
				break;
			}
			num = GetHeight(boxSlot) + num3;
			num2 = num;
		}
		int count = GetContainedBoxes(boxSlot).Count;
		if (num2 + box.GetHeight() < 1.6f)
		{
			if (count == 0)
			{
				GetContainedBoxes(boxSlot).Clear();
			}
			Vector3 boxPosition = Vector3.zero;
			boxPosition = GetBoxStartTransform(boxSlot).localPosition + num * Vector3.up;
			SetHeight(boxSlot, GetHeight(boxSlot) + box.GetHeight());
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_THROW);
			if (box.IsOpen())
			{
				box.Close();
			}
			EventManager.NotifyEvent(GameEvents.VEHICLE_BOX_PICK_STARTED, box);
			box.transform.SetParent(base.transform);
			box.transform.DOLocalRotate(GetBoxRotation(box.Type), 0.3f);
			box.transform.DoCurvedLocalMove(boxPosition, 0.3f, 2f).OnComplete(delegate
			{
				if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
				{
					box.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
				}
				box.SetBoxStored();
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
				EventManager.NotifyEvent(GameEvents.VEHICLE_BOX_PICKED_UP, box);
			}).OnKill(delegate
			{
				box.transform.localEulerAngles = GetBoxRotation(box.Type);
				box.transform.localPosition = boxPosition;
				if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
				{
					box.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
				}
				box.SetBoxStored();
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
				EventManager.NotifyEvent(GameEvents.VEHICLE_BOX_PICKED_UP, box);
			});
			switch (boxSlot)
			{
			case BoxSlot.FirstDualSlot:
				MoveBoxesUpBy(BoxSlot.CloserLeft, box.GetHeight());
				MoveBoxesUpBy(BoxSlot.CloserRight, box.GetHeight());
				break;
			case BoxSlot.SecondDualSlot:
				MoveBoxesUpBy(BoxSlot.FartherLeft, box.GetHeight());
				MoveBoxesUpBy(BoxSlot.FartherRight, box.GetHeight());
				break;
			}
			box.gameObject.layer = BoxManager.NOT_COLLIDING_BOX_LAYER;
			SingletonBehaviour<BoxManager>.Instance.WakeUpBoxesAbove(box);
			HapticController.Vibrate(PresetType.LightImpact);
			box.OnMouseHoverEnded();
			GetContainedBoxes(boxSlot).Add(box.BoxID);
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
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void OnBoxPickStarted()
	{
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (IsBoxContained(pickedBox.BoxID))
		{
			RemoveBox(pickedBox);
		}
	}

	private void OnBoxPickStartedVehicle(Box box)
	{
		if (IsBoxContained(box.BoxID))
		{
			box.transform.DOKill();
			RemoveBox(box);
		}
	}

	private bool IsBoxContained(int boxId)
	{
		if (!containedBoxIDsCloserLeft.Contains(boxId) && !containedBoxIDsCloserRight.Contains(boxId) && !containedBoxIDsFartherLeft.Contains(boxId) && !containedBoxIDsFartherRight.Contains(boxId) && !firstDualSlotBoxIDs.Contains(boxId))
		{
			return secondDualSlotBoxIDs.Contains(boxId);
		}
		return true;
	}

	private void SaveContent()
	{
		GenericDataSerializer.Save("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.CloserLeft, containedBoxIDsCloserLeft);
		GenericDataSerializer.Save("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.CloserRight, containedBoxIDsCloserRight);
		GenericDataSerializer.Save("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.FartherLeft, containedBoxIDsFartherLeft);
		GenericDataSerializer.Save("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.FartherRight, containedBoxIDsFartherRight);
		GenericDataSerializer.Save("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.FirstDualSlot, firstDualSlotBoxIDs);
		GenericDataSerializer.Save("PLATFORM_CART_BOX_LIST_KEY" + BoxSlot.SecondDualSlot, secondDualSlotBoxIDs);
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
		if (!SingletonBehaviour<TrayManager>.Instance.IsPicked && !SingletonBehaviour<VehicleManager>.Instance.IsOnPlatformCart)
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
	}

	public override void StopAnimation()
	{
		base.StopAnimation();
	}

	public override bool IsAnimating()
	{
		return false;
	}

	public override bool ContainsBox(int id)
	{
		return IsBoxContained(id);
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
		bool num = GetBoxCount() == 0;
		if (!num)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("cant_sell_handtruck", base.transform);
		}
		return num;
	}

	public bool CanUnload()
	{
		Vector3 center = platformCartTrigger.transform.position + platformCartTrigger.transform.TransformVector(platformCartTrigger.center);
		Vector3 halfExtents = platformCartTrigger.size * 0.5f * platformCartTrigger.transform.lossyScale.x;
		Quaternion rotation = platformCartTrigger.transform.rotation;
		Collider[] array = Physics.OverlapBox(center, halfExtents, rotation, SingletonBehaviour<VehicleManager>.Instance.BoxCarrierVehicleOverlapLayers, QueryTriggerInteraction.Ignore);
		bool flag = true;
		if (array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject != base.gameObject)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("cannot_unload_box", base.transform);
			}
		}
		return array.Length == 0 || flag;
	}
}
