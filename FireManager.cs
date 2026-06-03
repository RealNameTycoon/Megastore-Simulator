using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireManager : SingletonBehaviour<FireManager>
{
	[SerializeField]
	private GameObject fireWarningUI;

	[SerializeField]
	private DOTweenAnimation warningAnimation;

	[SerializeField]
	private TextMeshProUGUI fireProtectionOwnedText;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private Button purchaseButton;

	private const string FIRE_DAY_KEY = "FIRE_DAY_KEY";

	private const string FIRE_MIN_KEY = "FIRE_MIN_KEY";

	private const string BURNING_PLACEABLE_ID_KEY = "BURNING_PLACEABLE_ID_KEY";

	private const string BURNING_PLACEABLE_TYPE_KEY = "BURNING_PLACEABLE_TYPE_KEY";

	private const string FIRE_COUNT_KEY = "FIRE_COUNT_KEY";

	private int extinguishedFireCount;

	private int fireDay = -1;

	private int fireMin = -1;

	private Placeable burningPlaceable;

	private int burningPlaceableID = -1;

	private PlaceableType burningPlaceableType = PlaceableType.NONE;

	private bool fireProtectionPurchased;

	private string FIRE_PROTECTION_PURCHASED_KEY = "FIRE_PROTECTION_PURCHASED_KEY";

	public bool IsFireActive => burningPlaceableID != -1;

	public void StartFire()
	{
		burningPlaceable = SingletonBehaviour<SpawnManager>.Instance.GetRandomPlaceableToBurn();
		burningPlaceable.StartFire();
		fireWarningUI.gameObject.SetActive(value: true);
		warningAnimation.DORestart();
		SetBurningPlaceable(burningPlaceable);
		EventManager.NotifyEvent(SupermarketEvents.FIRE_STARTED);
	}

	private new void Awake()
	{
		base.Awake();
		fireProtectionPurchased = GenericDataSerializer.LoadBool(FIRE_PROTECTION_PURCHASED_KEY);
		if (fireProtectionPurchased)
		{
			fireProtectionOwnedText.enabled = true;
			purchaseButton.gameObject.SetActive(value: false);
			priceText.enabled = false;
		}
		fireProtectionPurchased = true;
		fireDay = GenericDataSerializer.LoadInt("FIRE_DAY_KEY", -1);
		fireMin = GenericDataSerializer.LoadInt("FIRE_MIN_KEY", -1);
		burningPlaceableID = GenericDataSerializer.LoadInt("BURNING_PLACEABLE_ID_KEY", -1);
		burningPlaceableType = GenericDataSerializer.Load("BURNING_PLACEABLE_TYPE_KEY", PlaceableType.NONE);
		extinguishedFireCount = GenericDataSerializer.LoadInt("FIRE_COUNT_KEY");
	}

	private void Start()
	{
		if (!fireProtectionPurchased)
		{
			EventManager.AddListener<PlaceableType>(PlaceableEvents.NEW_PLACEABLE_PLACED, OnNewPlaceablePlaced);
			StartCoroutine(Initialize());
		}
	}

	public void OnProtectionPurchased()
	{
		fireProtectionPurchased = true;
		GenericDataSerializer.SaveBool(FIRE_PROTECTION_PURCHASED_KEY, value: true);
		fireProtectionOwnedText.enabled = true;
		purchaseButton.gameObject.SetActive(value: false);
		priceText.enabled = false;
		if (IsFireActive)
		{
			StopFireEmitting();
			StopFire();
		}
		TimeManager.OnMinPassed = (Action)Delegate.Remove(TimeManager.OnMinPassed, new Action(OnMinPassed));
	}

	public bool IsBurning(Placeable placeable)
	{
		if (burningPlaceable != null && placeable.PlaceableID == burningPlaceable.PlaceableID)
		{
			return placeable.Type == burningPlaceable.Type;
		}
		return false;
	}

	private IEnumerator Initialize()
	{
		yield return new WaitForEndOfFrame();
		if (IsFireActive)
		{
			burningPlaceable = SingletonBehaviour<SpawnManager>.Instance.GetPlaceableById(burningPlaceableType, burningPlaceableID);
			if (burningPlaceable == null || !burningPlaceable.IsBurnable)
			{
				ClearBurningPlaceable();
				SetNewFireDay();
				yield return null;
			}
			else
			{
				burningPlaceable.StartFire();
				fireWarningUI.gameObject.SetActive(value: true);
				warningAnimation.DORestart();
				yield return null;
			}
		}
		if (fireDay == -1 && SingletonBehaviour<SpawnManager>.Instance.HasBurnable())
		{
			InitializeFireDay();
		}
		else if (fireDay != -1)
		{
			TimeManager.OnMinPassed = (Action)Delegate.Combine(TimeManager.OnMinPassed, new Action(OnMinPassed));
		}
	}

	private void InitializeFireDay()
	{
		int currentDay = SingletonBehaviour<TimeManager>.Instance.CurrentDay;
		fireDay = currentDay + 1;
		fireMin = UnityEngine.Random.Range(360, 720);
		GenericDataSerializer.SaveInt("FIRE_DAY_KEY", fireDay);
		GenericDataSerializer.SaveInt("FIRE_MIN_KEY", fireMin);
		TimeManager.OnMinPassed = (Action)Delegate.Combine(TimeManager.OnMinPassed, new Action(OnMinPassed));
	}

	private void OnNewPlaceablePlaced(PlaceableType type)
	{
		if (fireDay == -1 && (type == PlaceableType.FRIDGE || type == PlaceableType.FREEZER || type == PlaceableType.WIDE_FRIGE || type == PlaceableType.VENDING_MACHINE))
		{
			InitializeFireDay();
		}
	}

	private void OnMinPassed()
	{
		if (SingletonBehaviour<TimeManager>.Instance.CurrentDay == fireDay && SingletonBehaviour<TimeManager>.Instance.CurrentMin == fireMin && !fireProtectionPurchased)
		{
			burningPlaceable = SingletonBehaviour<SpawnManager>.Instance.GetRandomPlaceableToBurn();
			if (burningPlaceable == null)
			{
				ClearBurningPlaceable();
				SetNewFireDay();
				return;
			}
			burningPlaceable.StartFire();
			fireWarningUI.gameObject.SetActive(value: true);
			warningAnimation.DORestart();
			SetBurningPlaceable(burningPlaceable);
			EventManager.NotifyEvent(SupermarketEvents.FIRE_STARTED);
		}
	}

	public void StopFireEmitting()
	{
		burningPlaceable.StopFire();
	}

	public void StopFire()
	{
		fireWarningUI.gameObject.SetActive(value: false);
		warningAnimation.DOPause();
		EventManager.NotifyEvent(SupermarketEvents.FIRE_STOPPED);
		extinguishedFireCount++;
		GenericDataSerializer.SaveInt("FIRE_COUNT_KEY", extinguishedFireCount);
		ClearBurningPlaceable();
		SetNewFireDay();
		EventLogger.LogEvent("c_fire_extinguished");
	}

	private void SetNewFireDay()
	{
		int currentDay = SingletonBehaviour<TimeManager>.Instance.CurrentDay;
		if (extinguishedFireCount == 1)
		{
			fireDay = currentDay + 2;
		}
		else
		{
			fireDay = currentDay + 3;
		}
		fireMin = UnityEngine.Random.Range(360, 720);
		GenericDataSerializer.SaveInt("FIRE_DAY_KEY", fireDay);
		GenericDataSerializer.SaveInt("FIRE_MIN_KEY", fireMin);
	}

	private void SetBurningPlaceable(Placeable placeable)
	{
		burningPlaceableID = placeable.PlaceableID;
		GenericDataSerializer.SaveInt("BURNING_PLACEABLE_ID_KEY", placeable.PlaceableID);
		burningPlaceableType = placeable.Type;
		GenericDataSerializer.Save("BURNING_PLACEABLE_TYPE_KEY", placeable.Type);
	}

	private void ClearBurningPlaceable()
	{
		burningPlaceable = null;
		burningPlaceableID = -1;
		GenericDataSerializer.SaveInt("BURNING_PLACEABLE_ID_KEY", -1);
		burningPlaceableType = PlaceableType.NONE;
		GenericDataSerializer.Save("BURNING_PLACEABLE_TYPE_KEY", PlaceableType.NONE);
	}

	private void Update()
	{
	}
}
