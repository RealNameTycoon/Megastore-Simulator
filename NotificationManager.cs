using System;
using System.Collections;
using DFTGames.Localization;
using UnityEngine;
using UnityEngine.Events;

public class NotificationManager
{
	public static NotificationManager Instance;

	private static int apiLevel;

	private static int ApiLevel
	{
		get
		{
			if (apiLevel == 0)
			{
				apiLevel = GetAndroidVersion();
			}
			return apiLevel;
		}
	}

	public static IEnumerator RequestNotificationPermission(UnityAction callback = null)
	{
		yield return null;
	}

	public static void Init()
	{
	}

	public static void ClearAllNotifications()
	{
	}

	public static void ClearNotification(string id)
	{
	}

	public static void SetNotifications()
	{
	}

	public static void SetOrderNotification(DateTime spawnTime)
	{
		if ((spawnTime - DateTime.Now).TotalMinutes > 0.0)
		{
			int num = UnityEngine.Random.Range(1, 3);
			RepeatedNotification randomElement = SingletonBehaviour<NotificationController>.Instance.repeatedNotifications.GetRandomElement();
			SetCustomNotification(NotificationController.NotificationChannel.Order, Locale.CurrentLanguageStrings["order_notif_title" + num], Locale.CurrentLanguageStrings["order_notif_desc" + num], randomElement.smallIcon, randomElement.largeIcon, spawnTime);
		}
	}

	public static void SetCustomNotifications()
	{
		ClearNotification(NotificationController.NotificationChannel.EveryDay.ToString());
		for (int i = 0; i < 7; i++)
		{
			_ = DateTime.Now.Date;
			DateTime dateTime = ChangeTime(DateTime.Today.AddDays(i), 21, 0, 0, 0);
			if ((dateTime - DateTime.Now).TotalMinutes > 0.0)
			{
				RepeatedNotification randomElement = SingletonBehaviour<NotificationController>.Instance.repeatedNotifications.GetRandomElement();
				SetCustomNotification(NotificationController.NotificationChannel.EveryDay, randomElement.title, randomElement.text, randomElement.smallIcon, randomElement.largeIcon, dateTime);
			}
		}
	}

	public static DateTime ChangeTime(DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
	{
		return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hours, minutes, seconds, milliseconds, dateTime.Kind);
	}

	public static int CalculateOffset(DayOfWeek current, DayOfWeek desired)
	{
		int num = ((int)(7 - current) + (int)desired) % 7;
		if (num != 0)
		{
			return num;
		}
		return 7;
	}

	public static void SetCustomNotification(NotificationController.NotificationChannel channel, string title, string text, string smallIcon, string largeIcon, DateTime fireTime)
	{
	}

	private static DateTime GetNextWeekday(DayOfWeek weekday)
	{
		DateTime today = DateTime.Today;
		int num = 1;
		return today.AddDays(num);
	}

	private static int GetAndroidVersion()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build$VERSION");
		return androidJavaClass.GetStatic<int>("SDK_INT");
	}
}
