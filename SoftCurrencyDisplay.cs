using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SoftCurrencyDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI moneyText;

	[SerializeField]
	private TextMeshProUGUI moneyHighlightText;

	public Color moneyHighlightColor;

	public Color moneyReduceColor;

	private const string dollarSign = "<size=110%><color=#14ED00>$</color><size=110%>";

	private void Start()
	{
		moneyHighlightColor = moneyHighlightText.color;
		EventManager.AddListener(EconomyEvents.ADD_SOFT_CURRENCY, delegate(float amount)
		{
			UpdateMoney(amount, remove: false);
		});
		EventManager.AddListener(EconomyEvents.REMOVE_SOFT_CURRENCY, delegate(float amount)
		{
			UpdateMoney(amount, remove: true);
		});
		moneyText.text = "<size=110%><color=#14ED00>$</color><size=110%>" + SingletonBehaviour<EconomyManager>.Instance.SoftCurrency.ToString("0.00", CultureInfo.InvariantCulture);
	}

	private void UpdateMoney(float amount, bool remove)
	{
		if (!remove && amount >= 0f)
		{
			moneyHighlightText.text = "+" + amount.ToString("0.00", CultureInfo.InvariantCulture);
			moneyHighlightText.color = moneyHighlightColor;
			moneyHighlightText.transform.DOScale(1f, 0.4f).OnComplete(delegate
			{
				moneyHighlightText.DOFade(0f, 0.4f).SetDelay(0.4f).OnComplete(delegate
				{
					moneyHighlightText.transform.localScale = Vector3.zero;
				});
			});
		}
		else if (remove || amount < 0f)
		{
			moneyHighlightText.text = amount.ToString("0.00", CultureInfo.InvariantCulture);
			moneyHighlightText.color = moneyReduceColor;
			moneyHighlightText.transform.DOScale(1f, 0.4f).OnComplete(delegate
			{
				moneyHighlightText.DOFade(0f, 0.4f).SetDelay(0.4f).OnComplete(delegate
				{
					moneyHighlightText.transform.localScale = Vector3.zero;
				});
			});
		}
		moneyText.text = "<size=110%><color=#14ED00>$</color><size=110%>" + SingletonBehaviour<EconomyManager>.Instance.SoftCurrency.ToString("0.00", CultureInfo.InvariantCulture);
	}
}
