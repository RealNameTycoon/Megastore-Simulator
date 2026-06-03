using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CarManager : MonoBehaviour
{
	[SerializeField]
	private List<CarAI> cars;

	[SerializeField]
	private List<DOTweenPath> paths;

	[SerializeField]
	private List<CarAI> fastCars;

	[SerializeField]
	private List<DOTweenPath> fastPaths;

	private PathFollowerCar activeCar;

	private CarAI car;

	private void Start()
	{
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
		EventManager.AddListener(GameEvents.ROBERY_STARTED, delegate
		{
			if (car != null)
			{
				car.Stop();
				car.gameObject.SetActive(value: false);
			}
		});
		StartCoroutine(InitializationRoutine());
		StartCoroutine(FastCarsSpawnRoutine());
	}

	private IEnumerator InitializationRoutine()
	{
		MoveRandomCar(GetRandomCar());
		yield return new WaitForSeconds(12f);
		MoveRandomCar(GetRandomCar());
	}

	private IEnumerator FastCarsSpawnRoutine()
	{
		while (true)
		{
			MoveRandomFastCar(GetRandomFastCar());
			yield return new WaitForSeconds(Random.Range(0.75f, 3f));
		}
	}

	private void MoveRandomCar(CarAI randomCar)
	{
		DOTweenPath randomElement = paths.GetRandomElement();
		randomCar.transform.position = randomElement.transform.position;
		randomCar.transform.LookAt(randomElement.wps[0]);
		randomCar.gameObject.SetActive(value: true);
		randomCar.Move(randomElement.wps, delegate
		{
			randomCar.Stop();
			randomCar.gameObject.SetActive(value: false);
			MoveRandomCar(GetRandomCar());
		});
	}

	private void MoveRandomFastCar(CarAI randomCar)
	{
		if (!(randomCar == null))
		{
			DOTweenPath randomElement = fastPaths.GetRandomElement();
			randomCar.transform.position = randomElement.transform.position;
			randomCar.transform.LookAt(randomElement.wps[0]);
			randomCar.gameObject.SetActive(value: true);
			randomCar.Move(randomElement.wps, delegate
			{
				randomCar.Stop();
				randomCar.gameObject.SetActive(value: false);
			});
		}
	}

	private CarAI GetRandomFastCar()
	{
		List<CarAI> list = new List<CarAI>();
		for (int i = 0; i < fastCars.Count; i++)
		{
			if (!fastCars[i].gameObject.activeSelf)
			{
				list.Add(fastCars[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	private CarAI GetRandomCar()
	{
		List<CarAI> list = new List<CarAI>();
		for (int i = 0; i < cars.Count; i++)
		{
			if (!cars[i].gameObject.activeSelf)
			{
				list.Add(cars[i]);
			}
		}
		return list.GetRandomElement();
	}

	private void StopAndMove()
	{
	}

	private void OnNewDayStarted()
	{
		for (int i = 0; i < cars.Count; i++)
		{
			if (cars[i].gameObject.activeSelf)
			{
				cars[i].Stop();
				cars[i].gameObject.SetActive(value: false);
			}
		}
		StartCoroutine(InitializationRoutine());
	}
}
