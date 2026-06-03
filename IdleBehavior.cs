using UnityEngine;

public class IdleBehavior : StateMachineBehaviour
{
	[SerializeField]
	private int _numberOfIdleAnimations;

	private bool isStarted;

	private int _idleAnimation;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		ResetIdle();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!isStarted)
		{
			isStarted = true;
			_idleAnimation = Random.Range(1, _numberOfIdleAnimations + 1);
			animator.SetFloat("IdleAnimation", _idleAnimation - 1);
		}
		else if ((double)(stateInfo.normalizedTime % 1f) > 0.98)
		{
			_idleAnimation = Random.Range(1, _numberOfIdleAnimations + 1);
			animator.SetFloat("IdleAnimation", _idleAnimation - 1);
		}
	}

	private void ResetIdle()
	{
		isStarted = false;
	}
}
