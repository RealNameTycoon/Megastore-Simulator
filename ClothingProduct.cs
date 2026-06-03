using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ClothingProduct : Product
{
	[SerializeField]
	private GameObject hungCloth;

	[SerializeField]
	private GameObject foldedCloth;

	private const float INITIAL_SWING_ANGLE = 10f;

	private const float DAMPING_FACTOR = 0.5f;

	private const float BASE_DURATION = 0.55f;

	private static Vector3 INITIAL_HANGER_ROTATION = new Vector3(-90f, 0f, 90f);

	private WaitForSeconds waiter = new WaitForSeconds(Shelf.PLACE_ANIMATION_DURATION - 0.2f);

	public void EnableHangObjectOnly()
	{
		if (hungCloth != null)
		{
			hungCloth.SetActive(value: true);
			hungCloth.GetComponent<MeshRenderer>().enabled = false;
		}
		if (foldedCloth != null)
		{
			foldedCloth.SetActive(value: false);
		}
	}

	public void AssignFoldedAndHung()
	{
		hungCloth = null;
		foldedCloth = null;
		foreach (Transform item in base.transform)
		{
			if (!(item == null))
			{
				string text = item.name;
				if (hungCloth == null && text.IndexOf("hanger", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					hungCloth = item.gameObject;
				}
				else if (foldedCloth == null && text.IndexOf("Folded", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					foldedCloth = item.gameObject;
				}
			}
		}
	}

	public override void OnBeforePlace(PlaceableType placeableType, bool isStart)
	{
		base.OnBeforePlace(placeableType, isStart);
		if (placeableType >= PlaceableType.HANGER_CLOTH_RACK_1 && placeableType <= PlaceableType.HANGER_CLOTH_RACK_5)
		{
			if (foldedCloth != null)
			{
				foldedCloth.SetActive(value: false);
			}
			hungCloth.SetActive(value: true);
			if (!isStart)
			{
				StartCoroutine(SwingAnimationRoutine());
			}
		}
	}

	private IEnumerator SwingAnimationRoutine()
	{
		yield return waiter;
		AnimateSwing();
	}

	public void AnimateSwing()
	{
		if (hungCloth != null)
		{
			PlaySwingAnimation(hungCloth.transform);
		}
	}

	public override void OnBeforeTake()
	{
		base.OnBeforeTake();
		StopCoroutine(SwingAnimationRoutine());
		hungCloth.transform.DOKill();
		if (foldedCloth != null)
		{
			hungCloth.SetActive(value: false);
			foldedCloth.SetActive(value: true);
		}
		hungCloth.transform.localEulerAngles = INITIAL_HANGER_ROTATION;
	}

	private void PlaySwingAnimation(Transform hanger)
	{
		hanger.DOKill();
		Sequence sequence = DOTween.Sequence();
		float num = 10f;
		float duration = 0.55f;
		_ = hanger.localEulerAngles;
		sequence.Append(hanger.DOLocalRotate(new Vector3(num - 90f, INITIAL_HANGER_ROTATION.y, INITIAL_HANGER_ROTATION.z), duration).SetEase(Ease.Linear));
		sequence.Append(hanger.DOLocalRotate(new Vector3(0f - num - 90f, INITIAL_HANGER_ROTATION.y, INITIAL_HANGER_ROTATION.z), duration).SetEase(Ease.Linear));
		num *= 0.5f;
		sequence.Append(hanger.DOLocalRotate(new Vector3(num - 90f, INITIAL_HANGER_ROTATION.y, INITIAL_HANGER_ROTATION.z), duration).SetEase(Ease.Linear));
		sequence.Append(hanger.DOLocalRotate(new Vector3(0f - num - 90f, INITIAL_HANGER_ROTATION.y, INITIAL_HANGER_ROTATION.z), duration).SetEase(Ease.Linear));
		sequence.Append(hanger.DOLocalRotate(INITIAL_HANGER_ROTATION, duration).SetEase(Ease.Linear));
		sequence.SetEase(Ease.OutQuad);
		sequence.Play();
	}
}
