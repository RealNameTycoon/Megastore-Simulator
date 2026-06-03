using System;
using System.Collections;
using System.Collections.Generic;
using DFTGames.Localization;
using DG.Tweening;
using UnityEngine;

public class Tray : Clickable
{
	[SerializeField]
	private TrayShelf containerShelf;

	[SerializeField]
	private Transform corner1;

	[SerializeField]
	private Transform corner2;

	[SerializeField]
	private int trayID;

	[SerializeField]
	private Collider collider;

	private List<Product> containedProducts = new List<Product>();

	protected ProductType containedType = ProductType.NONE;

	private bool isCooked;

	public Action OnItemsFinished;

	private Action cookingFinishedAction;

	private Coroutine cookingRoutine;

	protected const string TRAY_PRODUCT_KEY = "TrayProduct";

	private const string TRAY_PRODUCT_COUNT_KEY = "TrayProductCount";

	private const string TRAY_COOKED = "TrayCooked";

	public TrayShelf ContainerShelf => containerShelf;

	public ProductType ContainedType => containedType;

	public bool IsCooked => isCooked;

	public int TrayID => trayID;

	protected Transform Corner1 => corner1;

	protected Transform Corner2 => corner2;

	public void StartCooking(Action onCookingFinished)
	{
		if (!isCooked)
		{
			cookingFinishedAction = onCookingFinished;
		}
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				((CookableProduct)containedProducts[i]).StartCooking();
			}
		}
		cookingRoutine = StartCoroutine(CookingRoutine());
	}

	private IEnumerator CookingRoutine()
	{
		collider.enabled = false;
		yield return new WaitForSeconds(CookableProduct.COOK_DURATION);
		FinishCooking();
	}

	public void OnDeactivateInstant()
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				((CookableProduct)containedProducts[i]).FinishCookingInstant();
			}
		}
		if (cookingRoutine != null)
		{
			StopCoroutine(cookingRoutine);
			FinishCooking();
		}
	}

	private void FinishCooking()
	{
		cookingFinishedAction?.Invoke();
		if (GetProductCount() != 0)
		{
			isCooked = true;
			MonoBehaviour.print(base.gameObject.name + " cooking done set true");
		}
		if (!containerShelf.IsReservedToStaff())
		{
			collider.enabled = true;
		}
		if (containerShelf != null)
		{
			containerShelf.OnCookingFinished();
		}
		SaveContent();
	}

	public ProductData GetContainedProduct()
	{
		if (containedProducts.Count == 0)
		{
			return null;
		}
		return containedProducts[GetFirstAvailableProductIndex()].Data;
	}

	private void Start()
	{
		EventManager.AddListener(PlaceableEvents.PLACE_PRODUCT, OnPlaceProduct);
	}

	public void AddItem(Product product)
	{
		if (product == null || product.Data == null || !product.Data.IsCookable())
		{
			return;
		}
		CookableProductData cookableProductData = (CookableProductData)product.Data;
		List<Vector3> list = MathUtils.CalculatePositions(corner1, corner2, cookableProductData.trayRowCount, cookableProductData.trayColumnCount, isVertical: false);
		product.transform.DOKill();
		product.transform.SetParent(base.transform);
		int firstEmptyProductIndex = GetFirstEmptyProductIndex();
		Vector3 to = list[firstEmptyProductIndex];
		product.transform.DoCurvedLocalMove(to, 0.5f, 2f);
		product.transform.DOLocalRotate(base.transform.forward, 0.5f);
		if (containedType == ProductType.NONE)
		{
			containedProducts = new List<Product>();
			for (int i = 0; i < list.Count; i++)
			{
				containedProducts.Add(null);
			}
			isCooked = true;
			containedType = cookableProductData.type;
		}
		containedProducts[firstEmptyProductIndex] = product;
		EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, product.Data.type, 1);
		if (containerShelf != null)
		{
			containerShelf.OnProductAdded();
		}
		SaveContent();
	}

	public Product RemoveAndGetItem()
	{
		containerShelf.OnBeforeProductRemoved();
		int firstAvailableProductIndex = GetFirstAvailableProductIndex();
		Product product = containedProducts[firstAvailableProductIndex];
		containedProducts[firstAvailableProductIndex] = null;
		EventManager.NotifyEvent(GameEvents.PRODUCT_REMOVED_FROM_BOX, product.Data.type, 1);
		if (GetProductCount() == 0)
		{
			OnItemsFinished?.Invoke();
			OnItemsFinished = null;
			isCooked = false;
			containedType = ProductType.NONE;
		}
		if (containerShelf != null)
		{
			containerShelf.OnProductRemoved();
		}
		SaveContent();
		return product;
	}

	private int GetFirstAvailableProductIndex()
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetFirstEmptyProductIndex()
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] == null)
			{
				return i;
			}
		}
		return 0;
	}

	public override void RepaintButtonsWindow()
	{
		Dictionary<KeyCode, (string, Action)> dictionary = new Dictionary<KeyCode, (string, Action)>();
		if (ContainerShelf.ParentFurniture.GetExtraButtonActions() != null)
		{
			foreach (var extraButtonAction in ContainerShelf.ParentFurniture.GetExtraButtonActions())
			{
				dictionary.Add(extraButtonAction.Item1, extraButtonAction.Item2);
			}
		}
		if (SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
		{
			dictionary.Add(KeyCode.Mouse1, ("place_move", delegate
			{
				ContainerShelf.StartParentPlacement();
			}));
		}
		dictionary.Add(KeyCode.Mouse0, (GetToolTip(), null));
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(dictionary, base.transform);
	}

	public virtual void PlaceProduct(Box box)
	{
		Product product = box.RemoveAndGetItem();
		PlaceProduct(product);
	}

	public void PlaceProduct(Product product)
	{
		ProductData data = product.Data;
		int capacityForProduct = GetCapacityForProduct(data);
		if (!data.IsCookable() || (containedType != ProductType.NONE && containedType != data.type) || GetProductCount() >= capacityForProduct)
		{
			return;
		}
		if (containedType == ProductType.NONE)
		{
			containedProducts = new List<Product>();
			for (int i = 0; i < capacityForProduct; i++)
			{
				containedProducts.Add(null);
			}
			isCooked = false;
			containedType = data.type;
		}
		CookableProductData obj = (CookableProductData)data;
		int trayRowCount = obj.trayRowCount;
		int trayColumnCount = obj.trayColumnCount;
		product.transform.SetParent(base.transform);
		int firstEmptyIndex = GetFirstEmptyIndex();
		List<Vector3> list = MathUtils.CalculatePositions(corner1, corner2, trayRowCount, trayColumnCount, isVertical: false);
		product.transform.DOLocalRotate(base.transform.forward, 0.5f);
		product.transform.DoCurvedLocalMove(list[firstEmptyIndex], 0.5f, 2f);
		containedProducts[firstEmptyIndex] = product;
		EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, product.Data.type, 1);
		if (containerShelf != null)
		{
			containerShelf.OnProductAdded(isNPC: true);
		}
		SaveContent();
	}

	protected virtual void OnPlaceProduct()
	{
		if (!base.Outline.enabled)
		{
			return;
		}
		ProductData carriedProduct = GetCarriedProduct();
		int capacityForProduct = GetCapacityForProduct(carriedProduct);
		if (containerShelf.IsReservedToStaff() && GetProductCount() == 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("reserved_staff_error_tray", base.transform);
		}
		else if (!carriedProduct.IsCookable())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowErrorWithFullText(Locale.GetWord("error_shelftype_different").Replace("{0}", Locale.GetWord(carriedProduct.shelfType.ToString())), base.transform);
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && isCooked && !IsEmpty())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowErrorWithFullText(Locale.GetWord("error_remove_baked"), base.transform);
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsBoxSpoiled())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_box_spoiled", base.transform);
		}
		else if ((containedType == ProductType.NONE || containedType == carriedProduct.type) && GetProductCount() < capacityForProduct)
		{
			if (SingletonBehaviour<TrayManager>.Instance.IsPicked && SingletonBehaviour<TrayManager>.Instance.PickedTray.IsCooked != isCooked && !IsEmpty())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowErrorWithFullText(Locale.GetWord("error_raw_baked_different"), base.transform);
				return;
			}
			if (containedType == ProductType.NONE)
			{
				containedProducts = new List<Product>();
				for (int i = 0; i < capacityForProduct; i++)
				{
					containedProducts.Add(null);
				}
				if (SingletonBehaviour<TrayManager>.Instance.IsPicked)
				{
					isCooked = SingletonBehaviour<TrayManager>.Instance.PickedTray.IsCooked;
				}
				else
				{
					isCooked = false;
				}
				containedType = carriedProduct.type;
			}
			CookableProductData obj = (CookableProductData)carriedProduct;
			int trayRowCount = obj.trayRowCount;
			int trayColumnCount = obj.trayColumnCount;
			Product product = RemoveAndGetItemFromCarrier();
			product.transform.SetParent(base.transform);
			int firstEmptyIndex = GetFirstEmptyIndex();
			List<Vector3> list = MathUtils.CalculatePositions(corner1, corner2, trayRowCount, trayColumnCount, isVertical: false);
			product.transform.DOLocalRotate(base.transform.forward, 0.5f);
			product.transform.DoCurvedLocalMove(list[firstEmptyIndex], 0.5f, 2f);
			HapticController.Vibrate(PresetType.MediumImpact);
			SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
			containedProducts[firstEmptyIndex] = product;
			EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, product.Data.type, 1);
			if (containerShelf != null)
			{
				containerShelf.OnProductAdded();
			}
			SaveContent();
			if (IsCarrierEmpty())
			{
				CloseInteractionElements();
			}
		}
		else if (containedType != carriedProduct.type && GetProductCount() < capacityForProduct)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_tray_different_product", base.transform);
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_tray_full", base.transform);
		}
	}

	private Product RemoveAndGetItemFromCarrier()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetPickedBox().RemoveAndGetItem();
		}
		return SingletonBehaviour<TrayManager>.Instance.PickedTray.RemoveAndGetItem();
	}

	private ProductData GetCarriedProduct()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetPickedBox().GetContainedProduct();
		}
		return SingletonBehaviour<TrayManager>.Instance.PickedTray.GetContainedProduct();
	}

	private bool IsCarrierEmpty()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty();
		}
		return SingletonBehaviour<TrayManager>.Instance.PickedTray.IsEmpty();
	}

	public virtual int GetProductCount()
	{
		int num = 0;
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public int GetCapacityForProduct(ProductData product)
	{
		if (!product.IsCookable())
		{
			return 0;
		}
		CookableProductData cookableProductData = (CookableProductData)product;
		return cookableProductData.trayRowCount * cookableProductData.trayColumnCount;
	}

	private void SaveContent()
	{
		GenericDataSerializer.SaveInt("TrayProductCount|" + trayID, GetProductCount());
		GenericDataSerializer.Save("TrayProduct|" + trayID, containedType);
		GenericDataSerializer.SaveBool("TrayCooked|" + trayID, isCooked);
	}

	public virtual int GetFirstEmptyIndex()
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] == null)
			{
				return i;
			}
		}
		return 0;
	}

	public bool IsEmpty()
	{
		return containedType == ProductType.NONE;
	}

	public void EnableCollider(bool enable)
	{
		collider.enabled = enable;
	}

	public void Release()
	{
		containerShelf.PutTrayBack(this);
		collider.enabled = true;
	}

	public void ReleaseAnimated()
	{
		containerShelf.PutTrayBackAnimated(this);
		collider.enabled = true;
	}

	public void ReleaseAnimatedNPC()
	{
		containerShelf.PutTrayBackAnimated(this, isNPC: true);
	}

	public void ChangeShelf(TrayShelf newShelf)
	{
		containerShelf.ReleaseTray();
		containerShelf = newShelf;
	}

	public void Initialize(int trayID, TrayShelf trayShelf)
	{
		containerShelf = trayShelf;
		this.trayID = trayID;
		int num = GenericDataSerializer.LoadInt("TrayProductCount|" + trayID);
		ProductType type = GenericDataSerializer.Load("TrayProduct|" + trayID, ProductType.NONE);
		isCooked = num != 0 && GenericDataSerializer.LoadBool("TrayCooked|" + trayID);
		if (num != 0)
		{
			FillItems(type, num);
		}
		if (!isCooked)
		{
			return;
		}
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				((CookableProduct)containedProducts[i]).CookInstantly();
			}
		}
	}

	private void FillItems(ProductType type, int count)
	{
		if (type == ProductType.NONE)
		{
			return;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(type);
		int capacityForProduct = GetCapacityForProduct(productData);
		if ((containedType != ProductType.NONE && containedType != productData.type) || containedProducts.Count >= capacityForProduct)
		{
			return;
		}
		CookableProductData obj = (CookableProductData)productData;
		int trayRowCount = obj.trayRowCount;
		int trayColumnCount = obj.trayColumnCount;
		List<Vector3> list = MathUtils.CalculatePositions(corner1, corner2, trayRowCount, trayColumnCount, isVertical: false);
		if (containedType == ProductType.NONE)
		{
			containedProducts = new List<Product>();
			for (int i = 0; i < capacityForProduct; i++)
			{
				containedProducts.Add(null);
			}
		}
		for (int j = 0; j < count; j++)
		{
			if (j <= list.Count - 1)
			{
				Product product = SingletonBehaviour<ProductPool>.Instance.GetProduct(type);
				product.transform.SetParent(base.transform);
				product.transform.localEulerAngles = base.transform.forward;
				product.transform.localPosition = list[j];
				containedProducts[j] = product;
			}
		}
		EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, productData.type, count);
		if (containedType == ProductType.NONE)
		{
			containedType = productData.type;
		}
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		if (base.Outline.enabled && !SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			if (containerShelf.IsReservedToStaff() || containerShelf.IsParentReservedToStaff())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("reserved_staff_error_tray", base.transform);
				return;
			}
			collider.enabled = false;
			SingletonBehaviour<TrayManager>.Instance.PickUpTray(this);
			containerShelf.OnTrayTaken();
		}
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty())
		{
			base.Outline.enabled = true;
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuWithPlace();
		}
		else if (SingletonBehaviour<TrayManager>.Instance.IsPicked && !SingletonBehaviour<TrayManager>.Instance.PickedTray.IsEmpty())
		{
			base.Outline.enabled = true;
			SingletonBehaviour<TrayManager>.Instance.UpdateMenuWithPlace();
		}
		else if (GenericBox.Instance.IsPicked && !GenericBox.Instance.IsEmpty())
		{
			base.Outline.enabled = true;
			GenericBox.Instance.UpdateMenuWithPlace();
		}
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
		if (base.Outline.enabled)
		{
			base.Outline.enabled = false;
		}
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		SingletonBehaviour<TrayManager>.Instance.UpdateMenu();
		GenericBox.Instance.UpdateMenu();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}
}
