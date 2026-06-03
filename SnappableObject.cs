using UnityEngine;

public class SnappableObject : Interactable, MoveableHolderInterface
{
	[SerializeField]
	private Moveable parentMoveable;

	[SerializeField]
	private BoxCollider snappableBoxCollider;

	public Moveable GetMoveable()
	{
		return parentMoveable;
	}

	public BoxCollider SnappableBoxCollider()
	{
		return snappableBoxCollider;
	}
}
