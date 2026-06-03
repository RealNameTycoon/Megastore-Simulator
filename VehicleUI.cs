using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleUI : SelectableUI
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI requiredStoreText;

	[SerializeField]
	private GameObject priceSection;

	[SerializeField]
	private GameObject InfoSection;

	[SerializeField]
	private TextMeshProUGUI price;

	[SerializeField]
	private TextMeshProUGUI pricePlaceholderText;

	[SerializeField]
	private Button purchase;

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Button wishlistUI;

	[SerializeField]
	private TextMeshProUGUI owned;

	[SerializeField]
	private TextMeshProUGUI vehicleInfoText;

	[SerializeField]
	private VehicleType vehicleType;

	[SerializeField]
	private Button sellButton;

	[SerializeField]
	private TextMeshProUGUI sellPriceText;

	private const string VEHICLE_INFO_KEY = "vehicle_info_";

	private ComputerPopup computerPopup;

	public VehicleType VehicleType => vehicleType;

	public void Initialize(ComputerPopup computerPopup)
	{
		this.computerPopup = computerPopup;
		title.text = Locale.GetWord(vehicleType.ToString());
		bool flag = SingletonBehaviour<VehicleManager>.Instance.IsVehiclePurchased(vehicleType, 0);
		int vehicleRequiredLevel = SingletonBehaviour<VehicleManager>.Instance.GetVehicleRequiredLevel(vehicleType);
		if (GameManager.isDemo && vehicleType != VehicleType.HAND_TRUCK)
		{
			wishlistUI.gameObject.SetActive(value: true);
			wishlistUI.onClick.AddListener(OnWishlist);
			requiredStoreText.enabled = false;
			vehicleInfoText.text = Locale.GetWord("vehicle_info_" + vehicleType);
			return;
		}
		if (vehicleRequiredLevel == 0 || vehicleType == VehicleType.HAND_TRUCK)
		{
			requiredStoreText.enabled = false;
		}
		else
		{
			requiredStoreText.text = Locale.GetWord("required_warehouse_expansion_n").Replace("{0}", vehicleRequiredLevel.ToString());
		}
		price.text = "$" + SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType);
		price.enabled = !flag;
		pricePlaceholderText.enabled = false;
		vehicleInfoText.text = Locale.GetWord("vehicle_info_" + vehicleType);
		requiredStoreText.enabled = !flag;
		purchase.gameObject.SetActive(!flag);
		sellButton.gameObject.SetActive(flag);
		sellPriceText.text = Locale.GetWord("sell_n").Replace("{0}", "$" + ((float)SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType) / 2f).ToString("0.00", CultureInfo.InvariantCulture));
		purchase.onClick.AddListener(OnPurchase);
		sellButton.onClick.AddListener(OnSell);
		resetButton.gameObject.SetActive(flag);
		resetButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<VehicleManager>.Instance.ResetVehicle(vehicleType, 0);
		});
		RepaintForExpansionLevel(SingletonBehaviour<StorageManager>.Instance.GrowthCount);
		if (vehicleType == VehicleType.PALLET_ROBOT)
		{
			priceSection.gameObject.SetActive(value: false);
			InfoSection.gameObject.SetActive(value: true);
			pricePlaceholderText.enabled = true;
			pricePlaceholderText.text = Locale.GetWord("coming_soon");
		}
		EventManager.AddListener<int>(SupermarketEvents.STORAGE_GROWTH_PURCHASED, RepaintForExpansionLevel);
	}

	private void OnWishlist()
	{
		Application.OpenURL("steam://store/3819640");
	}

	private void RepaintForExpansionLevel(int expansionLevel)
	{
		bool flag = expansionLevel >= SingletonBehaviour<VehicleManager>.Instance.GetVehicleRequiredLevel(vehicleType);
		requiredStoreText.color = (flag ? UIManager.GreenColor : UIManager.RedColor);
		purchase.interactable = flag;
	}

	private void OnPurchase()
	{
		if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType)))
		{
			PurchaseVehicle(updateNavigation: true);
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_cash", base.transform);
		}
	}

	public void PurchaseVehicle(bool updateNavigation = false)
	{
		EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType));
		SingletonBehaviour<VehicleManager>.Instance.PurchaseVehicle(vehicleType, 0);
		requiredStoreText.enabled = false;
		if (updateNavigation && SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
		{
			_ = purchase.navigation;
			Selectable availableSelectable = GetAvailableSelectable();
			SingletonBehaviour<InputManager>.Instance.SelectElement(availableSelectable.gameObject);
		}
		purchase.gameObject.SetActive(value: false);
		sellButton.gameObject.SetActive(value: true);
		price.enabled = false;
		resetButton.gameObject.SetActive(value: true);
		EventManager.NotifyEvent(UIEvents.TAB_SELECTED_OBJECT_CHANGED);
		OrderManager.OrderReceivingArea mostAvailableOrderReceivingArea = SingletonBehaviour<OrderManager>.Instance.GetMostAvailableOrderReceivingArea();
		TooltipUI instance = SingletonBehaviour<TooltipUI>.Instance;
		string word = Locale.GetWord("vehicle_delivery_dock_n");
		int num = (int)mostAvailableOrderReceivingArea;
		instance.ShowTimedTooltipWithFullText(word.Replace("{0}", num.ToString()), base.transform);
		SingletonBehaviour<OrderManager>.Instance.AddOrder(new List<BuyPanel.CartSlot>
		{
			new BuyPanel.CartSlot(SingletonBehaviour<VehicleManager>.Instance.GetDeliveryProduct(vehicleType), 1)
		}, mostAvailableOrderReceivingArea);
	}

	private void OnSell()
	{
		if (SingletonBehaviour<FinanceManager>.Instance.HasVehicleLoan(vehicleType))
		{
			computerPopup.Open(Locale.GetWord("active_loan_title"), Locale.GetWord("active_loan_desc"));
			return;
		}
		string description = Locale.GetWord("sell_vehicle_confirmation_n").Replace("{0}", "$" + ((float)SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType) / 2f).ToString("0.00", CultureInfo.InvariantCulture));
		computerPopup.Open(Locale.GetWord("confirm_sale"), description, delegate
		{
			OnSellConfirmed();
		});
	}

	private void OnSellConfirmed()
	{
		computerPopup.Close();
		if (SingletonBehaviour<VehicleManager>.Instance.CanSellVehicle(vehicleType, 0))
		{
			EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, (float)(SingletonBehaviour<VehicleManager>.Instance.GetVehiclePrice(vehicleType) / 2));
			SingletonBehaviour<VehicleManager>.Instance.SellVehicle(vehicleType, 0);
			requiredStoreText.enabled = true;
			if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
			{
				SingletonBehaviour<InputManager>.Instance.SelectElement(purchase.gameObject);
			}
			purchase.gameObject.SetActive(value: true);
			price.enabled = true;
			sellButton.gameObject.SetActive(value: false);
			resetButton.gameObject.SetActive(value: false);
			EventManager.NotifyEvent(GameEvents.VEHICLE_SOLD, vehicleType);
		}
	}

	public override Selectable GetSelectable()
	{
		if (resetButton.gameObject.activeSelf)
		{
			return resetButton;
		}
		if (purchase.gameObject.activeSelf)
		{
			return purchase;
		}
		return null;
	}

	public override Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		Navigation navigation = sellButton.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnLeft = left;
		navigation.selectOnRight = resetButton;
		navigation.selectOnUp = up;
		navigation.selectOnDown = down;
		sellButton.navigation = navigation;
		Navigation navigation2 = resetButton.navigation;
		navigation2.mode = Navigation.Mode.Explicit;
		navigation2.selectOnLeft = sellButton;
		navigation2.selectOnRight = right;
		navigation2.selectOnUp = up;
		navigation2.selectOnDown = down;
		resetButton.navigation = navigation2;
		Navigation navigation3 = purchase.navigation;
		navigation3.mode = Navigation.Mode.Explicit;
		navigation3.selectOnLeft = left;
		navigation3.selectOnRight = right;
		navigation3.selectOnUp = up;
		navigation3.selectOnDown = down;
		purchase.navigation = navigation3;
		return GetSelectable();
	}
}
