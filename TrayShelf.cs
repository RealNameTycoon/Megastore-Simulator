using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TrayShelf : Clickable, Restockable
{
	[SerializeField]
	private Furniture parentFurniture;

	[SerializeField]
	private int shelfID;

	[SerializeField]
	private Tray containedTray;

	[SerializeField]
	private int containedTrayID = -1;

	[SerializeField]
	private BoxCollider clickableCollider;

	[SerializeField]
	private bool isStorageShelf;

	[SerializeField]
	private GameObject trayHighlight;

	[SerializeField]
	private GameObject label;

	[SerializeField]
	private TextMeshPro labelText;

	[SerializeField]
	private SpriteRenderer labelImage;

	private bool trayTaken;

	private static string CONTAINED_TRAY_ID_KEY = "CONTAINED_TRAY_ID_KEY";

	private static string LAST_CONTAINED_TYPE_KEY = "LAST_CONTAINED_TYPE_KEY";

	private ProductType lastContainedType = ProductType.NONE;

	[SerializeField]
	private bool isReservedToStaff;

	[SerializeField]
	private bool isPlayerReserved;

	private Coroutine disablePlayerReservedCoroutine;

	private WaitForSeconds waiter = new WaitForSeconds(10f);

	public int ContainedTrayID => containedTrayID;

	public Tray ContainedTray => containedTray;

	public Furniture ParentFurniture => parentFurniture;

	public bool HasLabel()
	{
		return label.activeSelf;
	}

	private void Start()
	{
		Clickable component = label.GetComponent<Clickable>();
		if (component != null)
		{
			component.onClickAction.AddListener(OnLabelClicked);
			component.onRightClickAction.AddListener(OnLabelRightClicked);
			component.SetHoverStartedAction(OnLabelHoverStarted);
		}
	}

	private void OnLabelClicked()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = lastContainedType;
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_ADDEDTO_CART, param);
		}
		else if (containedTrayID == -1 || containedTray.IsEmpty())
		{
			if (IsReservedToStaff())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("reserved_staff_error_tray", base.transform);
				return;
			}
			label.SetActive(value: false);
			lastContainedType = ProductType.NONE;
			SaveLabel();
			EventManager.NotifyEvent(PlaceableEvents.TRACK_LABEL_REMOVED, this);
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
		GenericDataSerializer.Save(parentFurniture.Type.ToString() + parentFurniture.FurnitureID + LAST_CONTAINED_TYPE_KEY + shelfID, lastContainedType);
	}

	public bool IsTrayEmpty()
	{
		if (!(containedTray == null))
		{
			return containedTray.IsEmpty();
		}
		return true;
	}

	public bool IsTrayCooked()
	{
		if (containedTray != null)
		{
			return containedTray.IsCooked;
		}
		return false;
	}

	public bool HasInitializedBefore()
	{
		return GenericDataSerializer.HasKey(parentFurniture.Type.ToString() + parentFurniture.FurnitureID + CONTAINED_TRAY_ID_KEY + shelfID);
	}

	public void EnableClickableCollider(bool enable)
	{
		clickableCollider.enabled = enable;
	}

	public void InitializeNew()
	{
		EventManager.AddListener(GameEvents.TRAY_PUTDOWN, OnTrayPutDown);
		if (isStorageShelf)
		{
			containedTray.gameObject.SetActive(value: false);
			clickableCollider.enabled = true;
			containedTrayID = -1;
			return;
		}
		containedTrayID = SingletonBehaviour<TrayManager>.Instance.SpawnNewTrayID();
		containedTray.Initialize(containedTrayID, this);
		containedTray.gameObject.SetActive(value: true);
		clickableCollider.enabled = false;
		SaveTrayID();
		SingletonBehaviour<RestockerManager>.Instance.CheckAndRegisterShelf(this);
	}

	public void InitializeOld()
	{
		EventManager.AddListener(GameEvents.TRAY_PUTDOWN, OnTrayPutDown);
		containedTrayID = GenericDataSerializer.Load(parentFurniture.Type.ToString() + parentFurniture.FurnitureID + CONTAINED_TRAY_ID_KEY + shelfID, -1);
		lastContainedType = GenericDataSerializer.Load(parentFurniture.Type.ToString() + parentFurniture.FurnitureID + LAST_CONTAINED_TYPE_KEY + shelfID, ProductType.NONE);
		if (containedTrayID != -1)
		{
			containedTray.Initialize(containedTrayID, this);
			containedTray.gameObject.SetActive(value: true);
			clickableCollider.enabled = false;
			SingletonBehaviour<BakerManager>.Instance.RegisterTrayShelf(this);
			RefreshLabel();
			SingletonBehaviour<RestockerManager>.Instance.CheckAndRegisterShelf(this);
		}
		else
		{
			containedTray.gameObject.SetActive(value: false);
			clickableCollider.enabled = true;
			RefreshLabel();
		}
	}

	public void StartParentPlacement()
	{
		parentFurniture.StartNewPlacement();
	}

	private void OnTrayPutDown()
	{
		if (!trayHighlight.activeSelf)
		{
			return;
		}
		Tray pickedTray = SingletonBehaviour<TrayManager>.Instance.PickedTray;
		if (parentFurniture.Type == FurnitureType.OVEN && !isStorageShelf)
		{
			if (((Oven)parentFurniture).Cooking)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_oven_cooking_put", base.transform);
				return;
			}
		}
		else
		{
			if (IsParentReservedToStaff())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("oven_reserved_staff_error", base.transform);
				return;
			}
			if (IsReservedToStaff())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("reserved_staff_error", base.transform);
				return;
			}
		}
		if (CanPutTray(pickedTray))
		{
			PutTray(pickedTray);
		}
	}

	public void PutTray(Tray tray)
	{
		if (containedTrayID == -1)
		{
			tray.ChangeShelf(this);
			containedTray = tray;
			containedTrayID = tray.TrayID;
			SaveTrayID();
			RefreshLabel();
			trayTaken = false;
			SingletonBehaviour<BakerManager>.Instance.RegisterTrayShelf(this);
			SingletonBehaviour<RestockerManager>.Instance.AvailablitiyChanged(this);
		}
		else
		{
			trayHighlight.SetActive(value: false);
			clickableCollider.enabled = false;
			trayTaken = false;
			SingletonBehaviour<RestockerManager>.Instance.AvailablitiyChanged(this);
		}
	}

	public void ReleaseTray()
	{
		if (containedTray.ContainedType != ProductType.NONE)
		{
			SingletonBehaviour<BakerManager>.Instance.UnregisterTrayShelf(this, containedTray.ContainedType);
		}
		containedTray = null;
		containedTrayID = -1;
		RefreshLabel();
		SaveTrayID();
		clickableCollider.enabled = true;
		SingletonBehaviour<RestockerManager>.Instance.AvailablitiyChanged(this);
	}

	private void SaveTrayID()
	{
		GenericDataSerializer.SaveInt(parentFurniture.Type.ToString() + parentFurniture.FurnitureID + CONTAINED_TRAY_ID_KEY + shelfID, containedTrayID);
	}

	public void PutTrayBack(Tray tray)
	{
		tray.transform.DOKill();
		tray.transform.SetParent(base.transform);
		tray.transform.localPosition = Vector3.zero;
		tray.transform.localEulerAngles = Vector3.zero;
		clickableCollider.enabled = false;
		trayTaken = false;
		DisablePlayerReserved();
		SingletonBehaviour<RestockerManager>.Instance.AvailablitiyChanged(this);
	}

	public void PutTrayBackAnimated(Tray tray, bool isNPC = false)
	{
		tray.transform.DOKill();
		tray.transform.SetParent(base.transform);
		if (!isNPC)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
		}
		tray.transform.DOLocalMove(Vector3.zero, 0.3f);
		tray.transform.DOLocalRotate(Vector3.zero, 0.3f);
		clickableCollider.enabled = false;
		trayTaken = false;
		if (!isNPC)
		{
			DisablePlayerReserved();
		}
		SingletonBehaviour<RestockerManager>.Instance.AvailablitiyChanged(this);
	}

	public override void OnMouseHoverStarted()
	{
		if (SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			base.OnMouseHoverStarted();
			Tray pickedTray = SingletonBehaviour<TrayManager>.Instance.PickedTray;
			if (CanPutTray(pickedTray))
			{
				trayHighlight.SetActive(value: true);
				SingletonBehaviour<TrayManager>.Instance.UpdateMenuWithPut();
			}
		}
	}

	private bool CanPutTray(Tray tray)
	{
		if (tray.TrayID != containedTrayID)
		{
			return containedTrayID == -1;
		}
		return true;
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		if (SingletonBehaviour<TrayManager>.Instance.IsPicked && SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			SingletonBehaviour<TrayManager>.Instance.UpdateMenu();
		}
		trayHighlight.SetActive(value: false);
	}

	private void RefreshLabel()
	{
		if (parentFurniture.Type == FurnitureType.OVEN)
		{
			return;
		}
		if (containedTrayID != -1)
		{
			labelText.text = containedTray.GetProductCount().ToString();
			if (!containedTray.IsEmpty())
			{
				if (lastContainedType != containedTray.GetContainedProduct().type)
				{
					lastContainedType = containedTray.GetContainedProduct().type;
					SaveLabel();
				}
				labelImage.sprite = containedTray.GetContainedProduct().productSprite;
				label.SetActive(value: true);
			}
			else if (lastContainedType != ProductType.NONE)
			{
				ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(lastContainedType);
				labelText.text = "0";
				labelImage.sprite = productData.productSprite;
				label.SetActive(value: true);
			}
		}
		else if (lastContainedType != ProductType.NONE)
		{
			ProductData productData2 = SingletonBehaviour<ProductPool>.Instance.GetProductData(lastContainedType);
			labelText.text = "0";
			labelImage.sprite = productData2.productSprite;
			label.SetActive(value: true);
		}
		else
		{
			labelText.text = "0";
		}
	}

	public void OnCookingFinished()
	{
		SingletonBehaviour<BakerManager>.Instance.OnCooked(this);
	}

	public void OnProductAdded(bool isNPC = false)
	{
		if (containedTray.ContainedType != ProductType.NONE && containedTray.GetProductCount() == 1)
		{
			SingletonBehaviour<BakerManager>.Instance.RegisterTrayShelf(this);
		}
		RefreshLabel();
		EventManager.NotifyTrayProductAddedEvent(this, ContainedProductType());
		if (!isNPC)
		{
			DisablePlayerReserved();
		}
	}

	public void OnBeforeProductRemoved()
	{
		if (containedTray.ContainedType != ProductType.NONE && containedTray.GetProductCount() == 1)
		{
			SingletonBehaviour<BakerManager>.Instance.UnregisterTrayShelf(this, containedTray.ContainedType);
		}
	}

	public void OnProductRemoved()
	{
		RefreshLabel();
		EventManager.NotifyTrayProductRemovedEvent(this, ContainedProductType());
		DisablePlayerReserved();
	}

	public void OnTrayTaken()
	{
		trayTaken = true;
		SingletonBehaviour<RestockerManager>.Instance.AvailablitiyChanged(this);
	}

	public ProductType ContainedProductType()
	{
		if (containedTrayID == -1)
		{
			return ProductType.NONE;
		}
		return containedTray.ContainedType;
	}

	public ProductType PreviousContainedProductType()
	{
		return lastContainedType;
	}

	public Transform PickupPoint()
	{
		return parentFurniture.StaffInteractionPoint;
	}

	public bool HasProductLabel()
	{
		return label.activeSelf;
	}

	public int GetProductCount()
	{
		if (containedTrayID != -1)
		{
			return containedTray.GetProductCount();
		}
		return 0;
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public int GetCapacityForProduct(ProductData productData)
	{
		if (containedTrayID != -1)
		{
			return containedTray.GetCapacityForProduct(productData);
		}
		return 0;
	}

	public void PlaceProduct(Box box, bool instant = false)
	{
		containedTray.PlaceProduct(box);
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

	public bool IsParentReservedToStaff()
	{
		if (parentFurniture.Type == FurnitureType.OVEN)
		{
			return ((Oven)parentFurniture).IsReservedToStaff;
		}
		return false;
	}

	public ProductGroup GetProductGroup()
	{
		return ProductGroup.BAKERY;
	}

	public string RestocableID()
	{
		return parentFurniture.Type.ToString() + parentFurniture.FurnitureID + "|" + shelfID;
	}

	public bool IsBakeryPlaceable()
	{
		return false;
	}

	public bool IsAvailableForRestocking()
	{
		if (containedTrayID != -1)
		{
			return !trayTaken;
		}
		return false;
	}

	public void PlaceProduct(Tray tray)
	{
		throw new NotImplementedException();
	}

	public Transform StaffInteractionPoint()
	{
		return parentFurniture.StaffInteractionPoint;
	}

	public bool IsCookedShelf()
	{
		if (containedTray != null)
		{
			return containedTray.IsCooked;
		}
		return false;
	}

	public Transform ParentTransform()
	{
		return parentFurniture.transform;
	}

	public bool IsPlayerReserved()
	{
		return isPlayerReserved;
	}

	public void DisablePlayerReserved()
	{
		isPlayerReserved = true;
		if (disablePlayerReservedCoroutine != null)
		{
			StopCoroutine(disablePlayerReservedCoroutine);
		}
		disablePlayerReservedCoroutine = StartCoroutine(DisablePlayerReservedCoroutine());
	}

	private IEnumerator DisablePlayerReservedCoroutine()
	{
		yield return waiter;
		isPlayerReserved = false;
	}

	public bool IsUnloaderPlaceable()
	{
		return false;
	}

	public void OnPacked()
	{
		EventManager.NotifyEvent(PlaceableEvents.RESTOCKABLE_PACKED, (Restockable)this);
	}
}
