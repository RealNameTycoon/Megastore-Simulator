using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageGrowthUI : SelectableUI
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI requiredStoreText;

	[SerializeField]
	private TextMeshProUGUI price;

	[SerializeField]
	private TextMeshProUGUI unlocksLoadingDockText;

	[SerializeField]
	private TextMeshProUGUI requiredSectionText;

	[SerializeField]
	private Button purchase;

	[SerializeField]
	private TextMeshProUGUI owned;

	private int growthLevel = -1;

	private static Dictionary<int, int> growthLevelToRequiredLevelMap = new Dictionary<int, int>
	{
		{ 1, 10 },
		{ 2, 15 },
		{ 3, 20 },
		{ 4, 25 },
		{ 5, 30 },
		{ 6, 35 },
		{ 7, 40 },
		{ 8, 45 },
		{ 9, 50 },
		{ 10, 55 },
		{ 11, 60 },
		{ 12, 65 },
		{ 13, 70 },
		{ 14, 75 },
		{ 15, 80 },
		{ 16, 85 },
		{ 17, 90 },
		{ 18, 95 },
		{ 19, 100 },
		{ 20, 105 },
		{ 21, 110 },
		{ 22, 115 },
		{ 23, 120 },
		{ 24, 125 },
		{ 25, 130 },
		{ 26, 135 },
		{ 27, 140 },
		{ 28, 145 },
		{ 29, 150 },
		{ 30, 155 }
	};

	private static Dictionary<int, int> growthLevelToPriceMap = new Dictionary<int, int>
	{
		{ 1, 700 },
		{ 2, 1000 },
		{ 3, 2000 },
		{ 4, 3500 },
		{ 5, 5000 },
		{ 6, 7000 },
		{ 7, 9000 },
		{ 8, 11000 },
		{ 9, 13000 },
		{ 10, 15000 },
		{ 11, 17000 },
		{ 12, 19000 },
		{ 13, 21000 },
		{ 14, 23000 },
		{ 15, 25000 },
		{ 16, 27000 },
		{ 17, 29000 },
		{ 18, 31000 },
		{ 19, 33000 },
		{ 20, 35000 },
		{ 21, 37000 },
		{ 22, 39000 },
		{ 23, 41000 },
		{ 24, 43000 },
		{ 25, 45000 },
		{ 26, 47000 },
		{ 27, 49000 },
		{ 28, 51000 },
		{ 29, 53000 },
		{ 30, 55000 }
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
		if (growthLevel != -1)
		{
			title.text = Locale.GetWord("warehouse_expansion").Replace("{0}", growthLevel.ToString());
			requiredStoreText.text = Locale.GetWord("required_level_n").Replace("{0}", growthLevelToRequiredLevelMap[growthLevel].ToString());
		}
	}

	public void Initialize(int growthLevel)
	{
		this.growthLevel = growthLevel;
		title.text = Locale.GetWord("warehouse_expansion").Replace("{0}", growthLevel.ToString());
		requiredStoreText.text = Locale.GetWord("required_level_n").Replace("{0}", growthLevelToRequiredLevelMap[growthLevel].ToString());
		RepaintForLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		if (SingletonBehaviour<StorageManager>.Instance.GrowthPurchased(growthLevel))
		{
			price.enabled = false;
			purchase.gameObject.SetActive(value: false);
			owned.enabled = true;
		}
		else
		{
			price.text = "$" + growthLevelToPriceMap[growthLevel];
			price.enabled = true;
			purchase.gameObject.SetActive(value: true);
			owned.enabled = false;
			purchase.onClick.AddListener(OnPurchase);
		}
		EventManager.AddListener<int>(EconomyEvents.LEVEL_UP, RepaintForLevel);
		EventManager.AddListener<int>(SupermarketEvents.STORAGE_GROWTH_PURCHASED, delegate
		{
			RepaintForLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		});
	}

	private void RepaintForLevel(int newLevel)
	{
		bool flag = SingletonBehaviour<StorageManager>.Instance.GrowthPurchased(growthLevel - 1);
		bool flag2 = newLevel >= growthLevelToRequiredLevelMap[growthLevel];
		requiredStoreText.color = (flag2 ? UIManager.GreenColor : UIManager.RedColor);
		requiredSectionText.color = (flag ? UIManager.GreenColor : UIManager.RedColor);
		purchase.interactable = flag2 && flag;
		RepaintUnlocksLoadingDockText();
		RepaintRequiredSectionText();
	}

	private void RepaintUnlocksLoadingDockText()
	{
		if (growthLevel % 5 == 0)
		{
			unlocksLoadingDockText.enabled = true;
			unlocksLoadingDockText.text = Locale.GetWord("unlocks_loading_dock_n").Replace("{0}", (growthLevel / 5).ToString());
		}
		else
		{
			unlocksLoadingDockText.enabled = false;
		}
	}

	private void RepaintRequiredSectionText()
	{
		if (growthLevel == 1)
		{
			requiredSectionText.enabled = false;
			return;
		}
		requiredSectionText.enabled = true;
		requiredSectionText.text = Locale.GetWord("required_section_n").Replace("{0}", (growthLevel - 1).ToString());
	}

	private void OnPurchase()
	{
		if (SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel >= growthLevelToRequiredLevelMap[growthLevel] && SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(growthLevelToPriceMap[growthLevel]))
		{
			EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, (float)growthLevelToPriceMap[growthLevel]);
			SingletonBehaviour<StorageManager>.Instance.PurchaseGrowth(growthLevel);
			if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
			{
				_ = purchase.navigation;
				Selectable availableSelectable = GetAvailableSelectable();
				SingletonBehaviour<InputManager>.Instance.SelectElement(availableSelectable.gameObject);
			}
			price.enabled = false;
			purchase.gameObject.SetActive(value: false);
			owned.enabled = true;
			EventManager.NotifyEvent(SupermarketEvents.STORAGE_GROWTH_PURCHASED, growthLevel);
			EventManager.NotifyEvent(UIEvents.TAB_SELECTED_OBJECT_CHANGED);
			EventLogger.LogEvent("c_storage_growth_purchased_" + growthLevel);
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
