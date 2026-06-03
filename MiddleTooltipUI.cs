using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MiddleTooltipUI : SingletonBehaviour<MiddleTooltipUI>
{
	[SerializeField]
	private CanvasGroup parent;

	[SerializeField]
	private TextMeshProUGUI infoText;

	public void Open(string key)
	{
		infoText.text = Locale.GetWord(key);
		parent.DOKill();
		parent.DOFade(1f, 0.2f).OnComplete(delegate
		{
			parent.DOFade(0f, 0.2f).SetDelay(3f);
		});
	}

	public void Close()
	{
		parent.DOKill();
		parent.DOFade(1f, 0.2f);
	}
}
