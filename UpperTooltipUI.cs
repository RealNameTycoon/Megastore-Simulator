using DG.Tweening;
using TMPro;
using UnityEngine;

public class UpperTooltipUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup parent;

	[SerializeField]
	private TextMeshProUGUI infoText;

	private int haltedUserCount;

	private void Start()
	{
		EventManager.AddListener<bool>(CustomerEvents.CUSTOMER_HALTED, OnCustomerHalted);
	}

	private void OnCustomerHalted(bool state)
	{
		if (state)
		{
			haltedUserCount++;
		}
		else
		{
			haltedUserCount--;
		}
		if (haltedUserCount > 0)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	public void Open()
	{
		parent.gameObject.SetActive(value: true);
		parent.DOKill();
		parent.DOFade(1f, 0.2f);
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.ERROR_TOOLTIP);
	}

	public void Close()
	{
		parent.DOKill();
		parent.DOFade(1f, 0.2f).OnComplete(delegate
		{
			parent.gameObject.SetActive(value: false);
		});
	}

	public bool IsOpen()
	{
		return parent.gameObject.activeSelf;
	}
}
