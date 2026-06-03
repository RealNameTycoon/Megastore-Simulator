using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : SingletonBehaviour<UIManager>
{
	[SerializeField]
	private Image holdProgressImage;

	[SerializeField]
	private PosWindow posWindow;

	[SerializeField]
	private PriceWindow priceWindow;

	[SerializeField]
	private EndDayReportWindow endDayReportWindow;

	[SerializeField]
	private ShoppingWindow shoppingWindow;

	[SerializeField]
	private CashCheckoutWindow checkoutWindow;

	[SerializeField]
	private IncomeBoostUI incomeBoost;

	[SerializeField]
	private Button interactUI;

	[SerializeField]
	private RectTransform[] bannerAdaptedTransforms;

	[SerializeField]
	private Canvas mainCanvas;

	[SerializeField]
	private Canvas thiefCanvas;

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private Camera tppCamera;

	[SerializeField]
	private Camera thiefCamera;

	[SerializeField]
	private Canvas thiefCaughtWindow;

	[SerializeField]
	private AdBreakPopup adbreakPopup;

	[SerializeField]
	private Image dotImage;

	[SerializeField]
	private PrologueWindow prologueWindow;

	[SerializeField]
	private UnpaidWagesWindow unpaidWagesWindow;

	public static Color RedColor = new Color(44f / 51f, 26f / 85f, 26f / 85f);

	public static Color GreenColor = Color.green;

	public static Color WarningYellow = new Color(1f, 40f / 51f, 0f);

	public static Color LightBlue = new Color(9f / 85f, 14f / 15f, 1f);

	public static Color WhiteColor = new Color(0.8784314f, 0.8784314f, 0.8784314f);

	public Camera MainCamera => mainCamera;

	public void UpdateHoldProgress(float progress)
	{
		if (progress > 0.33f)
		{
			holdProgressImage.fillAmount = (progress - 0.33f) * 1.5151515f;
		}
	}

	public void ResetHoldProgress()
	{
		holdProgressImage.fillAmount = 0f;
	}

	public bool AllWindowsClosed()
	{
		bool flag = SingletonBehaviour<UIStackManager>.Instance.GetTopWindow() != null && SingletonBehaviour<UIStackManager>.Instance.GetTopWindow().IsOpen();
		if (!priceWindow.IsOpen() && !endDayReportWindow.IsOpen() && !prologueWindow.IsOpen() && !unpaidWagesWindow.IsOpen())
		{
			return !flag;
		}
		return false;
	}

	public void OpenShoppingWindow()
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MONITOR_OPENED);
		EventManager.NotifyEvent(UIEvents.SHOP_WINDOW_OPENED);
		shoppingWindow.Open();
	}

	public void CloseShoppingWindow()
	{
		shoppingWindow.Close();
	}

	public void OpenCheckoutWindow(float price, float givenMoney)
	{
		checkoutWindow.Open(price, givenMoney);
	}

	public void ActivateInteractionUI(bool activate)
	{
	}

	private void Start()
	{
		if (SingletonBehaviour<TutorialManager>.Instance.TutorialDone())
		{
			incomeBoost.Initialize();
		}
		interactUI.onClick.AddListener(delegate
		{
			EventManager.NotifyEvent(UIEvents.INTERACTION_CLICKED);
		});
		EventManager.AddListener(UIEvents.UI_WINDOW_CLOSED, OnWindowClosed);
	}

	public void EnableDot(bool state)
	{
		dotImage.enabled = state;
	}

	private void OnWindowClosed()
	{
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void SwitchToThiefCamera(bool state)
	{
		mainCanvas.enabled = !state;
		mainCamera.enabled = !state;
		thiefCamera.gameObject.SetActive(state);
		thiefCanvas.enabled = state;
	}

	public void OpenThiefCaughtWindow()
	{
		thiefCaughtWindow.enabled = true;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!thiefCaughtWindow.enabled);
	}

	public void CloseThiefCaughtWindow()
	{
		thiefCaughtWindow.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!thiefCaughtWindow.enabled);
	}

	private void Update()
	{
	}
}
