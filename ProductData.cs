using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductData", menuName = "ScriptableObjects/ProductData", order = 1)]
public class ProductData : ScriptableObject
{
	public Sprite productSprite;

	public ProductType type;

	public BoxType boxType;

	public ProductGroup productGroup;

	public StorageRequirement storageRequirement;

	public StorageSensitivity storageSensitivity;

	public int spoilableMaterialIndex;

	public float cost;

	public float unitMarketPrice;

	public string productNameKey;

	public int shelfRowCount;

	public int shelfColumnCount;

	public int boxRowCount;

	public int boxColumnCount;

	public ShelfType shelfType;

	public string brand;

	public int variantCount = 1;

	public int requiredLicense;

	public float profit;

	public bool facePlayer;

	public bool isVerticalBoxLayout;

	public bool isInflatable;

	public Vector3 inflatedScale = Vector3.one;

	public Vector3 deflatedScale = Vector3.one;

	public Vector3[] localPositions;

	public Vector3[] eulerAngles;

	public List<LayoutOverride> layoutOverrides;

	public List<Vector3> boxLocalPositions;

	public List<Vector3> boxEulerAngles;

	public int maxAmountAllowed = -1;

	public float height;

	public List<ContainerInfo> containerInfos;

	public List<ContainerOverride> containerOverrides;

	public void AddContainerInfo(ContainerType type, Transform localTransform, int activatingProductCount = 1, int overrideShelfType = -1)
	{
		ContainerInfo item = new ContainerInfo
		{
			type = type,
			localPosition = localTransform.localPosition,
			eulerAngle = localTransform.localEulerAngles,
			activatingProductCount = activatingProductCount
		};
		if (overrideShelfType != -1)
		{
			ContainerOverride item2 = default(ContainerOverride);
			if (containerOverrides.Count == 0)
			{
				containerOverrides = new List<ContainerOverride>();
			}
			else
			{
				for (int i = 0; i < containerOverrides.Count; i++)
				{
					if (containerOverrides[i].overrideValue == (PlaceableType)overrideShelfType)
					{
						item2 = containerOverrides[i];
						containerOverrides.RemoveAt(i);
						break;
					}
				}
			}
			if (item2.containerInfos == null)
			{
				item2.containerInfos = new List<ContainerInfo>();
			}
			item2.overrideValue = (PlaceableType)overrideShelfType;
			item2.containerInfos.Add(item);
			containerOverrides.Add(item2);
		}
		else
		{
			containerInfos.Add(item);
		}
	}

	public List<ContainerInfo> GetContainerInfos(PlaceableType shelfType)
	{
		if (HasContainerOverride(shelfType))
		{
			for (int i = 0; i < containerOverrides.Count; i++)
			{
				if (containerOverrides[i].overrideValue == shelfType)
				{
					return containerOverrides[i].containerInfos;
				}
			}
		}
		return containerInfos;
	}

	public bool HasContainerOverride(PlaceableType shelfType)
	{
		if (containerOverrides == null || containerOverrides.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < containerOverrides.Count; i++)
		{
			if (containerOverrides[i].overrideValue == shelfType)
			{
				return true;
			}
		}
		return false;
	}

	public void AddRotation(Vector3 rotation)
	{
		Quaternion quaternion = Quaternion.Euler(rotation);
		for (int i = 0; i < boxEulerAngles.Count; i++)
		{
			Quaternion quaternion2 = Quaternion.Euler(boxEulerAngles[i]);
			boxEulerAngles[i] = (quaternion * quaternion2).eulerAngles;
		}
		for (int j = 0; j < eulerAngles.Length; j++)
		{
			Quaternion quaternion3 = Quaternion.Euler(eulerAngles[j]);
			eulerAngles[j] = (quaternion * quaternion3).eulerAngles;
		}
	}

	public void ReverseBoxPositions()
	{
		if (boxLocalPositions != null && boxLocalPositions.Count > 0)
		{
			boxLocalPositions.Reverse();
			boxEulerAngles.Reverse();
		}
	}

	public void SetBoxPositions(Transform[] localTransforms)
	{
		boxLocalPositions = new List<Vector3>();
		boxEulerAngles = new List<Vector3>();
		for (int i = 0; i < localTransforms.Length; i++)
		{
			boxLocalPositions.Add(localTransforms[i].localPosition);
			boxEulerAngles.Add(localTransforms[i].localEulerAngles);
		}
	}

	public void MultiplyPositions(Vector3 multiplier)
	{
		for (int i = 0; i < localPositions.Length; i++)
		{
			localPositions[i] = new Vector3(localPositions[i].x * multiplier.x, localPositions[i].y * multiplier.y, localPositions[i].z * multiplier.z);
		}
	}

	public void SetPositions(Transform[] localTransforms, int shelfID = -1)
	{
		if (shelfID == -1)
		{
			localPositions = new Vector3[localTransforms.Length];
			eulerAngles = new Vector3[localTransforms.Length];
			for (int i = 0; i < localTransforms.Length; i++)
			{
				localPositions[i] = localTransforms[i].localPosition;
				eulerAngles[i] = localTransforms[i].localEulerAngles;
			}
			return;
		}
		for (int j = 0; j < layoutOverrides.Count; j++)
		{
			if (layoutOverrides[j].overrideValue == shelfID)
			{
				LayoutOverride value = layoutOverrides[j];
				Vector3[] array = new Vector3[localTransforms.Length];
				Vector3[] array2 = new Vector3[localTransforms.Length];
				for (int k = 0; k < localTransforms.Length; k++)
				{
					array[k] = localTransforms[k].localPosition;
					array2[k] = localTransforms[k].localEulerAngles;
				}
				value.localPositions = array;
				value.eulerAngles = array2;
				layoutOverrides[j] = value;
			}
		}
	}

	public void ReversePositions()
	{
		localPositions = localPositions.Reverse().ToArray();
		eulerAngles = eulerAngles.Reverse().ToArray();
	}

	public void AddToEulerY(float amount)
	{
		for (int i = 0; i < eulerAngles.Length; i++)
		{
			Vector3 vector = eulerAngles[i];
			vector.y += amount;
			eulerAngles[i] = vector;
		}
	}

	public float TotalCost()
	{
		return cost * (float)GetMaxProductCount();
	}

	public void SetProfit()
	{
		profit = unitMarketPrice - cost;
	}

	public void SetSprite()
	{
		productSprite = Resources.Load<Sprite>("Toys/" + base.name);
	}

	public bool IsShelf()
	{
		return type > ProductType.NONCONSUMABLE_START;
	}

	public bool HasOverridesForShelf(Shelf shelf)
	{
		if (layoutOverrides != null && layoutOverrides.Count > 0)
		{
			for (int i = 0; i < layoutOverrides.Count; i++)
			{
				if (layoutOverrides[i].condition == OverrideCondition.SHELF_ID && layoutOverrides[i].overrideValue == shelf.ShelfID)
				{
					return true;
				}
				if (layoutOverrides[i].condition == OverrideCondition.PLACEABLE_TYPE && layoutOverrides[i].overrideValue == (int)shelf.ParentPlaceableType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vector3[] GetLocalPositionsForShelf(Shelf shelf)
	{
		if (layoutOverrides != null && layoutOverrides.Count > 0)
		{
			LayoutOverride layoutOverride = layoutOverrides.FirstOrDefault((LayoutOverride o) => o.condition == OverrideCondition.SHELF_ID && o.overrideValue == shelf.ShelfID);
			if (layoutOverride.localPositions != null && layoutOverride.localPositions.Length != 0)
			{
				return layoutOverride.localPositions;
			}
			LayoutOverride layoutOverride2 = layoutOverrides.FirstOrDefault((LayoutOverride o) => o.condition == OverrideCondition.PLACEABLE_TYPE && o.overrideValue == (int)shelf.ParentPlaceableType);
			if (layoutOverride2.localPositions != null && layoutOverride2.localPositions.Length != 0)
			{
				return layoutOverride2.localPositions;
			}
		}
		return localPositions;
	}

	public Vector3[] GetEulerAnglesForShelf(Shelf shelf)
	{
		if (layoutOverrides != null && layoutOverrides.Count > 0)
		{
			LayoutOverride layoutOverride = layoutOverrides.FirstOrDefault((LayoutOverride o) => o.condition == OverrideCondition.SHELF_ID && o.overrideValue == shelf.ShelfID);
			if (layoutOverride.eulerAngles != null && layoutOverride.eulerAngles.Length != 0)
			{
				return layoutOverride.eulerAngles;
			}
			LayoutOverride layoutOverride2 = layoutOverrides.FirstOrDefault((LayoutOverride o) => o.condition == OverrideCondition.PLACEABLE_TYPE && o.overrideValue == (int)shelf.ParentPlaceableType);
			if (layoutOverride2.eulerAngles != null && layoutOverride2.eulerAngles.Length != 0)
			{
				return layoutOverride2.eulerAngles;
			}
		}
		return eulerAngles;
	}

	public bool IsCookable()
	{
		if (type > ProductType.COOKABLE_START)
		{
			return type < ProductType.COOKABLE_END;
		}
		return false;
	}

	public static bool IsCookable(ProductType type)
	{
		if (type > ProductType.COOKABLE_START)
		{
			return type < ProductType.COOKABLE_END;
		}
		return false;
	}

	public bool IsClothing()
	{
		if (type > ProductType.CLOTHING_START)
		{
			return type < ProductType.CLOTHING_END;
		}
		return false;
	}

	public int GetMaxProductCount()
	{
		if (boxLocalPositions != null && boxLocalPositions.Count > 0)
		{
			return boxLocalPositions.Count();
		}
		return boxColumnCount * boxRowCount;
	}
}
