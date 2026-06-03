using UnityEngine;

public class UIWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject defaultSelectedElement;

	[SerializeField]
	private GameObject lastSelectedElement;

	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	private bool refreshSelectedEveryTime;

	[SerializeField]
	private bool isWorldSpaceUI;

	private bool isOpen;

	public bool IsWorldSpaceUI => isWorldSpaceUI;

	public GameObject DefaultSelectedElement => defaultSelectedElement;

	public virtual void Open()
	{
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_PUSHED, this);
		if (!refreshSelectedEveryTime && SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad && lastSelectedElement != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(lastSelectedElement);
		}
		else if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad && defaultSelectedElement != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(defaultSelectedElement);
		}
		if (!isWorldSpaceUI)
		{
			canvas.enabled = true;
		}
		isOpen = true;
	}

	public virtual void Close()
	{
		lastSelectedElement = SingletonBehaviour<InputManager>.Instance.GetSelectedElement();
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_POPPED, this);
		if (!isWorldSpaceUI)
		{
			canvas.enabled = false;
		}
		isOpen = false;
	}

	public virtual void OnFocusGained()
	{
		if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad && lastSelectedElement != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(lastSelectedElement);
		}
		else if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad && defaultSelectedElement != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(defaultSelectedElement);
		}
	}

	public virtual void OnFocusLost()
	{
		lastSelectedElement = SingletonBehaviour<InputManager>.Instance.GetSelectedElement();
	}

	public virtual bool IsOpen()
	{
		return isOpen;
	}
}
