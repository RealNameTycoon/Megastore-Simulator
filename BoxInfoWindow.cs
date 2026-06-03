using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoxInfoWindow : SingletonBehaviour<BoxInfoWindow>
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Image productImage;

	[SerializeField]
	private TextMeshProUGUI productName;

	[SerializeField]
	private TextMeshProUGUI productCount;

	[SerializeField]
	private Sprite boxSprite;

	[SerializeField]
	private GameObject spoilageRow;

	[SerializeField]
	private TextMeshProUGUI spoilageText;

	[SerializeField]
	private TextMeshProUGUI spoilageValue;

	[SerializeField]
	private GameObject temperatureNeedRow;

	[SerializeField]
	private Image hotIcon;

	[SerializeField]
	private Image coldIcon;

	[SerializeField]
	private Image okIcon;

	[SerializeField]
	private TextMeshProUGUI temperatureNeedText;

	[SerializeField]
	private GameObject discardRow;

	public void Open(ProductData data, int count)
	{
		if (data == null)
		{
			productImage.sprite = boxSprite;
			productName.text = Locale.GetWord("empty");
			productCount.text = "0";
		}
		else
		{
			productImage.sprite = data.productSprite;
			productName.text = Locale.GetWord(data.type.ToString());
			productCount.text = count.ToString();
		}
		temperatureNeedRow.SetActive(value: false);
		spoilageRow.SetActive(value: false);
		discardRow.SetActive(value: false);
		canvasGroup.DOKill();
		canvasGroup.DOFade(1f, 5f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
	}

	public void Open(BoxType type)
	{
		productImage.sprite = boxSprite;
		productName.text = Locale.GetWord(type.ToString());
		productCount.text = Locale.GetWord("empty");
		temperatureNeedRow.SetActive(value: false);
		spoilageRow.SetActive(value: false);
		discardRow.SetActive(value: false);
		canvasGroup.DOKill();
		canvasGroup.DOFade(1f, 5f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
	}

	public void Open(ProductData data, int count, float freshnessSpoilageProgress, float frozenDamageProgress, StorageRequirement storageRequirement, float currentTemperature)
	{
		if (data == null)
		{
			productImage.sprite = boxSprite;
			productName.text = Locale.GetWord("empty");
			productCount.text = "0";
			temperatureNeedRow.SetActive(value: false);
			spoilageRow.SetActive(value: false);
		}
		else
		{
			productImage.sprite = data.productSprite;
			productName.text = Locale.GetWord(data.type.ToString());
			productCount.text = count.ToString();
			if (frozenDamageProgress > freshnessSpoilageProgress)
			{
				spoilageRow.SetActive(value: true);
				spoilageText.text = Locale.GetWord("frozen_damage_label");
				int a = (int)(frozenDamageProgress * 100f);
				a = Mathf.Min(a, 100);
				spoilageValue.text = a + "%";
			}
			else
			{
				spoilageRow.SetActive(value: true);
				if (storageRequirement == StorageRequirement.Freezer)
				{
					spoilageText.text = Locale.GetWord("thawing_damage_label");
				}
				else
				{
					spoilageText.text = Locale.GetWord("spoilage_damage_label");
				}
				int a2 = (int)(freshnessSpoilageProgress * 100f);
				a2 = Mathf.Min(a2, 100);
				spoilageValue.text = a2 + "%";
			}
			if (Mathf.Max(freshnessSpoilageProgress, frozenDamageProgress) >= 1f)
			{
				temperatureNeedRow.SetActive(value: false);
				discardRow.SetActive(value: true);
			}
			else
			{
				discardRow.SetActive(value: false);
				int num = BoxManager.storageRequirementToMinDegree[storageRequirement];
				int num2 = BoxManager.storageRequirementToMaxDegree[storageRequirement];
				bool active = currentTemperature < (float)num - Mathf.Epsilon;
				bool active2 = currentTemperature > (float)num2 + Mathf.Epsilon;
				bool flag = currentTemperature >= (float)num - Mathf.Epsilon && currentTemperature <= (float)num2 + Mathf.Epsilon;
				temperatureNeedRow.SetActive(value: true);
				hotIcon.gameObject.SetActive(active2);
				okIcon.gameObject.SetActive(flag);
				coldIcon.gameObject.SetActive(active);
				string word = Locale.GetWord("storage_" + storageRequirement);
				if (!flag)
				{
					temperatureNeedText.text = Locale.GetWord("needs_n").Replace("{0}", word);
				}
				else
				{
					temperatureNeedText.text = Locale.GetWord("n_ok").Replace("{0}", word);
				}
			}
			AnimateActiveIcon();
		}
		canvasGroup.DOKill();
		canvasGroup.DOFade(1f, 5f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
	}

	public void Open(ProductData data, int count, float currentTemperature, float minSpoilageProgress, float maxSpoilageProgress, float minFrozenDamageProgress, float maxFrozenDamageProgress)
	{
		if (data == null)
		{
			productImage.sprite = boxSprite;
			productName.text = Locale.GetWord("empty");
			productCount.text = "0";
		}
		else
		{
			productImage.sprite = data.productSprite;
			productName.text = Locale.GetWord(data.type.ToString());
			productCount.text = count.ToString();
		}
		temperatureNeedRow.SetActive(value: false);
		spoilageRow.SetActive(value: false);
		discardRow.SetActive(value: false);
		if (data != null && data.storageRequirement != StorageRequirement.None)
		{
			int num = BoxManager.storageRequirementToMinDegree[data.storageRequirement];
			int num2 = BoxManager.storageRequirementToMaxDegree[data.storageRequirement];
			bool active = currentTemperature < (float)num - Mathf.Epsilon;
			bool active2 = currentTemperature > (float)num2 + Mathf.Epsilon;
			bool flag = currentTemperature >= (float)num - Mathf.Epsilon && currentTemperature <= (float)num2 + Mathf.Epsilon;
			temperatureNeedRow.SetActive(value: true);
			hotIcon.gameObject.SetActive(active2);
			okIcon.gameObject.SetActive(flag);
			coldIcon.gameObject.SetActive(active);
			string word = Locale.GetWord("storage_" + data.storageRequirement);
			if (!flag)
			{
				temperatureNeedText.text = Locale.GetWord("needs_n").Replace("{0}", word);
			}
			else
			{
				temperatureNeedText.text = Locale.GetWord("n_ok").Replace("{0}", word);
			}
			if (maxFrozenDamageProgress > maxSpoilageProgress)
			{
				spoilageRow.SetActive(value: true);
				spoilageText.text = Locale.GetWord("frozen_damage_label");
				int a = (int)(maxFrozenDamageProgress * 100f);
				a = Mathf.Min(a, 100);
				int a2 = (int)(minFrozenDamageProgress * 100f);
				a2 = Mathf.Min(a2, 100);
				spoilageValue.text = a2 + "%-" + a + "%";
				if (a == a2)
				{
					spoilageValue.text = a2 + "%";
				}
				else
				{
					spoilageValue.text = a2 + "%-" + a + "%";
				}
			}
			else
			{
				spoilageRow.SetActive(value: true);
				if (data.storageRequirement == StorageRequirement.Freezer)
				{
					spoilageText.text = Locale.GetWord("thawing_damage_label");
				}
				else
				{
					spoilageText.text = Locale.GetWord("spoilage_damage_label");
				}
				int a3 = (int)(maxSpoilageProgress * 100f);
				a3 = Mathf.Min(a3, 100);
				int a4 = (int)(minSpoilageProgress * 100f);
				a4 = Mathf.Min(a4, 100);
				if (a3 == a4)
				{
					spoilageValue.text = a4 + "%";
				}
				else
				{
					spoilageValue.text = a4 + "%-" + a3 + "%";
				}
			}
			AnimateActiveIcon();
		}
		canvasGroup.DOKill();
		canvasGroup.DOFade(1f, 5f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
	}

	private void AnimateActiveIcon()
	{
		if (hotIcon.gameObject.activeSelf)
		{
			AnimateFade(hotIcon);
		}
		else if (coldIcon.gameObject.activeSelf)
		{
			AnimateFade(coldIcon);
		}
	}

	private void AnimateFade(Image icon)
	{
		icon.DOKill();
		icon.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
			.OnKill(delegate
			{
				Color color = icon.color;
				color.a = 1f;
				icon.color = color;
			});
	}

	public void Close()
	{
		canvasGroup.DOKill();
		canvasGroup.DOFade(0f, 5f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
		hotIcon.DOKill();
		coldIcon.DOKill();
	}
}
