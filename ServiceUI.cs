using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ServiceUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI requiredStoreText;

	[SerializeField]
	protected TextMeshProUGUI priceText;

	[SerializeField]
	protected Button purchase;

	[SerializeField]
	private int requiredLevel;

	[SerializeField]
	private int price;

	[SerializeField]
	private UnityEvent serviceAction;

	private void Start()
	{
		Initialize();
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		requiredStoreText.text = Locale.GetWord("required_level_n").Replace("{0}", requiredLevel.ToString());
	}

	public void Initialize()
	{
		requiredStoreText.text = Locale.GetWord("required_level_n").Replace("{0}", requiredLevel.ToString());
		RepaintForLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		priceText.text = "<sprite index=0>" + price;
		purchase.onClick.AddListener(OnPurchase);
		EventManager.AddListener<int>(EconomyEvents.LEVEL_UP, RepaintForLevel);
	}

	private void RepaintForLevel(int newLevel)
	{
		if (newLevel >= requiredLevel)
		{
			requiredStoreText.color = Color.green;
			purchase.interactable = true;
		}
		else
		{
			purchase.interactable = false;
		}
	}

	private void OnPurchase()
	{
		if (CanPurchase() && SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel >= requiredLevel && SingletonBehaviour<EconomyManager>.Instance.HasEnoughHardCurrency(price))
		{
			EventManager.NotifyEvent(EconomyEvents.REMOVE_HARD_CURRENCY, price);
			serviceAction?.Invoke();
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_energy", base.transform);
		}
	}

	protected virtual bool CanPurchase()
	{
		return true;
	}
}
