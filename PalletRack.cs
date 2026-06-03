using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PalletRack : Furniture
{
	[SerializeField]
	private List<PalletShelf> palletShelves;

	[SerializeField]
	private List<GameObject> labelDigitsFirst;

	[SerializeField]
	private List<GameObject> labelDigitsSecond;

	[SerializeField]
	private Clickable labelClickable1;

	[SerializeField]
	private Clickable labelClickable2;

	[SerializeField]
	private TextMeshPro rackLabel01;

	[SerializeField]
	private TextMeshPro rackLabel02;

	private const string RACK_LABEL_KEY = "RACK_LABEL_KEY";

	private TemperatureZone temperatureZone;

	private string GetRackLabelKey()
	{
		if (base.Type == FurnitureType.SMALL_PALLET_RACK)
		{
			return base.Type.ToString() + "RACK_LABEL_KEY" + base.FurnitureID;
		}
		return "RACK_LABEL_KEY" + base.FurnitureID;
	}

	private void Start()
	{
		labelClickable1.onClickAction.AddListener(OnLabelClicked);
		labelClickable2.onClickAction.AddListener(OnLabelClicked);
	}

	private void OnLabelClicked()
	{
		SingletonWindow<EditLabelWindow>.Instance.Open(this);
	}

	public string GetRackLabel()
	{
		return GenericDataSerializer.LoadString(GetRackLabelKey(), GetDisplayedID().ToString("00"));
	}

	public void SetRackLabel(string label)
	{
		GenericDataSerializer.Save(GetRackLabelKey(), label);
		rackLabel01.text = label;
		rackLabel02.text = label;
	}

	public override void InitializeOldFurniture(int id, bool isPacked = false)
	{
		base.InitializeOldFurniture(id, isPacked);
		if (isPacked)
		{
			int lowestAvailableDisplayedID = SingletonBehaviour<SpawnManager>.Instance.GetLowestAvailableDisplayedID(base.Type);
			SetDisplayedID(lowestAvailableDisplayedID);
		}
		string rackLabel = GetRackLabel();
		rackLabel01.text = rackLabel;
		rackLabel02.text = rackLabel;
		EventManager.AddListener(StartupEvents.TEMPERATURE_ZONES_INITIALIZED, CheckAndUpdateTemperatureZone);
		for (int i = 0; i < palletShelves.Count; i++)
		{
			palletShelves[i].Initialize();
		}
	}

	private void CheckAndUpdateTemperatureZone()
	{
		TemperatureZone temperatureZoneAtPosition = SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(base.transform.position);
		if (temperatureZone != temperatureZoneAtPosition)
		{
			if (temperatureZone != null)
			{
				temperatureZone.UnregisterPalletRack(this);
			}
			temperatureZone = temperatureZoneAtPosition;
			temperatureZoneAtPosition.RegisterPalletRack(this);
			base.transform.SetParent(temperatureZone.transform);
		}
	}

	public override void InitializeNewFurniture(int id)
	{
		base.InitializeNewFurniture(id);
		int lowestAvailableDisplayedID = SingletonBehaviour<SpawnManager>.Instance.GetLowestAvailableDisplayedID(base.Type);
		SetDisplayedID(lowestAvailableDisplayedID);
		string rackLabel = GetRackLabel();
		rackLabel01.text = rackLabel;
		rackLabel02.text = rackLabel;
		for (int i = 0; i < palletShelves.Count; i++)
		{
			palletShelves[i].Initialize();
		}
	}

	public override void SetFloorLayers()
	{
		LayerMask placeableFloorLayers = 1 << PlacementManager.STORAGE_FLOOR_LAYER;
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
	}

	public override void OnPlacementEnded()
	{
		base.OnPlacementEnded();
		CheckAndUpdateTemperatureZone();
		for (int i = 0; i < palletShelves.Count; i++)
		{
			palletShelves[i].OnParentRepositioned();
		}
	}

	public override bool CanPack()
	{
		bool flag = true;
		for (int i = 0; i < palletShelves.Count; i++)
		{
			if (palletShelves[i].ContainedPalletID != -1)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_remove_pallet", base.transform);
		}
		return flag;
	}

	public override List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		if (SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
		{
			List<(KeyCode, (string, Action))> list = new List<(KeyCode, (string, Action))>();
			list.Add((KeyCode.F, ("pack", delegate
			{
				Pack();
			})));
			if (type == FurnitureType.PALLET_RACK)
			{
				list.Add((KeyCode.C, ("convert_to_compact_rack", delegate
				{
					ConvertToCompactRack();
				})));
			}
			return list;
		}
		return null;
	}

	private void ConvertToCompactRack()
	{
		bool flag = true;
		int num = 0;
		for (int i = 0; i < palletShelves.Count; i++)
		{
			if (palletShelves[i].IsReservedToStaff() || palletShelves[i].IsContainerReservedToStaff())
			{
				flag = false;
			}
			if (palletShelves[i].ContainedPalletID != -1)
			{
				num++;
			}
		}
		if (!flag)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_convert_to_compact_rack_reserved", base.transform);
			return;
		}
		if (num > 6)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_convert_to_compact_rack_max_pallets", base.transform);
			return;
		}
		List<Pallet> list = new List<Pallet>();
		for (int j = 0; j < palletShelves.Count; j++)
		{
			if (palletShelves[j].ContainedPalletID != -1)
			{
				Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(palletShelves[j].ContainedPalletID);
				if (pallet != null)
				{
					list.Add(pallet);
				}
			}
			palletShelves[j].RemovePallet();
			palletShelves[j].OnPacked();
		}
		SingletonBehaviour<SpawnManager>.Instance.PackFurnitureWithoutBox(this);
		Furniture furniture = SingletonBehaviour<FurniturePool>.Instance.GetFurniture(FurnitureType.SMALL_PALLET_RACK);
		furniture.transform.position = base.transform.position;
		furniture.transform.eulerAngles = base.transform.eulerAngles;
		furniture.SetFloorLayers();
		int furnitureCount = SingletonBehaviour<SpawnManager>.Instance.GetFurnitureCount(FurnitureType.SMALL_PALLET_RACK);
		furniture.InitializeNewFurniture(furnitureCount);
		SingletonBehaviour<SpawnManager>.Instance.AddNewFurniture(furniture);
		EventManager.NotifyEvent(PlaceableEvents.NEW_FURNITURE_PLACED, furniture.Type);
		PalletRack palletRack = furniture as PalletRack;
		for (int k = 0; k < list.Count; k++)
		{
			palletRack.palletShelves[k].PlacePallet(list[k]);
		}
		palletRack.SavePosition();
		float param = SingletonBehaviour<ProductPool>.Instance.GetFurnitureData(FurnitureType.PALLET_RACK).cost - SingletonBehaviour<ProductPool>.Instance.GetFurnitureData(FurnitureType.SMALL_PALLET_RACK).cost;
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, param);
	}

	public override void Pack()
	{
		if (CanPack())
		{
			for (int i = 0; i < palletShelves.Count; i++)
			{
				palletShelves[i].OnPacked();
			}
			SingletonBehaviour<SpawnManager>.Instance.PackFurniture(this);
		}
	}
}
