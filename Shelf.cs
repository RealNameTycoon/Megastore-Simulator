using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Shelf : Interactable, Restockable
{
	[SerializeField]
	private Placeable parentPlaceable;

	[SerializeField]
	private Transform corner1;

	[SerializeField]
	private Transform corner2;

	[SerializeField]
	private int shelfID;

	[SerializeField]
	private GameObject priceTag;

	[SerializeField]
	private TextMeshPro textMesh;

	[SerializeField]
	private TextMeshPro soldOutText;

	[SerializeField]
	private GameObject labelDefault;

	[SerializeField]
	private GameObject labelDiscount;

	[SerializeField]
	private TextMeshPro textProductCount;

	[SerializeField]
	private SpriteRenderer productSprite;

	[SerializeField]
	private GameObject saleRibbon;

	[SerializeField]
	private Outline outline;

	[SerializeField]
	private bool alwaysStackVertically;

	[SerializeField]
	private int fixedColumnCount = -1;

	[SerializeField]
	private int fixedRowCount = -1;

	[SerializeField]
	private PlaceableType placeableTypeOverride = PlaceableType.NONE;

	[SerializeField]
	private int productCount;

	[SerializeField]
	private bool disableCulling;

	public static float PLACE_ANIMATION_DURATION = 0.5f;

	private Coroutine disablePlayerReservedCoroutine;

	public const float DISABLE_PLAYER_RESERVED_DURATION = 10f;

	private WaitForSeconds waiter = new WaitForSeconds(10f);

	private bool isDirty = true;

	private bool culled;

	private bool isReservedToStaff;

	private static float HOVER_DETECTION_DISTANCE = 4f;

	private static float PLACE_HOLD_DURATION = 1.2f;

	protected const string SHELF_PRODUCT_KEY = "ShelfProduct";

	private const string SHELF_PRODUCT_COUNT_KEY = "ShelfProductCount";

	private const string SHELF_PREVIOUS_PRODUCT_KEY = "ShelfPreviousProduct";

	private bool holdStarted;

	private float fillAmount;

	protected bool isHovered;

	[SerializeField]
	private bool isPlayerReserved;

	[SerializeField]
	private List<Product> containedProducts = new List<Product>();

	private List<bool> occupationStatus = new List<bool>();

	[SerializeField]
	protected ProductType containedType = ProductType.NONE;

	[SerializeField]
	protected ProductType previousContainedType = ProductType.NONE;

	private List<Transform> containedContainers = new List<Transform>();

	public Placeable ParentPlaceable => parentPlaceable;

	public int ShelfID => shelfID;

	protected Transform Corner1 => corner1;

	protected Transform Corner2 => corner2;

	protected GameObject PriceTag => priceTag;

	protected TextMeshPro PriceText => textMesh;

	public ProductType PreviousContainedType => previousContainedType;

	public PlaceableType ParentPlaceableType
	{
		get
		{
			if (placeableTypeOverride == PlaceableType.NONE)
			{
				return parentPlaceable.Type;
			}
			return placeableTypeOverride;
		}
	}

	public ProductType ContainedType => containedType;

	public virtual bool IsReservedToStaff()
	{
		return isReservedToStaff;
	}

	public virtual void SetReservedToStaff(bool value)
	{
		isReservedToStaff = value;
	}

	public virtual void ClearReservedToStaff()
	{
		isReservedToStaff = false;
	}

	public void AssignLabelParameters()
	{
		textMesh = priceTag.transform.Find("TextPrice").GetComponent<TextMeshPro>();
		labelDefault = priceTag.transform.Find("LabelDefault").gameObject;
		labelDiscount = priceTag.transform.Find("LabelDiscount").gameObject;
		textProductCount = priceTag.transform.Find("TextProductCount").GetComponent<TextMeshPro>();
		productSprite = priceTag.transform.Find("ProductSprite").GetComponent<SpriteRenderer>();
		soldOutText = priceTag.transform.Find("SoldOutText").GetComponent<TextMeshPro>();
		saleRibbon = priceTag.transform.Find("SaleRibbon").gameObject;
	}

	protected virtual void Start()
	{
		EventManager.AddListener(PlaceableEvents.PLACE_PRODUCT, OnPlaceProduct);
		EventManager.AddListener(PlaceableEvents.TAKE_PRODUCT, OnTakeProduct);
		EventManager.AddListener<ProductType, float>(ProductEvents.PRODUCT_PRICE_CHANGED, OnPriceChanged);
		Clickable component = priceTag.GetComponent<Clickable>();
		if (component != null)
		{
			component.onClickAction.AddListener(OnSetPrice);
			component.onRightClickAction.AddListener(OnSetPriceRightClick);
			component.SetHoverStartedAction(OnLabelHoverStarted);
		}
	}

	public void FillShelfWith(ProductData data)
	{
		int num = GetColumnCount(data) * GetRowCount(data);
		ProductType type = data.type;
		if (num != 0)
		{
			FillItems(type, num);
		}
		SaveShelfContent();
	}

	public int GetRowCount(ProductData data)
	{
		if (fixedRowCount != -1)
		{
			return fixedRowCount;
		}
		return data.shelfRowCount;
	}

	public int GetColumnCount(ProductData data)
	{
		if (fixedColumnCount != -1)
		{
			return fixedColumnCount;
		}
		return data.shelfColumnCount;
	}

	public virtual int GetProductCount()
	{
		return productCount;
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

	public bool IsProductAvailable()
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null && !containedProducts[i].isReserved)
			{
				return true;
			}
		}
		return false;
	}

	public List<Product> AvailableProducts()
	{
		List<Product> list = new List<Product>();
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null && !containedProducts[i].isReserved)
			{
				list.Add(containedProducts[i]);
			}
		}
		return list;
	}

	public Product GetRandomProduct()
	{
		List<Product> list = new List<Product>();
		for (int num = containedProducts.Count - 1; num >= 0; num--)
		{
			if (containedProducts[num] != null && !containedProducts[num].isReserved)
			{
				return containedProducts[num];
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	private void OnPriceChanged(ProductType type, float newPrice)
	{
		if (containedType == type)
		{
			RepaintPriceTagForPrice(newPrice);
		}
	}

	private void OnTakeProduct()
	{
		if (!isHovered)
		{
			return;
		}
		if (containedType == ProductType.NONE)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_empty", base.transform);
			return;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType);
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		Tray pickedTray = SingletonBehaviour<TrayManager>.Instance.PickedTray;
		bool flag = productData.IsCookable();
		if (flag && SingletonBehaviour<TrayManager>.Instance.PickedTray == null)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_need_tray", base.transform);
			return;
		}
		if (pickedBox != null && pickedBox.Type != productData.boxType)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("error_different_box").Replace("{0}", Locale.GetWord(productData.boxType.ToString())), base.transform);
			return;
		}
		if (pickedBox != null && !pickedBox.IsEmpty() && pickedBox.GetContainedProduct().type != productData.type)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("error_box_different_product"), base.transform);
			return;
		}
		if (pickedTray != null && !pickedTray.IsEmpty() && !pickedTray.IsCooked)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_raw_baked_different", base.transform);
			return;
		}
		if (pickedTray != null && !pickedTray.IsEmpty() && pickedTray.GetContainedProduct().type != productData.type)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("error_box_different_product"), base.transform);
			return;
		}
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			if (SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsBoxFrozen())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_box_frozen", base.transform);
				return;
			}
			if (SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsBoxSpoiled())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_box_spoiled", base.transform);
				return;
			}
		}
		Product randomProduct = GetRandomProduct();
		if (randomProduct == null)
		{
			if (containedProducts.Count > 0)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowError("cant_take", base.transform);
			}
			return;
		}
		if (GenericBox.Instance.IsPicked && !GenericBox.Instance.CanTakeProduct(randomProduct))
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("error_generic_box_capacity"), base.transform);
			return;
		}
		RemoveProduct(randomProduct);
		DisablePlayerReserved();
		randomProduct.OnBeforeTake();
		if (flag)
		{
			if (GenericBox.Instance.IsPicked)
			{
				GenericBox.Instance.AddItem(randomProduct);
				bool flag2 = GenericBox.Instance.HasSpace();
				bool flag3 = GetProductCount() > 0;
				GenericBox.Instance.UpdateMenuWithPlace(flag2 && flag3);
			}
			else
			{
				pickedTray.AddItem(randomProduct);
				CookableProductData cookableProductData = (CookableProductData)pickedTray.GetContainedProduct();
				bool flag4 = pickedTray.GetProductCount() < cookableProductData.trayRowCount * cookableProductData.trayColumnCount;
				bool flag5 = GetProductCount() > 0;
				SingletonBehaviour<TrayManager>.Instance.UpdateMenuWithPlace(flag4 && flag5);
			}
		}
		else if (GenericBox.Instance.IsPicked)
		{
			GenericBox.Instance.AddItem(randomProduct);
			bool flag6 = GenericBox.Instance.HasSpace();
			bool flag7 = GetProductCount() > 0;
			GenericBox.Instance.UpdateMenuWithPlace(flag6 && flag7);
		}
		else
		{
			pickedBox.AddItem(randomProduct);
			bool flag8 = pickedBox.ProductCount < pickedBox.GetContainedProduct().GetMaxProductCount();
			bool flag9 = GetProductCount() > 0;
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuWithPlace(flag8 && flag9);
		}
		RepaintPriceTagForProductCount();
		CheckAndUpdateContainers();
		SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
	}

	protected virtual void OnPlaceProduct()
	{
		if (!isHovered)
		{
			return;
		}
		if (isReservedToStaff && productCount == 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("reserved_staff_error"), base.transform);
			return;
		}
		ProductData carriedProduct = GetCarriedProduct();
		if (carriedProduct == null)
		{
			return;
		}
		int capacityForProduct = GetCapacityForProduct(carriedProduct);
		if (!IsSupported(carriedProduct.shelfType))
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("error_shelftype_different").Replace("{0}", Locale.GetWord(carriedProduct.shelfType.ToString())), base.transform);
			return;
		}
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			if (SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsBoxFrozen())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_box_frozen", base.transform);
				return;
			}
			if (SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsBoxSpoiled())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_box_spoiled", base.transform);
				return;
			}
		}
		if ((containedType == ProductType.NONE || containedType == carriedProduct.type) && productCount < capacityForProduct)
		{
			if (carriedProduct.IsCookable() && SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowError("error_raw", base.transform);
				return;
			}
			if (carriedProduct.IsCookable() && SingletonBehaviour<TrayManager>.Instance.IsPicked && !SingletonBehaviour<TrayManager>.Instance.PickedTray.IsCooked)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowError("error_raw", base.transform);
				return;
			}
			int rowCount = GetRowCount(carriedProduct);
			int columnCount = GetColumnCount(carriedProduct);
			Product product = RemoveAndGetItem();
			if (containedType == ProductType.NONE)
			{
				containedProducts = new List<Product>();
				for (int i = 0; i < capacityForProduct; i++)
				{
					containedProducts.Add(null);
				}
			}
			product.transform.DOKill();
			product.transform.SetParent(base.transform);
			product.OnBeforePlace(ParentPlaceableType, isStart: false);
			int firstEmptyIndex = GetFirstEmptyIndex();
			if (carriedProduct.HasOverridesForShelf(this))
			{
				Vector3 vector = carriedProduct.GetLocalPositionsForShelf(this)[firstEmptyIndex];
				MonoBehaviour.print("product is moving to: " + vector.ToString());
				product.transform.DOLocalRotate(carriedProduct.GetEulerAnglesForShelf(this)[firstEmptyIndex], PLACE_ANIMATION_DURATION);
				product.transform.DoCurvedLocalMove(carriedProduct.GetLocalPositionsForShelf(this)[firstEmptyIndex], PLACE_ANIMATION_DURATION, 2f).OnComplete(delegate
				{
					product.OnAfterPlace();
				});
			}
			else if (carriedProduct.localPositions == null || carriedProduct.localPositions.Length == 0)
			{
				List<Vector3> list = MathUtils.CalculatePositions(corner1, corner2, rowCount, columnCount, alwaysStackVertically, carriedProduct.height);
				product.transform.DOLocalRotate(base.transform.forward, PLACE_ANIMATION_DURATION);
				product.transform.DoCurvedLocalMove(list[firstEmptyIndex], PLACE_ANIMATION_DURATION, 2f).OnComplete(delegate
				{
					product.OnAfterPlace();
				});
			}
			else
			{
				product.transform.DOLocalRotate(carriedProduct.eulerAngles[firstEmptyIndex], PLACE_ANIMATION_DURATION);
				product.transform.DoCurvedLocalMove(carriedProduct.localPositions[firstEmptyIndex], PLACE_ANIMATION_DURATION, 2f).OnComplete(delegate
				{
					product.OnAfterPlace();
				});
			}
			HapticController.Vibrate(PresetType.MediumImpact);
			SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
			containedProducts[firstEmptyIndex] = product;
			productCount++;
			if (containedType == ProductType.NONE)
			{
				containedType = carriedProduct.type;
				if (previousContainedType != ProductType.NONE)
				{
					EventManager.NotifyEvent(PlaceableEvents.PRICE_TAG_CHANGED, this);
				}
				else
				{
					EventManager.NotifyEvent(PlaceableEvents.PRICE_TAG_ADDED, this);
				}
				RepaintPrice();
			}
			SaveShelfContent();
			if (IsCarrierEmpty())
			{
				bool flag = GetProductCount() > 0;
				if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
				{
					if (flag)
					{
						SingletonBehaviour<BoxManager>.Instance.UpdateMenuWithTake();
					}
					else
					{
						SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
					}
				}
				else if (SingletonBehaviour<TrayManager>.Instance.IsPicked)
				{
					if (flag)
					{
						SingletonBehaviour<TrayManager>.Instance.UpdateMenuWithTake();
					}
					else
					{
						SingletonBehaviour<TrayManager>.Instance.UpdateMenu();
					}
				}
				else if (GenericBox.Instance.IsPicked && flag)
				{
					GenericBox.Instance.UpdateMenuWithTake();
				}
			}
			else
			{
				UpdateMenuForHover();
			}
			RepaintPriceTagForProductCount();
			CheckAndUpdateContainers();
			DisablePlayerReserved();
			EventManager.NotifyProductAddedEvent(this, carriedProduct.type);
		}
		else if (containedType != carriedProduct.type && GetProductCount() < capacityForProduct)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_different_product", base.transform);
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_full", base.transform);
		}
	}

	public virtual void PlaceProduct(Box box, bool instant = false)
	{
		Product product = box.RemoveAndGetItem();
		PlaceProduct(product);
	}

	public virtual void PlaceProduct(Tray tray)
	{
		Product product = tray.RemoveAndGetItem();
		PlaceProduct(product);
	}

	public virtual void PlaceProduct(Product product)
	{
		ProductData data = product.Data;
		int capacityForProduct = GetCapacityForProduct(data);
		bool flag = GetProductCount() > 0 || previousContainedType == ProductType.NONE || data.type == previousContainedType;
		if (!((containedType == ProductType.NONE || containedType == data.type) && GetProductCount() < capacityForProduct && flag))
		{
			return;
		}
		int rowCount = GetRowCount(data);
		int columnCount = GetColumnCount(data);
		if (containedType == ProductType.NONE)
		{
			containedProducts = new List<Product>();
			for (int i = 0; i < capacityForProduct; i++)
			{
				containedProducts.Add(null);
			}
		}
		product.transform.DOKill();
		product.transform.SetParent(base.transform);
		product.OnBeforePlace(ParentPlaceableType, isStart: false);
		int firstEmptyIndex = GetFirstEmptyIndex();
		if (data.HasOverridesForShelf(this))
		{
			Vector3 vector = data.GetLocalPositionsForShelf(this)[firstEmptyIndex];
			MonoBehaviour.print("product is moving to: " + vector.ToString());
			product.transform.DOLocalRotate(data.GetEulerAnglesForShelf(this)[firstEmptyIndex], PLACE_ANIMATION_DURATION);
			product.transform.DoCurvedLocalMove(data.GetLocalPositionsForShelf(this)[firstEmptyIndex], PLACE_ANIMATION_DURATION, 2f).OnComplete(delegate
			{
				product.OnAfterPlace();
			});
		}
		else if (data.localPositions == null || data.localPositions.Length == 0)
		{
			List<Vector3> list = MathUtils.CalculatePositions(corner1, corner2, rowCount, columnCount, alwaysStackVertically, data.height);
			product.transform.DOLocalRotate(base.transform.forward, PLACE_ANIMATION_DURATION);
			product.transform.DoCurvedLocalMove(list[firstEmptyIndex], PLACE_ANIMATION_DURATION, 2f).OnComplete(delegate
			{
				product.OnAfterPlace();
			});
		}
		else
		{
			product.transform.DOLocalRotate(data.eulerAngles[firstEmptyIndex], PLACE_ANIMATION_DURATION);
			product.transform.DoCurvedLocalMove(data.localPositions[firstEmptyIndex], PLACE_ANIMATION_DURATION, 2f).OnComplete(delegate
			{
				product.OnAfterPlace();
			});
		}
		containedProducts[firstEmptyIndex] = product;
		productCount++;
		if (containedType == ProductType.NONE)
		{
			containedType = data.type;
			RepaintPrice();
		}
		SaveShelfContent();
		if (culled)
		{
			product.CullRenderers(cull: true);
		}
		isDirty = true;
		RepaintPriceTagForProductCount();
		CheckAndUpdateContainers();
		EventManager.NotifyProductAddedEvent(this, data.type);
	}

	private void CheckAndUpdateContainers()
	{
		if (containedType == ProductType.NONE)
		{
			if (containedContainers.Count > 0)
			{
				for (int i = 0; i < containedContainers.Count; i++)
				{
					SingletonBehaviour<ContainerPool>.Instance.PutBackToPool(containedContainers[i]);
				}
				containedContainers.Clear();
				return;
			}
			if (containedContainers.Count == 0)
			{
				return;
			}
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType);
		List<ContainerInfo> containerInfos = productData.GetContainerInfos(ParentPlaceableType);
		if (containerInfos == null || containerInfos.Count == 0 || productData == null)
		{
			return;
		}
		int num = 0;
		for (int j = 0; j < containerInfos.Count; j++)
		{
			if (productCount >= containerInfos[j].activatingProductCount)
			{
				num++;
			}
		}
		if (containedContainers.Count > num)
		{
			int num2 = containedContainers.Count - num;
			int count = containedContainers.Count;
			for (int k = 0; k < num2; k++)
			{
				SingletonBehaviour<ContainerPool>.Instance.PutBackToPool(containedContainers[count - k - 1]);
				containedContainers.RemoveAt(count - k - 1);
			}
		}
		else if (containedContainers.Count < num)
		{
			int num3 = num - containedContainers.Count;
			int count2 = containedContainers.Count;
			for (int l = count2; l < count2 + num3; l++)
			{
				Transform container = SingletonBehaviour<ContainerPool>.Instance.GetContainer(containerInfos[l].type);
				container.transform.SetParent(base.transform);
				container.transform.localPosition = containerInfos[l].localPosition;
				container.transform.localEulerAngles = containerInfos[l].eulerAngle;
				containedContainers.Add(container);
			}
		}
	}

	private ProductData GetCarriedProduct()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetPickedBox().GetContainedProduct();
		}
		if (GenericBox.Instance.IsPicked)
		{
			return GenericBox.Instance.GetContainedProduct();
		}
		return SingletonBehaviour<TrayManager>.Instance.PickedTray.GetContainedProduct();
	}

	private Product RemoveAndGetItem()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetPickedBox().RemoveAndGetItem();
		}
		if (GenericBox.Instance.IsPicked)
		{
			return GenericBox.Instance.RemoveAndGetItem();
		}
		return SingletonBehaviour<TrayManager>.Instance.PickedTray.RemoveAndGetItem();
	}

	private bool IsCarrierEmpty()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty();
		}
		if (GenericBox.Instance.IsPicked)
		{
			return GenericBox.Instance.IsEmpty();
		}
		return SingletonBehaviour<TrayManager>.Instance.PickedTray.IsEmpty();
	}

	public bool IsSupported(ShelfType shelfType)
	{
		PlaceableType parentPlaceableType = ParentPlaceableType;
		switch (shelfType)
		{
		case ShelfType.VEGETABLE_SHELF:
			if (parentPlaceableType != PlaceableType.VEGETABLE_SHELF && parentPlaceableType != PlaceableType.PRODUCE_SHELF_SMALL && parentPlaceableType != PlaceableType.PRODUCE_SHELF_WIDE && parentPlaceableType != PlaceableType.PRODUCE_SHELF_INSIDE_CORNER && parentPlaceableType != PlaceableType.PRODUCE_SHELF_OUTSIDE_CORNER && parentPlaceableType != PlaceableType.WOODEN_PRODUCE_SHELF)
			{
				return parentPlaceableType == PlaceableType.WOODEN_PRODUCE_SHELF_CIRCULAR;
			}
			return true;
		case ShelfType.SHELF:
			if (parentPlaceableType != PlaceableType.SMALL_SHELF)
			{
				return parentPlaceableType == PlaceableType.WIDE_SHELF;
			}
			return true;
		case ShelfType.FRIDGE:
			if (parentPlaceableType != PlaceableType.FRIDGE)
			{
				return parentPlaceableType == PlaceableType.WIDE_FRIGE;
			}
			return true;
		case ShelfType.FREEZER:
			return parentPlaceableType == PlaceableType.FREEZER;
		case ShelfType.VENDING_MACHINE:
			return parentPlaceableType == PlaceableType.VENDING_MACHINE;
		case ShelfType.WALL_SHELF:
			return parentPlaceableType == PlaceableType.WALL_SHELF;
		case ShelfType.BAKERY_SHELF:
			if (parentPlaceableType != PlaceableType.BAKERY_SHELF)
			{
				return parentPlaceableType == PlaceableType.BAKERY_SHELF_WIDE;
			}
			return true;
		case ShelfType.CIRCULAR_SHELF:
			return parentPlaceableType == PlaceableType.CIRCULAR_SHELF;
		case ShelfType.HANGER_CLOTH_RACK:
			if (parentPlaceableType >= PlaceableType.HANGER_CLOTH_RACK_1)
			{
				return parentPlaceableType <= PlaceableType.HANGER_CLOTH_RACK_5;
			}
			return false;
		case ShelfType.FOLDED_CLOTH_RACK:
			if (parentPlaceableType >= PlaceableType.HANGER_CLOTH_RACK_1)
			{
				return parentPlaceableType <= PlaceableType.FOLDED_CLOTH_RACK_3;
			}
			return false;
		case ShelfType.SPORTS_SHELF_TWO_RACK:
			return parentPlaceableType == PlaceableType.SPORTS_SHELF_TWO_RACK;
		case ShelfType.SPORTS_SHELF_SHORT_HANGER:
			return parentPlaceableType == PlaceableType.SPORTS_SHELF_SHORT_HANGER;
		case ShelfType.SPORTS_SHELF_LONG_HANGER:
			return parentPlaceableType == PlaceableType.SPORTS_SHELF_LONG_HANGER;
		case ShelfType.MUSIC_SHELF_HANGER:
			return parentPlaceableType == PlaceableType.MUSIC_SHELF_HANGER;
		case ShelfType.MUSIC_SHELF_TWO_RACK:
			return parentPlaceableType == PlaceableType.MUSIC_SHELF_TWO_RACK;
		case ShelfType.MUSIC_SHELF_LONG_HANGER:
			return parentPlaceableType == PlaceableType.MUSIC_SHELF_LONG_HANGER;
		case ShelfType.ELECTRONICS_SHELF_TABLE:
			return parentPlaceableType == PlaceableType.ELECTRONICS_SHELF_TABLE;
		case ShelfType.ELECTRONICS_SHELF_CIRCULAR:
			return parentPlaceableType == PlaceableType.ELECTRONICS_SHELF_CIRCULAR;
		case ShelfType.ELECTRONICS_SHELF_HANGER:
			return parentPlaceableType == PlaceableType.ELECTRONICS_SHELF_HANGER;
		case ShelfType.ELECTRONICS_SHELF_CUPBOARD:
			return parentPlaceableType == PlaceableType.ELECTRONICS_SHELF_CUPBOARD;
		case ShelfType.CANDY_STAND:
			return parentPlaceableType == PlaceableType.CANDY_STAND;
		case ShelfType.FISH_STAND:
			return parentPlaceableType == PlaceableType.FISH_STAND;
		case ShelfType.LOBSTER_TANK:
			return parentPlaceableType == PlaceableType.LOBSTER_TANK;
		case ShelfType.TOY_HANGER_SHELF:
			return parentPlaceableType == PlaceableType.TOY_HANGER_SHELF;
		case ShelfType.TOY_SHELF_DISPLAY:
			if (parentPlaceableType != PlaceableType.TOY_SHELF)
			{
				return parentPlaceableType == PlaceableType.TOY_WIDE_SHELF;
			}
			return true;
		case ShelfType.TOY_WIDE_SHELF_DISPLAY:
			if (parentPlaceableType != PlaceableType.TOY_WIDE_SHELF)
			{
				return parentPlaceableType == PlaceableType.TOY_SHELF;
			}
			return true;
		case ShelfType.TOY_BALL_SHELF:
			if (parentPlaceableType != PlaceableType.TOY_BALL_SHELF)
			{
				return parentPlaceableType == PlaceableType.TOY_BALL_SHELF_ROW;
			}
			return true;
		default:
			return false;
		}
	}

	public void RemoveProduct(Product product)
	{
		int index = containedProducts.IndexOf(product);
		containedProducts[index] = null;
		productCount--;
		if (GetProductCount() == 0)
		{
			previousContainedType = containedType;
			containedType = ProductType.NONE;
		}
		RepaintPriceTagForProductCount();
		CheckAndUpdateContainers();
		product.OnRemovedFromShelf();
		EventManager.NotifyProductRemovedEvent(this, product.Data.type);
		SaveShelfContent();
	}

	public void CullAllProducts(bool cull, bool cullPriceLabels)
	{
		if (disableCulling)
		{
			return;
		}
		culled = cull;
		if (cullPriceLabels)
		{
			if (cull)
			{
				soldOutText.GetComponent<MeshRenderer>().enabled = false;
				textMesh.GetComponent<MeshRenderer>().enabled = false;
				textProductCount.GetComponent<MeshRenderer>().enabled = false;
			}
			else
			{
				soldOutText.GetComponent<MeshRenderer>().enabled = true;
				textMesh.GetComponent<MeshRenderer>().enabled = true;
				textProductCount.GetComponent<MeshRenderer>().enabled = true;
			}
		}
		for (int i = 0; i < containedContainers.Count; i++)
		{
			if (containedContainers[i] != null)
			{
				containedContainers[i].gameObject.SetActive(!cull);
			}
		}
		for (int j = 0; j < containedProducts.Count; j++)
		{
			if (containedProducts[j] != null)
			{
				containedProducts[j].CullRenderers(cull);
			}
		}
		isDirty = false;
	}

	private void RepaintPriceTagForProductCount()
	{
		if (GetProductCount() == 0)
		{
			soldOutText.enabled = true;
			textMesh.enabled = false;
			saleRibbon.SetActive(value: false);
		}
		else
		{
			soldOutText.enabled = false;
			textMesh.enabled = true;
		}
		if (containedType != ProductType.NONE)
		{
			SetSpriteKeepSize(productSprite, SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType).productSprite);
		}
		else if (previousContainedType != ProductType.NONE)
		{
			SetSpriteKeepSize(productSprite, SingletonBehaviour<ProductPool>.Instance.GetProductData(previousContainedType).productSprite);
		}
		textProductCount.text = productCount.ToString();
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

	private void RepaintPriceTagForPrice(float newPrice)
	{
		if (containedType != ProductType.NONE || previousContainedType != ProductType.NONE)
		{
			ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData((containedType != ProductType.NONE) ? containedType : previousContainedType);
			if (newPrice <= 0.8f * productData.unitMarketPrice)
			{
				saleRibbon.SetActive(value: true);
				labelDefault.gameObject.SetActive(value: false);
				labelDiscount.gameObject.SetActive(value: true);
			}
			else if (newPrice < productData.unitMarketPrice)
			{
				saleRibbon.SetActive(value: false);
				labelDefault.gameObject.SetActive(value: false);
				labelDiscount.gameObject.SetActive(value: true);
			}
			else
			{
				saleRibbon.SetActive(value: false);
				labelDefault.gameObject.SetActive(value: true);
				labelDiscount.gameObject.SetActive(value: false);
			}
			PriceText.text = "$" + newPrice.ToString("0.00", CultureInfo.InvariantCulture);
		}
	}

	private void RepaintPrice()
	{
		float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice((containedType != ProductType.NONE) ? containedType : previousContainedType);
		if (unitPrice == -1f)
		{
			priceTag.gameObject.SetActive(value: false);
			return;
		}
		if (!priceTag.gameObject.activeSelf)
		{
			priceTag.gameObject.SetActive(!culled);
		}
		RepaintPriceTagForPrice(unitPrice);
	}

	public virtual void Initialize()
	{
		productCount = 0;
		int num = GenericDataSerializer.LoadInt(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfProductCount");
		ProductType productType = GenericDataSerializer.Load(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfProduct", ProductType.NONE);
		previousContainedType = GenericDataSerializer.Load(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfPreviousProduct", ProductType.NONE);
		if (previousContainedType != ProductType.NONE && !SingletonBehaviour<ProductPool>.Instance.HasProductData(previousContainedType))
		{
			GenericDataSerializer.Save(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfPreviousProduct", ProductType.NONE);
		}
		else if (productType == ProductType.NONE || SingletonBehaviour<ProductPool>.Instance.HasProductData(productType))
		{
			if (productType != ProductType.NONE && !IsSupported(SingletonBehaviour<ProductPool>.Instance.GetProductData(productType).shelfType))
			{
				productType = ProductType.NONE;
				previousContainedType = ProductType.NONE;
				num = 0;
				GenericDataSerializer.Save(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfProduct", ProductType.NONE);
				GenericDataSerializer.Save(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfPreviousProduct", ProductType.NONE);
				GenericDataSerializer.SaveInt(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfProductCount", 0);
			}
			if (num != 0)
			{
				FillItems(productType, num);
			}
			else if (previousContainedType != ProductType.NONE)
			{
				RepaintPriceTagForProductCount();
				RepaintPrice();
				EventManager.NotifyEvent(PlaceableEvents.PRICE_TAG_ADDED, this);
				priceTag.gameObject.SetActive(!culled);
			}
			RegisterRestockingJobs();
		}
	}

	public void RegisterRestockingJobs()
	{
		if (SingletonBehaviour<RestockerManager>.Instance != null)
		{
			SingletonBehaviour<RestockerManager>.Instance.CheckAndRegisterShelf(this);
		}
	}

	public float GetCapacityPercentage()
	{
		if (containedType == ProductType.NONE)
		{
			return 0f;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType);
		int capacityForProduct = GetCapacityForProduct(productData);
		if (capacityForProduct == 0)
		{
			return 1f;
		}
		return (float)GetProductCount() / (float)capacityForProduct;
	}

	public bool NeedsRestocking()
	{
		if (containedType == ProductType.NONE)
		{
			return true;
		}
		return GetCapacityPercentage() < 0.25f;
	}

	public bool HasProductLabel()
	{
		if (containedType == ProductType.NONE)
		{
			return previousContainedType != ProductType.NONE;
		}
		return true;
	}

	private void OnSetPrice()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = ((containedType == ProductType.NONE) ? previousContainedType : containedType);
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_ADDEDTO_CART, param);
		}
		else if (containedType == ProductType.NONE)
		{
			if (IsReservedToStaff())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowErrorWithFullText(Locale.GetWord("reserved_staff_error"), base.transform);
				return;
			}
			priceTag.gameObject.SetActive(value: false);
			EventManager.NotifyEvent(PlaceableEvents.BEFORE_PRICE_TAG_REMOVED, this);
			ProductType num = previousContainedType;
			previousContainedType = ProductType.NONE;
			SaveShelfContent();
			if (num != ProductType.NONE)
			{
				EventManager.NotifyEvent(PlaceableEvents.PRICE_TAG_REMOVED, this);
			}
		}
		else
		{
			SingletonWindow<PriceWindow>.Instance.Open(SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType));
			if (holdStarted)
			{
				EndHold();
			}
		}
	}

	private void OnSetPriceRightClick()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = ((containedType == ProductType.NONE) ? previousContainedType : containedType);
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_REMOVEDFROM_CART, param);
		}
	}

	private void OnLabelHoverStarted()
	{
		ProductType param = ((containedType == ProductType.NONE) ? previousContainedType : containedType);
		EventManager.NotifyEvent(ProductEvents.PRICE_TAG_HOVER_STARTED, param);
	}

	private void FillItems(ProductType type, int count)
	{
		if (type == ProductType.NONE)
		{
			RepaintPriceTagForProductCount();
			if (previousContainedType != ProductType.NONE)
			{
				EventManager.NotifyEvent(PlaceableEvents.PRICE_TAG_ADDED, this);
			}
			return;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(type);
		int capacityForProduct = GetCapacityForProduct(productData);
		if ((containedType != ProductType.NONE && containedType != productData.type) || containedProducts.Count >= capacityForProduct)
		{
			return;
		}
		List<Vector3> productPositions = GetProductPositions(productData);
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
			if (j <= productPositions.Count - 1)
			{
				Product product = SingletonBehaviour<ProductPool>.Instance.GetProduct(type);
				product.transform.SetParent(base.transform);
				product.OnBeforePlace(ParentPlaceableType, isStart: true);
				if (productData.IsCookable())
				{
					((CookableProduct)product).CookInstantly();
				}
				if (productData.HasOverridesForShelf(this))
				{
					product.transform.localEulerAngles = productData.GetEulerAnglesForShelf(this)[j];
				}
				else if (productData.eulerAngles == null || productData.eulerAngles.Length == 0)
				{
					product.transform.localEulerAngles = base.transform.forward;
				}
				else
				{
					product.transform.localEulerAngles = productData.eulerAngles[j];
				}
				product.transform.localPosition = productPositions[j];
				containedProducts[j] = product;
				productCount++;
			}
		}
		if (containedType == ProductType.NONE)
		{
			containedType = productData.type;
			RepaintPrice();
		}
		RepaintPriceTagForProductCount();
		CheckAndUpdateContainers();
		EventManager.NotifyEvent(PlaceableEvents.PRICE_TAG_ADDED, this);
		EventManager.NotifyProductAddedEvent(this, type);
	}

	private List<Vector3> GetProductPositions(ProductData data)
	{
		int rowCount = GetRowCount(data);
		int columnCount = GetColumnCount(data);
		if (data.HasOverridesForShelf(this))
		{
			return data.GetLocalPositionsForShelf(this).ToList();
		}
		if (data.localPositions == null || data.localPositions.Length == 0)
		{
			return MathUtils.CalculatePositions(corner1, corner2, rowCount, columnCount, alwaysStackVertically, data.height);
		}
		return data.localPositions.ToList();
	}

	public virtual int GetCapacityForProduct(ProductData product)
	{
		if (product.HasOverridesForShelf(this))
		{
			return product.GetLocalPositionsForShelf(this).Length;
		}
		if (product.localPositions != null && product.localPositions.Length != 0)
		{
			return product.localPositions.Length;
		}
		return GetRowCount(product) * GetColumnCount(product);
	}

	private void SaveShelfContent()
	{
		GenericDataSerializer.SaveInt(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfProductCount", GetProductCount());
		GenericDataSerializer.Save(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfProduct", containedType);
		GenericDataSerializer.Save(parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID + "ShelfPreviousProduct", previousContainedType);
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		if (!IsDetectionZone())
		{
			return;
		}
		if (FireExtinguisher.Instance.IsPicked)
		{
			if (SingletonBehaviour<FireManager>.Instance.IsBurning(parentPlaceable))
			{
				FireExtinguisher.Instance.Use();
			}
		}
		else if (!SingletonBehaviour<TrayManager>.Instance.IsPicked && !SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
		{
			holdStarted = true;
		}
	}

	public override void OnMouseButtonUp()
	{
		base.OnMouseButtonUp();
		if (holdStarted)
		{
			EndHold();
		}
		IsDetectionZone();
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		if (!IsDetectionZone())
		{
			CloseInteractionElements();
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked || SingletonBehaviour<TrayManager>.Instance.IsPicked || GenericBox.Instance.IsPicked)
		{
			if (outline != null)
			{
				outline.enabled = true;
			}
			else
			{
				parentPlaceable.Outline.enabled = true;
			}
			UpdateMenuForHover();
			isHovered = true;
		}
		else if (FireExtinguisher.Instance.IsPicked)
		{
			if (SingletonBehaviour<FireManager>.Instance.IsBurning(parentPlaceable))
			{
				parentPlaceable.Outline.enabled = true;
				FireExtinguisher.Instance.UpdateMenuWithExtinguish();
			}
		}
		else
		{
			if (!SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
			{
				return;
			}
			parentPlaceable.OnShelfHoverStarted();
			Dictionary<KeyCode, (string, Action)> dictionary = new Dictionary<KeyCode, (string, Action)>();
			if (parentPlaceable != null && parentPlaceable.GetExtraButtonActions() != null)
			{
				foreach (var extraButtonAction in parentPlaceable.GetExtraButtonActions())
				{
					dictionary.Add(extraButtonAction.Item1, extraButtonAction.Item2);
				}
			}
			if (GetProductCount() != 0)
			{
				if (!dictionary.ContainsKey(KeyCode.F))
				{
					dictionary.Add(KeyCode.F, ("pack", delegate
					{
						parentPlaceable.Pack();
					}));
				}
				dictionary.Add(KeyCode.T, ("set_price", delegate
				{
					OnSetPrice();
				}));
				dictionary.Add(KeyCode.Mouse1, ("place_move", delegate
				{
					parentPlaceable.StartNewPlacement();
				}));
				SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(dictionary, base.transform);
				return;
			}
			if (!dictionary.ContainsKey(KeyCode.F))
			{
				dictionary.Add(KeyCode.F, ("pack", delegate
				{
					parentPlaceable.Pack();
				}));
			}
			dictionary.Add(KeyCode.Mouse1, ("place_move", delegate
			{
				parentPlaceable.StartNewPlacement();
			}));
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(dictionary, base.transform);
		}
	}

	private void UpdateMenuForHover()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty() && !SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsPlaceableBox() && !SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsFurnitureBox())
		{
			Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
			bool flag = pickedBox.ProductCount < pickedBox.GetContainedProduct().GetMaxProductCount();
			bool flag2 = GetProductCount() > 0;
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuWithPlace(flag && flag2);
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty())
		{
			if (GetProductCount() > 0)
			{
				SingletonBehaviour<BoxManager>.Instance.UpdateMenuWithTake();
			}
			else
			{
				SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
			}
		}
		else if (SingletonBehaviour<TrayManager>.Instance.IsPicked && !SingletonBehaviour<TrayManager>.Instance.PickedTray.IsEmpty())
		{
			Tray pickedTray = SingletonBehaviour<TrayManager>.Instance.PickedTray;
			CookableProductData cookableProductData = (CookableProductData)pickedTray.GetContainedProduct();
			bool flag3 = pickedTray.GetProductCount() < cookableProductData.trayRowCount * cookableProductData.trayColumnCount;
			bool flag4 = GetProductCount() > 0;
			SingletonBehaviour<TrayManager>.Instance.UpdateMenuWithPlace(flag3 && flag4);
		}
		else if (SingletonBehaviour<TrayManager>.Instance.IsPicked && SingletonBehaviour<TrayManager>.Instance.PickedTray.IsEmpty())
		{
			if (GetProductCount() > 0)
			{
				SingletonBehaviour<TrayManager>.Instance.UpdateMenuWithTake();
			}
			else
			{
				SingletonBehaviour<TrayManager>.Instance.UpdateMenu();
			}
		}
		else if (GenericBox.Instance.IsPicked && !GenericBox.Instance.IsEmpty())
		{
			bool flag5 = GenericBox.Instance.HasSpace();
			bool flag6 = GetProductCount() > 0;
			GenericBox.Instance.UpdateMenuWithPlace(flag5 && flag6);
		}
		else if (GenericBox.Instance.IsPicked && GenericBox.Instance.IsEmpty())
		{
			if (GetProductCount() > 0)
			{
				GenericBox.Instance.UpdateMenuWithTake();
			}
			else
			{
				GenericBox.Instance.UpdateMenu();
			}
		}
	}

	private void TakeBack()
	{
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (holdStarted)
		{
			EndHold();
		}
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		if (IsDetectionZone())
		{
			parentPlaceable.OnShelfHoverEnded();
			CloseInteractionElements();
		}
	}

	protected void CloseInteractionElements()
	{
		if (parentPlaceable.Outline.enabled)
		{
			parentPlaceable.Outline.enabled = false;
		}
		if (outline != null)
		{
			outline.enabled = false;
		}
		isHovered = false;
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		FireExtinguisher.Instance.UpdateMenu();
		SingletonBehaviour<TrayManager>.Instance.UpdateMenu();
		if (GenericBox.Instance.IsPicked)
		{
			GenericBox.Instance.UpdateMenu();
		}
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private bool IsDetectionZone()
	{
		return true;
	}

	private void EndHold()
	{
		holdStarted = false;
		fillAmount = 0f;
		SingletonBehaviour<UIManager>.Instance.ResetHoldProgress();
	}

	public Transform PickupPoint()
	{
		return parentPlaceable.PickupPoint;
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public ProductGroup GetProductGroup()
	{
		return SingletonBehaviour<ProductPool>.Instance.GetShelfData(parentPlaceable.Type).productGroup;
	}

	public ProductType ContainedProductType()
	{
		return containedType;
	}

	public ProductType PreviousContainedProductType()
	{
		return previousContainedType;
	}

	public string RestocableID()
	{
		return parentPlaceable.Type.ToString() + parentPlaceable.PlaceableID + "|" + shelfID;
	}

	public bool IsBakeryPlaceable()
	{
		return parentPlaceable.IsBakeryPlaceable();
	}

	public bool IsAvailableForRestocking()
	{
		return true;
	}

	public Transform StaffInteractionPoint()
	{
		return parentPlaceable.StaffInteractionPoint;
	}

	public bool IsCookedShelf()
	{
		return false;
	}

	public Transform ParentTransform()
	{
		return parentPlaceable.transform;
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
