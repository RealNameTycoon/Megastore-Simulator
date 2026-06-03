using System;
using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using TMPro;
using UnityEngine;

public class VendingPlaceable : Placeable
{
	[SerializeField]
	private GameObject productInfoUI;

	[SerializeField]
	private TextMeshPro titleText;

	[SerializeField]
	private SpriteRenderer productSprite;

	[SerializeField]
	private TextMeshPro priceLabelText;

	[SerializeField]
	private TextMeshPro priceText;

	[SerializeField]
	private GameObject readyUI;

	[SerializeField]
	private TextMeshPro readyText;

	[SerializeField]
	private AudioSource paymentDoneSource;

	private string storedMoneyKey = "storedMoneyKey";

	private float storedMoney;

	public override void InitializeOldPlaceable(int id, bool isPacked = false)
	{
		base.InitializeOldPlaceable(id, isPacked);
		if (!isPacked)
		{
			storedMoney = GenericDataSerializer.LoadFloat(storedMoneyKey + type.ToString() + id);
			LocalizeBase.OnLanguageChanged += UpdateLocale;
			UpdateLocale();
		}
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= UpdateLocale;
	}

	private void UpdateLocale()
	{
		readyText.text = Locale.GetWord("ready");
		priceLabelText.text = Locale.GetWord("price_label");
	}

	public override void InitializeNewPlaceable(int id)
	{
		base.InitializeNewPlaceable(id);
		UpdateLocale();
		LocalizeBase.OnLanguageChanged += UpdateLocale;
	}

	public void ProductPurchased(float price)
	{
		storedMoney += price;
		GenericDataSerializer.SaveFloat(storedMoneyKey + type.ToString() + base.PlaceableID, storedMoney);
		if (isHovered)
		{
			ShowTooltip();
		}
		readyUI.SetActive(value: true);
		productInfoUI.SetActive(value: false);
	}

	public void CollectMoney()
	{
		if (!(storedMoney <= 0f) && !Mathf.Approximately(storedMoney, 0f))
		{
			EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, storedMoney);
			storedMoney = 0f;
			GenericDataSerializer.SaveFloat(storedMoneyKey + type.ToString() + base.PlaceableID, storedMoney);
			if (Vector3.Distance(base.transform.position, SingletonBehaviour<RayShooter>.Instance.MainCamera.transform.position) < paymentDoneSource.maxDistance + AudioManager.DISTANCE_BUFFER)
			{
				paymentDoneSource.PlayOneShot(SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.PAYMENT_DONE));
			}
			if (isHovered)
			{
				ShowTooltip();
			}
		}
	}

	public override List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		return new List<(KeyCode, (string, Action))> { (KeyCode.E, ("collect_money", delegate
		{
			CollectMoney();
		})) };
	}

	public override void OnShelfHoverStarted()
	{
		base.OnShelfHoverStarted();
		ShowTooltip();
	}

	public override void OnShelfHoverEnded()
	{
		base.OnShelfHoverEnded();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void ShowTooltip()
	{
		SingletonBehaviour<TooltipUI>.Instance.ShowTooltipWithFullText(Locale.GetWord("cash_inside_n").Replace("{0}", storedMoney.ToString("0.00", CultureInfo.InvariantCulture)), base.transform);
	}

	public override bool CanPack()
	{
		bool flag = base.CanPack();
		if (!flag)
		{
			return false;
		}
		if (storedMoney > 0f)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("vending_machine_money_error", base.transform);
			flag = false;
		}
		return flag;
	}

	public override LayerMask GetPlaceableFloorLayers()
	{
		return (1 << PlacementManager.AROUND_STORE_LAYER) | (1 << PlacementManager.FLOOR_LAYER);
	}

	public void ShowProductInfo(ProductData data, float price)
	{
		productInfoUI.SetActive(value: true);
		titleText.text = Locale.GetWord(data.type.ToString());
		SetSpriteKeepSize(productSprite, data.productSprite);
		priceText.text = "$" + price.ToString("0.00", CultureInfo.InvariantCulture);
		readyUI.SetActive(value: false);
	}

	private void SetSpriteKeepSize(SpriteRenderer sr, Sprite newSprite)
	{
		Sprite sprite = sr.sprite;
		Vector3 vector = (sprite ? sprite.bounds.size : newSprite.bounds.size);
		sr.sprite = newSprite;
		Vector3 size = newSprite.bounds.size;
		Transform obj = sr.transform;
		Vector3 localScale = obj.localScale;
		if (size.x != 0f)
		{
			localScale.x *= vector.x / size.x;
		}
		if (size.y != 0f)
		{
			localScale.y *= vector.y / size.y;
		}
		obj.localScale = localScale;
	}
}
