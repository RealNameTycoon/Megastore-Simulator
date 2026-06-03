using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LabeledStockManager : SingletonBehaviour<LabeledStockManager>
{
	[SerializeField]
	private SerializedDictionary<ProductType, List<Restockable>> allLabeledShelvesByProductType = new SerializedDictionary<ProductType, List<Restockable>>();

	private new void Awake()
	{
		base.Awake();
		EventManager.AddListener<Shelf>(PlaceableEvents.PRICE_TAG_ADDED, OnPriceTagAdded);
		EventManager.AddListener<Shelf>(PlaceableEvents.PRICE_TAG_CHANGED, OnPriceTagChanged);
		EventManager.AddListener<Shelf>(PlaceableEvents.BEFORE_PRICE_TAG_REMOVED, OnBeforePriceTagRemoved);
	}

	private void OnPriceTagAdded(Shelf shelf)
	{
		if (shelf.ContainedProductType() == ProductType.NONE)
		{
			AddToLabeledShelvesByProductType(shelf.PreviousContainedProductType(), shelf);
		}
		else
		{
			AddToLabeledShelvesByProductType(shelf.ContainedProductType(), shelf);
		}
	}

	private void OnPriceTagChanged(Shelf shelf)
	{
		RemoveFromLabeledShelvesByProductType(shelf.PreviousContainedProductType(), shelf);
		AddToLabeledShelvesByProductType(shelf.ContainedProductType(), shelf);
	}

	private void OnBeforePriceTagRemoved(Shelf shelf)
	{
		RemoveFromLabeledShelvesByProductType(shelf.PreviousContainedProductType(), shelf);
	}

	private void AddToLabeledShelvesByProductType(ProductType productType, Restockable shelf)
	{
		if (!shelf.IsUnloaderPlaceable())
		{
			if (!allLabeledShelvesByProductType.ContainsKey(productType))
			{
				allLabeledShelvesByProductType.Add(productType, new List<Restockable>());
				EventManager.NotifyEvent(GameEvents.LABELED_SHELVES_UPDATED);
			}
			allLabeledShelvesByProductType[productType].Add(shelf);
		}
	}

	private void RemoveFromLabeledShelvesByProductType(ProductType productType, Restockable shelf)
	{
		if (!shelf.IsUnloaderPlaceable() && allLabeledShelvesByProductType.ContainsKey(productType))
		{
			allLabeledShelvesByProductType[productType].Remove(shelf);
			if (allLabeledShelvesByProductType[productType].Count == 0)
			{
				EventManager.NotifyEvent(GameEvents.LABELED_SHELVES_UPDATED);
			}
		}
	}

	public bool HasLabeledShelfByProductType(ProductType productType)
	{
		if (allLabeledShelvesByProductType.ContainsKey(productType))
		{
			return allLabeledShelvesByProductType[productType].Count > 0;
		}
		return false;
	}
}
