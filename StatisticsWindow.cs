using UnityEngine;
using UnityEngine.UI;

public class StatisticsWindow : TabWindow
{
	[SerializeField]
	private Toggle oneDayToggle;

	[SerializeField]
	private Toggle sevenDaysToggle;

	[SerializeField]
	private Toggle thirtyDaysToggle;

	[SerializeField]
	private SummaryCardUI revenueCard;

	[SerializeField]
	private SummaryCardUI profitCard;

	[SerializeField]
	private SummaryCardUI customersCard;

	[SerializeField]
	private SummaryCardUI soldProductsCard;

	[SerializeField]
	private LineChartUI revenueLineChart;

	[SerializeField]
	private BarChartUI totalSoldProductsBarChart;

	[SerializeField]
	private BarChartUI salesByDepartmentBarChart;

	[SerializeField]
	private BarChartUI topSoldProductsBarChart;

	[SerializeField]
	private LineChartUI averageSpendPerCustomerLineChart;

	[SerializeField]
	private DoubleBarChartUI prevVsCurrentCustomerIssuesBarChart;

	[SerializeField]
	private DoubleBarChartUI revenueVsExpensesBarChart;

	[SerializeField]
	private LineChartUI balanceOverTimeLineChart;

	private int currentSelectedTimeFrame = 7;

	protected override void Start()
	{
		oneDayToggle.SetIsOnWithoutNotify(value: false);
		sevenDaysToggle.SetIsOnWithoutNotify(value: false);
		thirtyDaysToggle.SetIsOnWithoutNotify(value: false);
		oneDayToggle.targetGraphic.color = TabbedPanel.deselectedBGColor;
		sevenDaysToggle.targetGraphic.color = TabbedPanel.selectedBGColor;
		thirtyDaysToggle.targetGraphic.color = TabbedPanel.deselectedBGColor;
		oneDayToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				currentSelectedTimeFrame = 1;
				Repaint(1);
			}
			oneDayToggle.targetGraphic.color = (isOn ? TabbedPanel.selectedBGColor : TabbedPanel.deselectedBGColor);
		});
		sevenDaysToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				currentSelectedTimeFrame = 7;
				Repaint(7);
			}
			sevenDaysToggle.targetGraphic.color = (isOn ? TabbedPanel.selectedBGColor : TabbedPanel.deselectedBGColor);
		});
		thirtyDaysToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				currentSelectedTimeFrame = 30;
				Repaint(30);
			}
			thirtyDaysToggle.targetGraphic.color = (isOn ? TabbedPanel.selectedBGColor : TabbedPanel.deselectedBGColor);
		});
		currentSelectedTimeFrame = 7;
		Repaint(7);
		base.Start();
	}

	public void OnNewDayStarted()
	{
		Repaint(currentSelectedTimeFrame);
	}

	private void Repaint(int timeFrame)
	{
		var (value, previousValue) = SingletonBehaviour<StatisticsManager>.Instance.GetCurrentAndPreviousDayRevenue(timeFrame);
		var (value2, previousValue2) = SingletonBehaviour<StatisticsManager>.Instance.GetCurrentAndPreviousDayProfit(timeFrame);
		var (value3, previousValue3) = SingletonBehaviour<StatisticsManager>.Instance.GetCurrentAndPreviousDayCustomers(timeFrame);
		var (value4, previousValue4) = SingletonBehaviour<StatisticsManager>.Instance.GetCurrentAndPreviousDaySoldProducts(timeFrame);
		revenueCard.Repaint(value, previousValue, timeFrame, "$");
		profitCard.Repaint(value2, previousValue2, timeFrame, "$");
		customersCard.Repaint(value3, previousValue3, timeFrame);
		soldProductsCard.Repaint(value4, previousValue4, timeFrame);
		revenueLineChart.Repaint(SingletonBehaviour<StatisticsManager>.Instance.GetDailySalesChartData(timeFrame), shorten: true, "$");
		totalSoldProductsBarChart.Repaint(SingletonBehaviour<StatisticsManager>.Instance.GetDailySoldProductsChartData(timeFrame));
		salesByDepartmentBarChart.Repaint(SingletonBehaviour<StatisticsManager>.Instance.GetDailySalesByDepartmentChartData(timeFrame));
		topSoldProductsBarChart.Repaint(SingletonBehaviour<StatisticsManager>.Instance.GetDailyTopSoldProductsChartData(timeFrame));
		averageSpendPerCustomerLineChart.Repaint(SingletonBehaviour<StatisticsManager>.Instance.GetDailyAverageSpendPerCustomerChartData(timeFrame), shorten: true, "$");
		var (data, secondaryData) = SingletonBehaviour<StatisticsManager>.Instance.GetDailyPreviousAndCurrentCustomerIssuesChartData(timeFrame);
		prevVsCurrentCustomerIssuesBarChart.Repaint(data, secondaryData, timeFrame);
		var (data2, secondaryData2) = SingletonBehaviour<StatisticsManager>.Instance.GetDailyRevenueVsExpensesChartData(timeFrame);
		revenueVsExpensesBarChart.Repaint(data2, secondaryData2, timeFrame, shorten: true, "$");
		balanceOverTimeLineChart.Repaint(SingletonBehaviour<StatisticsManager>.Instance.GetDailyBalanceOverTimeChartData(timeFrame), shorten: true, "$");
	}

	public override Selectable GetFirstSelectable()
	{
		return oneDayToggle;
	}
}
