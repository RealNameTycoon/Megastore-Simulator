using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

public class LineChartUI : Selectable
{
	public struct ChartData
	{
		public string title;

		public string xAxisName;

		public string yAxisName;

		public List<ChartEntry> entries;
	}

	public struct ChartEntry(string title, string axisIndexLabelString = "", double value = 0.0)
	{
		public string title = title;

		public string axisIndexLabelString = axisIndexLabelString;

		public double value = value;
	}

	[SerializeField]
	private LineChart chart;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Sprite labelBackgroundSprite;

	[SerializeField]
	private LineStyle.Type lineType;

	[SerializeField]
	private float lineWidth;

	[SerializeField]
	private Color lineColor;

	[SerializeField]
	private TextMeshProUGUI notEnoughDataText;

	public void RandomRepaint(int lastDaysCount)
	{
		ChartData data = new ChartData
		{
			title = "TEST TITLE",
			entries = new List<ChartEntry>()
		};
		for (int i = 0; i < lastDaysCount; i++)
		{
			data.entries.Add(new ChartEntry
			{
				axisIndexLabelString = i.ToString(),
				value = Random.Range(0, 100)
			});
		}
		Repaint(data);
	}

	public void Repaint(ChartData data, bool shorten = false, string prefix = "")
	{
		notEnoughDataText.enabled = data.entries.Count == 0;
		if (chart == null)
		{
			chart = base.gameObject.AddComponent<LineChart>();
			chart.Init();
		}
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
		xAxis.type = Axis.AxisType.Category;
		yAxis.type = Axis.AxisType.Value;
		yAxis.axisLabel.formatterFunction = (int dataIndex, double value, string category, string content) => prefix + GetShortenedDataName(value);
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
		Line line = chart.AddSerie<Line>();
		line.lineStyle = new LineStyle(lineType, lineWidth);
		line.itemStyle.color = lineColor;
		if (data.entries.Count <= 7)
		{
			line.EnsureComponent<LabelStyle>();
			line.label.show = true;
			line.label.formatter = "{c}";
			if (shorten)
			{
				line.label.formatterFunction = (int dataIndex, double value, string category, string content) => prefix + GetShortenedDataName(value);
			}
			line.label.offset = new Vector3(0f, 25f, 0f);
			line.label.textStyle.fontSize = 30;
			if (labelBackgroundSprite != null)
			{
				line.label.background.show = true;
				line.label.background.color = UIManager.WhiteColor;
				line.label.background.sprite = labelBackgroundSprite;
				line.label.textStyle.color = Color.black;
			}
			else
			{
				line.label.textStyle.color = UIManager.WhiteColor;
			}
			line.symbol.show = true;
			line.symbol.type = SymbolType.Circle;
			line.symbol.size = 10f;
		}
		for (int num = 0; num < data.entries.Count; num++)
		{
			if (data.entries.Count <= 7)
			{
				chart.AddXAxisData(data.entries[num].axisIndexLabelString);
			}
			else
			{
				chart.AddXAxisData(data.entries[num].axisIndexLabelString);
			}
			if (data.entries[num].value == double.NaN)
			{
				chart.AddData(0, data.entries[num].value, prefix + GetShortenedDataName(0.0));
			}
			else
			{
				chart.AddData(0, data.entries[num].value, prefix + GetShortenedDataName(data.entries[num].value));
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
}
