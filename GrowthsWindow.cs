using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowthsWindow : TabWindow
{
	[SerializeField]
	private Button growthTabButton;

	[SerializeField]
	private List<GrowthUI> growthUIs;

	[SerializeField]
	private GameObject wishlistUI;

	private const int ITEM_PER_ROW = 3;

	public override void Initialize()
	{
		base.Initialize();
		int num;
		if (GameManager.isDemo)
		{
			wishlistUI.SetActive(value: true);
			num = 2;
		}
		else
		{
			wishlistUI.SetActive(value: false);
			num = SingletonBehaviour<GrowthManager>.Instance.NumberOfExpansions;
		}
		for (int i = 0; i < growthUIs.Count; i++)
		{
			if (i < num)
			{
				growthUIs[i].gameObject.SetActive(value: true);
				growthUIs[i].Initialize(i + 1);
			}
			else
			{
				growthUIs[i].gameObject.SetActive(value: false);
			}
		}
		RefreshNavigation();
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshIfOpen);
	}

	private void RepaintTexts()
	{
		_ = SingletonBehaviour<GrowthManager>.Instance.GrowthCount;
	}

	public override Selectable GetFirstSelectable()
	{
		for (int i = 0; i < growthUIs.Count; i++)
		{
			if (growthUIs[i].GetSelectable().gameObject.activeSelf)
			{
				return growthUIs[i].GetSelectable();
			}
		}
		return null;
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
		Selectable[] slots = new Selectable[growthUIs.Count];
		for (int i = 0; i < growthUIs.Count; i++)
		{
			GrowthUI growthUI = growthUIs[i];
			if (growthUI == null || !growthUI.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else if (!growthUI.GetSelectable().gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = growthUI.GetSelectable();
			}
		}
		for (int j = 0; j < slots.Length; j++)
		{
			if (slots[j] != null)
			{
				_ = slots[j];
				break;
			}
		}
		for (int k = 0; k < growthUIs.Count; k++)
		{
			GrowthUI growthUI2 = growthUIs[k];
			if (!(growthUI2 == null) && growthUI2.gameObject.activeSelf && !(slots[k] == null))
			{
				int rowLock = k / 3;
				Selectable up = FindNext(k, -3, -1) ?? growthTabButton;
				Selectable down = FindNext(k, 3, -1);
				Selectable left = FindNext(k, -1, rowLock);
				Selectable right = FindNext(k, 1, rowLock);
				growthUI2.RefreshNavigation(up, down, left, right);
			}
		}
		Selectable FindNext(int startIndex, int step, int num)
		{
			for (int l = startIndex + step; l >= 0 && l < slots.Length && (num == -1 || l / 3 == num); l += step)
			{
				if (slots[l] != null)
				{
					return slots[l];
				}
			}
			return null;
		}
	}
}
