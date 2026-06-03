using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulkDiscountInfoWindow : UIWindow
{
	[SerializeField]
	private CartWindow cartWindow;

	[SerializeField]
	private TextMeshProUGUI totalBulkDiscountedPalletsText;

	[SerializeField]
	private Button discountInfoButton;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private List<TextMeshProUGUI> discountInfoTexts;

	private void Start()
	{
		discountInfoButton.onClick.AddListener(Open);
		closeButton.onClick.AddListener(Close);
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		RefreshTexts();
	}

	public override void Open()
	{
		canvas.enabled = true;
		base.Open();
		totalBulkDiscountedPalletsText.text = Locale.GetWord("total_discounted_pallets").Replace("{0}", cartWindow.TotalBulkDiscountedPallets.ToString());
	}

	public override void Close()
	{
		canvas.enabled = false;
		base.Close();
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		RefreshTexts();
	}

	private void RefreshTexts()
	{
		Dictionary<ProductGroup, int> bulkDiscounts = OrderManager.bulkDiscounts;
		int num = 0;
		foreach (KeyValuePair<ProductGroup, int> item in bulkDiscounts)
		{
			if (num >= discountInfoTexts.Count)
			{
				discountInfoTexts.Add(Object.Instantiate(discountInfoTexts[0], discountInfoTexts[0].transform.parent, worldPositionStays: true));
			}
			string word = Locale.GetWord(item.Key.ToString().ToLower(CultureInfo.InvariantCulture));
			discountInfoTexts[num].text = Locale.GetWord("discount_row_n").Replace("{0}", word).Replace("{1}", item.Value.ToString());
			num++;
		}
	}
}
