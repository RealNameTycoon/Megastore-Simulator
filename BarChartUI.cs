using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

public class BarChartUI : Selectable
{
	[SerializeField]
	private BarChart chart;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private bool isHorizontal;

	[SerializeField]
	private Color lineColor;

	[SerializeField]
	private TextMeshProUGUI notEnoughDataText;

	private static float HORIZONTAL_TEXT_LIMIT_WIDTH = 225f;

	private static float BAR_MAX_WIDTH = 100f;

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
		Repaint(data);
	}

	public void Repaint(LineChartUI.ChartData data)
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
		chart.EnsureChartComponent<Tooltip>().show = true;
		chart.EnsureChartComponent<Tooltip>().numericFormatter = "N0";
		chart.EnsureChartComponent<Legend>().show = false;
		XAxis xAxis = chart.EnsureChartComponent<XAxis>();
		YAxis yAxis = chart.EnsureChartComponent<YAxis>();
		xAxis.show = true;
		yAxis.show = true;
		if (isHorizontal)
		{
			HideAxisVisuals(xAxis);
			HideAxisVisuals(yAxis, showLabel: true);
		}
		if (isHorizontal)
		{
			xAxis.type = Axis.AxisType.Value;
			yAxis.type = Axis.AxisType.Category;
			yAxis.axisLabel.textLimit.enable = true;
			yAxis.axisLabel.textLimit.maxWidth = HORIZONTAL_TEXT_LIMIT_WIDTH;
		}
		else
		{
			xAxis.type = Axis.AxisType.Category;
			yAxis.type = Axis.AxisType.Value;
		}
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
		xAxis.boundaryGap = true;
		chart.RemoveData();
		Bar bar = chart.AddSerie<Bar>();
		bar.barType = BarType.Normal;
		bar.itemStyle.color = lineColor;
		bar.barMaxWidth = BAR_MAX_WIDTH;
		if (data.entries.Count <= 7 || isHorizontal)
		{
			bar.EnsureComponent<LabelStyle>();
			bar.label.show = true;
			bar.label.formatter = "{c}";
			bar.label.numericFormatter = "N0";
			bar.label.offset = (isHorizontal ? new Vector3(45f, 0f, 0f) : new Vector3(0f, 25f, 0f));
			bar.label.textStyle.fontSize = 30;
			bar.label.textStyle.color = UIManager.WhiteColor;
		}
		for (int i = 0; i < data.entries.Count; i++)
		{
			if (isHorizontal)
			{
				chart.AddYAxisData(data.entries[i].axisIndexLabelString);
			}
			else
			{
				chart.AddXAxisData(data.entries[i].axisIndexLabelString);
			}
			if (data.entries[i].value == double.NaN)
			{
				chart.AddData(0, 0.0);
			}
			else
			{
				chart.AddData(0, data.entries[i].value);
			}
		}
	}

	private string FormatChartValue(double value)
	{
		return value.ToString("N0");
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
