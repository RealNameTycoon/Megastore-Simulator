using System;
using DG.Tweening;
using UnityEngine;

public class Employee : EscalatorRider
{
	[SerializeField]
	protected CharacterMovement characterMovement;

	[SerializeField]
	private EmployeeData employeeData;

	private Transform currentTarget;

	private Action onFloorReached;

	private Transform differentFloorTarget;

	public const float MID_FLOOR_Y = 4f;

	protected bool isPaused;

	private float currentDistance;

	protected bool isActive;

	public bool IsPaused()
	{
		return isPaused;
	}

	public int GetEmployeeID()
	{
		return employeeData.employeeID;
	}

	public EmployeeData GetEmployeeData()
	{
		return employeeData;
	}

	public override void OnDifferentFloorReached()
	{
		if (base.gameObject.activeSelf && !(differentFloorTarget == null))
		{
			characterMovement.EnableNavmesh(enable: true);
			TryMove(differentFloorTarget, onFloorReached, currentDistance);
		}
	}

	public virtual void SwitchPauseState()
	{
		isPaused = !isPaused;
	}

	public bool IsOnEscalator()
	{
		return !characterMovement.NavmeshEnabled();
	}

	public void TryMove(Transform target, Action targetReachedAction, float distance = 0f, bool firstFloorOnly = false)
	{
		if (!characterMovement.NavmeshEnabled())
		{
			onFloorReached = targetReachedAction;
			differentFloorTarget = target;
			currentDistance = distance;
			return;
		}
		bool flag = false;
		if (target.position.y > 4f && base.transform.position.y < 4f && !firstFloorOnly)
		{
			onFloorReached = targetReachedAction;
			differentFloorTarget = target;
			currentDistance = distance;
			target = ((employeeData.role != EmployeeRole.RESTOCKER) ? SingletonBehaviour<EscalatorManager>.Instance.UpwardEscalator.StartStep : SingletonBehaviour<EscalatorManager>.Instance.UpwardEscalator.StartStepRestocker);
			targetReachedAction = GetIntoUpwardEscalator;
			flag = true;
		}
		else if (target.position.y < 4f && base.transform.position.y > 4f)
		{
			onFloorReached = targetReachedAction;
			differentFloorTarget = target;
			currentDistance = distance;
			target = ((employeeData.role != EmployeeRole.RESTOCKER) ? SingletonBehaviour<EscalatorManager>.Instance.DownwardEscalator.StartStep : SingletonBehaviour<EscalatorManager>.Instance.DownwardEscalator.StartStepRestocker);
			targetReachedAction = GetIntoDownwardEscalator;
			flag = true;
		}
		currentTarget = target;
		base.transform.DOKill();
		if (!characterMovement.IsMoving())
		{
			AnimateWalk();
		}
		OnMovementStarted();
		if (flag)
		{
			characterMovement.MoveTo(target, targetReachedAction, 0f, firstFloorOnly);
		}
		else
		{
			characterMovement.MoveTo(target, targetReachedAction, distance, firstFloorOnly);
		}
	}

	protected bool IsMoving()
	{
		return characterMovement.IsMoving();
	}

	private void GetIntoDownwardEscalator()
	{
		Vector3 zero = Vector3.zero;
		if (employeeData.role == EmployeeRole.RESTOCKER)
		{
			float y = SingletonBehaviour<EscalatorManager>.Instance.DownwardEscalator.StartStepRestocker.eulerAngles.y;
			zero = new Vector3(base.transform.eulerAngles.x, y, base.transform.eulerAngles.z);
		}
		else
		{
			float y2 = SingletonBehaviour<EscalatorManager>.Instance.DownwardEscalator.StartStep.eulerAngles.y;
			zero = new Vector3(base.transform.eulerAngles.x, y2, base.transform.eulerAngles.z);
		}
		base.transform.DORotate(zero, 0.25f);
		AnimateIdle();
		characterMovement.EnableNavmesh(enable: false);
		SingletonBehaviour<EscalatorManager>.Instance.DownwardEscalator.TakeToAnotherFloor(this);
	}

	private void GetIntoUpwardEscalator()
	{
		Vector3 zero = Vector3.zero;
		if (employeeData.role == EmployeeRole.RESTOCKER)
		{
			float y = SingletonBehaviour<EscalatorManager>.Instance.UpwardEscalator.StartStepRestocker.eulerAngles.y;
			zero = new Vector3(base.transform.eulerAngles.x, y, base.transform.eulerAngles.z);
		}
		else
		{
			float y2 = SingletonBehaviour<EscalatorManager>.Instance.UpwardEscalator.StartStep.eulerAngles.y;
			zero = new Vector3(base.transform.eulerAngles.x, y2, base.transform.eulerAngles.z);
		}
		base.transform.DORotate(zero, 0.25f);
		AnimateIdle();
		characterMovement.EnableNavmesh(enable: false);
		SingletonBehaviour<EscalatorManager>.Instance.UpwardEscalator.TakeToAnotherFloor(this);
	}

	public override bool ShoulOpenTheDoor(AutoDoor door)
	{
		if (door.TemperatureZone != null)
		{
			if (SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(characterMovement.TargetPosition) == door.TemperatureZone)
			{
				return true;
			}
			if (SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(base.transform.position) == door.TemperatureZone)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public virtual void AnimateIdle()
	{
	}

	public virtual void AnimateWalk()
	{
	}

	public virtual void OnMovementStarted()
	{
	}

	public bool IsActive()
	{
		return isActive;
	}

	public virtual void TryDeactivate()
	{
	}

	public virtual void OnJobDisposed(RestockJob job)
	{
	}

	public virtual void DeactivateInstant()
	{
		onFloorReached = null;
		differentFloorTarget = null;
		currentDistance = 0f;
		currentTarget = null;
		if (characterMovement.navmeshAgent.enabled)
		{
			characterMovement.StopMoving();
		}
	}

	public virtual void OnOccupiedPlaceableRemoved()
	{
	}
}
