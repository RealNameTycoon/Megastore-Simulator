using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BoxStorageUnit : Interactable, Restockable
{
	[SerializeField]
	private Collider collider;

	[SerializeField]
	private Outline boxShelfOutline;

	[SerializeField]
	private GameObject rackLabel;

	[SerializeField]
	private SpriteRenderer productSprite;

	[SerializeField]
	private TextMeshPro productCountText;

	[SerializeField]
	private Furniture parentFurniture;

	[SerializeField]
	private Clickable labelClickable;

	[SerializeField]
	private RestockZone restockZone;

	[SerializeField]
	private TemperatureZone temperatureZone;

	[SerializeField]
	private int unitID;

	[SerializeField]
	private Transform staffInteractionPoint;

	private bool isReservedToStaff;

	private const string BOXES_LIST_KEY = "BOXES_LIST";

	protected const string STORED_PRODUCT_TYPE = "BSU_STORED_PRODUCT_TYPE";

	private List<int> containedBoxIDs = new List<int>();

	private bool isHovered;

	private ProductType containedType = ProductType.NONE;

	private int productCount;

	private Action onPlacementEnded;

	private const int MAX_BOXES_TO_ENABLE_RENDERERS = 4;

	public int boxAmountToWaitForReturn;

	public RestockZone RestockZone => restockZone;

	public bool IsEmpty => containedBoxIDs.Count == 0;

	public ProductType ContainedType => containedType;

	public int ProductCount => productCount;

	public int BoxCount => containedBoxIDs.Count;

	public int BoxAmountToWaitForReturn => boxAmountToWaitForReturn;

	private string GetBoxesListKey()
	{
		if (parentFurniture.Type == FurnitureType.BOX_SHELF_SMALL)
		{
			return "BOXES_LIST" + parentFurniture.Type.ToString() + parentFurniture.FurnitureID + "|" + unitID;
		}
		return "BOXES_LIST" + parentFurniture.FurnitureID + "|" + unitID;
	}

	private string GetStoredProductTypeKey()
	{
		if (parentFurniture.Type == FurnitureType.BOX_SHELF_SMALL)
		{
			return "BSU_STORED_PRODUCT_TYPE" + parentFurniture.Type.ToString() + parentFurniture.FurnitureID + "|" + unitID;
		}
		return "BSU_STORED_PRODUCT_TYPE" + parentFurniture.FurnitureID + "|" + unitID;
	}

	private void Initialize()
	{
		EventManager.AddListener(PlaceableEvents.STORE_BOX, OnStoreBox);
		EventManager.AddListener(GameEvents.BOX_PICK_STARTED, OnBoxPickStarted);
		EventManager.AddListener<Box>(GameEvents.VEHICLE_BOX_PICK_STARTED, OnVehicleBoxPickStarted);
		EventManager.AddListener(StartupEvents.TEMPERATURE_ZONES_INITIALIZED, CheckAndUpdateTemperatureZone);
		labelClickable.onClickAction.AddListener(OnLabelClicked);
		labelClickable.onRightClickAction.AddListener(OnLabelRightClicked);
		labelClickable.SetHoverStartedAction(OnLabelHoverStarted);
	}

	public void InitializeOldBoxStorageUnit()
	{
		Initialize();
		containedBoxIDs = GenericDataSerializer.Load(GetBoxesListKey(), new List<int>());
		containedType = GenericDataSerializer.Load(GetStoredProductTypeKey(), ProductType.NONE);
		if (!SingletonBehaviour<ProductPool>.Instance.HasProductData(containedType))
		{
			containedType = ProductType.NONE;
		}
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] != -1)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
				if (box == null || box.IsInReturnArea)
				{
					list.Add(containedBoxIDs[i]);
					continue;
				}
				PositionContainer boxPositions = SingletonBehaviour<BoxManager>.Instance.GetBoxPositions(box.Type);
				box.OnAddedToBoxStorageUnit(this);
				box.transform.SetParent(base.transform);
				box.transform.localPosition = boxPositions.localPositions[num];
				box.transform.localEulerAngles = boxPositions.localEulers[num];
				box.Collider.enabled = false;
				productCount += GetProductCount(box);
				num++;
			}
		}
		if (list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				containedBoxIDs.Remove(list[j]);
			}
			SaveContent();
		}
		FixOldIssuesWithBoxes();
		CheckAndUpdateRestockZone();
		RefreshBoxRenderers();
		RepaintLabel();
		RegisterUnloaderJobs();
	}

	public void InitializeNewBoxStorageUnit()
	{
		Initialize();
		CheckAndUpdateRestockZone();
		CheckAndUpdateTemperatureZone();
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
			ProductType param = containedType;
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_ADDEDTO_CART_BOX_SHELF, param);
		}
		else if (productCount == 0)
		{
			labelClickable.gameObject.SetActive(value: false);
			containedType = ProductType.NONE;
			SaveContent();
			OnLabelRemoved();
		}
	}

	private void OnLabelRightClicked()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = containedType;
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_REMOVEDFROM_CART_BOX_SHELF, param);
		}
	}

	private void OnLabelHoverStarted()
	{
		ProductType param = containedType;
		EventManager.NotifyEvent(ProductEvents.PRICE_TAG_HOVER_STARTED, param);
	}

	public void Initialize(int palletID)
	{
		unitID = palletID;
	}

	private void OnStoreBox()
	{
		if (!isHovered)
		{
			return;
		}
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (!containedBoxIDs.Contains(pickedBox.BoxID) && !SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			ProductData anyContainedProductData = pickedBox.GetAnyContainedProductData();
			ProductType productType = ((anyContainedProductData == null) ? ProductType.NONE : anyContainedProductData.type);
			int count = SingletonBehaviour<BoxManager>.Instance.GetBoxPositions(pickedBox.Type).localPositions.Count;
			if (containedType != ProductType.NONE && containedType != productType && containedBoxIDs.Count < count && containedBoxIDs.Count > 0)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_different_product", base.transform);
			}
			else if (IsReservedToStaff() || IsReservedToRestocker())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pallet_reserved_staff_error", base.transform);
			}
			else if (containedBoxIDs.Count >= count)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_full", base.transform);
			}
			else
			{
				PlaceBox(pickedBox);
			}
		}
	}

	public void PlaceReturnBox(Box box, bool instant = false)
	{
		box.Collider.enabled = false;
		boxAmountToWaitForReturn--;
		box.ReleaseBoxReservation();
		PlaceBox(box, isNPC: true, instant);
	}

	public void PlaceBox(Box box, bool isNPC = false, bool instant = false)
	{
		ProductData anyContainedProductData = box.GetAnyContainedProductData();
		ProductType productType = ((anyContainedProductData == null) ? ProductType.NONE : anyContainedProductData.type);
		PositionContainer boxPositions = SingletonBehaviour<BoxManager>.Instance.GetBoxPositions(box.Type);
		_ = boxPositions.localPositions.Count;
		if (containedType != ProductType.NONE && containedType != productType && containedBoxIDs.Count != 0)
		{
			return;
		}
		containedBoxIDs.Add(box.BoxID);
		box.OnAddedToBoxStorageUnit(this);
		if (restockZone != null)
		{
			box.RegisterToRestockZone(restockZone);
		}
		if (temperatureZone != null)
		{
			box.RegisterToTemperatureZone(temperatureZone);
		}
		if (!isNPC)
		{
			SingletonBehaviour<BoxManager>.Instance.OnBoxPut();
			box.Collider.enabled = false;
		}
		if (box.IsOpen())
		{
			box.Close();
		}
		box.transform.SetParent(base.transform);
		box.OnBoxPut();
		if (instant)
		{
			box.transform.localPosition = boxPositions.localPositions[containedBoxIDs.Count - 1];
			box.transform.localEulerAngles = boxPositions.localEulers[containedBoxIDs.Count - 1];
			RefreshBoxRenderers();
		}
		else
		{
			box.transform.DOLocalRotate(boxPositions.localEulers[containedBoxIDs.Count - 1], 0.4f);
			box.transform.DoCurvedLocalMove(boxPositions.localPositions[containedBoxIDs.Count - 1], 0.4f, 2f).OnComplete(delegate
			{
				if (containedBoxIDs.Contains(box.BoxID))
				{
					RefreshBoxRenderers();
				}
				if (!isNPC)
				{
					SingletonBehaviour<RayShooter>.Instance.ImitateHover();
				}
			});
		}
		if (containedType == ProductType.NONE || containedType != productType)
		{
			containedType = productType;
		}
		productCount += GetProductCount(box);
		SaveContent();
		if (!isNPC)
		{
			CloseInteractionElements();
		}
		RepaintLabel();
		OnBoxPut();
		EventManager.NotifyEvent(GameEvents.STORAGE_UNIT_STACKED_BOX);
	}

	public void AddBoxInstant(Box boxToAdd)
	{
		if (containedBoxIDs.Contains(boxToAdd.BoxID))
		{
			return;
		}
		ProductData anyContainedProductData = boxToAdd.GetAnyContainedProductData();
		ProductType productType = ((anyContainedProductData == null) ? ProductType.NONE : anyContainedProductData.type);
		PositionContainer boxPositions = SingletonBehaviour<BoxManager>.Instance.GetBoxPositions(boxToAdd.Type);
		int count = boxPositions.localPositions.Count;
		if ((containedType == ProductType.NONE || containedType == productType || containedBoxIDs.Count >= count || containedBoxIDs.Count <= 0) && containedBoxIDs.Count < count && (containedType == ProductType.NONE || containedType == productType || containedBoxIDs.Count == 0))
		{
			containedBoxIDs.Add(boxToAdd.BoxID);
			RefreshBoxRenderers();
			boxToAdd.OnAddedToBoxStorageUnit(this);
			boxToAdd.Collider.enabled = false;
			boxToAdd.transform.SetParent(base.transform);
			boxToAdd.transform.localPosition = boxPositions.localPositions[containedBoxIDs.Count - 1];
			boxToAdd.transform.localEulerAngles = boxPositions.localEulers[containedBoxIDs.Count - 1];
			boxToAdd.OnBoxPut();
			if (containedType == ProductType.NONE || containedType != productType)
			{
				containedType = productType;
			}
			productCount += GetProductCount(boxToAdd);
			SaveContent();
			RepaintLabel();
		}
	}

	public void StartNewPlacement(Action onPlacementEnded = null)
	{
		if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			this.onPlacementEnded = onPlacementEnded;
			parentFurniture.StartNewPlacement(onPlacementEnded);
			OnMouseHoverEnded();
		}
	}

	private void OnBoxPickStarted()
	{
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (containedBoxIDs.Contains(pickedBox.BoxID))
		{
			RemoveBox(pickedBox);
			pickedBox.SaveLocation();
			RepaintLabel();
		}
	}

	private void OnVehicleBoxPickStarted(Box pickedBox)
	{
		if (containedBoxIDs.Contains(pickedBox.BoxID))
		{
			RemoveBox(pickedBox);
			RepaintLabel();
		}
	}

	public void RemoveBox(Box box, bool willReturn = false)
	{
		containedBoxIDs.Remove(box.BoxID);
		box.OnRemovedFromBoxStorageUnit();
		productCount -= GetProductCount(box);
		box.OnBoxRemovedFromPallet();
		box.EnableOutline(enable: false);
		box.Collider.enabled = true;
		RefreshBoxRenderers();
		RepaintLabel();
		SaveContent();
		if (!willReturn)
		{
			OnBoxTaken();
		}
		if (willReturn)
		{
			boxAmountToWaitForReturn++;
		}
	}

	public void OnReturnBoxCanceled()
	{
		boxAmountToWaitForReturn--;
		OnBoxTaken();
	}

	private void SaveContent()
	{
		GenericDataSerializer.Save(GetBoxesListKey(), containedBoxIDs);
		GenericDataSerializer.Save(GetStoredProductTypeKey(), containedType);
	}

	private void RepaintLabel()
	{
		if (containedType != ProductType.NONE)
		{
			rackLabel.SetActive(value: true);
			productCountText.text = productCount.ToString();
			ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(containedType);
			SetSpriteKeepSize(productSprite, anyProductData.productSprite);
		}
		else
		{
			rackLabel.SetActive(value: false);
		}
	}

	public bool HasProductLabel()
	{
		return rackLabel.activeSelf;
	}

	private void SetSpriteKeepSize(SpriteRenderer sr, Sprite newSprite)
	{
		Sprite sprite = sr.sprite;
		Vector3 vector = (sprite ? sprite.bounds.size : newSprite.bounds.size);
		sr.sprite = newSprite;
		Vector3 size = newSprite.bounds.size;
		Transform obj = sr.transform;
		Vector3 localScale = obj.localScale;
		if (size.x != 0f)
		{
			localScale.x *= vector.x / size.x;
		}
		if (size.y != 0f)
		{
			localScale.y *= vector.y / size.y;
		}
		obj.localScale = localScale;
	}

	public void EnableOutline(bool enable)
	{
		boxShelfOutline.enabled = enable;
		EnableBoxOutlines(enable);
	}

	public void EnableCollider(bool enable)
	{
		collider.enabled = enable;
	}

	private void FixOldIssuesWithBoxes()
	{
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
			if (box != null && !box.Stored)
			{
				box.UnregisterFromRestockZone();
				box.UnregisterFromTemperatureZone();
				box.SetBoxStored();
			}
		}
	}

	private void RefreshBoxRenderers()
	{
		int num = 0;
		for (int num2 = containedBoxIDs.Count - 1; num2 >= 0; num2--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num2]);
			if (box != null)
			{
				PositionContainer boxPositions = SingletonBehaviour<BoxManager>.Instance.GetBoxPositions(box.Type);
				box.DOKill();
				box.transform.localPosition = boxPositions.localPositions[num2];
				box.transform.localEulerAngles = boxPositions.localEulers[num2];
			}
			if (box != null && num < 4)
			{
				box.EnableRenderers(enable: true);
				num++;
			}
			else if (box != null)
			{
				box.EnableRenderers(enable: false);
			}
		}
	}

	private int GetProductCount(Box box)
	{
		if (box.IsEmpty())
		{
			return 0;
		}
		if (box.GetAnyContainedProductData().type >= ProductType.NONCONSUMABLE_START)
		{
			return 1;
		}
		return box.ProductCount;
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		boxShelfOutline.enabled = productCount == 0;
		isHovered = true;
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir || GenericBox.Instance.IsPicked)
		{
			return;
		}
		if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
		{
			SingletonBehaviour<VehicleManager>.Instance.UpdateMenuPalletHovered(TakeLastBox);
			EnableBoxOutlines(enable: true);
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuWithStack();
			EnableBoxOutlines(enable: true);
		}
		else if (containedBoxIDs.Count >= 0)
		{
			Dictionary<KeyCode, (string, Action)> dictionary = new Dictionary<KeyCode, (string, Action)>();
			if (parentFurniture.GetExtraButtonActions() != null)
			{
				foreach (var extraButtonAction in parentFurniture.GetExtraButtonActions())
				{
					dictionary.Add(extraButtonAction.Item1, extraButtonAction.Item2);
				}
			}
			dictionary.Add(KeyCode.Mouse1, ("place_move", delegate
			{
				StartNewPlacement();
			}));
			if (SingletonBehaviour<BoxManager>.Instance.NoContainerPicked() && containedBoxIDs.Count > 0)
			{
				dictionary.Add(KeyCode.Mouse0, ("take_box", TakeLastBox));
			}
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(dictionary, base.transform);
			EnableBoxOutlines(enable: true);
		}
		if (containedType != ProductType.NONE)
		{
			OpenBoxInfoWindow();
		}
	}

	private void OpenBoxInfoWindow()
	{
		ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(containedType);
		if (containedBoxIDs.Count == 0)
		{
			SingletonBehaviour<BoxInfoWindow>.Instance.Open(anyProductData, productCount);
			return;
		}
		float num = 100f;
		float num2 = 0f;
		float num3 = 100f;
		float num4 = 0f;
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
			if (box != null)
			{
				num = Mathf.Min(num, box.FreshnessSpoilageProgress);
				num2 = Mathf.Max(num2, box.FreshnessSpoilageProgress);
				num3 = Mathf.Min(num3, box.FrozenDamageProgress);
				num4 = Mathf.Max(num4, box.FrozenDamageProgress);
			}
		}
		SingletonBehaviour<BoxInfoWindow>.Instance.Open(anyProductData, productCount, temperatureZone.Temperature, num, num2, num3, num4);
	}

	private void TakeLastBox()
	{
		if (containedBoxIDs.Count <= 0)
		{
			return;
		}
		Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[containedBoxIDs.Count - 1]);
		if (!(box == null))
		{
			if (box.ReservedForEmployeeID != -1 || IsReservedToStaff())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pallet_reserved_staff_error", base.transform);
			}
			else
			{
				box.OnMouseButtonDown();
			}
		}
	}

	public bool IsReservedToStaff()
	{
		if (containedBoxIDs.Count > 0)
		{
			for (int i = 0; i < containedBoxIDs.Count; i++)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
				if (box != null && box.IsReservedForRestocking)
				{
					return true;
				}
			}
		}
		if (isReservedToStaff || IsReservedToRestocker())
		{
			return true;
		}
		return false;
	}

	public bool IsReservedToRestocker()
	{
		return boxAmountToWaitForReturn > 0;
	}

	public Box GetLastBox()
	{
		if (containedBoxIDs.Count > 0)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[containedBoxIDs.Count - 1]);
		}
		return null;
	}

	public Box GetLastUnreservedBox()
	{
		for (int num = containedBoxIDs.Count - 1; num >= 0; num--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null && !box.IsReservedForRestocking)
			{
				return box;
			}
		}
		return null;
	}

	public Box GetLowestStockBox()
	{
		Box result = null;
		int num = int.MaxValue;
		for (int num2 = containedBoxIDs.Count - 1; num2 >= 0; num2--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num2]);
			if (box != null && !box.IsReservedForRestocking)
			{
				int num3 = box.ProductCount;
				if (num3 != 0 && num3 < num)
				{
					result = box;
					num = num3;
				}
			}
		}
		return result;
	}

	public bool IsLastBox(Box box)
	{
		if (containedBoxIDs.Count > 0)
		{
			return containedBoxIDs[containedBoxIDs.Count - 1] == box.BoxID;
		}
		return false;
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
		SingletonBehaviour<BoxInfoWindow>.Instance.Close();
		if (boxShelfOutline != null)
		{
			boxShelfOutline.enabled = false;
		}
		EnableBoxOutlines(enable: false);
		isHovered = false;
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		FireExtinguisher.Instance.UpdateMenu();
		SingletonBehaviour<VehicleManager>.Instance.UpdateMenu();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void EnableBoxOutlines(bool enable)
	{
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
			if (box != null)
			{
				box.EnableOutline(enable);
			}
		}
	}

	public void DeleteData()
	{
		GenericDataSerializer.DeleteKey(GetBoxesListKey());
		GenericDataSerializer.DeleteKey(GetStoredProductTypeKey());
	}

	public void OnPlacementEnded()
	{
		onPlacementEnded?.Invoke();
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public void CheckAndUpdateRestockZone()
	{
		restockZone = SingletonBehaviour<RestockZoneManager>.Instance.GetRestockZoneAtPosition(base.transform.position);
		if (!(restockZone != null))
		{
			return;
		}
		for (int num = containedBoxIDs.Count - 1; num >= 0; num--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null)
			{
				box.RegisterToRestockZone(restockZone);
			}
		}
	}

	public void CheckAndUpdateTemperatureZone()
	{
		temperatureZone = SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(base.transform.position);
		if (!(temperatureZone != null))
		{
			return;
		}
		for (int num = containedBoxIDs.Count - 1; num >= 0; num--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null)
			{
				box.RegisterToTemperatureZone(temperatureZone);
			}
		}
	}

	public void UnregisterFromRestockZone()
	{
		if (!(restockZone != null))
		{
			return;
		}
		foreach (int containedBoxID in containedBoxIDs)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxID);
			if (box != null)
			{
				box.UnregisterFromRestockZone();
			}
		}
	}

	public bool CanPack()
	{
		return false;
	}

	public override float GetInteractionDistance()
	{
		return 6f;
	}

	public BoxCollider SnappableBoxCollider()
	{
		return null;
	}

	public bool PlacedBefore()
	{
		return true;
	}

	public virtual bool IsCancelable()
	{
		return false;
	}

	public bool isEmpty()
	{
		return !rackLabel.activeSelf;
	}

	public Transform PickupPoint()
	{
		return staffInteractionPoint;
	}

	public int GetProductCount()
	{
		return containedBoxIDs.Count + boxAmountToWaitForReturn;
	}

	public int GetCapacityForProduct(ProductData productData)
	{
		return SingletonBehaviour<BoxManager>.Instance.GetBoxPositions(productData.boxType).localPositions.Count;
	}

	public void PlaceProduct(Box box, bool instant = false)
	{
		box.Collider.enabled = false;
		PlaceBox(box, isNPC: true, instant);
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

	public ProductGroup GetProductGroup()
	{
		if (containedType == ProductType.NONE)
		{
			return ProductGroup.NONE;
		}
		return SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType).productGroup;
	}

	public ProductType ContainedProductType()
	{
		return containedType;
	}

	public ProductType PreviousContainedProductType()
	{
		return containedType;
	}

	public string RestocableID()
	{
		return parentFurniture.Type.ToString() + parentFurniture.FurnitureID + "|" + unitID;
	}

	public bool IsBakeryPlaceable()
	{
		return false;
	}

	public bool IsUnloaderPlaceable()
	{
		return true;
	}

	public bool IsAvailableForRestocking()
	{
		return true;
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
		return parentFurniture.transform;
	}

	public bool IsPlayerReserved()
	{
		return false;
	}

	public void OnPacked()
	{
		EventManager.NotifyEvent(PlaceableEvents.RESTOCKABLE_PACKED, (Restockable)this);
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
}
