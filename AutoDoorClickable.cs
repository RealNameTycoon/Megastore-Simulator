using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoDoorClickable : Clickable
{
	[SerializeField]
	private AutoDoor autoDoor;

	protected override string GetToolTip()
	{
		if (autoDoor.IsOpen)
		{
			return "close_door";
		}
		return "open_door";
	}

	public override void RepaintButtonsWindow()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			KeyCode.Mouse0,
			(GetToolTip(), delegate
			{
				autoDoor.OpenOrCloseDoor();
			})
		} }, base.transform);
	}
}
