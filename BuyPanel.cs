using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyPanel : MonoBehaviour
{
	public struct CartSlot
	{
		public ProductType type;

		public int amount;

		public bool isInTruck;

		public bool isPallet;

		public CartSlot(ProductType type, int amount)
		{
			this.type = type;
			this.amount = amount;
			isInTruck = false;
			isPallet = true;
		}

		public CartSlot(ProductType type, int amount, bool isPallet = true)
		{
			this.type = type;
			this.amount = amount;
			isInTruck = false;
			this.isPallet = isPallet;
		}
	}

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private TextMeshProUGUI productCountText;

	[SerializeField]
	private Button buyButton;

	[SerializeField]
	private Button clearButton;

	[SerializeField]
	private Button cartButton;

	[SerializeField]
	private CartWindow cartWindow;

	[SerializeField]
	private ScannerCartWindow scannerCartWindow;

	private List<CartSlot> cartSlots = new List<CartSlot>();

	private List<CartSlot> scannerCartSlots = new List<CartSlot>();

	private int slotCount;

	private void Start()
	{
		EventManager.AddListener<ProductType, int>(ProductEvents.PRODUCT_ADDEDTO_CART, AddSingleItemToCart);
		EventManager.AddListener<ProductType, int>(ProductEvents.PRODUCT_REMOVEDFROM_CART, RemoveItemFromCart);
		EventManager.AddListener<ProductType, int>(ProductEvents.MULTIPLE_PRODUCTS_ADDED_TO_CART, AddItemToCart);
		EventManager.AddListener<ProductType, int>(ProductEvents.PRODUCT_DELETED_FROM_CART, DeleteItemFromCart);
		EventManager.AddListener(ProductEvents.SCANNER_PRODUCT_ADDEDTO_CART, delegate(ProductType type)
		{
			OnProductAddedToScannerCart(type);
		});
		EventManager.AddListener(ProductEvents.SCANNER_PRODUCT_ADDEDTO_CART_BOX_SHELF, delegate(ProductType type)
		{
			OnProductAddedToScannerCart(type, isBoxShelf: true);
		});
		EventManager.AddListener(ProductEvents.SCANNER_PRODUCT_REMOVEDFROM_CART, delegate(ProductType type)
		{
			OnProductRemovedFromScannerCart(type);
		});
		EventManager.AddListener(ProductEvents.SCANNER_PRODUCT_REMOVEDFROM_CART_BOX_SHELF, delegate(ProductType type)
		{
			OnProductRemovedFromScannerCart(type, isBoxShelf: true);
		});
		EventManager.AddListener<bool, int>(ProductEvents.PALLET_STATUS_CHANGED, PalletStatusChanged);
		Repaint();
		clearButton.onClick.AddListener(OnClear);
		buyButton.onClick.AddListener(OnBuy);
		cartButton.onClick.AddListener(cartWindow.Open);
	}

	private void OnProductAddedToScannerCart(ProductType type, bool isBoxShelf = false)
	{
		bool isPressingModifier = SingletonBehaviour<InputManager>.Instance.IsPressingModifier;
		int num = ((!isBoxShelf) ? SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(type) : SingletonBehaviour<BoxManager>.Instance.GetBoxCapacity(SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(type).boxType));
		int num2 = ((!isPressingModifier) ? 1 : num);
		for (int i = 0; i < scannerCartSlots.Count; i++)
		{
			if (scannerCartSlots[i].type == type && !IsFull(scannerCartSlots[i]))
			{
				int num3 = RemainingCapacity(scannerCartSlots[i]);
				if (num3 >= num2)
				{
					scannerCartSlots[i] = new CartSlot(type, scannerCartSlots[i].amount + num2);
					num2 = 0;
					break;
				}
				scannerCartSlots[i] = new CartSlot(type, scannerCartSlots[i].amount + num3);
				num2 -= num3;
			}
		}
		while (num2 > 0)
		{
			int num4 = Mathf.Min(num, num2);
			scannerCartSlots.Add(new CartSlot(type, num4));
			num2 -= num4;
		}
		RepaintScannerCart();
	}

	private void OnProductRemovedFromScannerCart(ProductType type, bool isBoxShelf = false)
	{
		bool isPressingModifier = SingletonBehaviour<InputManager>.Instance.IsPressingModifier;
		int num = ((!isBoxShelf) ? SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(type) : SingletonBehaviour<BoxManager>.Instance.GetBoxCapacity(SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(type).boxType));
		int num2 = ((!isPressingModifier) ? 1 : num);
		int num3 = scannerCartSlots.Count - 1;
		while (num3 >= 0 && num2 > 0)
		{
			if (scannerCartSlots[num3].type == type && scannerCartSlots[num3].amount > 0)
			{
				int num4 = Mathf.Min(num2, scannerCartSlots[num3].amount);
				scannerCartSlots[num3] = new CartSlot(type, scannerCartSlots[num3].amount - num4);
				num2 -= num4;
				if (scannerCartSlots[num3].amount == 0)
				{
					scannerCartSlots.RemoveAt(num3);
				}
			}
			num3--;
		}
		RepaintScannerCart();
	}

	private void AddSingleItemToCart(ProductType type, int index)
	{
		SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(type);
		CartSlot slot = cartSlots[index];
		if (RemainingCapacity(slot) >= 1)
		{
			int amount = slot.amount + 1;
			cartSlots[index] = new CartSlot(type, amount, slot.isPallet);
			Repaint();
		}
		else
		{
			AddItemToCart(type, 1);
		}
	}

	private void AddItemToCart(ProductType type, int amount)
	{
		int palletCapacity = SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(type);
		int num = amount;
		for (int i = 0; i < cartSlots.Count; i++)
		{
			if (cartSlots[i].type == type && !IsFull(cartSlots[i]))
			{
				int num2 = RemainingCapacity(cartSlots[i]);
				if (num2 >= num)
				{
					int amount2 = cartSlots[i].amount + num;
					cartSlots[i] = new CartSlot(type, amount2, cartSlots[i].isPallet);
					num = 0;
					break;
				}
				int amount3 = cartSlots[i].amount + num2;
				cartSlots[i] = new CartSlot(type, amount3, cartSlots[i].isPallet);
				num -= num2;
			}
		}
		while (num > 0)
		{
			int num3 = Mathf.Min(palletCapacity, num);
			cartSlots.Add(new CartSlot(type, num3, true));
			num -= num3;
		}
		Repaint();
	}

	private int CapacityForProduct(ProductData data)
	{
		int num = 0;
		for (int i = 0; i < cartSlots.Count; i++)
		{
			if (cartSlots[i].type == data.type)
			{
				num += RemainingCapacity(cartSlots[i]);
			}
			else if (cartSlots[i].amount == 0)
			{
				num += SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(data.boxType);
			}
		}
		return num;
	}

	private void RemoveItemFromCart(ProductType type, int index)
	{
		if (cartSlots[index].amount == 1)
		{
			cartSlots.RemoveAt(index);
		}
		else
		{
			cartSlots[index] = new CartSlot(type, cartSlots[index].amount - 1, cartSlots[index].isPallet);
		}
		Repaint();
	}

	public void MakeAllPallets(bool isPallet)
	{
		for (int i = 0; i < cartSlots.Count; i++)
		{
			cartSlots[i] = new CartSlot(cartSlots[i].type, cartSlots[i].amount, isPallet);
		}
		Repaint();
	}

	private void PalletStatusChanged(bool hasPallet, int index)
	{
		cartSlots[index] = new CartSlot(cartSlots[index].type, cartSlots[index].amount, hasPallet);
		Repaint();
	}

	private void DeleteItemFromCart(ProductType type, int index)
	{
		if (cartSlots.Count > index)
		{
			cartSlots.RemoveAt(index);
		}
		Repaint();
	}

	private void Repaint()
	{
		int num = 0;
		foreach (CartSlot cartSlot in cartSlots)
		{
			num += cartSlot.amount;
		}
		cartWindow.Repaint(cartSlots);
		priceText.text = "$" + cartWindow.CartPrice.ToString("0.00", CultureInfo.InvariantCulture);
		productCountText.text = num.ToString();
	}

	private void RepaintScannerCart()
	{
		scannerCartWindow.Repaint(scannerCartSlots);
	}

	public void OnClear()
	{
		cartSlots.Clear();
		Repaint();
	}

	public void OnBuy()
	{
		cartWindow.Open();
	}

	public void OnClearScannerCart()
	{
		scannerCartSlots.Clear();
		RepaintScannerCart();
	}

	private void Update()
	{
	}

	public static bool IsFull(CartSlot slot)
	{
		return SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(slot.type) <= slot.amount;
	}

	public int RemainingCapacity(CartSlot slot)
	{
		return SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(slot.type) - slot.amount;
	}
}
