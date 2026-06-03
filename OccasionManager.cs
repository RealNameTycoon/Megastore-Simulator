using System;
using System.Collections.Generic;
using UnityEngine;

public class OccasionManager : MonoBehaviour
{
	private Dictionary<string, int> occasionEventToPossibility = new Dictionary<string, int>
	{
		{ "CHECKOUT_LONG_WAIT", 35 },
		{ "REGISTER_GREETING", 75 },
		{ "PRICE_ADJUSTMENT_WAITING", 100 },
		{ "BAKERY_FRESHNESS_CONCERN", 50 },
		{ "ITEM_OUT_OF_STOCK", 50 },
		{ "PRICE_COMPLAINT", 50 }
	};

	private void Start()
	{
		EventManager.AddListener("CHECKOUT_LONG_WAIT", delegate(Customer customer)
		{
			TryPlayOccasionAudio(customer, "CHECKOUT_LONG_WAIT".ToString());
		});
		EventManager.AddListener("REGISTER_GREETING", delegate(Customer customer)
		{
			TryPlayOccasionAudio(customer, "REGISTER_GREETING".ToString());
		});
		EventManager.AddListener("PRICE_ADJUSTMENT_WAITING", delegate(Customer customer)
		{
			TryPlayOccasionAudio(customer, "PRICE_ADJUSTMENT_WAITING".ToString());
		});
		EventManager.AddListener("BAKERY_FRESHNESS_CONCERN", delegate(Customer customer)
		{
			TryPlayOccasionAudio(customer, "BAKERY_FRESHNESS_CONCERN".ToString());
		});
		EventManager.AddListener("ITEM_OUT_OF_STOCK", delegate(Customer customer)
		{
			TryPlayOccasionAudio(customer, "ITEM_OUT_OF_STOCK".ToString());
		});
		EventManager.AddListener("PRICE_COMPLAINT", delegate(Customer customer)
		{
			TryPlayOccasionAudio(customer, "PRICE_COMPLAINT".ToString());
		});
	}

	private void TryPlayOccasionAudio(Customer customer, string eventName)
	{
		if (UnityEngine.Random.Range(0, 100) <= occasionEventToPossibility[eventName])
		{
			PlayOccasionFor(eventName, customer);
		}
	}

	private void OnCheckoutLongWait(Customer customer)
	{
		if (UnityEngine.Random.Range(0, 100) <= occasionEventToPossibility["CHECKOUT_LONG_WAIT".ToString()])
		{
			PlayOccasionFor("CHECKOUT_LONG_WAIT", customer);
		}
	}

	private void OnRegisterGreeting(Customer customer)
	{
		if (UnityEngine.Random.Range(0, 100) <= occasionEventToPossibility["REGISTER_GREETING".ToString()])
		{
			PlayOccasionFor("REGISTER_GREETING", customer);
		}
	}

	private void OnPriceAdjusting(Customer customer)
	{
		if (UnityEngine.Random.Range(0, 100) <= occasionEventToPossibility["PRICE_ADJUSTMENT_WAITING".ToString()])
		{
			PlayOccasionFor("PRICE_ADJUSTMENT_WAITING", customer);
		}
	}

	private void PlayOccasionFor(string occasionEvent, Customer customer)
	{
		if (!SingletonBehaviour<AudioManager>.Instance.CanPlaySpeech())
		{
			return;
		}
		AudioManager.OccasionalAudioTypes? occasionAudio = GetOccasionAudio(customer.IsFemale, occasionEvent);
		if (occasionAudio.HasValue)
		{
			AudioClip occasionalAudioClip = SingletonBehaviour<AudioManager>.Instance.GetOccasionalAudioClip(occasionAudio.Value);
			if (!(occasionalAudioClip == null))
			{
				customer.PlayAudio(occasionalAudioClip);
			}
		}
	}

	private AudioManager.OccasionalAudioTypes? GetOccasionAudio(bool female, string occasionEvent)
	{
		if (TryParseAndHasClips(BuildGendered(occasionEvent, female), out var type))
		{
			return type;
		}
		return null;
	}

	private static string BuildGendered(string baseToken, bool female)
	{
		return baseToken + "_" + (female ? "FEMALE" : "MALE");
	}

	private bool TryParseAndHasClips(string enumName, out AudioManager.OccasionalAudioTypes type)
	{
		if (Enum.TryParse<AudioManager.OccasionalAudioTypes>(enumName, out type) && SingletonBehaviour<AudioManager>.Instance.GetOccasionalAudioCount(type) > 0)
		{
			return true;
		}
		return false;
	}
}
