using UnityEngine;
using UnityEngine.AI;

public class CustomerCar : CarAI
{
	public enum CustomerCarState
	{
		NONE = -1,
		WAITING_FOR_CUSTOMER_SPAWN,
		WAITING_FOR_CUSTOMER_RETURN,
		WAITING_TO_LEAVE_PARKING_LOT,
		LEAVING_PARKING_LOT
	}

	[SerializeField]
	private Transform driverDoorFront;

	[SerializeField]
	private NavMeshObstacle navMeshObstacle;

	private int parkingLotIndex = -1;

	private CustomerCarState state = CustomerCarState.NONE;

	public int ParkingLotIndex => parkingLotIndex;

	public Transform DriverDoorFront => driverDoorFront;

	public CustomerCarState State => state;

	public void SetParkingLotIndex(int index)
	{
		parkingLotIndex = index;
	}

	public void SetState(CustomerCarState newState)
	{
		state = newState;
	}

	public void EnableNavMeshObstacle(bool enable)
	{
	}

	private void GetOverlapBox(out Vector3 center, out Vector3 halfExtents, out Quaternion rot)
	{
		center = navMeshObstacle.transform.TransformPoint(navMeshObstacle.center);
		halfExtents = 1.32f * base.transform.lossyScale.x * navMeshObstacle.size / 2f;
		halfExtents.z /= 1.2f;
		rot = navMeshObstacle.transform.rotation;
	}

	private void DrawGizmo()
	{
		if ((bool)navMeshObstacle)
		{
			GetOverlapBox(out var center, out var halfExtents, out var rot);
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(center, rot, Vector3.one);
			Gizmos.color = Color.cyan;
			Gizmos.DrawCube(Vector3.zero, halfExtents * 2f);
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
			Gizmos.matrix = matrix;
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(center, 0.05f);
		}
	}

	private void OnDrawGizmos()
	{
		DrawGizmo();
	}
}
