using System;
using UnityEngine;

[Serializable]
public class RepeatedNotification
{
	public string title;

	public string text;

	public string smallIcon;

	public string largeIcon;

	[Range(0f, 23f)]
	public int hour;

	[Range(0f, 59f)]
	public int minute;

	public DayOfWeek weekDay;

	public RepeatedNotification(string title, string text, string smallIcon, string largeIcon, int hour, int minute, DayOfWeek weekDay)
	{
		this.title = title;
		this.text = text;
		this.smallIcon = smallIcon;
		this.largeIcon = largeIcon;
		this.hour = hour;
		this.minute = minute;
		this.weekDay = weekDay;
	}
}
