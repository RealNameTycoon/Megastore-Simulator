using System;
using System.Collections;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialManager : SingletonBehaviour<TutorialManager>
{
	[SerializeField]
	private PopupAnimation rotatePopup;

	[SerializeField]
	private PopupAnimation movePopup;

	[SerializeField]
	private Transform tutorialArrow;

	[SerializeField]
	private TutorialWindow tutorialWindow;

	[SerializeField]
	private Transform trash;

	[SerializeField]
	private Transform monitor;

	[SerializeField]
	private DOTweenAnimation shoppingButtonHand;

	[SerializeField]
	private DOTweenAnimation addToCartHand;

	[SerializeField]
	private DOTweenAnimation buyHand;

	[SerializeField]
	private DOTweenAnimation confirmCartHand;

	[SerializeField]
	private CanvasGroup shopTutorialWindow;

	[SerializeField]
	private TextMeshProUGUI shopTutorialText;

	[SerializeField]
	private Transform tutorialIndicator;

	[SerializeField]
	private Transform labelTutorialPosition;

	private string lastCompletedStepKey = "lastCompletedStepKey";

	private const int NAME_SUPERMARKET_TUTORIAL_ID = 1;

	public const int BOX_DELETE_TUTORIAL_ID = 5;

	public const int SET_PRICE_TUTORIAL_ID = 6;

	public const int SET_PRICE_CONFIRM_TUTORIAL_ID = 7;

	public const int OPEN_STORELABEL_TUTORIAL_ID = 8;

	public const int CHECKOUT_SIT_TUTORIAL_ID = 9;

	public const int CHECKOUT_TWO_CUSTOMER_TUTORIAL_ID = 10;

	private const int NEEDED_CHECKOUTCOUNT_FOR_TUTORIAL = 2;

	public const int ORDER_PRODUCT_TUTORIAL_ID = 11;

	public const int ADD_TO_CART_TUTORIAL_ID = 12;

	public const int BUY_TUTORIAL_ID = 13;

	public const int CONFIRM_CART_TUTORIAL_ID = 14;

	private int lastCompletedStep = -1;

	public const string TUTORIAL_BOX_SPAWNED_KEY = "TUTORIAL_BOX_SPAWNED_KEY";

	private bool boxDeleted;

	public int LastCompletedStep => lastCompletedStep;

	private new void Awake()
	{
		base.Awake();
		EventManager.AddListener(StartupEvents.SPAWN_MANAGER_INITIALIZED, OnSpawnManagerInitialized);
	}

	public void Start()
	{
		if (lastCompletedStep == -1)
		{
			lastCompletedStep = GenericDataSerializer.LoadInt(lastCompletedStepKey, -1);
		}
		if (lastCompletedStep == 6)
		{
			lastCompletedStep = 5;
		}
		if (lastCompletedStep == 11 || lastCompletedStep == 12 || lastCompletedStep == 13)
		{
			lastCompletedStep = 10;
		}
		EventManager.AddListener<int>(TutorialEvents.TUTORIAL_STEP_DONE, OnTutorialStepCompleted);
		EventManager.AddListener<int>(GameEvents.BOX_DELETED, delegate
		{
			OnBoxThrownOrDeleted();
		});
		EventManager.AddListener(GameEvents.BOX_THROWN, OnBoxThrownOrDeleted);
		EventManager.AddListener(CustomerEvents.SHOP_OPENED, OnShopOpened);
		EventManager.AddListener(PaymentEvents.SIT_ON_CHECKOUT_DESK, OnSitOnCashRegister);
		EventManager.AddListener<float>(PaymentEvents.CASH_PAYMENT_DONE, OnPaymentDone);
		EventManager.AddListener<float>(PaymentEvents.POS_PAYMENT_DONE, OnPaymentDone);
		EventManager.AddListener(UIEvents.SHOP_WINDOW_OPENED, OnShopWindowOpened);
		EventManager.AddListener<ProductType, int>(ProductEvents.PRODUCT_ADDEDTO_CART, OnProductAddedToCart);
		EventManager.AddListener<ProductType, int>(ProductEvents.MULTIPLE_PRODUCTS_ADDED_TO_CART, OnProductAddedToCart);
		EventManager.AddListener(UIEvents.CART_WINDOW_OPENED, OnCartWindowOpened);
		EventManager.AddListener<float>(PaymentEvents.CART_PUCHASED, delegate
		{
			OnCartPurchased();
		});
		EventManager.AddListener(GameEvents.OVEN_TURNED_ON, OnOvenTurnedOn);
		EventManager.AddListener(GameEvents.OVEN_FINISHED_COOKING, OnOvenFinishedCooking);
		EventManager.AddListener(GameEvents.NAME_SET_FIRST_TIME, OnSupermarketNameSet);
	}

	private void OnSpawnManagerInitialized()
	{
		StartCoroutine(InitializeRoutine());
	}

	private void OnSupermarketNameSet()
	{
		if (IsTutorialActive(1))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 1);
		}
	}

	public void OnOvenTurnedOn()
	{
		if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(6) && tutorialWindow.IsKeyActive("tutorial_oven_turnon"))
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TUTORIAL_STEP_DONE);
			tutorialWindow.Open("tutorial_oven_wait");
			tutorialArrow.gameObject.SetActive(value: false);
		}
	}

	public void OnOvenFinishedCooking()
	{
		if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(6) && tutorialWindow.IsKeyActive("tutorial_oven_wait"))
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TUTORIAL_STEP_DONE);
			ActivatePlaceBakeryShelf();
		}
	}

	private void ActivatePlaceBakeryShelf()
	{
		tutorialWindow.Open("tutorial_oven_place_bakery");
		tutorialArrow.transform.position = SingletonBehaviour<SpawnManager>.Instance.GetFirstBakeryShelf().TopShelf.transform.position;
		tutorialArrow.SetParent(SingletonBehaviour<SpawnManager>.Instance.GetFirstBakeryShelf().transform);
		tutorialArrow.transform.eulerAngles = Vector3.zero;
		tutorialArrow.gameObject.SetActive(value: true);
		((Oven)SingletonBehaviour<SpawnManager>.Instance.GetFirstOven()).SetTraysItemFinishedAction(OnTrayProductsFinished);
	}

	private void OnTrayProductsFinished()
	{
		if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(6) && tutorialWindow.IsKeyActive("tutorial_oven_place_bakery"))
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TUTORIAL_STEP_DONE);
			ActivateBakerySetPrice();
		}
	}

	private void ActivateBakerySetPrice()
	{
		Transform transform = SingletonBehaviour<SpawnManager>.Instance.GetFirstBakeryShelf().TopShelf.transform;
		tutorialArrow.transform.position = transform.position;
		tutorialArrow.SetParent(transform);
		tutorialArrow.transform.eulerAngles = Vector3.zero;
		tutorialArrow.gameObject.SetActive(value: true);
		tutorialWindow.Open("tutorial_set_price");
	}

	public void OnRotationTutorialClicked()
	{
		EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 1);
	}

	public void OnMoveTutorialClicked()
	{
		EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 2);
	}

	private IEnumerator InitializeRoutine()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		ActivateTutorial(lastCompletedStep + 1);
	}

	public bool TutorialDone()
	{
		if (lastCompletedStep == -1)
		{
			lastCompletedStep = GenericDataSerializer.LoadInt(lastCompletedStepKey, -1);
		}
		return lastCompletedStep == 14;
	}

	public bool IsTutorialActive(int tutorialID)
	{
		if (TutorialDone())
		{
			return false;
		}
		return tutorialID == lastCompletedStep + 1;
	}

	private void OnTutorialStepCompleted(int tutorialStep)
	{
		lastCompletedStep = tutorialStep;
		GenericDataSerializer.SaveInt(lastCompletedStepKey, lastCompletedStep);
		EventLogger.LogEvent("c_tutorial_completed_" + tutorialStep);
		StartCoroutine(SwitchTutorial());
	}

	private IEnumerator SwitchTutorial()
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TUTORIAL_STEP_DONE);
		DeactivateTutorial(lastCompletedStep);
		if (lastCompletedStep + 1 < 11)
		{
			yield return new WaitForSeconds(0.5f);
		}
		ActivateTutorial(lastCompletedStep + 1);
	}

	private void DeactivateTutorial(int tutorialStep)
	{
		switch (tutorialStep)
		{
		case 1:
			tutorialIndicator.gameObject.SetActive(value: false);
			if (!GenericDataSerializer.HasKey("TUTORIAL_BOX_SPAWNED_KEY"))
			{
				GenericDataSerializer.SaveBool("TUTORIAL_BOX_SPAWNED_KEY", value: true);
				SingletonBehaviour<BoxManager>.Instance.SpawnTutorialBox();
			}
			break;
		case 3:
			tutorialArrow.gameObject.SetActive(value: false);
			break;
		case 4:
			tutorialArrow.gameObject.SetActive(value: false);
			break;
		case 5:
			tutorialArrow.gameObject.SetActive(value: false);
			break;
		case 6:
			tutorialArrow.gameObject.SetActive(value: false);
			break;
		case 8:
			tutorialArrow.gameObject.SetActive(value: false);
			break;
		case 9:
			tutorialArrow.gameObject.SetActive(value: false);
			break;
		case 11:
			shoppingButtonHand.gameObject.SetActive(value: false);
			shoppingButtonHand.DOKill();
			tutorialArrow.gameObject.SetActive(value: false);
			tutorialWindow.Close();
			break;
		case 12:
			addToCartHand.gameObject.SetActive(value: false);
			addToCartHand.DOKill();
			shopTutorialWindow.alpha = 0f;
			break;
		case 13:
			buyHand.gameObject.SetActive(value: false);
			buyHand.DOKill();
			shopTutorialWindow.alpha = 0f;
			break;
		case 14:
			confirmCartHand.gameObject.SetActive(value: false);
			confirmCartHand.DOKill();
			shopTutorialWindow.alpha = 0f;
			SingletonBehaviour<ObjectiveManager>.Instance.Initialize();
			break;
		}
	}

	private void ActivateTutorial(int tutorialStep)
	{
		switch (tutorialStep)
		{
		case 1:
			if (SingletonWindow<SupermarketNameWindow>.Instance.IsNameSet())
			{
				SkipTutorial();
				break;
			}
			tutorialIndicator.transform.position = labelTutorialPosition.position;
			tutorialIndicator.gameObject.SetActive(value: true);
			tutorialWindow.Open("tutorial_name_megastore");
			break;
		case 2:
			SkipTutorial();
			break;
		case 3:
		{
			if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
			{
				SkipTutorial();
				break;
			}
			Box tutorialBox2 = SingletonBehaviour<BoxManager>.Instance.GetTutorialBox();
			if (tutorialBox2 == null || tutorialBox2.IsEmpty())
			{
				SkipTutorial();
				break;
			}
			tutorialArrow.transform.position = tutorialBox2.transform.position;
			tutorialArrow.SetParent(tutorialBox2.transform);
			tutorialArrow.transform.eulerAngles = Vector3.zero;
			tutorialArrow.gameObject.SetActive(value: true);
			tutorialWindow.Open("tutorial_box_pickup");
			break;
		}
		case 4:
		{
			Box tutorialBox = SingletonBehaviour<BoxManager>.Instance.GetTutorialBox();
			if (tutorialBox == null || tutorialBox.IsEmpty())
			{
				SkipTutorial();
				SkipTutorial();
				break;
			}
			tutorialBox.OnItemPlaced = (Action)Delegate.Combine(tutorialBox.OnItemPlaced, new Action(OnTutorialItemPlaced));
			var (transform3, num3) = SingletonBehaviour<SpawnManager>.Instance.GetTopInitialShelf();
			tutorialArrow.transform.SetParent(null);
			tutorialArrow.transform.position = transform3.position + Vector3.up * num3;
			tutorialArrow.transform.eulerAngles = Vector3.zero;
			tutorialArrow.SetParent(transform3, worldPositionStays: true);
			tutorialArrow.gameObject.SetActive(value: true);
			if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.BAKERY)
			{
				tutorialWindow.Open("tutorial_place_products_oven");
			}
			else
			{
				tutorialWindow.Open("tutorial_place_products");
			}
			break;
		}
		case 5:
			tutorialArrow.transform.position = trash.position;
			tutorialArrow.SetParent(trash);
			tutorialArrow.transform.eulerAngles = Vector3.zero;
			tutorialArrow.gameObject.SetActive(value: true);
			tutorialWindow.Open("tutorial_box_delete");
			break;
		case 6:
			if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.BAKERY)
			{
				Oven oven = (Oven)SingletonBehaviour<SpawnManager>.Instance.GetFirstOven();
				if (oven.AnyTrayCooked() || oven.Cooking)
				{
					ActivatePlaceBakeryShelf();
					break;
				}
				if (oven.AllTraysEmpty())
				{
					ActivateBakerySetPrice();
					break;
				}
				var (transform, num) = SingletonBehaviour<SpawnManager>.Instance.GetTopInitialShelf();
				tutorialArrow.transform.position = transform.position + Vector3.up * num;
				tutorialArrow.SetParent(transform);
				tutorialArrow.transform.eulerAngles = Vector3.zero;
				tutorialArrow.gameObject.SetActive(value: true);
				tutorialWindow.Open("tutorial_oven_turnon");
			}
			else
			{
				var (transform2, num2) = SingletonBehaviour<SpawnManager>.Instance.GetTopInitialShelf();
				tutorialArrow.transform.position = transform2.position + Vector3.up * num2;
				tutorialArrow.SetParent(transform2);
				tutorialArrow.gameObject.SetActive(value: true);
				tutorialWindow.Open("tutorial_set_price");
			}
			break;
		case 7:
			tutorialWindow.Open("tutorial_set_price_window");
			break;
		case 8:
			if (SingletonBehaviour<OpenCloseLabel>.Instance.IsOpen)
			{
				SkipTutorial();
				break;
			}
			tutorialArrow.transform.position = SingletonBehaviour<OpenCloseLabel>.Instance.ClosedSign.position;
			tutorialArrow.transform.eulerAngles = Vector3.zero;
			tutorialArrow.SetParent(null);
			tutorialArrow.gameObject.SetActive(value: true);
			tutorialWindow.Open("tutorial_open_store");
			break;
		case 9:
		{
			CheckoutManager checkoutManager = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(0, isSelfCheckout: false);
			if (checkoutManager == null)
			{
				SkipTutorial();
				break;
			}
			tutorialArrow.transform.position = checkoutManager.CheckoutTutorialPoint.position;
			tutorialArrow.SetParent(checkoutManager.CheckoutTutorialPoint);
			tutorialArrow.transform.eulerAngles = Vector3.zero;
			tutorialArrow.gameObject.SetActive(value: true);
			tutorialWindow.Open("tutorial_checkout");
			break;
		}
		case 10:
			if (SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount >= 2)
			{
				SkipTutorial();
			}
			else
			{
				tutorialWindow.OpenWithKeyAndParam("tutorial_perform_checkout", SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount.ToString());
			}
			break;
		case 11:
			shoppingButtonHand.gameObject.SetActive(value: true);
			shoppingButtonHand.DOPlay();
			tutorialArrow.transform.position = monitor.position;
			tutorialArrow.SetParent(monitor);
			tutorialArrow.transform.eulerAngles = Vector3.zero;
			tutorialArrow.gameObject.SetActive(value: true);
			tutorialWindow.Open("tutorial_use_computer");
			break;
		case 12:
			addToCartHand.gameObject.SetActive(value: true);
			addToCartHand.DOPlay();
			ShowShopTooltip("tutorial_add_to_cart");
			break;
		case 13:
			buyHand.gameObject.SetActive(value: true);
			buyHand.DOPlay();
			ShowShopTooltip("tutorial_buy");
			break;
		case 14:
			confirmCartHand.gameObject.SetActive(value: true);
			confirmCartHand.DOPlay();
			ShowShopTooltip("tutorial_confirm_cart");
			break;
		}
	}

	public void EnableAddToCartAnimation(bool state)
	{
		addToCartHand.gameObject.SetActive(state);
	}

	private void SkipTutorial()
	{
		lastCompletedStep++;
		EventLogger.LogEvent("c_tutorial_skipped_" + lastCompletedStep);
		GenericDataSerializer.SaveInt(lastCompletedStepKey, lastCompletedStep);
		ActivateTutorial(lastCompletedStep + 1);
	}

	private void OnShopOpened()
	{
		if (IsTutorialActive(8))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 8);
		}
	}

	private void OnSitOnCashRegister()
	{
		if (IsTutorialActive(9))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 9);
		}
	}

	private void OnPaymentDone(float payment)
	{
		if (IsTutorialActive(10))
		{
			tutorialWindow.UpdateText("tutorial_perform_checkout", SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount.ToString());
			if (SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount == 2)
			{
				EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 10);
			}
		}
	}

	private void OnShopWindowOpened()
	{
		if (IsTutorialActive(11))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 11);
		}
	}

	private void OnProductAddedToCart(ProductType type, int count)
	{
		if (IsTutorialActive(12))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 12);
		}
	}

	private void OnCartWindowOpened()
	{
		if (IsTutorialActive(13))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 13);
		}
	}

	private void OnCartPurchased()
	{
		if (IsTutorialActive(14))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 14);
		}
	}

	private void OnTutorialItemPlaced()
	{
		if (SingletonBehaviour<BoxManager>.Instance.GetTutorialBox().IsEmpty() && IsTutorialActive(4))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 4);
		}
	}

	private void OnBoxThrownOrDeleted()
	{
		boxDeleted = true;
		if (IsTutorialActive(5) && boxDeleted)
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 5);
		}
	}

	private void ShowShopTooltip(string key)
	{
		shopTutorialText.text = Locale.GetWord(key);
		shopTutorialWindow.DOFade(1f, 0.2f);
	}

	public void ShowTutorialTooltipTimed(string key)
	{
		ShowShopTooltip(key);
		shopTutorialWindow.DOFade(0f, 0.2f).SetDelay(3f);
	}
}
