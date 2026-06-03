using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Cashier : Employee
{
	[SerializeField]
	private int checkoutDeskID = -1;

	private WaitForSeconds waiter = new WaitForSeconds(0.5f);

	[SerializeField]
	private bool busyAnimating;

	[SerializeField]
	private bool quitWhenReady;

	[SerializeField]
	private bool switchWhenReady;

	[SerializeField]
	private bool isIdle = true;

	private int newCheckoutDeskID = -1;

	private const float CHECKOUT_ANIMATION_DURATION = 1.735f;

	private const float CHECKOUT_MOMENT_DURATION = 0.1f;

	private const float CHECKOUT_ANIMATION_ENDCALL = 1.335f;

	private const float TAKE_PAYMENT_ANIMATION_DURATION = 3.33f;

	private const float TAKE_PAYMENT_MOMENT_DURATION = 2.16f;

	private const float TAKE_PAYMENT_ANIMATION_ENDCALL = 0.8f;

	private WaitForSeconds checkoutMomentWaiter;

	private WaitForSeconds takePaymentMomentWaiter;

	private WaitForSeconds checkoutAnimationEndWaiter;

	private WaitForSeconds takePaymentAnimationEndWaiter;

	private Coroutine checkoutRoutine;

	private Coroutine takePaymentRoutine;

	private HiringManager.EmployeeStats employeeStats;

	public void Activate(HiringManager.EmployeeStats stats, int checkoutDeskID = 0)
	{
		employeeStats = stats;
		this.checkoutDeskID = checkoutDeskID;
		if (SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID) == null)
		{
			return;
		}
		isActive = true;
		if (quitWhenReady)
		{
			quitWhenReady = false;
			return;
		}
		if (base.gameObject.activeSelf)
		{
			characterMovement.StopMoving();
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
		float num = (float)stats.workSpeed / 100f;
		characterMovement.SetSpeedMultiplier(num, (float)stats.workSpeed / 100f);
		waiter = new WaitForSeconds(Mathf.Max(0.5f, 0.5f / num));
		checkoutMomentWaiter = new WaitForSeconds(0.1f / num);
		takePaymentMomentWaiter = new WaitForSeconds(2.16f / num);
		checkoutAnimationEndWaiter = new WaitForSeconds(1.335f / num);
		takePaymentAnimationEndWaiter = new WaitForSeconds(0.8f / num);
		MoveToCheckout();
	}

	public void SwitchCheckoutDesk()
	{
		if (checkoutDeskID != newCheckoutDeskID)
		{
			SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID).SetAutomated(state: false, null);
			checkoutDeskID = newCheckoutDeskID;
			newCheckoutDeskID = -1;
			MoveToCheckout();
		}
	}

	public void MoveToCheckout()
	{
		characterMovement.StopMoving();
		StopAllCoroutines();
		base.transform.DOKill();
		characterMovement.Animator.SetTrigger("walk");
		CheckoutManager checkoutDesk = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID);
		Transform cashierStandTransform = checkoutDesk.CashierStandTransform;
		if (IsCheckoutAvailableToSit(checkoutDesk))
		{
			TryMove(cashierStandTransform, RotateToMonitor);
			checkoutDesk.SetAutomated(state: true, this);
		}
		else
		{
			TryMove(checkoutDesk.CashierWaitTransform, OnCheckoutSideReached);
		}
	}

	public void TrySwitch(int newCheckoutDeskID)
	{
		this.newCheckoutDeskID = newCheckoutDeskID;
		if (!switchWhenReady)
		{
			if (isIdle)
			{
				SwitchCheckoutDesk();
			}
			else
			{
				switchWhenReady = true;
			}
		}
	}

	public override void TryDeactivate()
	{
		if (!quitWhenReady)
		{
			if (isIdle)
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
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID).SetAutomated(state: false, null);
		checkoutDeskID = -1;
		characterMovement.Animator.SetTrigger("walk");
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
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID).SetAutomated(state: false, null);
		checkoutDeskID = -1;
		base.transform.position = SingletonBehaviour<EmployeeManager>.Instance.DeactivationPoint().position;
		base.transform.rotation = SingletonBehaviour<EmployeeManager>.Instance.DeactivationPoint().rotation;
		base.gameObject.SetActive(value: false);
	}

	private void OnCheckoutSideReached()
	{
		CheckoutManager checkoutDesk = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID);
		Transform cashierStandTransform = checkoutDesk.CashierStandTransform;
		if (IsCheckoutAvailableToSit(checkoutDesk))
		{
			TryMove(cashierStandTransform, RotateToMonitor);
			checkoutDesk.SetAutomated(state: true, this);
		}
		else
		{
			base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, checkoutDesk.CashierWaitTransform.eulerAngles.y, base.transform.eulerAngles.z), 0.25f);
			characterMovement.Animator.SetTrigger("idle");
			StartCoroutine(WaitAndRotateToMonitor());
		}
	}

	private bool IsCheckoutAvailableToSit(CheckoutManager checkoutManager)
	{
		if (!checkoutManager.IsCheckout)
		{
			if (checkoutManager.IsAutomated)
			{
				return checkoutManager.AssignedCashierID() == GetEmployeeID();
			}
			return true;
		}
		return false;
	}

	private IEnumerator WaitAndRotateToMonitor()
	{
		CheckoutManager checkoutManager = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID);
		while (!IsCheckoutAvailableToSit(checkoutManager))
		{
			yield return null;
		}
		characterMovement.Animator.SetTrigger("walk");
		Transform cashierStandTransform = checkoutManager.CashierStandTransform;
		TryMove(cashierStandTransform, RotateToMonitor);
	}

	private void RotateToMonitor()
	{
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID).SetAutomated(state: true, this);
		characterMovement.Animator.SetTrigger("idle");
		Transform cashierStandTransform = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID).CashierStandTransform;
		base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, cashierStandTransform.eulerAngles.y, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
		{
			StartCoroutine(CheckoutRoutine());
		});
	}

	private IEnumerator CheckoutRoutine()
	{
		while (true)
		{
			yield return CheckAndDoWork();
			yield return waiter;
		}
	}

	private IEnumerator CheckAndDoWork()
	{
		CheckoutManager checkoutDesk = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID);
		if (checkoutDesk.PlacedItemCount != 0 && !busyAnimating)
		{
			if (checkoutDesk.CheckoutAvailable())
			{
				busyAnimating = true;
				isIdle = false;
				characterMovement.Animator.SetTrigger("checkout");
				yield return CheckoutAnimationRoutine();
			}
			else if (checkoutDesk.PaymentAvailable)
			{
				busyAnimating = true;
				isIdle = false;
				characterMovement.Animator.SetTrigger("takePayment");
				yield return TakePaymentAnimationRoutine();
			}
		}
		else if (checkoutDesk.PlacedItemCount != 0 && busyAnimating)
		{
			yield return null;
		}
	}

	private IEnumerator CheckoutAnimationRoutine()
	{
		Checkout();
		yield return checkoutAnimationEndWaiter;
		AnimationEnded();
	}

	private IEnumerator TakePaymentAnimationRoutine()
	{
		yield return TakePayment();
		yield return TakePaymentAnimationEnded();
	}

	public void Checkout()
	{
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID).CheckoutStep((float)employeeStats.workSpeed / 100f);
	}

	public IEnumerator TakePayment()
	{
		yield return takePaymentMomentWaiter;
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(checkoutDeskID).TakePayment();
	}

	public void AnimationEnded()
	{
		busyAnimating = false;
		characterMovement.Animator.SetTrigger("idle");
	}

	public IEnumerator TakePaymentAnimationEnded()
	{
		yield return takePaymentAnimationEndWaiter;
		busyAnimating = false;
		isIdle = true;
		if (quitWhenReady)
		{
			quitWhenReady = false;
			Deactivate();
		}
		else if (switchWhenReady)
		{
			switchWhenReady = false;
			SwitchCheckoutDesk();
		}
		else
		{
			characterMovement.Animator.SetTrigger("idle");
			yield return waiter;
		}
	}

	public override void AnimateIdle()
	{
		characterMovement.Animator.SetTrigger("idle");
	}

	public override void AnimateWalk()
	{
		characterMovement.Animator.SetTrigger("walk");
	}
}
