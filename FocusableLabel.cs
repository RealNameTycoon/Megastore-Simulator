using System;
using UnityEngine;

public class FocusableLabel : Clickable
{
	[SerializeField]
	private Collider clickableCollider;

	[SerializeField]
	private Transform lockTarget;

	[SerializeField]
	private SupermarketNameWindow labelUI;

	protected Action onFocusRemovedAction;

	private static float INTERACTION_DISTANCE = 12f;

	private void Start()
	{
		labelUI.SetOnCloseAction(RemoveFocus);
	}

	public void OnLabelClicked()
	{
		clickableCollider.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.RotationLocked = true;
		SingletonBehaviour<PlayerLook>.Instance.LockToTransform(lockTarget, 0.3f, delegate
		{
			labelUI.Open();
		});
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
	}

	public void RemoveFocus()
	{
		SingletonBehaviour<PlayerLook>.Instance.UnlockCamera(0.3f, delegate
		{
			clickableCollider.enabled = true;
		});
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		onFocusRemovedAction?.Invoke();
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
	}

	public override float GetInteractionDistance()
	{
		return INTERACTION_DISTANCE;
	}
}
