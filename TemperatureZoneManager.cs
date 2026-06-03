using System;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureZoneManager : SingletonBehaviour<TemperatureZoneManager>
{
	[SerializeField]
	private List<TemperatureZone> temperatureZones;

	[SerializeField]
	private TemperatureZone roomTemperatureZone;

	[SerializeField]
	private TemperatureZone truckZone;

	private static float ROOM_TEMPERATURE = 25f;

	private new void Awake()
	{
		base.Awake();
		roomTemperatureZone.SetTemperature(ROOM_TEMPERATURE);
	}

	private void Start()
	{
		TimeManager.OnMinPassed = (Action)Delegate.Combine(TimeManager.OnMinPassed, new Action(TemperatureTick));
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, NewDayStarted);
	}

	private void NewDayStarted()
	{
		foreach (TemperatureZone temperatureZone in temperatureZones)
		{
			temperatureZone.TemperatureTick(480);
		}
		roomTemperatureZone.TemperatureTick(480);
	}

	private void OnDestroy()
	{
		TimeManager.OnMinPassed = (Action)Delegate.Remove(TimeManager.OnMinPassed, new Action(TemperatureTick));
	}

	private void TemperatureTick()
	{
		foreach (TemperatureZone temperatureZone in temperatureZones)
		{
			temperatureZone.TemperatureTick();
		}
		roomTemperatureZone.TemperatureTick();
	}

	public void RegisterTemperatureZone(TemperatureZone temperatureZone)
	{
		temperatureZones.Add(temperatureZone);
	}

	public void UnregisterTemperatureZone(TemperatureZone temperatureZone)
	{
		temperatureZones.Remove(temperatureZone);
	}

	public TemperatureZone GetTemperatureZoneAtPosition(Vector3 position)
	{
		foreach (TemperatureZone temperatureZone in temperatureZones)
		{
			if (IsInZone(temperatureZone, position))
			{
				return temperatureZone;
			}
		}
		if (IsInZone(truckZone, position))
		{
			return truckZone;
		}
		return roomTemperatureZone;
	}

	public bool IsRoomTemperatureZone(TemperatureZone temperatureZone)
	{
		return temperatureZone == roomTemperatureZone;
	}

	private bool IsInZone(TemperatureZone temperatureZone, Vector3 position)
	{
		float num = Mathf.Min(temperatureZone.Corner1.position.x, temperatureZone.Corner2.position.x);
		float num2 = Mathf.Max(temperatureZone.Corner1.position.x, temperatureZone.Corner2.position.x);
		float num3 = Mathf.Min(temperatureZone.Corner1.position.z, temperatureZone.Corner2.position.z);
		float num4 = Mathf.Max(temperatureZone.Corner1.position.z, temperatureZone.Corner2.position.z);
		if (position.x >= num && position.x <= num2 && position.z >= num3 && position.z <= num4)
		{
			return true;
		}
		return false;
	}
}
