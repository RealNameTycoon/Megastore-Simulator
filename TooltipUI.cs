using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TooltipUI : SingletonBehaviour<TooltipUI>
{
	[SerializeField]
	private GameObject parent;

	[SerializeField]
	private TextMeshProUGUI infoText;

	[SerializeField]
	private TextMeshProUGUI errorText;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private Transform tooltipOpener;

	public void ShowTooltip(string key, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		infoText.text = Locale.GetWord(key);
		infoText.enabled = true;
		errorText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
	}

	public void ShowTooltipWithFullText(string text, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		infoText.text = text;
		infoText.enabled = true;
		errorText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
	}

	public void ShowTimedTooltip(string key, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		infoText.text = Locale.GetWord(key);
		infoText.enabled = true;
		errorText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
		canvasGroup.DOFade(0f, 0.2f).SetDelay(3f).OnComplete(Close);
	}

	public void ShowTimedTooltipWithFullText(string text, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		infoText.text = text;
		infoText.enabled = true;
		errorText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
		canvasGroup.DOFade(0f, 0.2f).SetDelay(3f).OnComplete(Close);
	}

	public bool HasOpened(Transform transform)
	{
		if (tooltipOpener != null)
		{
			return tooltipOpener.Equals(transform);
		}
		return false;
	}

	public void ShowTimedError(string key, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		errorText.text = Locale.GetWord(key);
		errorText.enabled = true;
		infoText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
		HapticController.Vibrate(PresetType.MediumImpact);
		canvasGroup.DOFade(0f, 0.2f).SetDelay(3f).OnComplete(Close);
	}

	public void ShowError(string key, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		errorText.text = Locale.GetWord(key);
		errorText.enabled = true;
		infoText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
	}

	public void ShowErrorWithFullText(string text, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		errorText.text = text;
		errorText.enabled = true;
		infoText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.ERROR_TOOLTIP);
	}

	public void ShowTimedErrorWithFullText(string text, Transform tooltipOpenerTransform)
	{
		tooltipOpener = tooltipOpenerTransform;
		errorText.text = text;
		errorText.enabled = true;
		infoText.enabled = false;
		parent.SetActive(value: true);
		canvasGroup.DOKill();
		canvasGroup.alpha = 1f;
		canvasGroup.DOFade(0f, 0.2f).SetDelay(3f).OnComplete(Close);
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.ERROR_TOOLTIP);
	}

	public void Close()
	{
		tooltipOpener = null;
		parent.SetActive(value: false);
	}

	public bool IsOpen()
	{
		return parent.activeSelf;
	}
}
