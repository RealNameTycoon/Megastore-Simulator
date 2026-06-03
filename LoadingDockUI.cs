using System;
using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingDockUI : Clickable
{
	[SerializeField]
	private Image truckImage;

	[SerializeField]
	private Image truckImageTarget;

	[SerializeField]
	private Image stateBGImage;

	[SerializeField]
	private TextMeshProUGUI stateText;

	[SerializeField]
	private List<LoadingDockProductRow> productRows;

	[SerializeField]
	private LoadingDockProductRow productRowPrefab;

	[SerializeField]
	private Collider clickableCollider;

	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	private Transform lockTarget;

	private Vector3 initialTruckImagePosition;

	protected Action onFocusRemovedAction;

	private DockState state;

	private List<BuyPanel.CartSlot> orders = new List<BuyPanel.CartSlot>();

	private Color redColor = new Color(0.95686275f, 0.2627451f, 18f / 85f, 1f);

	private Color yellowColor = new Color(0.9137255f, 63f / 85f, 0f, 1f);

	private Color greenColor = new Color(0.29803923f, 35f / 51f, 16f / 51f, 1f);

	private void Awake()
	{
		initialTruckImagePosition = truckImage.transform.localPosition;
	}

	public void OnUIClicked()
	{
		clickableCollider.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.RotationLocked = true;
		raycaster.enabled = true;
		SingletonBehaviour<PlayerLook>.Instance.LockToTransform(lockTarget, 0.3f, delegate
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					RemoveFocus();
				})
			} }, base.transform);
		});
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
	}

	public void RemoveFocus()
	{
		raycaster.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.UnlockCamera(0.3f, delegate
		{
			clickableCollider.enabled = true;
		});
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		onFocusRemovedAction?.Invoke();
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
	}

	public void RepaintStatus(DockState state)
	{
		this.state = state;
		stateText.text = Locale.GetWord(state.ToString());
		switch (state)
		{
		case DockState.TRUCK_IDLE:
			stateBGImage.color = Color.gray;
			truckImage.transform.localPosition = initialTruckImagePosition;
			break;
		case DockState.TRUCK_INCOMING:
			stateBGImage.color = yellowColor;
			break;
		case DockState.TRUCK_ARRIVED:
			stateBGImage.color = greenColor;
			truckImage.transform.localPosition = truckImageTarget.transform.localPosition;
			break;
		case DockState.TRUCK_WAITING_GATE:
			stateBGImage.color = redColor;
			truckImage.transform.localPosition = truckImageTarget.transform.localPosition;
			break;
		}
	}

	public void RepaintOrders(List<BuyPanel.CartSlot> orders)
	{
		this.orders = orders;
		for (int i = 0; i < orders.Count; i++)
		{
			if (i > productRows.Count - 1)
			{
				LoadingDockProductRow item = UnityEngine.Object.Instantiate(productRowPrefab, productRowPrefab.transform.parent);
				productRows.Add(item);
			}
			if (!orders[i].isInTruck)
			{
				productRows[i].Repaint(orders[i].type, orders[i].amount, Locale.GetWord("later_delivery"), i);
			}
			else if (state == DockState.TRUCK_INCOMING)
			{
				productRows[i].Repaint(orders[i].type, orders[i].amount, Locale.GetWord("on_the_way"), i);
			}
			productRows[i].gameObject.SetActive(value: true);
		}
		for (int j = orders.Count; j < productRows.Count; j++)
		{
			productRows[j].gameObject.SetActive(value: false);
		}
	}

	public void RepaintProgress(float progress)
	{
		truckImage.transform.localPosition = Vector3.Lerp(initialTruckImagePosition, truckImageTarget.transform.localPosition, progress);
	}
}
