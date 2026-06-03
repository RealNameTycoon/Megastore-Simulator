using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class LookAt : MonoBehaviour
{
	public Vector3 lookAtTargetPosition;

	public float lookAtCoolTime = 0.2f;

	public float lookAtHeatTime = 0.2f;

	public bool looking = true;

	private Vector3 lookAtPosition;

	private NavMeshAgent Agent;

	private Animator anim;

	private float lookAtWeight;

	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
	}

	private void Start()
	{
		anim = GetComponent<Animator>();
		lookAtTargetPosition = base.transform.position + base.transform.forward;
		lookAtPosition = lookAtTargetPosition;
	}

	private void Update()
	{
		lookAtTargetPosition = Agent.steeringTarget + base.transform.forward;
	}

	private void OnAnimatorIK()
	{
		lookAtTargetPosition.y = base.transform.position.y;
		float num = (looking ? 1f : 0f);
		Vector3 current = lookAtPosition - base.transform.position;
		Vector3 target = lookAtTargetPosition - base.transform.position;
		current = Vector3.RotateTowards(current, target, 6.28f * Time.deltaTime, float.PositiveInfinity);
		lookAtPosition = base.transform.position + current;
		float num2 = ((num > lookAtWeight) ? lookAtHeatTime : lookAtCoolTime);
		lookAtWeight = Mathf.MoveTowards(lookAtWeight, num, Time.deltaTime / num2);
		anim.SetLookAtWeight(lookAtWeight, 0.2f, 0.5f, 0.7f, 0.5f);
		anim.SetLookAtPosition(lookAtPosition);
	}
}
