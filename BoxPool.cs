using System;
using System.Collections.Generic;
using UnityEngine;

public class BoxPool : SingletonBehaviour<BoxPool>
{
	[Serializable]
	public class BoxDictionary : UnitySerializedDictionary<BoxType, BoxContainer>
	{
	}

	[SerializeField]
	private BoxDictionary boxPool;

	private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	public Box GetBox(BoxType type)
	{
		List<Box> boxes = boxPool[type].boxes;
		for (int i = 0; i < boxes.Count; i++)
		{
			if (!boxes[i].gameObject.activeSelf)
			{
				if (i == boxes.Count - 1)
				{
					Box box = UnityEngine.Object.Instantiate(boxes[i]);
					box.transform.SetParent(base.transform);
					box.transform.localPosition = Vector3.zero;
					boxPool[type].boxes.Add(box);
				}
				boxes[i].gameObject.SetActive(value: true);
				if (boxes[i].IsOpen())
				{
					boxes[i].CloseInstant();
				}
				boxes[i].ResetVisualState();
				return boxes[i];
			}
		}
		return null;
	}

	public void PutBackToPool(Box usedBox)
	{
		Debug.Log($"PutBackToPool called for box {usedBox.BoxID}");
		Debug.Log("Stack trace: " + StackTraceUtility.ExtractStackTrace());
		if (usedBox.IsOpen())
		{
			usedBox.CloseInstant();
		}
		DisableBox(usedBox);
	}

	private void DisableBox(Box usedBox)
	{
		usedBox.gameObject.SetActive(value: false);
		usedBox.RigidBody.linearVelocity = Vector3.zero;
		usedBox.RigidBody.angularVelocity = Vector3.zero;
		usedBox.RigidBody.constraints = RigidbodyConstraints.None;
		usedBox.RigidBody.isKinematic = false;
		usedBox.ReleaseBoxReservation();
		usedBox.Collider.enabled = true;
		usedBox.ResetBox();
		usedBox.transform.SetParent(base.transform);
		usedBox.transform.localPosition = Vector3.zero;
		usedBox.transform.localEulerAngles = Vector3.zero;
		usedBox.gameObject.layer = BoxManager.BOX_LAYER;
	}
}
