using UnityEngine;

public class VehicleGroundSnapper : MonoBehaviour
{
	[SerializeField]
	private VehichleInteractable vehicle;

	[SerializeField]
	private Transform snapGroundPosition;

	[SerializeField]
	private Transform forwardPointForRotation;

	[SerializeField]
	private Collider lastHitCollider;

	private const float DEGREE_PER_SECOND = 60f;

	private const float VERTICAL_SPEED = 10f;

	private Vector3 targetEuler;

	private void Update()
	{
		if (forwardPointForRotation != null && vehicle.IsDriving())
		{
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			float x = Mathf.MoveTowardsAngle(maxDelta: 60f * Time.deltaTime, current: localEulerAngles.x, target: targetEuler.x);
			base.transform.localEulerAngles = new Vector3(x, targetEuler.y, targetEuler.z);
		}
	}

	private void FixedUpdate()
	{
		if (!vehicle.IsDriving())
		{
			return;
		}
		int layerMask = RayShooter.GroundLayerMask;
		if (!Physics.Raycast(snapGroundPosition.position, Vector3.down, out var hitInfo, float.PositiveInfinity, layerMask))
		{
			return;
		}
		lastHitCollider = hitInfo.collider;
		Vector3 point = hitInfo.point;
		Vector3 position = base.transform.position;
		Vector3 b = new Vector3(position.x, point.y, position.z);
		base.transform.position = Vector3.Lerp(base.transform.position, b, Time.fixedDeltaTime * 10f);
		if (forwardPointForRotation != null && Physics.Raycast(forwardPointForRotation.position, Vector3.down, out var hitInfo2, float.PositiveInfinity, layerMask) && forwardPointForRotation != null)
		{
			float y = hitInfo2.point.y;
			float y2 = hitInfo.point.y;
			Vector3 a = new Vector3(hitInfo2.point.x, 0f, hitInfo2.point.z);
			Vector3 b2 = new Vector3(hitInfo.point.x, 0f, hitInfo.point.z);
			float x = Mathf.Max(Vector3.Distance(a, b2), 0.001f);
			float num = 57.29578f * Mathf.Atan2(y - y2, x);
			if (!(Vector3.Angle(hitInfo2.normal, Vector3.up) <= 1f) && Mathf.Abs(num) < 50f)
			{
				targetEuler = new Vector3(0f - num, base.transform.localEulerAngles.y, base.transform.localEulerAngles.z);
			}
			else
			{
				targetEuler = new Vector3(0f, base.transform.localEulerAngles.y, base.transform.localEulerAngles.z);
			}
		}
	}
}
