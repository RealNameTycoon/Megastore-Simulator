using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentsWindow : TabWindow
{
	[SerializeField]
	private Button equipmentsTabButton;

	[SerializeField]
	private List<VehicleUI> vehicleUIs;

	[SerializeField]
	private ComputerPopup computerPopup;

	private const int ITEM_PER_ROW = 3;

	public override void Initialize()
	{
		base.Initialize();
		for (int i = 0; i < vehicleUIs.Count; i++)
		{
			vehicleUIs[i].Initialize(computerPopup);
		}
		RefreshNavigation();
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshIfOpen);
		EventManager.AddListener<VehicleType>(GameEvents.VEHICLE_SOLD, delegate
		{
			RefreshIfOpen();
		});
	}

	public void PurchaseVehicle(VehicleType vehicleType)
	{
		for (int i = 0; i < vehicleUIs.Count; i++)
		{
			if (vehicleUIs[i].VehicleType == vehicleType)
			{
				vehicleUIs[i].PurchaseVehicle();
				break;
			}
		}
	}

	private void RefreshIfOpen()
	{
		if (IsOpen())
		{
			RefreshNavigation();
		}
	}

	private void RefreshNavigation()
	{
		if (equipmentsTabButton == null || vehicleUIs == null)
		{
			return;
		}
		Selectable[] slots = new Selectable[vehicleUIs.Count];
		for (int i = 0; i < vehicleUIs.Count; i++)
		{
			VehicleUI vehicleUI = vehicleUIs[i];
			if (vehicleUI == null || !vehicleUI.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else if (!vehicleUI.GetSelectable().gameObject.activeSelf || vehicleUI.VehicleType == VehicleType.PALLET_ROBOT)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = vehicleUI.GetSelectable();
			}
		}
		for (int j = 0; j < vehicleUIs.Count; j++)
		{
			VehicleUI vehicleUI2 = vehicleUIs[j];
			if (!(vehicleUI2 == null) && vehicleUI2.gameObject.activeSelf && !(slots[j] == null))
			{
				int rowLock = j / 3;
				Selectable up = FindNext(j, -3, -1) ?? equipmentsTabButton;
				Selectable down = FindNext(j, 3, -1);
				Selectable left = FindNext(j, -1, rowLock);
				Selectable right = FindNext(j, 1, rowLock);
				vehicleUI2.RefreshNavigation(up, down, left, right);
			}
		}
		Selectable FindNext(int startIndex, int step, int num)
		{
			for (int k = startIndex + step; k >= 0 && k < slots.Length && (num == -1 || k / 3 == num); k += step)
			{
				if (slots[k] != null)
				{
					return slots[k];
				}
			}
			return null;
		}
	}

	public override Selectable GetFirstSelectable()
	{
		for (int i = 0; i < vehicleUIs.Count; i++)
		{
			if (vehicleUIs[i].GetSelectable().gameObject.activeSelf)
			{
				return vehicleUIs[i].GetSelectable();
			}
		}
		return null;
	}
}
