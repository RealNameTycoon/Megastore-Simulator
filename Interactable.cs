using UnityEngine;

public class Interactable : Hoverable
{
	public virtual void OnMouseButtonDown()
	{
	}

	public virtual void OnMouseButtonUp()
	{
	}

	public virtual void OnMouseHoverStarted()
	{
	}

	public virtual void OnMouseHoverEnded()
	{
	}

	public virtual void OnMouseRMBDown()
	{
	}

	public virtual float GetInteractionDistance()
	{
		return RayShooter.DEFAULT_INTERACTION_DETECTION_DISTANCE;
	}

	public virtual LayerMask GetInteractableLayers()
	{
		return 0;
	}
}
