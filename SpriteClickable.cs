using UnityEngine;

public class SpriteClickable : Clickable
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	protected override void EnableOutline(bool state)
	{
		spriteRenderer.enabled = state;
	}

	protected override bool IsOutlineAssigned()
	{
		return spriteRenderer != null;
	}

	protected override bool IsOutlineEnabled()
	{
		return spriteRenderer.enabled;
	}

	public override float GetInteractionDistance()
	{
		return 8f;
	}
}
