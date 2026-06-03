using System;
using System.Collections.Generic;
using UnityEngine;

public class ParkingLot : MonoBehaviour
{
	[Serializable]
	public class ParkingSlot
	{
		public List<Transform> wayPoints;
	}

	[SerializeField]
	private Transform enterance;

	[SerializeField]
	private List<ParkingSlot> parkingSlots;

	[SerializeField]
	private Transform exit;

	private List<CustomerCar> cars = new List<CustomerCar>();

	private int carCount;

	private bool busy;

	public Transform Enterance => enterance;

	public Transform Exit => exit;

	public bool Busy => busy;

	public void SetBusy(bool value)
	{
		busy = value;
	}

	private void Awake()
	{
		for (int i = 0; i < parkingSlots.Count; i++)
		{
			cars.Add(null);
		}
	}

	private bool IsSlotAvailable(int slotIndex)
	{
		return cars[slotIndex] == null;
	}

	public bool HasSpace()
	{
		return carCount < parkingSlots.Count;
	}

	public ParkingSlot GetAndAllocateNextAvailableSlot(CustomerCar car)
	{
		for (int i = 0; i < parkingSlots.Count; i++)
		{
			if (IsSlotAvailable(i))
			{
				cars[i] = car;
				carCount++;
				return parkingSlots[i];
			}
		}
		return null;
	}

	public ParkingSlot GetSlotByCar(CustomerCar car)
	{
		if (!cars.Contains(car))
		{
			return null;
		}
		return parkingSlots[cars.IndexOf(car)];
	}

	public void ReleaseSlot(CustomerCar car)
	{
		int num = cars.IndexOf(car);
		if (num != -1)
		{
			cars[num] = null;
			carCount--;
		}
	}
}
