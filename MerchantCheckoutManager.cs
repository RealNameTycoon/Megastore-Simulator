using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MerchantCheckoutManager : SingletonBehaviour<MerchantCheckoutManager>
{
	private enum FruitType
	{
		APPLE,
		PINEAPPLE,
		WATERMELON
	}

	[SerializeField]
	private PlayerMove playerMove;

	[SerializeField]
	private PlayerLook playerLook;

	[SerializeField]
	private Transform playerSitTransform;

	[SerializeField]
	private Transform playerStandMoveTransform;

	[SerializeField]
	private Transform playerStandLookTransform;

	[SerializeField]
	private Transform scaleUpper;

	[SerializeField]
	private Transform scaleTarget;

	[SerializeField]
	private Transform scaleArror;

	[SerializeField]
	private Transform productTarget;

	[SerializeField]
	private Transform[] apples;

	[SerializeField]
	private Transform[] pineApples;

	[SerializeField]
	private Transform[] waterMelons;

	[SerializeField]
	private BoxCollider appleBox;

	[SerializeField]
	private BoxCollider pineappleBox;

	[SerializeField]
	private BoxCollider watermelonBox;

	[SerializeField]
	private BoxCollider scaleBox;

	[SerializeField]
	private Outline appleBoxOutline;

	[SerializeField]
	private Outline pineappleBoxOutline;

	[SerializeField]
	private Outline watermelonBoxOutline;

	[SerializeField]
	private Outline scaleOutline;

	[SerializeField]
	private Image dotImage;

	[SerializeField]
	private BoxCollider tableClickableCollider;

	[SerializeField]
	private List<Transform> queueTransforms;

	[SerializeField]
	private GameObject checkoutBag;

	[SerializeField]
	private GameObject checkoutBagCenter;

	[SerializeField]
	private GameObject weightUIParent;

	[SerializeField]
	private Image productImage;

	[SerializeField]
	private TextMeshProUGUI weightAmount;

	[SerializeField]
	private Image fillImage;

	[SerializeField]
	private Slider weightSlider;

	[SerializeField]
	private Sprite appleSprite;

	[SerializeField]
	private Sprite watermelonSprite;

	[SerializeField]
	private Sprite pineAppleSprite;

	[SerializeField]
	private Transform[] applePositions;

	[SerializeField]
	private GameObject[] unlockedSupermarketObjects;

	[SerializeField]
	private GameObject[] lockedSupermarketObjects;

	[SerializeField]
	private Button supermarketPurchaseButton;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Transform window;

	[SerializeField]
	private Transform target;

	private Vector3 initialPosition;

	private Vector3 initialScalePosition;

	private string SUPERMARKET_PURCHASED_KEY = "SUPERMARKET_PURCHASED_KEY";

	private string REMANINING_APPLE_KEY = "REMANINING_APPLE_KEY";

	private string REMANINING_PINEAPPLE_KEY = "REMANINING_PINEAPPLE_KEY";

	private string REMANINING_WATERMELON_KEY = "REMANINING_WATERMELON_KEY";

	private string CHECKOUT_CUSTOMER_COUNT = "MERCHANT_CUSTOMER_COUNT";

	private bool isCheckout;

	private bool supermarketPurchased;

	private int remainingApples;

	private int remainingPineapples;

	private int remainingWatermelons;

	private int notOrderedApples;

	private int notOrderedPineapples;

	private int notOrderedWatermelons;

	private int checkedOutCustomerCount;

	private List<Transform> itemsOnScale;

	private Queue<Customer> customers = new Queue<Customer>();

	private Queue<(int, FruitType)> orders = new Queue<(int, FruitType)>();

	public const float SUPERMARKET_PRICE = 500f;

	public bool SupermarketPurchased
	{
		get
		{
			return true;
		}
		set
		{
			supermarketPurchased = value;
			GenericDataSerializer.SaveBool(SUPERMARKET_PURCHASED_KEY, supermarketPurchased);
		}
	}

	private void SetCheckout(bool state)
	{
		isCheckout = state;
		dotImage.enabled = !isCheckout;
		playerLook.LockCursor(!state);
	}

	public void OpenPurchaseWindow()
	{
		window.DOKill();
		window.DOMove(target.position, 0.2f);
		canvas.enabled = true;
		playerLook.LockCursor(state: false);
	}

	public void Close()
	{
		window.DOKill();
		window.DOMove(initialPosition, 0.2f).OnComplete(delegate
		{
			canvas.enabled = false;
			playerLook.LockCursor(state: true);
		});
	}

	public bool HasProducts()
	{
		if (notOrderedApples <= 0 && notOrderedPineapples <= 0)
		{
			return notOrderedWatermelons > 0;
		}
		return true;
	}

	public void GetIntoTheQueue(Customer customer)
	{
		customers.Enqueue(customer);
		orders.Enqueue(GetRandomOrder());
	}

	private (int, FruitType) GetRandomOrder()
	{
		UnityEngine.Random.Range(0, 3);
		FruitType randomFruitType = GetRandomFruitType();
		switch (randomFruitType)
		{
		case FruitType.APPLE:
		{
			int num = 5;
			notOrderedApples -= num;
			return (num, randomFruitType);
		}
		case FruitType.PINEAPPLE:
			notOrderedPineapples--;
			break;
		case FruitType.WATERMELON:
			notOrderedWatermelons--;
			break;
		}
		return (1, randomFruitType);
	}

	private FruitType GetRandomFruitType()
	{
		List<FruitType> list = new List<FruitType>();
		if (notOrderedApples > 0)
		{
			list.Add(FruitType.APPLE);
		}
		if (notOrderedPineapples > 0)
		{
			list.Add(FruitType.PINEAPPLE);
		}
		if (notOrderedWatermelons > 0)
		{
			list.Add(FruitType.WATERMELON);
		}
		return list.GetRandomElement();
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

	private new void Awake()
	{
		base.Awake();
		supermarketPurchased = true;
		initialPosition = window.transform.position;
	}

	public void PurchaseSupermarket()
	{
		if (!supermarketPurchased && SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(500f))
		{
			EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, 460f);
			SupermarketPurchased = true;
			for (int i = 0; i < unlockedSupermarketObjects.Length; i++)
			{
				unlockedSupermarketObjects[i].SetActive(supermarketPurchased);
			}
			for (int j = 0; j < lockedSupermarketObjects.Length; j++)
			{
				lockedSupermarketObjects[j].SetActive(!supermarketPurchased);
			}
			EventManager.NotifyEvent(SupermarketEvents.SUPERMARKET_PURCHASED);
		}
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

	public void ActivateCheckoutBag(bool status)
	{
		checkoutBag.gameObject.SetActive(status);
		RepaintUI();
		if (isCheckout && status)
		{
			weightUIParent.SetActive(value: true);
		}
		UpdateClickables();
	}

	private void RepaintUI()
	{
		(int, FruitType) tuple = orders.Peek();
		int item = tuple.Item1;
		FruitType item2 = tuple.Item2;
		bool flag = false;
		fillImage.color = Color.red;
		switch (item2)
		{
		case FruitType.APPLE:
			productImage.sprite = appleSprite;
			weightAmount.text = (flag ? (((double)((float)item * 0.225f) * 2.2).ToString("0.00") + " lbs") : ((float)item * 0.225f + " kg"));
			weightSlider.value = 0f;
			break;
		case FruitType.PINEAPPLE:
			productImage.sprite = pineAppleSprite;
			weightAmount.text = (flag ? (((double)((float)item * 1.2f) * 2.2).ToString("0.00") + " lbs") : ((float)item * 1.2f + " kg"));
			weightSlider.value = 0f;
			break;
		case FruitType.WATERMELON:
			productImage.sprite = watermelonSprite;
			weightAmount.text = (flag ? (((double)((float)item * 8.5f) * 2.2).ToString("0.00") + " lbs") : ((float)item * 8.5f + " kg"));
			weightSlider.value = 0f;
			break;
		}
	}

	public void Initialize()
	{
		checkedOutCustomerCount = GenericDataSerializer.LoadInt(CHECKOUT_CUSTOMER_COUNT);
		if (checkedOutCustomerCount == 0 && Mathf.Approximately(SingletonBehaviour<EconomyManager>.Instance.SoftCurrency, 0f))
		{
			EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, 430f);
		}
		supermarketPurchaseButton.interactable = SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(500f);
		for (int i = 0; i < unlockedSupermarketObjects.Length; i++)
		{
			unlockedSupermarketObjects[i].SetActive(supermarketPurchased);
		}
		for (int j = 0; j < lockedSupermarketObjects.Length; j++)
		{
			lockedSupermarketObjects[j].SetActive(!supermarketPurchased);
		}
		initialScalePosition = scaleUpper.position;
		itemsOnScale = new List<Transform>();
		remainingApples = GenericDataSerializer.LoadInt(REMANINING_APPLE_KEY, apples.Length);
		remainingPineapples = GenericDataSerializer.LoadInt(REMANINING_PINEAPPLE_KEY, pineApples.Length);
		remainingWatermelons = GenericDataSerializer.LoadInt(REMANINING_WATERMELON_KEY, waterMelons.Length);
		notOrderedApples = remainingApples;
		notOrderedPineapples = remainingPineapples;
		notOrderedWatermelons = remainingWatermelons;
		for (int k = 0; k < remainingApples; k++)
		{
			apples[k].gameObject.SetActive(value: true);
		}
		for (int l = 0; l < remainingPineapples; l++)
		{
			pineApples[l].gameObject.SetActive(value: true);
		}
		for (int m = 0; m < remainingWatermelons; m++)
		{
			waterMelons[m].gameObject.SetActive(value: true);
		}
	}

	public void OnAppleBoxClicked()
	{
		if (remainingApples != 0)
		{
			Transform removedItem = apples[remainingApples - 1];
			remainingApples--;
			ItemClicked(removedItem, 0.2f);
		}
	}

	public void OnPineappleBoxClicked()
	{
		if (remainingPineapples != 0)
		{
			Transform removedItem = pineApples[remainingPineapples - 1];
			remainingPineapples--;
			ItemClicked(removedItem, 1f);
		}
	}

	public void OnWatermelonBoxClicked()
	{
		if (remainingWatermelons != 0)
		{
			Transform removedItem = waterMelons[remainingWatermelons - 1];
			remainingWatermelons--;
			ItemClicked(removedItem, 1f);
		}
	}

	public void OnScaleClicked()
	{
		if (itemsOnScale.Count != 0)
		{
			StartCoroutine(MoveItemsRoutine());
			scaleArror.DOKill();
			scaleArror.DOLocalRotate(new Vector3(-30f, 0f, 0f), 0.3f);
			scaleUpper.DOKill();
			scaleUpper.DOMove(initialScalePosition, 0.3f);
			weightUIParent.SetActive(value: false);
			scaleBox.enabled = false;
			scaleOutline.enabled = false;
		}
	}

	private IEnumerator MoveItemsRoutine()
	{
		int count = 0;
		for (int i = itemsOnScale.Count - 1; i >= 0; i--)
		{
			count++;
			Transform item = itemsOnScale[i];
			item.SetParent(base.transform);
			item.DOKill();
			item.DOScale(Vector3.zero, 0.3f).SetDelay(0.06f);
			item.DoCurvedLocalMove(checkoutBagCenter.transform.localPosition, 0.3f, 0f).OnComplete(delegate
			{
				item.gameObject.SetActive(value: false);
			});
			yield return new WaitForSeconds((float)count * 0.06f);
		}
		if (customers.Count != 0)
		{
			customers.Peek().GiveMoney();
		}
		itemsOnScale.Clear();
	}

	public void OnPaymentDone()
	{
		float param = 0f;
		if (orders.Peek().Item2 == FruitType.APPLE)
		{
			param = (float)orders.Peek().Item1 * 2f;
		}
		else if (orders.Peek().Item2 == FruitType.PINEAPPLE)
		{
			param = (float)orders.Peek().Item1 * 10f;
		}
		else if (orders.Peek().Item2 == FruitType.WATERMELON)
		{
			param = (float)orders.Peek().Item1 * 20f;
		}
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, param);
		SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.PAYMENT_DONE);
		supermarketPurchaseButton.interactable = SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(500f);
		checkedOutCustomerCount++;
		GenericDataSerializer.SaveInt(CHECKOUT_CUSTOMER_COUNT, checkedOutCustomerCount);
	}

	public void OnLeaveStarted()
	{
		customers.Dequeue();
		if (orders.Peek().Item2 == FruitType.APPLE)
		{
			GenericDataSerializer.SaveInt(REMANINING_APPLE_KEY, remainingApples);
		}
		else if (orders.Peek().Item2 == FruitType.PINEAPPLE)
		{
			GenericDataSerializer.SaveInt(REMANINING_PINEAPPLE_KEY, remainingPineapples);
		}
		else if (orders.Peek().Item2 == FruitType.WATERMELON)
		{
			GenericDataSerializer.SaveInt(REMANINING_WATERMELON_KEY, remainingWatermelons);
		}
		orders.Dequeue();
		weightUIParent.SetActive(value: false);
		if (SingletonBehaviour<CustomerManager>.Instance.ShoppingBazaarCustomerCount() == 0 && !SingletonBehaviour<TimeManager>.Instance.CanSpawnCustomer())
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
			SingletonBehaviour<TimeManager>.Instance.ShowNextDayUI();
		}
		foreach (Customer customer in customers)
		{
			customer.UpdateAndMoveToQueueSlot();
		}
	}

	private void ItemClicked(Transform removedItem, float weight)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
		removedItem.SetParent(scaleUpper);
		itemsOnScale.Add(removedItem);
		float totalWeight = weight * (float)itemsOnScale.Count;
		Transform transform = ((orders.Peek().Item2 != FruitType.APPLE) ? productTarget.transform : applePositions[itemsOnScale.Count - 1]);
		removedItem.DoCurvedMove3D(transform.position, 0.3f).OnComplete(delegate
		{
			scaleUpper.DOMove(Vector3.Lerp(initialScalePosition, scaleTarget.position, totalWeight), 0.3f);
			scaleArror.DOKill();
			scaleArror.DOLocalRotate(new Vector3(scaleArror.localEulerAngles.x, scaleArror.localEulerAngles.y, totalWeight * 90f), 0.5f);
		});
		weightSlider.DOKill();
		float num = (float)itemsOnScale.Count / (float)orders.Peek().Item1;
		weightSlider.DOValue(num, 0.3f);
		fillImage.DOColor(Color.Lerp(Color.red, Color.green, num), 0.3f);
		UpdateClickables();
	}

	public void OnCheckoutClicked()
	{
		EventManager.NotifyEvent(PaymentEvents.SIT_ON_FRUIT_STAND);
		SetCheckout(state: true);
		playerLook.RotationLocked = true;
		playerMove.MovementLocked = true;
		tableClickableCollider.enabled = !isCheckout;
		playerMove.transform.DOMove(playerSitTransform.position, 0.2f);
		playerLook.transform.DORotate(playerSitTransform.eulerAngles, 0.2f).OnComplete(delegate
		{
			UpdateClickables();
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Q,
				("leave", delegate
				{
					OnLeave();
				})
			} });
		});
		if (checkoutBag.activeSelf)
		{
			weightUIParent.SetActive(value: true);
		}
	}

	private void UpdateClickables()
	{
		appleBox.enabled = false;
		pineappleBox.enabled = false;
		watermelonBox.enabled = false;
		appleBoxOutline.enabled = false;
		pineappleBoxOutline.enabled = false;
		watermelonBoxOutline.enabled = false;
		scaleOutline.enabled = false;
		scaleBox.enabled = false;
		if (checkoutBag.gameObject.activeSelf && isCheckout)
		{
			if (itemsOnScale.Count == orders.Peek().Item1)
			{
				scaleOutline.enabled = true;
				scaleBox.enabled = true;
			}
			else if (orders.Peek().Item2 == FruitType.APPLE)
			{
				appleBoxOutline.enabled = true;
				appleBox.enabled = true;
			}
			else if (orders.Peek().Item2 == FruitType.PINEAPPLE)
			{
				pineappleBoxOutline.enabled = true;
				pineappleBox.enabled = true;
			}
			else if (orders.Peek().Item2 == FruitType.WATERMELON)
			{
				watermelonBoxOutline.enabled = true;
				watermelonBox.enabled = true;
			}
		}
	}

	private void OnLeave()
	{
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		SetCheckout(state: false);
		appleBox.enabled = isCheckout;
		pineappleBox.enabled = isCheckout;
		watermelonBox.enabled = isCheckout;
		appleBoxOutline.enabled = isCheckout;
		pineappleBoxOutline.enabled = isCheckout;
		watermelonBoxOutline.enabled = isCheckout;
		tableClickableCollider.enabled = !isCheckout;
		playerLook.RotationLocked = false;
		playerMove.MovementLocked = false;
		playerMove.transform.DOMove(playerStandMoveTransform.position, 0.2f);
		playerMove.transform.DORotate(playerStandMoveTransform.rotation.eulerAngles, 0.2f);
		playerLook.transform.DOLocalRotate(playerStandLookTransform.localEulerAngles, 0.2f);
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		weightUIParent.SetActive(value: false);
	}
}
