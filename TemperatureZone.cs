using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TemperatureZone : MonoBehaviour
{
	[SerializeField]
	private bool ignoreTick;

	[SerializeField]
	private BoxCollider volumeCollider;

	[SerializeField]
	private Transform corner1;

	[SerializeField]
	private Transform corner2;

	[SerializeField]
	private List<Box> registeredBoxes = new List<Box>();

	[SerializeField]
	private SerializedDictionary<ProductType, List<Box>> registeredBoxesByType = new SerializedDictionary<ProductType, List<Box>>();

	[SerializeField]
	private SerializedDictionary<ProductGroup, List<Box>> registeredBoxesByProductGroup = new SerializedDictionary<ProductGroup, List<Box>>();

	[SerializeField]
	private List<PalletRack> palletRacks = new List<PalletRack>();

	[SerializeField]
	private List<BoxShelf> boxShelves = new List<BoxShelf>();

	[SerializeField]
	private List<Pallet> freePallets = new List<Pallet>();

	private float temperature;

	private List<Box> EmptyBoxes = new List<Box>();

	public BoxCollider MoveableBlockerCollider => volumeCollider;

	public Transform Corner1 => corner1;

	public Transform Corner2 => corner2;

	public float Temperature => temperature;

	public void SetTemperature(float temperature)
	{
		this.temperature = temperature;
	}

	public void TemperatureTick(int minutes = 1)
	{
		if (ignoreTick)
		{
			return;
		}
		foreach (Box registeredBox in registeredBoxes)
		{
			registeredBox.TemperatureTick(temperature, minutes);
		}
	}

	public bool HasBoxes()
	{
		return registeredBoxes.Count > 0;
	}

	public void RegisterBox(Box box)
	{
		if (!registeredBoxes.Contains(box) && !box.IsInReturnArea && box.ContainsConsumableProduct())
		{
			ProductType type = box.GetContainedProduct().type;
			_ = box.GetContainedProduct().storageRequirement;
			registeredBoxes.Add(box);
			if (!box.Stored)
			{
				box.transform.SetParent(base.transform);
			}
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
		}
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
	}

	public void RegisterPalletRack(PalletRack palletRack)
	{
		palletRacks.Add(palletRack);
	}

	public void UnregisterPalletRack(PalletRack palletRack)
	{
		palletRacks.Remove(palletRack);
	}

	public void RegisterBoxShelf(BoxShelf boxShelf)
	{
		boxShelves.Add(boxShelf);
	}

	public void UnregisterBoxShelf(BoxShelf boxShelf)
	{
		boxShelves.Remove(boxShelf);
	}

	public void RegisterFreePallet(Pallet pallet)
	{
		freePallets.Add(pallet);
	}

	public void UnregisterFreePallet(Pallet pallet)
	{
		freePallets.Remove(pallet);
	}

	public void OnPositionChanged()
	{
		foreach (Box registeredBox in registeredBoxes)
		{
			if (!registeredBox.Stored)
			{
				registeredBox.SaveLocation();
			}
		}
		foreach (Pallet freePallet in freePallets)
		{
			freePallet.SaveLocation();
		}
		foreach (PalletRack palletRack in palletRacks)
		{
			palletRack.SavePosition();
		}
		foreach (BoxShelf boxShelf in boxShelves)
		{
			boxShelf.SavePosition();
		}
	}

	public bool HasPlaceablesInside()
	{
		if (palletRacks.Count <= 0 && boxShelves.Count <= 0)
		{
			return freePallets.Count > 0;
		}
		return true;
	}
}
