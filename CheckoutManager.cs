using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CheckoutManager : Furniture
{
	[SerializeField]
	private List<Transform> queueTransforms;

	[SerializeField]
	private List<Transform> productPositions;

	[SerializeField]
	private GameObject checkoutBag;

	[SerializeField]
	private Transform checkoutBagCenter;

	[SerializeField]
	private Transform playerSitTransform;

	[SerializeField]
	private Transform playerStandMoveTransform;

	[SerializeField]
	private Transform playerStandLookTransform;

	[SerializeField]
	private Transform cashierStandTransform;

	[SerializeField]
	private Transform cashierWaitTransform;

	[SerializeField]
	private PlayerLook playerLook;

	[SerializeField]
	private PlayerMove playerMove;

	[SerializeField]
	private Transform creditCardPosTransform;

	[SerializeField]
	private Transform moneyThrowPoint1;

	[SerializeField]
	private Transform moneyThrowPoint2;

	[SerializeField]
	private Transform checkoutTutorialPoint;

	[SerializeField]
	private Collider[] clickableColliders;

	[SerializeField]
	private GameObject noCashierColliders;

	[SerializeField]
	private Image dotImage;

	[SerializeField]
	private CashRegister cashRegister;

	[SerializeField]
	private PosWindow posWindow;

	[SerializeField]
	private Transform scanTopPoint;

	[SerializeField]
	private Transform productsParent;

	[SerializeField]
	private Transform receiptPos;

	[SerializeField]
	private Transform receiptCashRegister;

	[SerializeField]
	private Transform receiptPosTarget;

	[SerializeField]
	private Transform receiptCashRegisterTarget;

	[SerializeField]
	private AudioSource screenSource;

	[SerializeField]
	private AudioSource cashCheckoutSource;

	[SerializeField]
	private AudioSource scannerSource;

	[SerializeField]
	private MonitorCheckoutWindow monitorCheckoutWindow;

	[SerializeField]
	private Transform closedLabel;

	[SerializeField]
	private Transform closedLabelTarget;

	[SerializeField]
	private List<MoveableClickable> clickables;

	[SerializeField]
	private List<GameObject> labelDigitsFirst;

	[SerializeField]
	private List<GameObject> labelDigitsSecond;

	[SerializeField]
	private List<GameObject> labelDigitsFirst2;

	[SerializeField]
	private List<GameObject> labelDigitsSecond2;

	private float remainingTime;

	private float checkoutPrice;

	private Queue<Customer> customers = new Queue<Customer>();

	private List<Product> productsPlaced = new List<Product>();

	private List<Product> productsScanned = new List<Product>();

	private bool isCheckout;

	private int scannedProductCount;

	private bool isAutomated;

	private bool cashPaymentAvailable;

	private bool cardPaymentAvailable;

	private Tweener toTween;

	private float currentZMovement;

	private Vector3 receiptPosInitialRotation;

	private Vector3 receiptCashCheckoutInitialRotation;

	private float initialVolume;

	private Cashier cashier;

	private bool closed;

	private Vector3 closedSignInitialEuler;

	private const string CLOSED_KEY = "checkout_closed_";

	public Transform CheckoutTutorialPoint => checkoutTutorialPoint;

	public float CheckoutPrice => checkoutPrice;

	public bool IsCheckout => isCheckout;

	public bool IsAutomated => isAutomated;

	public int PlacedItemCount => productsPlaced.Count;

	public bool PaymentAvailable
	{
		get
		{
			if (!cardPaymentAvailable)
			{
				return cashPaymentAvailable;
			}
			return true;
		}
	}

	public bool CustomersInQueue => customers.Count > 0;

	public Transform CashierStandTransform => cashierStandTransform;

	public Transform CashierWaitTransform => cashierWaitTransform;

	public void TestCashValues()
	{
		for (int i = 0; i < 10000; i++)
		{
			float amount = UnityEngine.Random.Range(0f, 1500f);
			float givenCash = GetGivenCash(amount);
			Debug.Log("Checkout price: " + amount + " Given cash: " + givenCash);
		}
	}

	public override void InitializeOldFurniture(int id, bool isPacked = false)
	{
		base.InitializeOldFurniture(id, isPacked);
		if (isPacked)
		{
			int lowestAvailableDisplayedID = SingletonBehaviour<CheckoutDeskManager>.Instance.GetLowestAvailableDisplayedID(base.Type);
			SetDisplayedID(lowestAvailableDisplayedID);
		}
		int displayedID = GetDisplayedID();
		int index = displayedID % 10;
		int num = displayedID / 10;
		if (num > 9)
		{
			num = 9;
			index = 9;
		}
		labelDigitsFirst[num].SetActive(value: true);
		labelDigitsSecond[index].SetActive(value: true);
		labelDigitsFirst2[num].SetActive(value: true);
		labelDigitsSecond2[index].SetActive(value: true);
	}

	public override void InitializeNewFurniture(int id)
	{
		base.InitializeNewFurniture(id);
		int lowestAvailableDisplayedID = SingletonBehaviour<CheckoutDeskManager>.Instance.GetLowestAvailableDisplayedID(base.Type);
		SetDisplayedID(lowestAvailableDisplayedID);
		int displayedID = GetDisplayedID();
		int index = displayedID % 10;
		int num = displayedID / 10;
		if (num > 9)
		{
			num = 9;
			index = 9;
		}
		labelDigitsFirst[num].SetActive(value: true);
		labelDigitsSecond[index].SetActive(value: true);
		labelDigitsFirst2[num].SetActive(value: true);
		labelDigitsSecond2[index].SetActive(value: true);
	}

	private void SetCheckout(bool state)
	{
		isCheckout = state;
	}

	public int AssignedCashierID()
	{
		if (cashier != null)
		{
			return cashier.GetEmployeeID();
		}
		return -1;
	}

	public bool CheckoutAvailable()
	{
		return productsPlaced.Count > scannedProductCount;
	}

	public void CheckoutStep(float speedMultiplier = 1f)
	{
		isAutomated = true;
		if (productsPlaced.Count != 0)
		{
			productsPlaced[scannedProductCount].transform.DOKill();
			MoveToBag(productsPlaced[scannedProductCount], speedMultiplier);
		}
	}

	public void SetAutomated(bool state, Cashier cashier)
	{
		isAutomated = state;
		this.cashier = cashier;
		if (isCheckout)
		{
			OverCheckout();
		}
	}

	private void EnableClickableColliders(bool state)
	{
		for (int i = 0; i < clickableColliders.Length; i++)
		{
			clickableColliders[i].enabled = state;
		}
	}

	private void Awake()
	{
		receiptPosInitialRotation = receiptPos.transform.localEulerAngles;
		receiptCashCheckoutInitialRotation = receiptCashRegister.transform.localEulerAngles;
		closed = GenericDataSerializer.LoadBool("checkout_closed_" + base.FurnitureID);
		if (closedLabel != null)
		{
			closedSignInitialEuler = closedLabel.localEulerAngles;
			if (closed)
			{
				closedLabel.localEulerAngles = closedLabelTarget.localEulerAngles;
			}
		}
		monitorCheckoutWindow.SetClosed(closed);
		EventManager.AddListener(UIEvents.AUDIO_MULTIPLIER_CHANGED, OnAudioMultiplierChanged);
		EventManager.AddListener(CustomerEvents.VENDING_CUSTOMER_LEAVE_STARTED, OnVendingCustomerLeaveStarted);
		EventManager.AddListener(SupermarketEvents.FIRE_STARTED, delegate
		{
			ResetCheckout();
			ActivateCheckoutBag(status: false);
			customers.Clear();
			if (IsCheckout)
			{
				RepaintButtonsForCheckout();
			}
		});
		EventManager.AddListener<Shelf, ProductType>(PlaceableEvents.PRODUCT_ADDED, OnProductAdded);
		SetAdditionalClickableActions();
	}

	private void OnProductAdded(Shelf shelf, ProductType productType)
	{
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void SetAdditionalClickableActions()
	{
		List<(KeyCode, (string, Action))> list = new List<(KeyCode, (string, Action))>();
		list.Add((KeyCode.F, ("pack", PackCheckout)));
		list.Add((KeyCode.Mouse1, ("move", delegate
		{
			StartNewPlacement(delegate
			{
				if (isAutomated && cashier != null)
				{
					cashier.MoveToCheckout();
				}
			});
		})));
		list.Add((KeyCode.C, (closed ? "open_checkout" : "close_checkout", SwitchClosedSign)));
		for (int num = 0; num < clickables.Count; num++)
		{
			clickables[num].SetAdditionalActions(list);
		}
	}

	private void PackCheckout()
	{
		if (CanPack())
		{
			SingletonBehaviour<CheckoutDeskManager>.Instance.RemoveCheckoutManager(this);
			SingletonBehaviour<SpawnManager>.Instance.PackFurniture(this);
		}
	}

	private void SwitchClosedSign()
	{
		if (!closed && customers.Count > 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_cannot_close_checkout_with_customers", base.transform);
			return;
		}
		if (!closed && SingletonBehaviour<CheckoutDeskManager>.Instance.GetOpenCheckoutCount() == 1 && SingletonBehaviour<CustomerManager>.Instance.ShoppingCustomerCount() > 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_cannot_close_last_checkout_with_customers", base.transform);
			return;
		}
		if (!closed && SingletonBehaviour<CheckoutDeskManager>.Instance.GetCustomerCapacity() - GetCapacity() < SingletonBehaviour<CustomerManager>.Instance.ShoppingCustomerCount())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_cannot_close_capacity_checkout", base.transform);
			return;
		}
		closed = !closed;
		monitorCheckoutWindow.SetClosed(closed);
		if (base.Type == FurnitureType.SELF_CHECKOUT && Vector3.Distance(base.transform.position, SingletonBehaviour<RayShooter>.Instance.MainCamera.transform.position) < scannerSource.maxDistance + AudioManager.DISTANCE_BUFFER)
		{
			scannerSource.PlayOneShot(SingletonBehaviour<AudioManager>.Instance.GetUIAudioClip(AudioManager.UIAudioTypes.MONITOR_OPENED));
		}
		GenericDataSerializer.SaveBool("checkout_closed_" + base.FurnitureID, closed);
		SetAdditionalClickableActions();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		if (closedLabel != null)
		{
			closedLabel.DOKill();
			Vector3 localEulerAngles;
			if (closed)
			{
				localEulerAngles = closedLabelTarget.localEulerAngles;
				PlayAudioIfInRange(scannerSource, AudioManager.AudioTypes.CHECKOUT_BARRIER_CLOSE);
			}
			else
			{
				localEulerAngles = closedSignInitialEuler;
				PlayAudioIfInRange(scannerSource, AudioManager.AudioTypes.CHECKOUT_BARRIER_OPEN);
			}
			closedLabel.DOLocalRotate(localEulerAngles, 180f).SetEase(Ease.InSine).SetSpeedBased(isSpeedBased: true);
		}
		SingletonBehaviour<CheckoutDeskManager>.Instance.CheckoutStatusChanged();
	}

	private void PlayAudioIfInRange(AudioSource source, AudioManager.AudioTypes type)
	{
		if (Vector3.Distance(base.transform.position, SingletonBehaviour<RayShooter>.Instance.MainCamera.transform.position) < source.maxDistance + AudioManager.DISTANCE_BUFFER)
		{
			source.PlayOneShot(SingletonBehaviour<AudioManager>.Instance.GetAudioClip(type));
		}
	}

	private int GetCapacity()
	{
		if (base.Type == FurnitureType.SELF_CHECKOUT)
		{
			return 4;
		}
		return 7;
	}

	public int GetQueueLength()
	{
		return customers.Count;
	}

	private IEnumerator EnableClickablesRoutine()
	{
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < clickables.Count; i++)
		{
			clickables[i].gameObject.SetActive(value: true);
		}
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	private void OnAudioMultiplierChanged()
	{
	}

	public void RepaintButtonsForCheckout()
	{
		if (!IsCheckout)
		{
			return;
		}
		if (cashRegister.IsOpen)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Space,
					("approve", delegate
					{
						cashRegister.OnOk();
					})
				},
				{
					KeyCode.Mouse0,
					("add", null)
				},
				{
					KeyCode.Mouse1,
					("remove", null)
				}
			});
		}
		else if (posWindow.isLocked)
		{
			posWindow.RepaintButtonsForPos();
		}
		else if (CustomersInQueue && scannedProductCount < productsPlaced.Count)
		{
			if (scannedProductCount == 0)
			{
				SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
				{
					{
						SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
						("leave", delegate
						{
							OverCheckout();
						})
					},
					{
						KeyCode.Mouse0,
						("scan", ScanProduct)
					}
				});
			}
			else
			{
				SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
				{
					KeyCode.Mouse0,
					("scan", ScanProduct)
				} });
			}
		}
		else if (CustomersInQueue && customers.Peek().HandingMoney())
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("take", null)
			} });
		}
		else if (posWindow.IsOpen())
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("interact", null)
			} });
		}
		else
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					OverCheckout();
				})
			} });
		}
	}

	public void OnPaymentFinished(float takenAmount)
	{
		monitorCheckoutWindow.OnPaymentCompleted(takenAmount);
		float num = 0f;
		for (int i = 0; i < productsPlaced.Count; i++)
		{
			num += productsPlaced[i].Data.cost;
		}
		EventManager.NotifyEvent(StatisticsEvents.PROFIT_MADE, checkoutPrice - num);
		Customer customer = customers.Peek();
		EventManager.NotifyEvent(StatisticsEvents.PRODUCTS_SOLD, productsPlaced);
		if (checkoutPrice - takenAmount >= 0.005f)
		{
			customer.PaymentDone();
			HapticController.Vibrate(PresetType.MediumImpact);
			EventManager.NotifyEvent(StatisticsEvents.EXTRA_CHANGE_GIVEN);
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("extra_change_error").Replace("{0}", (checkoutPrice - takenAmount).ToString("0.00", CultureInfo.InvariantCulture)), base.transform);
			EventLogger.ExcessChangeCash();
		}
		else
		{
			EventManager.NotifyEvent(StatisticsEvents.CUSTOMER_SATISFIED);
			customer.PaymentDone();
		}
		productsPlaced.Clear();
		productsScanned.Clear();
		receiptCashRegister.gameObject.SetActive(value: false);
		receiptPos.gameObject.SetActive(value: false);
		if (!isAutomated)
		{
			RepaintButtonsForCheckout();
		}
	}

	public void LastCustomerNotEnoughCash()
	{
	}

	public void OnLeaveStarted()
	{
		customers.Dequeue();
		if (SingletonBehaviour<SpawnManager>.Instance.TotalContainedProduct() == 0 && customers.Count == 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("products_finished", base.transform);
		}
		UpdateCustomersAndMoveToQueue();
	}

	private void UpdateCustomersAndMoveToQueue()
	{
		foreach (Customer customer in customers)
		{
			customer.UpdateAndMoveToQueueSlot();
		}
	}

	private void OnVendingCustomerLeaveStarted()
	{
		if (SingletonBehaviour<SpawnManager>.Instance.TotalContainedProduct() == 0 && customers.Count == 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("products_finished", base.transform);
		}
	}

	public void GetIntoTheQueue(Customer customer)
	{
		customers.Enqueue(customer);
		RepaintButtonsForCheckout();
	}

	public bool HasCapacity()
	{
		return customers.Count < queueTransforms.Count;
	}

	public bool IsClosed()
	{
		return closed;
	}

	public Transform GetQueueSlot(Customer customer)
	{
		int num = 0;
		foreach (Customer customer2 in customers)
		{
			if (customer2.Equals(customer))
			{
				return queueTransforms[num];
			}
			num++;
		}
		return null;
	}

	public int GetOrder(Customer customer)
	{
		int num = 0;
		foreach (Customer customer2 in customers)
		{
			if (customer2.Equals(customer))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void PlaceProducts(List<Product> products)
	{
		ResetCheckout();
		productPositions.Shuffle();
		for (int i = 0; i < products.Count; i++)
		{
			Product currentProduct = products[i];
			currentProduct.gameObject.SetActive(value: true);
			currentProduct.transform.SetParent(productsParent.transform);
			currentProduct.transform.DoCurvedLocalMove3D(productPositions[i].localPosition, 0.2f);
			Vector3 endValue = productPositions[i].localEulerAngles + Vector3.up * UnityEngine.Random.Range(-8f, 8f);
			currentProduct.transform.DOLocalRotate(endValue, 0.2f).OnComplete(delegate
			{
				if (base.Type != FurnitureType.SELF_CHECKOUT)
				{
					currentProduct.SetOnClickAction(delegate
					{
						MoveToBag(currentProduct);
					});
					if (isCheckout)
					{
						currentProduct.EnableOutline(state: true);
					}
				}
			});
			productsPlaced.Add(currentProduct);
		}
		RepaintButtonsForCheckout();
	}

	public void ActivateCheckoutBag(bool status)
	{
		checkoutBag.gameObject.SetActive(status);
	}

	public void OnCheckoutClicked()
	{
		if (IsAutomated)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_automated_checkout", base.transform);
			return;
		}
		EventManager.NotifyEvent(PaymentEvents.SIT_ON_CHECKOUT_DESK);
		SetCheckout(state: true);
		EnableClickableColliders(!isCheckout);
		if (cashRegister.IsOpen)
		{
			cashRegister.EnableColliders(isCheckout);
		}
		if (posWindow.IsOpen())
		{
			posWindow.EnableClickableCollider(IsCheckout);
		}
		for (int i = 0; i < productsPlaced.Count; i++)
		{
			productsPlaced[i].EnableOutline(state: true);
		}
		playerMove.MovementLocked = true;
		playerMove.transform.DOMove(playerSitTransform.position, 0.2f);
		playerLook.transform.DORotate(playerSitTransform.eulerAngles, 0.2f).OnComplete(delegate
		{
			playerLook.UpdateClamp(playerSitTransform.localEulerAngles.x);
			RepaintButtonsForCheckout();
		});
	}

	public void StartCheckoutInstant()
	{
		EventManager.NotifyEvent(PaymentEvents.SIT_ON_CHECKOUT_DESK);
		SetCheckout(state: true);
		EnableClickableColliders(!isCheckout);
		for (int i = 0; i < productsPlaced.Count; i++)
		{
			productsPlaced[i].EnableOutline(state: true);
		}
		playerLook.RotationLocked = true;
		playerMove.MovementLocked = true;
		playerMove.transform.position = playerSitTransform.position;
		playerLook.transform.eulerAngles = playerSitTransform.eulerAngles;
	}

	private void OverCheckout()
	{
		ResetCheckoutAndClose();
		EventManager.NotifyEvent(PaymentEvents.LEFT_CHECKOUT_DESK);
	}

	private void ResetCheckoutAndClose()
	{
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		for (int i = 0; i < productsPlaced.Count; i++)
		{
			productsPlaced[i].EnableOutline(state: false);
		}
		SetCheckout(state: false);
		EnableClickableColliders(!isCheckout);
		cashRegister.EnableColliders(isCheckout);
		posWindow.EnableClickableCollider(IsCheckout);
		playerMove.transform.DOMove(playerStandMoveTransform.position, 0.2f);
		playerLook.CameraParent.transform.DORotate(playerStandMoveTransform.rotation.eulerAngles, 0.2f).OnComplete(delegate
		{
			playerLook.RotationLocked = false;
			playerMove.MovementLocked = false;
		});
		playerLook.transform.DOLocalRotate(playerStandLookTransform.localEulerAngles, 0.2f).OnComplete(delegate
		{
			playerLook.UpdateClamp(playerStandLookTransform.localEulerAngles.x);
		});
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
	}

	private void MoveToBag(Product product, float speedMultiplier = 1f)
	{
		ScanProduct(product);
		if (!isAutomated && scannedProductCount == 1)
		{
			RepaintButtonsForCheckout();
		}
		if (isAutomated)
		{
			_ = 0.76f / speedMultiplier;
		}
		if (base.Type != FurnitureType.SELF_CHECKOUT)
		{
			float z = product.transform.localPosition.z;
			float num = -999f;
			for (int i = 0; i < productsPlaced.Count; i++)
			{
				if (!productsScanned.Contains(productsPlaced[i]) && productsPlaced[i].transform.localPosition.z > num)
				{
					num = productsPlaced[i].transform.localPosition.z;
				}
			}
			if (z > num)
			{
				float num2 = num - z;
				currentZMovement += num2;
				productsParent.transform.DOKill();
				productsParent.transform.DOLocalMoveZ(currentZMovement, 0.4f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
			}
		}
		product.transform.SetParent(scanTopPoint.parent);
		product.transform.DORotate(Vector3.zero, 0.1f).SetDelay(0.1f).SetEase(Ease.Linear);
		DOVirtual.DelayedCall(0.2f, delegate
		{
			PlayAudioIfInRange(scannerSource, AudioManager.AudioTypes.SCANNER_BEEP);
		});
		product.transform.DoCurvedLocalMove3D(scanTopPoint.localPosition, 0.3f, 0.2f).SetEase(Ease.InSine).OnComplete(delegate
		{
			product.transform.DoCurvedLocalMove3D(checkoutBagCenter.localPosition, 0.3f, 0.2f).SetEase(Ease.InSine).OnComplete(delegate
			{
				product.gameObject.SetActive(value: false);
			});
		});
		if (scannedProductCount == productsPlaced.Count)
		{
			productsParent.DOKill();
			productsParent.transform.localPosition = Vector3.zero;
			currentZMovement = 0f;
			customers.Peek().GiveMoney();
			RepaintButtonsForCheckout();
		}
	}

	private void ScanProduct(Product product)
	{
		HapticController.Vibrate(PresetType.LightImpact);
		productsScanned.Add(product);
		scannedProductCount++;
		float tempPrice = product.TempPrice;
		checkoutPrice += tempPrice;
		monitorCheckoutWindow.RepaintTotalValue(checkoutPrice, productsScanned);
	}

	private void ResetCheckout()
	{
		productsPlaced.Clear();
		productsScanned.Clear();
		scannedProductCount = 0;
		checkoutPrice = 0f;
		monitorCheckoutWindow.RepaintTotalValue(checkoutPrice, null);
	}

	public void TakeCardPayment(Transform creditCard)
	{
		monitorCheckoutWindow.RepaintForCard();
		creditCard.SetParent(creditCardPosTransform);
		creditCard.DoCurvedMove(creditCardPosTransform.position, 0.2f, creditCard.position + Vector3.up * 0.2f).SetEase(Ease.Linear);
		creditCard.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.Linear).OnComplete(delegate
		{
			PlayPosAudio(AudioManager.AudioTypes.POS_APPEAR);
		});
		if (!isAutomated && base.Type == FurnitureType.CHECKOUT_DESK)
		{
			posWindow.Open(checkoutPrice);
			RepaintButtonsForCheckout();
		}
		else
		{
			cardPaymentAvailable = true;
		}
		DOVirtual.DelayedCall(0.4f, delegate
		{
			int num = 25;
			int num2 = UnityEngine.Random.Range(0, 3);
			PlayAudioIfInRange(screenSource, (AudioManager.AudioTypes)(num + num2));
			receiptPos.localEulerAngles = receiptPosInitialRotation;
			receiptPos.gameObject.SetActive(value: true);
			receiptPos.DOLocalRotate(receiptPosTarget.localEulerAngles, 1.2f);
		});
	}

	public void PlayPosAudio(AudioManager.AudioTypes type)
	{
		PlayAudioIfInRange(screenSource, type);
	}

	public void PlayCashRegisterAudio(AudioManager.AudioTypes type)
	{
		PlayAudioIfInRange(cashCheckoutSource, type);
	}

	public void TakeCashPayment()
	{
		float givenCash = GetGivenCash(checkoutPrice);
		monitorCheckoutWindow.RepaintForCash(givenCash);
		if (!isAutomated)
		{
			cashRegister.Open(checkoutPrice, givenCash);
			RepaintButtonsForCheckout();
		}
		else
		{
			cashPaymentAvailable = true;
		}
		DOVirtual.DelayedCall(0.4f, delegate
		{
			int num = 25;
			int num2 = UnityEngine.Random.Range(0, 3);
			PlayAudioIfInRange(cashCheckoutSource, (AudioManager.AudioTypes)(num + num2));
			receiptCashRegister.localEulerAngles = receiptCashCheckoutInitialRotation;
			receiptCashRegister.gameObject.SetActive(value: true);
			receiptCashRegister.DOLocalRotate(receiptCashRegisterTarget.localEulerAngles, 1.2f);
		});
	}

	public void TakePayment()
	{
		if (cashPaymentAvailable)
		{
			EventManager.NotifyEvent(PaymentEvents.CASH_PAYMENT_DONE, checkoutPrice);
			OnPaymentFinished(checkoutPrice);
			EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, checkoutPrice * BoostMultiplier());
			cashPaymentAvailable = false;
			PlayAudioIfInRange(screenSource, AudioManager.AudioTypes.PAYMENT_DONE);
		}
		else if (cardPaymentAvailable)
		{
			EventManager.NotifyEvent(PaymentEvents.POS_PAYMENT_DONE, checkoutPrice);
			OnPaymentFinished(checkoutPrice);
			EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, checkoutPrice * BoostMultiplier());
			cardPaymentAvailable = false;
			PlayAudioIfInRange(screenSource, AudioManager.AudioTypes.PAYMENT_DONE);
		}
	}

	public float BoostMultiplier()
	{
		return 1f;
	}

	private float GetGivenCash(float amount)
	{
		_ = (int)(float)(((int)amount / 100 + 1) * 100) / 10;
		float num = ((int)amount / 10 + 1) * 10;
		List<float> list = new List<float>();
		list.Add(amount);
		list.Add(num);
		if (num - 5f > amount)
		{
			list.Add(num - 5f);
		}
		float num2 = Mathf.Floor(amount);
		float num3 = amount - num2;
		if (num3 > 0.01f)
		{
			if (UnityEngine.Random.Range(0, 100) < 50)
			{
				list.Add(Mathf.Ceil(amount));
			}
			if (num3 < 0.5f && UnityEngine.Random.Range(0, 100) < 50)
			{
				list.Add(num2 + 0.5f);
			}
			if (num3 < 0.25f && UnityEngine.Random.Range(0, 100) < 50)
			{
				list.Add(num2 + 0.25f);
			}
			if (num3 < 0.1f && UnityEngine.Random.Range(0, 100) < 50)
			{
				list.Add(num2 + 0.1f);
			}
			if (num3 < 0.05f && UnityEngine.Random.Range(0, 100) < 50)
			{
				list.Add(num2 + 0.05f);
			}
		}
		float num4 = amount % 100f;
		if (num4 > Mathf.Epsilon)
		{
			if (UnityEngine.Random.Range(0, 100) < 25)
			{
				list.Add(amount + (100f - num4));
			}
			if (num4 < 50f + Mathf.Epsilon && UnityEngine.Random.Range(0, 100) < 35)
			{
				float num5 = amount % 50f;
				list.Add(amount + (50f - num5));
			}
			if (num4 < 20f + Mathf.Epsilon && UnityEngine.Random.Range(0, 100) < 40)
			{
				float num6 = amount % 20f;
				list.Add(amount + (20f - num6));
			}
		}
		return list.GetRandomElement();
	}

	public Vector3 GetRandomMoneyPosition()
	{
		UnityEngine.Random.Range(0f, 0.5f);
		UnityEngine.Random.Range(0f, 0.5f);
		float x = Mathf.Lerp(moneyThrowPoint1.position.x, moneyThrowPoint2.position.x, UnityEngine.Random.Range(0f, 1f));
		float y = Mathf.Lerp(moneyThrowPoint1.position.y, moneyThrowPoint2.position.y, UnityEngine.Random.Range(0f, 1f));
		float z = Mathf.Lerp(moneyThrowPoint1.position.z, moneyThrowPoint2.position.z, UnityEngine.Random.Range(0f, 1f));
		return new Vector3(x, y, z);
	}

	public override void SwitchLook(bool toSolidObject)
	{
		EnableClickableColliders(toSolidObject);
		base.SwitchLook(toSolidObject);
	}

	public override void OnPlacementEnded()
	{
		base.OnPlacementEnded();
		EventManager.NotifyEvent(PlaceableEvents.CHECKOUT_PLACEMENT_ENDED, base.FurnitureID);
		UpdateCustomersAndMoveToQueue();
		StartCoroutine(EnableClickablesRoutine());
	}

	public override void StartNewPlacement(Action onPlacementEnded = null, Action onCancelPlacement = null)
	{
		if (base.FurnitureID != -1)
		{
			if (!IsClosed())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_close_checkout_to_move", base.transform);
				return;
			}
			if (customers.Count > 0)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_move_customer", base.transform);
				return;
			}
		}
		for (int i = 0; i < clickables.Count; i++)
		{
			clickables[i].gameObject.SetActive(value: false);
		}
		base.StartNewPlacement(onPlacementEnded, onCancelPlacement);
	}

	public override bool CanPack()
	{
		if (!IsClosed())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_close_checkout", base.transform);
			return false;
		}
		if (customers.Count > 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_customer", base.transform);
			return false;
		}
		if (type == FurnitureType.CHECKOUT_DESK && SingletonBehaviour<EmployeeManager>.Instance.HasCashierForCheckout(base.FurnitureID))
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_fire_cashiers", base.transform);
			return false;
		}
		return true;
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < productPositions.Count; i++)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(productPositions[i].position, 0.05f);
		}
	}

	private void ScanProduct()
	{
		Vector3 pos = new Vector3(0.5f, 0.5f, 0f);
		if (Physics.Raycast(SingletonBehaviour<RayShooter>.Instance.MainCamera.ViewportPointToRay(pos), out var hitInfo, RayShooter.DEFAULT_INTERACTION_DETECTION_DISTANCE, 1 << RayShooter.PRODUCT_LAYER))
		{
			Product component = hitInfo.collider.gameObject.GetComponent<Product>();
			if (component != null)
			{
				component.OnScan();
			}
		}
	}
}
