using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageWindow : TabWindow
{
	[SerializeField]
	private Button storageTabButton;

	[SerializeField]
	private List<StorageGrowthUI> storageGrowthUIs;

	[SerializeField]
	private GameObject wishlistUI;

	private const int ITEM_PER_ROW = 3;

	public override void Initialize()
	{
		base.Initialize();
		if (GameManager.isDemo)
		{
			wishlistUI.SetActive(value: true);
			for (int i = 0; i < storageGrowthUIs.Count; i++)
			{
				if (i < 2)
				{
					storageGrowthUIs[i].Initialize(i + 1);
				}
				else
				{
					storageGrowthUIs[i].gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			for (int j = 0; j < storageGrowthUIs.Count; j++)
			{
				storageGrowthUIs[j].Initialize(j + 1);
			}
		}
		RefreshNavigation();
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshIfOpen);
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
		if (storageTabButton == null)
		{
			return;
		}
		Selectable[] slots = new Selectable[storageGrowthUIs.Count];
		for (int i = 0; i < storageGrowthUIs.Count; i++)
		{
			StorageGrowthUI storageGrowthUI = storageGrowthUIs[i];
			if (storageGrowthUI == null || !storageGrowthUI.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else if (!storageGrowthUI.GetSelectable().gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = storageGrowthUI.GetSelectable();
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
		for (int k = 0; k < storageGrowthUIs.Count; k++)
		{
			StorageGrowthUI storageGrowthUI2 = storageGrowthUIs[k];
			if (!(storageGrowthUI2 == null) && storageGrowthUI2.gameObject.activeSelf && !(slots[k] == null))
			{
				int rowLock = k / 3;
				Selectable up = FindNext(k, -3, -1) ?? storageTabButton;
				Selectable down = FindNext(k, 3, -1);
				Selectable left = FindNext(k, -1, rowLock);
				Selectable right = FindNext(k, 1, rowLock);
				storageGrowthUI2.RefreshNavigation(up, down, left, right);
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

	public override Selectable GetFirstSelectable()
	{
		for (int i = 0; i < storageGrowthUIs.Count; i++)
		{
			if (storageGrowthUIs[i].GetSelectable().gameObject.activeSelf)
			{
				return storageGrowthUIs[i].GetSelectable();
			}
		}
		return null;
	}
}
