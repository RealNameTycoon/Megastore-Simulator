using DFTGames.Localization;
using UnityEngine;
using UnityEngine.UI;

public class ProductStatRow : MonoBehaviour
{
	[SerializeField]
	private Image rowBackground;

	[SerializeField]
	private Image productImage;

	[SerializeField]
	private Text productName;

	[SerializeField]
	private Text lastDaySoldAmount;

	[SerializeField]
	private Text last7DaysSoldAmount;

	[SerializeField]
	private Text last30DaysSoldAmount;

	[SerializeField]
	private Button graphButton;

	private ProductData data;

	private static Color DARKER_COLOR = new Color(0.4627451f, 0.57254905f, 0.6745098f);

	private static Color LIGHTER_COLOR = new Color(29f / 85f, 0.42745098f, 0.5019608f);

	private void Awake()
	{
		graphButton.onClick.AddListener(delegate
		{
			EventManager.NotifyEvent(UIEvents.OPEN_PRODUCT_SALES_GRAPH, data.type);
		});
	}

	public void Repaint(ProductData data, int index, int oneDaySold, int last7DaysSold, int last30DaysSold)
	{
		this.data = data;
		productImage.sprite = data.productSprite;
		productName.text = Locale.GetWord(data.type.ToString());
		lastDaySoldAmount.text = oneDaySold.ToString();
		last7DaysSoldAmount.text = last7DaysSold.ToString();
		last30DaysSoldAmount.text = last30DaysSold.ToString();
		if (index % 2 == 0)
		{
			rowBackground.color = DARKER_COLOR;
		}
		else
		{
			rowBackground.color = LIGHTER_COLOR;
		}
	}

	public Selectable GetSelectable()
	{
		return graphButton;
	}

	public Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		AddBottomNavigation(graphButton, down);
		AddTopNavigation(graphButton, up);
		return graphButton;
	}

	private void AddBottomNavigation(Selectable target, Selectable bottom)
	{
		Navigation navigation = target.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnDown = bottom;
		target.navigation = navigation;
	}

	private void AddTopNavigation(Selectable target, Selectable top)
	{
		Navigation navigation = target.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnUp = top;
		target.navigation = navigation;
	}
}
