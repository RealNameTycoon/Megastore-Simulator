using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShelvesWindow : TabWindow
{
	[Serializable]
	public class FullVersionShelfDisplay
	{
		public GameObject shelfUI;

		public ProductGroup group;
	}

	[SerializeField]
	private Button shelvesTabButton;

	[SerializeField]
	private List<ShelfUI> shelfUIs;

	[SerializeField]
	private List<FullVersionShelfDisplay> fullVersionDisplays;

	[SerializeField]
	private List<ProductData> shelfData;

	[SerializeField]
	private List<ProductData> shelfDataDemo;

	[SerializeField]
	private GameObject storageShelfUnlockedUI;

	[SerializeField]
	private Image storageShelfBG;

	private const int ITEM_PER_ROW = 3;

	private ProductGroup currentFilter = ProductGroup.NONE;

	private void Awake()
	{
		EventManager.AddListener(SupermarketEvents.STORAGE_LICENSE_PURCHASED, OnStorageLicensePurchased);
	}

	public override void Initialize()
	{
		base.Initialize();
		RefreshShelves();
	}

	private void RefreshShelves()
	{
		int num = 0;
		if (GameManager.isDemo)
		{
			shelfData = shelfDataDemo;
		}
		for (int i = 0; i < shelfData.Count; i++)
		{
			if (shelfData[i].productGroup == currentFilter || shelfData[i].productGroup == ProductGroup.EQUIPMENTS)
			{
				shelfUIs[num].gameObject.SetActive(value: true);
				shelfUIs[num].Repaint(shelfData[i]);
				num++;
			}
		}
		for (int j = num; j < shelfUIs.Count; j++)
		{
			shelfUIs[j].gameObject.SetActive(value: false);
		}
		if (GameManager.isDemo)
		{
			for (int k = 0; k < fullVersionDisplays.Count; k++)
			{
				if (fullVersionDisplays[k].group != currentFilter)
				{
					fullVersionDisplays[k].shelfUI.SetActive(value: false);
				}
				else
				{
					fullVersionDisplays[k].shelfUI.SetActive(value: true);
				}
			}
		}
		RepaintStorageShelf();
		RefreshNavigation();
	}

	private void OnStorageLicensePurchased()
	{
		RepaintStorageShelf();
	}

	private void RepaintStorageShelf()
	{
		if (SingletonBehaviour<StorageManager>.Instance.StorageLicensePurchased)
		{
			storageShelfUnlockedUI.SetActive(value: true);
			Color color = storageShelfBG.color;
			color.a = 1f;
			storageShelfBG.color = color;
		}
	}

	public void ApplyFilter(ProductGroup productGroup)
	{
		currentFilter = productGroup;
		RefreshShelves();
	}

	private void RefreshNavigation()
	{
		List<ShelfUI> list = new List<ShelfUI>();
		for (int i = 0; i < shelfUIs.Count; i++)
		{
			if (shelfUIs[i] != null && shelfUIs[i].gameObject.activeSelf)
			{
				list.Add(shelfUIs[i]);
			}
		}
		if (list.Count != 0)
		{
			List<Selectable> list2 = new List<Selectable>(list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				list2.Add(list[j].RefreshNavigation(null, null, null, null));
			}
			for (int k = 0; k < list.Count; k++)
			{
				int num = k % 3;
				Selectable up = ((k < 3) ? shelvesTabButton : list2[k - 3]);
				Selectable down = ((k + 3 < list2.Count) ? list2[k + 3] : null);
				Selectable left = ((num > 0) ? list2[k - 1] : null);
				Selectable right = ((num < 2 && k + 1 < list2.Count) ? list2[k + 1] : null);
				list[k].RefreshNavigation(up, down, left, right);
			}
		}
	}

	public override Selectable GetFirstSelectable()
	{
		for (int i = 0; i < shelfUIs.Count; i++)
		{
			if (shelfUIs[i] != null && shelfUIs[i].gameObject.activeSelf)
			{
				return shelfUIs[i].GetSelectable();
			}
		}
		return null;
	}
}
