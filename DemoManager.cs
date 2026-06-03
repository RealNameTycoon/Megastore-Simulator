using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> demoObjects;

	private void Awake()
	{
		for (int i = 0; i < demoObjects.Count; i++)
		{
			demoObjects[i].SetActive(GameManager.isDemo);
		}
	}
}
