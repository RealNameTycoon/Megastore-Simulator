using System;
using System.Collections.Generic;
using UnityEngine;

public class PalletPool : SingletonBehaviour<PalletPool>
{
	[Serializable]
	public class PalletDictionary : UnitySerializedDictionary<PalletType, PalletContainer>
	{
	}

	[SerializeField]
	private PalletDictionary palletPool;

	public Pallet GetPallet(PalletType type)
	{
		List<Pallet> pallets = palletPool[type].pallets;
		for (int i = 0; i < pallets.Count; i++)
		{
			if (!pallets[i].gameObject.activeSelf)
			{
				if (i == pallets.Count - 1)
				{
					Pallet pallet = UnityEngine.Object.Instantiate(pallets[i]);
					pallet.transform.SetParent(base.transform);
					pallet.transform.localPosition = Vector3.zero;
					palletPool[type].pallets.Add(pallet);
				}
				pallets[i].gameObject.SetActive(value: true);
				return pallets[i];
			}
		}
		return null;
	}

	public void PutBackToPool(Pallet usedPallet)
	{
		usedPallet.gameObject.SetActive(value: false);
		usedPallet.transform.SetParent(base.transform);
		usedPallet.transform.localPosition = Vector3.zero;
		usedPallet.transform.localEulerAngles = Vector3.zero;
		usedPallet.ResetPallet();
	}
}
