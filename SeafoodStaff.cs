using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SeafoodStaff : Employee
{
	[SerializeField]
	private ChoppingStandPlaceable currentFishStand;

	[SerializeField]
	private float cuttingDelay = 2.25f;

	[SerializeField]
	private float choppingAnimationDuration = 0.5f;

	[SerializeField]
	private Transform knifeParent;

	private bool quitWhenReady;

	private WaitForSeconds waiter = new WaitForSeconds(4f);

	private HiringManager.EmployeeStats employeeStats;

	private Coroutine workCoroutine;

	public bool isWorking;

	private void Awake()
	{
		EventManager.AddListener(PlaceableEvents.PLACEMENT_ENDED, delegate(Placeable val)
		{
			OnPlacementEnded(val);
		});
	}

	private void OnPlacementEnded(Placeable placeable)
	{
		if (placeable == currentFishStand)
		{
			MoveToFishStand();
		}
	}

	public void Activate(HiringManager.EmployeeStats stats)
	{
		quitWhenReady = false;
		isWorking = false;
		employeeStats = stats;
		currentFishStand = SingletonBehaviour<SeafoodStaffManager>.Instance.GetServeNeedingFishStand(this);
		if (currentFishStand == null)
		{
			currentFishStand = SingletonBehaviour<SeafoodStaffManager>.Instance.GetAvailableFishStand(this);
		}
		isActive = true;
		float num = (float)stats.workSpeed / 100f;
		waiter = new WaitForSeconds(Mathf.Max(0.5f, 0.5f / num));
		Transform transform = ((currentFishStand == null) ? SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition() : currentFishStand.StaffInteractionPoint);
		base.transform.position = transform.position;
		base.transform.rotation = transform.rotation;
		if (base.gameObject.activeSelf)
		{
			characterMovement.StopMoving();
		}
		else
		{
			base.gameObject.SetActive(value: true);
			characterMovement.EnableNavmesh(enable: true);
		}
		characterMovement.SetSpeedMultiplier(num, (float)stats.workSpeed / 100f);
		currentFishStand?.OccupyByStaff(this);
		workCoroutine = StartCoroutine(WorkRoutine());
	}

	private IEnumerator WorkRoutine()
	{
		do
		{
			yield return waiter;
			yield return CheckAndDoWork();
		}
		while (!quitWhenReady || isWorking);
		Deactivate();
	}

	private IEnumerator CheckAndDoWork()
	{
		if (IsMoving())
		{
			yield break;
		}
		if (currentFishStand == null)
		{
			currentFishStand = SingletonBehaviour<SeafoodStaffManager>.Instance.GetServeNeedingFishStand(this);
			if (currentFishStand == null)
			{
				currentFishStand = SingletonBehaviour<SeafoodStaffManager>.Instance.GetAvailableFishStand(this);
			}
			if (currentFishStand != null)
			{
				currentFishStand.OccupyByStaff(this);
				MoveToFishStand();
			}
		}
		else if (currentFishStand != null && currentFishStand.HasCustomer())
		{
			isWorking = true;
			AnimateChopping();
			PickUpKnife();
			yield return new WaitForSeconds(cuttingDelay);
			while (!currentFishStand.ProductReadyToGive())
			{
				currentFishStand.ChopProductInstantly();
			}
			yield return new WaitForSeconds(0.5f);
			if (currentFishStand.ProductReadyToGive())
			{
				currentFishStand.JobStep();
				yield return new WaitForSeconds(1f);
				AnimateIdle();
				yield return new WaitForSeconds(2f);
			}
			isWorking = false;
		}
		else if (currentFishStand != null && !currentFishStand.HasCustomer())
		{
			ChoppingStandPlaceable serveNeedingFishStand = SingletonBehaviour<SeafoodStaffManager>.Instance.GetServeNeedingFishStand(this);
			if (serveNeedingFishStand != null)
			{
				currentFishStand?.OccupyByStaff(null);
				DropKnife();
				currentFishStand = serveNeedingFishStand;
				currentFishStand.OccupyByStaff(this);
				MoveToFishStand();
			}
		}
	}

	private void MoveToFishStand()
	{
		AnimateWalk();
		TryMove(currentFishStand.StaffInteractionPoint, delegate
		{
			AnimateIdle();
			RotateTowards(currentFishStand.StaffInteractionPoint);
		});
	}

	public override void AnimateIdle()
	{
		characterMovement.Animator.SetTrigger("idle");
		characterMovement.Animator.SetTrigger("handIdle");
	}

	public override void AnimateWalk()
	{
		if (!IsOnEscalator())
		{
			characterMovement.Animator.SetTrigger("walk");
			characterMovement.Animator.SetTrigger("handWalk");
		}
	}

	public void AnimateChopping()
	{
		characterMovement.Animator.SetTrigger("handWork");
	}

	private void RotateTowards(Transform target, Action onComplete = null)
	{
		base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, target.eulerAngles.y, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
		{
			onComplete?.Invoke();
		});
	}

	public override void TryDeactivate()
	{
		if (!quitWhenReady)
		{
			if (!isWorking)
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
		StopAllCoroutines();
		isActive = false;
		isWorking = false;
		currentFishStand?.OccupyByStaff(null);
		DropKnife();
		currentFishStand = null;
		AnimateWalk();
		TryMove(SingletonBehaviour<EmployeeManager>.Instance.DeactivationPoint(), delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	public override void DeactivateInstant()
	{
		base.DeactivateInstant();
		StopAllCoroutines();
		isActive = false;
		isWorking = false;
		currentFishStand?.OccupyByStaff(null);
		DropKnife();
		currentFishStand = null;
		base.transform.position = SingletonBehaviour<EmployeeManager>.Instance.DeactivationPoint().position;
		base.transform.rotation = SingletonBehaviour<EmployeeManager>.Instance.DeactivationPoint().rotation;
		base.gameObject.SetActive(value: false);
	}

	private void DropKnife()
	{
		knifeParent.gameObject.SetActive(value: false);
		currentFishStand?.EnableKnife(enable: true);
	}

	private void PickUpKnife()
	{
		knifeParent.gameObject.SetActive(value: true);
		currentFishStand?.EnableKnife(enable: false);
	}

	public override void OnOccupiedPlaceableRemoved()
	{
		currentFishStand = null;
		currentFishStand = SingletonBehaviour<SeafoodStaffManager>.Instance.GetServeNeedingFishStand(this);
		if (currentFishStand == null)
		{
			currentFishStand = SingletonBehaviour<SeafoodStaffManager>.Instance.GetAvailableFishStand(this);
		}
		if (currentFishStand != null)
		{
			currentFishStand.OccupyByStaff(this);
			MoveToFishStand();
			return;
		}
		Transform awaitingPosition = SingletonBehaviour<StorageManager>.Instance.GetNextRestockerIdlePosition();
		AnimateWalk();
		TryMove(awaitingPosition, delegate
		{
			AnimateIdle();
			RotateTowards(awaitingPosition);
		});
	}
}
