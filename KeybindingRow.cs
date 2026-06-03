using System;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeybindingRow : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private TextMeshProUGUI actionText;

	[SerializeField]
	private TextMeshProUGUI keyText;

	[SerializeField]
	private TextMeshProUGUI gamepadText;

	[SerializeField]
	private Button rebindButton;

	[SerializeField]
	private Button rebindGamepadButton;

	private InputActionReference _actionRef;

	private int keyboardBindingIndex;

	private int gamepadBindingIndex;

	private string labelKey;

	private Action onSelected;

	public Button RebindButton => rebindButton;

	public Button RebindGamepadButton => rebindGamepadButton;

	public void SetOnSelected(Action onSelected)
	{
		this.onSelected = onSelected;
	}

	public void SetUpAndDownSelectables(GameObject keyboardUp, GameObject keyboardDown, GameObject gamepadUp, GameObject gamepadDown)
	{
		Navigation navigation = rebindButton.navigation;
		rebindButton.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = ((keyboardUp != null) ? keyboardUp.GetComponent<Selectable>() : navigation.selectOnUp),
			selectOnDown = ((keyboardDown != null) ? keyboardDown.GetComponent<Selectable>() : navigation.selectOnDown),
			selectOnLeft = rebindGamepadButton,
			selectOnRight = rebindGamepadButton
		};
		Navigation navigation2 = rebindButton.navigation;
		rebindGamepadButton.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = ((gamepadUp != null) ? gamepadUp.GetComponent<Selectable>() : navigation2.selectOnUp),
			selectOnDown = ((gamepadDown != null) ? gamepadDown.GetComponent<Selectable>() : navigation2.selectOnDown),
			selectOnLeft = rebindButton,
			selectOnRight = rebindButton
		};
	}

	public void Repaint(InputActionReference actionRef, int keyboardBindingIndex = 0, int gamepadBindingIndex = 0, string labelKey = null)
	{
		_actionRef = actionRef;
		this.keyboardBindingIndex = keyboardBindingIndex;
		this.gamepadBindingIndex = gamepadBindingIndex;
		this.labelKey = labelKey;
		actionText.text = Locale.GetWord(this.labelKey);
		keyText.text = GetBindingDisplay(actionRef, keyboardBindingIndex);
		if (gamepadBindingIndex != -1)
		{
			gamepadText.text = GetBindingDisplay(actionRef, gamepadBindingIndex);
		}
		else
		{
			rebindGamepadButton.gameObject.SetActive(value: false);
		}
		if (rebindButton != null)
		{
			rebindButton.onClick.RemoveAllListeners();
			rebindButton.onClick.AddListener(BeginRebind);
		}
		if (rebindGamepadButton != null)
		{
			rebindGamepadButton.onClick.RemoveAllListeners();
			rebindGamepadButton.onClick.AddListener(BeginRebindGamepad);
		}
	}

	public void BeginRebind()
	{
		if (!(_actionRef == null) && _actionRef.action != null)
		{
			keyText.text = Locale.GetWord("press_a_key");
			SingletonBehaviour<KeyBindingManager>.Instance.StartRebind(_actionRef, keyboardBindingIndex, delegate
			{
				keyText.text = GetBindingDisplay(_actionRef, keyboardBindingIndex);
			}, delegate
			{
				keyText.text = GetBindingDisplay(_actionRef, keyboardBindingIndex);
			});
		}
	}

	public void BeginRebindGamepad()
	{
		if (!(_actionRef == null) && _actionRef.action != null)
		{
			gamepadText.text = Locale.GetWord("press_a_key");
			SingletonBehaviour<KeyBindingManager>.Instance.StartRebind(_actionRef, gamepadBindingIndex, delegate
			{
				gamepadText.text = GetBindingDisplay(_actionRef, gamepadBindingIndex);
			}, delegate
			{
				gamepadText.text = GetBindingDisplay(_actionRef, gamepadBindingIndex);
			});
		}
	}

	public static string GetBindingDisplay(InputActionReference actionRef, int bindingIndex)
	{
		if (actionRef?.action == null)
		{
			return "-";
		}
		InputAction action = actionRef.action;
		if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
		{
			return "-";
		}
		string deviceLayoutName;
		string controlPath;
		return action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.DontIncludeInteractions);
	}
}
