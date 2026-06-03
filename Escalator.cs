using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Escalator : MonoBehaviour
{
	[SerializeField]
	private List<Transform> steps;

	[SerializeField]
	private Transform startStepRestocker;

	[SerializeField]
	private Transform startStep;

	[SerializeField]
	private Transform endStep;

	[SerializeField]
	private bool upward;

	[SerializeField]
	private List<Rigidbody> stepRBs = new List<Rigidbody>();

	private List<BoxCollider> colliders = new List<BoxCollider>();

	private List<Vector3> initialPositions = new List<Vector3>();

	private Tweener tweener;

	private float stepProgress;

	private int cycleCount = 1;

	private float cycleDuration = 0.5f;

	private List<EscalatorRider> peopleOnEscalator = new List<EscalatorRider>();

	private List<int> personSteps = new List<int>();

	private const int START_STEP_INDEX = 2;

	private int END_STEP_INDEX;

	private bool isReversed;

	private Queue<EscalatorRider> waitingPeople = new Queue<EscalatorRider>();

	public Transform StartStepRestocker => startStepRestocker;

	public Transform StartStep => startStep;

	public Transform EndStep => endStep;

	public void ReverseSteps()
	{
		steps.Reverse();
	}

	public void ReverseStepRBs()
	{
		stepRBs.Reverse();
	}

	public void AssignSteps()
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < stepRBs.Count; i++)
		{
			list.Add(stepRBs[i].GetComponentInChildren<MeshRenderer>(includeInactive: false).transform);
		}
		steps = list;
	}

	private void Awake()
	{
		END_STEP_INDEX = stepRBs.Count - 3;
		for (int i = 0; i < stepRBs.Count; i++)
		{
			initialPositions.Add(stepRBs[i].position);
		}
	}

	private void FixedUpdate()
	{
		stepProgress += Time.fixedDeltaTime / cycleDuration;
		if (stepProgress >= 1f)
		{
			stepProgress -= 1f;
			cycleCount++;
			isReversed = false;
		}
		UpdateSteps();
	}

	private void UpdateSteps()
	{
		for (int i = 0; i < steps.Count; i++)
		{
			int num = (i + cycleCount) % steps.Count;
			if (num == 0)
			{
				stepRBs[i].transform.position = initialPositions[0];
				continue;
			}
			int num2 = num - 1;
			if (num2 == 2)
			{
				if (waitingPeople.Count > 0 && !isReversed && stepProgress < 0.2f)
				{
					isReversed = true;
					MovePerson(steps[i].transform, i);
				}
			}
			else if (num2 == END_STEP_INDEX && personSteps.Contains(i))
			{
				for (int j = 0; j < personSteps.Count; j++)
				{
					if (personSteps[j] == i)
					{
						EscalatorRider escalatorRider = peopleOnEscalator[j];
						escalatorRider.transform.SetParent(null);
						peopleOnEscalator.RemoveAt(j);
						personSteps.RemoveAt(j);
						j--;
						escalatorRider.OnDifferentFloorReached();
					}
				}
			}
			stepRBs[i].MovePosition(Vector3.Lerp(initialPositions[num2], initialPositions[num], stepProgress));
		}
	}

	public void TakeToAnotherFloor(EscalatorRider person)
	{
		waitingPeople.Enqueue(person);
	}

	private void MovePerson(Transform step, int stepIndex)
	{
		EscalatorRider escalatorRider = waitingPeople.Dequeue();
		escalatorRider.transform.SetParent(step);
		peopleOnEscalator.Add(escalatorRider);
		personSteps.Add(stepIndex);
	}
}
