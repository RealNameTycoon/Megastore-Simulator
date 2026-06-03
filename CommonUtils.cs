using System.Collections.Generic;
using UnityEngine;

public static class CommonUtils
{
	public static int[] GetRandomPrefabIndexes(int numRequired, ref GameObject[] peoplePrefabs)
	{
		List<int> list = new List<int>();
		List<GameObject> list2 = new List<GameObject>(peoplePrefabs);
		list2.Shuffle();
		peoplePrefabs = list2.ToArray();
		int i = 0;
		int num = 0;
		for (; i < numRequired; i++)
		{
			list.Add((num < peoplePrefabs.Length) ? num++ : (num = 0));
		}
		list.Shuffle();
		return list.ToArray();
	}
}
