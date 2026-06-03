using System;
using System.Collections.Generic;
using UnityEngine;

public class DockDoorClickable : Clickable
{
	[SerializeField]
	private LoadingDock loadingDock;

	public override void RepaintButtonsWindow()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			KeyCode.Mouse0,
			(GetToolTip(), delegate
			{
				loadingDock.CloseGate();
			})
		} }, base.transform);
	}
}
