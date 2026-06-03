using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
	public static T GetRandomElement<T>(this T[] array)
	{
		if (array.Length == 0)
		{
			return array[0];
		}
		int num = Random.Range(0, array.Length);
		return array[num];
	}

	public static T GetRandomElement<T>(this List<T> array)
	{
		int index = Random.Range(0, array.Count);
		return array[index];
	}

	public static T GetLastElement<T>(this List<T> array)
	{
		return array[array.Count - 1];
	}
}
