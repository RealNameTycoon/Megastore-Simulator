using DG.Tweening;
using UnityEngine;

public class HotkeyClickable : Clickable
{
	[SerializeField]
	protected Transform pickUpPosition;

	[SerializeField]
	protected Transform putDownPosition;

	protected float PICKUP_SPEED = 2f;

	protected float PICKUP_SPEED_ROTATION = 360f;

	[SerializeField]
	protected bool isPicked;

	public bool IsPicked => isPicked;

	protected virtual float GetPickUpSpeed()
	{
		return PICKUP_SPEED;
	}

	protected virtual float GetPickUpSpeedRotation()
	{
		return PICKUP_SPEED_ROTATION;
	}

	public virtual void PickUp()
	{
		base.gameObject.SetActive(value: true);
		isPicked = true;
		base.transform.DOKill();
		base.transform.DOLocalMove(pickUpPosition.localPosition, GetPickUpSpeed()).SetSpeedBased(isSpeedBased: true);
		base.transform.DOLocalRotate(pickUpPosition.localEulerAngles, GetPickUpSpeedRotation()).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
		{
			OnPickedUp();
		});
	}

	public virtual void OnPickedUp()
	{
	}

	public virtual void PutDown()
	{
		isPicked = false;
		base.transform.DOKill();
		base.transform.DOLocalMove(putDownPosition.localPosition, GetPickUpSpeed()).SetSpeedBased(isSpeedBased: true);
		base.transform.DOLocalRotate(putDownPosition.localEulerAngles, GetPickUpSpeedRotation()).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
		{
			OnPutDown();
			base.gameObject.SetActive(value: false);
		});
	}

	public virtual void OnPutDown()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void RepaintButtonsForInteractable(Interactable interactable)
	{
	}

	public virtual void RepaintButtonsForEndHover()
	{
	}
}
