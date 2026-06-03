using DG.Tweening;
using UnityEngine;

public class OpenCloseLabel : SingletonBehaviour<OpenCloseLabel>
{
	[SerializeField]
	private Transform openTransform;

	[SerializeField]
	private Transform closedTransform;

	private string IS_OPEN_KEY = "IsMarketOpen";

	private bool isOpen;

	private Vector3 openRotation = Vector3.zero;

	private Vector3 closedRotation = Vector3.up * 180f;

	public bool IsOpen => isOpen;

	public Transform ClosedSign => closedTransform;

	public void OnLabelClicked()
	{
		isOpen = !isOpen;
		GenericDataSerializer.SaveBool(IS_OPEN_KEY, isOpen);
		if (isOpen)
		{
			openTransform.DOKill();
			if (!openTransform.gameObject.activeSelf)
			{
				openTransform.gameObject.SetActive(value: true);
			}
			openTransform.DOScale(Vector3.one, 0.2f);
			closedTransform.DOKill();
			closedTransform.DOScale(Vector3.zero, 0.2f).OnComplete(delegate
			{
				closedTransform.gameObject.SetActive(value: false);
			});
			EventManager.NotifyEvent(CustomerEvents.SHOP_OPENED);
		}
		else
		{
			closedTransform.DOKill();
			if (!closedTransform.gameObject.activeSelf)
			{
				closedTransform.gameObject.SetActive(value: true);
			}
			closedTransform.DOScale(Vector3.one, 0.2f);
			openTransform.DOKill();
			openTransform.DOScale(Vector3.zero, 0.2f).OnComplete(delegate
			{
				openTransform.gameObject.SetActive(value: false);
			});
			EventManager.NotifyEvent(CustomerEvents.SHOP_CLOSED);
		}
		HapticController.Vibrate(PresetType.LightImpact);
	}

	private void Start()
	{
		isOpen = GenericDataSerializer.LoadBool(IS_OPEN_KEY);
		if (isOpen)
		{
			openTransform.localScale = Vector3.one;
			closedTransform.localScale = Vector3.zero;
			closedTransform.gameObject.SetActive(value: false);
		}
		else
		{
			openTransform.localScale = Vector3.zero;
			closedTransform.localScale = Vector3.one;
			openTransform.gameObject.SetActive(value: false);
		}
	}

	private void Rotate()
	{
		base.transform.DOKill();
		if (isOpen)
		{
			base.transform.DORotate(openRotation, 360f).SetSpeedBased(isSpeedBased: true);
		}
		else
		{
			base.transform.DORotate(closedRotation, 360f).SetSpeedBased(isSpeedBased: true);
		}
	}

	private void Update()
	{
	}
}
