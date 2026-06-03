using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PalletCarrierVehicle : VehichleInteractable
{
	[SerializeField]
	private Outline outline;

	[SerializeField]
	private Transform playerSitTransform;

	[SerializeField]
	private Transform wheels;

	[SerializeField]
	private List<BoxCollider> clickableColliders;

	[SerializeField]
	private Transform palletTarget;

	[SerializeField]
	private bool vehicleMoveSeperately;

	[SerializeField]
	private Transform vehicleParent;

	[SerializeField]
	private BoxCollider palletTrigger;

	[SerializeField]
	private Transform[] exitTransforms;

	private bool unloading;

	private Pallet containedPallet;

	[SerializeField]
	private List<Pallet> interactedPallets = new List<Pallet>();

	[SerializeField]
	protected List<PalletShelf> interactedPalletShelfs = new List<PalletShelf>();

	private Vector3 cachedHalfExtents;

	private static float vehicleYPosition = 0.5f;

	protected bool ContainsPallet => containedPallet != null;

	protected virtual void Start()
	{
		if (!SingletonBehaviour<VehicleManager>.Instance.IsVehiclePurchased(base.VehicleType, base.VehicleID))
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Activate();
		}
	}

	private void RefreshInteractedPalletShelfs()
	{
		Vector3 center = palletTrigger.transform.position + palletTrigger.transform.TransformVector(palletTrigger.center);
		Vector3 halfExtents = palletTrigger.size * 0.5f * palletTrigger.transform.lossyScale.x;
		Quaternion rotation = palletTrigger.transform.rotation;
		Collider[] array = Physics.OverlapBox(center, halfExtents, rotation, (1 << RayShooter.PALLET_SHELF_LAYER) | (1 << RayShooter.PALLET_LAYER));
		if (interactedPalletShelfs.Count > 0)
		{
			interactedPalletShelfs[0].OnInteractionFinished();
			interactedPalletShelfs.Clear();
		}
		if (interactedPallets.Count > 0)
		{
			interactedPallets[0].EnableOutline(enable: false);
			interactedPallets.Clear();
		}
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.gameObject.layer == RayShooter.PALLET_SHELF_LAYER)
			{
				PalletShelf component = collider.GetComponent<PalletShelf>();
				if (!interactedPalletShelfs.Contains(component))
				{
					interactedPalletShelfs.Add(component);
				}
			}
			else if (collider.gameObject.layer == RayShooter.PALLET_LAYER)
			{
				Pallet component2 = collider.GetComponent<Pallet>();
				if (component2 == null)
				{
					return;
				}
				if (!interactedPallets.Contains(component2))
				{
					interactedPallets.Add(component2);
				}
			}
		}
		InteractLastPallet();
		InteractLastPalletShelf();
		RefreshButtons();
	}

	private void Activate()
	{
		Vector3 position = GenericDataSerializer.Load(GetPositionKey(), base.transform.position);
		if (position.y < vehicleYPosition)
		{
			position.y = vehicleYPosition;
		}
		base.transform.position = position;
		base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, GenericDataSerializer.LoadFloat(GetRotationKey(), base.transform.localEulerAngles.y), base.transform.localEulerAngles.z);
		cachedHalfExtents = SingletonBehaviour<PlayerMove>.Instance.Bounds.size * 0.5f;
		EventManager.AddListener<int>(GameEvents.PALLET_TAKEN, OnPalletTaken);
	}

	private void OnPalletTaken(int palletID)
	{
		if (!(containedPallet == null) && containedPallet.PalletID == palletID)
		{
			return;
		}
		for (int num = interactedPallets.Count - 1; num >= 0; num--)
		{
			if (interactedPallets[num].PalletID == palletID)
			{
				interactedPallets.RemoveAt(num);
				break;
			}
		}
	}

	private void OnClicked()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return;
		}
		EventManager.NotifyEvent(GameEvents.VEHICLE_TAKEN, (VehichleInteractable)this);
		for (int i = 0; i < clickableColliders.Count; i++)
		{
			clickableColliders[i].enabled = false;
		}
		SingletonBehaviour<PlayerLook>.Instance.RotationLocked = true;
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
		SingletonBehaviour<PlayerMove>.Instance.transform.DOMove(new Vector3(playerSitTransform.position.x, playerSitTransform.position.y, playerSitTransform.position.z), 0.2f).OnComplete(delegate
		{
			SingletonBehaviour<PlayerLook>.Instance.RotationLocked = false;
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = vehicleMoveSeperately;
			isDriving = true;
			if (vehicleMoveSeperately)
			{
				SingletonBehaviour<PlayerMove>.Instance.transform.SetParent(playerSitTransform);
			}
			else
			{
				base.transform.SetParent(vehicleParent);
				base.transform.DOLocalMove(Vector3.zero, 0.2f);
			}
			RefreshInteractedPalletShelfs();
			if (containedPallet == null && interactedPallets.Count > 0)
			{
				interactedPallets[0].EnableOutline(enable: true);
			}
			if (outline.enabled)
			{
				outline.enabled = false;
			}
			if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
			{
				SingletonBehaviour<TooltipUI>.Instance.Close();
			}
			if (base.VehicleType == VehicleType.FORKLIFT)
			{
				EventLogger.ForkliftFirstTime();
			}
		});
		SingletonBehaviour<PlayerLook>.Instance.CameraParent.DORotate(Vector3.up * base.transform.localEulerAngles.y, 0.2f);
		SingletonBehaviour<PlayerLook>.Instance.transform.DOLocalRotate(Vector3.zero, 0.2f).OnComplete(delegate
		{
			SingletonBehaviour<PlayerLook>.Instance.UpdateClamp(0f);
		});
	}

	private void ClearAndOverlapBoxForPalletAndPalletShelf()
	{
		interactedPallets.Clear();
		interactedPalletShelfs.Clear();
	}

	public override void RefreshButtons()
	{
		if (!isDriving)
		{
			return;
		}
		if (containedPallet != null)
		{
			Dictionary<KeyCode, (string, Action)> actionDictionary = new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Space,
					("put_pallet", delegate
					{
						ReleasePallet();
					})
				},
				{
					SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
					("leave", delegate
					{
						Leave();
					})
				}
			};
			RepaintButtons(actionDictionary);
		}
		else if (containedPallet == null && interactedPallets.Count != 0)
		{
			Dictionary<KeyCode, (string, Action)> actionDictionary2 = new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Space,
					("take_pallet", delegate
					{
						TakePallet();
					})
				},
				{
					SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
					("leave", delegate
					{
						Leave();
					})
				}
			};
			RepaintButtons(actionDictionary2);
		}
		else
		{
			Dictionary<KeyCode, (string, Action)> actionDictionary3 = new Dictionary<KeyCode, (string, Action)> { 
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					Leave();
				})
			} };
			RepaintButtons(actionDictionary3);
		}
	}

	private void RepaintButtons(Dictionary<KeyCode, (string, Action)> actionDictionary)
	{
		Dictionary<KeyCode, (string, Action)> extraButtonActions = GetExtraButtonActions();
		if (extraButtonActions != null)
		{
			foreach (KeyValuePair<KeyCode, (string, Action)> item in extraButtonActions)
			{
				actionDictionary.Add(item.Key, item.Value);
			}
		}
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(actionDictionary);
	}

	public virtual Dictionary<KeyCode, (string, Action)> GetExtraButtonActions()
	{
		return null;
	}

	private void ReleasePallet()
	{
		bool flag = TryPlaceToFirstEmptyShelf(containedPallet);
		if (!flag && !CanUnload())
		{
			return;
		}
		bool flag2 = interactedPalletShelfs.Count > 0;
		if (!flag && flag2)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pallet_rack_full", base.transform);
			return;
		}
		if (!flag)
		{
			containedPallet.transform.SetParent(null);
			containedPallet.EnableCollider(enable: true);
			containedPallet.transform.DOKill();
			containedPallet.OnReleased();
		}
		interactedPallets.Remove(containedPallet);
		interactedPallets.Insert(0, containedPallet);
		InteractLastPallet();
		containedPallet = null;
		RefreshButtons();
	}

	public virtual bool CanUnload()
	{
		Vector3 center = palletTrigger.transform.position + palletTrigger.transform.TransformVector(palletTrigger.center);
		Vector3 halfExtents = palletTrigger.size * 0.5f * palletTrigger.transform.lossyScale.x;
		Quaternion rotation = palletTrigger.transform.rotation;
		Collider[] array = Physics.OverlapBox(center, halfExtents, rotation, SingletonBehaviour<VehicleManager>.Instance.PalletCarrierVehicleOverlapLayers, QueryTriggerInteraction.Ignore);
		bool flag = true;
		if (array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject.tag != RayShooter.PRICE_LABEL_TAG)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("cannot_unload_pallet", base.transform);
			}
		}
		return array.Length == 0 || flag;
	}

	private bool TryPlaceToFirstEmptyShelf(Pallet pallet)
	{
		if (interactedPalletShelfs.Count > 0 && interactedPalletShelfs[0].IsEmpty)
		{
			pallet.transform.DOKill();
			pallet.transform.SetParent(null);
			pallet.EnableCollider(enable: true);
			interactedPalletShelfs[0].PlacePallet(pallet);
			return true;
		}
		return false;
	}

	private void TakePallet()
	{
		if ((interactedPallets[0] == containedPallet && DOTween.IsTweening(containedPallet.transform)) || DOTween.IsTweening(interactedPallets[0].transform))
		{
			return;
		}
		if (interactedPallets[0].IsReservedToStaff())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pallet_reserved_staff_error", base.transform);
			return;
		}
		containedPallet = interactedPallets[0];
		containedPallet.OnTaken();
		containedPallet.EnableOutline(enable: false);
		containedPallet.EnableCollider(enable: false);
		containedPallet.transform.DOKill();
		containedPallet.transform.SetParent(palletTarget.transform.parent);
		containedPallet.transform.DOLocalMove(palletTarget.transform.localPosition, 0.3f);
		float y = containedPallet.transform.localEulerAngles.y;
		EventManager.NotifyEvent(GameEvents.PALLET_TAKEN, containedPallet.PalletID);
		SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
		if (interactedPalletShelfs.Count > 0 && interactedPalletShelfs[0].IsEmpty)
		{
			interactedPalletShelfs[0].RefreshInteraction(carrierHasPallet: true);
		}
		ShortcutExtensions.DOLocalRotate(endValue: new Vector3(0f, (!(y % 90f > 45f)) ? (y - y % 90f) : (y + (90f - y % 90f)), 0f), target: containedPallet.transform, duration: 0.3f).OnComplete(delegate
		{
			RefreshButtons();
		});
	}

	private void Leave()
	{
		if (unloading)
		{
			return;
		}
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		isDriving = false;
		for (int i = 0; i < clickableColliders.Count; i++)
		{
			clickableColliders[i].enabled = true;
		}
		if (interactedPallets.Count > 0)
		{
			interactedPallets[0].EnableOutline(enable: false);
		}
		if (interactedPalletShelfs.Count > 0)
		{
			interactedPalletShelfs[0].OnInteractionFinished();
		}
		EventManager.NotifyEvent(GameEvents.VEHICLE_RELEASED);
		if (vehicleMoveSeperately)
		{
			Transform exitTransform = GetExitTransform();
			Vector3 endValue = new Vector3(0f, exitTransform.eulerAngles.y, 0f);
			SingletonBehaviour<PlayerMove>.Instance.transform.SetParent(null, worldPositionStays: true);
			SingletonBehaviour<PlayerMove>.Instance.transform.DOMove(exitTransform.position, 0.2f);
			SingletonBehaviour<PlayerMove>.Instance.transform.DORotate(Vector3.zero, 0.2f).OnComplete(delegate
			{
				SingletonBehaviour<PlayerMove>.Instance.BrakeSpeed(new Vector3(0f, 0f, 0f));
				SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
				StartCoroutine(UnlockNextFixedUpdate());
			});
			SingletonBehaviour<PlayerLook>.Instance.CameraParent.transform.DOLocalRotate(endValue, 0.2f).OnComplete(delegate
			{
				SingletonBehaviour<PlayerLook>.Instance.RotationLocked = false;
			});
			SingletonBehaviour<PlayerLook>.Instance.transform.DOLocalRotate(Vector3.zero, 0.2f).OnComplete(delegate
			{
				SingletonBehaviour<PlayerLook>.Instance.UpdateClamp(0f);
			});
		}
		else
		{
			base.transform.DOKill();
			base.transform.SetParent(null);
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
			base.transform.DORotate(new Vector3(0f, base.transform.localEulerAngles.y, base.transform.localEulerAngles.z), 0.2f).OnComplete(delegate
			{
				SingletonBehaviour<PlayerLook>.Instance.UpdateClamp(0f);
			});
		}
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		StopAnimation();
		SaveLocation();
		OnLeft();
	}

	private Transform GetExitTransform()
	{
		_ = SingletonBehaviour<PlayerMove>.Instance.Bounds;
		Transform[] array = exitTransforms;
		foreach (Transform transform in array)
		{
			if (Physics.OverlapBox(transform.position, cachedHalfExtents, transform.rotation, -1, QueryTriggerInteraction.Ignore).Length == 0)
			{
				return transform;
			}
		}
		return exitTransforms[0];
	}

	private IEnumerator UnlockNextFixedUpdate()
	{
		yield return new WaitForFixedUpdate();
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
	}

	public virtual void OnLeft()
	{
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

	private void SaveContent()
	{
	}

	public override void OnMouseHoverStarted()
	{
		if (isDriving)
		{
			return;
		}
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			outline.enabled = true;
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuForStorageShelf();
		}
		else if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !FireExtinguisher.Instance.IsPicked && !SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			outline.enabled = true;
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("use", delegate
				{
					OnClicked();
				})
			} });
		}
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		CloseInteractionElements();
	}

	public override void StartAnimation()
	{
		if (wheels != null && !DOTween.IsTweening(wheels))
		{
			wheels.DOLocalRotate(new Vector3(360f, 0f, 0f), 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
		}
	}

	public override void StopAnimation()
	{
		base.StopAnimation();
		if (wheels != null)
		{
			wheels.DOKill();
			wheels.localEulerAngles = Vector3.zero;
		}
	}

	public override bool IsAnimating()
	{
		if (wheels == null)
		{
			return false;
		}
		return DOTween.IsTweening(wheels);
	}

	public override LayerMask GetInteractableLayers()
	{
		return 1 << RayShooter.VEHICLE_CLICKABLE_LAYER;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == RayShooter.PALLET_LAYER)
		{
			Pallet component = other.GetComponent<Pallet>();
			if (component == null)
			{
				return;
			}
			if (!interactedPallets.Contains(component))
			{
				interactedPallets.Add(component);
			}
			InteractLastPallet();
			RefreshButtons();
		}
		if (other.gameObject.layer == RayShooter.PALLET_SHELF_LAYER)
		{
			PalletShelf component2 = other.GetComponent<PalletShelf>();
			if (!interactedPalletShelfs.Contains(component2))
			{
				interactedPalletShelfs.Add(component2);
			}
			InteractLastPalletShelf();
			RefreshButtons();
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == RayShooter.PALLET_LAYER)
		{
			Pallet component = other.GetComponent<Pallet>();
			if (!(component == null))
			{
				if (interactedPallets.Count > 0 && interactedPallets.Contains(component))
				{
					component.EnableOutline(enable: false);
					interactedPallets.Remove(component);
					RefreshButtons();
				}
				InteractLastPallet();
			}
		}
		else if (other.gameObject.layer == RayShooter.PALLET_SHELF_LAYER)
		{
			PalletShelf component2 = other.GetComponent<PalletShelf>();
			if (interactedPalletShelfs.Count > 0 && interactedPalletShelfs.Contains(component2))
			{
				component2.OnInteractionFinished();
				interactedPalletShelfs.Remove(component2);
				RefreshButtons();
			}
			InteractLastPalletShelf();
		}
	}

	private void InteractLastPallet()
	{
		if (isDriving && !ContainsPallet && interactedPallets.Count > 0)
		{
			interactedPallets[0].InteractionStarted();
		}
	}

	private void InteractLastPalletShelf()
	{
		if (interactedPalletShelfs.Count > 0)
		{
			interactedPalletShelfs[0].OnInteractionStarted(containedPallet != null);
		}
	}

	public override bool CanSell()
	{
		bool num = containedPallet == null;
		if (!num)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("cant_sell_pallet_carrier_vehicle", base.transform);
		}
		return num;
	}

	public override void OnSell()
	{
		containedPallet = null;
		interactedPallets.Clear();
		interactedPalletShelfs.Clear();
	}
}
