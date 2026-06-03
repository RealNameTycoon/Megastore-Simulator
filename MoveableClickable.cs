using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveableClickable : Clickable, MoveableHolderInterface
{
	[SerializeField]
	private Moveable moveableObject;

	private List<(KeyCode, (string, Action))> additionalActions;

	public Moveable GetMoveableObject()
	{
		return moveableObject;
	}

	public void SetAdditionalActions(List<(KeyCode, (string, Action))> additionalActions)
	{
		this.additionalActions = additionalActions;
	}

	public override void RepaintButtonsWindow()
	{
		Dictionary<KeyCode, (string, Action)> dictionary = new Dictionary<KeyCode, (string, Action)>();
		for (int i = 0; i < additionalActions.Count; i++)
		{
			dictionary.Add(additionalActions[i].Item1, additionalActions[i].Item2);
		}
		if (GetToolTip() != "")
		{
			dictionary.Add(KeyCode.Mouse0, (GetToolTip(), null));
		}
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(dictionary, base.transform);
	}

	public Moveable GetMoveable()
	{
		return moveableObject;
	}

	public BoxCollider SnappableBoxCollider()
	{
		return null;
	}
}
