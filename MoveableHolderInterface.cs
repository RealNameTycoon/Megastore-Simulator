using UnityEngine;

public interface MoveableHolderInterface
{
	Moveable GetMoveable();

	BoxCollider SnappableBoxCollider();
}
