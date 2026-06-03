using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorationWindow : TabWindow
{
	[SerializeField]
	private Button decorationTabButton;

	[SerializeField]
	private List<DecorationUI> decorationUIsFloor;

	[SerializeField]
	private List<DecorationUI> decorationUIsWall;

	private const int ITEM_PER_ROW = 3;

	public override void Initialize()
	{
		base.Initialize();
		for (int i = 0; i < decorationUIsFloor.Count; i++)
		{
			decorationUIsFloor[i].Initialize();
		}
		for (int j = 0; j < decorationUIsWall.Count; j++)
		{
			decorationUIsWall[j].Initialize();
		}
		RefreshNavigation();
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshIfOpen);
	}

	private void RefreshIfOpen()
	{
		if (IsOpen())
		{
			RefreshNavigation();
			if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
			{
				SingletonBehaviour<InputManager>.Instance.SelectElement(decorationTabButton.gameObject);
			}
		}
	}

	private void RefreshNavigation()
	{
		RefreshNavigation(decorationUIsFloor, GetFirstWallSelectable());
		RefreshNavigation(decorationUIsWall, null, GetLastFloorSelectable());
	}

	private Selectable GetLastFloorSelectable()
	{
		for (int num = decorationUIsFloor.Count - 1; num >= 0; num--)
		{
			if (decorationUIsFloor[num].GetSelectable() != null)
			{
				return decorationUIsFloor[num].GetSelectable();
			}
		}
		return null;
	}

	private Selectable GetFirstWallSelectable()
	{
		for (int i = 0; i < decorationUIsWall.Count; i++)
		{
			if (decorationUIsWall[i].GetSelectable() != null)
			{
				return decorationUIsWall[i].GetSelectable();
			}
		}
		return null;
	}

	public override Selectable GetFirstSelectable()
	{
		for (int i = 0; i < decorationUIsFloor.Count; i++)
		{
			if (decorationUIsFloor[i].GetSelectable() != null)
			{
				return decorationUIsFloor[i].GetSelectable();
			}
		}
		return null;
	}

	private void RefreshNavigation(List<DecorationUI> decorationUIs, Selectable bottomSelectable, Selectable upperSelectableOverride = null)
	{
		if (decorationTabButton == null || decorationUIs == null || decorationUIs.Count == 0)
		{
			return;
		}
		Selectable[] slots = new Selectable[decorationUIs.Count];
		for (int i = 0; i < decorationUIs.Count; i++)
		{
			DecorationUI decorationUI = decorationUIs[i];
			if (decorationUI == null || !decorationUI.gameObject.activeSelf)
			{
				slots[i] = null;
				continue;
			}
			Selectable selectable = decorationUI.GetSelectable();
			if (selectable == null || !selectable.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = selectable;
			}
		}
		for (int j = 0; j < decorationUIs.Count; j++)
		{
			DecorationUI decorationUI2 = decorationUIs[j];
			if (!(decorationUI2 == null) && decorationUI2.gameObject.activeSelf && !(slots[j] == null))
			{
				int rowLock = j / 3;
				Selectable up = FindNext(j, -3, -1) ?? upperSelectableOverride ?? decorationTabButton;
				Selectable down = FindNext(j, 3, -1) ?? bottomSelectable;
				Selectable left = FindNext(j, -1, rowLock);
				Selectable right = FindNext(j, 1, rowLock);
				decorationUI2.RefreshNavigation(up, down, left, right);
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
}
