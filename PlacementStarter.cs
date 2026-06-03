using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacementStarter : Interactable, MoveableHolderInterface
{
	[SerializeField]
	private Furniture parentFurniture;

	[SerializeField]
	private Placeable parentPlaceable;

	private static float HOVER_DETECTION_DISTANCE = 4f;

	private static float PLACE_HOLD_DURATION = 1.2f;

	private bool holdStarted;

	private float fillAmount;

	protected bool isHovered;

	private Action onMouseHoveredAction;

	private Action onMouseHoverEndedAction;

	public void SetMouseHoverAction(Action hoverAction)
	{
		onMouseHoveredAction = hoverAction;
	}

	public void SetMouseHoverEndedAction(Action hoverEndedAction)
	{
		onMouseHoverEndedAction = hoverEndedAction;
	}

	public Moveable GetMoveable()
	{
		if (parentFurniture != null)
		{
			return parentFurniture.Moveable;
		}
		return parentPlaceable.Moveable;
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		if (IsDetectionZone() && !SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle && !SingletonBehaviour<TrayManager>.Instance.IsPicked && !FireExtinguisher.Instance.IsPicked && !SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			holdStarted = true;
		}
	}

	public override void OnMouseButtonUp()
	{
		base.OnMouseButtonUp();
		if (holdStarted)
		{
			EndHold();
		}
		IsDetectionZone();
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		if (!IsDetectionZone())
		{
			CloseInteractionElements();
			return;
		}
		if (SingletonBehaviour<BoxManager>.Instance.NoContainerPicked() && !FireExtinguisher.Instance.IsPicked)
		{
			Dictionary<KeyCode, (string, Action)> dictionary = new Dictionary<KeyCode, (string, Action)>();
			if (parentFurniture != null && parentFurniture.GetExtraButtonActions() != null)
			{
				foreach (var extraButtonAction in parentFurniture.GetExtraButtonActions())
				{
					dictionary.Add(extraButtonAction.Item1, extraButtonAction.Item2);
				}
			}
			else if (parentPlaceable != null && parentPlaceable.GetExtraButtonActions() != null)
			{
				foreach (var extraButtonAction2 in parentPlaceable.GetExtraButtonActions())
				{
					dictionary.Add(extraButtonAction2.Item1, extraButtonAction2.Item2);
				}
			}
			if (parentPlaceable != null)
			{
				parentPlaceable.OnShelfHoverStarted();
			}
			dictionary.Add(KeyCode.Mouse1, ("place_move", StartPlacement));
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(dictionary, base.transform);
		}
		onMouseHoveredAction?.Invoke();
	}

	private void StartPlacement()
	{
		if (parentPlaceable != null)
		{
			parentPlaceable.StartNewPlacement();
		}
		else
		{
			parentFurniture.StartNewPlacement();
		}
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (holdStarted)
		{
			EndHold();
		}
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		if (IsDetectionZone())
		{
			if (parentPlaceable != null)
			{
				parentPlaceable.OnShelfHoverEnded();
			}
			CloseInteractionElements();
			onMouseHoverEndedAction?.Invoke();
		}
	}

	protected void CloseInteractionElements()
	{
		isHovered = false;
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		FireExtinguisher.Instance.UpdateMenu();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private bool IsDetectionZone()
	{
		return true;
	}

	private void EndHold()
	{
		holdStarted = false;
		fillAmount = 0f;
		SingletonBehaviour<UIManager>.Instance.ResetHoldProgress();
	}

	public BoxCollider SnappableBoxCollider()
	{
		return null;
	}
}
