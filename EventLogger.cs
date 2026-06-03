using Steamworks;
using UnityEngine;

public class EventLogger
{
	public static void LogEvent(string eventName)
	{
	}

	public static void LogAchievement(string achievementId)
	{
		if (SteamManager.Initialized && !HasAchievement(achievementId))
		{
			SteamUserStats.SetAchievement(achievementId);
			PlayerPrefs.SetInt(achievementId, 1);
			SteamUserStats.StoreStats();
		}
	}

	public static void LogCheckout(int customerCount)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetStat("STAT_SUCCESSFUL_CHECKOUTS", customerCount);
			if (customerCount == 50)
			{
				SteamUserStats.IndicateAchievementProgress("ACHIEVEMENT_SUCCESSFUL_CHECKOUTS_02", 50u, 100u);
			}
			if (customerCount == 500)
			{
				SteamUserStats.IndicateAchievementProgress("ACHIEVEMENT_SUCCESSFUL_CHECKOUTS_03", 500u, 1000u);
			}
			if (customerCount >= 10)
			{
				SteamUserStats.SetAchievement("ACHIEVEMENT_SUCCESSFUL_CHECKOUTS_01");
			}
			if (customerCount >= 100)
			{
				SteamUserStats.SetAchievement("ACHIEVEMENT_SUCCESSFUL_CHECKOUTS_02");
			}
			if (customerCount >= 1000)
			{
				SteamUserStats.SetAchievement("ACHIEVEMENT_SUCCESSFUL_CHECKOUTS_03");
			}
			SteamUserStats.StoreStats();
		}
	}

	public static void ReachBalance1k()
	{
		LogAchievement("REACH_BALANCE_01");
	}

	public static bool HasAchievement(string achievementId)
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		if (!SteamUserStats.GetAchievement(achievementId, out var pbAchieved))
		{
			return false;
		}
		return pbAchieved;
	}

	public static void ReachBalance5k()
	{
		LogAchievement("REACH_BALANCE_02");
	}

	public static void ReachBalance25k()
	{
		LogAchievement("REACH_BALANCE_03");
	}

	public static void StorageUnlocked()
	{
		LogAchievement("STORAGE_UNLOCK");
	}

	public static void FirstDockDelivered()
	{
		LogAchievement("FIRST_DOCK_DELIVERY");
	}

	public static void FullExpansionWarehouse()
	{
		LogAchievement("FULL_EXPANSION_STORAGE");
	}

	public static void ForkliftFirstTime()
	{
		LogAchievement("FORKLIFT_FIRSTTIME");
	}

	public static void FirstEmployee()
	{
		LogAchievement("FIRST_EMPLOYEE");
	}

	public static void FiveEmployees()
	{
		LogAchievement("FIVE_EMPLOYEE");
	}

	public static void AllLicensesOneDepartment()
	{
		LogAchievement("ALL_LICENSES_ONE_DEPARTMENT");
	}

	public static void ExcessChangeCash()
	{
		LogAchievement("EXCESS_CHANGE_CASH");
	}

	public static void PennyChanges10()
	{
		LogAchievement("PENNY_CHANGE_10");
	}

	public static void ExcessPrice50()
	{
		LogAchievement("EXCESS_PRICE_50");
	}

	public static void SecondFloorUnlocked()
	{
		LogAchievement("UNLOCK_SECOND_FLOOR");
	}
}
