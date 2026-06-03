using System.Collections.Generic;
using UnityEngine;

public class ComputerTabManager : SingletonBehaviour<ComputerTabManager>
{
	[SerializeField]
	private List<ComputerTab> tabs;

	[SerializeField]
	private List<WindowTypes> windowTypes;

	private int activeTabIndex = -1;

	private void Start()
	{
		for (int i = 0; i < tabs.Count; i++)
		{
			int index = i;
			tabs[i].TabButton.onClick.AddListener(delegate
			{
				OnTabClicked(index);
			});
		}
	}

	private void OnTabClicked(int index)
	{
		if (!tabs[index].IsSelected)
		{
			SelectTab(index, instant: true);
		}
		else
		{
			MinimizeTab(index);
		}
	}

	public void SelectTab(WindowTypes type)
	{
		SelectTab(windowTypes.IndexOf(type), instant: false);
	}

	public void MinimizeTab(WindowTypes type)
	{
		tabs[windowTypes.IndexOf(type)].Deselect();
	}

	public void MinimizeTab(int index)
	{
		tabs[index].Deselect();
	}

	public void CloseTab(WindowTypes type)
	{
		ComputerTab computerTab = tabs[windowTypes.IndexOf(type)];
		computerTab.Deselect();
		computerTab.Close();
	}

	public void MinimizeAllTabs()
	{
		for (int i = 0; i < tabs.Count; i++)
		{
			if (tabs[i].IsSelected)
			{
				tabs[i].Deselect();
			}
		}
	}

	public void SelectTab(int tabIndex, bool instant)
	{
		if (activeTabIndex != -1)
		{
			tabs[activeTabIndex].DeselectInstant();
		}
		activeTabIndex = tabIndex;
		tabs[activeTabIndex].SelectInstant();
	}
}
