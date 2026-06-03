using System;
using System.Collections.Generic;
using UnityEngine;

public class TruckDoor : Clickable
{
	[SerializeField]
	private Truck ownerTruck;

	protected override string GetToolTip()
	{
		if (ownerTruck.IsDoorOpen)
		{
			return "truck_close";
		}
		return "truck_open";
	}

	public override void RepaintButtonsWindow()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			KeyCode.Mouse0,
			(GetToolTip(), delegate
			{
				ownerTruck.OnDoorClicked();
			})
		} }, base.transform);
	}
}
