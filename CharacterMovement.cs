using System;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class CharacterMovement : MonoBehaviour
{
	[SerializeField]
	private bool useNavmesh;

	private int walkableMask;

	private NavMeshTriangulation Triangulation;

	private NavMeshAgent Agent;

	private Animator animator;

	[SerializeField]
	public LookAt LookAt;

	[SerializeField]
	[Range(0f, 3f)]
	private float WaitDelay = 1f;

	private Vector2 Velocity;

	private Vector2 SmoothDeltaPosition;

	private Vector3 targetPosition;

	private int areaMask = -1;

	private Coroutine stopAndDoCoroutine;

	private bool waitingForArrival;

	private float requiredDistanceToTarget;

	public const float FIRST_FLOOR_Y = 0f;

	private bool isMoving;

	private NavMeshPath path;

	private Action onTargetReached;

	private float initialAgentSpeed;

	public Vector3 TargetPosition => targetPosition;

	public Animator Animator
	{
		get
		{
			if (animator == null)
			{
				animator = GetComponent<Animator>();
			}
			return animator;
		}
	}

	public NavMeshAgent navmeshAgent => Agent;

	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		LookAt = GetComponent<LookAt>();
		walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
		initialAgentSpeed = Agent.speed;
		path = new NavMeshPath();
		if (useNavmesh)
		{
			animator.applyRootMotion = false;
			Agent.updatePosition = true;
			Agent.updateRotation = true;
		}
		else
		{
			animator.applyRootMotion = true;
			Agent.updatePosition = false;
			Agent.updateRotation = true;
		}
	}

	public void Initialize()
	{
		if (useNavmesh)
		{
			animator.applyRootMotion = false;
			Agent.updatePosition = true;
			Agent.updateRotation = true;
		}
		else
		{
			animator.applyRootMotion = true;
			Agent.updatePosition = false;
			Agent.updateRotation = true;
		}
		targetPosition = default(Vector3);
	}

	private void OnAnimatorMove()
	{
		if (!useNavmesh)
		{
			Vector3 rootPosition = animator.rootPosition;
			rootPosition.y = Agent.nextPosition.y;
			base.transform.position = rootPosition;
			Agent.nextPosition = rootPosition;
		}
	}

	private void Update()
	{
		if (!Agent.pathPending && waitingForArrival && TargetReached())
		{
			isMoving = false;
			requiredDistanceToTarget = 0f;
			waitingForArrival = false;
			onTargetReached?.Invoke();
		}
	}

	public void SetSpeedMultiplier(float speedMultiplier, float workSpeedMultiplier)
	{
		Agent.speed = initialAgentSpeed * speedMultiplier;
		animator.SetFloat("speedMultiplier", speedMultiplier);
		animator.SetFloat("workSpeedMultiplier", workSpeedMultiplier);
	}

	public void StopMoving()
	{
		onTargetReached = null;
		if (stopAndDoCoroutine != null)
		{
			StopCoroutine(stopAndDoCoroutine);
			stopAndDoCoroutine = null;
		}
		isMoving = false;
		waitingForArrival = false;
		requiredDistanceToTarget = 0f;
		if (!(Agent == null) && Agent.enabled && Agent.isOnNavMesh)
		{
			Agent.isStopped = true;
		}
	}

	public bool IsMoving()
	{
		return isMoving;
	}

	private bool TargetReached()
	{
		return Agent.remainingDistance <= requiredDistanceToTarget + Agent.stoppingDistance;
	}

	public bool IsCloseToTarget(float range)
	{
		return Agent.remainingDistance <= range;
	}

	public bool IsCloseToTarget(Transform target)
	{
		return Vector3.Distance(base.transform.position, target.position) <= Agent.stoppingDistance;
	}

	public void MoveTo(Transform target, Action onTargetReached, bool projectYToFloor = false)
	{
		MoveTo(target, onTargetReached, 0f, projectYToFloor);
	}

	public void MoveTo(Transform target, Action onTargetReached, float distance, bool projectYToFloor = false)
	{
		MoveTo(target.position, onTargetReached, distance, projectYToFloor);
	}

	public void MoveTo(Vector3 targetPos, Action onTargetReached, float distance, bool projectYToFloor = false)
	{
		waitingForArrival = false;
		requiredDistanceToTarget = distance;
		isMoving = true;
		targetPosition = targetPos;
		if (projectYToFloor)
		{
			targetPosition.y = 0f;
		}
		this.onTargetReached = onTargetReached;
		Agent.enabled = true;
		Agent.isStopped = false;
		Agent.CalculatePath(targetPosition, path);
		if (path.status == NavMeshPathStatus.PathComplete)
		{
			SetPath(path);
			waitingForArrival = true;
			return;
		}
		if (NavMesh.SamplePosition(targetPosition, out var hit, 2f, walkableMask))
		{
			targetPosition = hit.position;
			Agent.CalculatePath(targetPosition, path);
			if (path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
			{
				SetPath(path);
				waitingForArrival = true;
				return;
			}
		}
		onTargetReached?.Invoke();
	}

	private void SetPath(NavMeshPath path)
	{
		Agent.SetPath(path);
	}

	public bool CanMove(Transform target)
	{
		return true;
	}

	public void RefreshPath()
	{
		Agent.SetDestination(targetPosition);
	}

	public void EnableNavmesh(bool enable)
	{
		Agent.enabled = enable;
	}

	public bool NavmeshEnabled()
	{
		return Agent.enabled;
	}
}
