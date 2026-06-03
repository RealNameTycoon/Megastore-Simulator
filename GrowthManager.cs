using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GrowthManager : SingletonBehaviour<GrowthManager>
{
	[SerializeField]
	private List<GameObject> sectionWalls;

	[SerializeField]
	private List<GameObject> forwardGrowthSections;

	[SerializeField]
	private List<GameObject> rightGrowthSections;

	[SerializeField]
	private List<GameObject> leftGrowthSections;

	[SerializeField]
	private BoxCollider secondDoorTrigger;

	[SerializeField]
	private BoxCollider secondDoorBlocker;

	[SerializeField]
	private List<GameObject> secondDoorDecals;

	private const string FORWARD_GROWTH_PURCHASED_KEY = "FORWARD_GROWTH_PURCHASED_KEYF1";

	private const string RIGHT_GROWTH_PURCHASED_KEY = "RIGHT_GROWTH_PURCHASED_KEYF1";

	private const string LEFT_GROWTH_PURCHASED_KEY = "LEFT_GROWTH_PURCHASED_KEYF1";

	private const string GROWTH_COUNT_KEY = "GROWTH_COUNTF1";

	private const int secondAutoDoorUnlockLevel = 17;

	private int growthCount = -1;

	private int leftGrowthLevel = -1;

	private int rightGrowthLevel = -1;

	private int forwardGrowthLevel = -1;

	private List<GameObject> SectionWalls => sectionWalls;

	public int NumberOfExpansions => SectionWalls.Count;

	public int GrowthCount
	{
		get
		{
			if (growthCount != -1)
			{
				return growthCount;
			}
			growthCount = GenericDataSerializer.LoadInt("GROWTH_COUNTF1");
			return growthCount;
		}
	}

	public void AnimateGrotwh(Transform camera)
	{
		StartCoroutine(GrowthAnimationRoutine(camera));
	}

	private IEnumerator GrowthAnimationRoutine(Transform camera)
	{
		float animDuration = 0.6f;
		for (int i = 0; i < 7; i++)
		{
			if (forwardGrowthSections.Count - 1 >= i)
			{
				forwardGrowthSections[i].SetActive(value: true);
				Vector3 localPosition = forwardGrowthSections[i].transform.localPosition;
				localPosition.y = 0f;
				forwardGrowthSections[i].transform.localPosition = localPosition;
			}
			if (leftGrowthSections.Count - 1 >= i)
			{
				leftGrowthSections[i].SetActive(value: true);
				Vector3 localPosition2 = leftGrowthSections[i].transform.localPosition;
				localPosition2.y = 0f;
				leftGrowthSections[i].transform.localPosition = localPosition2;
			}
			if (rightGrowthSections.Count - 1 >= i)
			{
				rightGrowthSections[i].SetActive(value: true);
				Vector3 localPosition3 = rightGrowthSections[i].transform.localPosition;
				localPosition3.y = 0f;
				rightGrowthSections[i].transform.localPosition = localPosition3;
			}
		}
		for (int j = 0; j < rightGrowthSections.Count; j++)
		{
			if (rightGrowthSections.Count - 1 >= j)
			{
				Vector3 localPosition4 = rightGrowthSections[j].transform.localPosition;
				localPosition4.y = 12f;
				rightGrowthSections[j].transform.DOLocalMove(localPosition4, animDuration);
				camera.DOLookAt(rightGrowthSections[j].transform.position, animDuration);
			}
			yield return new WaitForSeconds(animDuration);
		}
		camera.DOLookAt(forwardGrowthSections[0].transform.position, animDuration);
		yield return new WaitForSeconds(animDuration / 2f);
		for (int j = 0; j < forwardGrowthSections.Count; j++)
		{
			if (forwardGrowthSections.Count - 1 >= j)
			{
				Vector3 localPosition5 = forwardGrowthSections[j].transform.localPosition;
				localPosition5.y = 12f;
				if (j != 0)
				{
					camera.DOLookAt(forwardGrowthSections[j].transform.position, animDuration);
				}
				forwardGrowthSections[j].transform.DOLocalMove(localPosition5, animDuration);
			}
			yield return new WaitForSeconds(animDuration);
		}
		for (int j = 0; j < leftGrowthSections.Count; j++)
		{
			if (leftGrowthSections.Count - 1 >= j)
			{
				Vector3 localPosition6 = leftGrowthSections[j].transform.localPosition;
				localPosition6.y = 12f;
				leftGrowthSections[j].transform.DOLocalMove(localPosition6, animDuration);
				camera.DOLookAt(leftGrowthSections[j].transform.position, animDuration);
			}
			yield return new WaitForSeconds(animDuration);
		}
		Vector3 position = rightGrowthSections[rightGrowthSections.Count - 1].transform.position;
		position.y = 0f;
		camera.DOLookAt(position, animDuration * 8f).SetEase(Ease.Linear);
	}

	private new void Awake()
	{
		base.Awake();
		growthCount = GenericDataSerializer.LoadInt("GROWTH_COUNTF1");
		for (int i = 0; i < SectionWalls.Count; i++)
		{
			if (i < growthCount)
			{
				SectionWalls[i].gameObject.SetActive(value: false);
			}
			else
			{
				SectionWalls[i].gameObject.SetActive(value: true);
			}
		}
		secondDoorTrigger.enabled = growthCount >= 17;
		secondDoorBlocker.enabled = growthCount >= 17;
		for (int j = 0; j < secondDoorDecals.Count; j++)
		{
			if (growthCount >= 17)
			{
				secondDoorDecals[j].gameObject.SetActive(value: true);
			}
			else
			{
				secondDoorDecals[j].gameObject.SetActive(value: false);
			}
		}
	}

	public int NumberOfGrowthsFirstFloor()
	{
		return forwardGrowthSections.Count + rightGrowthSections.Count + leftGrowthSections.Count;
	}

	public bool GrowthPurchased(int growthLevel, GrowthDirection direction)
	{
		if (growthLevel == 0)
		{
			return true;
		}
		switch (direction)
		{
		case GrowthDirection.Forward:
			return GenericDataSerializer.LoadBool("FORWARD_GROWTH_PURCHASED_KEYF1" + growthLevel);
		case GrowthDirection.Right:
			return GenericDataSerializer.LoadBool("RIGHT_GROWTH_PURCHASED_KEYF1" + growthLevel);
		case GrowthDirection.Left:
			return GenericDataSerializer.LoadBool("LEFT_GROWTH_PURCHASED_KEYF1" + growthLevel);
		default:
			Debug.LogError("Invalid GrowthDirection specified: " + direction);
			return false;
		}
	}

	public void PurchaseGrowth()
	{
		SectionWalls[growthCount].gameObject.SetActive(value: false);
		growthCount++;
		secondDoorTrigger.enabled = growthCount >= 17;
		secondDoorBlocker.enabled = growthCount >= 17;
		for (int i = 0; i < secondDoorDecals.Count; i++)
		{
			if (growthCount >= 17)
			{
				secondDoorDecals[i].gameObject.SetActive(value: true);
			}
		}
		GenericDataSerializer.SaveInt("GROWTH_COUNTF1", growthCount);
	}

	public int GetGrowthLevel(GrowthDirection direction)
	{
		switch (direction)
		{
		case GrowthDirection.Forward:
			MonoBehaviour.print("Forward growth level: " + forwardGrowthLevel);
			return forwardGrowthLevel;
		case GrowthDirection.Right:
			MonoBehaviour.print("Right growth level: " + rightGrowthLevel);
			return rightGrowthLevel;
		case GrowthDirection.Left:
			MonoBehaviour.print("Left growth level: " + leftGrowthLevel);
			return leftGrowthLevel;
		default:
			Debug.LogError("Invalid GrowthDirection specified: " + direction);
			return -1;
		}
	}

	public int GetMaxLevel(GrowthDirection direction)
	{
		List<GameObject> list = null;
		switch (direction)
		{
		case GrowthDirection.Forward:
			list = forwardGrowthSections;
			break;
		case GrowthDirection.Right:
			list = rightGrowthSections;
			break;
		case GrowthDirection.Left:
			list = leftGrowthSections;
			break;
		}
		return list.Count;
	}

	public bool IsUpperFloor(int growthLevel)
	{
		if (growthLevel == 0)
		{
			return false;
		}
		return SectionWalls[growthLevel - 1].transform.position.y > 4f;
	}

	public void OnWishlistNow()
	{
		Application.OpenURL("steam://store/3819640");
	}
}
