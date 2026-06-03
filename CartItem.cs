using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartItem : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI productNameText;

	[SerializeField]
	private TextMeshProUGUI productCountText;

	[SerializeField]
	private TextMeshProUGUI capacityText;

	[SerializeField]
	private TextMeshProUGUI totalPriceText;

	[SerializeField]
	private Button addButton;

	[SerializeField]
	private Button removeButton;

	[SerializeField]
	private Toggle palletToggle;

	[SerializeField]
	private Button trashButton;

	[SerializeField]
	private Image backgroundImage;

	private ProductType type = ProductType.NONE;

	private int index;

	private Color defaultBGColor = new Color(0f, 0f, 0f, 40f / 51f);

	private Color stripedBGColor = new Color(20f / 51f, 20f / 51f, 20f / 51f, 40f / 51f);

	public void Repaint(ProductType type, int count, int index, bool hasPallet, OrderManager.OrderReceivingArea deliveryArea)
	{
		this.type = type;
		this.index = index;
		productNameText.text = Locale.GetWord(type.ToString());
		productCountText.text = count.ToString();
		totalPriceText.text = ((float)count * SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(type).TotalCost()).ToString("0.00", CultureInfo.InvariantCulture);
		capacityText.text = count + "/" + SingletonBehaviour<PalletManager>.Instance.GetPalletCapacity(type);
		if (backgroundImage != null)
		{
			if (index % 2 == 0)
			{
				backgroundImage.color = defaultBGColor;
			}
			else
			{
				backgroundImage.color = stripedBGColor;
			}
		}
		if (palletToggle != null)
		{
			palletToggle.SetIsOnWithoutNotify(hasPallet);
			palletToggle.gameObject.SetActive(deliveryArea != OrderManager.OrderReceivingArea.STORE_FRONT);
		}
	}

	private void AddItem()
	{
		EventManager.NotifyEvent(ProductEvents.PRODUCT_ADDEDTO_CART, type, index);
	}

	private void RemoveItem()
	{
		EventManager.NotifyEvent(ProductEvents.PRODUCT_REMOVEDFROM_CART, type, index);
	}

	private void DeleteItem()
	{
		SingletonBehaviour<InputManager>.Instance.SelectElement(null);
		EventManager.NotifyEvent(ProductEvents.PRODUCT_DELETED_FROM_CART, type, index);
	}

	private void PalletToggleChanged(bool value, int index)
	{
		EventManager.NotifyEvent(ProductEvents.PALLET_STATUS_CHANGED, value, index);
	}

	private void Start()
	{
		if (addButton != null)
		{
			addButton.onClick.AddListener(AddItem);
		}
		if (removeButton != null)
		{
			removeButton.onClick.AddListener(RemoveItem);
		}
		if (trashButton != null)
		{
			trashButton.onClick.AddListener(DeleteItem);
		}
		if (palletToggle != null)
		{
			palletToggle.onValueChanged.AddListener(delegate(bool value)
			{
				PalletToggleChanged(value, index);
			});
		}
	}

	public Selectable RefreshNavigation(Selectable up, Selectable down, Selectable trashRight, Selectable palletHeader = null)
	{
		AddBottomNavigation(addButton, down);
		AddBottomNavigation(removeButton, down);
		AddBottomNavigation(palletToggle, down);
		AddBottomNavigation(trashButton, down);
		AddTopNavigation(addButton, up);
		AddTopNavigation(removeButton, up);
		if (palletHeader != null)
		{
			AddTopNavigation(palletToggle, palletHeader);
		}
		else
		{
			AddTopNavigation(palletToggle, up);
		}
		AddTopNavigation(trashButton, up);
		AddRightNavigation(trashButton, trashRight);
		return addButton;
	}

	private void AddBottomNavigation(Selectable target, Selectable bottom)
	{
		Navigation navigation = target.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnDown = bottom;
		target.navigation = navigation;
	}

	private void AddTopNavigation(Selectable target, Selectable top)
	{
		Navigation navigation = target.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnUp = top;
		target.navigation = navigation;
	}

	private void AddRightNavigation(Selectable target, Selectable right)
	{
		Navigation navigation = target.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnRight = right;
		target.navigation = navigation;
	}

	public Selectable GetSelectable()
	{
		return trashButton;
	}
}
