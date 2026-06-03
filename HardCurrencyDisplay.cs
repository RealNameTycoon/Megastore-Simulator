using TMPro;
using UnityEngine;

public class HardCurrencyDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI moneyText;

	private void Start()
	{
		EventManager.AddListener<int>(EconomyEvents.ADD_HARD_CURRENCY, UpdateMoney);
		EventManager.AddListener<int>(EconomyEvents.REMOVE_HARD_CURRENCY, UpdateMoney);
		UpdateMoney(-1);
	}

	private void UpdateMoney(int amount)
	{
		moneyText.text = SingletonBehaviour<EconomyManager>.Instance.HardCurrency.ToString();
	}
}
