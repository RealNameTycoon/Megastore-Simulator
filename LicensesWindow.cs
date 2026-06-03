using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LicensesWindow : TabWindow
{
	[SerializeField]
	private Button licensesTabButton;

	[SerializeField]
	private List<LicenseUI> licenseUIs;

	[SerializeField]
	private ProductGroup productGroup;

	[SerializeField]
	private GameObject wishlistUI;

	private const int ITEM_PER_ROW = 3;

	public void ApplyFilter(ProductGroup productGroup)
	{
		if (this.productGroup != productGroup && IsOpen())
		{
			Close();
		}
		else if (this.productGroup == productGroup && !IsOpen())
		{
			Open();
			RefreshNavigation();
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		wishlistUI.gameObject.SetActive(GameManager.isDemo);
		if (GameManager.isDemo && (productGroup == ProductGroup.ELECTRONICS || productGroup == ProductGroup.MUSIC || productGroup == ProductGroup.SPORTS))
		{
			for (int i = 0; i < licenseUIs.Count; i++)
			{
				licenseUIs[i].gameObject.SetActive(value: false);
			}
			return;
		}
		for (int j = 0; j < licenseUIs.Count; j++)
		{
			if (licenseUIs[j].gameObject.activeSelf)
			{
				licenseUIs[j].Initialize(j + 1, productGroup);
			}
		}
		EventManager.AddListener<int, ProductGroup>(ProductEvents.PRODUCT_LICENSE_PURCHASED, OnLicensePurchased);
	}

	private void OnLicensePurchased(int level, ProductGroup group)
	{
		if (group == productGroup)
		{
			RefreshNavigation();
			SingletonBehaviour<InputManager>.Instance.SelectElement(licensesTabButton.gameObject);
			if (level == licenseUIs.Count)
			{
				EventLogger.AllLicensesOneDepartment();
			}
		}
	}

	private void RefreshNavigation()
	{
		if (!IsOpen())
		{
			return;
		}
		Selectable[] slots = new Selectable[licenseUIs.Count];
		for (int i = 0; i < licenseUIs.Count; i++)
		{
			LicenseUI licenseUI = licenseUIs[i];
			if (licenseUI == null || !licenseUI.gameObject.activeSelf)
			{
				slots[i] = null;
				continue;
			}
			Selectable selectable = licenseUI.GetSelectable();
			if (selectable == null || !selectable.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = selectable;
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
		for (int k = 0; k < slots.Length; k++)
		{
			Selectable selectable2 = slots[k];
			if (!(selectable2 == null))
			{
				int rowLock = k / 3;
				Selectable selectOnUp = FindNext(k, -3, -1) ?? licensesTabButton;
				Selectable selectOnDown = FindNext(k, 3, -1);
				Selectable selectOnLeft = FindNext(k, -1, rowLock);
				Selectable selectOnRight = FindNext(k, 1, rowLock);
				Navigation navigation = selectable2.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnUp = selectOnUp;
				navigation.selectOnDown = selectOnDown;
				navigation.selectOnLeft = selectOnLeft;
				navigation.selectOnRight = selectOnRight;
				selectable2.navigation = navigation;
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
		for (int i = 0; i < licenseUIs.Count; i++)
		{
			if (licenseUIs[i] != null && licenseUIs[i].GetSelectable().gameObject.activeSelf)
			{
				return licenseUIs[i].GetSelectable();
			}
		}
		return null;
	}
}
