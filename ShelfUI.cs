using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShelfUI : SelectableUI
{
	[SerializeField]
	private Image shelfImage;

	[SerializeField]
	private TextMeshProUGUI productName;

	[SerializeField]
	private TextMeshProUGUI unitPrice;

	[SerializeField]
	private TextMeshProUGUI amount;

	[SerializeField]
	private TextMeshProUGUI totalPrice;

	[SerializeField]
	private GameObject limitParent;

	[SerializeField]
	private TextMeshProUGUI limitText;

	[SerializeField]
	private Button addToCartButton;

	[SerializeField]
	private TextMeshProUGUI infoText;

	private int currentAmount = 1;

	private ProductData currentData;

	public ProductType Type
	{
		get
		{
			if (!(currentData == null))
			{
				return currentData.type;
			}
			return ProductType.NONE;
		}
	}

	public void Repaint(ProductData data)
	{
		shelfImage.sprite = data.productSprite;
		currentData = data;
		productName.text = Locale.GetWord(data.productNameKey);
		unitPrice.text = data.cost.ToString("0.00", CultureInfo.InvariantCulture);
		UpdateAmount();
		if (data.maxAmountAllowed != -1)
		{
			int orderedAmount = SingletonBehaviour<OrderManager>.Instance.GetOrderedAmount(data.type);
			limitParent.SetActive(value: true);
			limitText.text = orderedAmount + "/" + data.maxAmountAllowed;
		}
		else
		{
			limitParent.SetActive(value: false);
		}
		if (infoText != null)
		{
			infoText.text = Locale.GetWord(data.type.ToString() + "_info");
		}
	}

	private void Awake()
	{
		EventManager.AddListener<ProductType>(GameEvents.LIMITED_PRODUCT_COUNT_UPDATED, OnLimitedProductOrdered);
	}

	private void OnLimitedProductOrdered(ProductType type)
	{
		if (currentData != null && type == currentData.type)
		{
			int orderedAmount = SingletonBehaviour<OrderManager>.Instance.GetOrderedAmount(type);
			limitText.text = orderedAmount + "/" + currentData.maxAmountAllowed;
		}
	}

	public void OnPlus()
	{
		currentAmount++;
		UpdateAmount();
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
	}

	public void OnMinus()
	{
		if (currentAmount != 1)
		{
			currentAmount--;
			UpdateAmount();
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		}
	}

	public void OnAddToCart()
	{
		EventManager.NotifyEvent(ProductEvents.MULTIPLE_PRODUCTS_ADDED_TO_CART, currentData.type, currentAmount);
	}

	private void UpdateAmount()
	{
		amount.text = currentAmount.ToString();
		totalPrice.text = ((float)currentAmount * currentData.cost).ToString("0.00", CultureInfo.InvariantCulture);
	}

	public override Selectable GetSelectable()
	{
		return addToCartButton;
	}
}
