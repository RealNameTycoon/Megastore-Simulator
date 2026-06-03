using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

public class DoubleBarChartUI : Selectable
{
	[SerializeField]
	private BarChart chart;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Color lineColor;

	[SerializeField]
	private Color secondaryLineColor;

	[SerializeField]
	private Image primaryLegendIcon;

	[SerializeField]
	private Image secondaryLegendIcon;

	[SerializeField]
	private TextMeshProUGUI currentTimeFrameText;

	[SerializeField]
	private TextMeshProUGUI previousTimeFrameText;

	[SerializeField]
	private Vector3 xAxisLabelOffset = Vector3.zero;

	[SerializeField]
	private TextMeshProUGUI notEnoughDataText;

	public void RandomRepaint(int lastDaysCount)
	{
		LineChartUI.ChartData data = new LineChartUI.ChartData
		{
			title = "TEST TITLE",
			entries = new List<LineChartUI.ChartEntry>()
		};
		for (int i = 0; i < lastDaysCount; i++)
		{
			data.entries.Add(new LineChartUI.ChartEntry
			{
				axisIndexLabelString = i.ToString(),
				value = Random.Range(0, 100)
			});
		}
		LineChartUI.ChartData secondaryData = new LineChartUI.ChartData
		{
			entries = new List<LineChartUI.ChartEntry>()
		};
		for (int j = 0; j < lastDaysCount; j++)
		{
			secondaryData.entries.Add(new LineChartUI.ChartEntry
			{
				axisIndexLabelString = j.ToString(),
				value = Random.Range(0, 100)
			});
		}
		Repaint(data, secondaryData, 7);
	}

	public void Repaint(LineChartUI.ChartData data, LineChartUI.ChartData secondaryData, int timeFrame, bool shorten = false, string prefix = "")
	{
		notEnoughDataText.enabled = data.entries.Count == 0;
		if (titleText != null)
		{
			titleText.text = data.title;
		}
		else
		{
			chart.EnsureChartComponent<Title>().show = true;
			chart.EnsureChartComponent<Title>().text = data.title;
		}
		if (currentTimeFrameText != null && previousTimeFrameText != null)
		{
			currentTimeFrameText.text = Locale.GetWord("current_time_frame_n").Replace("{0}", timeFrame.ToString());
			previousTimeFrameText.text = Locale.GetWord("previous_time_frame_n").Replace("{0}", timeFrame.ToString());
		}
		XAxis xAxis = chart.EnsureChartComponent<XAxis>();
		YAxis yAxis = chart.EnsureChartComponent<YAxis>();
		xAxis.show = true;
		yAxis.show = true;
		xAxis.type = Axis.AxisType.Category;
		yAxis.type = Axis.AxisType.Value;
		if (shorten)
		{
			yAxis.axisLabel.formatterFunction = (int dataIndex, double value, string category, string content) => prefix + GetShortenedDataName(value);
		}
		primaryLegendIcon.color = lineColor;
		secondaryLegendIcon.color = secondaryLineColor;
		if (data.xAxisName != null)
		{
			xAxis.axisName.show = true;
			xAxis.axisName.name = data.xAxisName;
			xAxis.axisName.labelStyle.textStyle.color = UIManager.WhiteColor;
			xAxis.axisName.labelStyle.textStyle.fontSize = 30;
		}
		if (data.yAxisName != null)
		{
			yAxis.axisName.show = true;
			yAxis.axisName.name = data.yAxisName;
			yAxis.axisName.labelStyle.textStyle.color = UIManager.WhiteColor;
			yAxis.axisName.labelStyle.textStyle.fontSize = 30;
		}
		xAxis.max = 30.0;
		xAxis.splitNumber = ((data.entries.Count > 7) ? (data.entries.Count / 5 + 1) : data.entries.Count);
		xAxis.axisLabel.offset = xAxisLabelOffset;
		xAxis.boundaryGap = true;
		chart.RemoveData();
		Bar bar = chart.AddSerie<Bar>();
		bar.barType = BarType.Normal;
		bar.itemStyle.color = lineColor;
		Bar bar2 = chart.AddSerie<Bar>();
		bar2.barType = BarType.Normal;
		bar2.itemStyle.color = secondaryLineColor;
		chart.EnsureChartComponent<Tooltip>().show = true;
		chart.EnsureChartComponent<Tooltip>().numericFormatter = "N0";
		chart.EnsureChartComponent<Legend>().show = false;
		if (data.entries.Count <= 7)
		{
			bar.EnsureComponent<LabelStyle>();
			bar.label.show = true;
			bar.label.formatter = "{c}";
			bar.label.offset = new Vector3(0f, 25f, 0f);
			bar.label.textStyle.fontSize = 30;
			bar.label.textStyle.color = UIManager.WhiteColor;
			bar2.EnsureComponent<LabelStyle>();
			bar2.label.show = true;
			bar2.label.formatter = "{c}";
			bar2.label.offset = new Vector3(0f, 25f, 0f);
			bar2.label.textStyle.fontSize = 30;
			bar2.label.textStyle.color = UIManager.WhiteColor;
			if (shorten)
			{
				bar.label.formatterFunction = (int dataIndex, double value, string category, string content) => prefix + GetShortenedDataName(value);
				bar2.label.formatterFunction = (int dataIndex, double value, string category, string content) => prefix + GetShortenedDataName(value);
			}
		}
		for (int num = 0; num < data.entries.Count; num++)
		{
			chart.AddXAxisData(data.entries[num].axisIndexLabelString);
			if (data.entries[num].value == double.NaN)
			{
				chart.AddData(0, 0.0);
			}
			else
			{
				chart.AddData(0, data.entries[num].value);
			}
			if (secondaryData.entries[num].value == double.NaN)
			{
				chart.AddData(1, 0.0);
			}
			else
			{
				chart.AddData(1, secondaryData.entries[num].value);
			}
		}
	}

	private string GetShortenedDataName(double value)
	{
		if (value < 1000.0)
		{
			return value.ToString("0.##");
		}
		if (value < 1000000.0)
		{
			return (value / 1000.0).ToString("0.#") + "K";
		}
		return (value / 1000000.0).ToString("0.#") + "M";
	}

	private void HideAxisVisuals(Axis axis, bool showLabel = false)
	{
		axis.show = true;
		axis.axisLine.show = false;
		axis.axisTick.show = false;
		axis.axisLabel.show = showLabel;
		axis.splitLine.show = false;
		axis.splitArea.show = false;
		axis.axisName.show = false;
	}
}
