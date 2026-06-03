using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStackManager : SingletonBehaviour<UIStackManager>
{
	[SerializeField]
	private List<UIWindow> windowStack = new List<UIWindow>();

	private bool windowClosedThisFrame;

	public bool WindowClosedThisFrame => windowClosedThisFrame;

	private new void Awake()
	{
		base.Awake();
		if (!(SingletonBehaviour<UIStackManager>.Instance != this))
		{
			Debug.Log("UIStackManager Awake called, InstanceID: " + GetInstanceID());
			Object.DontDestroyOnLoad(base.gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
	}

	private void OnWindowPushed(UIWindow window)
	{
		UIWindow topWindow = GetTopWindow();
		if (topWindow != null)
		{
			topWindow.OnFocusLost();
		}
		windowStack.Add(window);
	}

	private void OnWindowPopped(UIWindow window)
	{
		windowClosedThisFrame = true;
		if (windowStack.Count != 0)
		{
			List<UIWindow> list = windowStack;
			UIWindow item = list[list.Count - 1];
			windowStack.Remove(item);
			RefreshFocus(SingletonBehaviour<LastInputDeviceTracker>.Instance.Mode);
			if (windowStack.Count == 0)
			{
				SingletonBehaviour<InputManager>.Instance.ClearSelection();
			}
			StartCoroutine(WaitForWindowClosed());
		}
	}

	private IEnumerator WaitForWindowClosed()
	{
		yield return new WaitForEndOfFrame();
		windowClosedThisFrame = false;
	}

	public UIWindow GetTopWindow()
	{
		if (windowStack.Count == 0)
		{
			return null;
		}
		List<UIWindow> list = windowStack;
		return list[list.Count - 1];
	}

	private void OnInputDeviceChanged(LastInputDeviceType deviceType)
	{
		RefreshFocus(deviceType);
	}

	private void RefreshFocus(LastInputDeviceType deviceType)
	{
		if (deviceType == LastInputDeviceType.Gamepad)
		{
			UIWindow topWindow = GetTopWindow();
			if (topWindow != null)
			{
				topWindow.OnFocusGained();
			}
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		windowStack.Clear();
		if (scene.buildIndex == 0)
		{
			EventManager.AddListener<UIWindow>(UIEvents.UI_WINDOW_PUSHED, OnWindowPushed);
			EventManager.AddListener<UIWindow>(UIEvents.UI_WINDOW_POPPED, OnWindowPopped);
			EventManager.AddListener<LastInputDeviceType>(UIEvents.INPUT_DEVICE_CHANGED, OnInputDeviceChanged);
		}
	}

	public bool IsAnyWindowOpen()
	{
		return windowStack.Count > 0;
	}
}
