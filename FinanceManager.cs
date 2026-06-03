using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using UnityEngine;

public class FinanceManager : SingletonBehaviour<FinanceManager>
{
	public struct CreditInfo
	{
		public CreditType type;

		public VehicleType vehicleType;

		public int dayActivated;

		public float amount;

		public int installmentAmount;

		public float financeFee;

		public float fixedCost;

		public int remainingInstallmentAmount;

		public float DailyPayment()
		{
			return (amount + financeFee + fixedCost) / (float)installmentAmount;
		}
	}

	public enum CreditType
	{
		BUSINESS_LOAN,
		EQUIPMENT_LOAN
	}

	public enum CreditScore
	{
		Poor,
		Risky,
		Fair,
		Good,
		Excellent
	}

	[SerializeField]
	private LoanWindow loanWindow;

	[SerializeField]
	private PaymentsWindow paymentsWindow;

	[SerializeField]
	private UnpaidLoansWindow unpaidLoansWindow;

	[SerializeField]
	private EquipmentsWindow equipmentsWindow;

	private const string ACTIVE_CREDITS_KEY = "ACTIVE_CREDITS_KEY";

	private const string MAX_CREDIT_LIMIT_KEY = "MAX_CREDIT_LIMIT_KEY";

	private const string CREDIT_SCORE_KEY = "CREDIT_SCORE_KEY";

	private List<CreditInfo> activeCredits = new List<CreditInfo>();

	private float creditLimit = 5000f;

	private CreditScore creditScore = CreditScore.Fair;

	private static float CREDIT_LIMIT_PER_LEVEL = 500f;

	private const float MIN_CREDIT_LIMIT = 5000f;

	private const float MAX_CREDIT_LIMIT = 200000f;

	private const float BASE_CREDIT_LIMIT = 5000f;

	private const float PROFIT_MULTIPLIER = 3f;

	private const float ROUND_STEP = 1000f;

	private const int LATE_PAYMENT_PENALTY_DAYS = 3;

	private const float LATE_PAYMENT_PENALTY_RATE = 0.02f;

	private float maxCreditLimit;

	private bool loanPaymentsFailed;

	private static float FIXED_COST_PERCENTAGE = 0.15f;

	public static readonly Dictionary<CreditScore, float> CreditScoreToInterestRateMap = new Dictionary<CreditScore, float>
	{
		{
			CreditScore.Poor,
			0.02f
		},
		{
			CreditScore.Risky,
			0.015f
		},
		{
			CreditScore.Fair,
			0.01f
		},
		{
			CreditScore.Good,
			0.008f
		},
		{
			CreditScore.Excellent,
			0.006f
		}
	};

	public bool LoanPaymentsFailed => loanPaymentsFailed;

	public float GetInterestRate()
	{
		return CreditScoreToInterestRateMap[creditScore];
	}

	private new void Awake()
	{
		base.Awake();
		activeCredits = GenericDataSerializer.Load("ACTIVE_CREDITS_KEY", new List<CreditInfo>());
		maxCreditLimit = GenericDataSerializer.LoadFloat("MAX_CREDIT_LIMIT_KEY", 5000f);
		creditScore = GenericDataSerializer.Load("CREDIT_SCORE_KEY", CreditScore.Fair);
		EventManager.AddListener(EconomyEvents.LEVEL_UP, UpdateCreditLimit);
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
		EventManager.AddListener(SupermarketEvents.EMPLOYEE_PAYMENTS_DONE, TryToPayLoans);
	}

	private void Start()
	{
		UpdateCreditLimit();
		UpdateDailyPayment();
		loanWindow.UpdateCreditLimit(maxCreditLimit);
		loanWindow.UpdateCreditScore(creditScore);
		paymentsWindow.UpdatePayments(activeCredits);
	}

	private void ReduceCreditScore()
	{
		creditScore = (CreditScore)Mathf.Max(0, (int)(creditScore - 1));
		GenericDataSerializer.Save("CREDIT_SCORE_KEY", creditScore);
		loanWindow.UpdateCreditScore(creditScore);
	}

	private void IncreaseCreditScore()
	{
		CreditScore creditScore = this.creditScore;
		this.creditScore = (CreditScore)Mathf.Min(4, (int)(this.creditScore + 1));
		if (this.creditScore > creditScore)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltip("loan_finished_credit_improved", base.transform);
		}
		GenericDataSerializer.Save("CREDIT_SCORE_KEY", this.creditScore);
		loanWindow.UpdateCreditScore(this.creditScore);
	}

	private void OnNewDayStarted()
	{
		UpdateCreditLimit();
		CheckLatePayments();
	}

	private void TryToPayLoans()
	{
		loanPaymentsFailed = false;
		if (activeCredits.Count == 0)
		{
			return;
		}
		int currentDay = SingletonBehaviour<TimeManager>.Instance.CurrentDay;
		bool flag = false;
		int num = 0;
		bool flag2 = true;
		while (flag2)
		{
			flag2 = false;
			for (int num2 = activeCredits.Count - 1; num2 >= 0; num2--)
			{
				CreditInfo creditInfo = activeCredits[num2];
				int num3 = creditInfo.installmentAmount - creditInfo.remainingInstallmentAmount + 1;
				int num4 = creditInfo.dayActivated + num3;
				int num5 = currentDay - num4;
				if (num5 >= 0)
				{
					if (num5 > num)
					{
						num = num5;
					}
					float currentInstallmentPrice = GetCurrentInstallmentPrice(creditInfo);
					if (!SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(currentInstallmentPrice))
					{
						flag2 = false;
						loanPaymentsFailed = true;
						break;
					}
					PayInstallment(num2, currentInstallmentPrice);
					flag = true;
					flag2 = true;
				}
			}
		}
		if (flag)
		{
			GenericDataSerializer.Save("ACTIVE_CREDITS_KEY", activeCredits);
			UpdateDailyPayment();
		}
		paymentsWindow.UpdatePayments(activeCredits);
		if (loanPaymentsFailed)
		{
			if (num >= 3)
			{
				unpaidLoansWindow.OpenWithFullText(Locale.GetWord("unpaid_loans_description_reduced_score_n").Replace("{0}", num.ToString()));
			}
			else if (num == 0)
			{
				unpaidLoansWindow.Open("unpaid_loans_description_warn");
			}
			else if (num < 3)
			{
				unpaidLoansWindow.OpenWithFullText(Locale.GetWord("unpaid_loans_description_warn_n").Replace("{0}", num.ToString()).Replace("{1}", 3.ToString()));
			}
		}
	}

	private void CheckLatePayments()
	{
		int currentDay = SingletonBehaviour<TimeManager>.Instance.CurrentDay;
		for (int i = 0; i < activeCredits.Count; i++)
		{
			CreditInfo creditInfo = activeCredits[i];
			int num = creditInfo.installmentAmount - creditInfo.remainingInstallmentAmount + 1;
			int num2 = creditInfo.dayActivated + num;
			if (currentDay - num2 >= 3)
			{
				ReduceCreditScore();
				break;
			}
		}
	}

	public bool CanTakeBusinessLoan()
	{
		for (int i = 0; i < activeCredits.Count; i++)
		{
			if (activeCredits[i].type == CreditType.BUSINESS_LOAN)
			{
				return false;
			}
		}
		return true;
	}

	public float DailyPayment()
	{
		float num = 0f;
		for (int i = 0; i < activeCredits.Count; i++)
		{
			num += activeCredits[i].DailyPayment();
		}
		return num;
	}

	private void UpdateDailyPayment()
	{
		loanWindow.UpdateDailyPayment(DailyPayment().ToString("0.00", CultureInfo.InvariantCulture));
	}

	private void UpdateCreditLimit()
	{
		int num = Mathf.Max(1, SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		float num2 = Mathf.Max(0f, SingletonBehaviour<StatisticsManager>.Instance.GetAverageProfit(5));
		float num3 = 5000f + (float)num * CREDIT_LIMIT_PER_LEVEL + num2 * 3f;
		num3 *= GetCreditScoreMultiplier(creditScore);
		num3 = Mathf.Clamp(num3, 5000f, 200000f);
		num3 = RoundToNearest(num3, 1000f);
		creditLimit = Mathf.Max(maxCreditLimit, num3);
		if (creditLimit > maxCreditLimit)
		{
			maxCreditLimit = creditLimit;
			GenericDataSerializer.SaveFloat("MAX_CREDIT_LIMIT_KEY", maxCreditLimit);
		}
		creditLimit = Mathf.Clamp(creditLimit, 5000f, 200000f);
		loanWindow.UpdateCreditLimit(creditLimit);
	}

	public void TakeBusinessLoan(int loanAmount, int term)
	{
		float interestRate = GetInterestRate();
		_ = (float)loanAmount * (1f + interestRate * (float)term) / (float)term;
		float num = (float)loanAmount * interestRate * (float)term;
		CreditInfo item = new CreditInfo
		{
			type = CreditType.BUSINESS_LOAN,
			dayActivated = SingletonBehaviour<TimeManager>.Instance.CurrentDay,
			amount = loanAmount,
			installmentAmount = term,
			financeFee = num * (1f - FIXED_COST_PERCENTAGE),
			fixedCost = num * FIXED_COST_PERCENTAGE,
			remainingInstallmentAmount = term
		};
		activeCredits.Add(item);
		GenericDataSerializer.Save("ACTIVE_CREDITS_KEY", activeCredits);
		paymentsWindow.UpdatePayments(activeCredits);
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, (float)loanAmount);
	}

	public bool HasVehicleLoan(VehicleType vehicleType)
	{
		for (int i = 0; i < activeCredits.Count; i++)
		{
			if (activeCredits[i].type == CreditType.EQUIPMENT_LOAN && activeCredits[i].vehicleType == vehicleType)
			{
				return true;
			}
		}
		return false;
	}

	public void TakeVehicleLoan(VehicleType vehicleType, float loanAmount, int term)
	{
		float interestRate = GetInterestRate();
		_ = loanAmount * (1f + interestRate * (float)term) / (float)term;
		float num = loanAmount * interestRate * (float)term;
		CreditInfo item = new CreditInfo
		{
			type = CreditType.EQUIPMENT_LOAN,
			vehicleType = vehicleType,
			dayActivated = SingletonBehaviour<TimeManager>.Instance.CurrentDay,
			amount = loanAmount,
			installmentAmount = term,
			financeFee = num * (1f - FIXED_COST_PERCENTAGE),
			fixedCost = num * FIXED_COST_PERCENTAGE,
			remainingInstallmentAmount = term
		};
		activeCredits.Add(item);
		GenericDataSerializer.Save("ACTIVE_CREDITS_KEY", activeCredits);
		paymentsWindow.UpdatePayments(activeCredits);
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, loanAmount);
		equipmentsWindow.PurchaseVehicle(vehicleType);
	}

	private float GetCreditScoreMultiplier(CreditScore creditScore)
	{
		return creditScore switch
		{
			CreditScore.Poor => 0.6f, 
			CreditScore.Risky => 0.8f, 
			CreditScore.Fair => 0.9f, 
			CreditScore.Good => 1f, 
			CreditScore.Excellent => 1.15f, 
			_ => 1f, 
		};
	}

	private float RoundToNearest(float value, float step)
	{
		return Mathf.Round(value / step) * step;
	}

	private bool PayInstallment(int index, float installmentPrice)
	{
		CreditInfo value = activeCredits[index];
		EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, installmentPrice);
		EventManager.NotifyEvent(PaymentEvents.LOAN_PAYMENT_DONE, installmentPrice);
		value.remainingInstallmentAmount--;
		if (value.remainingInstallmentAmount <= 0)
		{
			IncreaseCreditScore();
			bool num = value.type == CreditType.BUSINESS_LOAN;
			activeCredits.RemoveAt(index);
			if (num)
			{
				loanWindow.UpdateLoanAvailability();
			}
			return true;
		}
		activeCredits[index] = value;
		return false;
	}

	public void PayCredit(int index, Transform errorTransform = null)
	{
		CreditInfo creditInfo = activeCredits[index];
		float currentInstallmentPrice = GetCurrentInstallmentPrice(creditInfo);
		if (!SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(currentInstallmentPrice))
		{
			if (errorTransform != null)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_not_enough_cash_payment", errorTransform);
			}
		}
		else
		{
			PayInstallment(index, currentInstallmentPrice);
			GenericDataSerializer.Save("ACTIVE_CREDITS_KEY", activeCredits);
			paymentsWindow.UpdatePayments(activeCredits);
			UpdateDailyPayment();
		}
	}

	public float GetPayoffAmount(int index)
	{
		CreditInfo creditInfo = activeCredits[index];
		float num = 0f;
		int num2 = creditInfo.installmentAmount - creditInfo.remainingInstallmentAmount;
		for (int i = 0; i < creditInfo.remainingInstallmentAmount; i++)
		{
			int installmentNumber = num2 + i + 1;
			num += CalculateInstallmentPrice(creditInfo, installmentNumber);
		}
		return num;
	}

	public void PayOffCredit(int index, Transform errorTransform = null)
	{
		CreditInfo creditInfo = activeCredits[index];
		float payoffAmount = GetPayoffAmount(index);
		if (!SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(payoffAmount))
		{
			if (errorTransform != null)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_not_enough_cash_payoff", errorTransform);
			}
			return;
		}
		EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, payoffAmount);
		IncreaseCreditScore();
		bool num = creditInfo.type == CreditType.BUSINESS_LOAN;
		activeCredits.RemoveAt(index);
		if (num)
		{
			loanWindow.UpdateLoanAvailability();
		}
		GenericDataSerializer.Save("ACTIVE_CREDITS_KEY", activeCredits);
		paymentsWindow.UpdatePayments(activeCredits);
		UpdateDailyPayment();
		EventManager.NotifyEvent(PaymentEvents.LOAN_PAYMENT_DONE, payoffAmount);
	}

	public float GetCurrentInstallmentPrice(CreditInfo creditInfo)
	{
		int installmentNumber = creditInfo.installmentAmount - creditInfo.remainingInstallmentAmount + 1;
		return CalculateInstallmentPrice(creditInfo, installmentNumber);
	}

	private float CalculateInstallmentPrice(CreditInfo creditInfo, int installmentNumber)
	{
		int currentDay = SingletonBehaviour<TimeManager>.Instance.CurrentDay;
		int num = creditInfo.dayActivated + installmentNumber;
		float num2 = creditInfo.amount / (float)creditInfo.installmentAmount;
		float num3 = creditInfo.financeFee / (float)creditInfo.installmentAmount;
		float num4 = creditInfo.fixedCost / (float)creditInfo.installmentAmount;
		int num5 = currentDay - num;
		if (num5 > 0)
		{
			float num6 = num2 * 0.02f * (float)num5;
			return num2 + num3 + num6 + num4;
		}
		if (num5 < 0)
		{
			int num7 = Mathf.Abs(num5);
			float num8 = num3 / (float)creditInfo.installmentAmount * (float)num7;
			float num9 = Mathf.Max(0f, num3 - num8);
			return num2 + num9 + num4;
		}
		return num2 + num3 + num4;
	}
}
