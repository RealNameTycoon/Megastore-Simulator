using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductsWindow : TabWindow
{
	public enum StockStatusFilter
	{
		ALL_PRODUCTS,
		IN_STOCK,
		LOW_STOCK,
		OUT_OF_STOCK
	}

	[SerializeField]
	private Button productTabButton;

	[SerializeField]
	private List<ProductUI> productUIs;

	private ProductGroup currentFilter = ProductGroup.NONE;

	[SerializeField]
	private GameObject wishlistUI;

	[SerializeField]
	private TMP_Dropdown inShelfStockFilter;

	[SerializeField]
	private TMP_Dropdown offShelfStockFilter;

	[SerializeField]
	private TMP_Dropdown storageTypeFilter;

	[SerializeField]
	private Toggle includeOnlyLabeledProductsToggle;

	private List<ShelfType> currentShelfTypes = new List<ShelfType>();

	private List<ProductData> currentProducts = new List<ProductData>();

	private bool includeOnlyLabeledProducts;

	private const int ITEM_PER_ROW = 3;

	protected override void Start()
	{
		base.Start();
		EventManager.AddListener<int, ProductGroup>(ProductEvents.PRODUCT_LICENSE_PURCHASED, OnLicensePurchased);
		EventManager.AddListener<ProductType>(GameEvents.STOCKS_UPDATED, OnStocksUpdated);
		EventManager.AddListener(GameEvents.LABELED_SHELVES_UPDATED, OnLabeledShelvesUpdated);
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		InitializeFilters();
	}

	private void OnLanguageChanged()
	{
		RefreshFilterOptions();
	}

	private void InitializeFilters()
	{
		RefreshFilterOptions();
		inShelfStockFilter.onValueChanged.AddListener(OnInShelfStockFilterChanged);
		offShelfStockFilter.onValueChanged.AddListener(OnOffShelfStockFilterChanged);
		storageTypeFilter.onValueChanged.AddListener(OnStorageTypeFilterChanged);
		includeOnlyLabeledProductsToggle.onValueChanged.AddListener(OnIncludeOnlyLabeledProductsChanged);
	}

	private void OnIncludeOnlyLabeledProductsChanged(bool value)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TOGGLE);
		includeOnlyLabeledProducts = value;
		RefreshProductUIs();
	}

	private void RefreshFilterOptions()
	{
		inShelfStockFilter.options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData(Locale.GetWord("ALL_PRODUCTS")),
			new TMP_Dropdown.OptionData(Locale.GetWord("IN_STOCK")),
			new TMP_Dropdown.OptionData(Locale.GetWord("LOW_STOCK")),
			new TMP_Dropdown.OptionData(Locale.GetWord("OUT_OF_STOCK"))
		};
		offShelfStockFilter.options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData(Locale.GetWord("ALL_PRODUCTS")),
			new TMP_Dropdown.OptionData(Locale.GetWord("IN_STOCK")),
			new TMP_Dropdown.OptionData(Locale.GetWord("LOW_STOCK")),
			new TMP_Dropdown.OptionData(Locale.GetWord("OUT_OF_STOCK"))
		};
		storageTypeFilter.options.Clear();
		storageTypeFilter.options.Add(new TMP_Dropdown.OptionData(Locale.GetWord("dropdown_all")));
		for (int i = 0; i < currentShelfTypes.Count; i++)
		{
			storageTypeFilter.options.Add(new TMP_Dropdown.OptionData(Locale.GetWord(currentShelfTypes[i].ToString())));
		}
	}

	private void OnInShelfStockFilterChanged(int value)
	{
		RefreshProductUIs();
	}

	private void OnOffShelfStockFilterChanged(int value)
	{
		RefreshProductUIs();
	}

	private void OnStorageTypeFilterChanged(int value)
	{
		RefreshProductUIs();
	}

	private void OnStocksUpdated(ProductType type)
	{
		if (IsOpen())
		{
			RefreshProductUIs();
		}
	}

	private void OnLabeledShelvesUpdated()
	{
		if (IsOpen() && includeOnlyLabeledProductsToggle.isOn)
		{
			RefreshProductUIs();
		}
	}

	private int GetAvailableStockOnShelves(ProductType type)
	{
		return SingletonBehaviour<StockManager>.Instance.GetAvailableStockOnShelves(type);
	}

	private void OnLicensePurchased(int newLicense, ProductGroup group)
	{
		RefreshProductUIs();
	}

	public override void Initialize()
	{
		base.Initialize();
		RefreshProductUIs();
	}

	public void RefreshProductUIs()
	{
		currentProducts.Clear();
		List<ProductType> list = ((currentFilter == ProductGroup.VENDING) ? SingletonBehaviour<VendingStockManager>.Instance.PurchasableProducts : SingletonBehaviour<StockManager>.Instance.PurchasableProducts);
		bool flag = currentShelfTypes.Count > 0;
		for (int i = 0; i < list.Count; i++)
		{
			ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(list[i]);
			if (productData.productGroup != currentFilter)
			{
				continue;
			}
			if (GameManager.isDemo)
			{
				if (productData.requiredLicense <= 2 && productData.productGroup != ProductGroup.ELECTRONICS && productData.productGroup != ProductGroup.MUSIC && productData.productGroup != ProductGroup.SPORTS)
				{
					currentProducts.Add(productData);
				}
				continue;
			}
			if (!flag && !currentShelfTypes.Contains(productData.shelfType))
			{
				currentShelfTypes.Add(productData.shelfType);
			}
			currentProducts.Add(productData);
		}
		if (!flag)
		{
			List<TMP_Dropdown.OptionData> list2 = new List<TMP_Dropdown.OptionData>();
			list2.Add(new TMP_Dropdown.OptionData(Locale.GetWord("dropdown_all")));
			for (int j = 0; j < currentShelfTypes.Count; j++)
			{
				list2.Add(new TMP_Dropdown.OptionData(Locale.GetWord(currentShelfTypes[j].ToString())));
			}
			storageTypeFilter.options = list2;
			storageTypeFilter.SetValueWithoutNotify(0);
		}
		currentProducts.Sort((ProductData a, ProductData b) => a.requiredLicense.CompareTo(b.requiredLicense));
		int num = 0;
		for (int num2 = 0; num2 < currentProducts.Count; num2++)
		{
			if (currentProducts[num2].requiredLicense <= 999)
			{
				int availableStockOnShelves = GetAvailableStockOnShelves(currentProducts[num2].type);
				int availableStockInBoxes = SingletonBehaviour<StockManager>.Instance.GetAvailableStockInBoxes(currentProducts[num2].type);
				int maxProductCount = currentProducts[num2].GetMaxProductCount();
				if ((inShelfStockFilter.value != 1 || availableStockOnShelves != 0) && (inShelfStockFilter.value != 3 || availableStockOnShelves <= 0) && (offShelfStockFilter.value != 1 || availableStockInBoxes != 0) && (offShelfStockFilter.value != 3 || availableStockInBoxes <= 0) && (offShelfStockFilter.value != 2 || availableStockInBoxes < maxProductCount) && (inShelfStockFilter.value != 2 || availableStockOnShelves < maxProductCount) && (storageTypeFilter.value <= 0 || currentProducts[num2].shelfType == currentShelfTypes[storageTypeFilter.value - 1]) && (!includeOnlyLabeledProducts || SingletonBehaviour<LabeledStockManager>.Instance.HasLabeledShelfByProductType(currentProducts[num2].type)))
				{
					productUIs[num].Repaint(currentProducts[num2]);
					productUIs[num].UpdateStocks(availableStockOnShelves, availableStockInBoxes);
					productUIs[num].gameObject.SetActive(value: true);
					num++;
				}
			}
		}
		for (int num3 = num; num3 < productUIs.Count; num3++)
		{
			productUIs[num3].gameObject.SetActive(value: false);
		}
		wishlistUI.gameObject.SetActive(GameManager.isDemo);
		RefreshNavigation();
	}

	public void ApplyFilter(ProductGroup productGroup)
	{
		currentFilter = productGroup;
		currentShelfTypes.Clear();
		storageTypeFilter.value = 0;
		RefreshProductUIs();
	}

	public override void Open()
	{
		base.Open();
		for (int i = 0; i < productUIs.Count && productUIs[i].gameObject.activeSelf; i++)
		{
			productUIs[i].UpdateStocks(GetAvailableStockOnShelves(currentProducts[i].type), SingletonBehaviour<StockManager>.Instance.GetAvailableStockInBoxes(currentProducts[i].type));
		}
	}

	private void RefreshNavigation()
	{
		List<ProductUI> list = new List<ProductUI>();
		for (int i = 0; i < productUIs.Count; i++)
		{
			if (productUIs[i] != null && productUIs[i].gameObject.activeSelf)
			{
				list.Add(productUIs[i]);
			}
		}
		Selectable down = ((list.Count > 0) ? list[0].GetSelectable() : null);
		SetDownSelectable(offShelfStockFilter, down);
		SetDownSelectable(storageTypeFilter, down);
		SetDownSelectable(inShelfStockFilter, down);
		SetDownSelectable(includeOnlyLabeledProductsToggle, down);
		if (list.Count != 0)
		{
			List<Selectable> list2 = new List<Selectable>(list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				list2.Add(list[j].RefreshNavigation(null, null, null, null));
			}
			for (int k = 0; k < list.Count; k++)
			{
				int num = k % 3;
				Selectable up = ((k < 3) ? offShelfStockFilter : list2[k - 3]);
				Selectable down2 = ((k + 3 < list2.Count) ? list2[k + 3] : null);
				Selectable left = ((num > 0) ? list2[k - 1] : null);
				Selectable right = ((num < 2 && k + 1 < list2.Count) ? list2[k + 1] : null);
				list[k].RefreshNavigation(up, down2, left, right);
			}
		}
	}

	private void SetDownSelectable(Selectable selectableToSet, Selectable down)
	{
		Navigation navigation = selectableToSet.navigation;
		navigation.selectOnDown = down;
		selectableToSet.navigation = navigation;
	}

	public override Selectable GetFirstSelectable()
	{
		return offShelfStockFilter;
	}
}
