using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HiringWindow : TabWindow
{
	[SerializeField]
	private Button hiringTabButton;

	[SerializeField]
	private TMP_Dropdown filterDropdown;

	[SerializeField]
	private List<HiringCard> hiringCards;

	[SerializeField]
	private TextMeshProUGUI capacityText;

	[SerializeField]
	private TextMeshProUGUI nextIncreaseText;

	[SerializeField]
	private TextMeshProUGUI noApplicationText;

	[SerializeField]
	private Button descriptionWindowButton;

	[SerializeField]
	private Canvas descriptionWindowCanvas;

	[SerializeField]
	private TextMeshProUGUI descriptionWindowTitle;

	[SerializeField]
	private TextMeshProUGUI descriptionWindowText;

	private const int ITEM_PER_ROW = 3;

	public static List<EmployeeRole> EMPLOYEE_ROLES = new List<EmployeeRole>
	{
		EmployeeRole.CASHIER,
		EmployeeRole.RESTOCKER,
		EmployeeRole.BAKERY_STAFF,
		EmployeeRole.UNLOADER,
		EmployeeRole.SEAFOOD_STAFF
	};

	public static List<string> SHIFT_OPTIONS = new List<string> { "dropdown_early_shift", "dropdown_late_shift" };

	private void Awake()
	{
		EventManager.AddListener(StartupEvents.SPAWN_MANAGER_INITIALIZED, InitializeWindow);
	}

	private void InitializeWindow()
	{
		SetFilterOptions();
		SetShiftOptions();
		SetCheckoutOptions();
		filterDropdown.onValueChanged.AddListener(OnFilterChanged);
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
		EventManager.AddListener<FurnitureType>(PlaceableEvents.NEW_FURNITURE_PLACED, OnNewFurniturePlaced);
		EventManager.AddListener<FurnitureType>(PlaceableEvents.FURNITURE_REMOVED, OnNewFurniturePlaced);
		EventManager.AddListener<int>(EconomyEvents.LEVEL_UP, delegate
		{
			RepaintCapacity();
		});
		EventManager.AddListener<HiringManager.EmployeeStats>(SupermarketEvents.EMPLOYEE_HIRED, delegate
		{
			OnEmployeeHired();
		});
		EventManager.AddListener<HiringManager.EmployeeStats>(SupermarketEvents.EMPLOYEE_FIRED, delegate
		{
			RepaintCapacity();
		});
		descriptionWindowButton.onClick.AddListener(OpenDescriptionWindow);
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
		if (hiringTabButton == null || hiringCards == null)
		{
			return;
		}
		Navigation navigation = filterDropdown.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnDown = GetFirstHiringSelectable();
		navigation.selectOnUp = hiringTabButton;
		filterDropdown.navigation = navigation;
		Selectable[] slots = new Selectable[hiringCards.Count];
		for (int i = 0; i < hiringCards.Count; i++)
		{
			HiringCard hiringCard = hiringCards[i];
			if (hiringCard == null || !hiringCard.gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else if (!hiringCard.GetSelectable().gameObject.activeSelf)
			{
				slots[i] = null;
			}
			else
			{
				slots[i] = hiringCard.GetSelectable();
			}
		}
		for (int j = 0; j < hiringCards.Count; j++)
		{
			HiringCard hiringCard2 = hiringCards[j];
			if (!(hiringCard2 == null) && hiringCard2.gameObject.activeSelf && !(slots[j] == null))
			{
				int rowLock = j / 3;
				Selectable up = FindNext(j, -3, -1) ?? filterDropdown;
				Selectable down = FindNext(j, 3, -1);
				Selectable left = FindNext(j, -1, rowLock);
				Selectable right = FindNext(j, 1, rowLock);
				hiringCard2.RefreshNavigation(up, down, left, right);
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

	private void OpenDescriptionWindow()
	{
		descriptionWindowCanvas.enabled = true;
	}

	private void OnNewFurniturePlaced(FurnitureType type)
	{
		if (type == FurnitureType.CHECKOUT_DESK)
		{
			SetCheckoutOptions();
		}
	}

	public override void Open()
	{
		if (descriptionWindowCanvas.enabled)
		{
			descriptionWindowCanvas.enabled = false;
		}
		base.Open();
	}

	private void SetFilterOptions()
	{
		List<string> list = new List<string>();
		foreach (EmployeeRole eMPLOYEE_ROLE in EMPLOYEE_ROLES)
		{
			list.Add(Locale.GetWord(eMPLOYEE_ROLE.ToString()));
		}
		filterDropdown.ClearOptions();
		filterDropdown.AddOptions(list);
	}

	private void SetShiftOptions()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < SHIFT_OPTIONS.Count; i++)
		{
			list.Add(Locale.GetWord(SHIFT_OPTIONS[i]));
		}
		for (int j = 0; j < hiringCards.Count; j++)
		{
			hiringCards[j].SetShiftOptions(list);
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
		for (int j = 0; j < hiringCards.Count; j++)
		{
			hiringCards[j].SetCheckoutOptions(list, list2);
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

	private List<HiringManager.EmployeeStats> FilterApplications(int index)
	{
		List<HiringManager.EmployeeStats> list = new List<HiringManager.EmployeeStats>();
		EmployeeRole employeeRole = EMPLOYEE_ROLES[index];
		descriptionWindowText.text = Locale.GetWord(employeeRole.ToString() + "_desc");
		descriptionWindowTitle.text = Locale.GetWord(employeeRole.ToString() + "_title");
		for (int i = 0; i < SingletonBehaviour<HiringManager>.Instance.EmployeeApplications.Count; i++)
		{
			if (SingletonBehaviour<HiringManager>.Instance.EmployeeApplications[i].role == EMPLOYEE_ROLES[index])
			{
				list.Add(SingletonBehaviour<HiringManager>.Instance.EmployeeApplications[i]);
			}
		}
		return list;
	}

	private void OnFilterChanged(int index)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.MOUSE_CLICK);
		RefreshApplications();
	}

	public void RefreshApplications()
	{
		List<HiringManager.EmployeeStats> employeeApplications = FilterApplications(filterDropdown.value);
		Repaint(employeeApplications);
		RefreshNavigation();
	}

	private void Repaint(List<HiringManager.EmployeeStats> employeeApplications)
	{
		for (int i = 0; i < hiringCards.Count; i++)
		{
			if (i < employeeApplications.Count)
			{
				hiringCards[i].gameObject.SetActive(value: true);
				hiringCards[i].Repaint(employeeApplications[i]);
			}
			else
			{
				hiringCards[i].gameObject.SetActive(value: false);
			}
		}
		noApplicationText.enabled = employeeApplications.Count == 0;
		RepaintCapacity();
	}

	private void OnEmployeeHired()
	{
		RefreshApplications();
		RepaintCapacity();
		Selectable firstHiringSelectable = GetFirstHiringSelectable();
		if (firstHiringSelectable != null)
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(firstHiringSelectable.gameObject);
		}
		else
		{
			SingletonBehaviour<InputManager>.Instance.SelectElement(hiringTabButton.gameObject);
		}
	}

	private void RepaintCapacity()
	{
		int employeeCapacity = SingletonBehaviour<HiringManager>.Instance.GetEmployeeCapacity();
		capacityText.text = SingletonBehaviour<EmployeeManager>.Instance.GetHiredEmployeesCount() + "/" + employeeCapacity;
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

	public override Selectable GetFirstSelectable()
	{
		if (filterDropdown.gameObject.activeSelf)
		{
			return filterDropdown;
		}
		for (int i = 0; i < hiringCards.Count; i++)
		{
			if (hiringCards[i].GetSelectable().gameObject.activeSelf)
			{
				return hiringCards[i].GetSelectable();
			}
		}
		return null;
	}

	private Selectable GetFirstHiringSelectable()
	{
		for (int i = 0; i < hiringCards.Count; i++)
		{
			if (hiringCards[i].gameObject.activeSelf && hiringCards[i].GetSelectable().gameObject.activeSelf)
			{
				return hiringCards[i].GetSelectable();
			}
		}
		return null;
	}
}
