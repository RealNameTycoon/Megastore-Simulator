using System.Collections;
using System.Globalization;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndDayReportWindow : UIWindow
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI satisfiedCustomerValueText;

	[SerializeField]
	private TextMeshProUGUI unavailableProductValueText;

	[SerializeField]
	private TextMeshProUGUI foundExpensiveProductsValueText;

	[SerializeField]
	private TextMeshProUGUI overpaidCustomersValueText;

	[SerializeField]
	private TextMeshProUGUI storeExperienceValueText;

	[SerializeField]
	private TextMeshProUGUI totalCustomersValueText;

	[SerializeField]
	private TextMeshProUGUI totalSoldProductsValueText;

	[SerializeField]
	private TextMeshProUGUI mostSoldItemValueText;

	[SerializeField]
	private TextMeshProUGUI averagePurchaseValueText;

	[SerializeField]
	private TextMeshProUGUI salesProfitValueText;

	[SerializeField]
	private TextMeshProUGUI totalSalesValueText;

	[SerializeField]
	private TextMeshProUGUI revenueValueText;

	[SerializeField]
	private TextMeshProUGUI supplyCostValueText;

	[SerializeField]
	private TextMeshProUGUI upgradeCostValueText;

	[SerializeField]
	private TextMeshProUGUI loanPaymentsValueText;

	[SerializeField]
	private TextMeshProUGUI loanTakenValueText;

	[SerializeField]
	private TextMeshProUGUI totalProfitValueText;

	[SerializeField]
	private TextMeshProUGUI balanceValueText;

	[SerializeField]
	private TextMeshProUGUI dayText;

	[SerializeField]
	private Button startDayButton;

	[SerializeField]
	private PrologueWindow prologueWindow;

	private Color greenColor = new Color(0.29803923f, 35f / 51f, 16f / 51f);

	private Color redColor = new Color(0.95686275f, 0.2627451f, 18f / 85f);

	private Color blueColor = new Color(9f / 85f, 14f / 15f, 1f);

	private void Awake()
	{
		EventManager.AddListener(GameEvents.DAY_ENDED, OnDayEnded);
		startDayButton.onClick.AddListener(OnStartDay);
	}

	private void OnDayEnded()
	{
		Open();
	}

	private void OnStartDay()
	{
		Close();
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
		SingletonBehaviour<TimeManager>.Instance.StartTheNewDay();
		if (SingletonBehaviour<TimeManager>.Instance.CurrentDay == 2 && GameManager.isDemo)
		{
			StartCoroutine(PrologueRoutine());
		}
	}

	private IEnumerator PrologueRoutine()
	{
		yield return new WaitForSeconds(3f);
		prologueWindow.Open();
	}

	private void OnPrologueClosed()
	{
		prologueWindow.Close();
	}

	public override void Open()
	{
		dayText.text = Locale.GetWord("day_n_smallcap").Replace("{0}", SingletonBehaviour<TimeManager>.Instance.CurrentDay.ToString());
		satisfiedCustomerValueText.text = SingletonBehaviour<StatisticsManager>.Instance.SatisfiedCustomers.ToString();
		unavailableProductValueText.text = SingletonBehaviour<StatisticsManager>.Instance.NotFoundProducts.ToString();
		foundExpensiveProductsValueText.text = SingletonBehaviour<StatisticsManager>.Instance.FoundExpensiveProducts.ToString();
		overpaidCustomersValueText.text = SingletonBehaviour<StatisticsManager>.Instance.OverpaidCustomers.ToString();
		storeExperienceValueText.text = "+" + SingletonBehaviour<StatisticsManager>.Instance.StoreExperience;
		mostSoldItemValueText.text = Locale.GetWord(SingletonBehaviour<StatisticsManager>.Instance.MostSoldProduct.ToString());
		totalCustomersValueText.text = SingletonBehaviour<StatisticsManager>.Instance.TotalCustomers.ToString();
		totalSoldProductsValueText.text = SingletonBehaviour<StatisticsManager>.Instance.TotalSoldProducts.ToString();
		RepaintWithDollarValue(averagePurchaseValueText, SingletonBehaviour<StatisticsManager>.Instance.AveragePurchase);
		RepaintWithDollarValue(salesProfitValueText, SingletonBehaviour<StatisticsManager>.Instance.SalesProfit);
		RepaintWithDollarValue(totalSalesValueText, SingletonBehaviour<StatisticsManager>.Instance.TotalSales);
		RepaintWithDollarValue(revenueValueText, SingletonBehaviour<StatisticsManager>.Instance.Revenue);
		RepaintWithDollarValue(supplyCostValueText, 0f - SingletonBehaviour<StatisticsManager>.Instance.SupplyCost);
		RepaintWithDollarValue(upgradeCostValueText, 0f - SingletonBehaviour<StatisticsManager>.Instance.UpgradeCost);
		RepaintWithDollarValue(loanPaymentsValueText, 0f - SingletonBehaviour<StatisticsManager>.Instance.LoanPayments);
		RepaintWithDollarValue(loanTakenValueText, SingletonBehaviour<StatisticsManager>.Instance.LoanTaken);
		RepaintWithDollarValue(totalProfitValueText, SingletonBehaviour<StatisticsManager>.Instance.TotalProfit);
		RepaintWithDollarValue(balanceValueText, SingletonBehaviour<EconomyManager>.Instance.SoftCurrency);
		canvasGroup.alpha = 0f;
		base.Open();
		canvasGroup.DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
	}

	private void RepaintWithDollarValue(TextMeshProUGUI text, float value)
	{
		Color color = ((value > 0f) ? greenColor : ((value < 0f) ? redColor : blueColor));
		string text2 = ((value > 0f) ? "+" : ((value < 0f) ? "-" : ""));
		text.color = color;
		text.text = text2 + "$" + Mathf.Abs(value).ToString("0.00", CultureInfo.InvariantCulture);
	}
}
