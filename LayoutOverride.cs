using System;
using UnityEngine;

[Serializable]
public struct LayoutOverride
{
	public OverrideCondition condition;

	public int overrideValue;

	public Vector3[] localPositions;

	public Vector3[] eulerAngles;
}
