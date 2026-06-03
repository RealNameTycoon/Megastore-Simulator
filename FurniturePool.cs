using System;
using System.Collections.Generic;
using UnityEngine;

public class FurniturePool : SingletonBehaviour<FurniturePool>
{
	[Serializable]
	public class FurnitureDictionary : UnitySerializedDictionary<FurnitureType, FurnitureListContainer>
	{
	}

	[SerializeField]
	private FurnitureDictionary furniturePool;

	public Furniture GetFurniture(FurnitureType type)
	{
		List<Furniture> furnitures = furniturePool[type].furnitures;
		for (int i = 0; i < furnitures.Count; i++)
		{
			if (!furnitures[i].gameObject.activeSelf)
			{
				if (i == furnitures.Count - 1)
				{
					Furniture furniture = UnityEngine.Object.Instantiate(furnitures[i]);
					furniture.transform.SetParent(base.transform);
					furniture.transform.localPosition = new Vector3(0f, furnitures[i].transform.localPosition.y, 0f);
					furniturePool[type].furnitures.Add(furniture);
				}
				furnitures[i].gameObject.SetActive(value: true);
				return furnitures[i];
			}
		}
		return null;
	}

	public bool IsFurniturePurchased(FurnitureType type)
	{
		for (int i = 0; i < furniturePool[type].furnitures.Count; i++)
		{
			if (furniturePool[type].furnitures[i].gameObject.activeSelf)
			{
				return true;
			}
		}
		return false;
	}

	public List<Furniture> GetAllActiveFurnitures(FurnitureType type)
	{
		List<Furniture> furnitures = furniturePool[type].furnitures;
		List<Furniture> list = new List<Furniture>();
		for (int i = 0; i < furnitures.Count; i++)
		{
			if (furnitures[i].gameObject.activeSelf)
			{
				list.Add(furnitures[i]);
			}
		}
		return list;
	}

	public void RemoveFurniture(Furniture furniture)
	{
		furniturePool[furniture.Type].furnitures.Remove(furniture);
	}
}
