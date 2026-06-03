using UnityEngine;

public class OfferManager : SingletonBehaviour<OfferManager>
{
	[SerializeField]
	private OfferWindow offerWindow;

	[SerializeField]
	private GameObject offerButton;

	private string offerPurchasedKey = "offerPurchasedKey";

	public void Initialize()
	{
	}

	public void OnOfferPurchased()
	{
		GenericDataSerializer.SaveBool(offerPurchasedKey, value: true);
		DeactivateOffer();
	}

	public bool IsOfferPurchased()
	{
		if (!GenericDataSerializer.HasKey(offerPurchasedKey))
		{
			return AdManager.Instance.RemoveAdsPurchased;
		}
		return true;
	}

	public void ActivateOffer()
	{
	}

	public void ShowOffer()
	{
	}

	private void DeactivateOffer()
	{
		if (offerWindow.IsOpen())
		{
			offerWindow.Close();
		}
		offerButton.SetActive(value: false);
	}

	public void OnRemoveAdsPurchased()
	{
		DeactivateOffer();
	}

	private void Update()
	{
	}
}
