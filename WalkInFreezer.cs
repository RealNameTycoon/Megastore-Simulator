using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WalkInFreezer : Furniture
{
	[SerializeField]
	private TemperatureZone temperatureZone;

	[SerializeField]
	private WalkInFreezerUIClickable walkInFreezerUIClickable;

	[SerializeField]
	private WalkInFreezerUI walkInFreezerUI;

	[SerializeField]
	private AudioSource ambienceSource;

	[SerializeField]
	private List<MoveableClickable> frontClickables;

	private static string TEMPERATURE_KEY = "temperature";

	private float temperature = 6f;

	private const float MAX_TEMPERATURE = 12f;

	private const float MIN_TEMPERATURE = -18f;

	private float initialAmbienceVolume;

	private void Awake()
	{
		List<(KeyCode, (string, Action))> list = new List<(KeyCode, (string, Action))>();
		list.Add((KeyCode.Mouse1, ("move", delegate
		{
			TryMove();
		})));
		list.Add((KeyCode.F, ("pack", delegate
		{
			Pack();
		})));
		walkInFreezerUIClickable.SetAdditionalActions(list);
		initialAmbienceVolume = ambienceSource.volume;
		for (int num = 0; num < frontClickables.Count; num++)
		{
			frontClickables[num].SetAdditionalActions(list);
		}
	}

	public override void InitializeOldFurniture(int id, bool isPacked = false)
	{
		base.InitializeOldFurniture(id, isPacked);
		temperature = GenericDataSerializer.LoadFloat(type.ToString() + base.FurnitureID + "|" + TEMPERATURE_KEY);
		walkInFreezerUI.SetTemperature(temperature);
		temperatureZone.SetTemperature(temperature);
		SingletonBehaviour<TemperatureZoneManager>.Instance.RegisterTemperatureZone(temperatureZone);
	}

	public override void InitializeNewFurniture(int id)
	{
		base.InitializeNewFurniture(id);
		SingletonBehaviour<TemperatureZoneManager>.Instance.RegisterTemperatureZone(temperatureZone);
		temperatureZone.SetTemperature(temperature);
		walkInFreezerUI.SetTemperature(temperature);
	}

	public void IncreaseTemperature()
	{
		temperature += 1f;
		temperature = Mathf.Clamp(temperature, -18f, 12f);
		GenericDataSerializer.SaveFloat(type.ToString() + base.FurnitureID + "|" + TEMPERATURE_KEY, temperature);
		walkInFreezerUI.SetTemperature(temperature);
		temperatureZone.SetTemperature(temperature);
	}

	public void DecreaseTemperature()
	{
		temperature -= 1f;
		temperature = Mathf.Clamp(temperature, -18f, 12f);
		GenericDataSerializer.SaveFloat(type.ToString() + base.FurnitureID + "|" + TEMPERATURE_KEY, temperature);
		walkInFreezerUI.SetTemperature(temperature);
		temperatureZone.SetTemperature(temperature);
	}

	private void TryMove()
	{
		StartNewPlacement();
	}

	public override void Pack()
	{
		base.Pack();
		SingletonBehaviour<TemperatureZoneManager>.Instance.UnregisterTemperatureZone(temperatureZone);
	}

	public override bool CanPack()
	{
		int num;
		if (!HasBoxesInTemperatureZone())
		{
			num = ((!HasPlaceablesInside()) ? 1 : 0);
			if (num != 0)
			{
				goto IL_002c;
			}
		}
		else
		{
			num = 0;
		}
		SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pack_error_walkin_freezer", base.transform);
		goto IL_002c;
		IL_002c:
		return (byte)num != 0;
	}

	private bool HasBoxesInTemperatureZone()
	{
		return temperatureZone.HasBoxes();
	}

	private bool HasPlaceablesInside()
	{
		return temperatureZone.HasPlaceablesInside();
	}

	public override void SetFloorLayers()
	{
		LayerMask placeableFloorLayers = 1 << PlacementManager.STORAGE_FLOOR_LAYER;
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
	}

	public override void OnPlacementEnded()
	{
		base.OnPlacementEnded();
		temperatureZone.OnPositionChanged();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(AutoDoor.PlayerTag))
		{
			ambienceSource.DOKill();
			ambienceSource.Play();
			ambienceSource.DOFade(initialAmbienceVolume, 1f);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag(AutoDoor.PlayerTag))
		{
			ambienceSource.DOKill();
			ambienceSource.DOFade(0f, 1f).OnComplete(delegate
			{
				ambienceSource.Stop();
			});
		}
	}

	public override float GetPlacementRadius()
	{
		return PlacementManager.BIG_ITEM_PLACEMENT_RADIUS;
	}
}
