using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrowthUI : SelectableUI
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI requiredStoreText;

	[SerializeField]
	private TextMeshProUGUI requiredExpansionText;

	[SerializeField]
	private TextMeshProUGUI price;

	[SerializeField]
	private Button purchase;

	[SerializeField]
	private TextMeshProUGUI owned;

	[SerializeField]
	private TextMeshProUGUI expandInfoText;

	private int globalGrowthCount = -1;

	private int growthLevel = -1;

	private const int MAX_GROWTH_LEVEL_DEMO = 12;

	private static Dictionary<int, int> growthLevelToRequiredLevelMap = new Dictionary<int, int>
	{
		{ 1, 6 },
		{ 2, 8 },
		{ 3, 13 },
		{ 4, 16 },
		{ 5, 21 },
		{ 6, 26 },
		{ 7, 31 },
		{ 8, 36 },
		{ 9, 41 },
		{ 10, 47 },
		{ 11, 53 },
		{ 12, 58 },
		{ 13, 63 },
		{ 14, 68 },
		{ 15, 73 },
		{ 16, 78 },
		{ 17, 83 }
	};

	public static Dictionary<int, int> growthLevelToPriceMap = new Dictionary<int, int>
	{
		{ 1, 600 },
		{ 2, 1200 },
		{ 3, 1800 },
		{ 4, 1100 },
		{ 5, 1600 },
		{ 6, 1700 },
		{ 7, 2300 },
		{ 8, 3000 },
		{ 9, 3800 },
		{ 10, 4700 },
		{ 11, 5700 },
		{ 12, 6800 },
		{ 13, 8000 },
		{ 14, 9300 },
		{ 15, 10700 },
		{ 16, 12200 },
		{ 17, 13800 }
	};

	public static int GetRequiredPlayerLevelForGrowth(int growthLevel)
	{
		return 1 + growthLevel * 3;
	}

	public static int GetGrowthPrice(int growthLevel)
	{
		float num = 45f;
		return Mathf.RoundToInt((600f + num * (float)growthLevel * (float)growthLevel) / 100f) * 100;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		if (growthLevel != -1)
		{
			requiredStoreText.text = Locale.GetWord("required_level_n").Replace("{0}", GetRequiredPlayerLevelForGrowth(growthLevel).ToString());
		}
		RepaintExpandInfoText();
	}

	private void OnGrowthPurchased(int growthLevel)
	{
		if (SingletonBehaviour<GrowthManager>.Instance.IsUpperFloor(growthLevel - 1))
		{
			EventLogger.SecondFloorUnlocked();
		}
		RepaintContent();
	}

	private void RepaintExpandInfoText()
	{
		if (SingletonBehaviour<GrowthManager>.Instance.IsUpperFloor(growthLevel))
		{
			expandInfoText.text = Locale.GetWord("expand_info_upper");
		}
		else
		{
			expandInfoText.text = Locale.GetWord("expand_info_lower");
		}
	}

	public void Initialize(int growthLevel)
	{
		this.growthLevel = growthLevel;
		title.text = Locale.GetWord("store_expansion_n").Replace("{0}", growthLevel.ToString());
		RepaintExpandInfoText();
		RepaintContent();
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		EventManager.AddListener<int>(SupermarketEvents.GROWTH_PURCHASED, OnGrowthPurchased);
		EventManager.AddListener<int>(EconomyEvents.LEVEL_UP, RepaintRequiredLevel);
	}

	private void RepaintContent()
	{
		globalGrowthCount = SingletonBehaviour<GrowthManager>.Instance.GrowthCount + 1;
		requiredStoreText.text = Locale.GetWord("required_level_n").Replace("{0}", GetRequiredPlayerLevelForGrowth(growthLevel).ToString());
		if (growthLevel < globalGrowthCount)
		{
			price.enabled = false;
			purchase.gameObject.SetActive(value: false);
			requiredStoreText.enabled = false;
			owned.enabled = true;
			requiredExpansionText.enabled = false;
		}
		else
		{
			RepaintRequiredExpansion();
			price.text = "$" + GetGrowthPrice(growthLevel);
			price.enabled = true;
			purchase.gameObject.SetActive(value: true);
			owned.enabled = false;
			purchase.onClick.RemoveAllListeners();
			purchase.onClick.AddListener(OnPurchase);
		}
		RepaintRequiredLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		if (GameManager.isDemo && growthLevel >= 12)
		{
			purchase.GetComponentInChildren<TextMeshProUGUI>().text = Locale.GetWord("wishlist_now");
			requiredStoreText.enabled = false;
			requiredExpansionText.enabled = false;
		}
	}

	private void RepaintRequiredExpansion()
	{
		if (growthLevel == 1)
		{
			requiredExpansionText.enabled = false;
		}
		requiredExpansionText.text = Locale.GetWord("required_expansion_n").Replace("{0}", (growthLevel - 1).ToString());
		if (IsRequiredExpansionPurchased())
		{
			requiredExpansionText.color = Color.green;
		}
		else
		{
			requiredExpansionText.color = UIManager.RedColor;
		}
	}

	private bool IsRequiredExpansionPurchased()
	{
		return growthLevel == globalGrowthCount;
	}

	private void RepaintRequiredLevel(int newLevel)
	{
		if (newLevel >= GetRequiredPlayerLevelForGrowth(growthLevel))
		{
			requiredStoreText.color = Color.green;
		}
		else
		{
			requiredStoreText.color = UIManager.RedColor;
		}
		purchase.interactable = IsRequiredExpansionPurchased() && newLevel >= GetRequiredPlayerLevelForGrowth(growthLevel);
	}

	private void OnPurchase()
	{
		if (GameManager.isDemo && growthLevel >= 12)
		{
			Application.OpenURL("steam://store/3819640");
		}
		else if (SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel >= GetRequiredPlayerLevelForGrowth(growthLevel) && SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(GetGrowthPrice(growthLevel)))
		{
			EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, (float)GetGrowthPrice(growthLevel));
			EventManager.NotifyEvent(StatisticsEvents.UPGRADE_COST, (float)GetGrowthPrice(growthLevel));
			SingletonBehaviour<GrowthManager>.Instance.PurchaseGrowth();
			if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
			{
				_ = purchase.navigation;
				Selectable availableSelectable = GetAvailableSelectable();
				SingletonBehaviour<InputManager>.Instance.SelectElement(availableSelectable.gameObject);
			}
			EventManager.NotifyEvent(SupermarketEvents.GROWTH_PURCHASED, growthLevel + 1);
			EventManager.NotifyEvent(UIEvents.TAB_SELECTED_OBJECT_CHANGED);
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
