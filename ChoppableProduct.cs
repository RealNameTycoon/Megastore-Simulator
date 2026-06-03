using System.Collections.Generic;
using UnityEngine;

public class ChoppableProduct : Product
{
	[SerializeField]
	private ChoppableProductData choppableData;

	[SerializeField]
	private Slice sliceableMesh;

	[SerializeField]
	private GameObject renderersParent;

	[SerializeField]
	private Transform chopPointHead;

	[SerializeField]
	private Transform chopPointTail;

	[SerializeField]
	private int chopCount;

	[SerializeField]
	private bool forwardAlligned;

	[SerializeField]
	private bool sliceable = true;

	[SerializeField]
	private bool disposeLastFragment;

	[SerializeField]
	private List<Transform> cuttingPoints;

	private GameObject lastFragment;

	private bool headChopped;

	private bool tailChopped;

	[SerializeField]
	private bool isFullyChopped;

	[SerializeField]
	private int choppingIndex;

	public bool Sliceable => sliceable;

	public bool DisposeLastFragment => disposeLastFragment;

	public ChoppableProductData ChoppableData => choppableData;

	public bool ForwardAlligned => forwardAlligned;

	public GameObject LastFragment => lastFragment;

	public bool IsFullyChopped => isFullyChopped;

	public int ChoppingIndex => choppingIndex;

	public Vector3[] TrayLocalPositions => choppableData.localPositions;

	public Vector3[] TrayEulerAngles => choppableData.eulerAngles;

	public bool HasCustomCuttingPoints => cuttingPoints.Count > 0;

	public void AssignFields()
	{
		renderersParent = base.PrimaryRenderer.transform.parent.gameObject;
		sliceableMesh = base.gameObject.GetComponentInChildren<Slice>();
		chopPointHead = FindRecursive(base.gameObject.transform, "HeadChopPoint");
		chopPointTail = FindRecursive(base.gameObject.transform, "TailChopPoint");
	}

	public Transform GetNextCuttingPoint()
	{
		if (cuttingPoints.Count > 0)
		{
			return cuttingPoints[choppingIndex];
		}
		return null;
	}

	private void Awake()
	{
		_ = sliceable;
	}

	private void OnCompleted()
	{
	}

	public void OnPlacedOnChoppingStand()
	{
		if (sliceable)
		{
			sliceableMesh.gameObject.SetActive(value: true);
			renderersParent.SetActive(value: false);
		}
		else
		{
			EnableRenderers(enable: true);
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public Vector3 GetNextChopPoint()
	{
		if (!headChopped && chopPointHead != null)
		{
			return chopPointHead.position;
		}
		if (!tailChopped && chopPointTail != null)
		{
			return chopPointTail.position;
		}
		if (cuttingPoints.Count > 0)
		{
			return cuttingPoints[choppingIndex].position;
		}
		return Vector3.Lerp(chopPointHead.position, chopPointTail.position, (float)(choppingIndex + 1) / ((float)chopCount + 1f));
	}

	public bool ShouldDisposeNextCut()
	{
		if ((!tailChopped && chopPointTail != null) || (!headChopped && chopPointHead != null))
		{
			return true;
		}
		return false;
	}

	public GameObject Cut()
	{
		if (choppingIndex >= chopCount || isFullyChopped)
		{
			return null;
		}
		Vector3 sliceNormalWorld = (forwardAlligned ? base.transform.forward : base.transform.right);
		if (!headChopped && chopPointHead != null)
		{
			(GameObject topFragment, GameObject bottomFragment) tuple = sliceableMesh.ComputeSlice(sliceNormalWorld, chopPointHead.position);
			GameObject item = tuple.topFragment;
			GameObject item2 = tuple.bottomFragment;
			headChopped = true;
			lastFragment = item;
			return item2;
		}
		if (!tailChopped && chopPointTail != null)
		{
			(GameObject topFragment, GameObject bottomFragment) tuple2 = lastFragment.GetComponent<Slice>().ComputeSlice(sliceNormalWorld, chopPointTail.position);
			GameObject item3 = tuple2.topFragment;
			GameObject item4 = tuple2.bottomFragment;
			tailChopped = true;
			lastFragment = item4;
			return item3;
		}
		Vector3 zero = Vector3.zero;
		if (cuttingPoints.Count > 0)
		{
			zero = cuttingPoints[choppingIndex].position;
			sliceNormalWorld = cuttingPoints[choppingIndex].forward;
		}
		else
		{
			zero = Vector3.Lerp(chopPointHead.position, chopPointTail.position, (float)(choppingIndex + 1) / ((float)chopCount + 1f));
		}
		(GameObject topFragment, GameObject bottomFragment) tuple3 = ((lastFragment != null) ? lastFragment.GetComponent<Slice>() : sliceableMesh).ComputeSlice(sliceNormalWorld, zero);
		GameObject item5 = tuple3.topFragment;
		GameObject item6 = tuple3.bottomFragment;
		lastFragment = item5;
		if (choppingIndex == chopCount - 1)
		{
			isFullyChopped = true;
		}
		choppingIndex++;
		return item6;
	}

	public override void ResetProduct()
	{
		if (sliceable)
		{
			sliceableMesh.gameObject.SetActive(value: false);
			renderersParent.SetActive(value: true);
		}
		choppingIndex = 0;
		isFullyChopped = false;
		headChopped = false;
		tailChopped = false;
		if (lastFragment != null)
		{
			lastFragment = null;
		}
	}

	public Transform FindRecursive(Transform trm, string name)
	{
		Transform transform = null;
		foreach (Transform item in trm)
		{
			if (item.name == name)
			{
				return item;
			}
			if (item.childCount > 0)
			{
				transform = FindRecursive(item, name);
				if ((bool)transform)
				{
					return transform;
				}
			}
		}
		return transform;
	}
}
