using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HiringCard : SelectableUI
{
	[SerializeField]
	private Image employeeImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI roleValueText;

	[SerializeField]
	private TextMeshProUGUI wageValueText;

	[SerializeField]
	private TextMeshProUGUI moveSpeedValueText;

	[SerializeField]
	private TextMeshProUGUI workSpeedValueText;

	[SerializeField]
	private TMP_Dropdown shiftDropdown;

	[SerializeField]
	private TMP_Dropdown checkoutDropdown;

	[SerializeField]
	private GameObject checkoutDropdownParent;

	[SerializeField]
	private TMP_Dropdown departmentDropdown;

	[SerializeField]
	private GameObject departmentDropdownParent;

	[SerializeField]
	private TextMeshProUGUI requiredLevelText;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private Button hireButton;

	private HiringManager.EmployeeStats currentEmployeeStats;

	private List<int> checkoutOptions = new List<int>();

	private List<int> displayIDs = new List<int>();

	private void Start()
	{
		hireButton.onClick.AddListener(OnHire);
		shiftDropdown.onValueChanged.AddListener(delegate
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		});
		checkoutDropdown.onValueChanged.AddListener(delegate
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		});
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		EventManager.AddListener<int>(EconomyEvents.LEVEL_UP, RepaintRequiredLevel);
		EventManager.AddListener<FurnitureType>(PlaceableEvents.NEW_FURNITURE_PLACED, delegate
		{
			RepaintRequiredLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		});
		EventManager.AddListener<FurnitureType>(PlaceableEvents.FURNITURE_REMOVED, delegate
		{
			RepaintRequiredLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		});
		EventManager.AddListener<HiringManager.EmployeeStats>(SupermarketEvents.EMPLOYEE_HIRED, delegate
		{
			RepaintRequiredLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		});
		EventManager.AddListener<HiringManager.EmployeeStats>(SupermarketEvents.EMPLOYEE_FIRED, delegate
		{
			RepaintRequiredLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		});
		departmentDropdown.onValueChanged.AddListener(delegate
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		});
		AddDepartmentOptions();
	}

	private void AddDepartmentOptions()
	{
		List<string> list = new List<string>();
		foreach (ProductGroup dEPARTMENT_OPTION in EmployeeCard.DEPARTMENT_OPTIONS)
		{
			if (dEPARTMENT_OPTION == ProductGroup.NONE)
			{
				list.Add(Locale.GetWord("dropdown_all"));
			}
			else
			{
				list.Add(Locale.GetWord(dEPARTMENT_OPTION.ToString().ToLowerInvariant()));
			}
		}
		departmentDropdown.ClearOptions();
		departmentDropdown.AddOptions(list);
	}

	public override Selectable GetSelectable()
	{
		return hireButton;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		RepaintRequiredLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
		AddDepartmentOptions();
	}

	public EmployeeRole GetRole()
	{
		return currentEmployeeStats.role;
	}

	public void SetShiftOptions(List<string> shiftOptions)
	{
		shiftDropdown.ClearOptions();
		shiftDropdown.AddOptions(shiftOptions);
	}

	public void SetCheckoutOptions(List<int> checkoutDeskFurnitureIDs, List<int> displayIDs)
	{
		if (currentEmployeeStats.role == EmployeeRole.CASHIER)
		{
			checkoutOptions = checkoutDeskFurnitureIDs;
			this.displayIDs = displayIDs;
			checkoutDropdown.ClearOptions();
			List<string> list = new List<string>();
			for (int i = 0; i < displayIDs.Count; i++)
			{
				list.Add(" #" + displayIDs[i]);
			}
			checkoutDropdown.AddOptions(list);
		}
	}

	public void Repaint(HiringManager.EmployeeStats employeeStats)
	{
		currentEmployeeStats = employeeStats;
		switch (employeeStats.role)
		{
		case EmployeeRole.CASHIER:
			employeeImage.sprite = SingletonBehaviour<EmployeeManager>.Instance.GetCashier(employeeStats.employeeID).GetEmployeeData().employeeSprite;
			break;
		case EmployeeRole.RESTOCKER:
			employeeImage.sprite = SingletonBehaviour<EmployeeManager>.Instance.GetRestocker(employeeStats.employeeID).GetEmployeeData().employeeSprite;
			break;
		case EmployeeRole.BAKERY_STAFF:
			employeeImage.sprite = SingletonBehaviour<EmployeeManager>.Instance.GetBaker(employeeStats.employeeID).GetEmployeeData().employeeSprite;
			break;
		case EmployeeRole.UNLOADER:
			employeeImage.sprite = SingletonBehaviour<EmployeeManager>.Instance.GetUnloader(employeeStats.employeeID).GetEmployeeData().employeeSprite;
			break;
		case EmployeeRole.SEAFOOD_STAFF:
			employeeImage.sprite = SingletonBehaviour<EmployeeManager>.Instance.GetSeafoodStaff(employeeStats.employeeID).GetEmployeeData().employeeSprite;
			break;
		}
		nameText.text = employeeStats.employeeName;
		roleValueText.text = Locale.GetWord(employeeStats.role.ToString());
		wageValueText.text = "$" + employeeStats.dailyWage;
		moveSpeedValueText.text = "%" + employeeStats.moveSpeed;
		workSpeedValueText.text = "%" + employeeStats.workSpeed;
		priceText.text = "$" + employeeStats.dailyWage;
		checkoutDropdownParent.SetActive(employeeStats.role == EmployeeRole.CASHIER);
		departmentDropdownParent.SetActive(employeeStats.role == EmployeeRole.RESTOCKER);
		departmentDropdown.value = EmployeeCard.DEPARTMENT_OPTIONS.IndexOf(employeeStats.department);
		RepaintRequiredLevel(SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel);
	}

	private void RepaintRequiredLevel(int newLevel)
	{
		int num = HiringManager.EmployeeCountToRequiredLevelMap[SingletonBehaviour<EmployeeManager>.Instance.GetHiredEmployeesCount() + 1];
		if (GameManager.isDemo && num > HiringManager.EmployeeCountToRequiredLevelMap[1])
		{
			requiredLevelText.text = Locale.GetWord("no_cashier_capacity_demo");
			requiredLevelText.color = Color.red;
			hireButton.interactable = false;
			return;
		}
		if (SingletonBehaviour<EmployeeManager>.Instance.GetHiredEmployeesCount() == 45)
		{
			requiredLevelText.text = Locale.GetWord("max_capacity");
			requiredLevelText.color = Color.red;
			hireButton.interactable = false;
			return;
		}
		if (GetRole() == EmployeeRole.BAKERY_STAFF && !SingletonBehaviour<FurniturePool>.Instance.IsFurniturePurchased(FurnitureType.TRAY_STATION))
		{
			requiredLevelText.text = Locale.GetWord("requires_tray_station");
			requiredLevelText.color = Color.red;
			hireButton.interactable = false;
			return;
		}
		requiredLevelText.text = Locale.GetWord("required_level_n").Replace("{0}", num.ToString());
		int hiredEmployeesCount = SingletonBehaviour<EmployeeManager>.Instance.GetHiredEmployeesCount();
		if (newLevel >= HiringManager.EmployeeCountToRequiredLevelMap[hiredEmployeesCount + 1])
		{
			if (hiredEmployeesCount < SingletonBehaviour<HiringManager>.Instance.GetEmployeeCapacity())
			{
				requiredLevelText.color = Color.green;
				hireButton.interactable = true;
			}
			else
			{
				requiredLevelText.text = Locale.GetWord("no_employee_capacity");
				requiredLevelText.color = Color.red;
				hireButton.interactable = false;
			}
		}
		else
		{
			requiredLevelText.color = Color.red;
			hireButton.interactable = false;
		}
	}

	private void OnHire()
	{
		bool flag = shiftDropdown.value == 0;
		int checkoutDeskID = -1;
		if (checkoutOptions.Count != 0)
		{
			checkoutDeskID = (checkoutDropdownParent.activeSelf ? checkoutOptions[checkoutDropdown.value] : (-1));
		}
		ProductGroup department = EmployeeCard.DEPARTMENT_OPTIONS[departmentDropdown.value];
		if (currentEmployeeStats.role == EmployeeRole.CASHIER && !SingletonBehaviour<EmployeeManager>.Instance.IsAvailable(flag, checkoutDeskID))
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("register_not_available").Replace("{0}", displayIDs[checkoutDropdown.value].ToString()), base.transform);
		}
		else if (currentEmployeeStats.role == EmployeeRole.CASHIER && checkoutDropdown.options.Count == 0)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("no_checkout_desks", base.transform);
		}
		else if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughSoftCurrency(currentEmployeeStats.dailyWage))
		{
			currentEmployeeStats.isEarylyShift = flag;
			currentEmployeeStats.checkoutDeskID = checkoutDeskID;
			currentEmployeeStats.department = department;
			SingletonBehaviour<EmployeeManager>.Instance.HireEmployee(currentEmployeeStats);
			SingletonBehaviour<HiringManager>.Instance.RemoveApplication(currentEmployeeStats);
			EventManager.NotifyEvent(EconomyEvents.REMOVE_SOFT_CURRENCY, currentEmployeeStats.dailyWage);
			EventManager.NotifyEvent(SupermarketEvents.EMPLOYEE_HIRED, currentEmployeeStats);
			EventManager.NotifyEvent(StatisticsEvents.HIRING_COST, (float)currentEmployeeStats.dailyWage);
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_cash", base.transform);
		}
	}

	public override Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		List<Selectable> list = new List<Selectable>();
		list.Add(shiftDropdown);
		if (checkoutDropdownParent.gameObject.activeSelf)
		{
			list.Add(checkoutDropdown);
		}
		if (departmentDropdownParent.gameObject.activeSelf)
		{
			list.Add(departmentDropdown);
		}
		list.Add(hireButton);
		for (int i = 0; i < list.Count; i++)
		{
			if (i == 0)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = up,
					selectOnDown = null,
					selectOnLeft = left,
					selectOnRight = right
				};
				if (list.Count > i + 1)
				{
					navigation.selectOnDown = list[i + 1];
				}
				list[i].navigation = navigation;
				continue;
			}
			if (i == list.Count - 1)
			{
				Navigation navigation2 = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = null,
					selectOnDown = down,
					selectOnLeft = left,
					selectOnRight = right
				};
				if (list.Count > i - 1)
				{
					navigation2.selectOnUp = list[i - 1];
				}
				list[i].navigation = navigation2;
				continue;
			}
			Navigation navigation3 = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = null,
				selectOnDown = null,
				selectOnLeft = left,
				selectOnRight = right
			};
			if (list.Count > i - 1)
			{
				navigation3.selectOnUp = list[i - 1];
			}
			if (list.Count > i + 1)
			{
				navigation3.selectOnDown = list[i + 1];
			}
			list[i].navigation = navigation3;
		}
		return hireButton;
	}

	public override Selectable GetAvailableSelectable()
	{
		Navigation navigation = hireButton.navigation;
		if (navigation.selectOnRight != null)
		{
			return navigation.selectOnRight;
		}
		if (navigation.selectOnLeft != null)
		{
			return navigation.selectOnLeft;
		}
		if (shiftDropdown.navigation.selectOnUp != null)
		{
			return shiftDropdown.navigation.selectOnUp;
		}
		return null;
	}
}
