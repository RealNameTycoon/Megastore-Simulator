using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : SingletonBehaviour<ExperienceManager>
{
	[SerializeField]
	private TextMeshProUGUI currentLevelText;

	[SerializeField]
	private Image currentProgressFillImage;

	private const string currentXPKey = "currentXPKey";

	private const string currentLevelKey = "currentLevelKey";

	public const int BASE_LEVEL_UP_SOFT_CURRENCY_REWARD = 250;

	public const int BASE_LEVEL_UP_HARD_CURRENCY_REWARD = 5;

	private int currentXP = -1;

	private int currentLevel = -1;

	public int CurrentXP
	{
		get
		{
			return GenericDataSerializer.LoadInt("currentXPKey");
		}
		set
		{
			GenericDataSerializer.SaveInt("currentXPKey", value);
		}
	}

	public int CurrentLevel
	{
		get
		{
			return GenericDataSerializer.LoadInt("currentLevelKey", 1);
		}
		set
		{
			GenericDataSerializer.SaveInt("currentLevelKey", value);
		}
	}

	public void LevelUp()
	{
		currentLevel++;
		currentLevelText.text = currentLevel.ToString();
		CurrentXP = currentXP;
		CurrentLevel = currentLevel;
		EventManager.NotifyEvent(EconomyEvents.LEVEL_UP, currentLevel);
	}

	private void Start()
	{
		currentLevel = CurrentLevel;
		currentXP = CurrentXP;
		EventManager.AddListener<float>(PaymentEvents.CASH_PAYMENT_DONE, delegate
		{
			OnPaymentDone();
		});
		EventManager.AddListener<float>(PaymentEvents.POS_PAYMENT_DONE, delegate
		{
			OnPaymentDone();
		});
		EventManager.AddListener<float>(PaymentEvents.VENDING_PAYMENT_DONE, delegate
		{
			OnPaymentDone();
		});
		Repaint();
	}

	private void OnPaymentDone()
	{
		if (GameManager.isPlaytest)
		{
			currentXP += 5;
		}
		else
		{
			currentXP++;
		}
		if (currentXP >= GetCurrentRequiredXP(currentLevel))
		{
			EventLogger.LogEvent("c_level_finished" + currentLevel.ToString("000"));
			currentXP = 0;
			currentLevel++;
			CurrentLevel = currentLevel;
			currentLevelText.text = currentLevel.ToString();
			EventManager.NotifyEvent(EconomyEvents.LEVEL_UP, currentLevel);
		}
		currentProgressFillImage.fillAmount = (float)currentXP / (float)GetCurrentRequiredXP(currentLevel);
		CurrentXP = currentXP;
	}

	private void Repaint()
	{
		currentProgressFillImage.fillAmount = (float)currentXP / (float)GetCurrentRequiredXP(currentLevel);
		currentLevelText.text = currentLevel.ToString();
	}

	private int GetCurrentRequiredXP(int currentLevel)
	{
		return 5 * (currentLevel + 1);
	}
}
