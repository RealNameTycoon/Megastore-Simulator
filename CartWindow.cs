using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartWindow : UIWindow
{
	[SerializeField]
	private BuyPanel buyPanel;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private List<CartItem> cartItems;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TextMeshProUGUI totalPrice;

	[SerializeField]
	private Button buyButton;

	[SerializeField]
	private Button clearButton;

	[SerializeField]
	private TextMeshProUGUI couponText;

	[SerializeField]
	private Button voucherButton;

	[SerializeField]
	private Image oldPriceBlocker;

	[SerializeField]
	private TextMeshProUGUI discountedPriceText;

	[SerializeField]
	private TextMeshProUGUI voucherAppliedText;

	[SerializeField]
	private TextMeshProUGUI slotsText;

	[SerializeField]
	private TMP_Dropdown deliveryDropdown;

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
	private Button palletOnlyButton;

	[SerializeField]
	private GameObject storeFrontTooltipUI;

	[SerializeField]
	private UIWindow bulkDiscountInfoWindow;

	[SerializeField]
	private Image palletOnlyToggleImage;

	[SerializeField]
	private Sprite mixedImage;

	[SerializeField]
	private Sprite palletImage;

	private bool isPalletOnly = true;

	private int previousCartItemsCount;

	private float cartPrice;

	private int discountAmount;

	private const string FIRTS_VOUCHER_USED_KEY = "FIRTS_VOUCHER_USED_KEY";

	private const string LAST_SELECTED_DELIVERY_AREA_KEY = "LAST_SELECTED_DELIVERY_AREA_KEY";

	private bool isFirstVoucher;

	private int totalBulkDiscountedPallets;

	private bool initialized;

	private OrderManager.OrderReceivingArea selectedDeliveryArea;

	private List<OrderManager.OrderReceivingArea> deliveryOptions = new List<OrderManager.OrderReceivingArea>
	{
		OrderManager.OrderReceivingArea.STORE_FRONT,
		OrderManager.OrderReceivingArea.LOADING_DOCK_1,
		OrderManager.OrderReceivingArea.LOADING_DOCK_2,
		OrderManager.OrderReceivingArea.LOADING_DOCK_3,
		OrderManager.OrderReceivingArea.LOADING_DOCK_4
	};

	public float CartPrice => cartPrice;

	public int TotalBulkDiscountedPallets => totalBulkDiscountedPallets;

	private void Start()
	{
		closeButton.onClick.AddListener(Close);
		buyButton.onClick.AddListener(OnPurchaseProducts);
		isFirstVoucher = !GenericDataSerializer.HasKey("FIRTS_VOUCHER_USED_KEY");
		initialized = true;
		EventManager.AddListener<int>(SupermarketEvents.STORAGE_GROWTH_PURCHASED, OnStorageGrowthPurchased);
		deliveryDropdown.onValueChanged.AddListener(OnDeliveryDropdownValueChanged);
		SetDeliveryDropdownOptions();
		deliveryDropdown.value = GenericDataSerializer.LoadInt("LAST_SELECTED_DELIVERY_AREA_KEY");
		storeFrontTooltipUI.SetActive(selectedDeliveryArea == OrderManager.OrderReceivingArea.STORE_FRONT);
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		palletOnlyButton.onClick.AddListener(OnPalletOnlyButtonClicked);
		palletOnlyToggleImage.sprite = palletImage;
		palletOnlyButton.gameObject.SetActive(selectedDeliveryArea != OrderManager.OrderReceivingArea.STORE_FRONT);
	}

	private void OnPalletOnlyButtonClicked()
	{
		bool flag = false;
		bool flag2 = false;
		foreach (BuyPanel.CartSlot cartSlot in cartSlots)
		{
			if (cartSlot.isPallet)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		bool isPallet = (isPalletOnly = (flag && flag2) || !flag);
		buyPanel.MakeAllPallets(isPallet);
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
		int value = deliveryDropdown.value;
		deliveryDropdown.ClearOptions();
		for (int i = 0; i < deliveryOptions.Count; i++)
		{
			if (SingletonBehaviour<StorageManager>.Instance.IsDockUnlocked(deliveryOptions[i]))
			{
				deliveryDropdown.options.Add(new TMP_Dropdown.OptionData(Locale.GetWord(deliveryOptions[i].ToString())));
			}
		}
		deliveryDropdown.value = value;
		deliveryDropdown.RefreshShownValue();
	}

	private void OnDeliveryDropdownValueChanged(int value)
	{
		selectedDeliveryArea = deliveryOptions[value];
		GenericDataSerializer.SaveInt("LAST_SELECTED_DELIVERY_AREA_KEY", value);
		storeFrontTooltipUI.SetActive(selectedDeliveryArea == OrderManager.OrderReceivingArea.STORE_FRONT);
		palletOnlyButton.gameObject.SetActive(selectedDeliveryArea != OrderManager.OrderReceivingArea.STORE_FRONT);
		Repaint(cartSlots);
	}

	public void Repaint(List<BuyPanel.CartSlot> items)
	{
		if (!initialized)
		{
			isFirstVoucher = !GenericDataSerializer.HasKey("FIRTS_VOUCHER_USED_KEY");
			initialized = true;
		}
		cartSlots = items;
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		float num4 = 0f;
		totalBulkDiscountedPallets = 0;
		bool flag = false;
		bool flag2 = false;
		foreach (BuyPanel.CartSlot item in items)
		{
			num += SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(item.type).TotalCost() * (float)item.amount;
			if (item.isPallet)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
			if (num2 >= cartItems.Count)
			{
				cartItems.Add(Object.Instantiate(cartItems[0], cartItems[0].transform.parent, worldPositionStays: true));
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
				totalBulkDiscountedPallets++;
				ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(item.type);
				if (OrderManager.bulkDiscounts.ContainsKey(anyProductData.productGroup))
				{
					num4 += anyProductData.TotalCost() * (float)item.amount * ((float)OrderManager.bulkDiscounts[anyProductData.productGroup] / 100f);
				}
			}
			num2++;
		}
		if (flag && flag2)
		{
			palletOnlyToggleImage.enabled = true;
			palletOnlyToggleImage.sprite = mixedImage;
		}
		else if (flag)
		{
			palletOnlyToggleImage.enabled = true;
			palletOnlyToggleImage.sprite = palletImage;
		}
		else
		{
			palletOnlyToggleImage.enabled = false;
		}
		float num5 = PalletManager.PALLET_DEPOSIT_AMOUNT * (float)num3;
		cartPrice = num + num5 - num4;
		totalPrice.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
		subtotalText.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
		palletDepositText.text = num3 + " x $" + PalletManager.PALLET_DEPOSIT_AMOUNT;
		bulkDiscountText.text = "$" + num4.ToString("0.00", CultureInfo.InvariantCulture);
		grandTotalText.text = "$" + cartPrice.ToString("0.00", CultureInfo.InvariantCulture);
		couponText.text = Locale.GetWord("apply_voucher").Replace("{0}", GetCouponAmount().ToString());
		if (discountAmount != 0)
		{
			oldPriceBlocker.enabled = true;
			voucherAppliedText.enabled = true;
			float num6 = (1f - (float)discountAmount / 100f) * num;
			discountedPriceText.text = "$" + num6.ToString("0.00", CultureInfo.InvariantCulture);
			discountedPriceText.enabled = true;
			voucherButton.gameObject.SetActive(value: false);
		}
		for (int i = num2; i < cartItems.Count; i++)
		{
			if (cartItems[i].gameObject.activeSelf)
			{
				cartItems[i].gameObject.SetActive(value: false);
			}
		}
		if (previousCartItemsCount != items.Count)
		{
			SelectBottomCartItem();
		}
		previousCartItemsCount = cartSlots.Count;
		RefreshNavigation();
	}

	private void SelectBottomCartItem()
	{
		if (base.gameObject.activeSelf)
		{
			int num = -1;
			int num2 = 0;
			if (num2 < cartItems.Count && cartItems[num2].gameObject.activeSelf)
			{
				num = num2;
			}
			Selectable selectable = ((num == -1) ? buyButton : cartItems[num].GetSelectable());
			SingletonBehaviour<InputManager>.Instance.SelectElement(selectable.gameObject);
		}
	}

	private void RefreshNavigation()
	{
		List<CartItem> list = new List<CartItem>();
		for (int i = 0; i < cartItems.Count; i++)
		{
			if (cartItems[i].gameObject.activeSelf)
			{
				list.Add(cartItems[i]);
			}
		}
		bool num = list.Count > 0 && list[0].gameObject.activeSelf;
		Navigation navigation = clearButton.navigation;
		Navigation navigation2 = palletOnlyButton.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation2.mode = Navigation.Mode.Explicit;
		if (num)
		{
			navigation.selectOnDown = list[0].GetSelectable();
			navigation2.selectOnDown = list[0].GetSelectable();
		}
		else
		{
			navigation.selectOnDown = null;
			navigation2.selectOnDown = null;
		}
		clearButton.navigation = navigation;
		palletOnlyButton.navigation = navigation2;
		for (int j = 0; j < list.Count; j++)
		{
			CartItem cartItem = list[j];
			if (j == 0)
			{
				Selectable down = ((list.Count > j + 1) ? list[j + 1].GetSelectable() : buyButton);
				cartItem.RefreshNavigation(clearButton, down, buyButton, palletOnlyButton);
				if (list.Count != 1)
				{
				}
			}
			else if (j == list.Count - 1)
			{
				cartItem.RefreshNavigation(list[j - 1].GetSelectable(), null, buyButton);
			}
			else
			{
				cartItem.RefreshNavigation(list[j - 1].GetSelectable(), list[j + 1].GetSelectable(), buyButton);
			}
		}
	}

	private void RefreshUpperNavigation(Selectable upperSelectable)
	{
		Navigation navigation = buyButton.navigation;
		navigation.selectOnUp = upperSelectable;
		buyButton.navigation = navigation;
		Navigation navigation2 = clearButton.navigation;
		navigation2.selectOnUp = upperSelectable;
		clearButton.navigation = navigation2;
		Navigation navigation3 = deliveryDropdown.navigation;
		navigation3.selectOnUp = upperSelectable;
		deliveryDropdown.navigation = navigation3;
	}

	public void OnVoucherClicked()
	{
		AdManager.Instance.ShowRewarded(delegate
		{
			discountAmount = GetCouponAmount();
			Repaint(cartSlots);
			EventLogger.LogEvent("c_rewarded_voucher");
		});
	}

	private int GetCouponAmount()
	{
		if (isFirstVoucher)
		{
			return 25;
		}
		return 10;
	}

	public new void Open()
	{
		base.gameObject.SetActive(value: true);
		base.Open();
		EventManager.NotifyEvent(UIEvents.CART_WINDOW_OPENED);
		canvasGroup.DOKill();
		canvasGroup.DOFade(1f, 0.2f);
	}

	public new void Close()
	{
		if (bulkDiscountInfoWindow.IsOpen())
		{
			bulkDiscountInfoWindow.Close();
		}
		base.Close();
		canvasGroup.DOKill();
		canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private void OnPurchaseProducts()
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
			if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(cartPrice * (1f - (float)discountAmount / 100f)))
			{
				EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, cartPrice * (1f - (float)discountAmount / 100f));
				EventManager.NotifyEvent(PaymentEvents.CART_PUCHASED, cartPrice * (1f - (float)discountAmount / 100f));
				SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_APPEAR);
				SingletonBehaviour<OrderManager>.Instance.AddOrder(cartSlots, selectedDeliveryArea);
				buyPanel.OnClear();
				Close();
				if (selectedDeliveryArea == OrderManager.OrderReceivingArea.STORE_FRONT)
				{
					Truck truck = SingletonBehaviour<TruckManager>.Instance.GetTruck(selectedDeliveryArea);
					if (truck.IsActive && truck.IsDoorEnabled)
					{
						SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltipWithFullText(Locale.GetWord("truck_active_warning"), base.transform);
					}
				}
				if (discountAmount != 0)
				{
					if (isFirstVoucher)
					{
						isFirstVoucher = false;
						GenericDataSerializer.SaveBool("FIRTS_VOUCHER_USED_KEY", value: true);
					}
					discountAmount = 0;
					oldPriceBlocker.enabled = false;
					voucherAppliedText.enabled = false;
					discountedPriceText.enabled = false;
					couponText.text = Locale.GetWord("apply_voucher").Replace("{}", GetCouponAmount().ToString());
				}
			}
			else
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_cash", base.transform);
			}
		}
	}

	private void Update()
	{
	}
}
