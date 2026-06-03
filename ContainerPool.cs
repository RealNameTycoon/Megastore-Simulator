using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainerPool : SingletonBehaviour<ContainerPool>
{
	[Serializable]
	public class ContainerDictionary : UnitySerializedDictionary<ContainerType, ContainerListContainer>
	{
	}

	[SerializeField]
	private ContainerDictionary containerPool;

	public Transform GetContainer(ContainerType type)
	{
		List<Transform> containers = containerPool[type].containers;
		for (int i = 0; i < containers.Count; i++)
		{
			if (!containers[i].gameObject.activeSelf && containers[i].parent == base.transform)
			{
				containers[i].gameObject.SetActive(value: true);
				return containers[i];
			}
		}
		Transform transform = UnityEngine.Object.Instantiate(containers[0], base.transform, worldPositionStays: true);
		containerPool[type].containers.Add(transform);
		if (!transform.gameObject.activeSelf)
		{
			transform.gameObject.SetActive(value: true);
		}
		return transform;
	}

	public void PutBackToPool(Transform usedContainer)
	{
		usedContainer.gameObject.SetActive(value: false);
		usedContainer.transform.SetParent(base.transform);
		usedContainer.transform.localPosition = Vector3.zero;
	}
}
