using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BakerManager : SingletonBehaviour<BakerManager>
{
	[SerializeField]
	private SerializedDictionary<ProductType, List<TrayShelf>> ProductTypeToTrayShelvesMap = new SerializedDictionary<ProductType, List<TrayShelf>>();

	[SerializeField]
	private SerializedDictionary<ProductType, List<TrayShelf>> CookedProductTypeToTrayShelvesMap = new SerializedDictionary<ProductType, List<TrayShelf>>();

	private HashSet<TrayShelf> registeredTrayShelves = new HashSet<TrayShelf>();

	private HashSet<TrayShelf> registeredCookedTrayShelves = new HashSet<TrayShelf>();

	[SerializeField]
	private List<Oven> registeredOvens = new List<Oven>();

	private new void Awake()
	{
		base.Awake();
	}

	public void OnCooked(TrayShelf trayShelf)
	{
		if (trayShelf.ContainedTrayID != -1 && trayShelf.ContainedTray.IsCooked && trayShelf.ContainedProductType() != ProductType.NONE && !registeredCookedTrayShelves.Contains(trayShelf))
		{
			registeredCookedTrayShelves.Add(trayShelf);
			if (!CookedProductTypeToTrayShelvesMap.ContainsKey(trayShelf.ContainedTray.ContainedType))
			{
				CookedProductTypeToTrayShelvesMap.Add(trayShelf.ContainedTray.ContainedType, new List<TrayShelf>());
			}
			CookedProductTypeToTrayShelvesMap[trayShelf.ContainedTray.ContainedType].Add(trayShelf);
			if (registeredTrayShelves.Contains(trayShelf))
			{
				registeredTrayShelves.Remove(trayShelf);
				ProductTypeToTrayShelvesMap[trayShelf.ContainedProductType()].Remove(trayShelf);
			}
			EventManager.NotifyEvent(BakerEvents.COOKED_TRAY_REGISTERED);
		}
	}

	public void RegisterTrayShelf(TrayShelf trayShelf)
	{
		if (trayShelf.ContainedTray != null && trayShelf.ContainedTray.IsCooked)
		{
			if (!registeredCookedTrayShelves.Contains(trayShelf))
			{
				registeredCookedTrayShelves.Add(trayShelf);
				if (!CookedProductTypeToTrayShelvesMap.ContainsKey(trayShelf.ContainedTray.ContainedType))
				{
					CookedProductTypeToTrayShelvesMap.Add(trayShelf.ContainedTray.ContainedType, new List<TrayShelf>());
				}
				CookedProductTypeToTrayShelvesMap[trayShelf.ContainedTray.ContainedType].Add(trayShelf);
				EventManager.NotifyEvent(BakerEvents.COOKED_TRAY_REGISTERED);
			}
		}
		else if (trayShelf.ParentFurniture.Type != FurnitureType.OVEN && trayShelf.ContainedProductType() != ProductType.NONE && !registeredTrayShelves.Contains(trayShelf))
		{
			registeredTrayShelves.Add(trayShelf);
			if (!ProductTypeToTrayShelvesMap.ContainsKey(trayShelf.ContainedProductType()))
			{
				ProductTypeToTrayShelvesMap.Add(trayShelf.ContainedProductType(), new List<TrayShelf>());
			}
			ProductTypeToTrayShelvesMap[trayShelf.ContainedProductType()].Add(trayShelf);
			EventManager.NotifyEvent(BakerEvents.TRAY_REGISTERED);
		}
	}

	public void UnregisterTrayShelf(TrayShelf trayShelf, ProductType productType)
	{
		if (trayShelf.ContainedTray != null && trayShelf.ContainedTray.IsCooked)
		{
			registeredCookedTrayShelves.Remove(trayShelf);
			CookedProductTypeToTrayShelvesMap[trayShelf.ContainedTray.ContainedType].Remove(trayShelf);
		}
		else if (trayShelf.ParentFurniture.Type != FurnitureType.OVEN)
		{
			registeredTrayShelves.Remove(trayShelf);
			ProductTypeToTrayShelvesMap[productType].Remove(trayShelf);
		}
	}

	public bool HasTrayShelfToReserve(ProductType productType)
	{
		if (HasCookedTrayShelfToReserve(productType))
		{
			return true;
		}
		if (!ProductTypeToTrayShelvesMap.ContainsKey(productType))
		{
			return false;
		}
		for (int i = 0; i < ProductTypeToTrayShelvesMap[productType].Count; i++)
		{
			if (ProductTypeToTrayShelvesMap[productType][i].IsAvailableForRestocking() && !ProductTypeToTrayShelvesMap[productType][i].IsReservedToStaff() && !ProductTypeToTrayShelvesMap[productType][i].IsPlayerReserved())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCookedTrayShelfToReserve(ProductType productType)
	{
		if (CookedProductTypeToTrayShelvesMap.ContainsKey(productType))
		{
			foreach (TrayShelf item in CookedProductTypeToTrayShelvesMap[productType])
			{
				if (item.IsAvailableForRestocking() && !item.IsReservedToStaff() && !item.IsPlayerReserved())
				{
					return true;
				}
			}
		}
		return false;
	}

	public TrayShelf GetRandomTrayShelfToReserve(ProductType type)
	{
		if (ProductTypeToTrayShelvesMap.ContainsKey(type))
		{
			foreach (TrayShelf item in ProductTypeToTrayShelvesMap[type])
			{
				if (item.IsAvailableForRestocking() && !item.IsReservedToStaff() && !item.IsPlayerReserved())
				{
					return item;
				}
			}
		}
		return null;
	}

	public TrayShelf GetCookedTrayShelf(ProductType productType)
	{
		if (CookedProductTypeToTrayShelvesMap.ContainsKey(productType))
		{
			foreach (TrayShelf item in CookedProductTypeToTrayShelvesMap[productType])
			{
				if (item.IsAvailableForRestocking() && !item.IsReservedToStaff() && !item.IsPlayerReserved() && (item.ParentFurniture.Type != FurnitureType.OVEN || !((Oven)item.ParentFurniture).IsReservedToStaff))
				{
					return item;
				}
			}
		}
		return null;
	}

	public void RegisterOven(Oven oven)
	{
		if (!registeredOvens.Contains(oven))
		{
			registeredOvens.Add(oven);
		}
	}

	public void UnregisterOven(Oven oven)
	{
		registeredOvens.Remove(oven);
	}

	public bool IsOvenAvailable(Oven oven)
	{
		if (oven == null)
		{
			return false;
		}
		if (oven.IsReservedToStaff)
		{
			return false;
		}
		if (oven.Cooking)
		{
			return false;
		}
		if (oven.AnyTrayCooked())
		{
			return false;
		}
		if (!oven.HasEmptyCookingSlot())
		{
			return false;
		}
		if (oven.AnyCookingTrayTaken())
		{
			return false;
		}
		if (!oven.OvenIsEmpty())
		{
			return false;
		}
		return true;
	}

	public Oven GetAvailableOven(Transform bakerTransform)
	{
		Oven result = null;
		float num = float.MaxValue;
		for (int i = 0; i < registeredOvens.Count; i++)
		{
			if (IsOvenAvailable(registeredOvens[i]))
			{
				float num2 = Vector3.Distance(bakerTransform.position, registeredOvens[i].transform.position);
				if (num2 < num)
				{
					num = num2;
					result = registeredOvens[i];
				}
			}
		}
		return result;
	}

	public Placeable GetBakerIdlePosition(Employee employee)
	{
		for (int i = 0; i < SingletonBehaviour<SpawnManager>.Instance.BakeryShelfPlaceables.Count; i++)
		{
			if (!SingletonBehaviour<SpawnManager>.Instance.BakeryShelfPlaceables[i].IsOccupiedByStaff())
			{
				return SingletonBehaviour<SpawnManager>.Instance.BakeryShelfPlaceables[i];
			}
		}
		return null;
	}
}
