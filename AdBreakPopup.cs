using System.Collections;
using DFTGames.Localization;
using TMPro;
using UnityEngine;

public class AdBreakPopup : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TextMeshProUGUI timeText;

	private WaitForSeconds waiter;

	private IEnumerator CountDownRoutine()
	{
		int remainingTime = 5;
		timeText.text = Locale.CurrentLanguageStrings["ad_break_in"].Replace("{0}", remainingTime.ToString());
		for (int i = 0; i < 5; i++)
		{
			yield return waiter;
			remainingTime--;
			timeText.text = Locale.CurrentLanguageStrings["ad_break_in"].Replace("{0}", remainingTime.ToString());
		}
		AdManager.Instance.ShowInterstitial(delegate
		{
			EventLogger.LogEvent("c_interstitial_break");
			canvas.enabled = false;
		});
	}

	private void Start()
	{
		waiter = new WaitForSeconds(1f);
	}

	private void Update()
	{
	}
}
