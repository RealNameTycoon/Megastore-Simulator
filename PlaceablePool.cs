using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaceablePool : SingletonBehaviour<PlaceablePool>
{
	[Serializable]
	public class PlaceableDictionary : UnitySerializedDictionary<PlaceableType, PlaceableListContainer>
	{
	}

	[SerializeField]
	private PlaceableDictionary placeablePool;

	public Placeable GetPlaceable(PlaceableType type)
	{
		List<Placeable> placeables = placeablePool[type].placeables;
		for (int i = 0; i < placeables.Count; i++)
		{
			if (!placeables[i].gameObject.activeSelf)
			{
				if (i == placeables.Count - 1)
				{
					Placeable placeable = UnityEngine.Object.Instantiate(placeables[i]);
					placeable.transform.SetParent(base.transform);
					placeable.transform.localPosition = new Vector3(0f, placeables[i].transform.localPosition.y, 0f);
					placeablePool[type].placeables.Add(placeable);
				}
				placeables[i].gameObject.SetActive(value: true);
				return placeables[i];
			}
		}
		return null;
	}

	public void RemovePlaceable(Placeable placeable)
	{
		placeablePool[placeable.Type].placeables.Remove(placeable);
	}
}
