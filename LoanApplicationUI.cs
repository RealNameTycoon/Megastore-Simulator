using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoanApplicationUI : SelectableUI
{
	[SerializeField]
	private Slider loanAmountSlider;

	[SerializeField]
	private Slider termSlider;

	[SerializeField]
	private TextMeshProUGUI loanAmountText;

	[SerializeField]
	private TextMeshProUGUI termText;

	[SerializeField]
	private TextMeshProUGUI totalRepaymentValueText;

	[SerializeField]
	private TextMeshProUGUI dailyPaymentValueText;

	[SerializeField]
	private TextMeshProUGUI financeCostValueText;

	[SerializeField]
	private TextMeshProUGUI riskValueText;

	[SerializeField]
	private Button takeLoanButton;

	[SerializeField]
	private GameObject alreadyTakenLoanUI;

	[SerializeField]
	private GameObject requiredLevelUI;

	private static float minLoanAmount = 10f;

	private static float maxLoanAmount = 100000f;

	private static int loanAmountStep = 100;

	public void Initialize()
	{
		loanAmountSlider.onValueChanged.AddListener(OnLoanAmountChanged);
		termSlider.onValueChanged.AddListener(OnTermChanged);
		loanAmountSlider.minValue = (int)minLoanAmount;
		loanAmountSlider.wholeNumbers = true;
		termSlider.wholeNumbers = true;
		termSlider.minValue = 7f;
		termSlider.maxValue = 30f;
		loanAmountSlider.value = (int)minLoanAmount;
		termSlider.value = 7f;
		bool flag = SingletonBehaviour<FinanceManager>.Instance.CanTakeBusinessLoan();
		loanAmountSlider.interactable = flag;
		termSlider.interactable = flag;
		takeLoanButton.gameObject.SetActive(flag);
		alreadyTakenLoanUI.SetActive(!flag);
		takeLoanButton.onClick.AddListener(OnTakeLoanButtonClicked);
	}

	private void OnTakeLoanButtonClicked()
	{
		if (riskValueText.color == Color.red)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("high_risk_error", base.transform);
			return;
		}
		int num = (int)loanAmountSlider.value * loanAmountStep;
		int term = (int)termSlider.value;
		SingletonBehaviour<FinanceManager>.Instance.TakeBusinessLoan(num, term);
		string text = Locale.GetWord("loan_approved_money_added").Replace("{0}", num.ToString("N0"));
		SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltipWithFullText(text, base.transform);
		bool flag = SingletonBehaviour<FinanceManager>.Instance.CanTakeBusinessLoan();
		EventManager.NotifyEvent(PaymentEvents.LOAN_TAKEN, (float)num);
		loanAmountSlider.interactable = flag;
		termSlider.interactable = flag;
		takeLoanButton.gameObject.SetActive(flag);
		alreadyTakenLoanUI.SetActive(!flag);
	}

	private void OnLoanAmountChanged(float value)
	{
		UpdateFields();
	}

	private void OnTermChanged(float value)
	{
		UpdateFields();
	}

	private void UpdateFields()
	{
		int num = (int)(loanAmountSlider.value * (float)loanAmountStep);
		int num2 = (int)termSlider.value;
		float interestRate = SingletonBehaviour<FinanceManager>.Instance.GetInterestRate();
		float num3 = (float)num * (1f + interestRate * (float)num2);
		float dailyPayment = num3 / (float)num2;
		float num4 = (float)num * interestRate * (float)num2;
		(string, Color) riskOfPayment = GetRiskOfPayment(dailyPayment);
		totalRepaymentValueText.text = "$" + num3.ToString("N0");
		dailyPaymentValueText.text = "$" + dailyPayment.ToString("N0");
		financeCostValueText.text = "$" + num4.ToString("N0");
		riskValueText.text = riskOfPayment.Item1;
		riskValueText.color = riskOfPayment.Item2;
		termText.text = num2.ToString("N0");
		loanAmountText.text = "$" + num.ToString("N0");
	}

	private (string, Color) GetRiskOfPayment(float dailyPayment)
	{
		float averageProfit = SingletonBehaviour<StatisticsManager>.Instance.GetAverageProfit(5);
		if (Mathf.Approximately(averageProfit, 0f))
		{
			return (Locale.GetWord("unknown_risk"), Color.gray);
		}
		if (dailyPayment > averageProfit * 0.9f)
		{
			return (Locale.GetWord("high_risk"), Color.red);
		}
		if (dailyPayment > averageProfit * 0.5f)
		{
			return (Locale.GetWord("medium_risk"), Color.yellow);
		}
		return (Locale.GetWord("low_risk"), Color.green);
	}

	public void UpdateCreditLimit(float creditLimit)
	{
		loanAmountSlider.maxValue = (int)(creditLimit / (float)loanAmountStep);
	}

	public void UpdateLoanAvailability()
	{
		bool flag = SingletonBehaviour<FinanceManager>.Instance.CanTakeBusinessLoan();
		loanAmountSlider.interactable = flag;
		termSlider.interactable = flag;
		takeLoanButton.gameObject.SetActive(flag);
		alreadyTakenLoanUI.SetActive(!flag);
	}

	public Selectable GetFirstSelectable()
	{
		if (takeLoanButton.gameObject.activeSelf && takeLoanButton.interactable)
		{
			return takeLoanButton;
		}
		if (loanAmountSlider.interactable)
		{
			return loanAmountSlider;
		}
		if (termSlider.interactable)
		{
			return termSlider;
		}
		return null;
	}

	public override Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		List<Selectable> list = new List<Selectable>();
		list.Add(loanAmountSlider);
		list.Add(termSlider);
		list.Add(takeLoanButton);
		for (int i = 0; i < list.Count; i++)
		{
			if (i == 0)
			{
				list[i].navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = up,
					selectOnDown = ((i < list.Count - 1) ? list[i + 1] : null)
				};
			}
			else if (i == list.Count - 1)
			{
				list[i].navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = ((i > 0) ? list[i - 1] : null),
					selectOnDown = down,
					selectOnLeft = left,
					selectOnRight = right
				};
			}
			else
			{
				list[i].navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = ((i > 0) ? list[i - 1] : null),
					selectOnDown = ((i < list.Count - 1) ? list[i + 1] : null)
				};
			}
		}
		return list[0];
	}
}
