using UnityEngine;

public class FinishCollision : MonoBehaviour
{
	public GameObject win;

	private void Start()
	{
	}

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.name == "Present")
		{
			win.SetActive(value: true);
		}
	}

	private void Update()
	{
	}
}
