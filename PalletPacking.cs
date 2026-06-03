using System.Collections.Generic;
using UnityEngine;

public static class PalletPacking
{
	public struct Result
	{
		public List<Vector3> positions;

		public List<Vector3> eulerAngles;

		public int totalBoxes;

		public int layers;
	}

	private class O
	{
		public float fx;

		public float fz;

		public float h;

		public float yLift;

		public Vector3 euler;

		public int nx;

		public int nz;

		public int perLayer;

		public O(float fx, float fz, float h, Vector3 euler, float yLift)
		{
			this.fx = fx;
			this.fz = fz;
			this.h = h;
			this.euler = euler;
			this.yLift = yLift;
		}

		public void Precompute(float surfW, float surfL, float eps)
		{
			nx = Mathf.FloorToInt((surfW - eps) / Mathf.Max(fx, eps));
			nz = Mathf.FloorToInt((surfL - eps) / Mathf.Max(fz, eps));
			if (nx < 0)
			{
				nx = 0;
			}
			if (nz < 0)
			{
				nz = 0;
			}
			perLayer = nx * nz;
		}
	}

	public static Result Generate(Vector3 pivotLocal, float surfaceWidth, float surfaceLength, float stackHeight, float boxWidth, float boxLength, float boxHeight)
	{
		Result result = new Result
		{
			positions = new List<Vector3>(),
			eulerAngles = new List<Vector3>(),
			totalBoxes = 0,
			layers = 0
		};
		List<O> list = new List<O>(6)
		{
			new O(boxWidth, boxLength, boxHeight, new Vector3(0f, 0f, 0f), 0f),
			new O(boxLength, boxWidth, boxHeight, new Vector3(0f, 90f, 0f), 0f),
			new O(boxWidth, boxHeight, boxLength, new Vector3(90f, 0f, 0f), boxLength * 0.5f),
			new O(boxHeight, boxWidth, boxLength, new Vector3(90f, 90f, 0f), boxLength * 0.5f),
			new O(boxHeight, boxLength, boxWidth, new Vector3(0f, 0f, 90f), boxWidth * 0.5f),
			new O(boxLength, boxHeight, boxWidth, new Vector3(0f, 90f, 90f), boxWidth * 0.5f)
		};
		foreach (O item in list)
		{
			item.Precompute(surfaceWidth, surfaceLength, 0.0001f);
		}
		float num = stackHeight;
		float num2 = pivotLocal.y;
		while (true)
		{
			O o = null;
			float num3 = -1f;
			foreach (O item2 in list)
			{
				if (!(item2.h > num + 0.0001f) && item2.perLayer > 0)
				{
					float num4 = (float)item2.perLayer / Mathf.Max(item2.h, 0.0001f);
					if (num4 > num3)
					{
						num3 = num4;
						o = item2;
					}
				}
			}
			if (o == null)
			{
				break;
			}
			int num5 = o.nx;
			int num6 = o.nz;
			while ((float)num5 * o.fx > surfaceWidth + 0.0001f)
			{
				num5--;
			}
			while ((float)num6 * o.fz > surfaceLength + 0.0001f)
			{
				num6--;
			}
			if (num5 <= 0 || num6 <= 0)
			{
				break;
			}
			float num7 = pivotLocal.x - (float)num5 * o.fx * 0.5f;
			float num8 = pivotLocal.z - (float)num6 * o.fz * 0.5f;
			float y = num2 + o.yLift;
			for (int i = 0; i < num6; i++)
			{
				float z = num8 + (float)i * o.fz;
				for (int j = 0; j < num5; j++)
				{
					float x = num7 + (float)j * o.fx;
					result.positions.Add(new Vector3(x, y, z));
					result.eulerAngles.Add(o.euler);
				}
			}
			result.layers++;
			num -= o.h;
			num2 += o.h;
		}
		result.totalBoxes = result.positions.Count;
		return result;
	}
}
