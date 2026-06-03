using UnityEngine;
using UnityEngine.UI;

public class TabWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	protected GameObject scrollParent;

	private bool isInitialized;

	protected virtual void Start()
	{
		if (scrollParent != null && !IsOpen())
		{
			scrollParent.SetActive(value: false);
		}
	}

	public void RefreshScrollVisibility()
	{
		if (IsOpen() && scrollParent != null && !scrollParent.activeSelf)
		{
			scrollParent.SetActive(value: true);
		}
	}

	public virtual void BeforeOpen()
	{
		if (scrollParent != null)
		{
			scrollParent.SetActive(value: true);
		}
	}

	public virtual void Open()
	{
		if (!isInitialized)
		{
			Initialize();
		}
		BeforeOpen();
		if (canvas != null)
		{
			canvas.enabled = true;
			raycaster.enabled = true;
		}
	}

	public virtual void Close()
	{
		if (canvas != null)
		{
			canvas.enabled = false;
			raycaster.enabled = false;
		}
		AfterClose();
	}

	public virtual void AfterClose()
	{
		if (scrollParent != null)
		{
			scrollParent.SetActive(value: false);
		}
	}

	public bool IsOpen()
	{
		return canvas.enabled;
	}

	public virtual void Initialize()
	{
		isInitialized = true;
	}

	public void EnableRaycaster(bool state)
	{
		if (raycaster != null)
		{
			raycaster.enabled = state;
		}
	}

	public virtual Selectable GetFirstSelectable()
	{
		return null;
	}
}
