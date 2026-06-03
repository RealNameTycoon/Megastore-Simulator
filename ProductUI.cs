using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductUI : SelectableUI
{
	[SerializeField]
	private Image productImage;

	[SerializeField]
	private GameObject unlockedParent;

	[SerializeField]
	private TextMeshProUGUI productName;

	[SerializeField]
	private TextMeshProUGUI productBrand;

	[SerializeField]
	private TextMeshProUGUI countInBox;

	[SerializeField]
	private TextMeshProUGUI storageType;

	[SerializeField]
	private TextMeshProUGUI unitPrice;

	[SerializeField]
	private TextMeshProUGUI amount;

	[SerializeField]
	private TextMeshProUGUI totalPrice;

	[SerializeField]
	private GameObject lockedParent;

	[SerializeField]
	private TextMeshProUGUI licenseRequired;

	[SerializeField]
	private TextMeshProUGUI onShelvesValue;

	[SerializeField]
	private TextMeshProUGUI offShelvesValue;

	[SerializeField]
	private Button addToCartButton;

	[SerializeField]
	private Button minusButton;

	[SerializeField]
	private Button plusButton;

	[SerializeField]
	private Button fillPalletButton;

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Image temperatureBadgeParent;

	[SerializeField]
	private Image temperatureBadgeBG;

	[SerializeField]
	private TextMeshProUGUI safeTemperatureText;

	[SerializeField]
	private TextMeshProUGUI coldSpoilageTimeText;

	[SerializeField]
	private TextMeshProUGUI warmSpoilageTimeText;

	private int currentAmount = 1;

	private ProductData currentData;

	private Color normalStockColor = new Color(0f, 0.94509804f, 1f);

	private static readonly Color lowSensitivityColor = new Color32(0, 158, 9, byte.MaxValue);

	private static readonly Color mediumSensitivityColor = new Color32(146, 185, 39, byte.MaxValue);

	private static readonly Color highSensitivityColor = new Color32(171, 112, 18, byte.MaxValue);

	private static readonly Color criticalSensitivityColor = new Color32(139, 29, 29, byte.MaxValue);

	public ProductData CurrentData => currentData;

	public void Repaint(ProductData data)
	{
		currentData = data;
		if (!ProductLicenseManager.LicensePurchased(data.requiredLicense, data.productGroup))
		{
			unlockedParent.SetActive(value: false);
			licenseRequired.text = Locale.GetWord("required_license_" + data.productGroup).Replace("{0}", data.requiredLicense.ToString());
			lockedParent.gameObject.SetActive(value: true);
			return;
		}
		lockedParent.SetActive(value: false);
		productImage.sprite = data.productSprite;
		productName.text = Locale.GetWord(data.type.ToString());
		productBrand.text = data.brand;
		countInBox.text = data.GetMaxProductCount().ToString();
		storageType.text = Locale.GetWord(data.shelfType.ToString());
		unitPrice.text = data.cost.ToString("0.00", CultureInfo.InvariantCulture);
		if (SingletonBehaviour<OrderManager>.Instance.LastSelectedProductAmounts.ContainsKey(data.type))
		{
			currentAmount = SingletonBehaviour<OrderManager>.Instance.LastSelectedProductAmounts[data.type];
		}
		else
		{
			SingletonBehaviour<OrderManager>.Instance.LastSelectedProductAmounts[data.type] = 1;
			currentAmount = 1;
		}
		UpdateAmount();
		unlockedParent.SetActive(value: true);
		temperatureBadgeParent.gameObject.SetActive(data.storageRequirement != StorageRequirement.None);
		if (data.storageRequirement != StorageRequirement.None)
		{
			temperatureBadgeBG.color = GetSensitivityColor(data.storageSensitivity);
			safeTemperatureText.text = Locale.GetWord("storage_" + data.storageRequirement);
			coldSpoilageTimeText.gameObject.SetActive(data.storageRequirement != StorageRequirement.Freezer);
			coldSpoilageTimeText.text = Locale.GetWord("hour_abbreviated").Replace("{0}", (BoxManager.storageSensitivityToLowTemperatureSpoilageMinutes[data.storageSensitivity] / 60).ToString());
			warmSpoilageTimeText.text = Locale.GetWord("hour_abbreviated").Replace("{0}", (BoxManager.storageSensitivityToHighTemperatureSpoilageMinutes[data.storageSensitivity] / 60).ToString());
		}
	}

	private Color GetSensitivityColor(StorageSensitivity sensitivity)
	{
		return sensitivity switch
		{
			StorageSensitivity.Low => lowSensitivityColor, 
			StorageSensitivity.Medium => mediumSensitivityColor, 
			StorageSensitivity.High => highSensitivityColor, 
			StorageSensitivity.Critical => criticalSensitivityColor, 
			_ => Color.white, 
		};
	}

	public void UpdateStocks(int onShelves, int offShelves)
	{
		onShelvesValue.text = onShelves.ToString();
		offShelvesValue.text = offShelves.ToString();
		onShelvesValue.color = ((onShelves > 0) ? normalStockColor : UIManager.RedColor);
		offShelvesValue.color = ((offShelves > 0) ? normalStockColor : UIManager.RedColor);
	}

	public void OnPlus()
	{
		currentAmount++;
		SingletonBehaviour<OrderManager>.Instance.LastSelectedProductAmounts[currentData.type] = currentAmount;
		UpdateAmount();
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
	}

	public void OnMinus()
	{
		if (currentAmount != 1)
		{
			currentAmount--;
			SingletonBehaviour<OrderManager>.Instance.LastSelectedProductAmounts[currentData.type] = currentAmount;
			UpdateAmount();
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		}
	}

	public void OnFillPallet()
	{
		currentAmount = SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(currentData.type);
		SingletonBehaviour<OrderManager>.Instance.LastSelectedProductAmounts[currentData.type] = currentAmount;
		UpdateAmount();
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
	}

	public void OnResetAmount()
	{
		currentAmount = 1;
		SingletonBehaviour<OrderManager>.Instance.LastSelectedProductAmounts[currentData.type] = currentAmount;
		UpdateAmount();
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
	}

	public void OnAddToCart()
	{
		EventManager.NotifyEvent(ProductEvents.MULTIPLE_PRODUCTS_ADDED_TO_CART, currentData.type, currentAmount);
	}

	private void UpdateAmount()
	{
		amount.text = Locale.GetWord("amount_n").Replace("{0}", currentAmount.ToString());
		totalPrice.text = Locale.GetWord("total_n").Replace("{0}", ((float)currentAmount * currentData.TotalCost()).ToString("0.00", CultureInfo.InvariantCulture));
	}

	private void Start()
	{
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		if (!(currentData == null))
		{
			if (!ProductLicenseManager.LicensePurchased(currentData.requiredLicense, currentData.productGroup))
			{
				licenseRequired.text = Locale.GetWord("required_license_" + currentData.productGroup).Replace("{0}", currentData.requiredLicense.ToString());
			}
			productName.text = Locale.GetWord(currentData.type.ToString());
			storageType.text = Locale.GetWord(currentData.shelfType.ToString());
		}
	}

	public override Selectable GetSelectable()
	{
		return addToCartButton;
	}

	public override Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		Navigation navigation = minusButton.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnLeft = left;
		navigation.selectOnRight = plusButton;
		navigation.selectOnUp = up;
		navigation.selectOnDown = resetButton;
		minusButton.navigation = navigation;
		Navigation navigation2 = resetButton.navigation;
		navigation2.mode = Navigation.Mode.Explicit;
		navigation2.selectOnLeft = left;
		navigation2.selectOnRight = fillPalletButton;
		navigation2.selectOnUp = minusButton;
		navigation2.selectOnDown = addToCartButton;
		resetButton.navigation = navigation2;
		Navigation navigation3 = fillPalletButton.navigation;
		navigation3.mode = Navigation.Mode.Explicit;
		navigation3.selectOnLeft = resetButton;
		navigation3.selectOnRight = right;
		navigation3.selectOnUp = plusButton;
		navigation3.selectOnDown = addToCartButton;
		fillPalletButton.navigation = navigation3;
		Navigation navigation4 = plusButton.navigation;
		navigation4.mode = Navigation.Mode.Explicit;
		navigation4.selectOnLeft = minusButton;
		navigation4.selectOnRight = right;
		navigation4.selectOnUp = up;
		navigation4.selectOnDown = fillPalletButton;
		plusButton.navigation = navigation4;
		Navigation navigation5 = addToCartButton.navigation;
		navigation5.mode = Navigation.Mode.Explicit;
		navigation5.selectOnLeft = left;
		navigation5.selectOnRight = right;
		navigation5.selectOnUp = fillPalletButton;
		navigation5.selectOnDown = down;
		addToCartButton.navigation = navigation5;
		return addToCartButton;
	}
}
