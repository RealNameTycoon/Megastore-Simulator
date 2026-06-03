using System.Globalization;
using System.Text.RegularExpressions;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductPriceRow : MonoBehaviour
{
	[SerializeField]
	private Image rowBackground;

	[SerializeField]
	private Image productImage;

	[SerializeField]
	private TextMeshProUGUI productName;

	[SerializeField]
	private TextMeshProUGUI productCost;

	[SerializeField]
	private TextMeshProUGUI marketPrice;

	[SerializeField]
	private TextMeshProUGUI salesImpact;

	[SerializeField]
	private TextMeshProUGUI profit;

	[SerializeField]
	private TMP_InputField priceField;

	[SerializeField]
	private Button decreasePriceButton;

	[SerializeField]
	private Button increasePriceButton;

	[SerializeField]
	private Button marketPriceButton;

	[SerializeField]
	private Button applyButton;

	private ProductData data;

	private static float PRICE_CHANGE_THRESHOLD = 0.005f;

	private const int DARKER_ALPHA = 200;

	private const int LIGHTER_ALPHA = 150;

	private void Start()
	{
		decreasePriceButton.onClick.AddListener(DecreasePrice);
		increasePriceButton.onClick.AddListener(IncreasePrice);
		marketPriceButton.onClick.AddListener(SetMarketPrice);
		applyButton.onClick.AddListener(ApplyPrice);
		priceField.onEndEdit.AddListener(OnSubmitted);
		priceField.onSubmit.AddListener(OnSubmitted);
		priceField.onValidateInput = OnValidateChar;
		EventManager.AddListener<ProductType, float>(ProductEvents.PRODUCT_PRICE_CHANGED, OnPriceChanged);
		if (data != null)
		{
			float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
			profit.text = data.profit.ToString("0.00", CultureInfo.InvariantCulture);
			priceField.text = "$" + unitPrice.ToString("0.00", CultureInfo.InvariantCulture);
			RefreshApplyButton(unitPrice);
		}
	}

	private void OnPriceChanged(ProductType productType, float price)
	{
		if (productType == data.type)
		{
			SetPrice(price);
		}
	}

	public void Repaint(ProductData data, int index)
	{
		this.data = data;
		float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
		productImage.sprite = data.productSprite;
		productName.text = Locale.GetWord(data.type.ToString());
		productCost.text = data.cost.ToString("0.00", CultureInfo.InvariantCulture);
		marketPrice.text = data.unitMarketPrice.ToString("0.00", CultureInfo.InvariantCulture);
		profit.text = data.profit.ToString("0.00", CultureInfo.InvariantCulture);
		priceField.text = "$" + unitPrice.ToString("0.00", CultureInfo.InvariantCulture);
		if (index % 2 == 0)
		{
			rowBackground.color = new Color(0f, 0f, 0f, 40f / 51f);
		}
		else
		{
			rowBackground.color = new Color(0f, 0f, 0f, 0.5882353f);
		}
		SetPrice(unitPrice);
	}

	private void DecreasePrice()
	{
		float num = ParsePrice(priceField.text);
		num -= 0.01f;
		SetPrice(num);
	}

	private void IncreasePrice()
	{
		float num = ParsePrice(priceField.text);
		num += 0.01f;
		SetPrice(num);
	}

	private void SetPrice(float price)
	{
		if (!(price < 0f))
		{
			priceField.text = "$" + price.ToString("0.00", CultureInfo.InvariantCulture);
			OnSubmitted(priceField.text);
		}
	}

	private void RefreshApplyButton(float price)
	{
		applyButton.interactable = Mathf.Abs(price - SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type)) > PRICE_CHANGE_THRESHOLD;
	}

	public void SetMarketPrice()
	{
		SetPrice(data.unitMarketPrice);
	}

	public void ApplyPrice()
	{
		float num = ParsePrice(priceField.text);
		if (num > data.unitMarketPrice * 1.5f)
		{
			EventLogger.ExcessPrice50();
		}
		SingletonBehaviour<PriceManager>.Instance.SetUnitPrice(data.type, num);
		EventManager.NotifyEvent(ProductEvents.PRODUCT_PRICE_CHANGED, data.type, num);
		RefreshApplyButton(num);
	}

	private void OnSubmitted(string value)
	{
		if (!(data == null))
		{
			float num = ParsePrice(value);
			_ = data.unitMarketPrice;
			SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
			profit.text = "$" + (num - data.cost).ToString("0.00", CultureInfo.InvariantCulture);
			priceField.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
			float scoreOnMarketPrice = SingletonBehaviour<StockManager>.Instance.GetScoreOnMarketPrice(data.type);
			int num2 = (int)(100f * (SingletonBehaviour<StockManager>.Instance.CalculatePurchaseScore(num, data.unitMarketPrice, data.type) - scoreOnMarketPrice) / scoreOnMarketPrice);
			string text = "";
			Color color = Color.green;
			if (num2 > 0)
			{
				text = "+%";
			}
			else if (num2 == 0)
			{
				color = Color.white;
				text = "%";
			}
			else
			{
				color = Color.red;
				text = "-%";
			}
			salesImpact.text = text + Mathf.Abs(num2);
			salesImpact.color = color;
			priceField.SetTextWithoutNotify("$" + num.ToString("0.00", CultureInfo.InvariantCulture));
			RefreshApplyButton(num);
		}
	}

	private float ParsePrice(string text)
	{
		if (float.TryParse(Regex.Replace(text, "[^\\d.,]", "").Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return result;
		}
		return 0f;
	}

	private char OnValidateChar(string text, int charIndex, char added)
	{
		int num = IndexOfSep(text);
		if (num == -1 && text.Length >= 5 && added != '.' && added != ',')
		{
			return '\0';
		}
		if (char.IsDigit(added))
		{
			num = IndexOfSep(text);
			if (num >= 0 && charIndex > num && text.Length - (num + 1) >= 2)
			{
				return '\0';
			}
			return added;
		}
		if (added == '.' || added == ',')
		{
			if (text.Contains('.') || text.Contains(','))
			{
				return '\0';
			}
			return '.';
		}
		return '\0';
	}

	private static int IndexOfSep(string s)
	{
		int num = s.IndexOf('.');
		if (num >= 0)
		{
			return num;
		}
		int num2 = s.IndexOf(',');
		if (num2 >= 0)
		{
			return num2;
		}
		return -1;
	}

	public Selectable GetSelectable()
	{
		return marketPriceButton;
	}

	public Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		AddBottomNavigation(priceField, down);
		AddBottomNavigation(decreasePriceButton, down);
		AddBottomNavigation(increasePriceButton, down);
		AddBottomNavigation(marketPriceButton, down);
		AddBottomNavigation(applyButton, down);
		AddTopNavigation(priceField, up);
		AddTopNavigation(decreasePriceButton, up);
		AddTopNavigation(increasePriceButton, up);
		AddTopNavigation(marketPriceButton, up);
		AddTopNavigation(applyButton, up);
		return marketPriceButton;
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
