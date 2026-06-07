using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmployeeCard : SelectableUI
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
	private TMP_Dropdown includeWarehouseDropdown;

	[SerializeField]
	private GameObject includeWarehouseDropdownParent;

	[SerializeField]
	private Button fireButton;

	[SerializeField]
	private Button saveChangesButton;

	[SerializeField]
	private Button pauseButton;

	[SerializeField]
	private Button resumeButton;

	[SerializeField]
	private Toggle returnToRacksToggle;

	[SerializeField]
	private GameObject returnToRacksParent;

	private HiringManager.EmployeeStats currentEmployeeStats;

	public static List<ProductGroup> DEPARTMENT_OPTIONS = new List<ProductGroup>
	{
		ProductGroup.NONE,
		ProductGroup.BAKERY,
		ProductGroup.TOY,
		ProductGroup.GROCERY,
		ProductGroup.FISH,
		ProductGroup.CLOTHING,
		ProductGroup.ELECTRONICS,
		ProductGroup.MUSIC,
		ProductGroup.SPORTS,
		ProductGroup.VENDING,
		ProductGroup.BEACH
	};

	private List<int> checkoutOptions = new List<int>();

	private List<int> displayIDs = new List<int>();

	public HiringManager.EmployeeStats CurrentEmployeeStats => currentEmployeeStats;

	private void Start()
	{
		shiftDropdown.onValueChanged.AddListener(delegate
		{
			OnDropdownValueChanged();
		});
		checkoutDropdown.onValueChanged.AddListener(delegate
		{
			OnDropdownValueChanged();
		});
		departmentDropdown.onValueChanged.AddListener(delegate
		{
			OnDropdownValueChanged();
		});
		includeWarehouseDropdown.onValueChanged.AddListener(delegate
		{
			OnDropdownValueChanged();
		});
		returnToRacksToggle.onValueChanged.AddListener(delegate
		{
			OnToggleValueChanged();
		});
		fireButton.onClick.AddListener(OnFireButtonClicked);
		saveChangesButton.onClick.AddListener(OnSaveChangesButtonClicked);
		saveChangesButton.interactable = false;
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		pauseButton.onClick.AddListener(OnPauseButtonClicked);
		resumeButton.onClick.AddListener(OnPauseButtonClicked);
	}

	public void SwitchPauseState()
	{
		SingletonBehaviour<EmployeeManager>.Instance.SwitchState(currentEmployeeStats);
		currentEmployeeStats.isPaused = !currentEmployeeStats.isPaused;
		SingletonBehaviour<EmployeeManager>.Instance.UpdateEmployeeData(currentEmployeeStats);
		Repaint(currentEmployeeStats);
	}

	private void OnPauseButtonClicked()
	{
		SwitchPauseState();
		SingletonBehaviour<InputManager>.Instance.SelectElement(currentEmployeeStats.isPaused ? resumeButton.gameObject : pauseButton.gameObject);
		List<Selectable> list = new List<Selectable>();
		list = GetSelectableList();
		for (int i = 0; i < list.Count; i++)
		{
			Navigation navigation = list[i].navigation;
			navigation.selectOnLeft = (currentEmployeeStats.isPaused ? resumeButton : pauseButton);
			list[i].navigation = navigation;
		}
		EventManager.NotifyEvent(UIEvents.EMPLOYEE_PAUSE_CLICKED);
	}

	public List<Selectable> GetSelectableList()
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
		if (includeWarehouseDropdownParent.gameObject.activeSelf)
		{
			list.Add(includeWarehouseDropdown);
		}
		if (returnToRacksParent.gameObject.activeSelf)
		{
			list.Add(returnToRacksToggle);
		}
		return list;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		AddDepartmentAndIncludeWarehouseOptions();
	}

	private void OnToggleValueChanged()
	{
		RepaintSaveChangesButton();
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
	}

	private void OnDropdownValueChanged()
	{
		RepaintSaveChangesButton();
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
	}

	private void OnFireButtonClicked()
	{
		EventManager.NotifyEvent(UIEvents.OPEN_EMPLOYEE_FIRE_POPUP, currentEmployeeStats);
	}

	private void OnSaveChangesButtonClicked()
	{
		bool flag = shiftDropdown.value == 0;
		if (currentEmployeeStats.isEarylyShift != flag && !SingletonBehaviour<TimeManager>.Instance.DayOver())
		{
			Repaint(currentEmployeeStats);
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("shift_change_only_endday"), base.transform);
			return;
		}
		int checkoutDeskID = (checkoutDropdownParent.activeSelf ? checkoutOptions[checkoutDropdown.value] : (-1));
		int index = (departmentDropdownParent.activeSelf ? departmentDropdown.value : (-1));
		bool includeWarehouse = includeWarehouseDropdownParent.activeSelf && includeWarehouseDropdown.value == 0;
		if (currentEmployeeStats.role == EmployeeRole.CASHIER)
		{
			if (!SingletonBehaviour<EmployeeManager>.Instance.IsAvailable(flag, checkoutDeskID))
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("register_not_available").Replace("{0}", displayIDs[checkoutDropdown.value].ToString()), base.transform);
				Repaint(currentEmployeeStats);
				return;
			}
			currentEmployeeStats.checkoutDeskID = checkoutDeskID;
			currentEmployeeStats.isEarylyShift = flag;
			SingletonBehaviour<EmployeeManager>.Instance.UpdateEmployeeData(currentEmployeeStats);
			Repaint(currentEmployeeStats);
		}
		else if (currentEmployeeStats.role == EmployeeRole.RESTOCKER)
		{
			currentEmployeeStats.department = DEPARTMENT_OPTIONS[index];
			currentEmployeeStats.includeWarehouse = includeWarehouse;
			currentEmployeeStats.isEarylyShift = flag;
			currentEmployeeStats.returnToRacks = returnToRacksToggle.isOn;
			SingletonBehaviour<EmployeeManager>.Instance.UpdateEmployeeData(currentEmployeeStats);
			Repaint(currentEmployeeStats);
		}
		else if (currentEmployeeStats.role == EmployeeRole.BAKERY_STAFF)
		{
			currentEmployeeStats.isEarylyShift = flag;
			SingletonBehaviour<EmployeeManager>.Instance.UpdateEmployeeData(currentEmployeeStats);
			Repaint(currentEmployeeStats);
		}
		else if (currentEmployeeStats.role == EmployeeRole.UNLOADER || currentEmployeeStats.role == EmployeeRole.SEAFOOD_STAFF)
		{
			currentEmployeeStats.isEarylyShift = flag;
			SingletonBehaviour<EmployeeManager>.Instance.UpdateEmployeeData(currentEmployeeStats);
			Repaint(currentEmployeeStats);
		}
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
		checkoutOptions = checkoutDeskFurnitureIDs;
		this.displayIDs = displayIDs;
		checkoutDropdown.ClearOptions();
		List<string> list = new List<string>();
		for (int i = 0; i < displayIDs.Count; i++)
		{
			list.Add(" #" + displayIDs[i]);
		}
		checkoutDropdown.AddOptions(list);
		if (currentEmployeeStats.role == EmployeeRole.CASHIER && base.gameObject.activeSelf)
		{
			checkoutDropdown.SetValueWithoutNotify(checkoutOptions.IndexOf(currentEmployeeStats.checkoutDeskID));
			RepaintSaveChangesButton();
		}
	}

	private void AddDepartmentAndIncludeWarehouseOptions()
	{
		List<string> list = new List<string>();
		foreach (ProductGroup dEPARTMENT_OPTION in DEPARTMENT_OPTIONS)
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
		includeWarehouseDropdown.ClearOptions();
		includeWarehouseDropdown.AddOptions(new List<string>
		{
			Locale.GetWord("dropdown_warehouse"),
			Locale.GetWord("dropdown_zones")
		});
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
		bool isEarylyShift = employeeStats.isEarylyShift;
		shiftDropdown.SetValueWithoutNotify((!isEarylyShift) ? 1 : 0);
		checkoutDropdownParent.SetActive(employeeStats.role == EmployeeRole.CASHIER);
		departmentDropdownParent.SetActive(employeeStats.role == EmployeeRole.RESTOCKER);
		includeWarehouseDropdownParent.SetActive(employeeStats.role == EmployeeRole.RESTOCKER);
		returnToRacksParent.SetActive(employeeStats.role == EmployeeRole.RESTOCKER);
		if (employeeStats.role == EmployeeRole.RESTOCKER || employeeStats.role == EmployeeRole.UNLOADER || employeeStats.role == EmployeeRole.BAKERY_STAFF)
		{
			pauseButton.gameObject.SetActive(!employeeStats.isPaused);
			resumeButton.gameObject.SetActive(employeeStats.isPaused);
		}
		else
		{
			pauseButton.gameObject.SetActive(value: false);
			resumeButton.gameObject.SetActive(value: false);
		}
		if (employeeStats.role == EmployeeRole.CASHIER)
		{
			checkoutDropdown.SetValueWithoutNotify(checkoutOptions.IndexOf(employeeStats.checkoutDeskID));
		}
		else if (employeeStats.role == EmployeeRole.RESTOCKER)
		{
			if (departmentDropdown.options.Count == 0)
			{
				AddDepartmentAndIncludeWarehouseOptions();
			}
			includeWarehouseDropdown.SetValueWithoutNotify((!employeeStats.includeWarehouse) ? 1 : 0);
			departmentDropdown.SetValueWithoutNotify(DEPARTMENT_OPTIONS.IndexOf(employeeStats.department));
			returnToRacksToggle.SetIsOnWithoutNotify(employeeStats.returnToRacks);
		}
		RepaintSaveChangesButton();
	}

	private void RepaintSaveChangesButton()
	{
		for (int i = 0; i < checkoutOptions.Count; i++)
		{
		}
		bool flag = includeWarehouseDropdownParent.activeSelf && includeWarehouseDropdown.value == 0;
		bool flag2 = returnToRacksParent.activeSelf && returnToRacksToggle.isOn;
		saveChangesButton.interactable = currentEmployeeStats.isEarylyShift != (shiftDropdown.value == 0) || (currentEmployeeStats.role == EmployeeRole.CASHIER && currentEmployeeStats.checkoutDeskID != checkoutOptions[checkoutDropdown.value]) || (currentEmployeeStats.role == EmployeeRole.RESTOCKER && currentEmployeeStats.department != DEPARTMENT_OPTIONS[departmentDropdown.value]) || (currentEmployeeStats.role == EmployeeRole.RESTOCKER && currentEmployeeStats.includeWarehouse != flag) || (currentEmployeeStats.role == EmployeeRole.RESTOCKER && currentEmployeeStats.returnToRacks != flag2);
	}

	public override Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		List<Selectable> selectableList = GetSelectableList();
		Selectable selectOnRight = selectableList[selectableList.Count - 1];
		Selectable selectOnLeft = left;
		if (pauseButton.gameObject.activeSelf)
		{
			selectOnLeft = pauseButton;
		}
		else if (resumeButton.gameObject.activeSelf)
		{
			selectOnLeft = resumeButton;
		}
		for (int i = 0; i < selectableList.Count; i++)
		{
			if (i == 0)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = up,
					selectOnDown = null,
					selectOnLeft = selectOnLeft,
					selectOnRight = right
				};
				if (selectableList.Count > i + 1)
				{
					navigation.selectOnDown = selectableList[i + 1];
				}
				else
				{
					navigation.selectOnDown = fireButton;
				}
				selectableList[i].navigation = navigation;
				continue;
			}
			Navigation navigation2 = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = null,
				selectOnDown = null,
				selectOnLeft = selectOnLeft,
				selectOnRight = right
			};
			if (selectableList.Count > i - 1)
			{
				navigation2.selectOnUp = selectableList[i - 1];
			}
			if (selectableList.Count > i + 1)
			{
				navigation2.selectOnDown = selectableList[i + 1];
			}
			if (i == selectableList.Count - 1)
			{
				navigation2.selectOnDown = fireButton;
			}
			selectableList[i].navigation = navigation2;
		}
		if (pauseButton.gameObject.activeSelf || resumeButton.gameObject.activeSelf)
		{
			Navigation navigation3 = pauseButton.navigation;
			navigation3.mode = Navigation.Mode.Explicit;
			navigation3.selectOnUp = up;
			navigation3.selectOnDown = fireButton;
			navigation3.selectOnLeft = left;
			navigation3.selectOnRight = selectOnRight;
			pauseButton.navigation = navigation3;
			resumeButton.navigation = navigation3;
		}
		Navigation navigation4 = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = selectableList[selectableList.Count - 1],
			selectOnDown = down,
			selectOnLeft = left,
			selectOnRight = saveChangesButton
		};
		Navigation navigation5 = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = selectableList[selectableList.Count - 1],
			selectOnDown = down,
			selectOnLeft = fireButton,
			selectOnRight = right
		};
		fireButton.navigation = navigation4;
		saveChangesButton.navigation = navigation5;
		return fireButton;
	}

	public override Selectable GetSelectable()
	{
		return fireButton;
	}
}
