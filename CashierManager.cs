using UnityEngine;

public class CashierManager : SingletonBehaviour<CashierManager>
{
	[SerializeField]
	private Cashier cashier;

	[SerializeField]
	private CashierUI cashierUI;
}
