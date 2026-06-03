using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class RestockZoneManager : SingletonBehaviour<RestockZoneManager>
{
	[SerializeField]
	private List<RestockZone> restockZones;

	[SerializeField]
	private List<ReturnArea> returnAreas;

	[SerializeField]
	private RestockZone warehouseRestockZone;

	[SerializeField]
	private RestockZone unloadersRestockZone;

	[SerializeField]
	private SerializedDictionary<ProductType, List<Box>> returnAreaBoxes = new SerializedDictionary<ProductType, List<Box>>();

	[SerializeField]
	private List<TextMeshPro> restockZoneLabels;

	[SerializeField]
	private TextMeshPro returnAreaLabel;

	public ReturnArea ReturnArea => returnAreas[0];

	private void Start()
	{
		if (GameManager.isDemo)
		{
			for (int i = 0; i < restockZones.Count; i++)
			{
				restockZones[i].gameObject.SetActive(value: false);
			}
			for (int j = 0; j < returnAreas.Count; j++)
			{
				returnAreas[j].gameObject.SetActive(value: false);
			}
		}
		else
		{
			Initialize();
		}
		EventManager.NotifyEvent(StartupEvents.RESTOCK_ZONES_INITIALIZED);
		UpdateLabels();
		LocalizeBase.OnLanguageChanged += UpdateLabels;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= UpdateLabels;
	}

	private void UpdateLabels()
	{
		for (int i = 0; i < restockZoneLabels.Count; i++)
		{
			restockZoneLabels[i].text = Locale.GetWord("restock_zone");
		}
		returnAreaLabel.text = Locale.GetWord("return_area");
	}

	private void Initialize()
	{
		EventManager.AddListener<int>(SupermarketEvents.STORAGE_GROWTH_PURCHASED, OnStorageGrowthPurchased);
		int growthCount = SingletonBehaviour<StorageManager>.Instance.GrowthCount;
		for (int i = 0; i < restockZones.Count; i++)
		{
			if (growthCount + StorageManager.DEFAULT_GIVEN_EXTRA_GROWTH >= i * StorageManager.GROWTHS_NEEDED_PER_DOCK)
			{
				restockZones[i].gameObject.SetActive(value: true);
			}
			else
			{
				restockZones[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void OnStorageGrowthPurchased(int growthLevel)
	{
		if ((growthLevel + StorageManager.DEFAULT_GIVEN_EXTRA_GROWTH) % StorageManager.GROWTHS_NEEDED_PER_DOCK == 0 && (growthLevel + StorageManager.DEFAULT_GIVEN_EXTRA_GROWTH) / StorageManager.GROWTHS_NEEDED_PER_DOCK < restockZones.Count)
		{
			restockZones[(growthLevel + StorageManager.DEFAULT_GIVEN_EXTRA_GROWTH) / StorageManager.GROWTHS_NEEDED_PER_DOCK].gameObject.SetActive(value: true);
		}
	}

	public RestockZone GetRestockZoneAtPosition(Vector3 position)
	{
		foreach (RestockZone restockZone in restockZones)
		{
			Bounds bounds = restockZone.MoveableBlockerCollider.bounds;
			if (position.x >= bounds.min.x && position.x <= bounds.max.x && position.z >= bounds.min.z && position.z <= bounds.max.z)
			{
				return restockZone;
			}
		}
		if (warehouseRestockZone.gameObject.activeSelf)
		{
			Bounds bounds2 = warehouseRestockZone.MoveableBlockerCollider.bounds;
			if (position.x >= bounds2.min.x && position.x <= bounds2.max.x && position.z >= bounds2.min.z && position.z <= bounds2.max.z)
			{
				return warehouseRestockZone;
			}
		}
		if (unloadersRestockZone.gameObject.activeSelf)
		{
			Bounds bounds3 = unloadersRestockZone.MoveableBlockerCollider.bounds;
			if (position.x >= bounds3.min.x && position.x <= bounds3.max.x && position.z >= bounds3.min.z && position.z <= bounds3.max.z)
			{
				return unloadersRestockZone;
			}
		}
		return null;
	}

	public bool IsInWarehouse(Vector3 position)
	{
		Bounds bounds = warehouseRestockZone.MoveableBlockerCollider.bounds;
		if (position.x >= bounds.min.x && position.x <= bounds.max.x && position.z >= bounds.min.z)
		{
			return position.z <= bounds.max.z;
		}
		return false;
	}

	public Box GetRandomBox()
	{
		foreach (RestockZone restockZone in restockZones)
		{
			if (restockZone.HasBox(ProductGroup.CLOTHING))
			{
				return restockZone.GetRandomBox(ProductGroup.CLOTHING);
			}
		}
		return null;
	}

	public bool HasBoxWithProductTypeOnTruck(ProductType productType)
	{
		if (unloadersRestockZone.HasBoxToReserve(productType, ignorePalletShelfs: true))
		{
			return true;
		}
		return false;
	}

	public bool HasBoxWithProductType(ProductType productType, bool includeWarehouse = false)
	{
		if (returnAreaBoxes.ContainsKey(productType))
		{
			foreach (Box item in returnAreaBoxes[productType])
			{
				if (!item.IsReservedForRestocking && !item.IsEmpty())
				{
					return true;
				}
			}
		}
		foreach (RestockZone restockZone in restockZones)
		{
			if (restockZone.gameObject.activeSelf && restockZone.HasBoxToReserve(productType))
			{
				return true;
			}
		}
		if (includeWarehouse && warehouseRestockZone.HasBoxToReserve(productType))
		{
			return true;
		}
		return false;
	}

	public Box GetBoxWithProductType(ProductType productType, bool includeWarehouse = false)
	{
		if (returnAreaBoxes.ContainsKey(productType) && returnAreaBoxes[productType].Count > 0)
		{
			Box box = null;
			foreach (Box item in returnAreaBoxes[productType])
			{
				if (!item.IsReservedForRestocking && !item.IsEmpty())
				{
					box = item;
					break;
				}
			}
			if (box != null)
			{
				return box;
			}
		}
		foreach (RestockZone restockZone in restockZones)
		{
			if (restockZone.gameObject.activeSelf && restockZone.HasBoxToReserve(productType))
			{
				Box randomBoxToReserve = restockZone.GetRandomBoxToReserve(productType);
				return GetLowestStockBoxOnPallet(randomBoxToReserve, productType);
			}
		}
		if (includeWarehouse && warehouseRestockZone.HasBoxToReserve(productType))
		{
			Box randomBoxToReserve2 = warehouseRestockZone.GetRandomBoxToReserve(productType);
			return GetLowestStockBoxOnPallet(randomBoxToReserve2, productType);
		}
		return null;
	}

	public Box GetBoxWithProductTypeOnTruck(ProductType productType)
	{
		if (unloadersRestockZone.HasBoxToReserve(productType))
		{
			Box randomBoxToReserve = unloadersRestockZone.GetRandomBoxToReserve(productType);
			return GetTopBoxOnPallet(randomBoxToReserve, productType);
		}
		return null;
	}

	private Box GetLowestStockBoxOnPallet(Box box, ProductType requiredProductType)
	{
		if (box.ContainedPalletID != -1)
		{
			Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(box.ContainedPalletID);
			if (pallet != null)
			{
				return pallet.GetLowestStockBox();
			}
		}
		return box;
	}

	private Box GetTopBoxOnPallet(Box box, ProductType requiredProductType)
	{
		if (box.ContainedPalletID != -1)
		{
			Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(box.ContainedPalletID);
			if (pallet != null)
			{
				return pallet.GetLastUnreservedBox();
			}
		}
		return box;
	}

	public void RegisterReturnAreaBox(Box box)
	{
		if (!(box == null) && !(box.GetContainedProduct() == null))
		{
			ProductType type = box.GetContainedProduct().type;
			if (!returnAreaBoxes.ContainsKey(type))
			{
				returnAreaBoxes[type] = new List<Box>();
			}
			if (!returnAreaBoxes[type].Contains(box))
			{
				returnAreaBoxes[type].Add(box);
			}
		}
	}

	public void UnregisterReturnAreaBox(Box box)
	{
		if (!(box == null) && !(box.GetContainedProduct() == null))
		{
			ProductType type = box.GetContainedProduct().type;
			if (returnAreaBoxes.ContainsKey(type) && returnAreaBoxes[type].Contains(box))
			{
				returnAreaBoxes[type].Remove(box);
			}
		}
	}
}
