using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceWindow : SingletonWindow<PriceWindow>
{
	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	private Image productImage;

	[SerializeField]
	private TextMeshProUGUI productNameText;

	[SerializeField]
	private TextMeshProUGUI costLabelAndValueText;

	[SerializeField]
	private TextMeshProUGUI priceValueText;

	[SerializeField]
	private TextMeshProUGUI priceValueTextSlider;

	[SerializeField]
	private TextMeshProUGUI marketPriceLabelAndValueText;

	[SerializeField]
	private TextMeshProUGUI salesImpactValue;

	[SerializeField]
	private TextMeshProUGUI profitValueText;

	[SerializeField]
	private TextMeshProUGUI hardCurrencyPriceText;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Button decreasePriceButton;

	[SerializeField]
	private Button increasePriceButton;

	[SerializeField]
	private Button marketPriceButton;

	[SerializeField]
	private Button aboveMarketPriceButton;

	[SerializeField]
	private Button belowMarketPriceButton;

	[SerializeField]
	private Button okButton;

	[SerializeField]
	private TMP_InputField priceField;

	private const string spriteIndexPrefix = "<sprite index=0> ";

	private ProductData data;

	private int hardCurrencyPrice;

	public ProductData Data => data;

	private void Start()
	{
		decreasePriceButton.onClick.AddListener(DecreasePrice);
		increasePriceButton.onClick.AddListener(IncreasePrice);
		okButton.onClick.AddListener(OnOk);
		slider.onValueChanged.AddListener(delegate
		{
			UpdateTextsToSlider();
		});
		priceField.onEndEdit.AddListener(OnSubmitted);
		priceField.onSubmit.AddListener(OnSubmitted);
		priceField.onValidateInput = OnValidateChar;
		marketPriceButton.onClick.AddListener(delegate
		{
			if (!(data == null))
			{
				SetPrice(data.unitMarketPrice);
			}
		});
		aboveMarketPriceButton.onClick.AddListener(delegate
		{
			if (!(data == null))
			{
				SetPrice(data.unitMarketPrice * 1.1f);
			}
		});
		belowMarketPriceButton.onClick.AddListener(delegate
		{
			if (!(data == null))
			{
				SetPrice(data.unitMarketPrice * 0.9f);
			}
		});
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

	private void OnOk()
	{
		Close();
		raycaster.enabled = false;
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
		if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(7))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 7);
		}
		float num = 0f;
		if (priceField.gameObject.activeSelf)
		{
			num = ParsePrice(priceField.text);
		}
		else if (slider.gameObject.activeSelf)
		{
			num = GetTruncatedSliderValue();
		}
		if (num > data.unitMarketPrice * 1.5f)
		{
			EventLogger.ExcessPrice50();
		}
		SingletonBehaviour<PriceManager>.Instance.SetUnitPrice(data.type, num);
		EventManager.NotifyEvent(ProductEvents.PRODUCT_PRICE_CHANGED, data.type, num);
	}

	public void Open(ProductData data)
	{
		if (!IsOpen())
		{
			if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(6))
			{
				EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 6);
			}
			this.data = data;
			float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
			float unitMarketPrice = data.unitMarketPrice;
			hardCurrencyPrice = 0;
			hardCurrencyPriceText.text = "<sprite index=0> " + hardCurrencyPrice;
			productImage.sprite = data.productSprite;
			productNameText.text = Locale.GetWord(data.type.ToString());
			costLabelAndValueText.text = Locale.GetWord("cost").Replace("{0}", data.cost.ToString());
			marketPriceLabelAndValueText.text = Locale.GetWord("market_price").Replace("{0}", unitMarketPrice.ToString());
			if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
			{
				slider.gameObject.SetActive(value: true);
				priceValueTextSlider.gameObject.SetActive(value: true);
				priceField.gameObject.SetActive(value: false);
				slider.maxValue = unitMarketPrice * 1.66f;
				slider.minValue = 0.01f;
				slider.value = unitPrice;
				UpdateTextsToSlider();
			}
			else
			{
				slider.gameObject.SetActive(value: false);
				priceValueTextSlider.gameObject.SetActive(value: false);
				priceField.gameObject.SetActive(value: true);
				priceField.text = "$" + unitPrice.ToString("0.00", CultureInfo.InvariantCulture);
				OnSubmitted(priceField.text);
			}
			Open();
			raycaster.enabled = true;
			SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
		}
	}

	private void SetPrice(float price)
	{
		if (!(price < 0f))
		{
			if (priceField.gameObject.activeSelf)
			{
				priceField.text = "$" + price.ToString("0.00", CultureInfo.InvariantCulture);
				OnSubmitted(priceField.text);
			}
			if (slider.gameObject.activeSelf)
			{
				slider.value = price;
				UpdateTextsToSlider();
			}
		}
	}

	private void UpdateTextsToSlider()
	{
		if (!(data == null))
		{
			float num = data.unitMarketPrice * 1.01f;
			float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
			float num2 = ((num > unitPrice) ? num : unitPrice);
			float num3 = slider.value - num2;
			if (num3 > 0f && (double)slider.value > (double)data.unitMarketPrice * 1.01)
			{
				int num4 = (int)(num3 / 0.5f) + 1;
				hardCurrencyPrice = num4;
				hardCurrencyPriceText.text = "<sprite index=0> " + hardCurrencyPrice;
			}
			else
			{
				hardCurrencyPrice = 0;
				hardCurrencyPriceText.text = "<sprite index=0> " + hardCurrencyPrice;
			}
			float truncatedSliderValue = GetTruncatedSliderValue();
			profitValueText.text = "$" + (truncatedSliderValue - data.cost).ToString("0.00", CultureInfo.InvariantCulture);
			priceValueTextSlider.text = "$" + truncatedSliderValue.ToString("0.00", CultureInfo.InvariantCulture);
			float scoreOnMarketPrice = SingletonBehaviour<StockManager>.Instance.GetScoreOnMarketPrice(data.type);
			int num5 = (int)(100f * (SingletonBehaviour<StockManager>.Instance.CalculatePurchaseScore(truncatedSliderValue, data.unitMarketPrice, data.type) - scoreOnMarketPrice) / scoreOnMarketPrice);
			string text = "";
			Color color = Color.green;
			if (num5 > 0)
			{
				text = "+%";
			}
			else if (num5 == 0)
			{
				color = Color.white;
				text = "%";
			}
			else
			{
				color = Color.red;
				text = "-%";
			}
			salesImpactValue.text = text + Mathf.Abs(num5);
			salesImpactValue.color = color;
		}
	}

	private void OnSubmitted(string value)
	{
		if (!(data == null))
		{
			float num = ParsePrice(value);
			_ = data.unitMarketPrice;
			SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
			profitValueText.text = "$" + (num - data.cost).ToString("0.00", CultureInfo.InvariantCulture);
			priceValueText.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
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
			salesImpactValue.text = text + Mathf.Abs(num2);
			salesImpactValue.color = color;
			priceField.SetTextWithoutNotify("$" + num.ToString("0.00", CultureInfo.InvariantCulture));
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

	private float GetTruncatedSliderValue()
	{
		return (float)Math.Truncate(slider.value * 100f) / 100f;
	}
}
