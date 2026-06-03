using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleLoanApplicationUI : SelectableUI
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI vehiclePriceText;

	[SerializeField]
	private TextMeshProUGUI upfrontPaymentText;

	[SerializeField]
	private TextMeshProUGUI financedAmountText;

	[SerializeField]
	private VehicleType vehicleType;

	[SerializeField]
	private Slider termSlider;

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
	private TextMeshProUGUI requiredLevelText;

	private static float UPFRONT_PAYMENT_PERCENTAGE = 0.2f;

	public void Initialize()
	{
		titleText.text = Locale.GetWord("vehicle_loan_title_n").Replace("{0}", Locale.GetWord(vehicleType.ToString()));
		vehiclePriceText.text = "$" + SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType).ToString("0.00", CultureInfo.InvariantCulture);
		upfrontPaymentText.text = "$" + ((float)SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType) * UPFRONT_PAYMENT_PERCENTAGE).ToString("0.00", CultureInfo.InvariantCulture);
		financedAmountText.text = "$" + ((float)SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType) * (1f - UPFRONT_PAYMENT_PERCENTAGE)).ToString("0.00", CultureInfo.InvariantCulture);
		termSlider.onValueChanged.AddListener(OnTermChanged);
		requiredLevelText.text = Locale.GetWord("required_warehouse_expansion_n").Replace("{0}", SingletonBehaviour<VehicleManager>.Instance.GetVehicleRequiredLevel(vehicleType).ToString());
		termSlider.wholeNumbers = true;
		termSlider.minValue = 7f;
		termSlider.maxValue = 90f;
		termSlider.value = 7f;
		bool flag = !SingletonBehaviour<VehicleManager>.Instance.IsVehiclePurchased(vehicleType, 0);
		termSlider.interactable = flag;
		takeLoanButton.gameObject.SetActive(flag);
		alreadyTakenLoanUI.SetActive(!flag);
		takeLoanButton.onClick.AddListener(OnTakeLoanButtonClicked);
		RepaintForExpansionLevel(SingletonBehaviour<StorageManager>.Instance.GrowthCount);
		EventManager.AddListener<int>(SupermarketEvents.STORAGE_GROWTH_PURCHASED, RepaintForExpansionLevel);
	}

	public void OnVehicleSold(VehicleType vehicleType)
	{
		if (vehicleType == this.vehicleType)
		{
			UpdateLoanAvailability();
		}
	}

	private void RepaintForExpansionLevel(int expansionLevel)
	{
		if (SingletonBehaviour<VehicleManager>.Instance.IsVehiclePurchased(vehicleType, 0))
		{
			requiredLevelText.enabled = false;
			termSlider.interactable = false;
			takeLoanButton.gameObject.SetActive(value: false);
		}
		else
		{
			bool flag = expansionLevel >= SingletonBehaviour<VehicleManager>.Instance.GetVehicleRequiredLevel(vehicleType);
			requiredLevelText.enabled = !flag;
			termSlider.interactable = flag;
			takeLoanButton.gameObject.SetActive(flag);
		}
	}

	private void OnTakeLoanButtonClicked()
	{
		if (riskValueText.color == Color.red)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("high_risk_error", base.transform);
			return;
		}
		float amount = (float)SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType) * UPFRONT_PAYMENT_PERCENTAGE;
		if (!SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(amount))
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("upfront_payment_not_enough", base.transform);
			return;
		}
		float num = (float)SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType) * (1f - UPFRONT_PAYMENT_PERCENTAGE);
		int term = (int)termSlider.value;
		SingletonBehaviour<FinanceManager>.Instance.TakeVehicleLoan(vehicleType, num, term);
		EventManager.NotifyEvent(PaymentEvents.LOAN_TAKEN, num);
		UpdateLoanAvailability();
	}

	private void OnTermChanged(float value)
	{
		UpdateFields();
	}

	private void UpdateFields()
	{
		int vehiclePrice = SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType);
		int num = (int)termSlider.value;
		float interestRate = SingletonBehaviour<FinanceManager>.Instance.GetInterestRate();
		float num2 = (float)vehiclePrice * (1f + interestRate * (float)num);
		float dailyPayment = num2 / (float)num;
		float num3 = (float)vehiclePrice * interestRate * (float)num;
		(string, Color) riskOfPayment = GetRiskOfPayment(dailyPayment);
		totalRepaymentValueText.text = "$" + num2.ToString("N0");
		dailyPaymentValueText.text = "$" + dailyPayment.ToString("N0");
		financeCostValueText.text = "$" + num3.ToString("N0");
		riskValueText.text = riskOfPayment.Item1;
		riskValueText.color = riskOfPayment.Item2;
		termText.text = num.ToString("N0");
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

	public void UpdateLoanAvailability()
	{
		bool flag = !SingletonBehaviour<VehicleManager>.Instance.IsVehiclePurchased(vehicleType, 0);
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
		if (termSlider.interactable)
		{
			return termSlider;
		}
		return null;
	}

	public override Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		List<Selectable> list = new List<Selectable>();
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
					selectOnDown = ((i < list.Count - 1) ? list[i + 1] : null),
					selectOnLeft = left,
					selectOnRight = right
				};
			}
		}
		return list[0];
	}
}
