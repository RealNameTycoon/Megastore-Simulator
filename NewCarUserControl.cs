using UnityEngine;

[RequireComponent(typeof(NewCarController))]
public class NewCarUserControl : MonoBehaviour
{
	private NewCarController m_Car;

	private void Awake()
	{
		m_Car = GetComponent<NewCarController>();
	}

	private void FixedUpdate()
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		float axis3 = Input.GetAxis("Jump");
		m_Car.Move(axis, axis2, axis2, axis3);
	}
}
