using UnityEngine;

public class RotateMoveCamera : MonoBehaviour
{
	public GameObject Camera;

	public float minX = -360f;

	public float maxX = 360f;

	public float minY = -45f;

	public float maxY = 45f;

	public float sensX = 100f;

	public float sensY = 100f;

	private float rotationY;

	private float rotationX;

	private float MouseX;

	private float MouseY;

	private void Update()
	{
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		if (axis != MouseX || axis2 != MouseY)
		{
			rotationX += axis * sensX * Time.deltaTime;
			rotationY += axis2 * sensY * Time.deltaTime;
			rotationY = Mathf.Clamp(rotationY, minY, maxY);
			MouseX = axis;
			MouseY = axis2;
			Camera.transform.localEulerAngles = new Vector3(0f - rotationY, rotationX, 0f);
		}
		if (Input.GetKey(KeyCode.W))
		{
			base.transform.Translate(new Vector3(0f, 0f, 0.1f));
		}
		else if (Input.GetKey(KeyCode.S))
		{
			base.transform.Translate(new Vector3(0f, 0f, -0.1f));
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.Translate(new Vector3(0.1f, 0f, 0f));
		}
		else if (Input.GetKey(KeyCode.A))
		{
			base.transform.Translate(new Vector3(-0.1f, 0f, 0f));
		}
	}
}
