using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaymentsWindow : TabWindow
{
	[SerializeField]
	private List<PaymentUI> paymentUIs;

	[SerializeField]
	private Button paymentsTabButton;

	private const int ITEM_PER_ROW = 3;

	public override void Initialize()
	{
		base.Initialize();
		EventManager.AddListener<float>(PaymentEvents.LOAN_PAYMENT_DONE, OnLoanPaymentDone);
		RefreshNavigation();
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshIfOpen);
	}

	public void UpdatePayments(List<FinanceManager.CreditInfo> activeCredits)
	{
		for (int i = 0; i < paymentUIs.Count; i++)
		{
			if (i < activeCredits.Count)
			{
				paymentUIs[i].Repaint(activeCredits[i], i);
				paymentUIs[i].gameObject.SetActive(value: true);
			}
			else
			{
				paymentUIs[i].gameObject.SetActive(value: false);
			}
		}
		RefreshNavigation();
	}

	private void OnLoanPaymentDone(float payoffAmount)
	{
		RefreshNavigation();
		Selectable firstSelectable = GetFirstSelectable();
		if (firstSelectable != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(firstSelectable.gameObject);
		}
		else
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(paymentsTabButton.gameObject);
		}
	}

	private void RefreshNavigation()
	{
		if (paymentsTabButton == null || paymentUIs == null)
		{
			return;
		}
		Selectable[] slots = new Selectable[paymentUIs.Count];
		for (int i = 0; i < paymentUIs.Count; i++)
		{
			PaymentUI paymentUI = paymentUIs[i];
			if (paymentUI == null || !paymentUI.gameObject.activeSelf)
			{
				slots[i] = null;
				continue;
			}
			Selectable firstSelectable = paymentUI.GetFirstSelectable();
			if (firstSelectable == null || !firstSelectable.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = firstSelectable;
			}
		}
		for (int j = 0; j < paymentUIs.Count; j++)
		{
			PaymentUI paymentUI2 = paymentUIs[j];
			if (!(paymentUI2 == null) && paymentUI2.gameObject.activeSelf && !(slots[j] == null))
			{
				int rowLock = j / 3;
				Selectable up = FindNext(j, -3, -1) ?? paymentsTabButton;
				Selectable down = FindNext(j, 3, -1);
				Selectable left = FindNext(j, -1, rowLock);
				Selectable right = FindNext(j, 1, rowLock);
				paymentUI2.RefreshNavigation(up, down, left, right);
			}
		}
		Selectable FindNext(int startIndex, int step, int num)
		{
			for (int k = startIndex + step; k >= 0 && k < slots.Length && (num == -1 || k / 3 == num); k += step)
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
		for (int i = 0; i < paymentUIs.Count; i++)
		{
			if (paymentUIs[i].gameObject.activeSelf && paymentUIs[i].GetFirstSelectable() != null)
			{
				return paymentUIs[i].GetFirstSelectable();
			}
		}
		return null;
	}

	private void RefreshIfOpen()
	{
		if (IsOpen())
		{
			RefreshNavigation();
		}
	}
}
