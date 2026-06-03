using UnityEngine;

public class EscalatorRider : MonoBehaviour
{
	public virtual void OnDifferentFloorReached()
	{
	}

	public virtual bool ShoulOpenTheDoor(AutoDoor door)
	{
		return true;
	}

	public virtual bool ShouldCloseTheDoor(AutoDoor door)
	{
		return true;
	}
}
