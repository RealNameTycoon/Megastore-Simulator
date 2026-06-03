using System;
using System.Linq;
using System.Reflection;
using BugsnagUnity;
using UnityEngine;

public class BugSnagInitializer : SingletonBehaviour<BugSnagInitializer>
{
	protected override void Awake()
	{
		base.Awake();
		if (HasBepInExLoaded())
		{
			Debug.Log("Bugsnag not initialized because BepInEx was detected.");
		}
		else
		{
			Bugsnag.Start(BugsnagSettingsObject.LoadConfiguration());
		}
	}

	private static bool HasBepInExLoaded()
	{
		try
		{
			return AppDomain.CurrentDomain.GetAssemblies().Any(delegate(Assembly a)
			{
				try
				{
					return (a.GetName().Name ?? "").Contains("BepInEx", StringComparison.OrdinalIgnoreCase);
				}
				catch
				{
					return false;
				}
			});
		}
		catch
		{
			return false;
		}
	}
}
