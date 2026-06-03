using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Layouts;

public class LastInputDeviceTracker : SingletonBehaviour<LastInputDeviceTracker>
{
	[Header("Assign this (GameInputActions.inputactions)")]
	[SerializeField]
	private InputActionAsset actions;

	[Header("Tuning")]
	[SerializeField]
	private float switchCooldown = 0.25f;

	[SerializeField]
	private float stickDeadzone = 0.25f;

	[SerializeField]
	private float axisDeadzone = 0.2f;

	[SerializeField]
	private float mouseDeltaSqrThreshold = 4f;

	private InputDevice lastUsedDevice;

	private float _lastSwitchTime;

	public LastInputDeviceType Mode { get; private set; }

	public bool UseGamepad => Mode == LastInputDeviceType.Gamepad;

	public event Action<LastInputDeviceType> OnModeChanged;

	private new void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(this);
		actions.Enable();
		foreach (InputActionMap actionMap in actions.actionMaps)
		{
			foreach (InputAction action in actionMap.actions)
			{
				action.performed += OnAnyActionPerformed;
			}
		}
	}

	private void OnAnyActionPerformed(InputAction.CallbackContext ctx)
	{
		if (IsMeaningful(ctx))
		{
			InputDevice inputDevice = (lastUsedDevice = ctx.control?.device);
			if (inputDevice is Gamepad)
			{
				TrySet(LastInputDeviceType.Gamepad);
			}
			else if (inputDevice is Keyboard || inputDevice is Mouse)
			{
				TrySet(LastInputDeviceType.KeyboardMouse);
			}
		}
	}

	private void TrySet(LastInputDeviceType newMode)
	{
		if (Mode != newMode && !(Time.unscaledTime - _lastSwitchTime < switchCooldown))
		{
			Mode = newMode;
			_lastSwitchTime = Time.unscaledTime;
			this.OnModeChanged?.Invoke(Mode);
			EventManager.NotifyEvent(UIEvents.INPUT_DEVICE_CHANGED, Mode);
		}
	}

	private bool IsMeaningful(InputAction.CallbackContext ctx)
	{
		InputControl control = ctx.control;
		if (control == null)
		{
			return false;
		}
		if (control.device is Gamepad)
		{
			if (control is StickControl stickControl)
			{
				return stickControl.ReadValue().sqrMagnitude > stickDeadzone * stickDeadzone;
			}
			if (control is AxisControl axisControl)
			{
				return Mathf.Abs(axisControl.ReadValue()) > axisDeadzone;
			}
			return true;
		}
		if (control.device is Mouse)
		{
			if (control.path.Contains("/position"))
			{
				return false;
			}
			if (control is DeltaControl deltaControl)
			{
				return deltaControl.ReadValue().sqrMagnitude > mouseDeltaSqrThreshold;
			}
			return true;
		}
		_ = control.device is Keyboard;
		return true;
	}

	public bool IsPSController()
	{
		Gamepad gamepad = (lastUsedDevice as Gamepad) ?? Gamepad.current;
		if (gamepad == null)
		{
			return false;
		}
		if (gamepad is DualShockGamepad)
		{
			return true;
		}
		string text = (gamepad.layout ?? "").ToLowerInvariant();
		if (text.Contains("dualsense") || text.Contains("dualshock"))
		{
			return true;
		}
		InputDeviceDescription description = gamepad.description;
		string text2 = (description.manufacturer + " " + description.product + " " + description.interfaceName).ToLowerInvariant();
		if (text2.Contains("sony") || text2.Contains("playstation") || text2.Contains("dualsense") || text2.Contains("dualshock"))
		{
			return true;
		}
		return false;
	}
}
