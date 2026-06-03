using System.Collections;
using UnityEngine;

public class ProducePlacebleManager : MonoBehaviour
{
	private WaitForSeconds waterSprayInterval = new WaitForSeconds(45f);

	private WaitForSeconds waterSprayDuration = new WaitForSeconds(3.7f);

	private void Start()
	{
		StartCoroutine(WaterSprayRoutine());
	}

	private IEnumerator WaterSprayRoutine()
	{
		while (true)
		{
			yield return waterSprayInterval;
			EventManager.NotifyEvent(PlaceableEvents.WATER_SPRAY_STARTED);
			yield return waterSprayDuration;
			EventManager.NotifyEvent(PlaceableEvents.WATER_SPRAY_ENDED);
		}
	}
}
