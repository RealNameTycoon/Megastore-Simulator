using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Customer : EscalatorRider
{
	[SerializeField]
	private CharacterMovement characterMovement;

	[SerializeField]
	private Transform hand;

	[SerializeField]
	private Transform bagTop;

	[SerializeField]
	private Transform bagInside;

	[SerializeField]
	private Outline cashParent;

	[SerializeField]
	private Outline creditCardParent;

	[SerializeField]
	private Transform creditCard;

	[SerializeField]
	private Transform creditCardTarget;

	[SerializeField]
	private GameObject smartPhone;

	[SerializeField]
	private AudioSource speechAudioSource;

	[SerializeField]
	private Transform shoulderToRaise;

	[SerializeField]
	private bool isVending;

	[SerializeField]
	private bool isBazaar;

	[SerializeField]
	private bool isFemale;

	[SerializeField]
	private bool isOld;

	[SerializeField]
	private TextMeshPro speechText;

	private static int usedCashCount = 0;

	private static float ANGLE_DELTA = 1f;

	public const float MID_FLOOR_Y = 4f;

	[SerializeField]
	private List<Placeable> placeables = new List<Placeable>();

	[SerializeField]
	private List<Shelf> shelves = new List<Shelf>();

	[SerializeField]
	private List<Product> products = new List<Product>();

	[SerializeField]
	private List<Product> removedProducts = new List<Product>();

	[SerializeField]
	private List<CustomerBehavior> behaviors = new List<CustomerBehavior>();

	[SerializeField]
	private List<float> currentMarketPrices = new List<float>();

	[SerializeField]
	private int targetPlaceableIndex;

	[SerializeField]
	private GameObject creditCardPrefab;

	[SerializeField]
	private GameObject cashPrefab;

	[SerializeField]
	private GameObject palmPrefab;

	[SerializeField]
	private GameObject bagParentPrefab;

	[SerializeField]
	private GameObject speechTextPrefab;

	[SerializeField]
	private AudioSource audioSource;

	private bool canLeaveShop;

	private Transform endPosition;

	private CustomerCar car;

	private Action haltOverAction;

	private bool halted;

	private Transform currentTarget;

	private bool isLeaving;

	private Vector3 creditCardScale;

	private Transform queueSlot;

	private int checkoutDeskIndex = -1;

	private bool isSelfCheckout;

	private Coroutine complaintRoutine;

	private Coroutine checkoutRoutine;

	private Coroutine waitAndActivateMoneyRoutine;

	private Coroutine givePaymentRoutine;

	private float initialVolume;

	private WaitForSeconds waiter = new WaitForSeconds(2f);

	private bool busyAnimating;

	private bool serving;

	private Transform differentFloorTarget;

	private Action onFloorReached;

	public bool IsLeaving => isLeaving;

	public bool IsFemale => isFemale;

	public bool HandingMoney()
	{
		if (!cashParent.gameObject.activeSelf)
		{
			if (creditCardParent.gameObject.activeSelf)
			{
				return creditCardParent.enabled;
			}
			return false;
		}
		return true;
	}

	public Transform FindRecursive(Transform trm, string name)
	{
		Transform transform = null;
		foreach (Transform item in trm)
		{
			if (item.name == name)
			{
				return item;
			}
			if (item.childCount > 0)
			{
				transform = FindRecursive(item, name);
				if ((bool)transform)
				{
					return transform;
				}
			}
		}
		return transform;
	}

	private void Start()
	{
		initialVolume = speechAudioSource.volume;
		speechAudioSource.volume = initialVolume * SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier * SingletonBehaviour<AudioManager>.Instance.SpeechAudioMultiplier;
		creditCardScale = creditCard.localScale;
		EventManager.AddListener(GameEvents.GAME_PAUSED, StopSound);
		EventManager.AddListener(GameEvents.GAME_RESUMED, ResumeSound);
		EventManager.AddListener(UIEvents.AUDIO_MULTIPLIER_CHANGED, OnAudioMultiplierChanged);
		EventManager.AddListener(UIEvents.DIALOGUE_MULTIPLIER_CHANGED, OnAudioMultiplierChanged);
		EventManager.AddListener<Placeable>(PlaceableEvents.PLACEMENT_ENDED, OnPlacementEnded);
		EventManager.AddListener<Furniture>(PlaceableEvents.FURNITURE_PLACEMENT_ENDED, delegate
		{
			OnPlacementEnded(null);
		});
		EventManager.AddListener(SupermarketEvents.FIRE_STARTED, delegate
		{
			if (base.gameObject.activeSelf)
			{
				StopCoroutines();
				if (creditCardParent.gameObject.activeSelf)
				{
					creditCardParent.gameObject.SetActive(value: false);
					creditCard.SetParent(creditCardParent.transform);
					creditCard.localScale = creditCardScale;
					creditCard.transform.position = creditCardTarget.position;
					creditCard.transform.rotation = creditCardTarget.rotation;
				}
				if (cashParent.gameObject.activeSelf)
				{
					cashParent.enabled = false;
					cashParent.gameObject.SetActive(value: false);
				}
				SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).ActivateCheckoutBag(status: false);
				bagTop.gameObject.SetActive(value: true);
				isLeaving = true;
				for (int i = 0; i < removedProducts.Count; i++)
				{
					SingletonBehaviour<ProductPool>.Instance.PutBackToPool(removedProducts[i]);
				}
				removedProducts.Clear();
				for (int j = 0; j < products.Count; j++)
				{
					products[j].isReserved = false;
				}
				TryMove(endPosition, Deactivate);
			}
		});
	}

	public void GoWithoutPayment()
	{
		StopCoroutines();
		if (creditCardParent.gameObject.activeSelf)
		{
			creditCardParent.gameObject.SetActive(value: false);
			creditCard.SetParent(creditCardParent.transform);
			creditCard.localScale = creditCardScale;
			creditCard.transform.position = creditCardTarget.position;
			creditCard.transform.rotation = creditCardTarget.rotation;
		}
		if (cashParent.gameObject.activeSelf)
		{
			cashParent.enabled = false;
			cashParent.gameObject.SetActive(value: false);
		}
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).ActivateCheckoutBag(status: false);
		bagTop.gameObject.SetActive(value: true);
		isLeaving = true;
		for (int i = 0; i < removedProducts.Count; i++)
		{
			SingletonBehaviour<ProductPool>.Instance.PutBackToPool(removedProducts[i]);
		}
		removedProducts.Clear();
		for (int j = 0; j < products.Count; j++)
		{
			products[j].isReserved = false;
		}
		TryMove(endPosition, Deactivate, delegate
		{
			SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).OnLeaveStarted();
		});
	}

	private void OnPlacementEnded(Placeable placeable)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (!halted)
		{
			if (!characterMovement.CanMove(currentTarget))
			{
				Halt();
			}
		}
		else if (currentTarget != null && halted && characterMovement.CanMove(currentTarget))
		{
			haltOverAction?.Invoke();
		}
	}

	private void StopSound()
	{
		speechAudioSource.Pause();
	}

	private void ResumeSound()
	{
		speechAudioSource.UnPause();
	}

	private void OnAudioMultiplierChanged()
	{
		speechAudioSource.volume = initialVolume * SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier * SingletonBehaviour<AudioManager>.Instance.SpeechAudioMultiplier;
	}

	public void ActivateWithCar(Transform endPosition, CustomerCar car)
	{
		this.endPosition = endPosition;
		this.car = car;
		Activate(endPosition);
	}

	public void Activate(Transform endPosition)
	{
		this.endPosition = endPosition;
		canLeaveShop = true;
		bool flag = false;
		flag = ((!isVending) ? SingletonBehaviour<StockManager>.Instance.AssignCustomerPurchaseParameters(this) : SingletonBehaviour<VendingStockManager>.Instance.AssignCustomerPurchaseParameters(this));
		if (!flag || placeables == null || placeables.Count == 0)
		{
			_ = isVending;
			canLeaveShop = false;
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		if (!isVending)
		{
			EventManager.NotifyEvent(CustomerEvents.CUSTOMER_SHOP_STARTED);
		}
		TryMove(placeables[targetPlaceableIndex].PickupPoint, TakeProduct);
	}

	public void SetPurchaseParameters(List<Placeable> placeablesToPurchase, List<Shelf> shelvesToPurchase, List<Product> productsToPurchase, List<CustomerBehavior> customerBehaviors)
	{
		placeables = placeablesToPurchase;
		shelves = shelvesToPurchase;
		products = productsToPurchase;
		behaviors = customerBehaviors;
		targetPlaceableIndex = 0;
		for (int i = 0; i < productsToPurchase.Count; i++)
		{
			currentMarketPrices.Add(SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(productsToPurchase[i].Data.type));
		}
	}

	private void TryMove(Transform target, Action targetReachedAction, Action onMovementStarted = null)
	{
		if (!characterMovement.NavmeshEnabled())
		{
			onFloorReached = targetReachedAction;
			differentFloorTarget = target;
			return;
		}
		if (target.position.y > 4f && base.transform.position.y < 4f)
		{
			onFloorReached = targetReachedAction;
			differentFloorTarget = target;
			target = SingletonBehaviour<EscalatorManager>.Instance.UpwardEscalator.StartStep;
			targetReachedAction = GetIntoUpwardEscalator;
		}
		else if (target.position.y < 4f && base.transform.position.y > 4f)
		{
			onFloorReached = targetReachedAction;
			differentFloorTarget = target;
			target = SingletonBehaviour<EscalatorManager>.Instance.DownwardEscalator.StartStep;
			targetReachedAction = GetIntoDownwardEscalator;
		}
		currentTarget = target;
		base.transform.DOKill();
		haltOverAction = delegate
		{
			if (!characterMovement.IsMoving())
			{
				characterMovement.Animator.SetTrigger("walk0");
				if (smartPhone != null)
				{
					smartPhone.SetActive(value: false);
				}
			}
			onMovementStarted?.Invoke();
			onMovementStarted = null;
			characterMovement.MoveTo(target, targetReachedAction);
			if (halted)
			{
				halted = false;
				EventManager.NotifyEvent(CustomerEvents.CUSTOMER_HALTED, halted);
			}
		};
		if (characterMovement.CanMove(target))
		{
			if (halted)
			{
				halted = false;
				EventManager.NotifyEvent(CustomerEvents.CUSTOMER_HALTED, halted);
			}
			haltOverAction();
		}
		else
		{
			Halt();
		}
	}

	private void GetIntoUpwardEscalator()
	{
		characterMovement.Animator.SetTrigger("idle0");
		characterMovement.EnableNavmesh(enable: false);
		SingletonBehaviour<EscalatorManager>.Instance.UpwardEscalator.TakeToAnotherFloor(this);
	}

	private void GetIntoDownwardEscalator()
	{
		characterMovement.Animator.SetTrigger("idle0");
		characterMovement.EnableNavmesh(enable: false);
		SingletonBehaviour<EscalatorManager>.Instance.DownwardEscalator.TakeToAnotherFloor(this);
	}

	public override void OnDifferentFloorReached()
	{
		characterMovement.EnableNavmesh(enable: true);
		TryMove(differentFloorTarget, onFloorReached);
	}

	private void Halt()
	{
		if (!halted)
		{
			halted = true;
			EventManager.NotifyEvent(CustomerEvents.CUSTOMER_HALTED, halted);
		}
		if (characterMovement.IsMoving())
		{
			characterMovement.Animator.SetInteger("idleType", 0);
			characterMovement.Animator.SetTrigger("idle0");
			characterMovement.StopMoving();
		}
	}

	private int GetProductCount()
	{
		int num = UnityEngine.Random.Range(0, 100);
		if (num < 50)
		{
			return 1;
		}
		if (num < 75)
		{
			return 2;
		}
		if (num < 85)
		{
			return 3;
		}
		if (num < 95)
		{
			return 4;
		}
		if (num < 100)
		{
			return 5;
		}
		return 1;
	}

	public void Deactivate()
	{
		characterMovement.StopMoving();
		canLeaveShop = false;
		placeables.Clear();
		if (shelves != null)
		{
			shelves.Clear();
		}
		products.Clear();
		behaviors.Clear();
		currentMarketPrices.Clear();
		targetPlaceableIndex = 0;
		haltOverAction = null;
		halted = false;
		currentTarget = null;
		isLeaving = false;
		removedProducts.Clear();
		speechText.enabled = false;
		queueSlot = null;
		checkoutDeskIndex = -1;
		isSelfCheckout = false;
		base.transform.position = SingletonBehaviour<CustomerManager>.Instance.PoolTransform.position;
		base.gameObject.SetActive(value: false);
		if (car != null)
		{
			SingletonBehaviour<CarCustomerManager>.Instance.LeaveParkingLot(car);
			car = null;
		}
	}

	public void DeactivateReleaseAll()
	{
		for (int i = 0; i < products.Count; i++)
		{
			products[i].isReserved = false;
			SetPlaceableReserved(placeables[i], reserved: false);
		}
		Deactivate();
	}

	private void TakeProduct()
	{
		if (targetPlaceableIndex == placeables.Count - 1)
		{
			if (isVending)
			{
				EventManager.NotifyEvent(CustomerEvents.VENDING_CUSTOMER_SHOP_OVER);
			}
			else
			{
				EventManager.NotifyEvent(CustomerEvents.CUSTOMER_SHOP_OVER);
			}
		}
		ProductData data = products[targetPlaceableIndex].Data;
		float currentPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
		HashSet<Placeable> hashSet = new HashSet<Placeable>();
		if (behaviors[targetPlaceableIndex] == CustomerBehavior.PURCHASE && currentMarketPrices[targetPlaceableIndex] + 0.1f < currentPrice && data.unitMarketPrice < currentPrice)
		{
			behaviors[targetPlaceableIndex] = CustomerBehavior.OVER_PRICE_COMPLAINT;
			products[targetPlaceableIndex].isReserved = false;
			products[targetPlaceableIndex] = SingletonBehaviour<ProductPool>.Instance.GetProduct(data.type);
			for (int num = products.Count - 1; num > targetPlaceableIndex; num--)
			{
				if (products[num].Data.type == products[targetPlaceableIndex].Data.type)
				{
					behaviors[num] = CustomerBehavior.SKIP;
				}
				else
				{
					hashSet.Add(placeables[num]);
				}
			}
			for (int num2 = products.Count - 1; num2 > targetPlaceableIndex; num2--)
			{
				if (behaviors[num2] == CustomerBehavior.SKIP)
				{
					ReleaseIndex(num2, !hashSet.Contains(placeables[num2]));
				}
			}
		}
		if (behaviors[targetPlaceableIndex] == CustomerBehavior.PURCHASE)
		{
			if (Mathf.Abs(base.transform.eulerAngles.y - placeables[targetPlaceableIndex].PickupPoint.eulerAngles.y) < ANGLE_DELTA)
			{
				canLeaveShop = false;
				if (placeables[targetPlaceableIndex].IsServingPlaceable())
				{
					characterMovement.Animator.SetTrigger("idle0");
					AskWithSpeechText();
				}
				else
				{
					products[targetPlaceableIndex].OnBeforeTake();
					characterMovement.Animator.SetTrigger("pickup0");
				}
				return;
			}
			characterMovement.Animator.SetTrigger("idle0");
			base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, placeables[targetPlaceableIndex].PickupPoint.eulerAngles.y, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
			{
				canLeaveShop = false;
				if (placeables[targetPlaceableIndex].IsServingPlaceable())
				{
					AskWithSpeechText();
				}
				else
				{
					products[targetPlaceableIndex].OnBeforeTake();
					characterMovement.Animator.SetTrigger("pickup0");
				}
			});
		}
		else if (behaviors[targetPlaceableIndex] == CustomerBehavior.NO_STOCK_COMPLAINT)
		{
			EventManager.NotifyEvent("ITEM_OUT_OF_STOCK", this);
			EventManager.NotifyEvent(StatisticsEvents.PRODUCT_NOT_FOUND);
			base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, placeables[targetPlaceableIndex].PickupPoint.eulerAngles.y, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
			{
				canLeaveShop = false;
				characterMovement.Animator.SetTrigger("lookAround");
				if (isVending ? SingletonBehaviour<VendingStockManager>.Instance.IsProductOutStock(products[targetPlaceableIndex].Data.type) : SingletonBehaviour<StockManager>.Instance.IsProductOutStock(products[targetPlaceableIndex].Data.type))
				{
					string word = Locale.GetWord(products[targetPlaceableIndex].Data.type.ToString());
					speechText.text = Locale.GetWord("COMPLAIN_NOSTOCK_" + UnityEngine.Random.Range(1, 11)).Replace("{0}", word);
					speechText.enabled = true;
				}
			});
		}
		else if (behaviors[targetPlaceableIndex] == CustomerBehavior.OVER_PRICE_COMPLAINT)
		{
			EventManager.NotifyEvent("PRICE_COMPLAINT", this);
			EventManager.NotifyEvent(StatisticsEvents.PRODUCT_FOUND_EXPENSIVE);
			base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, placeables[targetPlaceableIndex].PickupPoint.eulerAngles.y, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
			{
				canLeaveShop = false;
				characterMovement.Animator.SetTrigger("lookAround");
				string word = Locale.GetWord(products[targetPlaceableIndex].Data.type.ToString());
				speechText.text = Locale.GetWord("COMPLAIN_PRICE_" + UnityEngine.Random.Range(1, 11)).Replace("{0}", word).Replace("{1}", currentPrice.ToString("0.00", CultureInfo.InvariantCulture));
				speechText.enabled = true;
			});
		}
	}

	private void AskWithSpeechText()
	{
		shelves[targetPlaceableIndex].RemoveProduct(products[targetPlaceableIndex]);
		ChoppingStandPlaceable choppingStandPlaceable = placeables[targetPlaceableIndex] as ChoppingStandPlaceable;
		ChoppableProduct choppableProduct = products[targetPlaceableIndex] as ChoppableProduct;
		List<Product> list = new List<Product>();
		if (!choppableProduct.Sliceable)
		{
			list.Add(products[targetPlaceableIndex]);
			List<Product> list2 = shelves[targetPlaceableIndex].AvailableProducts();
			int num = Mathf.Min(choppableProduct.ChoppableData.localPositions.Length - 1, list2.Count);
			int num2 = UnityEngine.Random.Range(0, num + 1);
			for (int i = 0; i < num2; i++)
			{
				list2[i].isReserved = true;
				shelves[targetPlaceableIndex].RemoveProduct(list2[i]);
				list.Add(list2[i]);
			}
			if (choppingStandPlaceable != null)
			{
				choppingStandPlaceable.AskForProduct(list, this);
			}
		}
		else
		{
			list.Add(products[targetPlaceableIndex]);
			if (choppingStandPlaceable != null)
			{
				choppingStandPlaceable.AskForProduct(list, this);
			}
		}
		int count = list.Count;
		if (count == 1)
		{
			speechText.text = Locale.GetWord("customer_ask_n").Replace("{0}", Locale.GetWord(products[targetPlaceableIndex].Data.type.ToString()));
		}
		else
		{
			speechText.text = Locale.GetWord("customer_ask_n_plural").Replace("{0}", Locale.GetWord(products[targetPlaceableIndex].Data.type.ToString())).Replace("{1}", count.ToString());
		}
		speechText.enabled = true;
	}

	public void GivePreparedProduct(Product product)
	{
		products[targetPlaceableIndex] = product;
		characterMovement.Animator.SetTrigger("pickup0");
		speechText.enabled = false;
	}

	private void ReleaseIndex(int releasedIndex, bool releasePlaceable)
	{
		products[releasedIndex].isReserved = false;
		if (releasePlaceable)
		{
			SetPlaceableReserved(placeables[releasedIndex], reserved: false);
		}
		products.RemoveAt(releasedIndex);
		shelves.RemoveAt(releasedIndex);
		placeables.RemoveAt(releasedIndex);
		currentMarketPrices.RemoveAt(releasedIndex);
		behaviors.RemoveAt(releasedIndex);
	}

	public void OnLookAroundFinished()
	{
		if (placeables.Count - 1 == targetPlaceableIndex)
		{
			speechText.enabled = false;
			SetPlaceableReserved(placeables[targetPlaceableIndex], reserved: false);
			if (isVending)
			{
				EventManager.NotifyEvent(CustomerEvents.VENDING_CUSTOMER_SHOP_OVER);
				bool param = false;
				float num = 0f;
				List<Product> list = new List<Product>();
				float num2 = 0f;
				for (int i = 0; i < products.Count; i++)
				{
					if (behaviors[i] == CustomerBehavior.PURCHASE)
					{
						list.Add(products[i]);
						param = true;
						float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(products[i].Data.type);
						num += unitPrice - products[i].Data.cost;
						num2 += unitPrice;
					}
				}
				EventManager.NotifyEvent(StatisticsEvents.VENDING_CUSTOMER_SATISFIED, param);
				EventManager.NotifyEvent(StatisticsEvents.VENDING_PROFIT_MADE, num);
				EventManager.NotifyEvent(StatisticsEvents.VENDING_PRODUCTS_SOLD, list);
				EventManager.NotifyEvent(PaymentEvents.VENDING_PAYMENT_DONE, num2);
			}
			else if (behaviors[targetPlaceableIndex] == CustomerBehavior.OVER_PRICE_COMPLAINT && removedProducts.Count > 0)
			{
				GetIntoTheQueue();
				return;
			}
			isLeaving = true;
			for (int j = 0; j < products.Count; j++)
			{
				SingletonBehaviour<ProductPool>.Instance.PutBackToPool(products[j]);
			}
			if (!isVending)
			{
				EventManager.NotifyEvent(CustomerEvents.LEAVE_STARTED_COMPLAINER);
			}
			else
			{
				EventManager.NotifyEvent(CustomerEvents.VENDING_LEAVE_STARTED_COMPLAINER);
			}
			TryMove(endPosition, Deactivate);
			return;
		}
		speechText.enabled = false;
		bool flag = true;
		targetPlaceableIndex++;
		for (int k = targetPlaceableIndex; k < placeables.Count; k++)
		{
			if (placeables[k].Equals(placeables[targetPlaceableIndex - 1]))
			{
				flag = false;
			}
		}
		if (flag)
		{
			SetPlaceableReserved(placeables[targetPlaceableIndex - 1], reserved: false);
		}
		if (placeables[targetPlaceableIndex].Equals(placeables[targetPlaceableIndex - 1]))
		{
			TakeProduct();
		}
		else
		{
			TryMove(placeables[targetPlaceableIndex].PickupPoint, TakeProduct);
		}
	}

	public void OnComplainFinished()
	{
		OnLookAroundFinished();
	}

	public void AnimatePlaceable()
	{
		placeables[targetPlaceableIndex].Animate();
	}

	public void GetProductToHand()
	{
		products[targetPlaceableIndex].transform.SetParent(hand);
		if (SingletonWindow<PriceWindow>.Instance.IsOpen() && SingletonWindow<PriceWindow>.Instance.Data != null && SingletonWindow<PriceWindow>.Instance.Data.type == products[targetPlaceableIndex].Data.type)
		{
			EventManager.NotifyEvent("PRICE_ADJUSTMENT_WAITING", this);
		}
		if (!isVending)
		{
			if (products[targetPlaceableIndex].Data.type != ProductType.FOOD_TRAY)
			{
				shelves[targetPlaceableIndex].RemoveProduct(products[targetPlaceableIndex]);
			}
			else
			{
				(products[targetPlaceableIndex] as FoodTrayProduct).isReserved = true;
			}
			if (products[targetPlaceableIndex].Data.IsCookable())
			{
				EventManager.NotifyEvent("BAKERY_FRESHNESS_CONCERN", this);
				CookablePackageProduct cookablePackageProduct = SingletonBehaviour<ProductPool>.Instance.GetProduct(ProductType.COOKABLE_PACK) as CookablePackageProduct;
				cookablePackageProduct.isReserved = true;
				cookablePackageProduct.SetProduct(products[targetPlaceableIndex].Data.type, products[targetPlaceableIndex].TempPrice);
				cookablePackageProduct.transform.SetParent(products[targetPlaceableIndex].transform.parent);
				cookablePackageProduct.transform.localPosition = products[targetPlaceableIndex].transform.localPosition;
				SingletonBehaviour<ProductPool>.Instance.PutBackToPool(products[targetPlaceableIndex]);
				products[targetPlaceableIndex] = cookablePackageProduct;
			}
			removedProducts.Add(products[targetPlaceableIndex]);
		}
		products[targetPlaceableIndex].transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.Linear);
		if (isVending)
		{
			products[targetPlaceableIndex].transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.2f).SetEase(Ease.Linear);
		}
	}

	public void GetProductToBag()
	{
		if (DOTween.IsTweening(products[targetPlaceableIndex].transform))
		{
			products[targetPlaceableIndex].transform.DOKill();
		}
		Product currentProduct = products[targetPlaceableIndex];
		currentProduct.transform.SetParent(bagTop);
		Vector3[] path = new Vector3[2]
		{
			Vector3.zero,
			bagInside.transform.localPosition
		};
		currentProduct.transform.DOLocalPath(path, 0.2f).SetEase(Ease.Linear).OnComplete(delegate
		{
			currentProduct.gameObject.SetActive(value: false);
		});
	}

	public void DropProducts()
	{
		if (isVending)
		{
			(placeables[targetPlaceableIndex] as VendingPlaceable).ShowProductInfo(products[targetPlaceableIndex].Data, products[targetPlaceableIndex].TempPrice);
			if (shelves != null && targetPlaceableIndex < shelves.Count && shelves[targetPlaceableIndex] != null)
			{
				shelves[targetPlaceableIndex].RemoveProduct(products[targetPlaceableIndex]);
				removedProducts.Add(products[targetPlaceableIndex]);
			}
		}
		products[targetPlaceableIndex].transform.DoCurvedMove3D(placeables[targetPlaceableIndex].DropPosition.position, 0.25f);
		characterMovement.Animator.SetTrigger("pickup1");
	}

	public void OnPickupFinished()
	{
		if (targetPlaceableIndex == placeables.Count - 1)
		{
			if (isVending)
			{
				EventManager.NotifyEvent(CustomerEvents.VENDING_CUSTOMER_SHOP_OVER);
				_ = products[targetPlaceableIndex].Data;
				(placeables[targetPlaceableIndex] as VendingPlaceable).ProductPurchased(products[targetPlaceableIndex].TempPrice);
				isLeaving = true;
				EventManager.NotifyEvent(StatisticsEvents.VENDING_CUSTOMER_SATISFIED);
				float num = 0f;
				float num2 = 0f;
				List<Product> list = new List<Product>();
				for (int i = 0; i < products.Count; i++)
				{
					if (behaviors[i] == CustomerBehavior.PURCHASE)
					{
						list.Add(products[i]);
						float tempPrice = products[i].TempPrice;
						num += tempPrice - products[i].Data.cost;
						num2 += tempPrice;
					}
				}
				EventManager.NotifyEvent(StatisticsEvents.VENDING_PROFIT_MADE, num);
				EventManager.NotifyEvent(StatisticsEvents.VENDING_PRODUCTS_SOLD, list);
				EventManager.NotifyEvent(PaymentEvents.VENDING_PAYMENT_DONE, num2);
				for (int j = 0; j < products.Count; j++)
				{
					SingletonBehaviour<ProductPool>.Instance.PutBackToPool(products[j]);
				}
				SetPlaceableReserved(placeables[targetPlaceableIndex], reserved: false);
				TryMove(endPosition, Deactivate, delegate
				{
					EventManager.NotifyEvent(CustomerEvents.VENDING_CUSTOMER_LEAVE_STARTED);
				});
			}
			else
			{
				SetPlaceableReserved(placeables[targetPlaceableIndex], reserved: false);
				GetIntoTheQueue();
			}
			return;
		}
		bool flag = true;
		if (isVending)
		{
			_ = products[targetPlaceableIndex].Data;
			(placeables[targetPlaceableIndex] as VendingPlaceable).ProductPurchased(products[targetPlaceableIndex].TempPrice);
		}
		targetPlaceableIndex++;
		for (int num3 = targetPlaceableIndex; num3 < products.Count; num3++)
		{
			if (placeables[num3].Equals(placeables[targetPlaceableIndex - 1]))
			{
				flag = false;
			}
		}
		if (flag)
		{
			SetPlaceableReserved(placeables[targetPlaceableIndex - 1], reserved: false);
		}
		if (placeables[targetPlaceableIndex].Equals(placeables[targetPlaceableIndex - 1]))
		{
			TakeProduct();
		}
		else
		{
			TryMove(placeables[targetPlaceableIndex].PickupPoint, TakeProduct);
		}
	}

	private void SetPlaceableReserved(Placeable placeable, bool reserved)
	{
		if (isVending)
		{
			SingletonBehaviour<VendingStockManager>.Instance.SetPlaceableReserved(placeable, this, reserved);
		}
		else
		{
			SingletonBehaviour<StockManager>.Instance.SetPlaceableReserved(placeable, this, reserved);
		}
	}

	private void GetIntoTheQueue()
	{
		(checkoutDeskIndex, isSelfCheckout) = SingletonBehaviour<CheckoutDeskManager>.Instance.GetAvailableCheckoutManager(base.transform);
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).GetIntoTheQueue(this);
		queueSlot = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).GetQueueSlot(this);
		TryMove(queueSlot, delegate
		{
			RotateToTheSlot(queueSlot);
		});
	}

	public void UpdateAndMoveToQueueSlot()
	{
		Transform queueSlot = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).GetQueueSlot(this);
		TryMove(queueSlot, delegate
		{
			RotateToTheSlot(queueSlot);
		});
	}

	private void RotateToTheSlot(Transform queueSlot)
	{
		characterMovement.Animator.SetFloat("idleOffset", UnityEngine.Random.Range(0f, 1f));
		characterMovement.Animator.SetInteger("idleType", 0);
		characterMovement.Animator.SetTrigger("idle0");
		base.transform.DORotate(new Vector3(base.transform.eulerAngles.x, queueSlot.eulerAngles.y, base.transform.eulerAngles.z), 0.25f).OnComplete(delegate
		{
			CheckoutManager checkoutManager = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout);
			if (checkoutManager.GetOrder(this) == 0)
			{
				checkoutManager.ActivateCheckoutBag(status: true);
				bagTop.gameObject.SetActive(value: false);
				checkoutManager.PlaceProducts(removedProducts);
				if (checkoutManager.Type == FurnitureType.SELF_CHECKOUT)
				{
					checkoutRoutine = StartCoroutine(CheckoutRoutine());
				}
				if (CanComplain())
				{
					complaintRoutine = StartCoroutine(ComplainRoutine());
				}
				else if (checkoutManager.IsCheckout)
				{
					EventManager.NotifyEvent("REGISTER_GREETING", this);
				}
			}
		});
	}

	private bool CanComplain()
	{
		if (!SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).IsAutomated && !SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).IsCheckout)
		{
			return SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).Type != FurnitureType.SELF_CHECKOUT;
		}
		return false;
	}

	private IEnumerator ComplainRoutine()
	{
		yield return new WaitForSeconds(15f);
		if (CanComplain())
		{
			EventManager.NotifyEvent("CHECKOUT_LONG_WAIT", this);
		}
		yield return new WaitForSeconds(30f);
		if (CanComplain())
		{
			EventManager.NotifyEvent("CHECKOUT_LONG_WAIT", this);
		}
	}

	public void PlayAudio(AudioClip clip)
	{
		speechAudioSource.PlayOneShot(clip);
	}

	public void GiveMoney()
	{
		if (complaintRoutine != null)
		{
			StopCoroutine(complaintRoutine);
			complaintRoutine = null;
		}
		if (SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).Type == FurnitureType.SELF_CHECKOUT)
		{
			waitAndActivateMoneyRoutine = StartCoroutine(WaitAndActivateMoney());
			return;
		}
		DeactivateSmartPhone();
		ActivateMoney();
		characterMovement.Animator.SetTrigger("giveMoney0");
	}

	private IEnumerator WaitAndActivateMoney()
	{
		ActivateMoney();
		yield return new WaitForSeconds(1f);
		characterMovement.Animator.SetTrigger("giveMoney0");
	}

	public void ActivateMoney()
	{
		bool flag = UnityEngine.Random.Range(0, 2) < 1;
		if (SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).Type == FurnitureType.SELF_CHECKOUT)
		{
			flag = false;
		}
		creditCardParent.gameObject.SetActive(!flag);
		cashParent.gameObject.SetActive(flag);
		CheckoutManager checkoutManager = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout);
		if (checkoutManager.IsAutomated || checkoutManager.Type == FurnitureType.SELF_CHECKOUT)
		{
			cashParent.enabled = false;
			creditCardParent.enabled = false;
			givePaymentRoutine = StartCoroutine(GivePaymentRoutine(flag));
		}
		else
		{
			cashParent.enabled = true;
			creditCardParent.enabled = true;
		}
	}

	private IEnumerator GivePaymentRoutine(bool useCash)
	{
		yield return new WaitForSeconds(2f);
		if (useCash)
		{
			BackToIdleAndGiveMoney();
		}
		else
		{
			BacktoIdleAndGiveCard();
		}
	}

	public void OnCashClicked()
	{
		if (!SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).IsAutomated)
		{
			BackToIdleAndGiveMoney();
		}
	}

	public void ActivateSmartPhone()
	{
		if (smartPhone != null)
		{
			smartPhone.gameObject.SetActive(value: true);
		}
	}

	public void DeactivateSmartPhone()
	{
		if (smartPhone != null)
		{
			smartPhone.gameObject.SetActive(value: false);
		}
	}

	public void BackToIdle()
	{
		characterMovement.Animator.SetInteger("idleType", UnityEngine.Random.Range(0, 2));
		characterMovement.Animator.SetTrigger("idle0");
	}

	private void BackToIdleAndGiveMoney()
	{
		characterMovement.Animator.SetInteger("idleType", UnityEngine.Random.Range(0, 2));
		characterMovement.Animator.SetTrigger("idle0");
		cashParent.gameObject.SetActive(value: false);
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).TakeCashPayment();
	}

	public void OnCreditCardClicked()
	{
		if (!SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).IsAutomated && creditCardParent.enabled)
		{
			BacktoIdleAndGiveCard();
		}
	}

	private void BacktoIdleAndGiveCard()
	{
		characterMovement.Animator.SetInteger("idleType", UnityEngine.Random.Range(0, 2));
		characterMovement.Animator.SetTrigger("idle0");
		busyAnimating = false;
		creditCardParent.enabled = false;
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).TakeCardPayment(creditCard);
	}

	public void PaymentDone()
	{
		if (creditCardParent.gameObject.activeSelf)
		{
			creditCardParent.gameObject.SetActive(value: false);
			creditCard.SetParent(creditCardParent.transform);
			creditCard.localScale = creditCardScale;
			creditCard.transform.position = creditCardTarget.position;
			creditCard.transform.rotation = creditCardTarget.rotation;
		}
		busyAnimating = false;
		StopCoroutines();
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).ActivateCheckoutBag(status: false);
		bagTop.gameObject.SetActive(value: true);
		isLeaving = true;
		for (int i = 0; i < products.Count; i++)
		{
			SingletonBehaviour<ProductPool>.Instance.PutBackToPool(products[i]);
		}
		EventManager.NotifyEvent(CustomerEvents.CUSTOMER_LEAVE_STARTED);
		TryMove(endPosition, Deactivate, delegate
		{
			SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).OnLeaveStarted();
		});
	}

	public void PaymentDoneHappy()
	{
		characterMovement.Animator.SetTrigger("happy");
	}

	public void SoundYes()
	{
		if (isFemale)
		{
			if (isOld)
			{
				SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.OLD_WOMAN_YES);
			}
			else
			{
				SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.WOMAN_YES);
			}
		}
		else if (isOld)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.OLD_MAN_YES);
		}
		else
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.MAN_YES);
		}
	}

	public void SoundNo()
	{
		if (isFemale)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.WOMAN_NO);
		}
		else if (isOld)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.OLD_MAN_NO);
		}
		else
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.MAN_NO);
		}
	}

	public void SayNo()
	{
		characterMovement.Animator.SetTrigger("no");
	}

	public void TryLeaveShop()
	{
		if (canLeaveShop)
		{
			for (int i = 0; i < products.Count; i++)
			{
				SingletonBehaviour<ProductPool>.Instance.PutBackToPool(products[i]);
			}
			EventManager.NotifyEvent(CustomerEvents.CUSTOMER_SHOP_HALTED);
			characterMovement.Animator.SetTrigger("walk0");
			characterMovement.MoveTo(endPosition, delegate
			{
				Deactivate();
			});
		}
	}

	private void LeaveImmediately()
	{
		for (int i = 0; i < products.Count; i++)
		{
			SingletonBehaviour<ProductPool>.Instance.PutBackToPool(products[i]);
		}
		characterMovement.Animator.SetTrigger("walk0");
		characterMovement.MoveTo(endPosition, delegate
		{
			Deactivate();
		});
	}

	public void Checkout()
	{
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).CheckoutStep();
	}

	public void TakePayment()
	{
		SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout).TakePayment();
	}

	public void AnimationEnded()
	{
		busyAnimating = false;
	}

	private void CheckAndDoWork()
	{
		CheckoutManager checkoutManager = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutManager(checkoutDeskIndex, isSelfCheckout);
		if (checkoutManager.PlacedItemCount != 0 && !busyAnimating)
		{
			if (checkoutManager.CheckoutAvailable())
			{
				busyAnimating = true;
				serving = true;
				characterMovement.Animator.SetTrigger("checkout");
			}
			else if (checkoutManager.PaymentAvailable)
			{
				busyAnimating = true;
				characterMovement.Animator.SetTrigger("takePayment");
			}
		}
	}

	private IEnumerator CheckoutRoutine()
	{
		yield return new WaitForSeconds(1.5f);
		while (true)
		{
			CheckAndDoWork();
			yield return waiter;
		}
	}

	public void RaiseShoulder()
	{
	}

	private void LateUpdate()
	{
		if (speechText.enabled)
		{
			Transform transform = SingletonBehaviour<PlayerLook>.Instance.MainCamera.transform;
			Vector3 vector = speechText.transform.position - transform.position;
			speechText.transform.LookAt(speechText.transform.position + vector);
		}
	}

	private void StopCoroutines()
	{
		if (checkoutRoutine != null)
		{
			StopCoroutine(checkoutRoutine);
			checkoutRoutine = null;
		}
		if (waitAndActivateMoneyRoutine != null)
		{
			StopCoroutine(waitAndActivateMoneyRoutine);
			waitAndActivateMoneyRoutine = null;
		}
		if (givePaymentRoutine != null)
		{
			StopCoroutine(givePaymentRoutine);
			givePaymentRoutine = null;
		}
		if (complaintRoutine != null)
		{
			StopCoroutine(complaintRoutine);
			complaintRoutine = null;
		}
	}
}
