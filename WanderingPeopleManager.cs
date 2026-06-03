using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderingPeopleManager : SingletonBehaviour<WanderingPeopleManager>
{
	[SerializeField]
	private List<CharacterMovement> wanderingPeople;

	[SerializeField]
	private List<CharacterMovement> wanderingPeopleReview;

	[SerializeField]
	private Transform startPosition;

	[SerializeField]
	private Transform endPosition;

	[SerializeField]
	private Transform startPosition2;

	[SerializeField]
	private Transform endPosition2;

	[SerializeField]
	private Transform startPosition3;

	[SerializeField]
	private Transform endPosition3;

	[SerializeField]
	private Transform poolTransform;

	private int index;

	private List<CharacterMovement> WanderingPeople => wanderingPeople;

	private void Start()
	{
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
		StartCoroutine(WanderRoutine());
	}

	private void OnNewDayStarted()
	{
		SpawnNewWanderer();
	}

	private IEnumerator WanderRoutine()
	{
		yield return new WaitForSeconds(2f);
		while (true)
		{
			SpawnNewWanderer();
			yield return new WaitForSeconds(Random.Range(6, 10));
		}
	}

	private void SpawnNewWanderer()
	{
		if (SingletonBehaviour<TimeManager>.Instance.CanSpawnWanderer() && !WanderingPeople[index % WanderingPeople.Count].gameObject.activeSelf)
		{
			int num = Random.Range(0, 7);
			Transform transform;
			Transform transform2;
			if (num < 2)
			{
				transform = startPosition2;
				transform2 = endPosition2;
			}
			if (num < 4)
			{
				transform = startPosition;
				transform2 = endPosition;
			}
			else
			{
				transform = startPosition3;
				transform2 = endPosition3;
			}
			Transform transform3;
			Transform target;
			if (Random.Range(0, 2) == 0)
			{
				transform3 = transform;
				target = transform2;
			}
			else
			{
				transform3 = transform2;
				target = transform;
			}
			CharacterMovement currentChar = WanderingPeople[index % WanderingPeople.Count];
			currentChar.transform.position = transform3.position;
			currentChar.transform.rotation = transform3.rotation;
			currentChar.gameObject.SetActive(value: true);
			currentChar.Animator.SetTrigger("walk0");
			currentChar.MoveTo(target, delegate
			{
				StopAndBackToPool(currentChar);
			});
			index++;
		}
	}

	private void StopAndBackToPool(CharacterMovement character)
	{
		character.StopMoving();
		character.transform.position = poolTransform.position;
		character.gameObject.SetActive(value: false);
	}
}
