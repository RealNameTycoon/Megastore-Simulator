using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Unloader : Employee
{
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
	private List<Box> boxesToReturn = new List<Box>();

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

	[SerializeField]
	private bool quitWhenReady;

	private Coroutine tryAssignNewJobsCoroutine;

	private WaitForSeconds placeIntervalWait = new WaitForSeconds(0.3f);

	[SerializeField]
	private bool maxCapacityReached;

	private Vector3 initialTrolleyPosition;

	[SerializeField]
	private bool isDeactivating;

	public Restocker.IdleReason idleReason;

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
		idleReason = Restocker.IdleReason.NONE;
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

	private void SetPhase(RestockerPhase newPhase)
	{
		if (!isDeactivating && currentPhase != newPhase)
		{
			currentPhase = newPhase;
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
			RestockJob restockJob = SingletonBehaviour<RestockerManager>.Instance.RequestUnloaderJob(this, preferLabeled);
			if (restockJob == null)
			{
				break;
			}
			idleReason = Restocker.IdleReason.NONE;
			speechText.enabled = false;
			int num2 = restockJob.NeededAmount;
			List<Box> list = new List<Box>();
			while (num2 > 0 && num < 1.2f)
			{
				Box boxWithProductTypeOnTruck = SingletonBehaviour<RestockZoneManager>.Instance.GetBoxWithProductTypeOnTruck(restockJob.ProductType);
				if (boxWithProductTypeOnTruck == null)
				{
					break;
				}
				list.Add(boxWithProductTypeOnTruck);
				boxesToTake.Add(boxWithProductTypeOnTruck);
				num += boxWithProductTypeOnTruck.GetHeight() / trolley.lossyScale.y;
				num2--;
				restockJob.AddBox(boxWithProductTypeOnTruck, GetEmployeeID());
			}
			if (list.Count == 0)
			{
				SingletonBehaviour<RestockerManager>.Instance.UnassignJob(restockJob);
				break;
			}
			assignedJobs.Add(restockJob);
			if (num >= 1.2f)
			{
				maxCapacityReached = true;
				break;
			}
		}
		if (boxesToTake.Count != 0)
		{
			SetPhase(RestockerPhase.TAKING_BOXES);
			StartTakingBoxes();
		}
	}

	private void ResetParameters()
	{
		assignedJobs.Clear();
		boxesToTake.Clear();
		boxesToReturn.Clear();
		boxesOnTrolley.Clear();
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
		Transform transform = boxesToTake[0].transform;
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
				pallet.RemoveBox(box);
				box.SaveLocation();
			}
			else
			{
				SingletonBehaviour<BoxManager>.Instance.WakeUpBoxesAbove(box);
			}
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
				StartStacking();
			}
		});
		box.transform.DOLocalRotate(boxStartPosition.localEulerAngles, 0.6f);
		currentBoxHeight += box.GetHeight() / trolley.lossyScale.y;
	}

	private void StartStacking()
	{
		if (!isDeactivating)
		{
			SetPhase(RestockerPhase.RESTOCKING_SHELVES);
			GetNextBoxFromTrolley();
		}
	}

	private void GetNextBoxFromTrolley()
	{
		if (!isDeactivating)
		{
			if (assignedJobs.Count == 0)
			{
				GoToIdlePosition();
				return;
			}
			assignedJobs.Last().GetReservedBoxes();
			ProcessCurrentJob();
		}
	}

	private void ProcessCurrentJob()
	{
		if (isDeactivating)
		{
			return;
		}
		if (assignedJobs.Count == 0)
		{
			AssignNewJobs();
			if (assignedJobs.Count == 0)
			{
				GoToIdlePosition();
			}
		}
		else
		{
			MoveToShelfAndRestock();
		}
	}

	private void MoveToShelfAndRestock()
	{
		RestockJob currentJob = assignedJobs.Last();
		Transform pickupPoint = currentJob.Shelf.PickupPoint();
		if (!SingletonBehaviour<StorageManager>.Instance.IsPointOnNavMesh(pickupPoint.position))
		{
			pickupPoint = currentJob.Shelf.ParentTransform();
		}
		Action RestockBoxes = delegate
		{
			RotateToFace(pickupPoint, delegate
			{
				characterMovement.Animator.SetTrigger("idle");
				if (restockingCoroutine != null)
				{
					StopCoroutine(restockingCoroutine);
				}
				restockingCoroutine = StartCoroutine(PlaceBoxesToPallets(currentJob));
			}, faceForward: true);
		};
		if (IsCloseEnoughTo(pickupPoint))
		{
			RestockBoxes();
			return;
		}
		characterMovement.Animator.SetTrigger("walk");
		TryMove(pickupPoint, delegate
		{
			RestockBoxes();
		}, 0.5f);
	}

	private IEnumerator PlaceBoxesToPallets(RestockJob job)
	{
		Restockable shelf = job.Shelf;
		ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(job.ProductType);
		int capacity = shelf.GetCapacityForProduct(anyProductData);
		List<Box> boxes = job.GetReservedBoxes();
		yield return placeIntervalWait;
		for (int i = boxes.Count - 1; i >= 0; i--)
		{
			if (shelf.GetProductCount() < capacity)
			{
				shelf.PlaceProduct(boxes[i]);
				boxesOnTrolley.Remove(boxes[i]);
				boxes[i].ReleaseBoxReservation();
				yield return placeIntervalWait;
			}
		}
		CompleteJob(job);
		yield return placeIntervalWait;
		if (assignedJobs.Count == 0 && quitWhenReady)
		{
			GoToIdleAndDeactivate();
		}
		else
		{
			ProcessCurrentJob();
		}
	}

	private void CompleteJob(RestockJob job)
	{
		int num = assignedJobs.IndexOf(job);
		if (num >= 0)
		{
			assignedJobs.RemoveAt(num);
		}
		job.ClearBoxesReservation();
		SingletonBehaviour<RestockerManager>.Instance.UnassignJob(job);
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
			RestockJob restockJob = assignedJobs[num];
			Restockable shelf = restockJob.Shelf;
			PalletShelf palletShelf = shelf as PalletShelf;
			int num2 = ((palletShelf != null) ? palletShelf.ContainedPalletID : (-1));
			ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(restockJob.ProductType);
			int capacityForProduct = shelf.GetCapacityForProduct(anyProductData);
			List<Box> reservedBoxes = restockJob.GetReservedBoxes();
			for (int num3 = reservedBoxes.Count - 1; num3 >= 0; num3--)
			{
				if (reservedBoxes[num3].ContainedPalletID == -1 || reservedBoxes[num3].ContainedPalletID != num2)
				{
					reservedBoxes[num3].transform.DOKill();
					if (reservedBoxes[num3].ContainedPalletID != -1)
					{
						Pallet pallet = SingletonBehaviour<PalletManager>.Instance.GetPallet(reservedBoxes[num3].ContainedPalletID);
						if (pallet != null)
						{
							pallet.RemoveBox(reservedBoxes[num3]);
							reservedBoxes[num3].SaveLocation();
							reservedBoxes[num3].OnBoxTakenByNPC();
						}
					}
					if (shelf.GetProductCount() < capacityForProduct)
					{
						shelf.PlaceProduct(reservedBoxes[num3], instant: true);
						if (boxesOnTrolley.Contains(reservedBoxes[num3]))
						{
							boxesOnTrolley.Remove(reservedBoxes[num3]);
						}
						reservedBoxes[num3].ReleaseBoxReservation();
					}
				}
			}
		}
		for (int num4 = assignedJobs.Count - 1; num4 >= 0; num4--)
		{
			RestockJob job = assignedJobs[num4];
			CompleteJob(job);
		}
		for (int i = 0; i < boxesToTake.Count; i++)
		{
			boxesToTake[i].ReleaseBoxReservation();
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
