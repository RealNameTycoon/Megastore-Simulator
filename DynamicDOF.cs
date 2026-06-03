using UnityEngine;

public class DynamicDOF : MonoBehaviour
{
	public Transform origin;

	public GameObject target;

	private void Start()
	{
	}

	private void Update()
	{
		Ray ray = new Ray(origin.transform.position, origin.transform.forward);
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity))
		{
			Debug.DrawRay(origin.transform.position, origin.transform.forward, Color.cyan);
			target.transform.position = hitInfo.point;
		}
		else
		{
			Debug.DrawRay(origin.transform.position, origin.transform.forward, Color.cyan);
		}
	}
}
