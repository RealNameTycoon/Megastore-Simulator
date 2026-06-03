using System;
using UnityEngine;

[Serializable]
public struct ContainerInfo
{
	public ContainerType type;

	public Vector3 localPosition;

	public Vector3 eulerAngle;

	public int activatingProductCount;
}
