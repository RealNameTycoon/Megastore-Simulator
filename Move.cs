using UnityEngine;

public class Move : MonoBehaviour
{
	public float bias = 0.2f;

	public float speed = 0.5f;

	public string state = "down";

	public Vector3 newPosition;

	private void Start()
	{
		newPosition = base.transform.position;
	}

	public void MoveUp()
	{
		if (state == "down")
		{
			newPosition = base.transform.position;
			newPosition.y += bias;
			state = "up";
		}
	}

	public void MoveDown()
	{
		if (state == "up")
		{
			newPosition = base.transform.position;
			newPosition.y -= bias;
			state = "down";
		}
	}

	private void Update()
	{
		float maxDistanceDelta = speed * Time.deltaTime;
		base.transform.position = Vector3.MoveTowards(base.transform.position, newPosition, maxDistanceDelta);
	}
}
