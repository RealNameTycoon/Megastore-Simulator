using System;
using System.Collections.Generic;

[Serializable]
public struct ContainerOverride
{
	public List<ContainerInfo> containerInfos;

	public PlaceableType overrideValue;
}
