using System;
using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductStatsWindow : TabWindow
{
	private enum OrderBy
	{
		LICENSE,
		PRODUCT_NAME,
		LAST_DAY_SOLD_AMOUNT,
		LAST_7_DAYS_SOLD_AMOUNT,
		LAST_30_DAYS_SOLD_AMOUNT,
		PRODUCT_NAME_REVERSE,
		LAST_DAY_SOLD_AMOUNT_REVERSE,
		LAST_7_DAYS_SOLD_AMOUNT_REVERSE,
		LAST_30_DAYS_SOLD_AMOUNT_REVERSE
	}

	[SerializeField]
	private ProductStatRow productStatRowPrefab;

	[SerializeField]
	private TMP_Dropdown filterDropdown;

	private List<ProductStatRow> productStatRows = new List<ProductStatRow>();

	private ProductGroup currentFilter = ProductGroup.NONE;

	private List<ProductData> currentProducts = new List<ProductData>();

	[SerializeField]
	private List<ProductGroup> filterOptions = new List<ProductGroup>();

	[SerializeField]
	private List<Button> headerButtons;

	[SerializeField]
	private ProductSalesLineChartUI productSalesLineChartUI;

	[SerializeField]
	private List<GameObject> upAndDownArrows;

	[SerializeField]
	private List<GameObject> downArrows;

	[SerializeField]
	private List<GameObject> upArrows;

	private static int productNameSortIndex = 0;

	private static int lastDaySoldAmountSortIndex = 1;

	private static int last7DaysSoldAmountSortIndex = 2;

	private static int last30DaysSoldAmountSortIndex = 3;

	private OrderBy currentOrderBy;

	public void OrderByProductName()
	{
		bool activeSelf = downArrows[productNameSortIndex].activeSelf;
		ResetArrows();
		upAndDownArrows[productNameSortIndex].SetActive(value: false);
		if (activeSelf)
		{
			upArrows[productNameSortIndex].SetActive(value: true);
		}
		else
		{
			downArrows[productNameSortIndex].SetActive(value: true);
		}
		currentOrderBy = ((!activeSelf) ? OrderBy.PRODUCT_NAME : OrderBy.PRODUCT_NAME_REVERSE);
		RepaintSorted();
	}

	public void OrderByLastDaySoldAmount()
	{
		bool activeSelf = downArrows[lastDaySoldAmountSortIndex].activeSelf;
		ResetArrows();
		upAndDownArrows[lastDaySoldAmountSortIndex].SetActive(value: false);
		if (activeSelf)
		{
			upArrows[lastDaySoldAmountSortIndex].SetActive(value: true);
		}
		else
		{
			downArrows[lastDaySoldAmountSortIndex].SetActive(value: true);
		}
		currentOrderBy = (activeSelf ? OrderBy.LAST_DAY_SOLD_AMOUNT_REVERSE : OrderBy.LAST_DAY_SOLD_AMOUNT);
		RepaintSorted();
	}

	public void OrderByLast7DaysSoldAmount()
	{
		bool activeSelf = downArrows[last7DaysSoldAmountSortIndex].activeSelf;
		ResetArrows();
		upAndDownArrows[last7DaysSoldAmountSortIndex].SetActive(value: false);
		currentOrderBy = (activeSelf ? OrderBy.LAST_7_DAYS_SOLD_AMOUNT_REVERSE : OrderBy.LAST_7_DAYS_SOLD_AMOUNT);
		if (activeSelf)
		{
			upArrows[last7DaysSoldAmountSortIndex].SetActive(value: true);
		}
		else
		{
			downArrows[last7DaysSoldAmountSortIndex].SetActive(value: true);
		}
		RepaintSorted();
	}

	public void OrderByLast30DaysSoldAmount()
	{
		bool activeSelf = downArrows[last30DaysSoldAmountSortIndex].activeSelf;
		ResetArrows();
		upAndDownArrows[last30DaysSoldAmountSortIndex].SetActive(value: false);
		currentOrderBy = (activeSelf ? OrderBy.LAST_30_DAYS_SOLD_AMOUNT_REVERSE : OrderBy.LAST_30_DAYS_SOLD_AMOUNT);
		if (activeSelf)
		{
			upArrows[last30DaysSoldAmountSortIndex].SetActive(value: true);
		}
		else
		{
			downArrows[last30DaysSoldAmountSortIndex].SetActive(value: true);
		}
		RepaintSorted();
	}

	private void ResetArrows()
	{
		for (int i = 0; i < upAndDownArrows.Count; i++)
		{
			upAndDownArrows[i].SetActive(value: true);
		}
		for (int j = 0; j < downArrows.Count; j++)
		{
			downArrows[j].SetActive(value: false);
		}
		for (int k = 0; k < upArrows.Count; k++)
		{
			upArrows[k].SetActive(value: false);
		}
	}

	private void Awake()
	{
		EventManager.AddListener(StartupEvents.SPAWN_MANAGER_INITIALIZED, Initialize);
		EventManager.AddListener<int, ProductGroup>(ProductEvents.PRODUCT_LICENSE_PURCHASED, OnLicensePurchased);
		EventManager.AddListener<ProductType>(UIEvents.OPEN_PRODUCT_SALES_GRAPH, OnOpenProductSalesGraph);
		EventManager.AddListener(UIEvents.CLOSE_PRODUCT_SALES_GRAPH, OnCloseProductSalesGraph);
	}

	private void OnCloseProductSalesGraph()
	{
		SingletonBehaviour<InputManager>.Instance.SelectElement(filterDropdown.gameObject);
	}

	private void OnOpenProductSalesGraph(ProductType type)
	{
		productSalesLineChartUI.Repaint(type);
		productSalesLineChartUI.Open();
	}

	private void OnLicensePurchased(int level, ProductGroup group)
	{
		if (group == currentFilter)
		{
			RepaintSorted();
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		for (int i = 0; i < EmployeeCard.DEPARTMENT_OPTIONS.Count; i++)
		{
			if (EmployeeCard.DEPARTMENT_OPTIONS[i] != ProductGroup.NONE && EmployeeCard.DEPARTMENT_OPTIONS[i] != ProductGroup.EQUIPMENTS)
			{
				filterOptions.Add(EmployeeCard.DEPARTMENT_OPTIONS[i]);
			}
		}
		RefreshFilterOptions();
		filterDropdown.value = filterOptions.IndexOf(SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup);
		filterDropdown.onValueChanged.AddListener(delegate(int index)
		{
			ApplyFilter(filterOptions[index]);
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		});
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		ApplyFilter(SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup);
	}

	public void OnNewDayStarted()
	{
		RepaintSorted();
	}

	private void OnLanguageChanged()
	{
		RefreshFilterOptions();
		RepaintSorted();
	}

	private void RefreshFilterOptions()
	{
		filterDropdown.ClearOptions();
		List<string> list = new List<string>();
		for (int i = 0; i < filterOptions.Count; i++)
		{
			list.Add(Locale.GetWord(filterOptions[i].ToString().ToLowerInvariant()));
		}
		filterDropdown.AddOptions(list);
	}

	public void ApplyFilter(ProductGroup productGroup)
	{
		currentFilter = productGroup;
		SortProducts();
		RepaintSorted();
		RefreshNavigation();
	}

	private void SortProducts()
	{
		currentProducts.Clear();
		List<ProductType> list = ((currentFilter == ProductGroup.VENDING) ? SingletonBehaviour<VendingStockManager>.Instance.PurchasableProducts : SingletonBehaviour<StockManager>.Instance.PurchasableProducts);
		for (int i = 0; i < list.Count; i++)
		{
			ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(list[i]);
			if (productData.productGroup == currentFilter && ProductLicenseManager.LicensePurchased(productData.requiredLicense, productData.productGroup))
			{
				currentProducts.Add(productData);
			}
		}
		if (currentOrderBy == OrderBy.LICENSE)
		{
			currentProducts.Sort((ProductData a, ProductData b) => a.requiredLicense.CompareTo(b.requiredLicense));
		}
		else if (currentOrderBy == OrderBy.PRODUCT_NAME)
		{
			currentProducts.Sort((ProductData a, ProductData b) => string.Compare(Locale.GetWord(b.type.ToString()), Locale.GetWord(a.type.ToString()), StringComparison.Ordinal));
		}
		else if (currentOrderBy == OrderBy.PRODUCT_NAME_REVERSE)
		{
			currentProducts.Sort((ProductData a, ProductData b) => string.Compare(Locale.GetWord(a.type.ToString()), Locale.GetWord(b.type.ToString()), StringComparison.Ordinal));
		}
		else if (currentOrderBy == OrderBy.LAST_DAY_SOLD_AMOUNT)
		{
			currentProducts.Sort((ProductData a, ProductData b) => SingletonBehaviour<StatisticsManager>.Instance.GetOneDaySoldAmount(b.type).CompareTo(SingletonBehaviour<StatisticsManager>.Instance.GetOneDaySoldAmount(a.type)));
		}
		else if (currentOrderBy == OrderBy.LAST_DAY_SOLD_AMOUNT_REVERSE)
		{
			currentProducts.Sort((ProductData a, ProductData b) => SingletonBehaviour<StatisticsManager>.Instance.GetOneDaySoldAmount(a.type).CompareTo(SingletonBehaviour<StatisticsManager>.Instance.GetOneDaySoldAmount(b.type)));
		}
		else if (currentOrderBy == OrderBy.LAST_7_DAYS_SOLD_AMOUNT)
		{
			currentProducts.Sort((ProductData a, ProductData b) => SingletonBehaviour<StatisticsManager>.Instance.Get7DaysSoldAmount(b.type).CompareTo(SingletonBehaviour<StatisticsManager>.Instance.Get7DaysSoldAmount(a.type)));
		}
		else if (currentOrderBy == OrderBy.LAST_7_DAYS_SOLD_AMOUNT_REVERSE)
		{
			currentProducts.Sort((ProductData a, ProductData b) => SingletonBehaviour<StatisticsManager>.Instance.Get7DaysSoldAmount(a.type).CompareTo(SingletonBehaviour<StatisticsManager>.Instance.Get7DaysSoldAmount(b.type)));
		}
		else if (currentOrderBy == OrderBy.LAST_30_DAYS_SOLD_AMOUNT)
		{
			currentProducts.Sort((ProductData a, ProductData b) => SingletonBehaviour<StatisticsManager>.Instance.Get30DaysSoldAmount(b.type).CompareTo(SingletonBehaviour<StatisticsManager>.Instance.Get30DaysSoldAmount(a.type)));
		}
		else if (currentOrderBy == OrderBy.LAST_30_DAYS_SOLD_AMOUNT_REVERSE)
		{
			currentProducts.Sort((ProductData a, ProductData b) => SingletonBehaviour<StatisticsManager>.Instance.Get30DaysSoldAmount(a.type).CompareTo(SingletonBehaviour<StatisticsManager>.Instance.Get30DaysSoldAmount(b.type)));
		}
	}

	private void RepaintSorted()
	{
		SortProducts();
		Repaint();
	}

	private void Repaint()
	{
		for (int i = 0; i < currentProducts.Count; i++)
		{
			if (currentProducts[i].requiredLicense <= 999)
			{
				if (i >= productStatRows.Count)
				{
					ProductStatRow productStatRow = UnityEngine.Object.Instantiate(productStatRowPrefab, productStatRowPrefab.transform.parent);
					int oneDaySoldAmount = SingletonBehaviour<StatisticsManager>.Instance.GetOneDaySoldAmount(currentProducts[i].type);
					int last7DaysSold = SingletonBehaviour<StatisticsManager>.Instance.Get7DaysSoldAmount(currentProducts[i].type);
					int last30DaysSold = SingletonBehaviour<StatisticsManager>.Instance.Get30DaysSoldAmount(currentProducts[i].type);
					productStatRow.Repaint(currentProducts[i], i, oneDaySoldAmount, last7DaysSold, last30DaysSold);
					productStatRows.Add(productStatRow);
					productStatRow.gameObject.SetActive(value: true);
				}
				else
				{
					int oneDaySoldAmount2 = SingletonBehaviour<StatisticsManager>.Instance.GetOneDaySoldAmount(currentProducts[i].type);
					int last7DaysSold2 = SingletonBehaviour<StatisticsManager>.Instance.Get7DaysSoldAmount(currentProducts[i].type);
					int last30DaysSold2 = SingletonBehaviour<StatisticsManager>.Instance.Get30DaysSoldAmount(currentProducts[i].type);
					productStatRows[i].Repaint(currentProducts[i], i, oneDaySoldAmount2, last7DaysSold2, last30DaysSold2);
					productStatRows[i].gameObject.SetActive(value: true);
				}
			}
		}
		for (int j = currentProducts.Count; j < productStatRows.Count; j++)
		{
			if (productStatRows[j] != null)
			{
				productStatRows[j].gameObject.SetActive(value: false);
			}
		}
	}

	private void RefreshNavigation()
	{
		if (productStatRows.Count > 0 && productStatRows[0].gameObject.activeSelf)
		{
			foreach (Button headerButton in headerButtons)
			{
				Navigation navigation = headerButton.navigation;
				navigation.selectOnDown = productStatRows[0].GetSelectable();
				headerButton.navigation = navigation;
			}
		}
		else
		{
			foreach (Button headerButton2 in headerButtons)
			{
				Navigation navigation2 = headerButton2.navigation;
				navigation2.selectOnDown = null;
				headerButton2.navigation = navigation2;
			}
		}
		for (int i = 0; i < productStatRows.Count; i++)
		{
			ProductStatRow productStatRow = productStatRows[i];
			if (!(productStatRow == null) && productStatRow.gameObject.activeSelf)
			{
				if (i == 0)
				{
					Selectable down = ((productStatRows.Count > i + 1) ? productStatRows[i + 1].GetSelectable() : null);
					productStatRow.RefreshNavigation(headerButtons[0], down, null, null);
				}
				else if (i == productStatRows.Count - 1)
				{
					productStatRow.RefreshNavigation(productStatRows[i - 1].GetSelectable(), null, null, null);
				}
				else
				{
					productStatRow.RefreshNavigation(productStatRows[i - 1].GetSelectable(), productStatRows[i + 1].GetSelectable(), null, null);
				}
			}
		}
	}

	public override Selectable GetFirstSelectable()
	{
		return filterDropdown;
	}
}
