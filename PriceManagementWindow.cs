using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceManagementWindow : TabbedPanel
{
	[SerializeField]
	private ProductPriceRow productPriceRowPrefab;

	[SerializeField]
	private TMP_Dropdown filterDropdown;

	[SerializeField]
	private Button setAllToMarketPriceButton;

	[SerializeField]
	private Button applyAllPricesButton;

	[SerializeField]
	private GameObject scrollParent;

	private List<ProductPriceRow> productPriceRows = new List<ProductPriceRow>();

	private ProductGroup currentFilter = ProductGroup.NONE;

	private List<ProductData> currentProducts = new List<ProductData>();

	[SerializeField]
	private List<ProductGroup> filterOptions = new List<ProductGroup>();

	private void Awake()
	{
		EventManager.AddListener(StartupEvents.SPAWN_MANAGER_INITIALIZED, Initialize);
		EventManager.AddListener<int, ProductGroup>(ProductEvents.PRODUCT_LICENSE_PURCHASED, OnLicensePurchased);
	}

	private void OnLicensePurchased(int level, ProductGroup group)
	{
		if (group == currentFilter)
		{
			Repaint();
		}
	}

	private void Initialize()
	{
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
		setAllToMarketPriceButton.onClick.AddListener(delegate
		{
			SetAllToMarketPrice();
		});
		applyAllPricesButton.onClick.AddListener(delegate
		{
			ApplyAllPrices();
		});
	}

	private void SetAllToMarketPrice()
	{
		if (!IsOpen())
		{
			return;
		}
		for (int i = 0; i < productPriceRows.Count; i++)
		{
			if (productPriceRows[i].gameObject.activeSelf)
			{
				productPriceRows[i].SetMarketPrice();
			}
		}
	}

	private void ApplyAllPrices()
	{
		if (!IsOpen())
		{
			return;
		}
		for (int i = 0; i < productPriceRows.Count; i++)
		{
			if (productPriceRows[i].gameObject.activeSelf)
			{
				productPriceRows[i].ApplyPrice();
			}
		}
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		RefreshFilterOptions();
		Repaint();
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

	public new void Repaint()
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
		currentProducts.Sort((ProductData a, ProductData b) => a.requiredLicense.CompareTo(b.requiredLicense));
		for (int num = 0; num < currentProducts.Count; num++)
		{
			if (currentProducts[num].requiredLicense <= 999)
			{
				if (num >= productPriceRows.Count)
				{
					ProductPriceRow productPriceRow = Object.Instantiate(productPriceRowPrefab, productPriceRowPrefab.transform.parent);
					productPriceRow.Repaint(currentProducts[num], num);
					productPriceRows.Add(productPriceRow);
					productPriceRow.gameObject.SetActive(value: true);
				}
				else
				{
					productPriceRows[num].Repaint(currentProducts[num], num);
					productPriceRows[num].gameObject.SetActive(value: true);
				}
			}
		}
		for (int num2 = currentProducts.Count; num2 < productPriceRows.Count; num2++)
		{
			productPriceRows[num2].gameObject.SetActive(value: false);
		}
	}

	public void ApplyFilter(ProductGroup productGroup)
	{
		currentFilter = productGroup;
		Repaint();
		RefreshNavigation();
	}

	private void RefreshNavigation()
	{
		bool num = productPriceRows.Count > 0 && productPriceRows[0].gameObject.activeSelf;
		Navigation navigation = applyAllPricesButton.navigation;
		Navigation navigation2 = setAllToMarketPriceButton.navigation;
		if (num)
		{
			navigation.selectOnDown = productPriceRows[0].GetSelectable();
			navigation2.selectOnDown = productPriceRows[0].GetSelectable();
		}
		else
		{
			navigation.selectOnDown = null;
			navigation2.selectOnDown = null;
		}
		applyAllPricesButton.navigation = navigation;
		setAllToMarketPriceButton.navigation = navigation2;
		for (int i = 0; i < productPriceRows.Count; i++)
		{
			ProductPriceRow productPriceRow = productPriceRows[i];
			if (!(productPriceRow == null) && productPriceRow.gameObject.activeSelf)
			{
				if (i == 0)
				{
					Selectable down = ((productPriceRows.Count > i + 1) ? productPriceRows[i + 1].GetSelectable() : null);
					productPriceRow.RefreshNavigation(filterDropdown, down, null, null);
				}
				else if (i == productPriceRows.Count - 1)
				{
					productPriceRow.RefreshNavigation(productPriceRows[i - 1].GetSelectable(), null, null, null);
				}
				else
				{
					productPriceRow.RefreshNavigation(productPriceRows[i - 1].GetSelectable(), productPriceRows[i + 1].GetSelectable(), null, null);
				}
			}
		}
	}

	public override void Open()
	{
		base.Open();
		scrollParent.SetActive(value: true);
	}

	public override void Close()
	{
		base.Close();
		scrollParent.SetActive(value: false);
	}
}
