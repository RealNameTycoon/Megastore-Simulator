using UnityEngine;

public class RotateCamera : MonoBehaviour
{
	private float MouseRotateSpeed = 80f;

	private void Start()
	{
	}

	private void Update()
	{
		float yAngle = Input.GetAxis("Mouse X") * MouseRotateSpeed * Time.deltaTime;
		float num = Input.GetAxis("Mouse Y") * MouseRotateSpeed * Time.deltaTime;
		base.transform.Rotate(0f - num, yAngle, 0f);
		Vector3 eulerAngles = Camera.main.transform.eulerAngles;
		base.transform.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, 0f);
	}
}
