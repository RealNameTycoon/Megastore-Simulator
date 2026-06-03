using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TabbedPanel : UIWindow
{
	[SerializeField]
	private List<Button> buttons;

	[SerializeField]
	private List<Image> buttonBgs;

	[SerializeField]
	private List<TabWindow> windows;

	[SerializeField]
	private List<TabWindow> demoWindows;

	[SerializeField]
	private Transform windowPoolPosition;

	[SerializeField]
	private Transform windowTargetPosition;

	[SerializeField]
	private Canvas tabbedPanelCanvas;

	[SerializeField]
	private Button minimizeButton;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private WindowTypes type;

	public static Color selectedBGColor = new Color(0.23137255f, 1f / 3f, 36f / 85f);

	public static Color deselectedBGColor = new Color(2f / 15f, 0.21960784f, 0.3019608f);

	private int selectedButtonIndex = -1;

	private List<TabWindow> Windows
	{
		get
		{
			if (GameManager.isDemo)
			{
				return demoWindows;
			}
			return windows;
		}
	}

	protected void Start()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			int index = i;
			buttons[index].onClick.AddListener(delegate
			{
				SelectButton(index, instant: false);
			});
		}
		for (int num = 0; num < Windows.Count; num++)
		{
			Windows[num].transform.position = windowPoolPosition.position;
		}
		minimizeButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<ComputerTabManager>.Instance.MinimizeTab(type);
		});
		closeButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<ComputerTabManager>.Instance.CloseTab(type);
		});
		SelectButton(0, instant: true);
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshTabNavigations);
		EventManager.AddListener(UIEvents.FAKE_LOADING_FINISHED, OnFakeLoadingFinished);
	}

	private void OnFakeLoadingFinished()
	{
		base.gameObject.SetActive(value: false);
	}

	public override void Open()
	{
		if (!tabbedPanelCanvas.enabled)
		{
			BeforeOpen();
			tabbedPanelCanvas.enabled = true;
			for (int i = 0; i < Windows.Count; i++)
			{
				Windows[i].EnableRaycaster(state: true);
			}
			base.Open();
			RefreshTabNavigations();
		}
	}

	public virtual void BeforeOpen()
	{
		base.gameObject.SetActive(value: true);
	}

	public virtual void RemoveFocus()
	{
		if (base.IsOpen())
		{
			base.Close();
		}
	}

	public override void Close()
	{
		if (tabbedPanelCanvas.enabled)
		{
			tabbedPanelCanvas.enabled = false;
			for (int i = 0; i < Windows.Count; i++)
			{
				Windows[i].EnableRaycaster(state: false);
			}
			if (base.IsOpen())
			{
				base.Close();
			}
			AfterClose();
		}
	}

	public virtual void AfterClose()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Repaint()
	{
		SelectButton(0, instant: true);
	}

	public void SelectButton(int index, bool instant)
	{
		if (buttons.Count != 0 && selectedButtonIndex != index)
		{
			if (selectedButtonIndex != -1)
			{
				DeselectButton(selectedButtonIndex, instant);
			}
			buttonBgs[index].color = selectedBGColor;
			if (instant)
			{
				Windows[index].transform.position = windowTargetPosition.position;
				Windows[index].Open();
			}
			else
			{
				Windows[index].transform.DOKill();
				Windows[index].Open();
				Windows[index].transform.DOMove(windowTargetPosition.position, 0.2f);
			}
			RefreshTabNavigations(index);
			selectedButtonIndex = index;
		}
	}

	public void RefreshTabNavigations()
	{
		if (!IsOpen())
		{
			return;
		}
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i].IsOpen())
			{
				RefreshTabNavigations(i);
				break;
			}
		}
	}

	private void RefreshTabNavigations(int index)
	{
		Selectable firstSelectable = GetWindow(index).GetFirstSelectable();
		for (int i = 0; i < buttons.Count; i++)
		{
			Navigation navigation = buttons[i].navigation;
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnDown = firstSelectable;
			buttons[i].navigation = navigation;
		}
	}

	public virtual TabWindow GetWindow(int index)
	{
		return Windows[index];
	}

	private void DeselectButton(int index, bool instant)
	{
		buttonBgs[index].color = deselectedBGColor;
		if (instant)
		{
			Windows[index].transform.position = windowPoolPosition.position;
			Windows[index].Close();
			return;
		}
		Windows[index].transform.DOKill();
		Windows[index].transform.DOMove(windowPoolPosition.position, 0.2f).OnComplete(delegate
		{
			Windows[index].Close();
		});
	}

	private void Update()
	{
	}
}
