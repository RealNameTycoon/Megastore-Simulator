using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RestockZone : MonoBehaviour
{
	[SerializeField]
	private BoxCollider moveableBlockerCollider;

	[SerializeField]
	private List<Box> registeredBoxes = new List<Box>();

	[SerializeField]
	private SerializedDictionary<ProductType, List<Box>> registeredBoxesByType = new SerializedDictionary<ProductType, List<Box>>();

	[SerializeField]
	private SerializedDictionary<ProductGroup, List<Box>> registeredBoxesByProductGroup = new SerializedDictionary<ProductGroup, List<Box>>();

	private List<Box> EmptyBoxes = new List<Box>();

	public BoxCollider MoveableBlockerCollider => moveableBlockerCollider;

	public void RegisterBox(Box box)
	{
		if (registeredBoxes.Contains(box) || box.IsInReturnArea || !box.ContainsConsumableProduct())
		{
			return;
		}
		registeredBoxes.Add(box);
		if (box.IsEmpty())
		{
			EmptyBoxes.Add(box);
			return;
		}
		ProductType type = box.GetContainedProduct().type;
		if (!registeredBoxesByType.ContainsKey(type))
		{
			registeredBoxesByType.Add(type, new List<Box>());
		}
		registeredBoxesByType[type].Add(box);
		ProductGroup productGroup = box.GetContainedProduct().productGroup;
		if (!registeredBoxesByProductGroup.ContainsKey(productGroup))
		{
			registeredBoxesByProductGroup.Add(productGroup, new List<Box>());
		}
		registeredBoxesByProductGroup[productGroup].Add(box);
		EventManager.NotifyEvent(RestockEvents.BOX_REGISTERED);
	}

	public void UnregisterBox(Box box)
	{
		if (registeredBoxes.Contains(box))
		{
			registeredBoxes.Remove(box);
		}
		if (box.IsEmpty() && EmptyBoxes.Contains(box))
		{
			EmptyBoxes.Remove(box);
			return;
		}
		if (box.GetContainedProduct() != null && registeredBoxesByType.ContainsKey(box.GetContainedProduct().type))
		{
			registeredBoxesByType[box.GetContainedProduct().type].Remove(box);
		}
		if (box.GetContainedProduct() != null && registeredBoxesByProductGroup.ContainsKey(box.GetContainedProduct().productGroup))
		{
			registeredBoxesByProductGroup[box.GetContainedProduct().productGroup].Remove(box);
		}
		EventManager.NotifyEvent(RestockEvents.BOX_UNREGISTERED);
	}

	public bool HasBox(ProductGroup group)
	{
		if (registeredBoxesByProductGroup.ContainsKey(group))
		{
			return registeredBoxesByProductGroup[group].Count > 0;
		}
		return false;
	}

	public Box GetRandomBox(ProductGroup group)
	{
		if (registeredBoxesByProductGroup.ContainsKey(group))
		{
			return registeredBoxesByProductGroup[group].GetRandomElement();
		}
		return null;
	}

	public bool HasBoxToReserve(ProductType type, bool ignorePalletShelfs = false)
	{
		if (!registeredBoxesByType.ContainsKey(type))
		{
			return false;
		}
		foreach (Box item in registeredBoxesByType[type])
		{
			if (item.IsReservedForRestocking || item.IsEmpty())
			{
				continue;
			}
			if (ignorePalletShelfs && item.ContainedPalletID != -1)
			{
				Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(item.ContainedPalletID);
				if (pallet != null && pallet.ContainerShelf != null)
				{
					continue;
				}
			}
			return true;
		}
		return false;
	}

	public Box GetRandomBoxToReserve(ProductType type)
	{
		if (registeredBoxesByType.ContainsKey(type))
		{
			foreach (Box item in registeredBoxesByType[type])
			{
				if (!item.IsReservedForRestocking && !item.IsEmpty())
				{
					return item;
				}
			}
		}
		return null;
	}

	public int GetBoxCountForType(ProductType type)
	{
		if (registeredBoxesByType.ContainsKey(type))
		{
			return registeredBoxesByType[type].Count;
		}
		return 0;
	}
}
