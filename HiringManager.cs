using System;
using System.Collections.Generic;
using RandomNameGen;
using UnityEngine;

public class HiringManager : SingletonBehaviour<HiringManager>
{
	public struct EmployeeStats(string name, EmployeeRole role, int moveSpeed, int workSpeed, int employeeID, int dailyWage, bool isPaused = false, bool returnToRacks = false)
	{
		public int employeeID = employeeID;

		public string employeeName = name;

		public EmployeeRole role = role;

		public int moveSpeed = moveSpeed;

		public int workSpeed = workSpeed;

		public int dailyWage = dailyWage;

		public bool isEarylyShift = true;

		public int checkoutDeskID = -1;

		public bool includeWarehouse = true;

		public ProductGroup department = ProductGroup.NONE;

		public bool isPaused = isPaused;

		public bool returnToRacks = returnToRacks;
	}

	[SerializeField]
	private HiringWindow hiringWindow;

	private string DAILY_EMPLOYEE_STATS_KEY = "DailyEmployeeStats";

	private string SHOULD_ADD_UNLOADER_APPLICATION_KEY = "ShouldAddUnloaderApplicationKey";

	private string SHOULD_ADD_SEAFOOD_STAFF_APPLICATION_KEY = "ShouldAddSeafoodStaffApplicationKey";

	private string SHOULD_CHANGE_RESTOCKER_APPLICATIONS = "SHOULD_CHANGE_RESTOCKER_APPLICATIONS";

	private List<EmployeeStats> employeeApplications = new List<EmployeeStats>();

	private List<Cashier> cashiers = new List<Cashier>();

	private int MIN_CASHIER_APPLICATIONS = 2;

	private int MAX_CASHIER_APPLICATIONS = 5;

	private int MIN_RESTOCKER_APPLICATIONS = 1;

	private int MAX_RESTOCKER_APPLICATIONS = 3;

	private int MIN_BAKERY_STAFF_APPLICATIONS = 1;

	private int MAX_BAKERY_STAFF_APPLICATIONS = 3;

	private int MIN_UNLOADER_APPLICATIONS = 1;

	private int MAX_UNLOADER_APPLICATIONS = 3;

	private int MIN_SEAFOOD_STAFF_APPLICATIONS = 1;

	private int MAX_SEAFOOD_STAFF_APPLICATIONS = 3;

	private RandomName randomName;

	public const int MAX_EMPLOYEE_COUNT = 45;

	public static Dictionary<int, int> EmployeeCountToRequiredLevelMap = new Dictionary<int, int>
	{
		{ 1, 6 },
		{ 2, 8 },
		{ 3, 13 },
		{ 4, 16 },
		{ 5, 21 },
		{ 6, 26 },
		{ 7, 31 },
		{ 8, 36 },
		{ 9, 41 },
		{ 10, 47 },
		{ 11, 52 },
		{ 12, 57 },
		{ 13, 62 },
		{ 14, 67 },
		{ 15, 72 },
		{ 16, 77 },
		{ 17, 82 },
		{ 18, 87 },
		{ 19, 92 },
		{ 20, 97 },
		{ 21, 102 },
		{ 22, 107 },
		{ 23, 112 },
		{ 24, 117 },
		{ 25, 122 },
		{ 26, 127 },
		{ 27, 132 },
		{ 28, 137 },
		{ 29, 142 },
		{ 30, 147 },
		{ 31, 150 },
		{ 32, 155 },
		{ 33, 160 },
		{ 34, 165 },
		{ 35, 170 },
		{ 36, 175 },
		{ 37, 180 },
		{ 38, 185 },
		{ 39, 190 },
		{ 40, 195 },
		{ 41, 200 },
		{ 42, 205 },
		{ 43, 210 },
		{ 44, 215 },
		{ 45, 220 },
		{ 46, 999999 }
	};

	public List<EmployeeStats> EmployeeApplications => employeeApplications;

	private void Start()
	{
		randomName = new RandomName(new System.Random());
		if (!GenericDataSerializer.HasKey(DAILY_EMPLOYEE_STATS_KEY))
		{
			UpdateApplications();
		}
		else
		{
			employeeApplications = GenericDataSerializer.Load(DAILY_EMPLOYEE_STATS_KEY, new List<EmployeeStats>());
			CheckAndAddUnloaderApplication();
			CheckAndAddSeafoodStaffApplication();
			CheckAndChangeRestockerApplications();
			hiringWindow.RefreshApplications();
		}
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
	}

	private void CheckAndAddUnloaderApplication()
	{
		if (!GenericDataSerializer.LoadBool(SHOULD_ADD_UNLOADER_APPLICATION_KEY, value: true))
		{
			return;
		}
		GenericDataSerializer.SaveBool(SHOULD_ADD_UNLOADER_APPLICATION_KEY, value: false);
		for (int i = 0; i < employeeApplications.Count; i++)
		{
			if (employeeApplications[i].role == EmployeeRole.UNLOADER)
			{
				return;
			}
		}
		int num = UnityEngine.Random.Range(MIN_UNLOADER_APPLICATIONS, MAX_UNLOADER_APPLICATIONS + 1);
		List<Unloader> randomAvailableUnloaders = SingletonBehaviour<EmployeeManager>.Instance.GetRandomAvailableUnloaders(num);
		for (int j = 0; j < num; j++)
		{
			string text = randomName.GenerateName(randomAvailableUnloaders[j].GetEmployeeData().sex);
			EmployeeRole role = EmployeeRole.UNLOADER;
			int randomMoveSpeed = GetRandomMoveSpeed();
			int randomWorkSpeed = GetRandomWorkSpeed();
			int randomDailyWage = GetRandomDailyWage();
			EmployeeStats item = new EmployeeStats(text, role, randomMoveSpeed, randomWorkSpeed, randomAvailableUnloaders[j].GetEmployeeID(), randomDailyWage);
			employeeApplications.Add(item);
		}
		GenericDataSerializer.Save(DAILY_EMPLOYEE_STATS_KEY, employeeApplications);
	}

	private void CheckAndChangeRestockerApplications()
	{
		if (!GenericDataSerializer.LoadBool(SHOULD_CHANGE_RESTOCKER_APPLICATIONS, value: true))
		{
			return;
		}
		for (int i = 0; i < employeeApplications.Count; i++)
		{
			if (employeeApplications[i].role == EmployeeRole.RESTOCKER)
			{
				EmployeeStats value = employeeApplications[i];
				value.returnToRacks = true;
				employeeApplications[i] = value;
			}
		}
		GenericDataSerializer.Save(DAILY_EMPLOYEE_STATS_KEY, employeeApplications);
		GenericDataSerializer.SaveBool(SHOULD_CHANGE_RESTOCKER_APPLICATIONS, value: false);
	}

	private void CheckAndAddSeafoodStaffApplication()
	{
		if (!GenericDataSerializer.LoadBool(SHOULD_ADD_SEAFOOD_STAFF_APPLICATION_KEY, value: true))
		{
			return;
		}
		GenericDataSerializer.SaveBool(SHOULD_ADD_SEAFOOD_STAFF_APPLICATION_KEY, value: false);
		for (int i = 0; i < employeeApplications.Count; i++)
		{
			if (employeeApplications[i].role == EmployeeRole.SEAFOOD_STAFF)
			{
				return;
			}
		}
		int num = UnityEngine.Random.Range(MIN_SEAFOOD_STAFF_APPLICATIONS, MAX_SEAFOOD_STAFF_APPLICATIONS + 1);
		List<SeafoodStaff> randomAvailableSeafoodStaff = SingletonBehaviour<EmployeeManager>.Instance.GetRandomAvailableSeafoodStaff(num);
		for (int j = 0; j < num; j++)
		{
			string text = randomName.GenerateName(randomAvailableSeafoodStaff[j].GetEmployeeData().sex);
			EmployeeRole role = EmployeeRole.SEAFOOD_STAFF;
			int randomMoveSpeed = GetRandomMoveSpeed();
			int randomWorkSpeed = GetRandomWorkSpeed();
			int randomDailyWage = GetRandomDailyWage();
			EmployeeStats item = new EmployeeStats(text, role, randomMoveSpeed, randomWorkSpeed, randomAvailableSeafoodStaff[j].GetEmployeeID(), randomDailyWage);
			employeeApplications.Add(item);
		}
		GenericDataSerializer.Save(DAILY_EMPLOYEE_STATS_KEY, employeeApplications);
	}

	private void OnNewDayStarted()
	{
		UpdateApplications();
	}

	public void RemoveApplication(EmployeeStats employeeStats)
	{
		EmployeeStats item = default(EmployeeStats);
		for (int i = 0; i < employeeApplications.Count; i++)
		{
			if (employeeApplications[i].employeeID == employeeStats.employeeID && employeeApplications[i].role == employeeStats.role)
			{
				item = employeeApplications[i];
				break;
			}
		}
		employeeApplications.Remove(item);
		GenericDataSerializer.Save(DAILY_EMPLOYEE_STATS_KEY, employeeApplications);
	}

	public int GetEmployeeCapacity()
	{
		int currentLevel = SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel;
		for (int num = EmployeeCountToRequiredLevelMap.Count; num >= 1; num--)
		{
			if (currentLevel >= EmployeeCountToRequiredLevelMap[num])
			{
				if (GameManager.isDemo)
				{
					return 1;
				}
				return num;
			}
		}
		return 0;
	}

	public int GetNextIncreaseLevel()
	{
		int currentLevel = SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel;
		for (int num = EmployeeCountToRequiredLevelMap.Count; num >= 1; num--)
		{
			if (currentLevel >= EmployeeCountToRequiredLevelMap[num])
			{
				return EmployeeCountToRequiredLevelMap[num + 1];
			}
		}
		return EmployeeCountToRequiredLevelMap[1];
	}

	public void UpdateApplications()
	{
		employeeApplications.Clear();
		int count = UnityEngine.Random.Range(MIN_CASHIER_APPLICATIONS, MAX_CASHIER_APPLICATIONS + 1);
		cashiers = SingletonBehaviour<EmployeeManager>.Instance.GetRandomAvailableCashiers(count);
		for (int i = 0; i < cashiers.Count; i++)
		{
			string text = randomName.GenerateName(cashiers[i].GetEmployeeData().sex);
			EmployeeRole role = EmployeeRole.CASHIER;
			int randomMoveSpeed = GetRandomMoveSpeed();
			int randomWorkSpeed = GetRandomWorkSpeed();
			int randomDailyWage = GetRandomDailyWage();
			EmployeeStats item = new EmployeeStats(text, role, randomMoveSpeed, randomWorkSpeed, cashiers[i].GetEmployeeID(), randomDailyWage);
			employeeApplications.Add(item);
		}
		int count2 = UnityEngine.Random.Range(MIN_RESTOCKER_APPLICATIONS, MAX_RESTOCKER_APPLICATIONS + 1);
		List<Restocker> randomAvailableRestockers = SingletonBehaviour<EmployeeManager>.Instance.GetRandomAvailableRestockers(count2);
		for (int j = 0; j < randomAvailableRestockers.Count; j++)
		{
			string text2 = randomName.GenerateName(randomAvailableRestockers[j].GetEmployeeData().sex);
			EmployeeRole role2 = EmployeeRole.RESTOCKER;
			int randomMoveSpeed2 = GetRandomMoveSpeed();
			int randomWorkSpeed2 = GetRandomWorkSpeed();
			int randomDailyWage2 = GetRandomDailyWage();
			EmployeeStats item2 = new EmployeeStats(text2, role2, randomMoveSpeed2, randomWorkSpeed2, randomAvailableRestockers[j].GetEmployeeID(), randomDailyWage2, isPaused: false, returnToRacks: true);
			employeeApplications.Add(item2);
		}
		int count3 = UnityEngine.Random.Range(MIN_BAKERY_STAFF_APPLICATIONS, MAX_BAKERY_STAFF_APPLICATIONS + 1);
		List<Baker> randomAvailableBakers = SingletonBehaviour<EmployeeManager>.Instance.GetRandomAvailableBakers(count3);
		for (int k = 0; k < randomAvailableBakers.Count; k++)
		{
			string text3 = randomName.GenerateName(randomAvailableBakers[k].GetEmployeeData().sex);
			EmployeeRole role3 = EmployeeRole.BAKERY_STAFF;
			int randomMoveSpeed3 = GetRandomMoveSpeed();
			int randomWorkSpeed3 = GetRandomWorkSpeed();
			int randomDailyWage3 = GetRandomDailyWage();
			EmployeeStats item3 = new EmployeeStats(text3, role3, randomMoveSpeed3, randomWorkSpeed3, randomAvailableBakers[k].GetEmployeeID(), randomDailyWage3);
			employeeApplications.Add(item3);
		}
		int count4 = UnityEngine.Random.Range(MIN_UNLOADER_APPLICATIONS, MAX_UNLOADER_APPLICATIONS + 1);
		List<Unloader> randomAvailableUnloaders = SingletonBehaviour<EmployeeManager>.Instance.GetRandomAvailableUnloaders(count4);
		for (int l = 0; l < randomAvailableUnloaders.Count; l++)
		{
			string text4 = randomName.GenerateName(randomAvailableUnloaders[l].GetEmployeeData().sex);
			EmployeeRole role4 = EmployeeRole.UNLOADER;
			int randomMoveSpeed4 = GetRandomMoveSpeed();
			int randomWorkSpeed4 = GetRandomWorkSpeed();
			int randomDailyWage4 = GetRandomDailyWage();
			EmployeeStats item4 = new EmployeeStats(text4, role4, randomMoveSpeed4, randomWorkSpeed4, randomAvailableUnloaders[l].GetEmployeeID(), randomDailyWage4);
			employeeApplications.Add(item4);
		}
		int count5 = UnityEngine.Random.Range(MIN_SEAFOOD_STAFF_APPLICATIONS, MAX_SEAFOOD_STAFF_APPLICATIONS + 1);
		List<SeafoodStaff> randomAvailableSeafoodStaff = SingletonBehaviour<EmployeeManager>.Instance.GetRandomAvailableSeafoodStaff(count5);
		for (int m = 0; m < randomAvailableSeafoodStaff.Count; m++)
		{
			string text5 = randomName.GenerateName(randomAvailableSeafoodStaff[m].GetEmployeeData().sex);
			EmployeeRole role5 = EmployeeRole.SEAFOOD_STAFF;
			int randomMoveSpeed5 = GetRandomMoveSpeed();
			int randomWorkSpeed5 = GetRandomWorkSpeed();
			int randomDailyWage5 = GetRandomDailyWage();
			EmployeeStats item5 = new EmployeeStats(text5, role5, randomMoveSpeed5, randomWorkSpeed5, randomAvailableSeafoodStaff[m].GetEmployeeID(), randomDailyWage5);
			employeeApplications.Add(item5);
		}
		GenericDataSerializer.Save(DAILY_EMPLOYEE_STATS_KEY, employeeApplications);
		hiringWindow.RefreshApplications();
	}

	public int GetRandomMoveSpeed()
	{
		return UnityEngine.Random.Range(80, 130);
	}

	public int GetRandomWorkSpeed()
	{
		return UnityEngine.Random.Range(80, 130);
	}

	public int GetRandomDailyWage()
	{
		return UnityEngine.Random.Range(85, 160);
	}
}
