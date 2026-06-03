using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoxContentRow : MonoBehaviour
{
	[SerializeField]
	private RectTransform rowTransform;

	[SerializeField]
	private Image productImage;

	[SerializeField]
	private TextMeshProUGUI productNameText;

	[SerializeField]
	private TextMeshProUGUI productCountText;

	[SerializeField]
	private Image selectedBackground;

	private static float defaultHeight = 50f;

	private static float totalHeight = 550f;

	public void Repaint(ProductType type, int count, bool isSelected, int totalCount)
	{
		float y = Mathf.Min(defaultHeight, totalHeight / (float)totalCount);
		rowTransform.sizeDelta = new Vector2(rowTransform.sizeDelta.x, y);
		productImage.sprite = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(type).productSprite;
		productNameText.text = Locale.GetWord(type.ToString());
		productCountText.text = count.ToString();
		selectedBackground.enabled = isSelected;
	}
}
