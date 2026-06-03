using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PopupAnimation : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Transform content;

	[SerializeField]
	private Transform target;

	private const float moveDuration = 0.25f;

	private Vector3 initialPosition;

	private bool animating;

	private void Start()
	{
		initialPosition = content.transform.position;
	}

	public void AnimatePopup()
	{
		if (canvas != null)
		{
			canvas.enabled = true;
		}
		content.transform.position = initialPosition;
		content.transform.DOMove(target.position, 0.25f).SetEase(Ease.OutSine).OnComplete(delegate
		{
			animating = false;
		});
	}

	public void AnimatePopupWithDelay(float delay)
	{
		animating = true;
		if (canvas != null)
		{
			canvas.enabled = true;
		}
		content.transform.position = initialPosition;
		content.transform.DOMove(target.position, 0.25f).SetEase(Ease.OutSine).SetDelay(delay)
			.OnComplete(delegate
			{
				StartCoroutine(WaitAndStopAnimating());
			});
	}

	private IEnumerator WaitAndStopAnimating()
	{
		yield return new WaitForSeconds(1.5f);
		animating = false;
	}

	public void ResetPopup()
	{
		content.transform.DOKill();
		content.transform.position = initialPosition;
		if (canvas != null)
		{
			canvas.enabled = false;
		}
	}

	public bool IsAnimating()
	{
		return animating;
	}

	public bool IsOpen()
	{
		return canvas.enabled;
	}

	public bool IsEnabled()
	{
		return content.transform.position != initialPosition;
	}
}
