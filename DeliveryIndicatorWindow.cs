using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryIndicatorWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowParent;

	[SerializeField]
	private TextMeshProUGUI deliveryInfoText;

	[SerializeField]
	private Image truckImage;

	[SerializeField]
	private Color deliveryIncomingColor;

	[SerializeField]
	private Color deliveredColor;

	[SerializeField]
	private Truck truck;

	private void Awake()
	{
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.TRUCK_INCOMING, OnDeliveryIncoming);
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.TRUCK_ARRIVED, OnDelivered);
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.TRUCK_LEAVING, OnDeliveryLeft);
	}

	private void Start()
	{
		if (truck.gameObject.activeSelf && !truck.IsMoving)
		{
			windowParent.SetActive(value: true);
			truckImage.color = deliveredColor;
		}
	}

	private void OnDeliveryIncoming(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == OrderManager.OrderReceivingArea.STORE_FRONT)
		{
			windowParent.SetActive(value: true);
			truckImage.color = deliveryIncomingColor;
			truckImage.DOKill();
			truckImage.DOFade(0f, 1f).SetLoops(-1, LoopType.Yoyo);
		}
	}

	private void OnDelivered(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == OrderManager.OrderReceivingArea.STORE_FRONT)
		{
			truckImage.DOKill();
			truckImage.DOFade(0f, 2f).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
			{
				truckImage.color = deliveredColor;
			});
		}
	}

	private void OnDeliveryLeft(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == OrderManager.OrderReceivingArea.STORE_FRONT)
		{
			Close();
		}
	}

	private void Close()
	{
		windowParent.SetActive(value: false);
		truckImage.color = deliveryIncomingColor;
	}
}
