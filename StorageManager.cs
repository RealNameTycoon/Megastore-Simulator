using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class StorageManager : SingletonBehaviour<StorageManager>
{
	[SerializeField]
	private List<GameObject> sectionWalls;

	[SerializeField]
	private TextMeshProUGUI storageLicenseOwnedText;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private Button purchaseButton;

	[SerializeField]
	private GameObject storageOpenDoors;

	[SerializeField]
	private GameObject storageClosedDoors;

	[SerializeField]
	private Trash palletTrash;

	[SerializeField]
	private List<Transform> restockerIdlePositions;

	[SerializeField]
	private Transform palletTrashDisposalPoint;

	[SerializeField]
	private Transform palletTrashDisposalPointTop;

	[SerializeField]
	private Transform outsideIdlePosition;

	[SerializeField]
	private Transform restockerCorridorPosition;

	[SerializeField]
	private Transform secondFloorStartPosition;

	private const string GROWTH_PURCHASED_KEY = "STORAGE_GROWTH_PURCHASED";

	private const string GROWTH_COUNT_KEY = "STORAGE_GROWTH_COUNT";

	private const string STORAGE_LICENSE_KEY = "STORAGE_LICENSE";

	private bool storageLicensePurchased;

	public static int GROWTHS_NEEDED_PER_DOCK = 5;

	public static int MAX_GROWTH_LEVEL = 29;

	public static int DEFAULT_GIVEN_EXTRA_GROWTH = 1;

	private int growthCount = -1;

	private int idleRestockerPositionIndex = -1;

	private int retryCount;

	private int walkableMask;

	public Trash PalletTrash => palletTrash;

	public Transform PalletTrashDisposalPoint => palletTrashDisposalPoint;

	public Transform PalletTrashDisposalPointTop => palletTrashDisposalPointTop;

	public Transform RestockerCorridorPosition => restockerCorridorPosition;

	public Transform SecondFloorStartPosition => secondFloorStartPosition;

	public bool StorageLicensePurchased => storageLicensePurchased;

	public int GrowthCount => growthCount;

	public Transform GetNextRestockerIdlePosition()
	{
		idleRestockerPositionIndex++;
		Transform transform = restockerIdlePositions[idleRestockerPositionIndex % restockerIdlePositions.Count];
		if (IsPointOnNavMesh(transform.position))
		{
			return transform;
		}
		retryCount++;
		if (retryCount > restockerIdlePositions.Count)
		{
			retryCount = 0;
			return outsideIdlePosition;
		}
		return GetNextRestockerIdlePosition();
	}

	public bool IsPointOnNavMesh(Vector3 point, float maxDistance = 0.2f)
	{
		NavMeshHit hit;
		return NavMesh.SamplePosition(point, out hit, maxDistance, walkableMask);
	}

	private new void Awake()
	{
		base.Awake();
		growthCount = GenericDataSerializer.LoadInt("STORAGE_GROWTH_COUNT");
		growthCount = Mathf.Min(growthCount, MAX_GROWTH_LEVEL);
		RepaintSectionWalls();
		walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
		storageLicensePurchased = GenericDataSerializer.LoadBool("STORAGE_LICENSE");
		if (storageLicensePurchased)
		{
			storageLicenseOwnedText.enabled = true;
			purchaseButton.gameObject.SetActive(value: false);
			priceText.enabled = false;
		}
		bool flag = true;
		storageClosedDoors.gameObject.SetActive(!flag);
		storageOpenDoors.gameObject.SetActive(flag);
	}

	private void RepaintSectionWalls()
	{
		int num = growthCount;
		for (int i = 0; i < sectionWalls.Count; i++)
		{
			if (i < num)
			{
				sectionWalls[i].gameObject.SetActive(value: false);
			}
			else
			{
				sectionWalls[i].gameObject.SetActive(value: true);
			}
		}
	}

	public void OnLicensePurchased()
	{
		storageLicensePurchased = true;
		GenericDataSerializer.SaveBool("STORAGE_LICENSE", value: true);
		storageLicenseOwnedText.enabled = true;
		purchaseButton.gameObject.SetActive(value: false);
		priceText.enabled = false;
		storageClosedDoors.gameObject.SetActive(!storageLicensePurchased);
		storageOpenDoors.gameObject.SetActive(storageLicensePurchased);
		EventManager.NotifyEvent(SupermarketEvents.STORAGE_LICENSE_PURCHASED);
	}

	public bool GrowthPurchased(int growthLevel)
	{
		if (growthLevel == 0)
		{
			return true;
		}
		return GenericDataSerializer.LoadBool("STORAGE_GROWTH_PURCHASED" + growthLevel);
	}

	public void PurchaseGrowth(int growthLevel)
	{
		if (growthCount < sectionWalls.Count)
		{
			sectionWalls[growthCount].gameObject.SetActive(value: false);
		}
		growthCount++;
		if (growthCount == 1)
		{
			EventLogger.StorageUnlocked();
		}
		RepaintSectionWalls();
		GenericDataSerializer.SaveInt("STORAGE_GROWTH_COUNT", growthCount);
		GenericDataSerializer.SaveBool("STORAGE_GROWTH_PURCHASED" + growthLevel, value: true);
		bool flag = true;
		storageClosedDoors.gameObject.SetActive(!flag);
		storageOpenDoors.gameObject.SetActive(flag);
		if (growthLevel == sectionWalls.Count)
		{
			EventLogger.FullExpansionWarehouse();
		}
	}

	public bool IsDockUnlocked(OrderManager.OrderReceivingArea dock)
	{
		return dock switch
		{
			OrderManager.OrderReceivingArea.STORE_FRONT => true, 
			OrderManager.OrderReceivingArea.LOADING_DOCK_1 => true, 
			OrderManager.OrderReceivingArea.LOADING_DOCK_2 => growthCount + DEFAULT_GIVEN_EXTRA_GROWTH >= GROWTHS_NEEDED_PER_DOCK, 
			OrderManager.OrderReceivingArea.LOADING_DOCK_3 => growthCount + DEFAULT_GIVEN_EXTRA_GROWTH >= GROWTHS_NEEDED_PER_DOCK * 2, 
			OrderManager.OrderReceivingArea.LOADING_DOCK_4 => growthCount + DEFAULT_GIVEN_EXTRA_GROWTH >= GROWTHS_NEEDED_PER_DOCK * 3, 
			_ => false, 
		};
	}

	public void ShowWarehouseLicenseRequired()
	{
		SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("unlock_warehouse", base.transform);
	}
}
