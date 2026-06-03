using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartWindow : UIWindow
{
	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private Image buttonMask;

	[SerializeField]
	private Button okButton;

	[SerializeField]
	private Toggle bakeryToggle;

	[SerializeField]
	private Image bakeryHighlight;

	[SerializeField]
	private GameObject bakerySelectedCircle;

	[SerializeField]
	private GameObject bakeryNotSelectedCircle;

	[SerializeField]
	private Toggle toyCandyToggle;

	[SerializeField]
	private Image toyCandyHighlight;

	[SerializeField]
	private GameObject toyCandySelectedCircle;

	[SerializeField]
	private GameObject toyCandyNotSelectedCircle;

	[SerializeField]
	private Toggle clothingToggle;

	[SerializeField]
	private Image clothingHighlight;

	[SerializeField]
	private GameObject clothingSelectedCircle;

	[SerializeField]
	private GameObject clothingNotSelectedCircle;

	[SerializeField]
	private Toggle groceryToggle;

	[SerializeField]
	private Image groceryHighlight;

	[SerializeField]
	private GameObject grocerySelectedCircle;

	[SerializeField]
	private GameObject groceryNotSelectedCircle;

	[SerializeField]
	private Canvas refundWindow;

	private bool isInitialized;

	public static string supermarketNameKey = "supermarketNameKey";

	public static string supermarketStyleKey = "supermarketStyleKey";

	public static string supermarketLabelStyleKey = "supermarketLabelStyleKey";

	public const int TUTORIAL_ID = 0;

	public bool IsInitialized => isInitialized;

	public void Initialize()
	{
		if (!isInitialized)
		{
			if (!GenericDataSerializer.HasKey("INITIAL_PRDUCTGROUP_KEY"))
			{
				Open();
				SingletonBehaviour<PlayerLook>.Instance.LockCursor(!IsOpen());
				SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
			}
			else if (GenericDataSerializer.HasKey(LoadScreen.returnedMoneyKey))
			{
				SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
				refundWindow.enabled = true;
			}
			inputField.onValueChanged.AddListener(OnValueChanged);
			bakeryToggle.onValueChanged.AddListener(delegate
			{
				ToggleLook(bakeryToggle, bakeryHighlight, bakerySelectedCircle, bakeryNotSelectedCircle);
			});
			toyCandyToggle.onValueChanged.AddListener(delegate
			{
				ToggleLook(toyCandyToggle, toyCandyHighlight, toyCandySelectedCircle, toyCandyNotSelectedCircle);
			});
			clothingToggle.onValueChanged.AddListener(delegate
			{
				ToggleLook(clothingToggle, clothingHighlight, clothingSelectedCircle, clothingNotSelectedCircle);
			});
			groceryToggle.onValueChanged.AddListener(delegate
			{
				ToggleLook(groceryToggle, groceryHighlight, grocerySelectedCircle, groceryNotSelectedCircle);
			});
			okButton.onClick.AddListener(OnOk);
			isInitialized = true;
		}
	}

	public void OnRefundOk()
	{
		refundWindow.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		float param = GenericDataSerializer.LoadFloat(LoadScreen.returnedMoneyKey);
		GenericDataSerializer.DeleteKey(LoadScreen.returnedMoneyKey);
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, param);
	}

	private void OnValueChanged(string newValue)
	{
	}

	private void OnOk()
	{
		if (bakeryToggle.isOn)
		{
			GenericDataSerializer.Save("INITIAL_PRDUCTGROUP_KEY", ProductGroup.BAKERY);
			ProductLicenseManager.PurchaseLicense(1, ProductGroup.BAKERY, defaultPurchase: true);
			EventManager.NotifyEvent(ProductEvents.PRODUCT_LICENSE_PURCHASED, 1, ProductGroup.BAKERY);
		}
		else if (toyCandyToggle.isOn)
		{
			GenericDataSerializer.Save("INITIAL_PRDUCTGROUP_KEY", ProductGroup.TOY);
			ProductLicenseManager.PurchaseLicense(1, ProductGroup.TOY, defaultPurchase: true);
			EventManager.NotifyEvent(ProductEvents.PRODUCT_LICENSE_PURCHASED, 1, ProductGroup.TOY);
		}
		else if (clothingToggle.isOn)
		{
			GenericDataSerializer.Save("INITIAL_PRDUCTGROUP_KEY", ProductGroup.CLOTHING);
			ProductLicenseManager.PurchaseLicense(1, ProductGroup.CLOTHING, defaultPurchase: true);
			EventManager.NotifyEvent(ProductEvents.PRODUCT_LICENSE_PURCHASED, 1, ProductGroup.CLOTHING);
		}
		else if (groceryToggle.isOn)
		{
			GenericDataSerializer.Save("INITIAL_PRDUCTGROUP_KEY", ProductGroup.GROCERY);
			ProductLicenseManager.PurchaseLicense(1, ProductGroup.GROCERY, defaultPurchase: true);
			EventManager.NotifyEvent(ProductEvents.PRODUCT_LICENSE_PURCHASED, 1, ProductGroup.GROCERY);
		}
		SingletonBehaviour<SpawnManager>.Instance.Initialize();
		EventManager.NotifyEvent(ProductEvents.INITIAL_LICENSE_DECIDED);
		if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(0))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 0);
		}
		Close();
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!IsOpen());
	}

	private void ToggleLook(Toggle toggle, Image highlightImage, GameObject selectedCircle, GameObject notSelectedCircle)
	{
		if (toggle.isOn)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TOGGLE);
		}
		highlightImage.enabled = toggle.isOn;
		selectedCircle.SetActive(toggle.isOn);
		notSelectedCircle.SetActive(!toggle.isOn);
	}
}
