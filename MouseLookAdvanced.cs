using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLookAdvanced : MonoBehaviour
{
	public float sensitivityX = 5f;

	public float sensitivityY = 5f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -90f;

	public float maximumY = 90f;

	public float smoothSpeed = 20f;

	private float verticalAcceleration;

	private float rotationX;

	private float smoothRotationX;

	private float rotationY;

	private float smoothRotationY;

	private Vector3 vMousePos;

	public float Speed = 100f;

	private void Start()
	{
		rotationY = 0f - base.transform.localEulerAngles.x;
		rotationX = base.transform.localEulerAngles.y;
		smoothRotationX = base.transform.localEulerAngles.y;
		smoothRotationY = 0f - base.transform.localEulerAngles.x;
	}

	private void Update()
	{
		verticalAcceleration = 0f;
		if (Input.GetMouseButtonDown(1))
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			Cursor.visible = !Cursor.visible;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			verticalAcceleration = 1f;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			verticalAcceleration = -1f;
		}
		if (Cursor.lockState == CursorLockMode.Locked)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			smoothRotationX += (rotationX - smoothRotationX) * smoothSpeed * Time.smoothDeltaTime;
			smoothRotationY += (rotationY - smoothRotationY) * smoothSpeed * Time.smoothDeltaTime;
			base.transform.localEulerAngles = new Vector3(0f - smoothRotationY, smoothRotationX, 0f);
			Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			Vector3 vector2 = base.transform.rotation * vector;
			base.transform.position += vector2 * Speed * Time.smoothDeltaTime;
			base.transform.position += new Vector3(0f, Speed / 2f * verticalAcceleration * Time.smoothDeltaTime, 0f);
			base.transform.position += base.transform.rotation * Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * 200f;
		}
	}
}
