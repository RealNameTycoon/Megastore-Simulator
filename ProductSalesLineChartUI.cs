using UnityEngine;
using UnityEngine.UI;

public class ProductSalesLineChartUI : LineChartUI
{
	[SerializeField]
	private Image productImage;

	[SerializeField]
	private Toggle lastDayToggle;

	[SerializeField]
	private Toggle sevenDaysToggle;

	[SerializeField]
	private Toggle thirtyDaysToggle;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Canvas canvas;

	private ProductType currentType = ProductType.NONE;

	private new void Start()
	{
		lastDayToggle.SetIsOnWithoutNotify(value: false);
		sevenDaysToggle.SetIsOnWithoutNotify(value: true);
		thirtyDaysToggle.SetIsOnWithoutNotify(value: false);
		sevenDaysToggle.targetGraphic.color = TabbedPanel.selectedBGColor;
		thirtyDaysToggle.targetGraphic.color = TabbedPanel.deselectedBGColor;
		lastDayToggle.targetGraphic.color = TabbedPanel.deselectedBGColor;
		lastDayToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				Repaint(currentType, 1);
			}
			lastDayToggle.targetGraphic.color = (isOn ? TabbedPanel.selectedBGColor : TabbedPanel.deselectedBGColor);
		});
		sevenDaysToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				Repaint(currentType, 7);
			}
			sevenDaysToggle.targetGraphic.color = (isOn ? TabbedPanel.selectedBGColor : TabbedPanel.deselectedBGColor);
		});
		thirtyDaysToggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				Repaint(currentType, 30);
			}
			thirtyDaysToggle.targetGraphic.color = (isOn ? TabbedPanel.selectedBGColor : TabbedPanel.deselectedBGColor);
		});
		closeButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
			EventManager.NotifyEvent(UIEvents.CLOSE_PRODUCT_SALES_GRAPH);
		});
	}

	public void Repaint(ProductType type, int lastDaysCount)
	{
		productImage.sprite = SingletonBehaviour<ProductPool>.Instance.GetProductData(type).productSprite;
		ChartData chartData = SingletonBehaviour<StatisticsManager>.Instance.GetChartData(type, lastDaysCount);
		Repaint(chartData);
	}

	public void Repaint(ProductType type)
	{
		currentType = type;
		if (lastDayToggle.isOn)
		{
			Repaint(type, 1);
		}
		if (sevenDaysToggle.isOn)
		{
			Repaint(type, 7);
		}
		else if (thirtyDaysToggle.isOn)
		{
			Repaint(type, 30);
		}
	}

	public void Open()
	{
		canvas.enabled = true;
		base.gameObject.SetActive(value: true);
		SingletonBehaviour<InputManager>.Instance.SelectElement(closeButton.gameObject);
	}
}
