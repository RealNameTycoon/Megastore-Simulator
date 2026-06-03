using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
	private static Dictionary<string, Delegate> events = new Dictionary<string, Delegate>();

	private static Dictionary<string, Action> unparametrizedEvents = new Dictionary<string, Action>();

	private static bool IsCompatible(Action a1, Action a2)
	{
		return a1.GetType() == a2.GetType();
	}

	private static bool IsCompatible<T>(Action<T> a1, Action<T> a2)
	{
		return a1.GetType() == a2.GetType();
	}

	private static bool IsCompatible<T, U>(Action<T, U> a1, Action<T, U> a2)
	{
		return a1.GetType() == a2.GetType();
	}

	private static bool WasRegistered(string eventName)
	{
		return events.ContainsKey(eventName);
	}

	public static void AddListener(string eventName, Action action)
	{
		if (unparametrizedEvents.ContainsKey(eventName))
		{
			Dictionary<string, Action> dictionary = unparametrizedEvents;
			dictionary[eventName] = (Action)Delegate.Combine(dictionary[eventName], action);
		}
		else
		{
			unparametrizedEvents[eventName] = action;
		}
	}

	public static void RemoveListener(string eventName, Action action)
	{
		Dictionary<string, Action> dictionary = unparametrizedEvents;
		dictionary[eventName] = (Action)Delegate.Remove(dictionary[eventName], action);
		if (unparametrizedEvents[eventName] == null)
		{
			unparametrizedEvents.Remove(eventName);
		}
	}

	public static void RemoveAllListeners()
	{
		unparametrizedEvents.Clear();
		events.Clear();
	}

	public static void NotifyEvent(string eventName)
	{
		if (unparametrizedEvents.ContainsKey(eventName))
		{
			unparametrizedEvents[eventName]?.Invoke();
		}
	}

	public static void AddListener<T>(string eventName, Action<T> action)
	{
		if (events.ContainsKey(eventName))
		{
			if (!IsCompatible((Action<T>)events[eventName], action))
			{
				Debug.LogError("Incompatible delegate types to combine");
			}
			else
			{
				events[eventName] = Delegate.Combine(events[eventName], action);
			}
		}
		else
		{
			events[eventName] = action;
		}
	}

	public static void RemoveListener<T>(string eventName, Action<T> action)
	{
		if (!IsCompatible((Action<T>)events[eventName], action))
		{
			Debug.LogError("Incompatible delegate types to remove");
			return;
		}
		events[eventName] = (Action<T>)Delegate.Remove((Action<T>)events[eventName], action);
		if ((object)events[eventName] == null)
		{
			events.Remove(eventName);
		}
	}

	public static void NotifyEvent<T>(string eventName, T param1)
	{
		if (WasRegistered(eventName))
		{
			events[eventName]?.DynamicInvoke(param1);
		}
	}

	public static void AddListener<T, U>(string eventName, Action<T, U> action)
	{
		if (events.ContainsKey(eventName))
		{
			if (!IsCompatible((Action<T, U>)events[eventName], action))
			{
				Debug.LogError("Incompatible delegate types to combine");
			}
			else
			{
				events[eventName] = Delegate.Combine(events[eventName], action);
			}
		}
		else
		{
			events[eventName] = action;
		}
	}

	public static void RemoveListener<T, U>(string eventName, Action<T, U> action)
	{
		if (!IsCompatible((Action<T, U>)events[eventName], action))
		{
			Debug.LogError("Incompatible delegate types to remove");
			return;
		}
		events[eventName] = (Action<T, U>)Delegate.Remove((Action<T, U>)events[eventName], action);
		if ((object)events[eventName] == null)
		{
			events.Remove(eventName);
		}
	}

	public static void NotifyEvent<T, U>(string eventName, T param1, U param2)
	{
		if (WasRegistered(eventName))
		{
			Delegate[] array = events[eventName]?.GetInvocationList();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DynamicInvoke(param1, param2);
			}
		}
	}

	public static void NotifyProductAddedEvent(Shelf param1, ProductType param2)
	{
		if (WasRegistered(PlaceableEvents.PRODUCT_ADDED))
		{
			((Action<Shelf, ProductType>)events[PlaceableEvents.PRODUCT_ADDED])?.Invoke(param1, param2);
		}
	}

	public static void NotifyProductRemovedEvent(Shelf param1, ProductType param2)
	{
		if (WasRegistered(PlaceableEvents.PRODUCT_REMOVED))
		{
			((Action<Shelf, ProductType>)events[PlaceableEvents.PRODUCT_REMOVED])?.Invoke(param1, param2);
		}
	}

	public static void NotifyTrayProductAddedEvent(TrayShelf param1, ProductType param2)
	{
		if (WasRegistered(PlaceableEvents.TRAY_PRODUCT_ADDED))
		{
			((Action<TrayShelf, ProductType>)events[PlaceableEvents.TRAY_PRODUCT_ADDED])?.Invoke(param1, param2);
		}
	}

	public static void NotifyTrayProductRemovedEvent(TrayShelf param1, ProductType param2)
	{
		WasRegistered(PlaceableEvents.TRAY_PRODUCT_REMOVED);
	}
}
