using System;
using System.Collections.Generic;

public class NotificationController : SingletonBehaviour<NotificationController>
{
	public enum NotificationChannel
	{
		Daily,
		Once_AWeek,
		EveryDay,
		Order
	}

	public List<Notification> notifications;

	public List<RepeatedNotification> repeatedNotifications;

	private new void Awake()
	{
		base.Awake();
		NotificationManager.Init();
	}

	private void Start()
	{
		if (SingletonBehaviour<TutorialManager>.Instance.TutorialDone())
		{
			Initialize();
		}
		else
		{
			EventManager.AddListener<int>(TutorialEvents.TUTORIAL_STEP_DONE, OnTutorialStepCompleted);
		}
	}

	public void SetOrderNotification(DateTime time)
	{
		ClearNotification(NotificationChannel.Order);
		if (SingletonBehaviour<TutorialManager>.Instance.TutorialDone())
		{
			NotificationManager.SetOrderNotification(time);
		}
	}

	private void OnTutorialStepCompleted(int tutorialStep)
	{
		if (tutorialStep == 14)
		{
			Initialize();
		}
	}

	private void LogNotification(string channel)
	{
		EventLogger.LogEvent("c_click_notification_" + channel);
	}

	public void Initialize()
	{
		ClearAllNotifications();
		NotificationManager.SetCustomNotifications();
	}

	public void RequestNotificationPermission()
	{
		StartCoroutine(NotificationManager.RequestNotificationPermission());
	}

	public void AddDailyNotification(string title, string text, string smallIcon, string largeIcon, int day, int hour, int minute)
	{
		Notification item = new Notification(title, text, smallIcon, largeIcon, day, hour, minute);
		notifications.Add(item);
	}

	public void AddRepeatedNotification(string title, string text, string smallIcon, string largeIcon, DayOfWeek dayOfWeek, int hour, int minute)
	{
		RepeatedNotification item = new RepeatedNotification(title, text, smallIcon, largeIcon, hour, minute, dayOfWeek);
		repeatedNotifications.Add(item);
	}

	public void OnNotificationStatusUpdated()
	{
		NotificationManager.ClearAllNotifications();
	}

	public void SetCustomNotification(NotificationChannel channel, string title, string text, string smallIcon, string largeIcon, DateTime fireTime)
	{
		NotificationManager.SetCustomNotification(channel, title, text, smallIcon, largeIcon, fireTime);
	}

	public void ClearAllNotifications()
	{
		NotificationManager.ClearAllNotifications();
	}

	public void ClearNotification(NotificationChannel channel)
	{
		NotificationManager.ClearNotification(channel.ToString());
	}
}
