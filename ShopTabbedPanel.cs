using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopTabbedPanel : TabbedPanel
{
	[Serializable]
	public class FilterButtonData
	{
		public Button button;

		public Image background;

		public ProductGroup productGroup;
	}

	[SerializeField]
	private ProductsWindow productsWindow;

	[SerializeField]
	private ShelvesWindow shelvesWindow;

	[SerializeField]
	private List<LicensesWindow> licensesWindows;

	[SerializeField]
	private ProductGroup initialFilter = ProductGroup.NONE;

	[SerializeField]
	private List<FilterButtonData> filterButtons;

	[SerializeField]
	private CartWindow cartWindow;

	private int selectedTabIndex = -1;

	private void Awake()
	{
		EventManager.AddListener(StartupEvents.SPAWN_MANAGER_INITIALIZED, Initialize);
	}

	private void Initialize()
	{
		initialFilter = SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup;
		for (int i = 0; i < filterButtons.Count; i++)
		{
			int index = i;
			filterButtons[i].button.onClick.AddListener(delegate
			{
				SelectFilterButton(index);
			});
			if (filterButtons[i].productGroup == initialFilter)
			{
				SelectFilterButton(i);
			}
		}
		EventManager.AddListener<int>(TutorialEvents.TUTORIAL_STEP_DONE, OnTutorialStepDone);
		EventManager.AddListener(ProductEvents.INITIAL_LICENSE_DECIDED, OnInitialLicenseDecided);
	}

	public override void AfterClose()
	{
		base.AfterClose();
		productsWindow.AfterClose();
	}

	public override void BeforeOpen()
	{
		base.BeforeOpen();
		productsWindow.RefreshScrollVisibility();
	}

	private void OnTutorialStepDone(int stepID)
	{
		if (stepID != 11)
		{
			return;
		}
		SelectButton(0, instant: true);
		initialFilter = SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup;
		for (int i = 0; i < filterButtons.Count; i++)
		{
			if (filterButtons[i].productGroup == initialFilter)
			{
				SelectFilterButton(i);
				break;
			}
		}
	}

	private void OnInitialLicenseDecided()
	{
		initialFilter = SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup;
		for (int i = 0; i < filterButtons.Count; i++)
		{
			if (filterButtons[i].productGroup == initialFilter)
			{
				SelectFilterButton(i);
			}
		}
	}

	public void SelectFilterButton(int index)
	{
		if (index != selectedTabIndex)
		{
			if (selectedTabIndex != -1)
			{
				DeselectFilterButton(selectedTabIndex);
			}
			if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(12))
			{
				SingletonBehaviour<TutorialManager>.Instance.EnableAddToCartAnimation(filterButtons[index].productGroup == SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup);
			}
			filterButtons[index].background.color = TabbedPanel.selectedBGColor;
			selectedTabIndex = index;
			ApplyFilter(filterButtons[index].productGroup);
		}
	}

	private void DeselectFilterButton(int index)
	{
		filterButtons[index].background.color = TabbedPanel.deselectedBGColor;
	}

	private void ApplyFilter(ProductGroup productGroup)
	{
		productsWindow.ApplyFilter(productGroup);
		shelvesWindow.ApplyFilter(productGroup);
		for (int i = 0; i < licensesWindows.Count; i++)
		{
			licensesWindows[i].ApplyFilter(productGroup);
		}
		RefreshTabNavigations();
	}

	public override TabWindow GetWindow(int index)
	{
		if (base.GetWindow(index).transform == licensesWindows[0].transform.parent)
		{
			for (int i = 0; i < licensesWindows.Count; i++)
			{
				if (licensesWindows[i].IsOpen())
				{
					return licensesWindows[i];
				}
			}
		}
		return base.GetWindow(index);
	}

	public override void RemoveFocus()
	{
		if (cartWindow.IsOpen())
		{
			cartWindow.Close();
		}
		base.RemoveFocus();
	}
}
