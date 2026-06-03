using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingDockProductRow : MonoBehaviour
{
	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private TextMeshProUGUI productNameText;

	[SerializeField]
	private TextMeshProUGUI productCountText;

	[SerializeField]
	private TextMeshProUGUI productStatusText;

	private ProductType type = ProductType.NONE;

	private int index = -1;

	private const int DARKER_ALPHA = 200;

	private const int LIGHTER_ALPHA = 150;

	public void Repaint(ProductType type, int count, string status, int index)
	{
		this.type = type;
		this.index = index;
		productNameText.text = Locale.GetWord(type.ToString());
		productCountText.text = count.ToString();
		productStatusText.text = status;
		if (index % 2 == 0)
		{
			backgroundImage.color = new Color(0f, 0f, 0f, 40f / 51f);
		}
		else
		{
			backgroundImage.color = new Color(0f, 0f, 0f, 0.5882353f);
		}
	}
}
