using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
	public enum AnimationType
	{
		NONE = -1,
		CLICK_SCALE_ANIMATION,
		BOUNCE,
		BOUNCE_IN_AWHILE
	}

	[SerializeField]
	private AnimationType animationType;

	[SerializeField]
	private bool useGamepadColor;

	[SerializeField]
	private Color gamepadColor = new Color(36f / 85f, 0.5882353f, 0.7372549f, 1f);

	private const float scaleDuration = 0.1f;

	private const float scaleDownFactor = 0.97f;

	private const float bounceDuration = 0.25f;

	private const float bounceScaleFactor = 1.03f;

	private const float bounceDelay = 2f;

	private Selectable parentButton;

	private float initialScale;

	private Color initialColor;

	private void Awake()
	{
		base.transform.DOKill();
		initialScale = base.transform.localScale.x;
		parentButton = base.gameObject.GetComponent<Selectable>();
		if (parentButton != null && parentButton.targetGraphic != null)
		{
			initialColor = parentButton.targetGraphic.color;
		}
		if (animationType == AnimationType.BOUNCE)
		{
			base.transform.DOScale(initialScale * 1.03f, 0.25f).SetEase(Ease.Linear).SetUpdate(isIndependentUpdate: true)
				.SetLoops(-1, LoopType.Yoyo);
		}
		else if (animationType == AnimationType.BOUNCE_IN_AWHILE)
		{
			Sequence sequence = DOTween.Sequence();
			sequence.Append(base.transform.DOScale(initialScale * 1.03f, 0.25f).SetEase(Ease.Linear).SetUpdate(isIndependentUpdate: true)
				.SetLoops(2, LoopType.Yoyo));
			sequence.SetDelay(2f);
			sequence.SetLoops(-1);
			sequence.Play();
		}
	}

	private void OnDestroy()
	{
		if (DOTween.IsTweening(base.transform))
		{
			base.transform.DOKill();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		ScaleDown();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		ScaleUp();
	}

	private void ScaleDown()
	{
		if (!(parentButton != null) || parentButton.interactable)
		{
			base.transform.DOKill();
			if (animationType == AnimationType.CLICK_SCALE_ANIMATION)
			{
				base.transform.DOScale(initialScale * 0.97f, 0.1f).SetEase(Ease.OutSine).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	private void ScaleUp()
	{
		if (!(parentButton != null) || parentButton.interactable)
		{
			base.transform.DOKill();
			if (animationType == AnimationType.CLICK_SCALE_ANIMATION)
			{
				base.transform.DOScale(initialScale, 0.1f).SetEase(Ease.OutSine).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		ScaleDown();
		if (useGamepadColor && SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
		{
			parentButton.targetGraphic.color = gamepadColor;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		ScaleUp();
		if (useGamepadColor && SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
		{
			parentButton.targetGraphic.color = initialColor;
		}
	}
}
