using System;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class UnityServiceInitializer : SingletonBehaviour<UnityServiceInitializer>
{
	private new void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(this);
		InitServices();
	}

	private async void InitServices()
	{
		try
		{
			await UnityServices.InitializeAsync();
			if (UnityServices.State == ServicesInitializationState.Initialized)
			{
				Debug.Log("Servisler hazır, raporlama aktif.");
				AnalyticsService.Instance.StartDataCollection();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
