using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
	private static float RANDOM_HEIGHT_MULTIPLIER_MAX = 0.85f;

	private static float RANDOM_HEIGHT_MULTIPLIER_MIN = 0.65f;

	public static List<Vector3> CalculatePositions(Transform corner1, Transform corner2, int rowCount, int columnCount, bool isVertical, float productHeight = 0f, bool randomizeHeight = false)
	{
		List<Vector3> list = new List<Vector3>();
		int num = 0;
		for (int num2 = rowCount - 1; num2 >= 0; num2--)
		{
			num++;
			int num3 = 0;
			for (int i = 0; i < columnCount; i++)
			{
				num3++;
				if (isVertical)
				{
					float t = (float)num3 / ((float)columnCount + 1f);
					Vector3 localPosition = corner1.localPosition;
					Vector3 localPosition2 = corner2.localPosition;
					Vector3 vector = Vector3.Lerp(localPosition, localPosition2, t);
					int num4 = rowCount - 1 - num2;
					Vector3 item = vector + Vector3.up * (randomizeHeight ? (productHeight * Random.Range(RANDOM_HEIGHT_MULTIPLIER_MIN, RANDOM_HEIGHT_MULTIPLIER_MAX) * (float)num4) : (productHeight * (float)num4));
					list.Add(item);
				}
				else
				{
					float x = Mathf.Lerp(corner1.localPosition.x, corner2.localPosition.x, ((float)i + 0.5f) / (float)columnCount);
					float y = Mathf.Lerp(corner1.localPosition.y, corner2.localPosition.y, ((float)num2 + 0.5f) / (float)rowCount);
					float z = Mathf.Lerp(corner1.localPosition.z, corner2.localPosition.z, ((float)num2 + 0.5f) / (float)rowCount);
					Vector3 item2 = new Vector3(x, y, z);
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public static List<Vector3> CalculatePositionsForBox(Transform corner1, Transform corner2, int rowCount, int columnCount, bool isVertical, float boxHeight = 0f)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < columnCount; i++)
		{
			int num = 0;
			for (int num2 = rowCount - 1; num2 >= 0; num2--)
			{
				num++;
				if (isVertical)
				{
					float x = 0.5f * (corner1.localPosition.x + corner2.localPosition.x);
					float y = corner1.localPosition.y;
					float y2 = corner2.localPosition.y;
					float num3 = Mathf.Min(y, y2);
					float num4 = ((boxHeight > 0f) ? (num3 + boxHeight * 0.9f) : Mathf.Max(y, y2));
					float num5 = Mathf.Max(0f, num4 - num3);
					float num6 = ((rowCount > 0) ? (num5 / (float)rowCount) : 0f);
					float y3 = num3 + (float)num2 * num6;
					float z = Mathf.Lerp(corner1.localPosition.z, corner2.localPosition.z, ((float)i + 0.5f) / (float)columnCount);
					Vector3 item = new Vector3(x, y3, z);
					list.Add(item);
				}
				else
				{
					float x2 = Mathf.Lerp(corner1.localPosition.x, corner2.localPosition.x, ((float)i + 0.5f) / (float)columnCount);
					float y4 = Mathf.Lerp(corner1.localPosition.y, corner2.localPosition.y, ((float)num2 + 0.5f) / (float)rowCount);
					float z2 = Mathf.Lerp(corner1.localPosition.z, corner2.localPosition.z, ((float)num2 + 0.5f) / (float)rowCount);
					Vector3 item2 = new Vector3(x2, y4, z2);
					list.Add(item2);
				}
			}
		}
		return list;
	}
}
