using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FocusableUI : MoveableClickable
{
	[SerializeField]
	private Collider clickableCollider;

	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	private Transform lockTarget;

	[SerializeField]
	private UIWindow uiWindow;

	protected Action onFocusRemovedAction;

	private void Awake()
	{
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, delegate
		{
			if (!clickableCollider.enabled)
			{
				RemoveFocus();
			}
		});
	}

	public virtual void OnUIClicked()
	{
		clickableCollider.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.RotationLocked = true;
		raycaster.enabled = true;
		OnMouseHoverEnded();
		if (uiWindow != null)
		{
			uiWindow.Open();
		}
		SingletonBehaviour<PlayerLook>.Instance.LockToTransform(lockTarget, 0.3f, delegate
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					RemoveFocus();
				})
			} }, base.transform);
		});
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
	}

	public virtual void RemoveFocus()
	{
		raycaster.enabled = false;
		onFocusRemovedAction?.Invoke();
		if (uiWindow != null)
		{
			uiWindow.Close();
		}
		SingletonBehaviour<PlayerLook>.Instance.UnlockCamera(0.3f, delegate
		{
			clickableCollider.enabled = true;
		});
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
	}
}
