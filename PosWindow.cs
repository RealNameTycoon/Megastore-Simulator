using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PosWindow : UIWindow
{
	[SerializeField]
	private CheckoutManager checkoutManager;

	[SerializeField]
	private BoxCollider posDeviceClickableCollider;

	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	private TextMeshProUGUI inputText;

	[SerializeField]
	private Transform posLockTarget;

	[SerializeField]
	private GameObject interactableKeys;

	[SerializeField]
	private Image fakeKeys;

	private float requiredValue = -1f;

	private Vector3 initialPosition;

	private string inputString = string.Empty;

	private bool tooltipOpened;

	public bool isLocked => raycaster.enabled;

	private void Start()
	{
		EventManager.AddListener(SupermarketEvents.FIRE_STARTED, delegate
		{
			if (raycaster.enabled)
			{
				Close();
			}
		});
	}

	public void Open(float requiredValue)
	{
		posDeviceClickableCollider.enabled = true;
		this.requiredValue = requiredValue;
		fakeKeys.enabled = false;
		interactableKeys.SetActive(value: true);
	}

	public override bool IsOpen()
	{
		return requiredValue != -1f;
	}

	public void RepaintButtonsForPos()
	{
		if (raycaster.enabled)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
				("leave", delegate
				{
					posDeviceClickableCollider.enabled = true;
					raycaster.enabled = false;
					base.Close();
					SingletonBehaviour<PlayerLook>.Instance.UnlockCamera();
					checkoutManager.RepaintButtonsForCheckout();
				})
			} });
		}
	}

	public void EnableClickableCollider(bool state)
	{
		posDeviceClickableCollider.enabled = state;
	}

	public void OnPosClicked()
	{
		posDeviceClickableCollider.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.RotationLocked = true;
		raycaster.enabled = true;
		base.Open();
		checkoutManager.RepaintButtonsForCheckout();
		SingletonBehaviour<PlayerLook>.Instance.LockToTransform(posLockTarget);
	}

	public new void Close()
	{
		posDeviceClickableCollider.enabled = false;
		requiredValue = -1f;
		raycaster.enabled = false;
		base.Close();
		fakeKeys.enabled = true;
		interactableKeys.SetActive(value: false);
		SingletonBehaviour<PlayerLook>.Instance.UnlockCamera();
		checkoutManager.RepaintButtonsForCheckout();
	}

	private void Update()
	{
		if (requiredValue == -1f || !raycaster.enabled)
		{
			return;
		}
		for (KeyCode keyCode = KeyCode.Alpha0; keyCode <= KeyCode.Alpha9; keyCode++)
		{
			if (Input.GetKeyDown(keyCode))
			{
				OnButtonPressed(((int)(keyCode - 48)).ToString());
			}
		}
		for (KeyCode keyCode2 = KeyCode.Keypad0; keyCode2 <= KeyCode.Keypad9; keyCode2++)
		{
			if (Input.GetKeyDown(keyCode2))
			{
				OnButtonPressed(((int)(keyCode2 - 256)).ToString());
			}
		}
		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
		{
			OnDelete();
		}
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			OnOk();
		}
		string text = Input.inputString;
		if (text.Contains('.') || text.Contains(',') || Input.GetKeyDown(KeyCode.KeypadPeriod))
		{
			OnButtonPressed(".");
		}
	}

	public void OnButtonPressed(string number)
	{
		if (inputString.Length < 10)
		{
			checkoutManager.PlayPosAudio(AudioManager.AudioTypes.POS_CLICK);
			HapticController.Vibrate(PresetType.LightImpact);
			inputString += number;
			inputText.text = inputString;
			EventSystem.current.SetSelectedGameObject(null);
			if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
			{
				TryAcceptPayment();
			}
		}
	}

	public void OnDelete()
	{
		if (inputString != string.Empty)
		{
			inputString = inputString.Remove(inputString.Length - 1);
			inputText.text = inputString;
			checkoutManager.PlayPosAudio(AudioManager.AudioTypes.POS_CLICK);
			HapticController.Vibrate(PresetType.LightImpact);
		}
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void OnCancel()
	{
		if (inputString != string.Empty)
		{
			inputString = "";
			inputText.text = inputString;
			checkoutManager.PlayPosAudio(AudioManager.AudioTypes.POS_CLICK);
			HapticController.Vibrate(PresetType.LightImpact);
		}
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void OnOk()
	{
		if (float.TryParse(inputString.Replace(".", ","), NumberStyles.Any, new NumberFormatInfo
		{
			NumberDecimalSeparator = ","
		}, out var result) && (double)Mathf.Abs(result - requiredValue) < 0.005)
		{
			EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, requiredValue * checkoutManager.BoostMultiplier());
			EventManager.NotifyEvent(PaymentEvents.POS_PAYMENT_DONE, requiredValue);
			checkoutManager.OnPaymentFinished(requiredValue);
			inputString = string.Empty;
			inputText.text = inputString;
			requiredValue = -1f;
			Close();
			if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
			{
				SingletonBehaviour<TooltipUI>.Instance.Close();
			}
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.PAYMENT_DONE);
			HapticController.Vibrate(PresetType.LightImpact);
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_wrong_value", base.transform);
		}
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void TryAcceptPayment()
	{
		if (float.TryParse(inputString.Replace(".", ","), NumberStyles.Any, new NumberFormatInfo
		{
			NumberDecimalSeparator = ","
		}, out var result) && (double)Mathf.Abs(result - requiredValue) < 0.005)
		{
			EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, requiredValue * checkoutManager.BoostMultiplier());
			EventManager.NotifyEvent(PaymentEvents.POS_PAYMENT_DONE, requiredValue);
			checkoutManager.OnPaymentFinished(requiredValue);
			inputString = string.Empty;
			inputText.text = inputString;
			requiredValue = -1f;
			Close();
			if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
			{
				SingletonBehaviour<TooltipUI>.Instance.Close();
			}
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.PAYMENT_DONE);
			HapticController.Vibrate(PresetType.LightImpact);
		}
	}
}
