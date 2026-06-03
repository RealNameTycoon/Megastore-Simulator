using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoanWindow : TabWindow
{
	[SerializeField]
	private TextMeshProUGUI maxCreditLimitText;

	[SerializeField]
	private TextMeshProUGUI creditScoreText;

	[SerializeField]
	private TextMeshProUGUI dailyPaymentText;

	[SerializeField]
	private LoanApplicationUI loanApplicationUI;

	[SerializeField]
	private List<VehicleLoanApplicationUI> vehicleLoanApplicationUIs;

	[SerializeField]
	private Button creditScoreInfoButton;

	[SerializeField]
	private Canvas creditScoreInfoCanvas;

	[SerializeField]
	private Button closeCreditScoreInfoButton;

	public override void Initialize()
	{
		base.Initialize();
		creditScoreInfoButton.onClick.AddListener(OnCreditScoreInfoButtonClicked);
		closeCreditScoreInfoButton.onClick.AddListener(OnCloseCreditScoreInfoButtonClicked);
		EventManager.AddListener<float>(PaymentEvents.LOAN_TAKEN, OnLoanTaken);
		EventManager.AddListener<VehicleType>(GameEvents.VEHICLE_SOLD, OnVehicleSold);
		loanApplicationUI.Initialize();
		for (int i = 0; i < vehicleLoanApplicationUIs.Count; i++)
		{
			vehicleLoanApplicationUIs[i].Initialize();
		}
		RefreshNavigation();
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshIfOpen);
	}

	private void OnVehicleSold(VehicleType vehicleType)
	{
		for (int i = 0; i < vehicleLoanApplicationUIs.Count; i++)
		{
			vehicleLoanApplicationUIs[i].OnVehicleSold(vehicleType);
		}
		RefreshNavigation();
	}

	private void OnCreditScoreInfoButtonClicked()
	{
		creditScoreInfoCanvas.enabled = true;
		SingletonBehaviour<InputManager>.Instance.SelectElement(closeCreditScoreInfoButton.gameObject);
	}

	public void OnCloseCreditScoreInfoButtonClicked()
	{
		creditScoreInfoCanvas.enabled = false;
		SingletonBehaviour<InputManager>.Instance.SelectElement(GetFirstSelectable().gameObject);
	}

	public void UpdateCreditLimit(float maxCreditLimit)
	{
		maxCreditLimitText.text = maxCreditLimit.ToString("0.00", CultureInfo.InvariantCulture);
		loanApplicationUI.UpdateCreditLimit(maxCreditLimit);
	}

	public void UpdateCreditScore(FinanceManager.CreditScore creditScore)
	{
		creditScoreText.text = creditScore.ToString();
	}

	public void UpdateDailyPayment(string dailyPayment)
	{
		dailyPaymentText.text = dailyPayment;
	}

	public void UpdateLoanAvailability()
	{
		loanApplicationUI.UpdateLoanAvailability();
		RefreshNavigation();
	}

	private void RefreshIfOpen()
	{
		if (IsOpen())
		{
			RefreshNavigation();
		}
	}

	private void OnLoanTaken(float loanAmount)
	{
		Selectable firstSelectable = GetFirstSelectable();
		if (firstSelectable != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(firstSelectable.gameObject);
		}
		else
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(creditScoreInfoButton.gameObject);
		}
		RefreshNavigation();
	}

	private void RefreshNavigation()
	{
		Navigation navigation = creditScoreInfoButton.navigation;
		navigation.selectOnDown = GetFirstSelectableLoan();
		creditScoreInfoButton.navigation = navigation;
		Selectable[] slots = new Selectable[3];
		Selectable firstSelectable = loanApplicationUI.GetFirstSelectable();
		slots[0] = ((firstSelectable != null && firstSelectable.gameObject.activeSelf) ? firstSelectable : null);
		for (int i = 0; i < vehicleLoanApplicationUIs.Count && i < 2; i++)
		{
			if (!(vehicleLoanApplicationUIs[i] == null))
			{
				Selectable firstSelectable2 = vehicleLoanApplicationUIs[i].GetFirstSelectable();
				slots[i + 1] = ((firstSelectable2 != null && firstSelectable2.gameObject.activeSelf) ? firstSelectable2 : null);
			}
		}
		if (slots[0] != null)
		{
			Selectable right = FindRight(0);
			loanApplicationUI.RefreshNavigation(creditScoreInfoButton, null, null, right);
		}
		for (int j = 0; j < vehicleLoanApplicationUIs.Count && j < 2; j++)
		{
			int num = j + 1;
			if (slots[num] != null)
			{
				Selectable left = FindLeft(num);
				Selectable right2 = FindRight(num);
				vehicleLoanApplicationUIs[j].RefreshNavigation(creditScoreInfoButton, null, left, right2);
			}
		}
		Selectable FindLeft(int startIndex)
		{
			for (int num2 = startIndex - 1; num2 >= 0; num2--)
			{
				if (slots[num2] != null)
				{
					return slots[num2];
				}
			}
			return null;
		}
		Selectable FindRight(int startIndex)
		{
			for (int k = startIndex + 1; k < slots.Length; k++)
			{
				if (slots[k] != null)
				{
					return slots[k];
				}
			}
			return null;
		}
	}

	public override Selectable GetFirstSelectable()
	{
		return creditScoreInfoButton;
	}

	private Selectable GetFirstSelectableLoan()
	{
		if (loanApplicationUI.GetFirstSelectable() != null)
		{
			return loanApplicationUI.GetFirstSelectable();
		}
		if (vehicleLoanApplicationUIs[0].GetFirstSelectable() != null)
		{
			return vehicleLoanApplicationUIs[0].GetFirstSelectable();
		}
		if (vehicleLoanApplicationUIs[1].GetFirstSelectable() != null)
		{
			return vehicleLoanApplicationUIs[1].GetFirstSelectable();
		}
		return null;
	}
}
