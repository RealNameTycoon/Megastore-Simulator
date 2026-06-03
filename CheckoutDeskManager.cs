using System.Collections.Generic;
using UnityEngine;

public class CheckoutDeskManager : SingletonBehaviour<CheckoutDeskManager>
{
	[SerializeField]
	private List<CheckoutManager> checkoutDesks = new List<CheckoutManager>();

	[SerializeField]
	private List<CheckoutManager> selfCheckouts = new List<CheckoutManager>();

	private int checkedOutCustomerCount;

	private const string CHECKED_OUT_CUSTOMER_KEY = "CHECKED_OUT_CUSTOMER_KEY";

	public const int CAPACITY_PER_DESK = 7;

	public const int CAPACITY_PER_SELF_CHECKOUT = 4;

	public List<CheckoutManager> CheckoutDesks => checkoutDesks;

	public int CheckedOutCustomerCount => checkedOutCustomerCount;

	private new void Awake()
	{
		base.Awake();
		checkedOutCustomerCount = (checkedOutCustomerCount = GenericDataSerializer.LoadInt("CHECKED_OUT_CUSTOMER_KEY"));
		EventManager.AddListener<float>(PaymentEvents.POS_PAYMENT_DONE, OnPaymentFinished);
		EventManager.AddListener<float>(PaymentEvents.CASH_PAYMENT_DONE, OnPaymentFinished);
	}

	public void AddNewCheckoutManager(CheckoutManager checkoutManager)
	{
		if (checkoutManager.Type == FurnitureType.CHECKOUT_DESK)
		{
			checkoutDesks.Add(checkoutManager);
			checkoutDesks.Sort((CheckoutManager a, CheckoutManager b) => a.GetDisplayedID().CompareTo(b.GetDisplayedID()));
		}
		else if (checkoutManager.Type == FurnitureType.SELF_CHECKOUT)
		{
			selfCheckouts.Add(checkoutManager);
			selfCheckouts.Sort((CheckoutManager a, CheckoutManager b) => a.GetDisplayedID().CompareTo(b.GetDisplayedID()));
		}
		EventManager.NotifyEvent(SupermarketEvents.CUSTOMER_CAPACITY_CHANGED);
	}

	public void RemoveCheckoutManager(CheckoutManager checkoutManager)
	{
		if (checkoutManager.Type == FurnitureType.CHECKOUT_DESK)
		{
			checkoutDesks.Remove(checkoutManager);
		}
		else if (checkoutManager.Type == FurnitureType.SELF_CHECKOUT)
		{
			selfCheckouts.Remove(checkoutManager);
		}
		EventManager.NotifyEvent(SupermarketEvents.CUSTOMER_CAPACITY_CHANGED);
	}

	private void OnPaymentFinished(float amount)
	{
		checkedOutCustomerCount++;
		GenericDataSerializer.SaveInt("CHECKED_OUT_CUSTOMER_KEY", checkedOutCustomerCount);
		EventLogger.LogEvent("c_payment_done_" + checkedOutCustomerCount);
		EventLogger.LogCheckout(checkedOutCustomerCount);
	}

	public (int, bool) GetAvailableCheckoutManager(Transform customerTransform)
	{
		List<int> list = new List<int>();
		int num = int.MaxValue;
		for (int i = 0; i < checkoutDesks.Count; i++)
		{
			if (checkoutDesks[i].gameObject.activeSelf && checkoutDesks[i].HasCapacity() && !checkoutDesks[i].IsClosed())
			{
				if (checkoutDesks[i].GetQueueLength() < num)
				{
					list.Clear();
					list.Add(i);
					num = checkoutDesks[i].GetQueueLength();
				}
				else if (checkoutDesks[i].GetQueueLength() == num)
				{
					list.Add(i);
				}
			}
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < selfCheckouts.Count; j++)
		{
			if (selfCheckouts[j].gameObject.activeSelf && selfCheckouts[j].HasCapacity() && !selfCheckouts[j].IsClosed())
			{
				if (selfCheckouts[j].GetQueueLength() < num)
				{
					list2.Clear();
					list2.Add(j);
					num = selfCheckouts[j].GetQueueLength();
					list.Clear();
				}
				else if (selfCheckouts[j].GetQueueLength() == num)
				{
					list2.Add(j);
				}
			}
		}
		float num2 = float.MaxValue;
		CheckoutManager checkoutManager = null;
		for (int k = 0; k < list.Count; k++)
		{
			float num3 = Vector3.Distance(customerTransform.position, checkoutDesks[list[k]].transform.position);
			if (num3 < num2)
			{
				num2 = num3;
				checkoutManager = checkoutDesks[list[k]];
			}
		}
		for (int l = 0; l < list2.Count; l++)
		{
			float num4 = Vector3.Distance(customerTransform.position, selfCheckouts[list2[l]].transform.position);
			if (num4 < num2)
			{
				num2 = num4;
				checkoutManager = selfCheckouts[list2[l]];
			}
		}
		return (checkoutManager.FurnitureID, checkoutManager.Type == FurnitureType.SELF_CHECKOUT);
	}

	public int GetCustomerCapacity()
	{
		int num = 0;
		for (int i = 0; i < checkoutDesks.Count; i++)
		{
			if (checkoutDesks[i].gameObject.activeSelf && !checkoutDesks[i].IsClosed())
			{
				num += 7;
			}
		}
		for (int j = 0; j < selfCheckouts.Count; j++)
		{
			if (selfCheckouts[j].gameObject.activeSelf && !selfCheckouts[j].IsClosed())
			{
				num += 4;
			}
		}
		return num;
	}

	public int GetOpenCheckoutCount()
	{
		int num = 0;
		for (int i = 0; i < checkoutDesks.Count; i++)
		{
			if (checkoutDesks[i].gameObject.activeSelf && !checkoutDesks[i].IsClosed())
			{
				num++;
			}
		}
		for (int j = 0; j < selfCheckouts.Count; j++)
		{
			if (selfCheckouts[j].gameObject.activeSelf && !selfCheckouts[j].IsClosed())
			{
				num++;
			}
		}
		return num;
	}

	public CheckoutManager GetCheckoutDesk(int furnitureID)
	{
		for (int i = 0; i < checkoutDesks.Count; i++)
		{
			if (checkoutDesks[i].FurnitureID == furnitureID)
			{
				return checkoutDesks[i];
			}
		}
		return null;
	}

	public CheckoutManager GetSelfCheckout(int furnitureID)
	{
		for (int i = 0; i < selfCheckouts.Count; i++)
		{
			if (selfCheckouts[i].FurnitureID == furnitureID)
			{
				return selfCheckouts[i];
			}
		}
		return null;
	}

	public CheckoutManager GetCheckoutManager(int furnitureID, bool isSelfCheckout)
	{
		if (isSelfCheckout)
		{
			return GetSelfCheckout(furnitureID);
		}
		return GetCheckoutDesk(furnitureID);
	}

	public int GetLowestAvailableDisplayedID(FurnitureType type)
	{
		int num = int.MaxValue;
		switch (type)
		{
		case FurnitureType.CHECKOUT_DESK:
		{
			for (int j = 0; j < checkoutDesks.Count; j++)
			{
				if (checkoutDesks[j].GetDisplayedID() != j + 1)
				{
					num = j + 1;
					break;
				}
			}
			if (num == int.MaxValue)
			{
				num = checkoutDesks.Count + 1;
			}
			break;
		}
		case FurnitureType.SELF_CHECKOUT:
		{
			for (int i = 0; i < selfCheckouts.Count; i++)
			{
				if (selfCheckouts[i].GetDisplayedID() != i + 1)
				{
					num = i + 1;
					break;
				}
			}
			if (num == int.MaxValue)
			{
				num = selfCheckouts.Count + 1;
			}
			break;
		}
		}
		if (num > 99)
		{
			return 99;
		}
		return num;
	}

	public void CheckoutStatusChanged()
	{
		if (SingletonBehaviour<CheckoutDeskManager>.Instance.GetCustomerCapacity() == 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("no_checkout_no_customer", base.transform);
		}
		else if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}
}
