using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HomelessPerson : MonoBehaviour
{
	[SerializeField]
	private CharacterMovement characterMovement;

	[SerializeField]
	private Transform tentEnterance;

	[SerializeField]
	private List<Transform> animationSpots;

	[SerializeField]
	private List<string> animationTriggerNames;

	[SerializeField]
	private UIWindow meetPopup;

	private Transform currentTarget;

	private int currentAnimationIndex = -1;

	private List<int> allIndexes = new List<int>();

	public void SetAnimation(int animation)
	{
		StopAllCoroutines();
		new List<int>(allIndexes).Remove(currentAnimationIndex);
		int randomIndex = animation;
		characterMovement.Animator.SetTrigger("walk");
		TryMove(animationSpots[randomIndex], delegate
		{
			characterMovement.Animator.SetTrigger(animationTriggerNames[randomIndex]);
			currentAnimationIndex = randomIndex;
			StartCoroutine(WaitAndAnimateRandom());
		});
	}

	private void Start()
	{
		for (int i = 0; i < animationSpots.Count; i++)
		{
			allIndexes.Add(i);
		}
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
		EventManager.AddListener(GameEvents.SPAWN_TIME_OVER, OnSpawnTimeOver);
		Initialize();
	}

	private void OnNewDayStarted()
	{
		base.gameObject.SetActive(value: true);
		StopAllCoroutines();
		PlayRandomAnimation();
	}

	private void OnSpawnTimeOver()
	{
		StopAllCoroutines();
		characterMovement.Animator.SetTrigger("walk");
		TryMove(tentEnterance, delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private void Initialize()
	{
		if (SingletonBehaviour<TimeManager>.Instance.CanSpawnWanderer())
		{
			List<int> list = new List<int>(allIndexes);
			list.Remove(currentAnimationIndex);
			int randomElement = list.GetRandomElement();
			base.transform.position = animationSpots[randomElement].position;
			base.transform.eulerAngles = animationSpots[randomElement].eulerAngles;
			characterMovement.Animator.SetTrigger(animationTriggerNames[randomElement]);
			currentAnimationIndex = randomElement;
			StartCoroutine(WaitAndAnimateRandom());
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void PlayRandomAnimation()
	{
		List<int> list = new List<int>(allIndexes);
		list.Remove(currentAnimationIndex);
		int randomIndex = list.GetRandomElement();
		characterMovement.Animator.SetTrigger("walk");
		TryMove(animationSpots[randomIndex], delegate
		{
			characterMovement.Animator.SetTrigger(animationTriggerNames[randomIndex]);
			currentAnimationIndex = randomIndex;
			StartCoroutine(WaitAndAnimateRandom());
		});
	}

	private IEnumerator WaitAndAnimateRandom()
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(95, 150));
		PlayRandomAnimation();
	}

	private void TryMove(Transform target, Action targetReachedAction, Action onMovementStarted = null)
	{
		currentTarget = target;
		base.transform.DOKill();
		if (!characterMovement.IsMoving())
		{
			characterMovement.Animator.SetTrigger("walk");
		}
		characterMovement.navmeshAgent.updateRotation = true;
		onMovementStarted?.Invoke();
		onMovementStarted = null;
		targetReachedAction = (Action)Delegate.Combine(targetReachedAction, (Action)delegate
		{
			RotateToTheSlot(target);
		});
		characterMovement.MoveTo(target, targetReachedAction);
	}

	private void RotateToTheSlot(Transform queueSlot)
	{
		characterMovement.navmeshAgent.updateRotation = false;
		base.transform.eulerAngles = queueSlot.eulerAngles;
	}

	public void OpenMeetPopup()
	{
		meetPopup.Open();
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
	}

	public void CloseMeetPopup()
	{
		meetPopup.Close();
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
	}
}
