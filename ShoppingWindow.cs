using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Button shopButton;

	[SerializeField]
	private Transform shopButtonTarget;

	[SerializeField]
	private Image shopSelectedFG;

	[SerializeField]
	private TabbedPanel shopWindow;

	[SerializeField]
	private Button licensesButton;

	[SerializeField]
	private Transform licenseButtonTarget;

	[SerializeField]
	private Image licenseSelectedFG;

	[SerializeField]
	private TabbedPanel LicenseWindow;

	[SerializeField]
	private Button managementButton;

	[SerializeField]
	private Transform managementButtonTarget;

	[SerializeField]
	private Image managementSelectedFG;

	[SerializeField]
	private TabbedPanel managementWindow;

	[SerializeField]
	private Button bankButton;

	[SerializeField]
	private Transform bankButtonTarget;

	[SerializeField]
	private Image bankSelectedFG;

	[SerializeField]
	private TabbedPanel bankWindow;

	[SerializeField]
	private Transform targetWindowTransform;

	[SerializeField]
	private Transform poolWindowTransform;

	private Vector3 shopInitialPosition;

	private Vector3 licenseInitialPosition;

	private Vector3 managementInitialPosition;

	private Vector3 bankInitialPosition;

	private const int ITEMS_TAB_INDEX = 3;

	public void OnShopClicked()
	{
		OnButtonClicked(shopSelectedFG, shopButton, shopButtonTarget, shopWindow);
	}

	public void OnManagementClicked()
	{
		OnButtonClicked(managementSelectedFG, managementButton, managementButtonTarget, managementWindow);
	}

	public void OnBankClicked()
	{
		OnButtonClicked(bankSelectedFG, bankButton, bankButtonTarget, bankWindow);
	}

	public void OnLicensesClicked()
	{
		OnButtonClicked(licenseSelectedFG, licensesButton, licenseButtonTarget, LicenseWindow);
	}

	private void OnButtonClicked(Image selectedFG, Button button, Transform target, TabbedPanel window)
	{
		if (!selectedFG.enabled)
		{
			DeselectSelected();
			button.interactable = false;
			button.transform.DOKill();
			button.transform.DOMove(target.position, 0.2f);
			selectedFG.enabled = true;
			window.transform.DOKill();
			window.Repaint();
			window.transform.DOMove(targetWindowTransform.position, 0.2f);
			window.Open();
		}
	}

	private void SelectInstant(Image selectedFG, Button button, Transform target, TabbedPanel window)
	{
		DeselectSelectedInstant();
		window.transform.localPosition = targetWindowTransform.localPosition;
		button.interactable = false;
		button.transform.position = target.position;
		selectedFG.enabled = true;
		window.Repaint();
		window.Open();
	}

	private void DeselectSelected()
	{
		if (shopSelectedFG.enabled)
		{
			shopButton.interactable = true;
			shopButton.transform.DOKill();
			shopButton.transform.DOMove(shopInitialPosition, 0.2f);
			shopSelectedFG.enabled = false;
			shopWindow.transform.DOKill();
			shopWindow.transform.DOMove(poolWindowTransform.position, 0.2f).OnComplete(delegate
			{
				shopWindow.Close();
			});
		}
		if (licenseSelectedFG.enabled)
		{
			licensesButton.interactable = true;
			licensesButton.transform.DOKill();
			licensesButton.transform.DOMove(licenseInitialPosition, 0.2f);
			licenseSelectedFG.enabled = false;
			LicenseWindow.transform.DOKill();
			LicenseWindow.transform.DOMove(poolWindowTransform.position, 0.2f).OnComplete(delegate
			{
				LicenseWindow.Close();
			});
		}
		if (managementSelectedFG.enabled)
		{
			managementButton.interactable = true;
			managementButton.transform.DOKill();
			managementButton.transform.DOMove(managementInitialPosition, 0.2f);
			managementSelectedFG.enabled = false;
			managementWindow.transform.DOKill();
			managementWindow.transform.DOMove(poolWindowTransform.position, 0.2f).OnComplete(delegate
			{
				managementWindow.Close();
			});
		}
		if (bankSelectedFG.enabled)
		{
			bankButton.interactable = true;
			bankButton.transform.DOKill();
			bankButton.transform.DOMove(bankInitialPosition, 0.2f);
			bankSelectedFG.enabled = false;
			bankWindow.transform.DOKill();
			bankWindow.transform.DOMove(poolWindowTransform.position, 0.2f).OnComplete(delegate
			{
				bankWindow.Close();
			});
		}
	}

	private void DeselectSelectedInstant()
	{
		if (shopSelectedFG.enabled)
		{
			shopButton.interactable = true;
			shopButton.transform.position = shopInitialPosition;
			shopSelectedFG.enabled = false;
			shopWindow.transform.position = poolWindowTransform.position;
			shopWindow.Close();
		}
		if (licenseSelectedFG.enabled)
		{
			licensesButton.interactable = true;
			licensesButton.transform.position = licenseInitialPosition;
			licenseSelectedFG.enabled = false;
			LicenseWindow.transform.position = poolWindowTransform.position;
			LicenseWindow.Close();
		}
		if (managementSelectedFG.enabled)
		{
			managementButton.interactable = true;
			managementButton.transform.position = managementInitialPosition;
			managementSelectedFG.enabled = false;
			managementWindow.transform.position = poolWindowTransform.position;
			managementWindow.Close();
		}
		if (bankSelectedFG.enabled)
		{
			bankButton.interactable = true;
			bankButton.transform.position = bankInitialPosition;
			bankSelectedFG.enabled = false;
			bankWindow.transform.position = poolWindowTransform.position;
			bankWindow.Close();
		}
	}

	public void Open()
	{
		shopWindow.transform.position = poolWindowTransform.position;
		managementWindow.transform.position = poolWindowTransform.position;
		LicenseWindow.transform.position = poolWindowTransform.position;
		bankWindow.transform.position = poolWindowTransform.position;
		SelectInstant(shopSelectedFG, shopButton, shopButtonTarget, shopWindow);
		canvas.enabled = true;
		canvasGroup.blocksRaycasts = true;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
	}

	public void OpenBankInstant()
	{
		shopWindow.transform.position = poolWindowTransform.position;
		managementWindow.transform.position = poolWindowTransform.position;
		LicenseWindow.transform.position = poolWindowTransform.position;
		bankWindow.transform.position = poolWindowTransform.position;
		SelectInstant(bankSelectedFG, bankButton, bankButtonTarget, bankWindow);
		canvas.enabled = true;
		canvasGroup.blocksRaycasts = true;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
	}

	public void OnItemsClicked()
	{
		shopWindow.transform.position = poolWindowTransform.position;
		managementWindow.transform.position = poolWindowTransform.position;
		LicenseWindow.transform.position = poolWindowTransform.position;
		bankWindow.transform.position = poolWindowTransform.position;
		SelectInstant(managementSelectedFG, managementButton, managementButtonTarget, managementWindow);
		managementWindow.SelectButton(3, instant: true);
		canvas.enabled = true;
		canvasGroup.blocksRaycasts = true;
	}

	public bool IsOpen()
	{
		return canvas.enabled;
	}

	public void Close()
	{
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		canvas.enabled = false;
		canvasGroup.blocksRaycasts = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
	}

	private void Start()
	{
		shopInitialPosition = shopButton.transform.position;
		licenseInitialPosition = licensesButton.transform.position;
		managementInitialPosition = managementButton.transform.position;
		bankInitialPosition = bankButton.transform.position;
	}

	private void Update()
	{
	}

	public void OnWishlistNow()
	{
		Application.OpenURL("steam://store/3819640");
	}
}
