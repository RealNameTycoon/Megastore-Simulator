using System;
using System.Collections.Generic;
using UnityEngine;

public class BoxShelf : Furniture
{
	[SerializeField]
	private List<BoxStorageUnit> boxStorageUnits;

	private TemperatureZone temperatureZone;

	public override void InitializeOldFurniture(int id, bool isPacked = false)
	{
		base.InitializeOldFurniture(id, isPacked);
		EventManager.AddListener(StartupEvents.TEMPERATURE_ZONES_INITIALIZED, CheckAndUpdateTemperatureZone);
		if (isPacked)
		{
			int lowestAvailableDisplayedID = SingletonBehaviour<SpawnManager>.Instance.GetLowestAvailableDisplayedID(base.Type);
			SetDisplayedID(lowestAvailableDisplayedID);
		}
		for (int i = 0; i < boxStorageUnits.Count; i++)
		{
			boxStorageUnits[i].InitializeOldBoxStorageUnit();
		}
	}

	private void CheckAndUpdateTemperatureZone()
	{
		TemperatureZone temperatureZoneAtPosition = SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(base.transform.position);
		if (temperatureZone != temperatureZoneAtPosition)
		{
			if (temperatureZone != null)
			{
				temperatureZone.UnregisterBoxShelf(this);
			}
			temperatureZone = temperatureZoneAtPosition;
			temperatureZoneAtPosition.RegisterBoxShelf(this);
			base.transform.SetParent(temperatureZone.transform);
		}
	}

	public override void InitializeNewFurniture(int id)
	{
		base.InitializeNewFurniture(id);
		int lowestAvailableDisplayedID = SingletonBehaviour<SpawnManager>.Instance.GetLowestAvailableDisplayedID(base.Type);
		SetDisplayedID(lowestAvailableDisplayedID);
		for (int i = 0; i < boxStorageUnits.Count; i++)
		{
			boxStorageUnits[i].InitializeNewBoxStorageUnit();
		}
	}

	public override void SetFloorLayers()
	{
		LayerMask placeableFloorLayers = 1 << PlacementManager.STORAGE_FLOOR_LAYER;
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
	}

	public override bool CanPack()
	{
		bool flag = true;
		for (int i = 0; i < boxStorageUnits.Count; i++)
		{
			if (!boxStorageUnits[i].isEmpty())
			{
				flag = false;
			}
		}
		if (!flag)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_storage_unit_not_empty", base.transform);
		}
		return flag;
	}

	public override List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		if (SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
		{
			return new List<(KeyCode, (string, Action))> { (KeyCode.F, ("pack", delegate
			{
				Pack();
			})) };
		}
		return null;
	}

	public override void Pack()
	{
		if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && CanPack())
		{
			for (int i = 0; i < boxStorageUnits.Count; i++)
			{
				boxStorageUnits[i].OnPacked();
			}
			SingletonBehaviour<SpawnManager>.Instance.PackFurniture(this);
		}
	}

	public override void OnPlacementEnded()
	{
		base.OnPlacementEnded();
		CheckAndUpdateTemperatureZone();
		for (int i = 0; i < boxStorageUnits.Count; i++)
		{
			boxStorageUnits[i].CheckAndUpdateTemperatureZone();
		}
	}
}
