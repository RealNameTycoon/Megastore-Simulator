using System.Collections.Generic;

public class PlayerSittingManager : SingletonBehaviour<PlayerSittingManager>
{
	private bool isSittingOnChoppingStand;

	private bool isSittingOnCheckoutDesk;

	public bool IsSittingOnChoppingStand => isSittingOnChoppingStand;

	public bool IsSittingOnCheckoutDesk => isSittingOnCheckoutDesk;

	private new void Awake()
	{
		base.Awake();
		EventManager.AddListener(PlaceableEvents.SIT_ON_CHOPPING_STAND, OnSitOnChoppingStand);
		EventManager.AddListener(PlaceableEvents.LEFT_CHOPPING_STAND, OnLeftChoppingStand);
		EventManager.AddListener(PaymentEvents.SIT_ON_CHECKOUT_DESK, OnSitOnCheckoutDesk);
		EventManager.AddListener(PaymentEvents.LEFT_CHECKOUT_DESK, OnLeftCheckoutDesk);
	}

	public List<string> GetChoppingStandInteractableTags()
	{
		return new List<string> { RayShooter.CHOPPING_STAND_TAG };
	}

	public List<string> GetCheckoutInteractableTags()
	{
		return new List<string> { RayShooter.CHECKOUT_CLICKABLE_TAG };
	}

	private void OnSitOnChoppingStand()
	{
		isSittingOnChoppingStand = true;
	}

	private void OnLeftChoppingStand()
	{
		isSittingOnChoppingStand = false;
	}

	private void OnSitOnCheckoutDesk()
	{
		isSittingOnCheckoutDesk = true;
	}

	private void OnLeftCheckoutDesk()
	{
		isSittingOnCheckoutDesk = false;
	}
}
