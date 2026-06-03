using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PathFollowerCar : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> carBodies;

	[SerializeField]
	private DOTweenPath path;

	private GameObject activeCar;

	public void Activate()
	{
		activeCar = carBodies.GetRandomElement();
		activeCar.SetActive(value: true);
		path.DORestart();
	}

	public void OnPathComplete()
	{
		if (activeCar != null)
		{
			activeCar.SetActive(value: false);
			activeCar = null;
		}
		EventManager.NotifyEvent(GameEvents.CAR_PATH_FINISHED);
	}

	public void Deactivate()
	{
		if (activeCar != null)
		{
			activeCar.SetActive(value: false);
			activeCar = null;
			path.DOPause();
		}
	}
}
