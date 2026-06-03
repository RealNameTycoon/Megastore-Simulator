using System;
using System.Collections.Generic;
using DFTGames.Localization;
using DG.Tweening;
using UnityEngine;

public class TrashFurniture : Furniture
{
	[SerializeField]
	private BoxCollider triggerCollider;

	[SerializeField]
	private Transform trashCover;

	[SerializeField]
	private Transform trashTarget;

	[SerializeField]
	private Transform boxTargetTop;

	[SerializeField]
	private Transform boxTargetBottom;

	[SerializeField]
	private Outline outline;

	[SerializeField]
	private PlacementStarter placementStarter;

	private Action onPlacementEnded;

	private float TRASH_ANIMATION_DURATION = 0.5f;

	private Vector3 initialPosition;

	private Vector3 initialRotation;

	private const float SELLING_PRICE_MULTIPLIER = 0f;

	private const float RECYCLE_BIN_SELLING_PRICE_MULTIPLIER = 0.5f;

	public Transform BoxTargetTop => boxTargetTop;

	public Transform BoxTargetBottom => boxTargetBottom;

	public bool IsRecycleBin => type == FurnitureType.RECYCLE_BIN;

	private void Awake()
	{
		if (trashCover != null)
		{
			initialPosition = trashCover.localPosition;
			initialRotation = trashCover.localEulerAngles;
		}
		EventManager.AddListener<int>(GameEvents.PALLET_TRASHED, OnPalletTrashed);
		placementStarter.SetMouseHoverAction(OnMouseHovered);
		placementStarter.SetMouseHoverEndedAction(OnMouseHoverEnded);
	}

	public override void SetFloorLayers()
	{
		LayerMask placeableFloorLayers = (1 << PlacementManager.FLOOR_LAYER) | (1 << PlacementManager.SERVICE_ROOM_FLOOR_LAYER) | (1 << PlacementManager.STORAGE_FLOOR_LAYER) | (1 << PlacementManager.VEHICLE_FLOOR_LAYER) | (1 << PlacementManager.AROUND_STORE_LAYER);
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
	}

	private void OnMouseHovered()
	{
		outline.enabled = true;
		RepaintButtonsWindow();
	}

	private void OnPalletTrashed(int palletID)
	{
		if (!outline.enabled)
		{
			return;
		}
		Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(palletID);
		if (!(pallet != null))
		{
			return;
		}
		pallet.transform.DOKill();
		pallet.EnableCollider(enable: false);
		if (trashCover != null)
		{
			trashCover.DOKill();
			if (!DOTween.IsTweening(trashCover))
			{
				trashCover.DOLocalMove(trashTarget.localPosition, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.OutSine).OnComplete(delegate
				{
					trashCover.DOLocalMove(initialPosition, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.InSine);
				});
				trashCover.DOLocalRotate(trashTarget.localEulerAngles, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.OutSine).OnComplete(delegate
				{
					trashCover.DOLocalRotate(initialRotation, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.InSine);
				});
			}
		}
		Vector3 initialPalletScale = pallet.transform.localScale;
		SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_GARBAGE);
		pallet.transform.DoCurvedMove(boxTargetBottom.position, TRASH_ANIMATION_DURATION, boxTargetTop.position);
		pallet.transform.DOScale(initialPalletScale / 2f, TRASH_ANIMATION_DURATION).OnComplete(delegate
		{
			pallet.gameObject.SetActive(value: false);
			pallet.transform.localScale = initialPalletScale;
			pallet.EnableCollider(enable: true);
			SingletonBehaviour<PalletManager>.Instance.DeletePallet(palletID);
			pallet.OnMouseHoverEnded();
			if (IsRecycleBin)
			{
				EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, PalletManager.PALLET_DEPOSIT_AMOUNT);
				SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.PAYMENT_DONE);
			}
		});
	}

	private void RepaintButtonsWindow()
	{
		if ((SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsDisposable()) || SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("box_throw", delegate
				{
					ThrowToTrash();
				})
			} }, base.transform);
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty() && IsRecycleBin)
		{
			float num = 0f;
			Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
			if (pickedBox != null)
			{
				num = pickedBox.GetBoxPrice(IsRecycleBin ? 0.5f : 0f);
			}
			string item = Locale.GetWord("sell_n").Replace("{0}", "$" + num);
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				(item, delegate
				{
					pickedBox.transform.DOKill();
					SingletonBehaviour<BoxManager>.Instance.ThrowBox();
					ThrowToTrash(pickedBox, sell: true);
				})
			} }, base.transform, localizeDescription: false);
		}
		else if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.F,
					("pack", delegate
					{
						Pack();
					})
				},
				{
					KeyCode.Mouse1,
					("move", delegate
					{
						StartNewPlacement();
					})
				}
			}, base.transform);
		}
	}

	public void OnMouseHoverEnded()
	{
		outline.enabled = false;
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		SingletonBehaviour<PalletManager>.Instance.UpdateMenu();
	}

	private void ThrowToTrash()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsDisposable())
		{
			SingletonBehaviour<BoxManager>.Instance.GetPickedBox().transform.DOKill();
			SingletonBehaviour<BoxManager>.Instance.ThrowBox();
		}
		else if (SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
		{
			SingletonBehaviour<PalletManager>.Instance.TrashPallet();
		}
	}

	private void ThrowToTrash(Box box, bool sell = false)
	{
		if (!(box != null) || !(box.IsDisposable() || sell))
		{
			return;
		}
		box.Collider.enabled = false;
		box.RigidBody.linearVelocity = (base.transform.position - box.transform.position).normalized * 1f;
		if (trashCover != null)
		{
			trashCover.DOKill();
			if (!DOTween.IsTweening(trashCover))
			{
				trashCover.DOLocalMove(trashTarget.localPosition, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.OutSine).OnComplete(delegate
				{
					trashCover.DOLocalMove(initialPosition, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.InSine);
				});
				trashCover.DOLocalRotate(trashTarget.localEulerAngles, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.OutSine).OnComplete(delegate
				{
					trashCover.DOLocalRotate(initialRotation, TRASH_ANIMATION_DURATION / 2f).SetEase(Ease.InSine);
				});
			}
		}
		Vector3 initialBoxScale = box.transform.localScale;
		SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_GARBAGE);
		box.transform.DoCurvedMove(boxTargetBottom.position, TRASH_ANIMATION_DURATION, boxTargetTop.position);
		if (sell)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.PAYMENT_DONE);
		}
		box.transform.DOScale(initialBoxScale / 2f, TRASH_ANIMATION_DURATION).OnComplete(delegate
		{
			box.gameObject.SetActive(value: false);
			box.transform.localScale = initialBoxScale;
			if (sell)
			{
				float boxPrice = box.GetBoxPrice(IsRecycleBin ? 0.5f : 0f);
				EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, boxPrice);
				ProductData anyContainedProductData = box.GetAnyContainedProductData();
				if (anyContainedProductData != null && anyContainedProductData.maxAmountAllowed != -1)
				{
					SingletonBehaviour<OrderManager>.Instance.ProductRecycled(anyContainedProductData.type);
				}
			}
			SingletonBehaviour<BoxManager>.Instance.DeleteBox(box.BoxID);
			box.OnMouseHoverEnded();
		});
	}

	private void OnTriggerEnter(Collider other)
	{
		Box component = other.gameObject.GetComponent<Box>();
		ThrowToTrash(component);
	}

	public override bool CanPack()
	{
		return true;
	}

	public new void Pack()
	{
		if (CanPack())
		{
			SingletonBehaviour<SpawnManager>.Instance.PackFurniture(this);
		}
	}
}
