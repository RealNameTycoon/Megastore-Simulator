using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManageEmployeeWindow : TabWindow
{
	public enum ShiftFilter
	{
		ALL,
		EARLY,
		LATE
	}

	[SerializeField]
	private Button manageEmployeeTabButton;

	[SerializeField]
	private TMP_Dropdown filterDropdown;

	[SerializeField]
	private List<EmployeeCard> employeeCards;

	[SerializeField]
	private TextMeshProUGUI capacityText;

	[SerializeField]
	private TextMeshProUGUI nextIncreaseText;

	[SerializeField]
	private TextMeshProUGUI totalDailyWage;

	[SerializeField]
	private UIWindow fireEmployeeWindow;

	[SerializeField]
	private Canvas fireEmployeeCanvas;

	[SerializeField]
	private TextMeshProUGUI fireDescription;

	[SerializeField]
	private Button employeeFireButton;

	[SerializeField]
	private TMP_Dropdown shiftFilter;

	[SerializeField]
	private GridLayoutGroup employeeGridLayout;

	[SerializeField]
	private GameObject topSettingsGroup;

	[SerializeField]
	private Button pauseAllButton;

	[SerializeField]
	private Button resumeAllButton;

	[SerializeField]
	private GameObject sliderGroup;

	[SerializeField]
	private Slider restockThresholdSlider;

	[SerializeField]
	private TextMeshProUGUI restockThresholdValueText;

	private const int ITEM_PER_ROW = 3;

	private const int GRID_LAYOUT_TOP_PADDING_MOVED = 120;

	private const int GRID_LAYOUT_TOP_PADDING_DEFAULT = 10;

	private List<HiringManager.EmployeeStats> filteredEmployees;

	private float initialCellHeight;

	public static List<string> SHIFT_OPTIONS = new List<string> { "dropdown_all_shift", "dropdown_early_shift", "dropdown_late_shift" };

	private void Awake()
	{
		EventManager.AddListener(StartupEvents.SPAWN_MANAGER_INITIALIZED, InitializeWindow);
		initialCellHeight = employeeGridLayout.cellSize.y;
	}

	private void InitializeWindow()
	{
		SetFilterOptions();
		SetShiftOptions();
		SetCheckoutOptions();
		filterDropdown.onValueChanged.AddListener(OnRoleFilterChanged);
		shiftFilter.onValueChanged.AddListener(OnFilterChanged);
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		EventManager.AddListener<FurnitureType>(PlaceableEvents.NEW_FURNITURE_PLACED, OnNewFurniturePlaced);
		EventManager.AddListener<FurnitureType>(PlaceableEvents.FURNITURE_REMOVED, OnNewFurniturePlaced);
		EventManager.AddListener<int>(EconomyEvents.LEVEL_UP, delegate
		{
			RefreshEmployeeCards();
		});
		EventManager.AddListener<HiringManager.EmployeeStats>(SupermarketEvents.EMPLOYEE_HIRED, delegate
		{
			RefreshEmployeeCards();
		});
		EventManager.AddListener<HiringManager.EmployeeStats>(SupermarketEvents.EMPLOYEE_FIRED, OnEmployeeFired);
		EventManager.AddListener<HiringManager.EmployeeStats>(UIEvents.OPEN_EMPLOYEE_FIRE_POPUP, OpenFireEmployeePopup);
		RefreshEmployeeCards();
		EventManager.AddListener(UIEvents.TAB_SELECTED_OBJECT_CHANGED, RefreshIfOpen);
		pauseAllButton.onClick.AddListener(OnPauseButtonClicked);
		resumeAllButton.onClick.AddListener(OnPauseButtonClicked);
		restockThresholdValueText.text = SingletonBehaviour<RestockerManager>.Instance.RestockThreshold + "%";
		restockThresholdSlider.value = SingletonBehaviour<RestockerManager>.Instance.RestockThreshold;
		restockThresholdSlider.onValueChanged.AddListener(OnRestockThresholdChanged);
		EventManager.AddListener(UIEvents.EMPLOYEE_PAUSE_CLICKED, RefreshPauseAllButtons);
	}

	private void RefreshPauseAllButtons()
	{
		EmployeeRole employeeRole = HiringWindow.EMPLOYEE_ROLES[filterDropdown.value];
		if (employeeRole != EmployeeRole.RESTOCKER && employeeRole != EmployeeRole.UNLOADER && employeeRole != EmployeeRole.BAKERY_STAFF)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < employeeCards.Count; i++)
		{
			if (employeeCards[i].gameObject.activeSelf && !employeeCards[i].CurrentEmployeeStats.isPaused)
			{
				flag = false;
				break;
			}
		}
		pauseAllButton.gameObject.SetActive(!flag);
		resumeAllButton.gameObject.SetActive(flag);
	}

	private void OnRestockThresholdChanged(float value)
	{
		int restockThreshold = (int)value;
		restockThresholdValueText.text = restockThreshold + "%";
		SingletonBehaviour<RestockerManager>.Instance.SetRestockThreshold(restockThreshold);
	}

	public override void Open()
	{
		base.Open();
		RefreshEmployeeCards();
	}

	private void OpenFireEmployeePopup(HiringManager.EmployeeStats employeeStats)
	{
		fireEmployeeCanvas.enabled = true;
		fireEmployeeWindow.Open();
		fireDescription.text = Locale.GetWord("fire_employee_description").Replace("{0}", employeeStats.employeeName);
		employeeFireButton.onClick.RemoveAllListeners();
		employeeFireButton.onClick.AddListener(delegate
		{
			SingletonBehaviour<EmployeeManager>.Instance.FireEmployee(employeeStats);
			fireEmployeeCanvas.enabled = false;
			fireEmployeeWindow.Close();
			SingletonBehaviour<InputManager>.Instance.SelectElement(manageEmployeeTabButton.gameObject);
		});
	}

	public void CloseFireEmployeePopup()
	{
		fireEmployeeCanvas.enabled = false;
		fireEmployeeWindow.Close();
	}

	private void OnNewFurniturePlaced(FurnitureType type)
	{
		if (type == FurnitureType.CHECKOUT_DESK)
		{
			SetCheckoutOptions();
		}
	}

	private void SetFilterOptions()
	{
		List<string> list = new List<string>();
		foreach (EmployeeRole eMPLOYEE_ROLE in HiringWindow.EMPLOYEE_ROLES)
		{
			list.Add(Locale.GetWord(eMPLOYEE_ROLE.ToString()));
		}
		filterDropdown.ClearOptions();
		filterDropdown.AddOptions(list);
		List<string> list2 = new List<string>();
		foreach (string sHIFT_OPTION in SHIFT_OPTIONS)
		{
			list2.Add(Locale.GetWord(sHIFT_OPTION));
		}
		shiftFilter.ClearOptions();
		shiftFilter.AddOptions(list2);
	}

	private void SetShiftOptions()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < HiringWindow.SHIFT_OPTIONS.Count; i++)
		{
			list.Add(Locale.GetWord(HiringWindow.SHIFT_OPTIONS[i]));
		}
		for (int j = 0; j < employeeCards.Count; j++)
		{
			employeeCards[j].SetShiftOptions(list);
		}
	}

	private void SetCheckoutOptions()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < SingletonBehaviour<CheckoutDeskManager>.Instance.CheckoutDesks.Count; i++)
		{
			list.Add(SingletonBehaviour<CheckoutDeskManager>.Instance.CheckoutDesks[i].FurnitureID);
			list2.Add(SingletonBehaviour<CheckoutDeskManager>.Instance.CheckoutDesks[i].GetDisplayedID());
		}
		for (int j = 0; j < employeeCards.Count; j++)
		{
			employeeCards[j].SetCheckoutOptions(list, list2);
		}
	}

	private void OnLanguageChanged()
	{
		SetFilterOptions();
		SetShiftOptions();
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnRoleFilterChanged(int index)
	{
		EmployeeRole employeeRole = HiringWindow.EMPLOYEE_ROLES[index];
		topSettingsGroup.SetActive(employeeRole == EmployeeRole.RESTOCKER || employeeRole == EmployeeRole.UNLOADER || employeeRole == EmployeeRole.BAKERY_STAFF);
		sliderGroup.SetActive(employeeRole == EmployeeRole.RESTOCKER);
		OnFilterChanged(index);
	}

	private void OnFilterChanged(int index)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		RefreshEmployeeCards();
	}

	private void OnEmployeeFired(HiringManager.EmployeeStats employeeStats)
	{
		RefreshEmployeeCards();
		Selectable firstEmployeeSelectable = GetFirstEmployeeSelectable();
		if (firstEmployeeSelectable != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(firstEmployeeSelectable.gameObject);
		}
		else
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(manageEmployeeTabButton.gameObject);
		}
	}

	private void RefreshEmployeeCards()
	{
		filteredEmployees = FilterEmployees(filterDropdown.value, shiftFilter.value);
		Repaint(filteredEmployees);
		RepaintTopSection();
		RefreshPauseAllButtons();
		RefreshNavigation();
		if (employeeCards[0].gameObject.activeSelf)
		{
			float num = 180f - Mathf.Max(2.7f, employeeCards[0].GetSelectableList().Count) * 45f;
			employeeGridLayout.cellSize = new Vector2(employeeGridLayout.cellSize.x, initialCellHeight - num);
		}
	}

	private List<HiringManager.EmployeeStats> FilterEmployees(int index, int shift)
	{
		List<HiringManager.EmployeeStats> list = new List<HiringManager.EmployeeStats>();
		for (int i = 0; i < SingletonBehaviour<EmployeeManager>.Instance.HiredEmployees.Count; i++)
		{
			HiringManager.EmployeeStats employeeStats = SingletonBehaviour<EmployeeManager>.Instance.HiredEmployees[i];
			if (employeeStats.role == HiringWindow.EMPLOYEE_ROLES[index] && (shift == 0 || !employeeStats.isEarylyShift || shift != 2) && (employeeStats.isEarylyShift || shift != 1))
			{
				list.Add(SingletonBehaviour<EmployeeManager>.Instance.HiredEmployees[i]);
			}
		}
		return list;
	}

	private void Repaint(List<HiringManager.EmployeeStats> employeeStats)
	{
		for (int i = 0; i < employeeCards.Count; i++)
		{
			if (i < employeeStats.Count)
			{
				if (!IsValid(employeeStats[i]))
				{
					SingletonBehaviour<EmployeeManager>.Instance.FireEmployee(employeeStats[i]);
					continue;
				}
				employeeCards[i].gameObject.SetActive(value: true);
				employeeCards[i].Repaint(employeeStats[i]);
			}
			else
			{
				employeeCards[i].gameObject.SetActive(value: false);
			}
		}
	}

	private bool IsValid(HiringManager.EmployeeStats stats)
	{
		if (stats.role == EmployeeRole.CASHIER)
		{
			return SingletonBehaviour<CheckoutDeskManager>.Instance.GetCheckoutDesk(stats.checkoutDeskID) != null;
		}
		return true;
	}

	private void RepaintTopSection()
	{
		int employeeCapacity = SingletonBehaviour<HiringManager>.Instance.GetEmployeeCapacity();
		capacityText.text = SingletonBehaviour<EmployeeManager>.Instance.GetHiredEmployeesCount() + "/" + employeeCapacity;
		totalDailyWage.text = "$" + SingletonBehaviour<EmployeeManager>.Instance.GetTotalWageCostPerDay();
		if (GameManager.isDemo)
		{
			nextIncreaseText.enabled = false;
		}
		else if (employeeCapacity == 45)
		{
			nextIncreaseText.text = Locale.GetWord("max_employee_capacity");
		}
		else
		{
			nextIncreaseText.text = Locale.GetWord("next_increase_n").Replace("{0}", SingletonBehaviour<HiringManager>.Instance.GetNextIncreaseLevel().ToString());
		}
	}

	private void RefreshIfOpen()
	{
		if (IsOpen())
		{
			RefreshNavigation();
		}
	}

	private void RefreshNavigation()
	{
		if (manageEmployeeTabButton == null || employeeCards == null)
		{
			return;
		}
		Selectable selectable = null;
		if (!topSettingsGroup.gameObject.activeSelf)
		{
			selectable = null;
		}
		else if (pauseAllButton.gameObject.activeSelf)
		{
			selectable = pauseAllButton;
		}
		else if (resumeAllButton.gameObject.activeSelf)
		{
			selectable = resumeAllButton;
		}
		Navigation navigation = filterDropdown.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnDown = ((selectable != null) ? selectable : GetFirstEmployeeSelectable());
		navigation.selectOnUp = manageEmployeeTabButton;
		navigation.selectOnRight = shiftFilter;
		filterDropdown.navigation = navigation;
		Navigation navigation2 = shiftFilter.navigation;
		navigation2.mode = Navigation.Mode.Explicit;
		navigation2.selectOnDown = ((selectable != null) ? selectable : GetFirstEmployeeSelectable());
		navigation2.selectOnUp = manageEmployeeTabButton;
		navigation2.selectOnLeft = filterDropdown;
		shiftFilter.navigation = navigation2;
		Navigation navigation3 = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnDown = GetFirstEmployeeSelectable(),
			selectOnUp = filterDropdown,
			selectOnLeft = null,
			selectOnRight = (sliderGroup.activeSelf ? restockThresholdSlider : null)
		};
		pauseAllButton.navigation = navigation3;
		resumeAllButton.navigation = navigation3;
		if (sliderGroup.activeSelf)
		{
			Navigation navigation4 = restockThresholdSlider.navigation;
			navigation4.mode = Navigation.Mode.Explicit;
			navigation4.selectOnDown = GetFirstEmployeeSelectable();
			navigation4.selectOnUp = filterDropdown;
			navigation4.selectOnLeft = null;
			navigation4.selectOnRight = null;
			restockThresholdSlider.navigation = navigation4;
		}
		Selectable[] slots = new Selectable[employeeCards.Count];
		for (int i = 0; i < employeeCards.Count; i++)
		{
			EmployeeCard employeeCard = employeeCards[i];
			if (employeeCard == null || !employeeCard.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else if (!employeeCard.GetSelectable().gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = employeeCard.GetSelectable();
			}
		}
		for (int j = 0; j < employeeCards.Count; j++)
		{
			EmployeeCard employeeCard2 = employeeCards[j];
			if (!(employeeCard2 == null) && employeeCard2.gameObject.activeSelf && !(slots[j] == null))
			{
				int rowLock = j / 3;
				Selectable selectable2 = FindNext(j, -3, -1);
				Selectable up = ((((object)selectable2 != null) ? ((bool)selectable2) : (selectable != null)) ? selectable : filterDropdown);
				Selectable down = FindNext(j, 3, -1);
				Selectable left = FindNext(j, -1, rowLock);
				Selectable right = FindNext(j, 1, rowLock);
				employeeCard2.RefreshNavigation(up, down, left, right);
			}
		}
		Selectable FindNext(int startIndex, int step, int num)
		{
			for (int k = startIndex + step; k >= 0 && k < slots.Length && (num == -1 || k / 3 == num); k += step)
			{
				if (slots[k] != null)
				{
					return slots[k];
				}
			}
			return null;
		}
	}

	public override Selectable GetFirstSelectable()
	{
		if (filterDropdown.gameObject.activeSelf)
		{
			return filterDropdown;
		}
		for (int i = 0; i < employeeCards.Count; i++)
		{
			if (employeeCards[i].GetSelectable().gameObject.activeSelf)
			{
				return employeeCards[i].GetSelectable();
			}
		}
		return null;
	}

	private Selectable GetFirstEmployeeSelectable()
	{
		for (int i = 0; i < employeeCards.Count; i++)
		{
			if (employeeCards[i].GetSelectable().gameObject.activeSelf)
			{
				return employeeCards[i].GetSelectable();
			}
		}
		return null;
	}

	private void OnPauseButtonClicked()
	{
		bool activeSelf = pauseAllButton.gameObject.activeSelf;
		for (int i = 0; i < employeeCards.Count; i++)
		{
			if (employeeCards[i].gameObject.activeSelf && ((activeSelf && !employeeCards[i].CurrentEmployeeStats.isPaused) || (!activeSelf && employeeCards[i].CurrentEmployeeStats.isPaused)))
			{
				employeeCards[i].SwitchPauseState();
			}
		}
		pauseAllButton.gameObject.SetActive(!activeSelf);
		resumeAllButton.gameObject.SetActive(activeSelf);
		SingletonBehaviour<InputManager>.Instance.SelectElement(activeSelf ? resumeAllButton.gameObject : pauseAllButton.gameObject);
	}
}
