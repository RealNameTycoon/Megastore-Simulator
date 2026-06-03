using UnityEngine;

public class SensorManager : MonoBehaviour
{
	private CarAI carAI;

	public string tagName;

	private void Start()
	{
		carAI = base.gameObject.transform.parent.GetComponent<CarAI>();
	}

	private void OnTriggerEnter(Collider car)
	{
		if (car.gameObject.CompareTag(tagName))
		{
			carAI.move = false;
		}
	}

	private void OnTriggerExit(Collider car)
	{
		if (car.gameObject.CompareTag(tagName))
		{
			carAI.move = true;
		}
	}
}
