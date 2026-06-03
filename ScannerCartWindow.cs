using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScannerCartWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private List<CartItem> cartItems;

	[SerializeField]
	private TextMeshProUGUI totalPrice;

	[SerializeField]
	private TextMeshProUGUI deliveryPointText;

	[SerializeField]
	private RectTransform openedPosition;

	[SerializeField]
	private RectTransform closedPosition;

	[SerializeField]
	private BuyPanel buyPanel;

	private Dictionary<ProductData, int> data;

	private List<BuyPanel.CartSlot> cartSlots = new List<BuyPanel.CartSlot>();

	[SerializeField]
	private TextMeshProUGUI subtotalText;

	[SerializeField]
	private TextMeshProUGUI palletDepositText;

	[SerializeField]
	private TextMeshProUGUI bulkDiscountText;

	[SerializeField]
	private TextMeshProUGUI grandTotalText;

	[SerializeField]
	private GameObject storeFrontTooltipUI;

	private float cartPrice;

	private bool initialized;

	private int selectedDeliveryAreaIndex;

	private const string SCANNER_LAST_SELECTED_DELIVERY_AREA_KEY = "SCANNER_LAST_SELECTED_DELIVERY_AREA_KEY";

	private List<OrderManager.OrderReceivingArea> availableDeliveryAreas = new List<OrderManager.OrderReceivingArea>();

	private OrderManager.OrderReceivingArea selectedDeliveryArea;

	private List<OrderManager.OrderReceivingArea> deliveryOptions = new List<OrderManager.OrderReceivingArea>
	{
		OrderManager.OrderReceivingArea.STORE_FRONT,
		OrderManager.OrderReceivingArea.LOADING_DOCK_1,
		OrderManager.OrderReceivingArea.LOADING_DOCK_2,
		OrderManager.OrderReceivingArea.LOADING_DOCK_3,
		OrderManager.OrderReceivingArea.LOADING_DOCK_4
	};

	private void Start()
	{
		initialized = true;
		EventManager.AddListener<int>(SupermarketEvents.STORAGE_GROWTH_PURCHASED, OnStorageGrowthPurchased);
		SetDeliveryDropdownOptions();
		RepaintCurrentDeliveryArea();
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		SetDeliveryDropdownOptions();
	}

	private void OnStorageGrowthPurchased(int growthLevel)
	{
		SetDeliveryDropdownOptions();
	}

	private void SetDeliveryDropdownOptions()
	{
		availableDeliveryAreas.Clear();
		for (int i = 0; i < deliveryOptions.Count; i++)
		{
			if (SingletonBehaviour<StorageManager>.Instance.IsDockUnlocked(deliveryOptions[i]))
			{
				availableDeliveryAreas.Add(deliveryOptions[i]);
			}
		}
		selectedDeliveryAreaIndex = GenericDataSerializer.LoadInt("SCANNER_LAST_SELECTED_DELIVERY_AREA_KEY");
		selectedDeliveryAreaIndex %= availableDeliveryAreas.Count;
		selectedDeliveryArea = availableDeliveryAreas[selectedDeliveryAreaIndex];
	}

	public void NextDeliveryArea()
	{
		selectedDeliveryAreaIndex++;
		selectedDeliveryAreaIndex %= availableDeliveryAreas.Count;
		selectedDeliveryArea = availableDeliveryAreas[selectedDeliveryAreaIndex];
		GenericDataSerializer.SaveInt("SCANNER_LAST_SELECTED_DELIVERY_AREA_KEY", selectedDeliveryAreaIndex);
		RepaintCurrentDeliveryArea();
		Repaint(cartSlots);
	}

	private void RepaintCurrentDeliveryArea()
	{
		deliveryPointText.text = Locale.GetWord(selectedDeliveryArea.ToString());
	}

	public void Repaint(List<BuyPanel.CartSlot> items)
	{
		if (!initialized)
		{
			initialized = true;
		}
		cartSlots = items;
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		float num4 = 0f;
		foreach (BuyPanel.CartSlot item in items)
		{
			num += SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(item.type).TotalCost() * (float)item.amount;
			if (num2 >= cartItems.Count)
			{
				cartItems.Add(Object.Instantiate(cartItems[0]));
				cartItems[num2].transform.SetParent(cartItems[0].transform.parent);
				cartItems[num2].transform.localScale = Vector3.one;
			}
			cartItems[num2].Repaint(item.type, item.amount, num2, item.isPallet, selectedDeliveryArea);
			if (!cartItems[num2].gameObject.activeSelf)
			{
				cartItems[num2].gameObject.SetActive(value: true);
			}
			if (item.isPallet && selectedDeliveryArea != OrderManager.OrderReceivingArea.STORE_FRONT)
			{
				num3++;
			}
			if (item.amount == SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(item.type))
			{
				ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(item.type);
				if (OrderManager.bulkDiscounts.ContainsKey(anyProductData.productGroup))
				{
					num4 += anyProductData.TotalCost() * (float)item.amount * ((float)OrderManager.bulkDiscounts[anyProductData.productGroup] / 100f);
				}
			}
			num2++;
		}
		float num5 = PalletManager.PALLET_DEPOSIT_AMOUNT * (float)num3;
		cartPrice = num + num5 - num4;
		totalPrice.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
		subtotalText.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
		palletDepositText.text = num3 + " x $" + PalletManager.PALLET_DEPOSIT_AMOUNT;
		bulkDiscountText.text = "$" + num4.ToString("0.00", CultureInfo.InvariantCulture);
		grandTotalText.text = "$" + cartPrice.ToString("0.00", CultureInfo.InvariantCulture);
		for (int i = num2; i < cartItems.Count; i++)
		{
			if (cartItems[i].gameObject.activeSelf)
			{
				cartItems[i].gameObject.SetActive(value: false);
			}
		}
		storeFrontTooltipUI.SetActive(selectedDeliveryArea == OrderManager.OrderReceivingArea.STORE_FRONT);
	}

	public void Open()
	{
		canvas.enabled = true;
		base.transform.DOKill();
		base.transform.DOMove(openedPosition.position, 0.2f).SetEase(Ease.OutSine);
		Repaint(cartSlots);
	}

	public void Close()
	{
		base.transform.DOKill();
		base.transform.DOMove(closedPosition.position, 0.2f).SetEase(Ease.OutSine).OnComplete(delegate
		{
			canvas.enabled = false;
		});
	}

	public bool IsOpen()
	{
		return canvas.enabled;
	}

	public void OnPurchaseProducts()
	{
		if (cartSlots.Count == 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("no_products_in_cart", base.transform);
		}
		else
		{
			if (!SingletonBehaviour<OrderManager>.Instance.CanAddOrder(cartSlots))
			{
				return;
			}
			if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(cartPrice))
			{
				EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, cartPrice);
				EventManager.NotifyEvent(PaymentEvents.CART_PUCHASED, cartPrice);
				SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_APPEAR);
				SingletonBehaviour<OrderManager>.Instance.AddOrder(cartSlots, selectedDeliveryArea);
				buyPanel.OnClearScannerCart();
				if (selectedDeliveryArea == OrderManager.OrderReceivingArea.STORE_FRONT)
				{
					Truck truck = SingletonBehaviour<TruckManager>.Instance.GetTruck(selectedDeliveryArea);
					if (truck.IsActive && truck.IsDoorEnabled)
					{
						SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltipWithFullText(Locale.GetWord("truck_active_warning"), base.transform);
					}
				}
			}
			else
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_cash", base.transform);
			}
		}
	}

	public void OnClearScannerCart()
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.HANDSCANNER_SWITCH);
		buyPanel.OnClearScannerCart();
	}

	private void Update()
	{
	}
}
