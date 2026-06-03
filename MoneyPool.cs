using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoneyPool : MonoBehaviour
{
	[Serializable]
	public class MoneyDictionary : UnitySerializedDictionary<MoneyType, MoneyContainer>
	{
	}

	[SerializeField]
	private MoneyDictionary moneyPool;

	private const int INITIAL_COIN_SCALE = 100;

	public Transform GetMoney(MoneyType type)
	{
		List<Transform> moneys = moneyPool[type].moneys;
		for (int i = 1; i < moneys.Count; i++)
		{
			if (!moneys[i].gameObject.activeSelf)
			{
				moneys[i].transform.localPosition = moneys[0].localPosition;
				moneys[i].gameObject.SetActive(value: true);
				return moneys[i];
			}
		}
		Transform transform = UnityEngine.Object.Instantiate(moneys[0]);
		transform.SetParent(base.transform);
		transform.transform.localPosition = moneys[0].localPosition;
		moneyPool[type].moneys.Add(transform);
		if (!transform.gameObject.activeSelf)
		{
			transform.gameObject.SetActive(value: true);
		}
		return transform;
	}

	public Transform GetPoolTransform(MoneyType type)
	{
		return moneyPool[type].moneys[0];
	}

	public Transform GetLastUsedMoney(MoneyType type)
	{
		List<Transform> moneys = moneyPool[type].moneys;
		for (int num = moneys.Count - 1; num > 0; num--)
		{
			if (moneys[num].gameObject.activeSelf && !DOTween.IsTweening(moneys[num].transform))
			{
				return moneys[num];
			}
		}
		return null;
	}

	public void PutBackToPool(Transform usedMoney)
	{
		usedMoney.gameObject.SetActive(value: false);
		usedMoney.transform.SetParent(base.transform);
		usedMoney.transform.localPosition = Vector3.zero;
	}
}
