using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LoadingDock : MonoBehaviour
{
	[SerializeField]
	private OrderManager.OrderReceivingArea receivingArea;

	[SerializeField]
	private Transform gate;

	[SerializeField]
	private Transform gateTarget;

	[SerializeField]
	private Collider dockBlockerCollider;

	[SerializeField]
	private AudioSource gateAudioSource;

	[SerializeField]
	private Collider clickableCollider;

	[SerializeField]
	private LoadingDockUI loadingDockUI;

	private Vector3 gateStartPosition;

	private bool gateOpen;

	private Coroutine updateProgressRoutine;

	private DockState state;

	public bool GateOpen => gateOpen;

	private void Awake()
	{
		gateStartPosition = gate.localPosition;
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.TRUCK_ARRIVED, OnTruckArrived);
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.TRUCK_UNLOADED, OnTruckUnloaded);
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.TRUCK_UNLOADED_BY_STAFF, OnTruckUnloadedByStaff);
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.TRUCK_INCOMING, OnTruckIncoming);
		EventManager.AddListener<OrderManager.OrderReceivingArea>(GameEvents.ORDERS_UPDATED, OnOrdersUpdated);
	}

	private void Start()
	{
		if (SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea).Arrived)
		{
			state = DockState.TRUCK_ARRIVED;
			OpenGate(instant: true);
		}
		loadingDockUI.RepaintStatus(state);
		loadingDockUI.RepaintOrders(SingletonBehaviour<OrderManager>.Instance.GetOrders(receivingArea));
		loadingDockUI.RepaintProgress(0f);
	}

	private void OnTruckUnloaded(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == receivingArea)
		{
			state = DockState.TRUCK_WAITING_GATE;
			loadingDockUI.RepaintStatus(state);
		}
	}

	private void OnTruckUnloadedByStaff(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == receivingArea)
		{
			CloseGate();
		}
	}

	private void OnTruckIncoming(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == receivingArea)
		{
			state = DockState.TRUCK_INCOMING;
			loadingDockUI.RepaintStatus(state);
			loadingDockUI.RepaintProgress(0f);
			if (updateProgressRoutine != null)
			{
				StopCoroutine(updateProgressRoutine);
			}
			updateProgressRoutine = StartCoroutine(UpdateProgressRoutine());
		}
	}

	private IEnumerator UpdateProgressRoutine()
	{
		Truck truck = SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea);
		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			loadingDockUI.RepaintProgress(truck.ShipmentProgress);
		}
	}

	private void OnTruckArrived(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == receivingArea)
		{
			state = DockState.TRUCK_ARRIVED;
			loadingDockUI.RepaintStatus(state);
			loadingDockUI.RepaintProgress(1f);
			if (updateProgressRoutine != null)
			{
				StopCoroutine(updateProgressRoutine);
			}
			OpenGate();
		}
	}

	private void OnOrdersUpdated(OrderManager.OrderReceivingArea orderReceivingArea)
	{
		if (orderReceivingArea == receivingArea)
		{
			List<BuyPanel.CartSlot> orders = SingletonBehaviour<OrderManager>.Instance.GetOrders(receivingArea);
			loadingDockUI.RepaintOrders(orders);
		}
	}

	private void OpenGate(bool instant = false)
	{
		gateOpen = true;
		clickableCollider.enabled = true;
		dockBlockerCollider.enabled = false;
		if (instant)
		{
			gate.localPosition = gateTarget.localPosition;
			return;
		}
		gate.DOKill();
		gateAudioSource.Play();
		float num = gateTarget.transform.localPosition.y - gateStartPosition.y;
		gate.DOLocalMove(gateTarget.localPosition, num / 4f).SetEase(Ease.OutSine).SetSpeedBased(isSpeedBased: true);
		gateAudioSource.Play();
	}

	public void CloseGate()
	{
		if (!SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea).IsEmpty())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("truck_unload_warning_gate", base.transform);
			return;
		}
		gateOpen = false;
		clickableCollider.enabled = false;
		dockBlockerCollider.enabled = true;
		gate.DOKill();
		gateAudioSource.Play();
		float num = gateTarget.transform.localPosition.y - gateStartPosition.y;
		gate.DOLocalMove(gateStartPosition, num / 4f).SetEase(Ease.InSine).SetSpeedBased(isSpeedBased: true)
			.OnComplete(delegate
			{
				Truck truck = SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea);
				truck.SellPalletsInsideTruck();
				if (SingletonBehaviour<OrderManager>.Instance.GetOrders(receivingArea).Count == 0)
				{
					state = DockState.TRUCK_IDLE;
					loadingDockUI.RepaintStatus(state);
				}
				truck.Leave();
				loadingDockUI.RepaintProgress(0f);
			});
	}
}
