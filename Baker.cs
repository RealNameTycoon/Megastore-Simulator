using System;
using System.Collections;
using System.Collections.Generic;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Baker : Employee
{
	[Header("Settings")]
	[SerializeField]
	private bool preferLabeled = true;

	[Header("References")]
	[SerializeField]
	private Transform hand;

	[Header("Debug")]
	[SerializeField]
	private List<RestockJob> assignedJobs = new List<RestockJob>();

	private Coroutine tryAssignNewJobsCoroutine;

	private WaitForSeconds waiter = new WaitForSeconds(4f);

	private BakerPhase currentPhase;

	[SerializeField]
	private Tray currentTrayInHand;

	[SerializeField]
	private Oven targetOven;

	[SerializeField]
	private List<TrayShelf> trayShelvesWithRawItems = new List<TrayShelf>();

	[SerializeField]
	private List<(Tray tray, TrayShelf originalShelf)> traysPutToOven = new List<(Tray, TrayShelf)>();

	[SerializeField]
	private TextMeshPro speechText;

	private HiringManager.EmployeeStats employeeStats;

	private Coroutine workCoroutine;

	private Placeable idlePlaceable;

	private bool quitWhenReady;

	private const float REQUIRED_DISTANCE = 1f;

	private const int MAX_TRAY_COUNT = 2;

	private WaitForSeconds workInterval;

	private Transform originalParent;

	public Restocker.IdleReason idleReason;

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

	public BakerPhase CurrentPhase => currentPhase;

	private void Awake()
	{
		EventManager.AddListener(PlaceableEvents.PLACEMENT_ENDED, delegate(Placeable val)
		{
			OnPlacementEnded(val);
		});
	}

	public override void SwitchPauseState()
	{
		base.SwitchPauseState();
		if (isPaused && currentPhase != BakerPhase.IDLE)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltipWithFullText(Locale.GetWord("employee_break_n").Replace("{0}", employeeStats.employeeName), base.transform);
		}
		if (!isPaused && speechText.enabled)
		{
			speechText.enabled = false;
		}
	}

	private void OnPlacementEnded(Placeable placeable)
	{
		if (placeable == idlePlaceable && currentPhase == BakerPhase.IDLE)
		{
			GoToIdlePlaceable();
		}
	}

	private void OnJobAvailable()
	{
	}

	public void Activate(HiringManager.EmployeeStats stats)
	{
		if (originalParent == null)
		{
			originalParent = base.transform.parent;
		}
		employeeStats = stats;
		workInterval = new WaitForSeconds(0.3f / ((float)stats.workSpeed / 100f));
		isPaused = stats.isPaused;
		isActive = true;
		idlePlaceable = SingletonBehaviour<BakerManager>.Instance.GetBakerIdlePosition(this);
		Transform transform = idlePlaceable?.StaffInteractionPoint ?? SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition();
		idlePlaceable?.OccupyByStaff(this);
		base.transform.position = transform.position;
		base.transform.rotation = transform.rotation;
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
		characterMovement.SetSpeedMultiplier((float)stats.moveSpeed / 100f, (float)stats.workSpeed / 100f);
		SetPhase(BakerPhase.IDLE);
		tryAssignNewJobsCoroutine = StartCoroutine(WaitAndTryAssignNewJobs());
	}

	private IEnumerator WaitAndTryAssignNewJobs()
	{
		while (true)
		{
			yield return waiter;
			if (isActive && currentPhase == BakerPhase.IDLE)
			{
				AssignNewJobs();
			}
		}
	}

	public override void TryDeactivate()
	{
		if (!quitWhenReady)
		{
			if (currentPhase == BakerPhase.IDLE)
			{
				Deactivate();
			}
			else
			{
				quitWhenReady = true;
			}
		}
	}

	private void Deactivate()
	{
		ResetState();
		isActive = false;
		base.gameObject.SetActive(value: false);
		if (idlePlaceable != null)
		{
			idlePlaceable.OccupyByStaff(null);
			idlePlaceable = null;
		}
		if (tryAssignNewJobsCoroutine != null)
		{
			StopCoroutine(tryAssignNewJobsCoroutine);
			tryAssignNewJobsCoroutine = null;
		}
	}

	public override void DeactivateInstant()
	{
		base.DeactivateInstant();
		SetPhase(BakerPhase.IDLE);
		if (workCoroutine != null)
		{
			StopCoroutine(workCoroutine);
		}
		workCoroutine = null;
		base.transform.DOKill();
		base.transform.SetParent(originalParent, worldPositionStays: true);
		bool flag = false;
		if (targetOven != null && targetOven.Cooking)
		{
			targetOven.OnDeactivateInstant();
		}
		foreach (var item in traysPutToOven)
		{
			item.tray.transform.DOKill();
			if (currentTrayInHand != null && item.tray.TrayID == currentTrayInHand.TrayID)
			{
				flag = true;
				currentTrayInHand = null;
			}
			ReturnTray(item.originalShelf, item.tray);
		}
		if (currentTrayInHand != null && !flag)
		{
			currentTrayInHand.transform.DOKill();
			currentTrayInHand.EnableCollider(enable: true);
			currentTrayInHand.ReleaseAnimatedNPC();
			currentTrayInHand = null;
		}
		foreach (RestockJob assignedJob in assignedJobs)
		{
			SingletonBehaviour<RestockerManager>.Instance.UnassignJob(assignedJob);
		}
		foreach (TrayShelf trayShelvesWithRawItem in trayShelvesWithRawItems)
		{
			trayShelvesWithRawItem.ClearReservedToStaff();
		}
		if (targetOven != null)
		{
			targetOven.Release();
			targetOven = null;
		}
		if (speechText != null)
		{
			speechText.enabled = false;
		}
		Deactivate();
	}

	private void SetPhase(BakerPhase phase)
	{
		if (currentPhase != phase)
		{
			if (currentPhase == BakerPhase.IDLE && idlePlaceable != null)
			{
				idlePlaceable.OccupyByStaff(null);
				idlePlaceable = null;
			}
			currentPhase = phase;
		}
	}

	private void ResetState()
	{
		assignedJobs.Clear();
		trayShelvesWithRawItems.Clear();
		traysPutToOven.Clear();
		currentTrayInHand = null;
		quitWhenReady = false;
		if (targetOven != null)
		{
			targetOven.Release();
			targetOven = null;
		}
	}

	private void GoIdle()
	{
		ResetState();
		SetPhase(BakerPhase.IDLE);
		idlePlaceable = SingletonBehaviour<BakerManager>.Instance.GetBakerIdlePosition(this);
		GoToIdlePlaceable();
	}

	private void GoToIdlePlaceable()
	{
		Transform idlePos = idlePlaceable?.StaffInteractionPoint ?? SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition();
		idlePlaceable?.OccupyByStaff(this);
		MoveIfNotClose(idlePos, delegate
		{
			RotateTowards(idlePos, delegate
			{
				SetIdleStandingAnimation();
				if (quitWhenReady)
				{
					GoToIdleAndDeactivate();
				}
			});
		});
	}

	private void AssignNewJobs()
	{
		if (quitWhenReady)
		{
			return;
		}
		if (IsPaused())
		{
			speechText.text = Locale.GetWord("employee_paused_desc");
			speechText.enabled = true;
			return;
		}
		ResetState();
		RestockJob restockJob = SingletonBehaviour<RestockerManager>.Instance.RequestCookedBakerJob(this, preferLabeled);
		if (restockJob != null)
		{
			TrayShelf cookedTrayShelf = SingletonBehaviour<BakerManager>.Instance.GetCookedTrayShelf(restockJob.ProductType);
			if (cookedTrayShelf != null)
			{
				trayShelvesWithRawItems.Add(cookedTrayShelf);
				assignedJobs.Add(restockJob);
				restockJob.ReserveTrayShelf(cookedTrayShelf, GetEmployeeID());
				SetPhase(BakerPhase.FETCHING_TRAYS);
				FetchNextTray();
				SetIdleReason(Restocker.IdleReason.NONE);
				return;
			}
			ReturnJob(restockJob);
		}
		targetOven = SingletonBehaviour<BakerManager>.Instance.GetAvailableOven(base.transform);
		if (targetOven == null)
		{
			SetIdleReason(Restocker.IdleReason.NO_OVEN);
			return;
		}
		targetOven.Reserve(GetEmployeeID());
		int num = Mathf.Min(2, targetOven.GetEmptyCookingSlotCount());
		while (trayShelvesWithRawItems.Count < num)
		{
			var (restockJob2, idleReason) = SingletonBehaviour<RestockerManager>.Instance.RequestBakerJob(this, preferLabeled);
			if (restockJob2 == null)
			{
				if (assignedJobs.Count == 0)
				{
					SetIdleReason(idleReason);
					targetOven.Release();
				}
				break;
			}
			TrayShelf randomTrayShelfToReserve = SingletonBehaviour<BakerManager>.Instance.GetRandomTrayShelfToReserve(restockJob2.ProductType);
			if (randomTrayShelfToReserve == null)
			{
				if (assignedJobs.Count == 0)
				{
					targetOven.Release();
				}
				SingletonBehaviour<RestockerManager>.Instance.UnassignJob(restockJob2);
				SetIdleReason(Restocker.IdleReason.NO_TRAYS);
				break;
			}
			SetIdleReason(Restocker.IdleReason.NONE);
			assignedJobs.Add(restockJob2);
			trayShelvesWithRawItems.Add(randomTrayShelfToReserve);
			restockJob2.ReserveTrayShelf(randomTrayShelfToReserve, GetEmployeeID());
		}
		if (trayShelvesWithRawItems.Count > 0)
		{
			SetPhase(BakerPhase.FETCHING_TRAYS);
			FetchNextTray();
		}
		else
		{
			ResetState();
		}
	}

	private void SetIdleReason(Restocker.IdleReason idleReason)
	{
		this.idleReason = idleReason;
		switch (idleReason)
		{
		case Restocker.IdleReason.NO_OVEN:
			speechText.text = Locale.GetWord("baker_no_oven_desc");
			break;
		case Restocker.IdleReason.NO_JOBS:
			speechText.text = Locale.GetWord("baker_no_jobs_desc").Replace("{0}", "%75");
			break;
		case Restocker.IdleReason.NO_TRAYS:
			speechText.text = Locale.GetWord("baker_no_trays_desc");
			break;
		default:
			speechText.text = "";
			break;
		}
		speechText.enabled = idleReason != Restocker.IdleReason.NONE;
	}

	private void ReturnJob(RestockJob job)
	{
		assignedJobs.Remove(job);
		SingletonBehaviour<RestockerManager>.Instance.UnassignJob(job);
	}

	private void FetchNextTray()
	{
		if (trayShelvesWithRawItems.Count == 0)
		{
			MoveToOvenAndTurnOn();
			return;
		}
		TrayShelf shelfToTakeTrayFrom = trayShelvesWithRawItems[0];
		MoveIfNotClose(shelfToTakeTrayFrom.ParentFurniture.StaffInteractionPoint, delegate
		{
			RotateTowards(shelfToTakeTrayFrom.ParentFurniture.StaffInteractionPoint, delegate
			{
				PickupTray(shelfToTakeTrayFrom);
			});
		});
	}

	private void PickupTray(TrayShelf shelfToTakeTrayFrom)
	{
		trayShelvesWithRawItems.RemoveAt(0);
		currentTrayInHand = shelfToTakeTrayFrom.ContainedTray;
		SetIdleStandingAnimation();
		TrayShelf originalShelf = shelfToTakeTrayFrom;
		if (shelfToTakeTrayFrom.ContainedTray.IsCooked)
		{
			AnimateTrayToHand(delegate
			{
				RestockCurrentJob();
			});
		}
		else
		{
			AnimateTrayToHand(delegate
			{
				MoveToOvenAndPlace(originalShelf);
			});
		}
	}

	private void MoveToOvenAndPlace(TrayShelf shelfToPlaceInOven)
	{
		TrayShelf cookingSlot = targetOven.GetEmptyCookingSlot();
		if (cookingSlot == null)
		{
			GoIdle();
			return;
		}
		MoveIfNotClose(targetOven.StaffInteractionPoint, delegate
		{
			SetIdleStandingAnimation();
			RotateTowards(targetOven.StaffInteractionPoint, delegate
			{
				PlaceInOven(cookingSlot, shelfToPlaceInOven);
			});
		});
	}

	private void PlaceInOven(TrayShelf cookingSlot, TrayShelf shelfWithRawItem)
	{
		Tray tray = currentTrayInHand;
		currentTrayInHand = null;
		if (cookingSlot.ContainedTrayID != -1 && cookingSlot.ContainedTray.IsEmpty())
		{
			StartCoroutine(TransferItemsToOvenTray(tray, cookingSlot.ContainedTray, shelfWithRawItem));
			return;
		}
		traysPutToOven.Add((tray, shelfWithRawItem));
		cookingSlot.PutTray(tray);
		tray.ReleaseAnimatedNPC();
		if (trayShelvesWithRawItems.Count == 0)
		{
			SetIdleStandingAnimation();
		}
		FetchNextTray();
	}

	private IEnumerator TransferItemsToOvenTray(Tray trayWithRawItems, Tray ovenTray, TrayShelf shelfWithRawItems)
	{
		while (!trayWithRawItems.IsEmpty())
		{
			Product product = trayWithRawItems.RemoveAndGetItem();
			if (product != null)
			{
				ovenTray.PlaceProduct(product);
			}
			yield return workInterval;
		}
		currentTrayInHand = trayWithRawItems;
		ReturnEmptyTrayToShelf(shelfWithRawItems, delegate
		{
			workCoroutine = StartCoroutine(FixJobsAfterTransfer(ovenTray, trayWithRawItems, shelfWithRawItems));
		});
	}

	private IEnumerator FixJobsAfterTransfer(Tray ovenTray, Tray transferingTray, TrayShelf originalShelf)
	{
		originalShelf.ClearReservedToStaff();
		transferingTray.EnableCollider(enable: true);
		RestockJob restockJob = null;
		for (int i = 0; i < assignedJobs.Count; i++)
		{
			if (assignedJobs[i].GetReservedTrayShelf() == originalShelf)
			{
				restockJob = assignedJobs[i];
				break;
			}
		}
		restockJob.ReserveTrayShelf(ovenTray.ContainerShelf, GetEmployeeID());
		yield return workInterval;
		FetchNextTray();
	}

	private void ReturnEmptyTrayToShelf(TrayShelf targetShelf, Action onComplete)
	{
		MoveIfNotClose(targetShelf.ParentFurniture.StaffInteractionPoint, delegate
		{
			SetIdleStandingAnimation();
			RotateTowards(targetShelf.ParentFurniture.StaffInteractionPoint, delegate
			{
				targetShelf.PutTray(currentTrayInHand);
				currentTrayInHand.ReleaseAnimatedNPC();
				currentTrayInHand = null;
				SetIdleStandingAnimation();
				onComplete?.Invoke();
			});
		});
	}

	private void MoveToOvenAndTurnOn()
	{
		SetPhase(BakerPhase.WAITING_FOR_COOK);
		MoveIfNotClose(targetOven.StaffInteractionPoint, delegate
		{
			SetIdleStandingAnimation();
			RotateTowards(targetOven.StaffInteractionPoint, delegate
			{
				if (targetOven.CanCook())
				{
					targetOven.TurnOn(OnCookingFinished, isNPC: true);
				}
				else
				{
					GoIdle();
				}
			});
		});
	}

	private void OnCookingFinished()
	{
		SetPhase(BakerPhase.RESTOCKING);
		TakeCurrentTray(delegate
		{
			RestockCurrentJob();
		});
	}

	private void RestockCurrentJob()
	{
		if (assignedJobs.Count == 0)
		{
			GoIdle();
			return;
		}
		RestockJob job = assignedJobs[0];
		Tray reservedTray = job.ReservedTray;
		if (currentTrayInHand == null || currentTrayInHand.ContainedType != job.ProductType)
		{
			Transform staffInteractionPoint = reservedTray.ContainerShelf.ParentFurniture.StaffInteractionPoint;
			MoveIfNotClose(staffInteractionPoint, delegate
			{
				RotateTowards(staffInteractionPoint, delegate
				{
					currentTrayInHand = reservedTray;
					SetIdleStandingAnimation();
					AnimateTrayToHand(delegate
					{
						MoveToShelfAndRestock(job);
					});
				});
			});
		}
		else
		{
			MoveToShelfAndRestock(job);
		}
	}

	private void TakeCurrentTray(Action onComplete)
	{
		Tray tray = assignedJobs[0].ReservedTray;
		currentTrayInHand = tray;
		MoveIfNotClose(tray.ContainerShelf.ParentFurniture.StaffInteractionPoint, delegate
		{
			RotateTowards(tray.ContainerShelf.ParentFurniture.StaffInteractionPoint, delegate
			{
				AnimateTrayToHand(delegate
				{
					onComplete?.Invoke();
				});
			});
		});
	}

	private void MoveToShelfAndRestock(RestockJob job)
	{
		Transform shelfPoint = job.Shelf.StaffInteractionPoint();
		MoveIfNotClose(shelfPoint, delegate
		{
			SetIdleStandingAnimation();
			RotateTowards(shelfPoint, delegate
			{
				if (workCoroutine != null)
				{
					StopCoroutine(workCoroutine);
				}
				workCoroutine = StartCoroutine(RestockShelf(job));
			});
		});
	}

	private IEnumerator RestockShelf(RestockJob job)
	{
		Restockable shelf = job.Shelf;
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(job.ProductType);
		int capacity = shelf.GetCapacityForProduct(productData);
		yield return workInterval;
		while (!currentTrayInHand.IsEmpty() && shelf.GetProductCount() < capacity)
		{
			shelf.PlaceProduct(currentTrayInHand);
			yield return workInterval;
		}
		ReturnCurrentTray(delegate
		{
			CompleteJob(job);
			if (assignedJobs.Count > 0)
			{
				TakeCurrentTray(delegate
				{
					RestockCurrentJob();
				});
			}
			else if (quitWhenReady)
			{
				GoToIdleAndDeactivate();
			}
			else
			{
				AssignNewJobs();
				if (assignedJobs.Count == 0)
				{
					GoIdle();
				}
			}
		});
	}

	private void GoToIdleAndDeactivate()
	{
		Transform nextRestockerIdlePosition = SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition();
		MoveIfNotClose(nextRestockerIdlePosition, delegate
		{
			DeactivateInstant();
		});
	}

	private void ReturnCurrentTray(Action onComplete)
	{
		Tray currentTray = currentTrayInHand;
		TrayShelf currentTrayShelf = assignedJobs[0].GetReservedTrayShelf();
		MoveIfNotClose(currentTrayShelf.ParentFurniture.StaffInteractionPoint, delegate
		{
			RotateTowards(currentTrayShelf.ParentFurniture.StaffInteractionPoint, delegate
			{
				ReturnTray(currentTrayShelf, currentTray);
				onComplete?.Invoke();
			});
		});
	}

	private void ReturnTray(TrayShelf currentTrayShelf, Tray currentTray)
	{
		currentTrayShelf.PutTray(currentTray);
		currentTray.ReleaseAnimatedNPC();
		currentTray.EnableCollider(enable: true);
	}

	private void CompleteJob(RestockJob job)
	{
		assignedJobs.Remove(job);
		SingletonBehaviour<RestockerManager>.Instance.UnassignJob(job);
	}

	private void AnimateTrayToHand(Action onComplete)
	{
		currentTrayInHand.EnableCollider(enable: false);
		currentTrayInHand.transform.parent = hand;
		currentTrayInHand.transform.DOLocalMove(Vector3.zero, 0.35f);
		currentTrayInHand.transform.DOLocalRotate(Vector3.zero, 0.35f).OnComplete(delegate
		{
			SetHandAnimation();
			onComplete?.Invoke();
		});
	}

	private void SetHandAnimation()
	{
		if (currentTrayInHand != null && currentTrayInHand.transform.parent == hand)
		{
			characterMovement.Animator.SetTrigger("holdBox");
		}
		else
		{
			characterMovement.Animator.SetTrigger("handWalk");
		}
	}

	private void SetIdleStandingAnimation()
	{
		if (currentTrayInHand == null || currentTrayInHand.transform.parent != hand)
		{
			characterMovement.Animator.SetTrigger("handIdle");
		}
		characterMovement.Animator.SetTrigger("idle");
	}

	private void SetWalkingAnimation()
	{
		characterMovement.Animator.SetTrigger("walk");
		SetHandAnimation();
	}

	public override void AnimateIdle()
	{
		if (currentTrayInHand == null || currentTrayInHand.transform.parent != hand)
		{
			characterMovement.Animator.SetTrigger("handIdle");
		}
		characterMovement.Animator.SetTrigger("idle");
	}

	public override void AnimateWalk()
	{
		characterMovement.Animator.SetTrigger("walk");
		SetHandAnimation();
	}

	private void MoveIfNotClose(Transform target, Action onComplete)
	{
		if (Vector3.Distance(base.transform.position, target.position) > characterMovement.navmeshAgent.stoppingDistance)
		{
			SetWalkingAnimation();
			TryMove(target, onComplete);
		}
		else
		{
			onComplete?.Invoke();
		}
	}

	private void RotateTowards(Transform target, Action onComplete = null)
	{
		base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, target.eulerAngles.y, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
		{
			onComplete?.Invoke();
		});
	}

	public override void OnJobDisposed(RestockJob job)
	{
		assignedJobs.Remove(job);
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
