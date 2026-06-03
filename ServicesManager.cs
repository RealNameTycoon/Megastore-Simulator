using UnityEngine;

public class ServicesManager : MonoBehaviour
{
	public void CollectAllEmptyBoxes()
	{
		EventLogger.LogEvent("c_service_garbage_collected");
		SingletonBehaviour<BoxManager>.Instance.ClearAllEmptyBoxes();
		SingletonBehaviour<MiddleTooltipUI>.Instance.Open("empty_box_tooltip");
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
