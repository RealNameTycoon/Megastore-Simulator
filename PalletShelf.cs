using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PalletShelf : Interactable, MoveableHolderInterface, Restockable
{
	[SerializeField]
	private Furniture palletRack;

	[SerializeField]
	private Outline palletRackOutline;

	[SerializeField]
	private int shelfID;

	[SerializeField]
	private GameObject palletGhost;

	[SerializeField]
	private Moveable moveable;

	[SerializeField]
	private GameObject label;

	[SerializeField]
	private TextMeshPro labelText;

	[SerializeField]
	private SpriteRenderer labelImage;

	[SerializeField]
	private Transform staffInteractionPoint;

	private const string CONTAINED_PALLET_ID_KEY = "CONTAINED_PALLET_ID_KEY";

	[SerializeField]
	private int containedPalletID = -1;

	[SerializeField]
	private Pallet containedPallet;

	private static string LAST_CONTAINED_TYPE_KEY = "LAST_CONTAINED_TYPE_KEY";

	private ProductType lastContainedType = ProductType.NONE;

	private bool isReservedToStaff;

	public bool IsEmpty => containedPalletID == -1;

	public int ContainedPalletID => containedPalletID;

	private string GetContainedTypeKey()
	{
		if (palletRack.Type == FurnitureType.SMALL_PALLET_RACK)
		{
			return palletRack.Type.ToString() + palletRack.FurnitureID + "CONTAINED_PALLET_ID_KEY" + shelfID;
		}
		return palletRack.FurnitureID + "CONTAINED_PALLET_ID_KEY" + shelfID;
	}

	public void Initialize()
	{
		EventManager.AddListener<int>(GameEvents.PALLET_TAKEN, OnPalletTaken);
		containedPalletID = GenericDataSerializer.Load(GetContainedTypeKey(), -1);
		if (containedPalletID != -1)
		{
			containedPallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(containedPalletID);
			if (containedPallet == null || containedPallet.ContainerShelf != null)
			{
				containedPallet = null;
				containedPalletID = -1;
				GenericDataSerializer.Save(GetContainedTypeKey(), -1);
				return;
			}
			containedPallet.transform.SetParent(base.transform);
			containedPallet.UnregisterFromRestockZone();
			containedPallet.transform.localPosition = palletGhost.transform.localPosition;
			containedPallet.transform.localEulerAngles = palletGhost.transform.localEulerAngles;
			containedPallet.SetContainerShelf(this);
			containedPallet.CheckAndUpdateRestockZone();
			containedPallet.CheckAndUpdateTemperatureZone();
		}
		Clickable component = label.GetComponent<Clickable>();
		if (component != null)
		{
			component.onClickAction.AddListener(OnLabelClicked);
			component.onRightClickAction.AddListener(OnLabelRightClicked);
			component.SetHoverStartedAction(OnLabelHoverStarted);
		}
		RefreshLabel();
		RegisterUnloaderJobs();
	}

	public void RegisterUnloaderJobs()
	{
		if (SingletonBehaviour<RestockerManager>.Instance != null)
		{
			SingletonBehaviour<RestockerManager>.Instance.CheckAndRegisterShelf(this);
		}
	}

	private void OnLabelClicked()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = lastContainedType;
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_ADDEDTO_CART, param);
		}
		else if (containedPalletID == -1)
		{
			label.SetActive(value: false);
			lastContainedType = ProductType.NONE;
			SaveLabel();
		}
	}

	private void OnLabelRightClicked()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = lastContainedType;
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_REMOVEDFROM_CART, param);
		}
	}

	private void OnLabelHoverStarted()
	{
		ProductType param = lastContainedType;
		EventManager.NotifyEvent(ProductEvents.PRICE_TAG_HOVER_STARTED, param);
	}

	private void SaveLabel()
	{
		GenericDataSerializer.Save(GetContainedTypeKey(), lastContainedType);
	}

	private void OnPalletTaken(int palletID)
	{
		if (containedPalletID != -1 && containedPallet.PalletID == palletID)
		{
			containedPallet.SetContainerShelf(null);
			RemovePallet();
		}
	}

	public void OnInteractionStarted(bool carrierHasPallet)
	{
		if (containedPalletID == -1 && carrierHasPallet)
		{
			palletGhost.SetActive(value: true);
		}
	}

	public void OnInteractionFinished()
	{
		if (containedPalletID == -1)
		{
			palletGhost.SetActive(value: false);
		}
	}

	public void PlacePallet(Pallet pallet)
	{
		containedPalletID = pallet.PalletID;
		GenericDataSerializer.Save(GetContainedTypeKey(), containedPalletID);
		if (containedPalletID != -1)
		{
			containedPallet = pallet;
			containedPallet.SetContainerShelf(this);
			containedPallet.DOKill();
			containedPallet.transform.SetParent(base.transform);
			containedPallet.transform.DOLocalMove(palletGhost.transform.localPosition, 0.3f);
			containedPallet.transform.DOLocalRotate(palletGhost.transform.localEulerAngles, 0.3f).OnComplete(delegate
			{
				containedPallet.SaveLocation(checkForRestockingZone: true);
			});
			EventManager.NotifyEvent(PlaceableEvents.PALLET_SHELF_PALLET_ADDED, (Restockable)this);
		}
		if (palletGhost.activeSelf)
		{
			palletGhost.SetActive(value: false);
		}
		RefreshLabel();
	}

	public void RemovePallet()
	{
		containedPalletID = -1;
		containedPallet = null;
		GenericDataSerializer.Save(GetContainedTypeKey(), -1);
		RefreshLabel();
		EventManager.NotifyEvent(PlaceableEvents.PALLET_SHELF_PALLET_REMOVED, (Restockable)this);
	}

	public void OnParentRepositioned()
	{
		if (containedPalletID != -1 && !(containedPallet == null))
		{
			containedPallet.SaveLocation();
			containedPallet.CheckAndUpdateTemperatureZone();
		}
	}

	public void RefreshInteraction(bool carrierHasPallet)
	{
		OnInteractionStarted(carrierHasPallet);
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		if (!SingletonBehaviour<BoxManager>.Instance.NoContainerPicked() || FireExtinguisher.Instance.IsPicked)
		{
			return;
		}
		palletRackOutline.enabled = true;
		Dictionary<KeyCode, (string, Action)> dictionary = new Dictionary<KeyCode, (string, Action)>();
		dictionary.Add(KeyCode.Mouse1, ("place_move", delegate
		{
			palletRack.StartNewPlacement();
		}));
		if (palletRack.GetExtraButtonActions() != null)
		{
			foreach (var extraButtonAction in palletRack.GetExtraButtonActions())
			{
				dictionary.Add(extraButtonAction.Item1, extraButtonAction.Item2);
			}
		}
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(dictionary, base.transform);
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		CloseInteractionElements();
	}

	protected void CloseInteractionElements()
	{
		if (palletRackOutline.enabled)
		{
			palletRackOutline.enabled = false;
		}
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		FireExtinguisher.Instance.UpdateMenu();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	public Moveable GetMoveable()
	{
		return moveable;
	}

	private void RefreshLabel()
	{
	}

	public void OnBoxPut()
	{
		EventManager.NotifyEvent(PlaceableEvents.BOX_ADDED, (Restockable)this);
	}

	public void OnBoxTaken()
	{
		EventManager.NotifyEvent(PlaceableEvents.BOX_REMOVED, (Restockable)this);
	}

	public void OnLabelRemoved()
	{
		EventManager.NotifyEvent(PlaceableEvents.PALLET_SHELF_LABEL_REMOVED, (Restockable)this);
	}

	public Transform PickupPoint()
	{
		return staffInteractionPoint;
	}

	public bool HasProductLabel()
	{
		if (containedPallet != null)
		{
			return containedPallet.HasProductLabel();
		}
		return false;
	}

	public int GetProductCount()
	{
		if (!(containedPallet != null))
		{
			return 0;
		}
		return containedPallet.BoxCount + containedPallet.BoxAmountToWaitForReturn;
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public int GetCapacityForProduct(ProductData productData)
	{
		return SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(productData.type);
	}

	public void PlaceReturnBox(Box box, bool instant = false)
	{
		box.Collider.enabled = false;
		containedPallet.PlaceReturnBox(box, instant);
	}

	public void PlaceProduct(Box box, bool instant = false)
	{
		box.Collider.enabled = false;
		containedPallet.PlaceBox(box, isNPC: true, instant);
	}

	public void PlaceProduct(Tray tray)
	{
	}

	public void SetReservedToStaff(bool value)
	{
		isReservedToStaff = value;
	}

	public void ClearReservedToStaff()
	{
		isReservedToStaff = false;
	}

	public bool IsReservedToStaff()
	{
		return isReservedToStaff;
	}

	public bool IsContainerReservedToStaff()
	{
		if (containedPallet != null)
		{
			return containedPallet.IsReservedToStaff();
		}
		return false;
	}

	public ProductGroup GetProductGroup()
	{
		if (containedPallet == null || containedPallet.ContainedType == ProductType.NONE)
		{
			return ProductGroup.NONE;
		}
		return SingletonBehaviour<ProductPool>.Instance.GetProductData(containedPallet.ContainedType).productGroup;
	}

	public ProductType ContainedProductType()
	{
		if (containedPallet == null || containedPallet.ContainedType == ProductType.NONE)
		{
			return ProductType.NONE;
		}
		return containedPallet.ContainedType;
	}

	public ProductType PreviousContainedProductType()
	{
		if (containedPallet == null || containedPallet.ContainedType == ProductType.NONE)
		{
			return ProductType.NONE;
		}
		return containedPallet.ContainedType;
	}

	public string RestocableID()
	{
		return palletRack.Type.ToString() + palletRack.FurnitureID + "|" + shelfID;
	}

	public bool IsBakeryPlaceable()
	{
		return false;
	}

	public bool IsAvailableForRestocking()
	{
		return containedPallet != null;
	}

	public Transform StaffInteractionPoint()
	{
		return staffInteractionPoint;
	}

	public bool IsCookedShelf()
	{
		return false;
	}

	public Transform ParentTransform()
	{
		return palletRack.transform;
	}

	public bool IsPlayerReserved()
	{
		return false;
	}

	public bool IsUnloaderPlaceable()
	{
		return true;
	}

	public void OnPacked()
	{
		EventManager.NotifyEvent(PlaceableEvents.RESTOCKABLE_PACKED, (Restockable)this);
	}

	public BoxCollider SnappableBoxCollider()
	{
		return null;
	}

	public override float GetInteractionDistance()
	{
		return 6f;
	}
}
