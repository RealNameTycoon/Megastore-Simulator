using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class ReturnArea : MonoBehaviour
{
	[Serializable]
	public class ReferenceBoxStartingPositionsDictionary : UnitySerializedDictionary<BoxType, ListContainer>
	{
	}

	[Serializable]
	public class ListContainer
	{
		public List<Transform> positions;
	}

	[SerializeField]
	private Transform cornerStart;

	[SerializeField]
	private Transform cornerEnd;

	[SerializeField]
	private float stackSpacing = 0.1f;

	[SerializeField]
	private List<Transform> restockerSlotPositions;

	[SerializeField]
	private ReferenceBoxStartingPositionsDictionary referenceBoxStartingPositions;

	private int currentRestockerSlotIndex = -1;

	[SerializeField]
	private SerializedDictionary<BoxType, List<Box>> boxDictionary = new SerializedDictionary<BoxType, List<Box>>();

	private float currentHorizontalOffset;

	private List<int> danglingBoxIDs = new List<int>();

	public Transform CornerStart => cornerStart;

	public Transform CornerEnd => cornerEnd;

	public Transform GetNextRestockerSlot()
	{
		currentRestockerSlotIndex++;
		return restockerSlotPositions[currentRestockerSlotIndex % restockerSlotPositions.Count];
	}

	public void PlaceBox(Box box, bool instant = false, Action onComplete = null)
	{
		if (box == null)
		{
			return;
		}
		ProductType num = box.GetContainedProduct()?.type ?? ProductType.NONE;
		BoxType boxType = box.Type;
		if (num == ProductType.NONE)
		{
			return;
		}
		if (boxDictionary.ContainsKey(boxType))
		{
			_ = boxDictionary[boxType].Count;
		}
		box.transform.SetParent(base.transform);
		var (vector, vector2) = GetReferenceBoxStartingToNewBox(box);
		if (instant)
		{
			box.transform.localPosition = vector;
			box.transform.localEulerAngles = vector2;
			if (!boxDictionary.ContainsKey(boxType))
			{
				boxDictionary[boxType] = new List<Box>();
			}
			boxDictionary[boxType].Add(box);
			box.RigidBody.isKinematic = true;
			box.SetBoxInReturnArea(inReturnArea: true);
			box.ReleaseBoxReservation();
			SingletonBehaviour<RestockZoneManager>.Instance.RegisterReturnAreaBox(box);
			onComplete?.Invoke();
			return;
		}
		box.transform.DOKill();
		box.transform.DoCurvedLocalMove3D(vector, 0.3f, box.GetHeight());
		box.transform.DOLocalRotate(vector2, 0.3f).OnComplete(delegate
		{
			if (!boxDictionary.ContainsKey(boxType))
			{
				boxDictionary[boxType] = new List<Box>();
			}
			boxDictionary[boxType].Add(box);
			box.RigidBody.isKinematic = true;
			box.SetBoxInReturnArea(inReturnArea: true);
			box.ReleaseBoxReservation();
			SingletonBehaviour<RestockZoneManager>.Instance.RegisterReturnAreaBox(box);
			Vector3 localPositionForExistingBox = GetLocalPositionForExistingBox(box);
			if (Vector3.Distance(base.transform.position, localPositionForExistingBox) > 0.1f)
			{
				box.transform.localPosition = localPositionForExistingBox;
			}
			onComplete?.Invoke();
		});
	}

	private Vector3 GetLocalPositionForExistingBox(Box box)
	{
		int count = referenceBoxStartingPositions[box.Type].positions.Count;
		int num = (boxDictionary.ContainsKey(box.Type) ? boxDictionary[box.Type].IndexOf(box) : 0);
		int index = num % count;
		return referenceBoxStartingPositions[box.Type].positions[index].localPosition + Vector3.up * box.GetHeight() * (num / count);
	}

	private (Vector3, Vector3) GetReferenceBoxStartingToNewBox(Box box)
	{
		int count = referenceBoxStartingPositions[box.Type].positions.Count;
		int num = (boxDictionary.ContainsKey(box.Type) ? boxDictionary[box.Type].Count : 0);
		int index = num % count;
		Vector3 localPosition = referenceBoxStartingPositions[box.Type].positions[index].localPosition;
		return new ValueTuple<Vector3, Vector3>(item2: referenceBoxStartingPositions[box.Type].positions[index].localEulerAngles, item1: localPosition + Vector3.up * box.GetHeight() * (num / count));
	}

	public Transform GetBoxTakePosition(BoxType type)
	{
		int count = referenceBoxStartingPositions[type].positions.Count;
		return referenceBoxStartingPositions[type].positions[count - 1];
	}

	public void RemoveBox(Box box)
	{
		if (!(box == null) && boxDictionary.ContainsKey(box.Type) && boxDictionary[box.Type].Contains(box))
		{
			boxDictionary[box.Type].Remove(box);
			box.SetBoxInReturnArea(inReturnArea: false);
			SingletonBehaviour<RestockZoneManager>.Instance.UnregisterReturnAreaBox(box);
			RestackBoxesOfType(box.Type);
		}
	}

	private void RestackBoxesOfType(BoxType type)
	{
		if (boxDictionary.ContainsKey(type))
		{
			List<Box> list = boxDictionary[type];
			for (int i = 0; i < list.Count; i++)
			{
				int count = referenceBoxStartingPositions[type].positions.Count;
				int num = i;
				int index = num % count;
				Vector3 localPosition = referenceBoxStartingPositions[type].positions[index].localPosition;
				Vector3 localEulerAngles = referenceBoxStartingPositions[type].positions[index].localEulerAngles;
				Vector3 endValue = localPosition + Vector3.up * list[i].GetHeight() * (num / count);
				list[i].transform.DOKill();
				list[i].transform.DOLocalMove(endValue, 0.3f);
				list[i].transform.DOLocalRotate(localEulerAngles, 0.3f);
			}
		}
	}

	public bool IsPositionInReturnArea(Vector3 position)
	{
		Vector3 vector = Vector3.Min(cornerStart.position, cornerEnd.position);
		Vector3 vector2 = Vector3.Max(cornerStart.position, cornerEnd.position);
		if (position.x >= vector.x && position.x <= vector2.x && position.z >= vector.z)
		{
			return position.z <= vector2.z;
		}
		return false;
	}
}
