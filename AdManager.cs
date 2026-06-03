using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
	[SerializeField]
	private GameObject bannerObject;

	[SerializeField]
	private Canvas blockerCanvas;

	[SerializeField]
	public bool IsTest;

	private const string REMOVE_ADS_PURCHASED_KEY = "removeAdsPurchased";

	private const string NO_ADS_OFFER_PURCHASED_KEY = "noAdsOfferPurchased";

	private int appOpenBackgroundCount = -1;

	private bool removeAdsPurchased;

	private bool noAdsOfferPurchased;

	private float lastInterstitialShowTime;

	private float lastAppOpenShowTime;

	public bool RemoveAdsPurchased => removeAdsPurchased;

	public bool NoAdsOfferPurchased
	{
		get
		{
			if (IsTest)
			{
				return true;
			}
			return noAdsOfferPurchased;
		}
	}

	public bool AdsRemoved
	{
		get
		{
			if (!removeAdsPurchased)
			{
				return noAdsOfferPurchased;
			}
			return true;
		}
	}

	public static AdManager Instance { get; protected set; }

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	protected void Start()
	{
		Initialize();
	}

	public void OnRemoveAdsPurchased()
	{
		bannerObject.SetActive(value: false);
		PlayerPrefs.SetInt("removeAdsPurchased", 1);
		removeAdsPurchased = true;
	}

	private void OnNoAdsOfferPurchased()
	{
		PlayerPrefs.SetInt("noAdsOfferPurchased", 1);
		noAdsOfferPurchased = true;
		HideBanner();
	}

	private void Initialize()
	{
		removeAdsPurchased = PlayerPrefs.HasKey("removeAdsPurchased");
		if (IsTest)
		{
			noAdsOfferPurchased = true;
		}
		else
		{
			noAdsOfferPurchased = PlayerPrefs.HasKey("noAdsOfferPurchased");
		}
	}

	public void ShowBanner()
	{
	}

	public void HideBanner()
	{
		if (!AdsRemoved)
		{
			bannerObject.SetActive(value: false);
		}
	}

	public void ShowInterstitial(Action onCompleteCallback)
	{
	}

	public void ShowRewarded(Action onCompleteCallback)
	{
	}
}
