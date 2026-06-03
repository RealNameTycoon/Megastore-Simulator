using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Trash : Clickable, MoveableObjectInterface, MoveableHolderInterface
{
	[SerializeField]
	private BoxCollider triggerCollider;

	[SerializeField]
	private Moveable moveable;

	[SerializeField]
	private Transform trashCover;

	[SerializeField]
	private Transform trashTarget;

	[SerializeField]
	private Transform boxTargetTop;

	[SerializeField]
	private Transform boxTargetBottom;

	[SerializeField]
	private GameObject solidObject;

	[SerializeField]
	private int trashID;

	private static string TRASH_POSITION_KEY = "TrashPosition";

	private static string TRASH_ROTATION_KEY = "TrashRotation";

	private Action onPlacementEnded;

	private float TRASH_ANIMATION_DURATION = 0.5f;

	private Vector3 initialPosition;

	private Vector3 initialRotation;

	private const float SELLING_PRICE_MULTIPLIER = 0f;

	public Transform BoxTargetTop => boxTargetTop;

	public Transform BoxTargetBottom => boxTargetBottom;

	public Moveable Moveable => moveable;

	private void Awake()
	{
		LayerMask placeableFloorLayers = (1 << PlacementManager.FLOOR_LAYER) | (1 << PlacementManager.SERVICE_ROOM_FLOOR_LAYER) | (1 << PlacementManager.STORAGE_FLOOR_LAYER) | (1 << PlacementManager.VEHICLE_FLOOR_LAYER) | (1 << PlacementManager.AROUND_STORE_LAYER);
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
		if (trashCover != null)
		{
			initialPosition = trashCover.localPosition;
			initialRotation = trashCover.localEulerAngles;
		}
		base.transform.position = GenericDataSerializer.Load(TRASH_POSITION_KEY + trashID, base.transform.position);
		base.transform.localEulerAngles = GenericDataSerializer.Load(TRASH_ROTATION_KEY + trashID, base.transform.localEulerAngles);
		EventManager.AddListener<int>(GameEvents.PALLET_TRASHED, OnPalletTrashed);
	}

	private void OnPalletTrashed(int palletID)
	{
		if (!base.Outline.enabled)
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
		});
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
	}

	public override void RepaintButtonsWindow()
	{
		if ((SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && (SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty() || SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsBoxSpoiled() || SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsBoxFrozen())) || SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
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
		else if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse1,
				("move", delegate
				{
					StartNewPlacement();
				})
			} }, base.transform);
		}
		else
		{
			base.RepaintButtonsWindow();
		}
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
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
		box.transform.DOScale(initialBoxScale / 2f, TRASH_ANIMATION_DURATION).OnComplete(delegate
		{
			box.gameObject.SetActive(value: false);
			box.transform.localScale = initialBoxScale;
			if (sell)
			{
				if (box.GetPackableID() != -1)
				{
					if (box.ContainedPlaceableType != PlaceableType.NONE)
					{
						SingletonBehaviour<SpawnManager>.Instance.DeletePlaceable(box.ContainedPlaceableType, box.GetPackableID());
					}
					else if (box.ContainedFurnitureType != FurnitureType.NONE)
					{
						SingletonBehaviour<SpawnManager>.Instance.DeleteFurniture(box.ContainedFurnitureType, box.GetPackableID());
					}
				}
				float boxPrice = box.GetBoxPrice(0f);
				if (boxPrice > 0f)
				{
					EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, boxPrice);
				}
			}
			SingletonBehaviour<BoxManager>.Instance.DeleteBox(box.BoxID);
			box.OnMouseHoverEnded();
		});
	}

	public void DeleteBoxTrash(Box box)
	{
		box.Collider.enabled = false;
		SingletonBehaviour<BoxManager>.Instance.DeleteBox(box.BoxID);
		box.OnMouseHoverEnded();
	}

	private void StartNewPlacement(Action onPlacementEnded = null)
	{
		this.onPlacementEnded = onPlacementEnded;
		SingletonBehaviour<PlacementManager>.Instance.StartPlacement(moveable, facePlayer: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		Box component = other.gameObject.GetComponent<Box>();
		ThrowToTrash(component);
	}

	public void SwitchLook(bool toSolidObject)
	{
		moveable.gameObject.SetActive(!toSolidObject);
		triggerCollider.enabled = toSolidObject;
		solidObject.gameObject.SetActive(toSolidObject);
	}

	public void SavePosition()
	{
		GenericDataSerializer.Save(TRASH_POSITION_KEY + trashID, base.transform.position);
		GenericDataSerializer.Save(TRASH_ROTATION_KEY + trashID, base.transform.localEulerAngles);
	}

	public void OnPlacementEnded()
	{
		onPlacementEnded?.Invoke();
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public Moveable GetMoveable()
	{
		return moveable;
	}

	public bool CanPack()
	{
		return false;
	}

	public BoxCollider SnappableBoxCollider()
	{
		return null;
	}

	public bool PlacedBefore()
	{
		return true;
	}
}
