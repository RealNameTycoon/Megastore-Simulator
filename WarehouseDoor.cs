using DG.Tweening;
using UnityEngine;

public class WarehouseDoor : Clickable
{
	[SerializeField]
	private Transform door;

	private Vector3 doorClosedEuler = Vector3.zero;

	private Vector3 doorOpenedEuler = new Vector3(0f, 100f, 0f);

	private bool isOpen;

	public void OnDoorClicked()
	{
		if (isOpen)
		{
			door.DOKill();
			door.DOLocalRotate(doorClosedEuler, 180f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
		}
		else
		{
			door.DOKill();
			door.DOLocalRotate(doorOpenedEuler, 180f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
		}
		isOpen = !isOpen;
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		OnDoorClicked();
	}

	protected override string GetToolTip()
	{
		if (isOpen)
		{
			return "close_door";
		}
		return "open_door";
	}
}
