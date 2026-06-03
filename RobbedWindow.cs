using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RobbedWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private List<GameObject> stolenItems;

	[SerializeField]
	private List<Image> stolenItemImages;

	[SerializeField]
	private List<TextMeshProUGUI> stolenItemTexts;

	[SerializeField]
	private Button skipToNewDayButton;

	[SerializeField]
	private Button policeButton;

	[SerializeField]
	private TextMeshProUGUI costText;

	private const int CALL_POLICE_COST = 5;

	private void Start()
	{
		skipToNewDayButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<ThiefManager>.Instance.RemoveAllProducts();
			EventManager.NotifyEvent(GameEvents.ROBERY_FINISHED);
			canvas.enabled = false;
			SingletonBehaviour<PlayerLook>.Instance.LockCursor(!canvas.enabled);
			canvasGroup.alpha = 0f;
		});
		policeButton.onClick.AddListener(delegate
		{
			if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughHardCurrency(5))
			{
				EventLogger.LogEvent("c_purchase_police");
				EventManager.NotifyEvent(EconomyEvents.REMOVE_HARD_CURRENCY, 5);
				SingletonBehaviour<ThiefManager>.Instance.ReleaseAllProducts();
				SingletonBehaviour<UIManager>.Instance.OpenThiefCaughtWindow();
				EventManager.NotifyEvent(GameEvents.ROBERY_FINISHED);
				canvas.enabled = false;
				canvasGroup.alpha = 0f;
			}
			else
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_energy", base.transform);
			}
		});
	}

	public void Open(List<ProductData> items, List<int> counts)
	{
		for (int i = 0; i < stolenItems.Count; i++)
		{
			stolenItems[i].gameObject.SetActive(value: false);
		}
		float num = 0f;
		for (int j = 0; j < items.Count; j++)
		{
			float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(items[j].type);
			num += (float)counts[j] * unitPrice;
			stolenItemImages[j].sprite = items[j].productSprite;
			stolenItemTexts[j].text = "x" + counts[j];
			stolenItems[j].gameObject.SetActive(value: true);
		}
		costText.text = "$" + num.ToString("0.00", CultureInfo.InvariantCulture);
		canvasGroup.DOFade(1f, 0.4f);
		canvas.enabled = true;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!canvas.enabled);
	}

	private void Update()
	{
	}
}
