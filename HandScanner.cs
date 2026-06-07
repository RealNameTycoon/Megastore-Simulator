using System;
using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HandScanner : HotkeyClickable
{
	[SerializeField]
	private Transform usingPosition;

	[SerializeField]
	private AudioSource handScannerAudioSource;

	[SerializeField]
	private ScannerCartWindow scannerCartWindow;

	[SerializeField]
	private Light spotLight;

	[SerializeField]
	private SpriteRenderer productSpriteRenderer;

	[SerializeField]
	private TextMeshPro orderTitleText;

	[SerializeField]
	private TextMeshPro priceText;

	[SerializeField]
	private TextMeshPro boxAmountText;

	private bool isUsing;

	private const float SPOT_LIGHT_INTENSITY = 4f;

	public static HandScanner Instance { get; protected set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		base.transform.position = putDownPosition.position;
		base.transform.eulerAngles = putDownPosition.eulerAngles;
		orderTitleText.text = Locale.GetWord("order_title");
		EventManager.AddListener<ProductType>(ProductEvents.PRICE_TAG_HOVER_STARTED, OnPriceTagHoverStarted);
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		base.gameObject.SetActive(value: false);
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

	private void OnLanguageChanged()
	{
		orderTitleText.text = Locale.GetWord("order_title");
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void ClearScannerUI()
	{
		productSpriteRenderer.gameObject.SetActive(value: false);
		orderTitleText.text = "";
		boxAmountText.text = "";
		priceText.text = "";
	}

	private void RepaintScannerUI(ProductType type)
	{
		ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(type);
		SetSpriteKeepSize(productSpriteRenderer, anyProductData.productSprite);
		if (!productSpriteRenderer.gameObject.activeSelf)
		{
			productSpriteRenderer.gameObject.SetActive(value: true);
		}
		boxAmountText.text = anyProductData.GetMaxProductCount().ToString();
		priceText.text = anyProductData.TotalCost().ToString("0.00", CultureInfo.InvariantCulture);
	}

	private void OnPriceTagHoverStarted(ProductType type)
	{
		RepaintScannerUI(type);
		if (scannerCartWindow.IsOpen() && base.IsPicked)
		{
			ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(type);
			if (anyProductData != null)
			{
				SingletonBehaviour<ProductInfoWindow>.Instance.Open(anyProductData);
			}
		}
	}

	public override void PickUp()
	{
		isUsing = false;
		base.PickUp();
		scannerCartWindow.Open();
	}

	private void MoveToPlayerHolding()
	{
		base.transform.DOKill();
		base.transform.DOLocalMove(pickUpPosition.localPosition, GetPickUpSpeed()).SetSpeedBased(isSpeedBased: true);
		base.transform.DOLocalRotate(pickUpPosition.localEulerAngles, GetPickUpSpeedRotation()).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
		{
		});
	}

	public override void OnPickedUp()
	{
		UpdateMenu();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	public override void PutDown()
	{
		base.PutDown();
		scannerCartWindow.Close();
		SingletonBehaviour<ProductInfoWindow>.Instance.Close();
	}

	public void UpdateMenu()
	{
		if (base.IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.V,
					("scanner_clear", delegate
					{
						scannerCartWindow.OnClearScannerCart();
					})
				},
				{
					KeyCode.Tab,
					("scanner_change_dock", delegate
					{
						scannerCartWindow.NextDeliveryArea();
					})
				},
				{
					KeyCode.E,
					("scanner_purchase", delegate
					{
						scannerCartWindow.OnPurchaseProducts();
					})
				},
				{
					KeyCode.LeftShift,
					("scanner_add_bulk", null)
				},
				{
					KeyCode.Mouse1,
					("scanner_remove", null)
				},
				{
					KeyCode.Mouse0,
					("scanner_add", null)
				}
			});
			ClearScannerUI();
		}
	}

	public override void RepaintButtonsForEndHover()
	{
		UpdateMenu();
		SingletonBehaviour<ProductInfoWindow>.Instance.Close();
	}

	public void Use(Transform lookAtTarget = null)
	{
		if (isUsing)
		{
			return;
		}
		isUsing = true;
		base.transform.DOKill();
		spotLight.DOKill();
		spotLight.enabled = false;
		base.transform.localPosition = pickUpPosition.localPosition;
		base.transform.DOLocalMove(usingPosition.localPosition, 0.15f).SetEase(Ease.OutSine).OnComplete(delegate
		{
			handScannerAudioSource.PlayOneShot(SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.HAND_SCANNER_BEEP));
			spotLight.intensity = 4f;
			spotLight.enabled = true;
			spotLight.DOIntensity(0f, 0.2f).SetEase(Ease.OutSine).SetDelay(0.2f)
				.OnComplete(delegate
				{
					spotLight.enabled = false;
				});
			isUsing = false;
			base.transform.DOLocalMove(pickUpPosition.localPosition, 0.15f).SetDelay(0.2f);
		});
		base.transform.DOLookAt(lookAtTarget.position, 0.15f).SetEase(Ease.OutSine).OnComplete(delegate
		{
			base.transform.DOLocalRotate(pickUpPosition.localEulerAngles, 0.15f).SetDelay(0.2f);
		});
	}

	public override void Reset()
	{
		base.Reset();
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		isUsing = false;
		spotLight.DOKill();
		spotLight.enabled = false;
	}

	public override LayerMask GetInteractableLayers()
	{
		return 1 << RayShooter.CLICKABLE_LAYER;
	}

	public List<string> GetInteractableTags()
	{
		return new List<string> { RayShooter.PRICE_LABEL_TAG };
	}

	public override void RepaintButtonsForInteractable(Interactable interactable)
	{
		if (interactable.gameObject.tag == RayShooter.PRICE_LABEL_TAG)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.V,
					("scanner_clear", delegate
					{
						scannerCartWindow.OnClearScannerCart();
					})
				},
				{
					KeyCode.Tab,
					("scanner_change_dock", delegate
					{
						scannerCartWindow.NextDeliveryArea();
					})
				},
				{
					KeyCode.E,
					("scanner_purchase", delegate
					{
						scannerCartWindow.OnPurchaseProducts();
					})
				},
				{
					KeyCode.LeftShift,
					("scanner_add_bulk", null)
				},
				{
					KeyCode.Mouse1,
					("scanner_remove", delegate
					{
						Use(interactable.transform);
					})
				},
				{
					KeyCode.Mouse0,
					("scanner_add", delegate
					{
						Use(interactable.transform);
					})
				}
			});
		}
	}
}
