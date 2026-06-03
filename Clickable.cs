using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : Interactable
{
	[SerializeField]
	private Outline outline;

	[SerializeField]
	public UnityEvent onClickAction;

	[SerializeField]
	public UnityEvent onRightClickAction;

	[SerializeField]
	private bool useInteractionUI = true;

	[SerializeField]
	private float interactionDistanceOverride = RayShooter.DEFAULT_INTERACTION_DETECTION_DISTANCE;

	private static float HOVER_DETECTION_DISTANCE = 4f;

	private Action hoverStartedAction;

	private Action hoverEndedAction;

	protected Outline Outline => outline;

	public void SetHoverStartedAction(Action hoverStartedAction)
	{
		this.hoverStartedAction = hoverStartedAction;
	}

	public void SetHoverEndedAction(Action hoverEndedAction)
	{
		this.hoverEndedAction = hoverEndedAction;
	}

	public override float GetInteractionDistance()
	{
		return interactionDistanceOverride;
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		if (IsDetectionZone() && (!IsOutlineAssigned() || IsOutlineEnabled()))
		{
			OnClick();
			onClickAction?.Invoke();
		}
	}

	public override void OnMouseRMBDown()
	{
		base.OnMouseRMBDown();
		if (IsDetectionZone() && (!IsOutlineAssigned() || IsOutlineEnabled()))
		{
			onRightClickAction?.Invoke();
		}
	}

	public override void OnMouseButtonUp()
	{
		base.OnMouseButtonUp();
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		if (!IsDetectionZone())
		{
			if (IsOutlineAssigned() && IsOutlineEnabled())
			{
				EnableOutline(state: false);
				if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
				{
					SingletonBehaviour<TooltipUI>.Instance.Close();
				}
				if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
				{
					SingletonBehaviour<ButtonsWindow>.Instance.Close();
				}
			}
		}
		else
		{
			EnableOutline(state: true);
			if (SingletonBehaviour<HotKeyManager>.Instance.SelectedHotkeyIndex != -1)
			{
				SingletonBehaviour<HotKeyManager>.Instance.RepaintButtonsForHover(this);
			}
			else
			{
				RepaintButtonsWindow();
			}
			hoverStartedAction?.Invoke();
		}
	}

	public virtual void RepaintButtonsWindow()
	{
		if (GetToolTip() != "")
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				(GetToolTip(), null)
			} }, base.transform);
		}
	}

	public void RepaintTooltip()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			KeyCode.Mouse0,
			(GetToolTip(), null)
		} }, base.transform);
	}

	protected virtual void EnableOutline(bool state)
	{
		if (outline != null)
		{
			outline.enabled = state;
			if (useInteractionUI)
			{
				SingletonBehaviour<UIManager>.Instance.ActivateInteractionUI(state);
			}
		}
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (IsOutlineAssigned() && IsOutlineEnabled())
		{
			EnableOutline(state: false);
			if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
			{
				SingletonBehaviour<TooltipUI>.Instance.Close();
			}
			if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
			{
				SingletonBehaviour<ButtonsWindow>.Instance.Close();
				SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
				SingletonBehaviour<VehicleManager>.Instance.UpdateMenu();
			}
		}
		hoverEndedAction?.Invoke();
	}

	private bool IsDetectionZone()
	{
		return true;
	}

	protected virtual bool IsOutlineAssigned()
	{
		return outline != null;
	}

	protected virtual bool IsOutlineEnabled()
	{
		return outline.enabled;
	}

	protected virtual void OnClick()
	{
	}
}
