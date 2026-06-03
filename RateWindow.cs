using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private List<Button> stars;

	[SerializeField]
	private Image[] filledStarImages;

	[SerializeField]
	private Button sendButton;

	[SerializeField]
	private Button closeButton;

	private const string RATED_3_PLUS_KEY = "RATED_3_PLUS_KEY";

	private const string RATING_SHOWN_1 = "RATING_SHOWN_1";

	private const string RATING_SHOWN_2 = "RATING_SHOWN_2";

	private const string RATING_SHOWN_3 = "RATING_SHOWN_3";

	private int currentRate;

	private void Start()
	{
		sendButton.onClick.AddListener(OnSend);
		closeButton.onClick.AddListener(Close);
		for (int i = 0; i < stars.Count; i++)
		{
			int index = i;
			stars[i].onClick.AddListener(delegate
			{
				OnStarClicked(index);
			});
		}
		EventManager.AddListener<float>(PaymentEvents.CASH_PAYMENT_DONE, OnPaymentDone);
		EventManager.AddListener<float>(PaymentEvents.POS_PAYMENT_DONE, OnPaymentDone);
	}

	private void OnPaymentDone(float value)
	{
	}

	private void TryShowRateUS()
	{
		if (!GenericDataSerializer.LoadBool("RATED_3_PLUS_KEY"))
		{
			Open();
		}
	}

	private void OnStarClicked(int id)
	{
		for (int i = 0; i < stars.Count; i++)
		{
			if (i <= id)
			{
				filledStarImages[i].enabled = true;
			}
			else
			{
				filledStarImages[i].enabled = false;
			}
		}
		currentRate = id + 1;
	}

	private void OnSend()
	{
		EventLogger.LogEvent("c_rated_" + currentRate);
		if (currentRate >= 4)
		{
			GenericDataSerializer.SaveBool("RATED_3_PLUS_KEY", value: true);
		}
		Close();
	}

	private void Open()
	{
	}

	private void Close()
	{
		EventLogger.LogEvent("c_rate_us_closed");
		canvasGroup.alpha = 0f;
		canvas.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!canvas.enabled);
	}
}
