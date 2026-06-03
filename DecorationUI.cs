using System;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationUI : SelectableUI
{
	[Serializable]
	public enum DecorationType
	{
		FLOOR,
		WALL
	}

	[SerializeField]
	private int index;

	[SerializeField]
	private DecorationType decorationType;

	[SerializeField]
	private Button purchaseButton;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private Button useButton;

	[SerializeField]
	private GameObject usedUI;

	[SerializeField]
	private float price;

	[SerializeField]
	private TextMeshProUGUI titleText;

	public void Initialize()
	{
		EventManager.AddListener<DecorationType, int>(DecorationEvents.DECORATION_USED, OnDecorationUsed);
		purchaseButton.onClick.AddListener(delegate
		{
			if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(price))
			{
				EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, price);
				SingletonBehaviour<DecorationManager>.Instance.PurchaseDecoration(decorationType, index);
				SingletonBehaviour<DecorationManager>.Instance.UseDecoration(decorationType, index);
				EventLogger.LogEvent("c_purchased_" + decorationType.ToString() + index);
				EventManager.NotifyEvent(UIEvents.TAB_SELECTED_OBJECT_CHANGED);
				Repaint();
			}
			else
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_cash", base.transform);
			}
		});
		useButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<DecorationManager>.Instance.UseDecoration(decorationType, index);
			EventManager.NotifyEvent(UIEvents.TAB_SELECTED_OBJECT_CHANGED);
			Repaint();
		});
		priceText.text = "$" + (int)price;
		titleText.text = Locale.GetWord(decorationType.ToString() + "_" + index);
		Repaint();
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		titleText.text = Locale.GetWord(decorationType.ToString() + "_" + index);
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnDecorationUsed(DecorationType type, int index)
	{
		if ((type == decorationType && this.index == index) || usedUI.activeSelf)
		{
			Repaint();
		}
	}

	private void Repaint()
	{
		if (SingletonBehaviour<DecorationManager>.Instance.IsDecorationPurchased(decorationType, index))
		{
			purchaseButton.gameObject.SetActive(value: false);
			priceText.enabled = false;
			if (IsUsed())
			{
				useButton.gameObject.SetActive(value: false);
				usedUI.gameObject.SetActive(value: true);
			}
			else
			{
				useButton.gameObject.SetActive(value: true);
				usedUI.gameObject.SetActive(value: false);
			}
		}
		else
		{
			purchaseButton.gameObject.SetActive(value: true);
			priceText.enabled = true;
			useButton.gameObject.SetActive(value: false);
			usedUI.gameObject.SetActive(value: false);
		}
	}

	private bool IsUsed()
	{
		if (decorationType == DecorationType.FLOOR && SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationType == DecorationType.FLOOR)
		{
			return index == SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex;
		}
		if (decorationType == DecorationType.WALL && SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationType == DecorationType.WALL)
		{
			return index == SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex;
		}
		return false;
	}

	public override Selectable GetSelectable()
	{
		if (purchaseButton.gameObject.activeSelf)
		{
			return purchaseButton;
		}
		if (useButton.gameObject.activeSelf)
		{
			return useButton;
		}
		return null;
	}
}
