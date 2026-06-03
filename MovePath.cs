using System;
using UnityEngine;

[Serializable]
public class MovePath : MonoBehaviour
{
	[SerializeField]
	public Vector3 startPos;

	[SerializeField]
	public Vector3 finishPos;

	[SerializeField]
	public int w;

	[SerializeField]
	public int targetPoint;

	[SerializeField]
	public int targetPointsTotal;

	[SerializeField]
	public string animName;

	[SerializeField]
	public float walkSpeed;

	[SerializeField]
	public float runSpeed;

	[SerializeField]
	public bool loop;

	[SerializeField]
	public bool forward;

	[SerializeField]
	public GameObject walkPath;

	[HideInInspector]
	public float randXFinish;

	[HideInInspector]
	public float randZFinish;

	[SerializeField]
	[Tooltip("Set your animation speed / Установить свою скорость анимации?")]
	private bool _overrideDefaultAnimationMultiplier;

	[SerializeField]
	[Tooltip("Speed animation walking / Скорость анимации ходьбы")]
	private float _customWalkAnimationMultiplier = 1f;

	[SerializeField]
	[Tooltip("Running animation speed / Скорость анимации бега")]
	private float _customRunAnimationMultiplier = 1f;

	public void InitializeAnimation(bool overrideAnimation, float walk, float run)
	{
		_overrideDefaultAnimationMultiplier = overrideAnimation;
		_customWalkAnimationMultiplier = walk;
		_customRunAnimationMultiplier = run;
	}

	public void MyStart(int _w, int _i, string anim, bool _loop, bool _forward, float _walkSpeed, float _runSpeed)
	{
		forward = _forward;
		walkSpeed = _walkSpeed;
		runSpeed = _runSpeed;
		WalkPath component = walkPath.GetComponent<WalkPath>();
		w = _w;
		targetPointsTotal = component.getPointsTotal(0) - 2;
		loop = _loop;
		animName = anim;
		if (loop)
		{
			if (_i < targetPointsTotal && _i > 0)
			{
				if (forward)
				{
					targetPoint = _i + 1;
					finishPos = component.getNextPoint(w, _i + 1);
				}
				else
				{
					targetPoint = _i;
					finishPos = component.getNextPoint(w, _i);
				}
			}
			else if (forward)
			{
				targetPoint = 1;
				finishPos = component.getNextPoint(w, 1);
			}
			else
			{
				targetPoint = targetPointsTotal;
				finishPos = component.getNextPoint(w, targetPointsTotal);
			}
		}
		else if (forward)
		{
			targetPoint = _i + 1;
			finishPos = component.getNextPoint(w, _i + 1);
		}
		else
		{
			targetPoint = _i;
			finishPos = component.getNextPoint(w, _i);
		}
	}

	public void SetLookPosition()
	{
		Vector3 worldPosition = new Vector3(finishPos.x, base.transform.position.y, finishPos.z);
		base.transform.LookAt(worldPosition);
	}

	private void Start()
	{
		Animator component = GetComponent<Animator>();
		component.CrossFade(animName, 0.1f, 0, UnityEngine.Random.Range(0f, 1f));
		if (animName == "walk")
		{
			if (_overrideDefaultAnimationMultiplier)
			{
				component.speed = walkSpeed * _customWalkAnimationMultiplier;
			}
			else
			{
				component.speed = walkSpeed * 1.2f;
			}
		}
		else if (animName == "run")
		{
			if (_overrideDefaultAnimationMultiplier)
			{
				component.speed = runSpeed * _customRunAnimationMultiplier;
			}
			else
			{
				component.speed = runSpeed / 3f;
			}
		}
	}

	private void Update()
	{
		if (Physics.Raycast(base.transform.position + new Vector3(0f, 2f, 0f), -base.transform.up, out var hitInfo))
		{
			finishPos.y = hitInfo.point.y;
			base.transform.position = new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z);
		}
		Vector3 vector = new Vector3(finishPos.x + randXFinish, finishPos.y, finishPos.z + randZFinish);
		Vector3 vector2 = new Vector3(vector.x, base.transform.position.y, vector.z);
		WalkPath component = walkPath.GetComponent<WalkPath>();
		float num = Vector3.Distance(Vector3.ProjectOnPlane(base.transform.position, Vector3.up), Vector3.ProjectOnPlane(vector, Vector3.up));
		if (num < 0.2f && animName == "walk" && (loop || (!loop && targetPoint > 0 && targetPoint < targetPointsTotal)))
		{
			if (forward)
			{
				vector2 = ((targetPoint >= targetPointsTotal) ? component.getNextPoint(w, 0) : component.getNextPoint(w, targetPoint + 1));
				vector2.y = base.transform.position.y;
			}
			else
			{
				vector2 = ((targetPoint <= 0) ? component.getNextPoint(w, targetPointsTotal) : component.getNextPoint(w, targetPoint - 1));
				vector2.y = base.transform.position.y;
			}
		}
		if (num < 0.5f && animName == "run" && (loop || (!loop && targetPoint > 0 && targetPoint < targetPointsTotal)))
		{
			if (forward)
			{
				vector2 = ((targetPoint >= targetPointsTotal) ? component.getNextPoint(w, 0) : component.getNextPoint(w, targetPoint + 1));
				vector2.y = base.transform.position.y;
			}
			else
			{
				vector2 = ((targetPoint <= 0) ? component.getNextPoint(w, targetPointsTotal) : component.getNextPoint(w, targetPoint - 1));
				vector2.y = base.transform.position.y;
			}
		}
		Vector3 vector3 = vector2 - base.transform.position;
		if (vector3 != Vector3.zero)
		{
			Vector3 zero = Vector3.zero;
			zero = Vector3.RotateTowards(base.transform.forward, vector3, 2f * Time.deltaTime, 0f);
			base.transform.rotation = Quaternion.LookRotation(zero);
		}
		if (num > 1f)
		{
			if (Time.deltaTime > 0f)
			{
				base.transform.position = Vector3.MoveTowards(base.transform.position, finishPos, Time.deltaTime * 1f * ((animName == "walk") ? walkSpeed : runSpeed));
			}
		}
		else if (num <= 1f && forward)
		{
			if (targetPoint != targetPointsTotal)
			{
				targetPoint++;
				finishPos = component.getNextPoint(w, targetPoint);
			}
			else if (targetPoint == targetPointsTotal)
			{
				if (loop)
				{
					finishPos = component.getStartPoint(w);
					targetPoint = 0;
				}
				else
				{
					component.SpawnOnePeople(w, forward, walkSpeed, runSpeed);
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
		else
		{
			if (!(num <= 1f) || forward)
			{
				return;
			}
			if (targetPoint > 0)
			{
				targetPoint--;
				finishPos = component.getNextPoint(w, targetPoint);
			}
			else if (targetPoint == 0)
			{
				if (loop)
				{
					finishPos = component.getNextPoint(w, targetPointsTotal);
					targetPoint = targetPointsTotal;
				}
				else
				{
					component.SpawnOnePeople(w, forward, walkSpeed, runSpeed);
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
	}
}
