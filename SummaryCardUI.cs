using System;
using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryCardUI : Selectable
{
	[SerializeField]
	private TextMeshProUGUI valueText;

	[SerializeField]
	private TextMeshProUGUI previousValueText;

	[SerializeField]
	private Color positiveColor = new Color(33f / 85f, 43f / 51f, 4f / 15f);

	[SerializeField]
	private Color negativeColor = new Color(40f / 51f, 0.29411766f, 0.29411766f);

	public void Repaint(int value, int previousValue, int timeFrame)
	{
		if (value == 0)
		{
			valueText.text = Locale.GetWord("no_data");
			previousValueText.text = "-";
			return;
		}
		valueText.text = value.ToString();
		if (previousValue == 0)
		{
			previousValueText.text = Locale.GetWord("no_data");
			previousValueText.color = Color.gray;
			return;
		}
		float num = (float)(value - previousValue) / (float)previousValue * 100f;
		previousValueText.color = ((num > 0f) ? positiveColor : negativeColor);
		string text = ((num > 0f) ? "+%" : "-%");
		text += Mathf.Abs(num).ToString("0.00", CultureInfo.InvariantCulture);
		previousValueText.text = Locale.GetWord("stat_comparison_info").Replace("{0}", text).Replace("{1}", timeFrame.ToString());
	}

	public void Repaint(float value, float previousValue, int timeFrame, string prefix)
	{
		if (Mathf.Approximately(value, 0f))
		{
			valueText.text = Locale.GetWord("no_data");
			previousValueText.text = "-";
			return;
		}
		valueText.text = prefix + FormatMoneySummary(value);
		if (Mathf.Approximately(previousValue, 0f))
		{
			previousValueText.text = Locale.GetWord("no_data");
			previousValueText.color = Color.gray;
			return;
		}
		float num = (value - previousValue) / previousValue * 100f;
		previousValueText.color = ((num > 0f) ? positiveColor : negativeColor);
		string text = ((num > 0f) ? "+%" : "-%");
		text += Mathf.Abs(num).ToString("0.00", CultureInfo.InvariantCulture);
		previousValueText.text = Locale.GetWord("stat_comparison_info").Replace("{0}", text).Replace("{1}", timeFrame.ToString());
	}

	private string FormatMoneySummary(double value)
	{
		double num = Math.Abs(value);
		if (num >= 1000000.0)
		{
			return (value / 1000000.0).ToString("0.##") + "M";
		}
		if (num >= 1000.0)
		{
			return (value / 1000.0).ToString("0.##") + "K";
		}
		return value.ToString("0.##");
	}
}
