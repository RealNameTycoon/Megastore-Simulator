using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LicenseUI : SelectableUI
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private List<TextMeshProUGUI> productNames;

	[SerializeField]
	private TextMeshProUGUI price;

	[SerializeField]
	private Button purchase;

	[SerializeField]
	private TextMeshProUGUI requiredLevel;

	[SerializeField]
	private TextMeshProUGUI owned;

	[SerializeField]
	private List<Image> itemImages;

	private string productNameText = "<color=#00F1FF>{0} </color> {1}";

	private int licenseLevel = -1;

	private ProductGroup productGroup;

	private static Dictionary<int, int> licenseLevelToPriceMap = new Dictionary<int, int>
	{
		{ 1, 100 },
		{ 2, 150 },
		{ 3, 200 },
		{ 4, 300 },
		{ 5, 400 },
		{ 6, 800 },
		{ 7, 1000 },
		{ 8, 1200 },
		{ 9, 1400 },
		{ 10, 1600 },
		{ 11, 1800 },
		{ 12, 2000 },
		{ 13, 2200 },
		{ 14, 2400 },
		{ 15, 2600 },
		{ 16, 2800 },
		{ 17, 3000 },
		{ 18, 3200 },
		{ 19, 3400 },
		{ 20, 3600 },
		{ 21, 3800 },
		{ 22, 4000 },
		{ 23, 4200 },
		{ 24, 4400 },
		{ 25, 4600 }
	};

	private static Dictionary<ProductGroup, int> productGroupToPriceMultiplierMap = new Dictionary<ProductGroup, int>
	{
		{
			ProductGroup.GROCERY,
			1
		},
		{
			ProductGroup.TOY,
			2
		},
		{
			ProductGroup.CLOTHING,
			2
		},
		{
			ProductGroup.BAKERY,
			2
		},
		{
			ProductGroup.SPORTS,
			4
		},
		{
			ProductGroup.MUSIC,
			4
		},
		{
			ProductGroup.ELECTRONICS,
			6
		},
		{
			ProductGroup.VENDING,
			2
		},
		{
			ProductGroup.FISH,
			3
		},
		{
			ProductGroup.BEACH,
			3
		}
	};

	private static Dictionary<int, int> licenseLevelToRequiredLevelMap = new Dictionary<int, int>
	{
		{ 1, 1 },
		{ 2, 3 },
		{ 3, 5 },
		{ 4, 7 },
		{ 5, 9 },
		{ 6, 10 },
		{ 7, 11 },
		{ 8, 13 },
		{ 9, 16 },
		{ 10, 19 },
		{ 11, 22 },
		{ 12, 24 },
		{ 13, 27 },
		{ 14, 30 },
		{ 15, 32 },
		{ 16, 35 },
		{ 17, 37 },
		{ 18, 40 },
		{ 19, 42 },
		{ 20, 44 },
		{ 21, 47 },
		{ 22, 50 },
		{ 23, 53 },
		{ 24, 56 },
		{ 25, 59 }
	};

	private void Start()
	{
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		if (licenseLevel != -1)
		{
			title.text = Locale.GetWord("product_license_n").Replace("{0}", licenseLevel.ToString());
			requiredLevel.text = Locale.GetWord("required_level_n").Replace("{0}", licenseLevelToRequiredLevelMap[licenseLevel].ToString());
		}
	}

	public void SetImages(List<ProductData> products)
	{
		if (products.Count == 4)
		{
			itemImages[0].sprite = products[0].productSprite;
			itemImages[0].gameObject.SetActive(value: true);
			itemImages[1].sprite = products[1].productSprite;
			itemImages[1].gameObject.SetActive(value: true);
			itemImages[2].gameObject.SetActive(value: false);
			itemImages[3].sprite = products[2].productSprite;
			itemImages[3].gameObject.SetActive(value: true);
			itemImages[4].sprite = products[3].productSprite;
			itemImages[4].gameObject.SetActive(value: true);
			itemImages[5].gameObject.SetActive(value: false);
			return;
		}
		for (int i = 0; i < itemImages.Count; i++)
		{
			if (i < products.Count && products[i].productSprite != null)
			{
				itemImages[i].sprite = products[i].productSprite;
				itemImages[i].gameObject.SetActive(value: true);
			}
		}
		for (int j = products.Count; j < itemImages.Count; j++)
		{
			itemImages[j].gameObject.SetActive(value: false);
		}
	}

	public void Initialize(int licenseLevel, ProductGroup licenseGroup)
	{
		this.licenseLevel = licenseLevel;
		productGroup = licenseGroup;
		title.text = Locale.GetWord("product_license_n").Replace("{0}", licenseLevel.ToString());
		RepaintForLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		requiredLevel.text = Locale.GetWord("required_level_n").Replace("{0}", licenseLevelToRequiredLevelMap[licenseLevel].ToString());
		if (ProductLicenseManager.LicensePurchased(licenseLevel, licenseGroup))
		{
			price.enabled = false;
			purchase.gameObject.SetActive(value: false);
			owned.enabled = true;
		}
		else
		{
			price.text = "$" + licenseLevelToPriceMap[licenseLevel] * productGroupToPriceMultiplierMap[productGroup];
			price.enabled = true;
			purchase.gameObject.SetActive(value: true);
			owned.enabled = false;
			purchase.onClick.AddListener(OnPurchase);
		}
		EventManager.AddListener<int>(EconomyEvents.LEVEL_UP, RepaintForLevel);
		EventManager.AddListener<int, ProductGroup>(ProductEvents.PRODUCT_LICENSE_PURCHASED, OnLicensePurchased);
	}

	private void OnLicensePurchased(int level, ProductGroup group)
	{
		if (licenseLevel == level && group == productGroup && price.enabled)
		{
			price.enabled = false;
			purchase.gameObject.SetActive(value: false);
			owned.enabled = true;
			EventManager.NotifyEvent(UIEvents.TAB_SELECTED_OBJECT_CHANGED);
		}
	}

	private void RepaintForLevel(int newLevel)
	{
		if (newLevel >= licenseLevelToRequiredLevelMap[licenseLevel])
		{
			requiredLevel.color = Color.green;
			purchase.interactable = true;
		}
		else
		{
			purchase.interactable = false;
		}
	}

	private void OnPurchase()
	{
		if (SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel >= licenseLevelToRequiredLevelMap[licenseLevel] && SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(licenseLevelToPriceMap[licenseLevel] * productGroupToPriceMultiplierMap[productGroup]))
		{
			EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, (float)(licenseLevelToPriceMap[licenseLevel] * productGroupToPriceMultiplierMap[productGroup]));
			ProductLicenseManager.PurchaseLicense(licenseLevel, productGroup);
			EventManager.NotifyEvent(ProductEvents.PRODUCT_LICENSE_PURCHASED, licenseLevel, productGroup);
			EventLogger.LogEvent("c_license_purchased_" + licenseLevel);
			price.enabled = false;
			purchase.gameObject.SetActive(value: false);
			owned.enabled = true;
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_cash", base.transform);
		}
	}

	public override Selectable GetSelectable()
	{
		return purchase;
	}
}
