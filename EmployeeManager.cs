using System.Collections.Generic;
using UnityEngine;

public class EmployeeManager : SingletonBehaviour<EmployeeManager>
{
	[SerializeField]
	private List<Cashier> cashiers;

	[SerializeField]
	private List<Restocker> restockers;

	[SerializeField]
	private List<Baker> bakers;

	[SerializeField]
	private List<Unloader> unloaders;

	[SerializeField]
	private List<SeafoodStaff> seafoodStaff;

	[SerializeField]
	private Transform deactivationPoint;

	[SerializeField]
	private Transform serviceRoomEmployeeActivationPoint;

	[SerializeField]
	private List<Transform> activationPoints;

	[SerializeField]
	private UnpaidWagesWindow unpaidWagesWindow;

	private Dictionary<int, Cashier> cashierDictionary = new Dictionary<int, Cashier>();

	private Dictionary<int, Restocker> restockerDictionary = new Dictionary<int, Restocker>();

	private Dictionary<int, Baker> bakerDictionary = new Dictionary<int, Baker>();

	private Dictionary<int, Unloader> unloadersDictionary = new Dictionary<int, Unloader>();

	private Dictionary<int, SeafoodStaff> seafoodStaffDictionary = new Dictionary<int, SeafoodStaff>();

	private const string HIRED_EMPLOYEES_KEY = "HIRED_EMPLOYEES_KEY";

	private List<HiringManager.EmployeeStats> hiredEmployees;

	private const string UPDATE_RESTOCKERS = "UPDATE_RESTOCKERS";

	public List<HiringManager.EmployeeStats> HiredEmployees => hiredEmployees;

	private new void Awake()
	{
		base.Awake();
		for (int i = 0; i < cashiers.Count; i++)
		{
			cashierDictionary.Add(cashiers[i].GetEmployeeID(), cashiers[i]);
		}
		for (int j = 0; j < restockers.Count; j++)
		{
			restockerDictionary.Add(restockers[j].GetEmployeeID(), restockers[j]);
		}
		for (int k = 0; k < bakers.Count; k++)
		{
			bakerDictionary.Add(bakers[k].GetEmployeeID(), bakers[k]);
		}
		for (int l = 0; l < unloaders.Count; l++)
		{
			unloadersDictionary.Add(unloaders[l].GetEmployeeID(), unloaders[l]);
		}
		for (int m = 0; m < seafoodStaff.Count; m++)
		{
			seafoodStaffDictionary.Add(seafoodStaff[m].GetEmployeeID(), seafoodStaff[m]);
		}
		hiredEmployees = GenericDataSerializer.Load("HIRED_EMPLOYEES_KEY", new List<HiringManager.EmployeeStats>());
		CheckAndUpdateRestockers();
		EventManager.AddListener(GameEvents.EARLY_SHIFT_OVER, OnEarlyShiftOver);
		EventManager.AddListener(GameEvents.DAY_ENDED, OnDayEnded);
		EventManager.AddListener(GameEvents.DAY_ENDED_NO_CUSTOMERS_LEFT, OnDayEndedNoCustomersLeft);
	}

	private void CheckAndUpdateRestockers()
	{
		if (!GenericDataSerializer.LoadBool("UPDATE_RESTOCKERS", value: true))
		{
			return;
		}
		GenericDataSerializer.SaveBool("UPDATE_RESTOCKERS", value: false);
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (hiredEmployees[i].role == EmployeeRole.RESTOCKER)
			{
				HiringManager.EmployeeStats value = hiredEmployees[i];
				value.returnToRacks = true;
				hiredEmployees[i] = value;
			}
		}
		GenericDataSerializer.Save("HIRED_EMPLOYEES_KEY", hiredEmployees);
	}

	private void Start()
	{
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
	}

	private void OnDayEndedNoCustomersLeft()
	{
		for (int i = 0; i < cashiers.Count; i++)
		{
			if (cashiers[i].IsActive())
			{
				cashiers[i].TryDeactivate();
			}
		}
	}

	public void Initialize()
	{
		if (!SingletonBehaviour<TimeManager>.Instance.IsEarlyShiftOver())
		{
			ActivateEmployees(earlyShift: true, activate: true);
		}
		else if (!SingletonBehaviour<TimeManager>.Instance.DayOver())
		{
			ActivateEmployees(earlyShift: false, activate: true);
		}
	}

	private void OnEarlyShiftOver()
	{
		ActivateEmployees(earlyShift: true, activate: false);
		ActivateEmployees(earlyShift: false, activate: true);
	}

	private void OnDayEnded()
	{
		DeactivateAllEmployeesInstant();
	}

	public void ActivateAllEmployees()
	{
		ActivateEmployees(earlyShift: false, activate: true);
	}

	public void DeactivateAllEmployeesInstant()
	{
		for (int i = 0; i < cashiers.Count; i++)
		{
			if (cashiers[i].IsActive())
			{
				cashiers[i].DeactivateInstant();
			}
		}
		for (int j = 0; j < restockers.Count; j++)
		{
			if (restockers[j].IsActive())
			{
				restockers[j].DeactivateInstant();
			}
		}
		for (int k = 0; k < bakers.Count; k++)
		{
			if (bakers[k].IsActive())
			{
				bakers[k].DeactivateInstant();
			}
		}
		for (int l = 0; l < unloaders.Count; l++)
		{
			if (unloaders[l].IsActive())
			{
				unloaders[l].DeactivateInstant();
			}
		}
		for (int m = 0; m < seafoodStaff.Count; m++)
		{
			if (seafoodStaff[m].IsActive())
			{
				seafoodStaff[m].DeactivateInstant();
			}
		}
	}

	private void OnNewDayStarted()
	{
		ActivateEmployees(earlyShift: true, activate: true);
		List<string> list = new List<string>();
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(hiredEmployees[i].dailyWage))
			{
				EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, hiredEmployees[i].dailyWage);
				EventManager.NotifyEvent(StatisticsEvents.HIRING_COST, (float)hiredEmployees[i].dailyWage);
			}
			else
			{
				list.Add(hiredEmployees[i].employeeName);
				FireEmployee(hiredEmployees[i]);
			}
		}
		if (list.Count > 0)
		{
			unpaidWagesWindow.Open(list);
		}
		else
		{
			EventManager.NotifyEvent(SupermarketEvents.EMPLOYEE_PAYMENTS_DONE);
		}
	}

	private void ActivateEmployees(bool earlyShift, bool activate, bool instant = false)
	{
		List<HiringManager.EmployeeStats> list = hiredEmployees;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].role == EmployeeRole.RESTOCKER && list[i].isEarylyShift == earlyShift)
			{
				if (activate)
				{
					restockerDictionary[list[i].employeeID].Activate(list[i]);
				}
				else
				{
					restockerDictionary[list[i].employeeID].TryDeactivate();
				}
			}
			else if (list[i].role == EmployeeRole.BAKERY_STAFF && list[i].isEarylyShift == earlyShift)
			{
				if (activate)
				{
					bakers[list[i].employeeID].Activate(list[i]);
				}
				else
				{
					bakers[list[i].employeeID].TryDeactivate();
				}
			}
			else if (list[i].role == EmployeeRole.UNLOADER && list[i].isEarylyShift == earlyShift)
			{
				if (activate)
				{
					unloaders[list[i].employeeID].Activate(list[i]);
				}
				else
				{
					unloaders[list[i].employeeID].TryDeactivate();
				}
			}
			else if (list[i].role == EmployeeRole.CASHIER && list[i].isEarylyShift == earlyShift)
			{
				int checkoutDeskID = list[i].checkoutDeskID;
				if (checkoutDeskID != -1)
				{
					if (activate)
					{
						Cashier cashier = cashierDictionary[list[i].employeeID];
						cashier.transform.position = activationPoints.GetRandomElement().position;
						cashier.Activate(list[i], checkoutDeskID);
					}
					else
					{
						cashierDictionary[list[i].employeeID].TryDeactivate();
					}
				}
			}
			else if (list[i].role == EmployeeRole.SEAFOOD_STAFF && list[i].isEarylyShift == earlyShift)
			{
				if (activate)
				{
					seafoodStaffDictionary[list[i].employeeID].Activate(list[i]);
				}
				else
				{
					seafoodStaffDictionary[list[i].employeeID].TryDeactivate();
				}
			}
		}
	}

	public void UpdateEmployeeData(HiringManager.EmployeeStats employeeStats)
	{
		List<HiringManager.EmployeeStats> list = hiredEmployees;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].employeeID == employeeStats.employeeID && list[i].role == employeeStats.role)
			{
				if (list[i].role == EmployeeRole.CASHIER && list[i].checkoutDeskID != employeeStats.checkoutDeskID && cashierDictionary[list[i].employeeID].IsActive())
				{
					cashierDictionary[list[i].employeeID].TrySwitch(employeeStats.checkoutDeskID);
				}
				else if (list[i].role == EmployeeRole.RESTOCKER && list[i].department != employeeStats.department && restockerDictionary[list[i].employeeID].IsActive())
				{
					restockerDictionary[list[i].employeeID].TrySwitchDepartment(employeeStats.department);
				}
				else if (list[i].role == EmployeeRole.RESTOCKER && list[i].includeWarehouse != employeeStats.includeWarehouse && restockerDictionary[list[i].employeeID].IsActive())
				{
					restockerDictionary[list[i].employeeID].TrySwitchIncludeWarehouse(employeeStats.includeWarehouse);
				}
				else if (list[i].role == EmployeeRole.RESTOCKER && list[i].returnToRacks != employeeStats.returnToRacks && restockerDictionary[list[i].employeeID].IsActive())
				{
					restockerDictionary[list[i].employeeID].SetReturnToRacks(employeeStats.returnToRacks);
				}
				list[i] = employeeStats;
				hiredEmployees[i] = employeeStats;
				GenericDataSerializer.Save("HIRED_EMPLOYEES_KEY", list);
				break;
			}
		}
	}

	public void SwitchState(HiringManager.EmployeeStats employeeStats)
	{
		if (employeeStats.role == EmployeeRole.RESTOCKER)
		{
			employeeStats.isPaused = !employeeStats.isPaused;
			UpdateEmployeeData(employeeStats);
			if (restockerDictionary[employeeStats.employeeID].IsActive())
			{
				restockerDictionary[employeeStats.employeeID].SwitchPauseState();
			}
		}
		else if (employeeStats.role == EmployeeRole.BAKERY_STAFF)
		{
			employeeStats.isPaused = !employeeStats.isPaused;
			UpdateEmployeeData(employeeStats);
			if (bakers[employeeStats.employeeID].IsActive())
			{
				bakers[employeeStats.employeeID].SwitchPauseState();
			}
		}
		else if (employeeStats.role == EmployeeRole.UNLOADER)
		{
			employeeStats.isPaused = !employeeStats.isPaused;
			UpdateEmployeeData(employeeStats);
			if (unloaders[employeeStats.employeeID].IsActive())
			{
				unloaders[employeeStats.employeeID].SwitchPauseState();
			}
		}
	}

	public bool IsPaused(HiringManager.EmployeeStats employeeStats)
	{
		if (employeeStats.role == EmployeeRole.RESTOCKER)
		{
			return restockerDictionary[employeeStats.employeeID].IsPaused();
		}
		if (employeeStats.role == EmployeeRole.UNLOADER)
		{
			return unloaders[employeeStats.employeeID].IsPaused();
		}
		return false;
	}

	public bool IsAvailable(bool earlyShift, int checkoutDeskID)
	{
		List<HiringManager.EmployeeStats> list = hiredEmployees;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].role == EmployeeRole.CASHIER && list[i].isEarylyShift == earlyShift && list[i].checkoutDeskID == checkoutDeskID)
			{
				return false;
			}
		}
		return true;
	}

	public void HireEmployee(HiringManager.EmployeeStats employeeStats)
	{
		List<HiringManager.EmployeeStats> list = hiredEmployees;
		if ((employeeStats.isEarylyShift && !SingletonBehaviour<TimeManager>.Instance.IsEarlyShiftOver()) || (!employeeStats.isEarylyShift && SingletonBehaviour<TimeManager>.Instance.IsEarlyShiftOver()))
		{
			ActivateEmployee(employeeStats);
		}
		list.Add(employeeStats);
		if (list.Count >= 1)
		{
			EventLogger.FirstEmployee();
		}
		if (list.Count >= 5)
		{
			EventLogger.FiveEmployees();
		}
		hiredEmployees = list;
		GenericDataSerializer.Save("HIRED_EMPLOYEES_KEY", list);
	}

	private void ActivateEmployee(HiringManager.EmployeeStats employeeStats)
	{
		switch (employeeStats.role)
		{
		case EmployeeRole.CASHIER:
			cashierDictionary[employeeStats.employeeID].Activate(employeeStats, employeeStats.checkoutDeskID);
			break;
		case EmployeeRole.RESTOCKER:
			restockerDictionary[employeeStats.employeeID].Activate(employeeStats);
			break;
		case EmployeeRole.BAKERY_STAFF:
			bakers[employeeStats.employeeID].Activate(employeeStats);
			break;
		case EmployeeRole.UNLOADER:
			unloaders[employeeStats.employeeID].Activate(employeeStats);
			break;
		case EmployeeRole.SEAFOOD_STAFF:
			seafoodStaffDictionary[employeeStats.employeeID].Activate(employeeStats);
			break;
		}
	}

	public void FireEmployee(HiringManager.EmployeeStats employeeStats)
	{
		List<HiringManager.EmployeeStats> list = hiredEmployees;
		list.Remove(employeeStats);
		hiredEmployees = list;
		GenericDataSerializer.Save("HIRED_EMPLOYEES_KEY", list);
		switch (employeeStats.role)
		{
		case EmployeeRole.CASHIER:
			if (cashierDictionary[employeeStats.employeeID].IsActive())
			{
				cashierDictionary[employeeStats.employeeID].TryDeactivate();
			}
			break;
		case EmployeeRole.RESTOCKER:
			if (restockerDictionary[employeeStats.employeeID].IsActive())
			{
				restockerDictionary[employeeStats.employeeID].TryDeactivate();
			}
			break;
		case EmployeeRole.BAKERY_STAFF:
			if (bakers[employeeStats.employeeID].IsActive())
			{
				bakers[employeeStats.employeeID].TryDeactivate();
			}
			break;
		case EmployeeRole.UNLOADER:
			if (unloaders[employeeStats.employeeID].IsActive())
			{
				unloaders[employeeStats.employeeID].TryDeactivate();
			}
			break;
		case EmployeeRole.SEAFOOD_STAFF:
			if (seafoodStaffDictionary[employeeStats.employeeID].IsActive())
			{
				seafoodStaffDictionary[employeeStats.employeeID].TryDeactivate();
			}
			break;
		}
		EventManager.NotifyEvent(SupermarketEvents.EMPLOYEE_FIRED, employeeStats);
	}

	public int GetHiredEmployeesCount()
	{
		int num = 0;
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			num++;
		}
		return num;
	}

	public int GetTotalWageCostPerDay()
	{
		int num = 0;
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			num += hiredEmployees[i].dailyWage;
		}
		return num;
	}

	public List<Cashier> GetRandomAvailableCashiers(int count)
	{
		List<Cashier> list = new List<Cashier>();
		list.AddRange(cashiers);
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (hiredEmployees[i].role == EmployeeRole.CASHIER)
			{
				list.Remove(cashierDictionary[hiredEmployees[i].employeeID]);
			}
		}
		List<Cashier> list2 = new List<Cashier>();
		int num = Mathf.Min(count, list.Count);
		for (int j = 0; j < num; j++)
		{
			int index = Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public Cashier GetCashier(int employeeID)
	{
		if (!cashierDictionary.ContainsKey(employeeID))
		{
			return null;
		}
		return cashierDictionary[employeeID];
	}

	public bool HasCashierForCheckout(int checkoutDeskID)
	{
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (hiredEmployees[i].role == EmployeeRole.CASHIER && hiredEmployees[i].checkoutDeskID == checkoutDeskID)
			{
				return true;
			}
		}
		return false;
	}

	public List<Restocker> GetRandomAvailableRestockers(int count)
	{
		List<Restocker> list = new List<Restocker>();
		list.AddRange(restockers);
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (hiredEmployees[i].role == EmployeeRole.RESTOCKER)
			{
				list.Remove(restockerDictionary[hiredEmployees[i].employeeID]);
			}
		}
		List<Restocker> list2 = new List<Restocker>();
		int num = Mathf.Min(count, list.Count);
		for (int j = 0; j < num; j++)
		{
			int index = Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public Restocker GetRestocker(int employeeID)
	{
		if (!restockerDictionary.ContainsKey(employeeID))
		{
			return null;
		}
		return restockerDictionary[employeeID];
	}

	public List<Baker> GetRandomAvailableBakers(int count)
	{
		List<Baker> list = new List<Baker>();
		list.AddRange(bakers);
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (hiredEmployees[i].role == EmployeeRole.BAKERY_STAFF)
			{
				list.Remove(bakers[hiredEmployees[i].employeeID]);
			}
		}
		List<Baker> list2 = new List<Baker>();
		int num = Mathf.Min(count, list.Count);
		for (int j = 0; j < num; j++)
		{
			int index = Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public Baker GetBaker(int employeeID)
	{
		if (!bakerDictionary.ContainsKey(employeeID))
		{
			return null;
		}
		return bakerDictionary[employeeID];
	}

	public List<Unloader> GetRandomAvailableUnloaders(int count)
	{
		List<Unloader> list = new List<Unloader>();
		list.AddRange(unloaders);
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (hiredEmployees[i].role == EmployeeRole.UNLOADER)
			{
				list.Remove(unloaders[hiredEmployees[i].employeeID]);
			}
		}
		List<Unloader> list2 = new List<Unloader>();
		int num = Mathf.Min(count, list.Count);
		for (int j = 0; j < num; j++)
		{
			int index = Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public List<SeafoodStaff> GetRandomAvailableSeafoodStaff(int count)
	{
		List<SeafoodStaff> list = new List<SeafoodStaff>();
		list.AddRange(seafoodStaff);
		for (int i = 0; i < hiredEmployees.Count; i++)
		{
			if (hiredEmployees[i].role == EmployeeRole.SEAFOOD_STAFF)
			{
				list.Remove(seafoodStaff[hiredEmployees[i].employeeID]);
			}
		}
		List<SeafoodStaff> list2 = new List<SeafoodStaff>();
		int num = Mathf.Min(count, list.Count);
		for (int j = 0; j < num; j++)
		{
			int index = Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public SeafoodStaff GetSeafoodStaff(int employeeID)
	{
		if (!seafoodStaffDictionary.ContainsKey(employeeID))
		{
			return null;
		}
		return seafoodStaffDictionary[employeeID];
	}

	public Unloader GetUnloader(int employeeID)
	{
		if (!unloadersDictionary.ContainsKey(employeeID))
		{
			return null;
		}
		return unloadersDictionary[employeeID];
	}

	public Transform ServiceRoomEmployeeActivationPoint()
	{
		return serviceRoomEmployeeActivationPoint;
	}

	public Transform DeactivationPoint()
	{
		return deactivationPoint;
	}

	public List<Cashier> GetAvailableCashier()
	{
		return cashiers;
	}
}
