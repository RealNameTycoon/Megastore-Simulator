using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaymentUI : SelectableUI
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI paymentAmountText;

	[SerializeField]
	private TextMeshProUGUI principalText;

	[SerializeField]
	private TextMeshProUGUI dueText;

	[SerializeField]
	private TextMeshProUGUI financeFeeText;

	[SerializeField]
	private TextMeshProUGUI paymentText;

	[SerializeField]
	private TextMeshProUGUI remainingPaymentsText;

	[SerializeField]
	private Button payButton;

	[SerializeField]
	private Button payOffButton;

	[SerializeField]
	private TextMeshProUGUI payButtonText;

	[SerializeField]
	private TextMeshProUGUI payOffButtonText;

	private int index;

	public void Repaint(FinanceManager.CreditInfo creditInfo, int index)
	{
		this.index = index;
		if (creditInfo.type == FinanceManager.CreditType.EQUIPMENT_LOAN)
		{
			title.text = Locale.GetWord("vehicle_loan_title_n").Replace("{0}", Locale.GetWord(creditInfo.vehicleType.ToString()));
		}
		else
		{
			title.text = Locale.GetWord(creditInfo.type.ToString());
		}
		float num = creditInfo.DailyPayment();
		paymentAmountText.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
		paymentText.text = creditInfo.installmentAmount - creditInfo.remainingInstallmentAmount + 1 + "/" + creditInfo.installmentAmount;
		float num2 = creditInfo.amount / (float)creditInfo.installmentAmount;
		principalText.text = "$" + num2.ToString("0.00", CultureInfo.InvariantCulture);
		remainingPaymentsText.text = ((float)creditInfo.remainingInstallmentAmount * creditInfo.DailyPayment()).ToString("0.00", CultureInfo.InvariantCulture);
		int num3 = creditInfo.dayActivated + creditInfo.installmentAmount - creditInfo.remainingInstallmentAmount + 1;
		dueText.text = Locale.GetWord("day_n_camel").Replace("{0}", num3.ToString());
		if (num3 < SingletonBehaviour<TimeManager>.Instance.CurrentDay)
		{
			dueText.color = UIManager.RedColor;
		}
		else if (num3 == SingletonBehaviour<TimeManager>.Instance.CurrentDay)
		{
			dueText.color = UIManager.WarningYellow;
		}
		else
		{
			dueText.color = UIManager.LightBlue;
		}
		float num4 = (creditInfo.financeFee + creditInfo.fixedCost) / (float)creditInfo.installmentAmount;
		financeFeeText.text = "$" + num4.ToString("0.00", CultureInfo.InvariantCulture);
		float currentInstallmentPrice = SingletonBehaviour<FinanceManager>.Instance.GetCurrentInstallmentPrice(creditInfo);
		if (currentInstallmentPrice < num)
		{
			payButtonText.text = Locale.GetWord("pay_early_n").Replace("{0}", "$" + currentInstallmentPrice.ToString("0.00", CultureInfo.InvariantCulture));
		}
		else
		{
			payButtonText.text = Locale.GetWord("pay_n").Replace("{0}", "$" + currentInstallmentPrice.ToString("0.00", CultureInfo.InvariantCulture));
		}
		float payoffAmount = SingletonBehaviour<FinanceManager>.Instance.GetPayoffAmount(index);
		payOffButtonText.text = Locale.GetWord("pay_off_n").Replace("{0}", "$" + payoffAmount.ToString("0.00", CultureInfo.InvariantCulture));
		payButton.onClick.RemoveAllListeners();
		payOffButton.onClick.RemoveAllListeners();
		payButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<FinanceManager>.Instance.PayCredit(index, base.transform);
		});
		payOffButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<FinanceManager>.Instance.PayOffCredit(index, base.transform);
		});
	}

	public Selectable GetFirstSelectable()
	{
		if (payButton.interactable)
		{
			return payButton;
		}
		if (payOffButton.interactable)
		{
			return payOffButton;
		}
		return null;
	}

	public override Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		Navigation navigation = payOffButton.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnLeft = left;
		navigation.selectOnRight = payButton;
		navigation.selectOnUp = up;
		navigation.selectOnDown = down;
		payOffButton.navigation = navigation;
		Navigation navigation2 = payButton.navigation;
		navigation2.mode = Navigation.Mode.Explicit;
		navigation2.selectOnLeft = payOffButton;
		navigation2.selectOnRight = right;
		navigation2.selectOnUp = up;
		navigation2.selectOnDown = down;
		payButton.navigation = navigation2;
		return payButton;
	}
}
