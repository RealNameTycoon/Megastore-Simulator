using System;
using System.Collections.Generic;

public static class ShfuffleExtension
{
	private static readonly Random RandomGenerator = new Random();

	public static void Shuffle<T>(this IList<T> shuffleList)
	{
		int num = shuffleList.Count;
		while (num > 1)
		{
			num--;
			int index = RandomGenerator.Next(num + 1);
			T value = shuffleList[index];
			shuffleList[index] = shuffleList[num];
			shuffleList[num] = value;
		}
	}
}
