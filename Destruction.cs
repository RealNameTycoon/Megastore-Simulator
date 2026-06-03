using UnityEngine;

public class Destruction : MonoBehaviour
{
	public float minForce;

	public float maxForce;

	public float radius;

	private void Start()
	{
		Explode();
	}

	public void Explode()
	{
		foreach (Transform item in base.transform)
		{
			Rigidbody component = item.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.AddExplosionForce(Random.Range(minForce, maxForce), base.transform.position, radius);
			}
		}
	}
}
