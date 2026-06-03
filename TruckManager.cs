using System;
using UnityEngine;

public class TruckManager : SingletonBehaviour<TruckManager>
{
	[Serializable]
	public class TruckDictionary : UnitySerializedDictionary<OrderManager.OrderReceivingArea, Truck>
	{
	}

	[SerializeField]
	private TruckDictionary truckDictionary;

	public Truck GetTruck(OrderManager.OrderReceivingArea area)
	{
		return truckDictionary[area];
	}
}
