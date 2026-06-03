using System;
using UnityEngine;

[Serializable]
public class Notification
{
	public string title;

	public string text;

	public string smallIcon;

	public string largeIcon;

	public int day;

	[Range(0f, 23f)]
	public int hour;

	[Range(0f, 59f)]
	public int minute;

	public Notification(string title, string text, string smallIcon, string largeIcon, int day, int hour, int minute)
	{
		this.title = title;
		this.text = text;
		this.smallIcon = smallIcon;
		this.largeIcon = largeIcon;
		this.day = day;
		this.hour = hour;
		this.minute = minute;
	}
}
