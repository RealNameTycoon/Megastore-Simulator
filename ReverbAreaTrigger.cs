using UnityEngine;

public class ReverbAreaTrigger : MonoBehaviour
{
	[SerializeField]
	private ReverbAreaType areaType;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			SingletonBehaviour<ReverbManager>.Instance.SetArea(areaType);
		}
	}
}
