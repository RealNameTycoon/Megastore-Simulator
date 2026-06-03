using TMPro;
using UnityEngine;

public class CashierUI : ServiceUI
{
	[SerializeField]
	private TextMeshProUGUI remainingTimeText;

	public void UpdateTime(string time)
	{
		if (remainingTimeText.enabled)
		{
			remainingTimeText.text = time;
		}
	}

	public void SetPurchasable(bool purchasable)
	{
		remainingTimeText.enabled = !purchasable;
		priceText.enabled = purchasable;
		purchase.gameObject.SetActive(purchasable);
	}

	public void OnPurchase()
	{
	}

	private void UpdateUI()
	{
	}

	protected override bool CanPurchase()
	{
		return true;
	}
}
