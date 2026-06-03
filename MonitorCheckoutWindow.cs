using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonitorCheckoutWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject cardWindow;

	[SerializeField]
	private GameObject cardWindowContentParent;

	[SerializeField]
	private GameObject CashPaymentContentParent;

	[SerializeField]
	private GameObject totalPriceParent;

	[SerializeField]
	private TextMeshProUGUI totalValueText1;

	[SerializeField]
	private TextMeshProUGUI totalValueText2;

	[SerializeField]
	private TextMeshProUGUI totalValueTextCard;

	[SerializeField]
	private TextMeshProUGUI receivedCashValue;

	[SerializeField]
	private TextMeshProUGUI changeValue;

	[SerializeField]
	private TextMeshProUGUI givenValueText;

	[SerializeField]
	private List<GameObject> productInfoParents;

	[SerializeField]
	private List<Image> productImages;

	[SerializeField]
	private List<TextMeshProUGUI> productNames;

	[SerializeField]
	private List<TextMeshProUGUI> productQuantities;

	[SerializeField]
	private List<TextMeshProUGUI> productPrices;

	[SerializeField]
	private GameObject outOfServiceWindow;

	private float totalValue;

	private float receivedValue;

	private void Start()
	{
		EventManager.AddListener(SupermarketEvents.FIRE_STARTED, delegate
		{
			if (!totalPriceParent.activeSelf)
			{
				totalPriceParent.gameObject.SetActive(value: true);
			}
			cardWindow.SetActive(value: false);
			CashPaymentContentParent.transform.localScale = Vector3.zero;
			totalValue = 0f;
			RepaintTotalValue(totalValue, null);
		});
	}

	public void RepaintTotalValue(float totalValue, List<Product> productTypes)
	{
		this.totalValue = totalValue;
		totalValueText1.text = "$" + totalValue.ToString("0.00", CultureInfo.InvariantCulture);
		totalValueTextCard.text = "$" + totalValue.ToString("0.00", CultureInfo.InvariantCulture);
		if (productTypes == null)
		{
			for (int i = 0; i < productInfoParents.Count; i++)
			{
				if (productInfoParents[i].activeSelf)
				{
					productInfoParents[i].SetActive(value: false);
				}
			}
			return;
		}
		Dictionary<ProductType, (int, float)> dictionary = new Dictionary<ProductType, (int, float)>();
		for (int j = 0; j < productTypes.Count; j++)
		{
			Product product = productTypes[j];
			ProductType productType = product.Data.type;
			int num = 1;
			if (productType == ProductType.COOKABLE_PACK)
			{
				productType = (product as CookablePackageProduct).ContainedType;
			}
			if (productType == ProductType.FOOD_TRAY)
			{
				FoodTrayProduct obj = product as FoodTrayProduct;
				productType = obj.ContainedType;
				num = obj.ProductCount;
			}
			if (!dictionary.ContainsKey(productType))
			{
				dictionary[productType] = (num, product.TempPrice);
			}
			else
			{
				dictionary[productType] = (dictionary[productType].Item1 + num, dictionary[productType].Item2 + product.TempPrice);
			}
		}
		List<ProductType> list = dictionary.Keys.ToList();
		list.Reverse();
		for (int k = 0; k < productInfoParents.Count; k++)
		{
			if (k > list.Count - 1)
			{
				productInfoParents[k].SetActive(value: false);
				continue;
			}
			ProductType productType2 = list[k];
			productInfoParents[k].SetActive(value: true);
			productQuantities[k].text = dictionary[productType2].Item1.ToString();
			productNames[k].text = Locale.GetWord(productType2.ToString());
			productImages[k].sprite = SingletonBehaviour<ProductPool>.Instance.GetProductData(productType2).productSprite;
			productPrices[k].text = "$" + dictionary[productType2].Item2.ToString("0.00", CultureInfo.InvariantCulture);
		}
	}

	public void RepaintForCash(float receivedValue)
	{
		this.receivedValue = receivedValue;
		totalValueText2.text = "$" + totalValue.ToString("0.00", CultureInfo.InvariantCulture);
		receivedCashValue.text = "$" + receivedValue.ToString("0.00", CultureInfo.InvariantCulture);
		changeValue.text = (receivedValue - totalValue).ToString("0.00", CultureInfo.InvariantCulture);
		givenValueText.text = "0.00";
		givenValueText.color = ((receivedValue - totalValue < 0.005f) ? Color.green : Color.red);
		CashPaymentContentParent.transform.DOScale(Vector3.one, 0.2f);
	}

	public void RepaintForCard()
	{
		totalPriceParent.gameObject.SetActive(value: false);
		cardWindow.SetActive(value: true);
		cardWindowContentParent.transform.DOScale(Vector3.one, 0.2f);
	}

	public void OnPaymentCompleted(float payment)
	{
		if (!totalPriceParent.activeSelf)
		{
			totalPriceParent.gameObject.SetActive(value: true);
		}
		cardWindow.SetActive(value: false);
		CashPaymentContentParent.transform.localScale = Vector3.zero;
		cardWindowContentParent.transform.localScale = Vector3.zero;
		totalValue = 0f;
		RepaintTotalValue(totalValue, null);
	}

	public void SetGivenCash(float givenAmount)
	{
		if ((double)Mathf.Abs(givenAmount - (receivedValue - totalValue)) < 0.005)
		{
			givenValueText.color = Color.green;
		}
		else
		{
			givenValueText.color = Color.red;
		}
		givenValueText.text = givenAmount.ToString("0.00", CultureInfo.InvariantCulture);
	}

	public void SetClosed(bool state)
	{
		if (outOfServiceWindow != null)
		{
			outOfServiceWindow.SetActive(state);
		}
	}

	private void Update()
	{
	}
}
