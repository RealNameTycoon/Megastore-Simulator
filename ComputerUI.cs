using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ComputerUI : FocusableUI
{
	[SerializeField]
	private ComputerTabManager tabManager;

	[SerializeField]
	private Button shoppingButton;

	[SerializeField]
	private Button managementButton;

	[SerializeField]
	private Button priceManagementButton;

	[SerializeField]
	private Button licenseButton;

	[SerializeField]
	private Button statisticsButton;

	[SerializeField]
	private Button bankButton;

	[SerializeField]
	private Button gamesButton;

	[SerializeField]
	private FocusableUI focusableUI;

	[SerializeField]
	private UIWindow fullReleaseWindow;

	[SerializeField]
	private Canvas fullReleasePopup;

	[SerializeField]
	private GraphicRaycaster fullReleaseRaycaster;

	private bool employeeHired;

	private EmployeeRole hiredEmployeeRole;

	private void Start()
	{
		shoppingButton.onClick.AddListener(delegate
		{
			OnButtonClicked(WindowTypes.SHOPPING);
			EventManager.NotifyEvent(UIEvents.SHOP_WINDOW_OPENED);
		});
		EventManager.AddListener<HiringManager.EmployeeStats>(SupermarketEvents.EMPLOYEE_HIRED, OnEmployeeHired);
		managementButton.onClick.AddListener(delegate
		{
			OnButtonClicked(WindowTypes.MANAGEMENT);
		});
		priceManagementButton.onClick.AddListener(delegate
		{
			OnButtonClicked(WindowTypes.PRICE_MANAGEMENT);
		});
		licenseButton.onClick.AddListener(delegate
		{
			OnButtonClicked(WindowTypes.LICENSES);
		});
		bankButton.onClick.AddListener(delegate
		{
			OnButtonClicked(WindowTypes.BANK);
		});
		statisticsButton.onClick.AddListener(delegate
		{
			OnButtonClicked(WindowTypes.STATISTICS);
		});
		gamesButton.onClick.AddListener(OpenFullReleasePopup);
		onFocusRemovedAction = (Action)Delegate.Combine(onFocusRemovedAction, (Action)delegate
		{
			tabManager.MinimizeAllTabs();
			if (fullReleasePopup.enabled)
			{
				fullReleasePopup.enabled = false;
				fullReleaseRaycaster.enabled = false;
				fullReleaseWindow.Close();
			}
		});
	}

	private void OnEmployeeHired(HiringManager.EmployeeStats employeeStats)
	{
		hiredEmployeeRole = employeeStats.role;
		employeeHired = true;
	}

	private void OpenFullReleasePopup()
	{
		if (!fullReleasePopup.enabled)
		{
			fullReleasePopup.transform.localScale = Vector3.one * 0.2f;
			fullReleasePopup.enabled = true;
			fullReleaseRaycaster.enabled = true;
			fullReleasePopup.transform.DOScale(Vector3.one, 0.25f);
			fullReleaseWindow.Open();
		}
	}

	private void OnButtonClicked(WindowTypes type)
	{
		tabManager.SelectTab(type);
	}

	public override void OnUIClicked()
	{
		base.OnUIClicked();
		employeeHired = false;
	}

	public override void RemoveFocus()
	{
		base.RemoveFocus();
		if (employeeHired)
		{
			StartCoroutine(OpenTutorialRoutine());
		}
	}

	private IEnumerator OpenTutorialRoutine()
	{
		yield return new WaitForSeconds(2f);
		if (!SingletonBehaviour<UIStackManager>.Instance.IsAnyWindowOpen() && GameManager.GetSaveVersion() >= 1)
		{
			if (hiredEmployeeRole == EmployeeRole.BAKERY_STAFF)
			{
				SingletonWindow<TutorialVideoWindowUI>.Instance.Open(TutorialVideoWindowType.BAKER_TUTORIAL);
			}
			else if (hiredEmployeeRole == EmployeeRole.RESTOCKER)
			{
				SingletonWindow<TutorialVideoWindowUI>.Instance.Open(TutorialVideoWindowType.RESTOCKER_TUTORIAL);
			}
			else if (hiredEmployeeRole == EmployeeRole.UNLOADER)
			{
				SingletonWindow<TutorialVideoWindowUI>.Instance.Open(TutorialVideoWindowType.UNLOADER_TUTORIAL);
			}
			else if (hiredEmployeeRole == EmployeeRole.SEAFOOD_STAFF)
			{
				SingletonWindow<TutorialVideoWindowUI>.Instance.Open(TutorialVideoWindowType.SEAFOOD_STAFF_TUTORIAL);
			}
		}
	}
}
