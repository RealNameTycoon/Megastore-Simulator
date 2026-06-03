using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CashRegister : MonoBehaviour
{
	[SerializeField]
	private CheckoutManager checkoutManager;

	[SerializeField]
	private Transform cashDraw;

	[SerializeField]
	private Transform cashDrawTarget;

	[SerializeField]
	private List<GameObject> drawContent;

	[SerializeField]
	private List<Collider> clickableCollider;

	[SerializeField]
	private MoneyPool moneyPool;

	[SerializeField]
	private MonitorCheckoutWindow monitorCheckoutWindow;

	private Vector3 cashDrawInitialPosition;

	private float requiredChange = -1f;

	private float givenMoney = -1f;

	private float price = -1f;

	private float currentChange;

	private string inputString = string.Empty;

	private List<Transform> spawnedMoneys = new List<Transform>();

	private int moneyCount;

	private bool isOpen;

	private int paperMoneyAmount;

	public bool IsOpen => isOpen;

	private void Start()
	{
		cashDrawInitialPosition = cashDraw.localPosition;
		EventManager.AddListener(SupermarketEvents.FIRE_STARTED, delegate
		{
		});
	}

	public void EnableColliders(bool state)
	{
		for (int i = 0; i < clickableCollider.Count; i++)
		{
			clickableCollider[i].enabled = state;
		}
	}

	public void Open(float price, float givenMoneyAmount)
	{
		checkoutManager.PlayCashRegisterAudio(AudioManager.AudioTypes.CASH_REGISTER_OPEN);
		requiredChange = givenMoneyAmount - price;
		givenMoney = givenMoneyAmount;
		this.price = price;
		EnableColliders(state: true);
		EnableDrawContent(state: true);
		cashDraw.DOKill();
		cashDraw.DOMove(cashDrawTarget.position, 0.3f).SetEase(Ease.Linear);
		isOpen = true;
	}

	private void EnableDrawContent(bool state)
	{
		for (int i = 0; i < drawContent.Count; i++)
		{
			drawContent[i].SetActive(state);
		}
	}

	public void Close()
	{
		ClearMoney();
		requiredChange = -1f;
		price = -1f;
		givenMoney = -1f;
		currentChange = 0f;
		moneyCount = 0;
		paperMoneyAmount = 0;
		cashDraw.DOKill();
		EnableColliders(state: false);
		cashDraw.DOLocalMove(cashDrawInitialPosition, 0.3f).SetEase(Ease.Linear).OnComplete(delegate
		{
			EnableDrawContent(state: false);
		});
		isOpen = false;
	}

	private void Update()
	{
	}

	public void GiveChange(int cent)
	{
		if (currentChange + (float)cent / 100f > 100f)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_above_100", base.transform);
			return;
		}
		if (cent < 100)
		{
			checkoutManager.PlayCashRegisterAudio(AudioManager.AudioTypes.GIVE_COIN_1);
		}
		else
		{
			paperMoneyAmount += cent;
			int num = 2;
			int num2 = Random.Range(0, 2);
			checkoutManager.PlayCashRegisterAudio((AudioManager.AudioTypes)(num + num2));
		}
		Transform money = moneyPool.GetMoney((MoneyType)cent);
		spawnedMoneys.Add(money);
		Vector3 vector = checkoutManager.GetRandomMoneyPosition() + Vector3.up * moneyCount / 5000f;
		Vector3 topPosition = vector + Vector3.up * 0.3f;
		money.transform.DoCurvedMove(vector, 0.4f, topPosition);
		money.DOLocalRotate(new Vector3(0f, Random.Range(0f, 360f), 0f), 0.2f);
		moneyCount++;
		currentChange += (float)cent / 100f;
		monitorCheckoutWindow.SetGivenCash(currentChange);
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	public void GetChangeBack(int cent)
	{
		Transform moneyToGetBack = moneyPool.GetLastUsedMoney((MoneyType)cent);
		if (!(moneyToGetBack == null))
		{
			if (cent < 100)
			{
				checkoutManager.PlayCashRegisterAudio(AudioManager.AudioTypes.GIVE_COIN_1);
			}
			else
			{
				paperMoneyAmount -= cent;
				int num = 2;
				int num2 = Random.Range(0, 2);
				checkoutManager.PlayCashRegisterAudio((AudioManager.AudioTypes)(num + num2));
			}
			Transform poolTransform = moneyPool.GetPoolTransform((MoneyType)cent);
			Vector3 topPosition = poolTransform.position + Vector3.up * 0.5f;
			moneyToGetBack.DoCurvedMove(poolTransform.position, 0.4f, topPosition).OnComplete(delegate
			{
				moneyPool.PutBackToPool(moneyToGetBack);
				spawnedMoneys.Remove(moneyToGetBack);
			});
			moneyToGetBack.DOLocalRotate(poolTransform.localEulerAngles, 0.2f);
			moneyCount--;
			currentChange -= (float)cent / 100f;
			monitorCheckoutWindow.SetGivenCash(currentChange);
			if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
			{
				SingletonBehaviour<TooltipUI>.Instance.Close();
			}
		}
	}

	public void OnReset()
	{
		ClearMoney();
		currentChange = 0f;
		monitorCheckoutWindow.SetGivenCash(currentChange);
		moneyCount = 0;
		paperMoneyAmount = 0;
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void ClearMoney()
	{
		for (int i = 0; i < spawnedMoneys.Count; i++)
		{
			moneyPool.PutBackToPool(spawnedMoneys[i]);
		}
		spawnedMoneys.Clear();
	}

	public void OnOk()
	{
		if (currentChange >= requiredChange || (double)Mathf.Abs(currentChange - requiredChange) < 0.005)
		{
			if (givenMoney - currentChange > 0f)
			{
				EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, (givenMoney - currentChange) * checkoutManager.BoostMultiplier());
			}
			else
			{
				if (currentChange - givenMoney > 15f)
				{
					SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("change_too_much", base.transform);
					checkoutManager.LastCustomerNotEnoughCash();
					return;
				}
				EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, givenMoney - currentChange);
			}
			if (paperMoneyAmount == 0 && currentChange > 10f)
			{
				EventLogger.PennyChanges10();
			}
			float num = givenMoney - currentChange;
			EventManager.NotifyEvent(PaymentEvents.CASH_PAYMENT_DONE, num);
			checkoutManager.OnPaymentFinished(num);
			checkoutManager.PlayCashRegisterAudio(AudioManager.AudioTypes.PAYMENT_DONE);
			Close();
			if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
			{
				SingletonBehaviour<TooltipUI>.Instance.Close();
			}
			checkoutManager.RepaintButtonsForCheckout();
		}
		else
		{
			checkoutManager.LastCustomerNotEnoughCash();
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_change", base.transform);
		}
	}
}
