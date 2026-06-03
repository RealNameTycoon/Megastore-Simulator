using System;

public interface AdProvider
{
	void Initialize();

	void ShowBanner();

	void HideBanner();

	void DestroyBanner();

	void ShowInterstitial(Action onCompleteCallback);

	void ShowRewarded(Action onCompleteCallback);

	bool IsRewardedReady();

	bool IsInterstitialReady();

	void LoadInterstitial();

	void ShowAppOpenAdIfReady()
	{
		throw new NotImplementedException();
	}

	bool IsAppOpenAdReady()
	{
		throw new NotImplementedException();
	}

	void LoadAppOpenAd()
	{
		throw new NotImplementedException();
	}
}
