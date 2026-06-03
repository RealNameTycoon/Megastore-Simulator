using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CashCheckoutWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Transform cashCheckoutTransform;

	[SerializeField]
	private Transform targetPosition;

	[SerializeField]
	private RectTransform cashCheckoutRectTransform;

	[SerializeField]
	private RectTransform targetRect;

	private float requiredChange = -1f;

	private float givenMoney = -1f;

	private float price = -1f;

	private float currentChange;

	private Vector3 initialPosition;

	private string inputString = string.Empty;

	private List<Transform> spawnedMoneys = new List<Transform>();

	private int moneyCount;

	private void Start()
	{
		initialPosition = cashCheckoutTransform.position;
		EventManager.AddListener(SupermarketEvents.FIRE_STARTED, delegate
		{
			if (canvas.enabled)
			{
				Close();
			}
		});
		EventManager.AddListener(ReviewEvents.PAYMENTS_FAILED_REVIEW, delegate
		{
			if (canvas.enabled)
			{
				Close();
			}
		});
	}

	public void Open(float price, float givenMoneyAmount)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.CASH_REGISTER_OPEN);
		requiredChange = givenMoneyAmount - price;
		givenMoney = givenMoneyAmount;
		this.price = price;
		canvas.enabled = true;
		cashCheckoutTransform.DOKill();
		cashCheckoutTransform.DOMove(targetPosition.position, 0.2f).SetEase(Ease.Linear);
	}

	public void Close()
	{
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		ClearMoney();
		requiredChange = -1f;
		price = -1f;
		givenMoney = -1f;
		currentChange = 0f;
		moneyCount = 0;
		cashCheckoutTransform.DOKill();
		cashCheckoutTransform.DOMove(initialPosition, 0.2f).SetEase(Ease.Linear).OnComplete(delegate
		{
			canvas.enabled = false;
		});
	}

	private void Update()
	{
	}

	public void GiveChange(int cent)
	{
	}

	public void OnReset()
	{
		ClearMoney();
		currentChange = 0f;
		moneyCount = 0;
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void ClearMoney()
	{
		spawnedMoneys.Clear();
	}

	public void OnOk()
	{
	}
}
