using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Restocker : Employee
{
	public enum IdleReason
	{
		NONE,
		NO_JOBS,
		NO_BOXES,
		NO_OVEN,
		NO_TRAYS
	}

	[Header("Settings")]
	[SerializeField]
	private float trolleyDistanceThreshold = 5f;

	[SerializeField]
	private bool preferLabeled = true;

	[Header("References")]
	[SerializeField]
	private Transform trolley;

	[SerializeField]
	private Transform trolleyLoadingPoint;

	[SerializeField]
	private Transform trolleyPickupPoint;

	[SerializeField]
	private Transform boxStartPosition;

	[SerializeField]
	private Transform hand;

	[SerializeField]
	private Transform escalatorTrolleyPosition;

	[Header("Debug")]
	[SerializeField]
	private List<RestockJob> assignedJobs = new List<RestockJob>();

	[SerializeField]
	private List<Box> boxesToTake = new List<Box>();

	[SerializeField]
	private List<Box> boxesOnTrolley = new List<Box>();

	[SerializeField]
	private GameObject speechTextPrefab;

	[SerializeField]
	private TextMeshPro speechText;

	private Transform originalParent;

	private WaitForSeconds waiter = new WaitForSeconds(2f);

	private Box currentBoxInHand;

	[SerializeField]
	private float currentBoxHeight;

	private RestockerPhase currentPhase;

	private HiringManager.EmployeeStats employeeStats;

	private ProductGroup currentDepartment = ProductGroup.NONE;

	private Coroutine restockingCoroutine;

	private const int MAX_BOX_COUNT = 5;

	private const float MAX_CARRY_HEIGHT = 1.2f;

	private const float BOX_TAKE_DURATION = 0.6f;

	private const float REQUIRED_DISTANCE_TO_TARGET = 1f;

	private const float TROLLEY_STOPPING_DISTANCE = 2.5f;

	private const int MAX_SHOWN_ITEM_COUNT = 3;

	[SerializeField]
	private bool quitWhenReady;

	private Coroutine tryAssignNewJobsCoroutine;

	private WaitForSeconds placeIntervalWait = new WaitForSeconds(0.3f);

	[SerializeField]
	private bool maxCapacityReached;

	[SerializeField]
	private bool returnBackToPalletRacks = true;

	private Vector3 initialTrolleyPosition;

	[SerializeField]
	private bool isDeactivating;

	public IdleReason idleReason;

	[SerializeField]
	private SerializedDictionary<Box, Pallet> boxReturnPallets = new SerializedDictionary<Box, Pallet>();

	[SerializeField]
	private SerializedDictionary<Box, BoxStorageUnit> boxReturnBoxStorageUnits = new SerializedDictionary<Box, BoxStorageUnit>();

	private Tween virtualCallDelay;

	private static float SPEED_BALANCING_FACTOR = 1.15f;

	private bool includeWarehouse;

	public bool PreferLabeled
	{
		get
		{
			return preferLabeled;
		}
		set
		{
			preferLabeled = value;
		}
	}

	public RestockerPhase CurrentPhase => currentPhase;

	private void Awake()
	{
		initialTrolleyPosition = trolley.localPosition;
	}

	public override void SwitchPauseState()
	{
		base.SwitchPauseState();
		if (isPaused && currentPhase != RestockerPhase.IDLE)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltipWithFullText(Locale.GetWord("employee_break_n").Replace("{0}", employeeStats.employeeName), base.transform);
		}
		if (!isPaused && speechText.enabled)
		{
			speechText.enabled = false;
		}
	}

	public void Activate(HiringManager.EmployeeStats stats)
	{
		if (originalParent == null)
		{
			originalParent = base.transform.parent;
		}
		returnBackToPalletRacks = stats.returnToRacks;
		isDeactivating = false;
		quitWhenReady = false;
		employeeStats = stats;
		currentDepartment = employeeStats.department;
		placeIntervalWait = new WaitForSeconds(0.3f / ((float)employeeStats.workSpeed * SPEED_BALANCING_FACTOR / 100f));
		isPaused = stats.isPaused;
		isActive = true;
		Transform nextRestockerIdlePosition = SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition();
		base.transform.position = nextRestockerIdlePosition.position;
		base.transform.rotation = nextRestockerIdlePosition.rotation;
		idleReason = IdleReason.NONE;
		if (base.gameObject.activeSelf)
		{
			characterMovement.StopMoving();
		}
		else
		{
			base.gameObject.SetActive(value: true);
			characterMovement.EnableNavmesh(enable: true);
		}
		includeWarehouse = stats.includeWarehouse;
		characterMovement.SetSpeedMultiplier(SPEED_BALANCING_FACTOR * (float)stats.moveSpeed / 100f, SPEED_BALANCING_FACTOR * (float)stats.workSpeed / 100f);
		SetPhase(RestockerPhase.IDLE);
		tryAssignNewJobsCoroutine = StartCoroutine(WaitAndTryAssignNewJobs());
	}

	private IEnumerator WaitAndTryAssignNewJobs()
	{
		while (true)
		{
			yield return waiter;
			if (isActive && currentPhase == RestockerPhase.IDLE)
			{
				AssignNewJobs();
			}
		}
	}

	public void TrySwitchDepartment(ProductGroup newDepartment)
	{
		if (currentDepartment != newDepartment)
		{
			currentDepartment = newDepartment;
			if (isActive && currentPhase == RestockerPhase.IDLE)
			{
				AssignNewJobs();
			}
		}
	}

	public void TrySwitchIncludeWarehouse(bool newIncludeWarehouse)
	{
		if (includeWarehouse != newIncludeWarehouse)
		{
			includeWarehouse = newIncludeWarehouse;
			if (isActive && currentPhase == RestockerPhase.IDLE)
			{
				AssignNewJobs();
			}
		}
	}

	public void SetReturnToRacks(bool newReturnToRacks)
	{
		if (returnBackToPalletRacks != newReturnToRacks)
		{
			returnBackToPalletRacks = newReturnToRacks;
		}
	}

	private void SetPhase(RestockerPhase newPhase)
	{
		if (!isDeactivating && currentPhase != newPhase)
		{
			currentPhase = newPhase;
			switch (newPhase)
			{
			case RestockerPhase.RETURNING_HALF_BOXES:
				ExecuteReturnHalfBoxes();
				break;
			case RestockerPhase.DISPOSING_EMPTY_BOXES:
				ExecuteDisposeEmptyBoxes();
				break;
			}
		}
	}

	private void GoToIdlePosition()
	{
		SetPhase(RestockerPhase.IDLE);
		Transform idlePosition = SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition();
		MoveToOrExecute(idlePosition, delegate
		{
			characterMovement.Animator.SetTrigger("idle");
			characterMovement.Animator.SetTrigger("handIdle");
			base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, idlePosition.rotation.eulerAngles.y, base.transform.eulerAngles.z), 0.25f);
		}, 0f);
	}

	private void AssignNewJobs()
	{
		if (quitWhenReady || isDeactivating)
		{
			return;
		}
		if (IsPaused())
		{
			speechText.text = Locale.GetWord("employee_paused_desc");
			speechText.enabled = true;
			return;
		}
		ResetParameters();
		float num = 0f;
		while (boxesToTake.Count < 5 && num < 1.2f)
		{
			var (restockJob, idleReason, hashSet) = SingletonBehaviour<RestockerManager>.Instance.RequestJobByProductGroup(this, currentDepartment, preferLabeled, includeWarehouse);
			if (restockJob == null)
			{
				if (assignedJobs.Count != 0)
				{
					break;
				}
				this.idleReason = idleReason;
				if (idleReason == IdleReason.NO_JOBS)
				{
					speechText.text = Locale.GetWord("restocker_no_jobs_desc").Replace("{0}", "%" + SingletonBehaviour<RestockerManager>.Instance.RestockThreshold);
				}
				else if (hashSet.Count > 0)
				{
					string text = "";
					int num2 = Mathf.Min(hashSet.Count, 3);
					for (int i = 0; i < num2; i++)
					{
						text = text + Locale.GetWord(hashSet.ElementAt(i).ToString()) + ",";
					}
					text = text.TrimEnd(',');
					speechText.text = Locale.GetWord("restocker_no_box_desc_n").Replace("{0}", text);
				}
				else
				{
					speechText.text = Locale.GetWord("restocker_no_box_desc");
				}
				speechText.enabled = true;
				break;
			}
			this.idleReason = IdleReason.NONE;
			speechText.enabled = false;
			Box boxWithProductType = SingletonBehaviour<RestockZoneManager>.Instance.GetBoxWithProductType(restockJob.ProductType, includeWarehouse);
			int count = assignedJobs.Count;
			assignedJobs.Add(restockJob);
			boxesToTake.Add(boxWithProductType);
			num += boxWithProductType.GetHeight() / trolley.lossyScale.y;
			int num3 = boxWithProductType.ProductCount - restockJob.NeededAmount;
			while (num3 > 0)
			{
				RestockJob restockJob2 = SingletonBehaviour<RestockerManager>.Instance.RequestJobByType(this, restockJob.ProductType);
				if (restockJob2 == null)
				{
					break;
				}
				assignedJobs.Add(restockJob2);
				num3 -= restockJob2.NeededAmount;
			}
			for (int j = count; j < assignedJobs.Count; j++)
			{
				assignedJobs[j].ReserveBox(boxWithProductType, GetEmployeeID());
			}
			if (num >= 1.2f)
			{
				maxCapacityReached = true;
				break;
			}
		}
		if (boxesToTake.Count != 0)
		{
			ReorderBoxesToTake();
			ReorderAssignedJobs();
			SetPhase(RestockerPhase.TAKING_BOXES);
			StartTakingBoxes();
		}
	}

	private void ReorderBoxesToTake()
	{
		if (boxesToTake.Count <= 1)
		{
			return;
		}
		List<Box> list = new List<Box>();
		List<Box> list2 = new List<Box>();
		foreach (Box item in boxesToTake)
		{
			(item.IsInReturnArea ? list2 : list).Add(item);
		}
		List<Box> list3 = new List<Box>(boxesToTake.Count);
		OrderBoxesByDistance(list, list3, base.transform.position);
		list3.AddRange(list2);
		boxesToTake.Clear();
		boxesToTake.AddRange(list3);
	}

	private void ReorderAssignedJobs()
	{
		if (assignedJobs.Count <= 1)
		{
			return;
		}
		List<RestockJob> list = new List<RestockJob>(assignedJobs.Count);
		List<RestockJob> list2 = new List<RestockJob>(assignedJobs.Count);
		foreach (RestockJob assignedJob in assignedJobs)
		{
			(IsInSecondFloor(assignedJob) ? list2 : list).Add(assignedJob);
		}
		List<RestockJob> list3 = new List<RestockJob>(assignedJobs.Count);
		OrderJobsByDistance(list, list3, SingletonBehaviour<StorageManager>.Instance.RestockerCorridorPosition.position);
		OrderJobsByDistance(list2, list3, SingletonBehaviour<StorageManager>.Instance.SecondFloorStartPosition.position);
		assignedJobs.Clear();
		assignedJobs.AddRange(list3);
	}

	private bool IsInSecondFloor(RestockJob job)
	{
		return job.Shelf.PickupPoint().position.y > 4f;
	}

	private void OrderJobsByDistance(List<RestockJob> remaining, List<RestockJob> ordered, Vector3 fromPosition)
	{
		while (remaining.Count > 0)
		{
			int nearestJobIndex = GetNearestJobIndex(remaining, fromPosition);
			RestockJob restockJob = remaining[nearestJobIndex];
			remaining.RemoveAt(nearestJobIndex);
			ordered.Add(restockJob);
			fromPosition = restockJob.Shelf.PickupPoint().position;
		}
	}

	private int GetNearestJobIndex(List<RestockJob> jobs, Vector3 fromPosition)
	{
		int result = 0;
		float num = float.MaxValue;
		for (int i = 0; i < jobs.Count; i++)
		{
			float sqrMagnitude = (jobs[i].Shelf.PickupPoint().position - fromPosition).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
			}
		}
		return result;
	}

	private void OrderBoxesByDistance(List<Box> remaining, List<Box> ordered, Vector3 fromPosition)
	{
		while (remaining.Count > 0)
		{
			int nearestBoxIndex = GetNearestBoxIndex(remaining, fromPosition);
			Box box = remaining[nearestBoxIndex];
			remaining.RemoveAt(nearestBoxIndex);
			ordered.Add(box);
			fromPosition = GetBoxPickupPosition(box);
		}
	}

	private int GetNearestBoxIndex(List<Box> boxes, Vector3 fromPosition)
	{
		int result = 0;
		float num = float.MaxValue;
		for (int i = 0; i < boxes.Count; i++)
		{
			float sqrMagnitude = (GetBoxPickupPosition(boxes[i]) - fromPosition).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
			}
		}
		return result;
	}

	private Vector3 GetBoxPickupPosition(Box box)
	{
		if (box.IsInReturnArea)
		{
			return SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.GetBoxTakePosition(box.Type).position;
		}
		if (box.ContainedPalletID != -1)
		{
			Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(box.ContainedPalletID);
			if (pallet != null && pallet.ContainerShelf != null)
			{
				return pallet.ContainerShelf.StaffInteractionPoint().position;
			}
		}
		return box.transform.position;
	}

	private void ResetParameters()
	{
		assignedJobs.Clear();
		boxesToTake.Clear();
		boxesOnTrolley.Clear();
		boxReturnPallets.Clear();
		boxReturnBoxStorageUnits.Clear();
		currentBoxHeight = 0f;
		maxCapacityReached = false;
		quitWhenReady = false;
	}

	private void ReturnJob(RestockJob job)
	{
		if (assignedJobs.Remove(job))
		{
			job.ClearBoxReservation();
			SingletonBehaviour<RestockerManager>.Instance.UnassignJob(job);
		}
	}

	private void StartTakingBoxes()
	{
		if (isDeactivating)
		{
			return;
		}
		if (boxesToTake.Count == 0)
		{
			GoToIdlePosition();
			return;
		}
		Box box = boxesToTake[0];
		Transform transform = box.transform;
		if (box.ContainedPalletID != -1)
		{
			Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(box.ContainedPalletID);
			if (pallet != null && pallet.ContainerShelf != null && SingletonBehaviour<StorageManager>.Instance.IsPointOnNavMesh(pallet.ContainerShelf.StaffInteractionPoint().position))
			{
				transform = pallet.ContainerShelf.StaffInteractionPoint();
			}
		}
		if (box.IsInReturnArea)
		{
			transform = SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.GetBoxTakePosition(box.Type);
		}
		if (Vector3.Distance(base.transform.position, transform.position) > characterMovement.navmeshAgent.stoppingDistance + 1f)
		{
			characterMovement.Animator.SetTrigger("walk");
			SetHandAnimation();
			TryMove(transform, delegate
			{
				characterMovement.Animator.SetTrigger("idle");
				TakeBoxToTrolley();
			}, 1f, firstFloorOnly: true);
		}
		else
		{
			TakeBoxToTrolley();
		}
	}

	private void TakeBoxToTrolley()
	{
		if (isDeactivating || boxesToTake.Count == 0)
		{
			return;
		}
		Box box = boxesToTake[0];
		if (box.ContainedPalletID != -1)
		{
			Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(box.ContainedPalletID);
			if (pallet != null)
			{
				if (returnBackToPalletRacks && pallet.ContainerShelf != null && !box.IsDisposable())
				{
					boxReturnPallets.Add(box, pallet);
					pallet.RemoveBox(box, returnBackToPalletRacks);
				}
				else
				{
					pallet.RemoveBox(box);
				}
				box.SaveLocation();
			}
			else
			{
				SingletonBehaviour<BoxManager>.Instance.WakeUpBoxesAbove(box);
			}
		}
		if (box.ContainedBoxStorageUnit != null)
		{
			if (returnBackToPalletRacks && !box.IsDisposable())
			{
				boxReturnBoxStorageUnits.Add(box, box.ContainedBoxStorageUnit);
				box.ContainedBoxStorageUnit.RemoveBox(box, returnBackToPalletRacks);
			}
			else
			{
				box.ContainedBoxStorageUnit.RemoveBox(box);
			}
			box.SaveLocation();
		}
		boxesToTake.RemoveAt(0);
		box.OnBoxTakenByNPC();
		boxesOnTrolley.Add(box);
		box.transform.parent = boxStartPosition.parent;
		Vector3 to = boxStartPosition.localPosition + currentBoxHeight * Vector3.up;
		to += box.GetLength() / trolley.lossyScale.x / 2f * Vector3.forward;
		box.transform.DOKill();
		box.transform.DoCurvedLocalMove(to, 0.6f, 2f).OnComplete(delegate
		{
			if (boxesToTake.Count > 0)
			{
				StartTakingBoxes();
			}
			else
			{
				StartRestocking();
			}
		});
		box.transform.DOLocalRotate(boxStartPosition.localEulerAngles, 0.6f);
		currentBoxHeight += box.GetHeight() / trolley.lossyScale.y;
	}

	private void StartRestocking()
	{
		if (!isDeactivating)
		{
			SetPhase(RestockerPhase.RESTOCKING_SHELVES);
			GetNextBoxFromTrolley();
		}
	}

	private void GetNextBoxFromTrolley()
	{
		if (isDeactivating)
		{
			return;
		}
		if (assignedJobs.Count == 0)
		{
			FinishRestocking();
			return;
		}
		RestockJob restockJob = assignedJobs[0];
		Box reservedBox = restockJob.GetReservedBox();
		if (reservedBox == null || !boxesOnTrolley.Contains(reservedBox))
		{
			ReturnRemainingJobsForCurrentBox();
			if (assignedJobs.Count == 0)
			{
				FinishRestocking();
			}
			else
			{
				GetNextBoxFromTrolley();
			}
		}
		else
		{
			float num = Vector3.Distance(trolley.position, restockJob.Shelf.GetTransform().position);
			GetBoxFromTrolley(reservedBox, num < trolleyDistanceThreshold);
		}
	}

	private void GetBoxFromTrolley(Box box, bool trolleyIsClose)
	{
		RestockJob restockJob = assignedJobs[0];
		if (trolleyIsClose)
		{
			trolley.DOKill();
			trolley.SetParent(null);
			MoveToOrExecute(trolleyLoadingPoint, delegate
			{
				RotateTowards(trolleyLoadingPoint.eulerAngles.y, delegate
				{
					TakeBoxFromTrolley(box, delegate
					{
						ProcessCurrentJob(moveToShelf: true);
					});
				});
			});
			return;
		}
		Transform pickupPoint = restockJob.Shelf.PickupPoint();
		MoveToOrExecute(pickupPoint, delegate
		{
			RotateTowards(TargetParkRotation(pickupPoint).y, delegate
			{
				trolley.DOKill();
				trolley.SetParent(null);
				MoveToOrExecute(trolleyLoadingPoint, delegate
				{
					RotateTowards(trolleyLoadingPoint.eulerAngles.y, delegate
					{
						TakeBoxFromTrolley(box, delegate
						{
							ProcessCurrentJob(moveToShelf: true);
						});
					});
				}, 0f);
			});
		}, 2.5f);
	}

	private Vector3 TargetParkRotation(Transform placeableTransform)
	{
		float y = placeableTransform.eulerAngles.y;
		float y2 = trolley.eulerAngles.y;
		float num = y + 90f;
		float num2 = y - 90f;
		float num3 = Mathf.Abs(Mathf.DeltaAngle(y2, num));
		float num4 = Mathf.Abs(Mathf.DeltaAngle(y2, num2));
		float y3 = ((num3 <= num4) ? num : num2);
		Vector3 eulerAngles = trolley.eulerAngles;
		eulerAngles.x = 0f;
		eulerAngles.y = y3;
		eulerAngles.z = 0f;
		return eulerAngles;
	}

	private void TakeBoxFromTrolley(Box box, Action onComplete)
	{
		RemoveBoxFromTrolley(box);
		currentBoxInHand = box;
		SetHandAnimation();
		box.transform.parent = hand;
		box.transform.DOLocalMove(Vector3.zero, 0.35f);
		box.transform.DOLocalRotate(Vector3.zero, 0.35f).OnComplete(delegate
		{
			box.Open(playSound: false);
			onComplete?.Invoke();
		});
	}

	private void ProcessCurrentJob(bool moveToShelf = false)
	{
		if (isDeactivating)
		{
			return;
		}
		if (assignedJobs.Count == 0)
		{
			FinishCurrentBox();
			return;
		}
		RestockJob restockJob = assignedJobs[0];
		Box reservedBox = restockJob.GetReservedBox();
		if (currentBoxInHand != reservedBox)
		{
			FinishCurrentBox();
			return;
		}
		if (currentBoxInHand.IsDisposable())
		{
			ReturnRemainingJobsForCurrentBox();
			FinishCurrentBox();
			return;
		}
		float num = Vector3.Distance(base.transform.position, restockJob.Shelf.GetTransform().position);
		bool num2 = base.transform.position.y > 4f;
		bool flag = restockJob.Shelf.PickupPoint().position.y > 4f;
		bool flag2 = num2 != flag;
		num = (moveToShelf ? 0f : num);
		if (num > trolleyDistanceThreshold || flag2)
		{
			PutBoxBackAndMoveToShelf(restockJob);
		}
		else
		{
			MoveToShelfAndRestock();
		}
	}

	private void PutBoxBackAndMoveToShelf(RestockJob job)
	{
		MoveToOrExecute(trolleyLoadingPoint, delegate
		{
			characterMovement.Animator.SetTrigger("idle");
			RotateTowards(trolleyLoadingPoint.eulerAngles.y, delegate
			{
				PutBoxBackOnTrolley(delegate
				{
					Box boxToRetrieve = job.GetReservedBox();
					if (trolley.parent != base.transform)
					{
						TakeTrolley(delegate
						{
							GetBoxFromTrolley(boxToRetrieve, trolleyIsClose: false);
						});
					}
					else
					{
						GetBoxFromTrolley(boxToRetrieve, trolleyIsClose: false);
					}
				});
			});
		}, 0f);
	}

	private void MoveToShelfAndRestock()
	{
		RestockJob currentJob = assignedJobs[0];
		Transform target = currentJob.Shelf.PickupPoint();
		Action startRestocking = delegate
		{
			RotateToFace(currentJob.Shelf.PickupPoint(), delegate
			{
				characterMovement.Animator.SetTrigger("idle");
				if (restockingCoroutine != null)
				{
					StopCoroutine(restockingCoroutine);
				}
				restockingCoroutine = StartCoroutine(PlaceItemsOnShelf(currentJob));
			}, faceForward: true);
		};
		if (IsCloseEnoughTo(target))
		{
			startRestocking();
			return;
		}
		characterMovement.Animator.SetTrigger("walk");
		TryMove(target, delegate
		{
			startRestocking();
		}, 0.5f);
	}

	private IEnumerator PlaceItemsOnShelf(RestockJob job)
	{
		Restockable shelf = job.Shelf;
		if (currentBoxInHand == null || currentBoxInHand.IsDisposable())
		{
			ReturnRemainingJobsForCurrentBox();
			FinishCurrentBox();
			yield break;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(job.ProductType);
		int capacity = shelf.GetCapacityForProduct(productData);
		yield return placeIntervalWait;
		while (currentBoxInHand != null && !currentBoxInHand.IsDisposable() && shelf.GetProductCount() < capacity)
		{
			shelf.PlaceProduct(currentBoxInHand);
			yield return placeIntervalWait;
		}
		if (currentBoxInHand != null && currentBoxInHand.IsDisposable())
		{
			if (boxReturnPallets.ContainsKey(currentBoxInHand))
			{
				boxReturnPallets[currentBoxInHand].OnReturnBoxCanceled();
				boxReturnPallets.Remove(currentBoxInHand);
			}
			else if (boxReturnBoxStorageUnits.ContainsKey(currentBoxInHand))
			{
				boxReturnBoxStorageUnits[currentBoxInHand].OnReturnBoxCanceled();
				boxReturnBoxStorageUnits.Remove(currentBoxInHand);
			}
		}
		CompleteJob(job);
		ProcessCurrentJob();
	}

	private void CompleteJob(RestockJob job)
	{
		int num = assignedJobs.IndexOf(job);
		if (num >= 0)
		{
			assignedJobs.RemoveAt(num);
		}
		job.ClearBoxReservation();
		SingletonBehaviour<RestockerManager>.Instance.UnassignJob(job);
	}

	private void ReturnRemainingJobsForCurrentBox()
	{
		if (assignedJobs.Count == 0)
		{
			return;
		}
		List<RestockJob> list = new List<RestockJob>();
		for (int i = 0; i < assignedJobs.Count; i++)
		{
			if (assignedJobs[i].GetReservedBox() == currentBoxInHand)
			{
				RestockJob item = assignedJobs[i];
				list.Add(item);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			RestockJob restockJob = list[j];
			restockJob.ClearBoxReservation();
			SingletonBehaviour<RestockerManager>.Instance.UnassignJob(restockJob);
			assignedJobs.Remove(restockJob);
		}
	}

	private void FinishCurrentBox()
	{
		if (currentBoxInHand == null)
		{
			if (assignedJobs.Count == 0)
			{
				FinishRestocking();
			}
			else
			{
				GetNextBoxFromTrolley();
			}
			return;
		}
		MoveToOrExecute(trolleyLoadingPoint, delegate
		{
			characterMovement.Animator.SetTrigger("idle");
			RotateTowards(trolleyLoadingPoint.eulerAngles.y, delegate
			{
				PutBoxBackOnTrolley(delegate
				{
					if (assignedJobs.Count == 0)
					{
						if (trolley.parent != base.transform)
						{
							TakeTrolley(delegate
							{
								FinishRestocking();
							});
						}
						else
						{
							FinishRestocking();
						}
					}
					else if (Vector3.Distance(trolley.position, assignedJobs[0].Shelf.GetTransform().position) > trolleyDistanceThreshold && trolley.parent != base.transform)
					{
						TakeTrolley(delegate
						{
							GetNextBoxFromTrolley();
						});
					}
					else
					{
						GetNextBoxFromTrolley();
					}
				});
			});
		}, 0f);
	}

	private void FinishRestocking()
	{
		if (HasPalletShelvesToReturn())
		{
			MoveAndReturnNextPalletBox();
		}
		else
		{
			MoveToReturnOrDispose();
		}
	}

	private void MoveToReturnOrDispose()
	{
		if (HasBoxesToReturn())
		{
			Transform returnArea = SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.GetNextRestockerSlot();
			MoveToOrExecute(returnArea, delegate
			{
				RotateToFace(returnArea, delegate
				{
					SetPhase(RestockerPhase.RETURNING_HALF_BOXES);
				});
			});
		}
		else if (boxesOnTrolley.Count > 0)
		{
			MoveToOrExecute(SingletonBehaviour<StorageManager>.Instance.PalletTrashDisposalPoint, delegate
			{
				RotateToFace(SingletonBehaviour<StorageManager>.Instance.PalletTrashDisposalPoint, delegate
				{
					SetPhase(RestockerPhase.DISPOSING_EMPTY_BOXES);
				});
			});
		}
		else
		{
			if (boxesOnTrolley.Count != 0)
			{
				return;
			}
			if (quitWhenReady)
			{
				GoToIdleAndDeactivate();
				return;
			}
			currentBoxHeight = 0f;
			AssignNewJobs();
			if (assignedJobs.Count == 0)
			{
				GoToIdlePosition();
			}
		}
	}

	private bool HasBoxesToReturn()
	{
		for (int i = 0; i < boxesOnTrolley.Count; i++)
		{
			if (!boxesOnTrolley[i].IsDisposable())
			{
				return true;
			}
		}
		return false;
	}

	private bool HasPalletShelvesToReturn()
	{
		for (int i = 0; i < boxesOnTrolley.Count; i++)
		{
			if (!boxesOnTrolley[i].IsDisposable() && (boxReturnPallets.ContainsKey(boxesOnTrolley[i]) || boxReturnBoxStorageUnits.ContainsKey(boxesOnTrolley[i])))
			{
				return true;
			}
		}
		return false;
	}

	private (Box, Transform) GetNextPalletShelfToReturn()
	{
		foreach (KeyValuePair<Box, Pallet> boxReturnPallet in boxReturnPallets)
		{
			if (boxesOnTrolley.Contains(boxReturnPallet.Key) && boxReturnPallet.Value.ContainerShelf != null)
			{
				return (boxReturnPallet.Key, boxReturnPallet.Value.ContainerShelf.PickupPoint());
			}
		}
		foreach (KeyValuePair<Box, BoxStorageUnit> boxReturnBoxStorageUnit in boxReturnBoxStorageUnits)
		{
			if (boxesOnTrolley.Contains(boxReturnBoxStorageUnit.Key))
			{
				return (boxReturnBoxStorageUnit.Key, boxReturnBoxStorageUnit.Value.PickupPoint());
			}
		}
		return (null, null);
	}

	private void MoveAndReturnNextPalletBox()
	{
		var (boxToReturn, getNextPalletShelf) = GetNextPalletShelfToReturn();
		if (boxToReturn == null || getNextPalletShelf == null)
		{
			MoveToReturnOrDispose();
			return;
		}
		MoveToOrExecute(getNextPalletShelf, delegate
		{
			RotateToFace(getNextPalletShelf, delegate
			{
				ReturnBoxToPallet(boxToReturn);
			});
		});
	}

	private void ReturnBoxToPallet(Box box)
	{
		if (boxReturnPallets.ContainsKey(box))
		{
			Pallet pallet = boxReturnPallets[box];
			if (pallet.ContainerShelf != null)
			{
				pallet.ContainerShelf.PlaceReturnBox(box);
			}
			boxReturnPallets.Remove(box);
		}
		else if (boxReturnBoxStorageUnits.ContainsKey(box))
		{
			boxReturnBoxStorageUnits[box].PlaceReturnBox(box);
			boxReturnBoxStorageUnits.Remove(box);
		}
		RemoveBoxFromTrolley(box);
		MoveAndReturnNextPalletBox();
	}

	private void ExecuteReturnHalfBoxes()
	{
		List<Box> list = new List<Box>();
		for (int i = 0; i < boxesOnTrolley.Count; i++)
		{
			if (!boxesOnTrolley[i].IsDisposable())
			{
				list.Add(boxesOnTrolley[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Box box = list[j];
			if (boxReturnPallets.ContainsKey(box))
			{
				boxReturnPallets[box].OnReturnBoxCanceled();
				boxReturnPallets.Remove(box);
			}
			else if (boxReturnBoxStorageUnits.ContainsKey(box))
			{
				boxReturnBoxStorageUnits[box].OnReturnBoxCanceled();
				boxReturnBoxStorageUnits.Remove(box);
			}
			RemoveBoxFromTrolley(box);
			if (j == list.Count - 1)
			{
				SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.PlaceBox(box, instant: false, delegate
				{
					SetPhase(RestockerPhase.DISPOSING_EMPTY_BOXES);
				});
			}
			else
			{
				SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.PlaceBox(box);
			}
		}
		list.Clear();
	}

	private void ExecuteDisposeEmptyBoxes()
	{
		if (boxesOnTrolley.Count == 0)
		{
			if (quitWhenReady)
			{
				GoToIdleAndDeactivate();
				return;
			}
			currentBoxHeight = 0f;
			AssignNewJobs();
			if (assignedJobs.Count == 0)
			{
				GoToIdlePosition();
			}
			return;
		}
		MoveToOrExecute(SingletonBehaviour<StorageManager>.Instance.PalletTrashDisposalPoint, delegate
		{
			RotateToFace(SingletonBehaviour<StorageManager>.Instance.PalletTrashDisposalPoint, delegate
			{
				foreach (Box box in boxesOnTrolley)
				{
					box.transform.SetParent(null);
					box.transform.DOKill();
					box.transform.DoCurvedMove(SingletonBehaviour<StorageManager>.Instance.PalletTrash.BoxTargetTop.position, 0.5f, SingletonBehaviour<StorageManager>.Instance.PalletTrashDisposalPointTop.position).OnComplete(delegate
					{
						DisposeBox(box);
					});
				}
				boxesOnTrolley.Clear();
				currentBoxHeight = 0f;
				if (quitWhenReady)
				{
					GoToIdleAndDeactivate();
				}
				else
				{
					AssignNewJobs();
					if (assignedJobs.Count == 0)
					{
						GoToIdlePosition();
					}
				}
			});
		});
	}

	private void DisposeBox(Box box)
	{
		if (box != null && !box.IsDisposable())
		{
			SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.PlaceBox(box, instant: true);
		}
		else
		{
			SingletonBehaviour<StorageManager>.Instance.PalletTrash.DeleteBoxTrash(box);
		}
	}

	private void GoToIdleAndDeactivate()
	{
		Transform nextRestockerIdlePosition = SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition();
		MoveToOrExecute(nextRestockerIdlePosition, delegate
		{
			DeactivateInstant();
		});
	}

	public override void DeactivateInstant()
	{
		isDeactivating = true;
		base.DeactivateInstant();
		currentPhase = RestockerPhase.IDLE;
		if (virtualCallDelay != null)
		{
			virtualCallDelay.Kill();
			virtualCallDelay = null;
		}
		base.transform.DOKill();
		base.transform.SetParent(originalParent, worldPositionStays: true);
		if (restockingCoroutine != null)
		{
			StopCoroutine(restockingCoroutine);
			restockingCoroutine = null;
		}
		if (tryAssignNewJobsCoroutine != null)
		{
			StopCoroutine(tryAssignNewJobsCoroutine);
			tryAssignNewJobsCoroutine = null;
		}
		for (int num = assignedJobs.Count - 1; num >= 0; num--)
		{
			if (assignedJobs[num] != null)
			{
				ReturnJob(assignedJobs[num]);
			}
		}
		for (int i = 0; i < boxesToTake.Count; i++)
		{
			if (!(boxesToTake[i] == null))
			{
				boxesToTake[i].ReleaseBoxReservation();
			}
		}
		if (currentBoxInHand != null)
		{
			currentBoxInHand.transform.DOKill();
			currentBoxInHand.ReleaseBoxReservation();
			if (currentBoxInHand.IsOpen())
			{
				currentBoxInHand.CloseInstant();
			}
			if (currentBoxInHand.IsDisposable())
			{
				DisposeBox(currentBoxInHand);
			}
			else
			{
				currentBoxInHand.ReleaseBoxReservation();
				if (boxReturnPallets.ContainsKey(currentBoxInHand) && boxReturnPallets[currentBoxInHand].ContainerShelf != null)
				{
					boxReturnPallets[currentBoxInHand].ContainerShelf.PlaceReturnBox(currentBoxInHand, instant: true);
					boxReturnPallets.Remove(currentBoxInHand);
				}
				else if (boxReturnBoxStorageUnits.ContainsKey(currentBoxInHand))
				{
					boxReturnBoxStorageUnits[currentBoxInHand].PlaceReturnBox(currentBoxInHand, instant: true);
					boxReturnBoxStorageUnits.Remove(currentBoxInHand);
				}
				else
				{
					SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.PlaceBox(currentBoxInHand, instant: true);
				}
			}
			currentBoxInHand = null;
		}
		foreach (Box item in boxesOnTrolley)
		{
			if (item == null || !SingletonBehaviour<BoxManager>.Instance.HasBox(item.BoxID))
			{
				continue;
			}
			item.transform.DOKill();
			if (!item.IsDisposable())
			{
				item.ReleaseBoxReservation();
				if (boxReturnPallets.ContainsKey(item) && boxReturnPallets[item].ContainerShelf != null)
				{
					boxReturnPallets[item].ContainerShelf.PlaceReturnBox(item, instant: true);
					boxReturnPallets.Remove(item);
				}
				else if (boxReturnBoxStorageUnits.ContainsKey(item))
				{
					boxReturnBoxStorageUnits[item].PlaceReturnBox(item, instant: true);
					boxReturnBoxStorageUnits.Remove(item);
				}
				else
				{
					SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.PlaceBox(item, instant: true);
				}
			}
			else
			{
				DisposeBox(item);
			}
		}
		if (trolley.parent != base.transform)
		{
			trolley.SetParent(base.transform);
		}
		trolley.DOKill();
		speechText.enabled = false;
		trolley.localPosition = initialTrolleyPosition;
		trolley.localEulerAngles = Vector3.zero;
		ResetParameters();
		isActive = false;
		base.gameObject.SetActive(value: false);
	}

	private void PutBoxBackOnTrolley(Action onComplete)
	{
		Box box = currentBoxInHand;
		if (box.IsOpen())
		{
			box.Close(playSound: false);
		}
		currentBoxInHand = null;
		AddBoxToTrolley(box);
		virtualCallDelay = DOVirtual.DelayedCall(0.6f, delegate
		{
			onComplete?.Invoke();
		});
	}

	private void AddBoxToTrolley(Box box)
	{
		if (box.IsOpen())
		{
			box.Close(playSound: false);
		}
		box.transform.parent = boxStartPosition.parent;
		Vector3 to = boxStartPosition.localPosition + currentBoxHeight * Vector3.up;
		to += box.GetLength() / trolley.lossyScale.x / 2f * Vector3.forward;
		box.transform.DOKill();
		box.transform.DoCurvedLocalMove(to, 0.6f, 2f);
		box.transform.DOLocalRotate(boxStartPosition.localEulerAngles, 0.6f);
		currentBoxHeight += box.GetHeight() / trolley.lossyScale.y;
		boxesOnTrolley.Add(box);
	}

	private void RemoveBoxFromTrolley(Box box)
	{
		int num = boxesOnTrolley.IndexOf(box);
		float num2 = box.GetHeight() / trolley.lossyScale.y;
		currentBoxHeight -= num2;
		for (int i = num + 1; i < boxesOnTrolley.Count; i++)
		{
			boxesOnTrolley[i].transform.DOKill();
			boxesOnTrolley[i].transform.DOLocalMove(boxesOnTrolley[i].transform.localPosition - Vector3.up * num2, 0.2f);
		}
		boxesOnTrolley.Remove(box);
	}

	private void TakeTrolley(Action onComplete)
	{
		characterMovement.Animator.SetTrigger("walk");
		SetHandAnimation();
		TryMove(trolleyPickupPoint, delegate
		{
			RotateTowards(trolleyPickupPoint.eulerAngles.y, delegate
			{
				trolley.DOKill();
				trolley.SetParent(base.transform);
				trolley.DOLocalMove(initialTrolleyPosition, 0.25f);
				onComplete?.Invoke();
				SetHandAnimation();
			});
		});
	}

	private void SetHandAnimation()
	{
		if (currentBoxInHand != null)
		{
			characterMovement.Animator.SetTrigger("holdBox");
		}
		else if (trolley.parent == base.transform)
		{
			characterMovement.Animator.SetTrigger("holdTrolley");
		}
		else
		{
			characterMovement.Animator.SetTrigger("handWalk");
		}
	}

	private bool IsCloseEnoughTo(Transform target)
	{
		return Vector3.Distance(base.transform.position, target.position) <= characterMovement.navmeshAgent.stoppingDistance + 1f;
	}

	private void MoveToOrExecute(Transform target, Action onArrived, float stoppingDistance = -1f)
	{
		if (stoppingDistance < 0f)
		{
			stoppingDistance = 1f;
		}
		if (IsCloseEnoughTo(target))
		{
			onArrived?.Invoke();
			return;
		}
		if (trolley.parent == base.transform)
		{
			SetHandAnimation();
		}
		characterMovement.Animator.SetTrigger("walk");
		TryMove(target, delegate
		{
			onArrived?.Invoke();
			SetHandAnimation();
		}, stoppingDistance);
	}

	private void RotateToFace(Transform target, Action onComplete = null, bool faceForward = false)
	{
		Vector3 vector = (faceForward ? target.forward : (-target.forward));
		if (vector != Vector3.zero)
		{
			float y = Quaternion.LookRotation(vector).eulerAngles.y;
			RotateTowards(y, onComplete);
		}
		else
		{
			onComplete?.Invoke();
		}
	}

	private void RotateTowards(float targetY, Action onComplete = null)
	{
		base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, targetY, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
		{
			onComplete?.Invoke();
		});
	}

	public override void AnimateIdle()
	{
		characterMovement.Animator.SetTrigger("idle");
		characterMovement.Animator.SetTrigger("handIdle");
		if (trolley.parent == base.transform)
		{
			trolley.DOKill();
			trolley.DOLocalMove(escalatorTrolleyPosition.localPosition, 0.25f);
		}
	}

	public override void AnimateWalk()
	{
		characterMovement.Animator.SetTrigger("walk");
		SetHandAnimation();
		if (trolley.parent == base.transform)
		{
			trolley.DOKill();
			trolley.DOLocalMove(initialTrolleyPosition, 0.25f);
		}
	}

	public override void TryDeactivate()
	{
		if (!quitWhenReady)
		{
			if (currentPhase == RestockerPhase.IDLE)
			{
				DeactivateInstant();
			}
			else
			{
				quitWhenReady = true;
			}
		}
	}

	private void LateUpdate()
	{
		if (speechText.enabled)
		{
			Transform transform = SingletonBehaviour<PlayerLook>.Instance.MainCamera.transform;
			Vector3 vector = speechText.transform.position - transform.position;
			speechText.transform.LookAt(speechText.transform.position + vector);
		}
	}
}
