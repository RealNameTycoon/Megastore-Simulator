using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductInfoWindow : SingletonBehaviour<ProductInfoWindow>
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Image productImage;

	[SerializeField]
	private TextMeshProUGUI ProductNameTitle;

	[SerializeField]
	private TextMeshProUGUI onShelvesCount;

	[SerializeField]
	private TextMeshProUGUI offShelvesCount;

	private Color normalStockColor = new Color(0f, 0.94509804f, 1f);

	private ProductType currentProductType;

	private bool isOpen;

	private Color initialColor;

	private void Start()
	{
		EventManager.AddListener<ProductType>(GameEvents.STOCKS_UPDATED, OnStocksUpdated);
		initialColor = offShelvesCount.color;
	}

	private void OnStocksUpdated(ProductType type)
	{
		if (isOpen && currentProductType == type)
		{
			onShelvesCount.text = SingletonBehaviour<StockManager>.Instance.GetAvailableStockOnShelves(type).ToString();
			offShelvesCount.text = SingletonBehaviour<StockManager>.Instance.GetAvailableStockInBoxes(type).ToString();
		}
	}

	public void Open(ProductData data)
	{
		if (!(data == null))
		{
			currentProductType = data.type;
			productImage.sprite = data.productSprite;
			ProductNameTitle.text = Locale.GetWord(data.type.ToString());
			int availableStockOnShelves = SingletonBehaviour<StockManager>.Instance.GetAvailableStockOnShelves(data.type);
			int availableStockInBoxes = SingletonBehaviour<StockManager>.Instance.GetAvailableStockInBoxes(data.type);
			onShelvesCount.text = availableStockOnShelves.ToString();
			offShelvesCount.text = availableStockInBoxes.ToString();
			onShelvesCount.color = ((availableStockOnShelves > 0) ? normalStockColor : UIManager.RedColor);
			offShelvesCount.color = ((availableStockInBoxes > 0) ? normalStockColor : UIManager.RedColor);
			isOpen = true;
			canvasGroup.DOKill();
			canvasGroup.DOFade(1f, 5f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
		}
	}

	public void Close()
	{
		isOpen = false;
		canvasGroup.DOKill();
		canvasGroup.DOFade(0f, 5f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
	}
}
