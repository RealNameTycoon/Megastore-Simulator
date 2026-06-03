using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CarCustomerManager : SingletonBehaviour<CarCustomerManager>
{
	[SerializeField]
	private List<CustomerCar> cars;

	[SerializeField]
	private DOTweenPath pathToParkingLotEntrance;

	[SerializeField]
	private List<ParkingLot> parkingLots;

	[SerializeField]
	private Transform exitFromMegastore;

	[SerializeField]
	private Transform exitToHighway;

	private List<CustomerCar> spawnWaitingCars = new List<CustomerCar>();

	private List<CustomerCar> departureWaitingCars = new List<CustomerCar>();

	private new void Awake()
	{
		base.Awake();
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
	}

	private void OnNewDayStarted()
	{
		for (int i = 0; i < cars.Count; i++)
		{
			if (cars[i].gameObject.activeSelf && cars[i].State != CustomerCar.CustomerCarState.WAITING_FOR_CUSTOMER_SPAWN)
			{
				parkingLots[cars[i].ParkingLotIndex].ReleaseSlot(cars[i]);
				cars[i].Stop();
				cars[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void SpawnCarCustomer()
	{
		MoveRandomCarToEntrance(GetRandomCar());
	}

	private void MoveRandomCarToEntrance(CustomerCar randomCar)
	{
		if (randomCar == null)
		{
			return;
		}
		randomCar.transform.position = pathToParkingLotEntrance.transform.position;
		randomCar.transform.LookAt(pathToParkingLotEntrance.wps[0]);
		int num = Random.Range(0, parkingLots.Count);
		ParkingLot parkingLot = parkingLots[num];
		ParkingLot.ParkingSlot andAllocateNextAvailableSlot = parkingLot.GetAndAllocateNextAvailableSlot(randomCar);
		List<Vector3> list = new List<Vector3>(pathToParkingLotEntrance.wps) { parkingLot.Enterance.transform.position };
		foreach (Transform wayPoint in andAllocateNextAvailableSlot.wayPoints)
		{
			list.Add(wayPoint.position);
		}
		randomCar.SetParkingLotIndex(num);
		randomCar.gameObject.SetActive(value: true);
		randomCar.Move(list, delegate
		{
			randomCar.Stop();
			randomCar.EnableNavMeshObstacle(enable: true);
			if (SingletonBehaviour<CustomerManager>.Instance.TrySpawnCarCustomer(randomCar))
			{
				randomCar.SetState(CustomerCar.CustomerCarState.WAITING_FOR_CUSTOMER_RETURN);
			}
			else
			{
				spawnWaitingCars.Add(randomCar);
				randomCar.SetState(CustomerCar.CustomerCarState.WAITING_FOR_CUSTOMER_SPAWN);
			}
		});
	}

	public bool HasWaitingCar()
	{
		return spawnWaitingCars.Count > 0;
	}

	public void TrySpawnWaitingCar()
	{
		if (HasWaitingCar())
		{
			CustomerCar customerCar = spawnWaitingCars[0];
			spawnWaitingCars.RemoveAt(0);
			customerCar.SetState(CustomerCar.CustomerCarState.WAITING_FOR_CUSTOMER_SPAWN);
			if (SingletonBehaviour<CustomerManager>.Instance.TrySpawnCarCustomer(customerCar))
			{
				customerCar.SetState(CustomerCar.CustomerCarState.WAITING_FOR_CUSTOMER_RETURN);
			}
		}
	}

	private CustomerCar GetRandomCar()
	{
		List<CustomerCar> list = new List<CustomerCar>();
		for (int i = 0; i < cars.Count; i++)
		{
			if (!cars[i].gameObject.activeSelf)
			{
				list.Add(cars[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	public void LeaveParkingLot(CustomerCar car)
	{
		if (!car.gameObject.activeSelf)
		{
			return;
		}
		car.EnableNavMeshObstacle(enable: false);
		car.SetState(CustomerCar.CustomerCarState.WAITING_TO_LEAVE_PARKING_LOT);
		ParkingLot parkingLot = parkingLots[car.ParkingLotIndex];
		List<Vector3> list = new List<Vector3>();
		ParkingLot.ParkingSlot slotByCar = parkingLot.GetSlotByCar(car);
		if (slotByCar == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			list.Add(slotByCar.wayPoints[i].position);
		}
		car.Move(new List<Vector3> { slotByCar.wayPoints[0].position }, delegate
		{
			parkingLots[car.ParkingLotIndex].ReleaseSlot(car);
			car.Move(new List<Vector3>
			{
				parkingLot.Exit.position,
				exitFromMegastore.position,
				exitToHighway.position
			}, delegate
			{
				car.Stop();
				car.gameObject.SetActive(value: false);
			});
		}, reverse: true);
	}
}
