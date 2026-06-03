using System;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI actionText;

	[SerializeField]
	private GameObject buttonUI;

	[SerializeField]
	private Image leftMB;

	[SerializeField]
	private Image rightMB;

	[SerializeField]
	private Image middleMB;

	[SerializeField]
	private GameObject spaceUI;

	[SerializeField]
	private GameObject shiftUI;

	[SerializeField]
	private TextMeshProUGUI longButtonText;

	[SerializeField]
	private TextMeshProUGUI keycodeText;

	[Header("Gamepad Icons (Optional)")]
	[SerializeField]
	private GameObject xboxLetterButton;

	[SerializeField]
	private TextMeshProUGUI xboxLetterText;

	[SerializeField]
	private Image gamepadIconImage;

	private Action buttonAction;

	private InputActionReference currentActionRef;

	public InputActionReference CurrentActionReference => currentActionRef;

	public Action CurrentAction => buttonAction;

	public void Repaint(string key, Action action)
	{
		buttonAction = action;
		actionText.text = Locale.GetWord(key);
		currentActionRef = null;
	}

	public void Repaint(KeyCode keyCode, string key, Action action)
	{
		buttonUI.SetActive(keyCode != KeyCode.Mouse0 && keyCode != KeyCode.Mouse1 && keyCode != KeyCode.Mouse2 && keyCode != KeyCode.Space && keyCode != KeyCode.LeftShift && keyCode != KeyCode.LeftControl && keyCode != KeyCode.Tab);
		leftMB.gameObject.SetActive(keyCode == KeyCode.Mouse0);
		rightMB.gameObject.SetActive(keyCode == KeyCode.Mouse1);
		middleMB.gameObject.SetActive(keyCode == KeyCode.Mouse2);
		spaceUI.gameObject.SetActive(keyCode == KeyCode.Space);
		shiftUI.gameObject.SetActive(keyCode == KeyCode.LeftShift || keyCode == KeyCode.LeftControl || keyCode == KeyCode.Tab);
		longButtonText.text = keyCode switch
		{
			KeyCode.LeftControl => "Ctrl", 
			KeyCode.LeftShift => "Shift", 
			_ => "Tab", 
		};
		if (gamepadIconImage != null)
		{
			gamepadIconImage.gameObject.SetActive(value: false);
		}
		if (keyCode == KeyCode.Escape)
		{
			keycodeText.text = "Esc";
		}
		else
		{
			keycodeText.text = keyCode.ToString();
		}
		buttonAction = action;
		actionText.text = Locale.GetWord(key);
		currentActionRef = null;
	}

	public void Repaint(InputActionReference actionRef, string actionDescription, Action action, bool useGamepad = false)
	{
		currentActionRef = actionRef;
		buttonAction = action;
		actionText.text = actionDescription;
		if (actionRef == null || actionRef.action == null)
		{
			HideAllInputIcons();
			keycodeText.text = "";
		}
		else
		{
			string bindingPath = GetBindingPath(actionRef, useGamepad);
			DisplayBindingIcon(bindingPath, useGamepad);
		}
	}

	private string GetBindingPath(InputActionReference actionRef, bool useGamepad)
	{
		if (actionRef?.action == null)
		{
			return "";
		}
		ReadOnlyArray<InputBinding> bindings = actionRef.action.bindings;
		string value = (useGamepad ? "Gamepad" : "KeyboardMouse");
		for (int i = 0; i < bindings.Count; i++)
		{
			InputBinding inputBinding = bindings[i];
			if (!inputBinding.isComposite && !inputBinding.isPartOfComposite && (string.IsNullOrEmpty(inputBinding.groups) || inputBinding.groups.Contains(value)))
			{
				return inputBinding.effectivePath;
			}
		}
		for (int j = 0; j < bindings.Count; j++)
		{
			if (!bindings[j].isComposite && !bindings[j].isPartOfComposite)
			{
				return bindings[j].effectivePath;
			}
		}
		return "";
	}

	private void DisplayBindingIcon(string bindingPath, bool useGamepad)
	{
		HideAllInputIcons();
		if (string.IsNullOrEmpty(bindingPath))
		{
			keycodeText.text = "?";
			buttonUI.SetActive(value: true);
			return;
		}
		bindingPath = bindingPath.ToLower();
		if (!useGamepad)
		{
			if (bindingPath.Contains("leftbutton") || bindingPath.Contains("mouse0"))
			{
				leftMB.gameObject.SetActive(value: true);
			}
			else if (bindingPath.Contains("rightbutton") || bindingPath.Contains("mouse1"))
			{
				rightMB.gameObject.SetActive(value: true);
			}
			else if (bindingPath.Contains("scroll") || bindingPath.Contains("mouse2"))
			{
				middleMB.gameObject.SetActive(value: true);
			}
			else if (bindingPath.Contains("space"))
			{
				spaceUI.SetActive(value: true);
			}
			else if (bindingPath.Contains("leftshift"))
			{
				shiftUI.SetActive(value: true);
				longButtonText.text = "Shift";
			}
			else if (bindingPath.Contains("leftctrl") || bindingPath.Contains("leftcontrol"))
			{
				shiftUI.SetActive(value: true);
				longButtonText.text = "Ctrl";
			}
			else if (bindingPath.Contains("tab"))
			{
				shiftUI.SetActive(value: true);
				longButtonText.text = "Tab";
			}
			else if (bindingPath.Contains("escape"))
			{
				buttonUI.SetActive(value: true);
				keycodeText.text = "Esc";
			}
			else
			{
				buttonUI.SetActive(value: true);
				keycodeText.text = GetKeyDisplayName(bindingPath);
			}
		}
		else if (useGamepad)
		{
			Sprite gamepadSprite = SingletonBehaviour<InputManager>.Instance.GetGamepadSprite(bindingPath);
			if (gamepadSprite != null)
			{
				xboxLetterButton.SetActive(value: true);
				gamepadIconImage.sprite = gamepadSprite;
			}
			else
			{
				buttonUI.SetActive(value: true);
				keycodeText.text = GetGamepadButtonName(bindingPath);
			}
		}
		else
		{
			buttonUI.SetActive(value: true);
			keycodeText.text = GetGamepadButtonName(bindingPath);
		}
	}

	private void HideAllInputIcons()
	{
		buttonUI.SetActive(value: false);
		leftMB.gameObject.SetActive(value: false);
		rightMB.gameObject.SetActive(value: false);
		middleMB.gameObject.SetActive(value: false);
		spaceUI.SetActive(value: false);
		shiftUI.SetActive(value: false);
		if (xboxLetterButton != null)
		{
			xboxLetterButton.SetActive(value: false);
		}
	}

	private string GetKeyDisplayName(string bindingPath)
	{
		int num = bindingPath.LastIndexOf('/');
		if (num >= 0 && num < bindingPath.Length - 1)
		{
			string text = bindingPath.Substring(num + 1);
			if (text.Length > 0)
			{
				return char.ToUpper(text[0]) + ((text.Length > 1) ? text.Substring(1) : "");
			}
		}
		return bindingPath;
	}

	private string GetGamepadButtonName(string bindingPath)
	{
		if (bindingPath.Contains("buttonsouth"))
		{
			return "A";
		}
		if (bindingPath.Contains("buttoneast"))
		{
			return "B";
		}
		if (bindingPath.Contains("buttonwest"))
		{
			return "X";
		}
		if (bindingPath.Contains("buttonnorth"))
		{
			return "Y";
		}
		if (bindingPath.Contains("lefttrigger"))
		{
			return "LT";
		}
		if (bindingPath.Contains("righttrigger"))
		{
			return "RT";
		}
		if (bindingPath.Contains("leftshoulder"))
		{
			return "LB";
		}
		if (bindingPath.Contains("rightshoulder"))
		{
			return "RB";
		}
		if (bindingPath.Contains("select"))
		{
			return "Back";
		}
		if (bindingPath.Contains("start"))
		{
			return "Start";
		}
		if (bindingPath.Contains("dpad/up"))
		{
			return "D-Up";
		}
		if (bindingPath.Contains("dpad/down"))
		{
			return "D-Down";
		}
		if (bindingPath.Contains("dpad/left"))
		{
			return "D-Left";
		}
		if (bindingPath.Contains("dpad/right"))
		{
			return "D-Right";
		}
		if (bindingPath.Contains("leftstickpress"))
		{
			return "L3";
		}
		if (bindingPath.Contains("rightstickpress"))
		{
			return "R3";
		}
		int num = bindingPath.LastIndexOf('/');
		if (num >= 0 && num < bindingPath.Length - 1)
		{
			return bindingPath.Substring(num + 1).ToUpper();
		}
		return "?";
	}

	public void EmulateClick()
	{
		buttonAction?.Invoke();
	}
}
