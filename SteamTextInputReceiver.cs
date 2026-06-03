using DFTGames.Localization;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SteamTextInputReceiver : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField target;

	[SerializeField]
	private bool useFloatingBottomNumeric;

	private Callback<GamepadTextInputDismissed_t> cb;

	private bool keyboardOpen;

	private bool callbackRegistered;

	private float nextOpenTime;

	private void OnEnable()
	{
		TryRegisterCallback();
		if (target != null)
		{
			target.onSelect.AddListener(OnTargetSelected);
		}
	}

	private void OnDisable()
	{
		if (callbackRegistered)
		{
			cb?.Unregister();
			callbackRegistered = false;
		}
		if (target != null)
		{
			target.onSelect.RemoveListener(OnTargetSelected);
		}
	}

	private void Update()
	{
		if (!callbackRegistered)
		{
			TryRegisterCallback();
		}
	}

	private void TryRegisterCallback()
	{
		if (!callbackRegistered && SteamManager.Initialized)
		{
			cb = Callback<GamepadTextInputDismissed_t>.Create(OnDismissed);
			callbackRegistered = true;
		}
	}

	private void OnTargetSelected(string _)
	{
		TryOpenKeyboard();
	}

	private void TryOpenKeyboard()
	{
		if (!(target == null) && SteamManager.Initialized && SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad && !keyboardOpen && !(Time.unscaledTime < nextOpenTime) && !(EventSystem.current.currentSelectedGameObject != target.gameObject))
		{
			if (useFloatingBottomNumeric ? GamepadKeyboard.OpenSteamFloatingBottomNumeric() : GamepadKeyboard.OpenSteam(Locale.GetWord("change_name_title"), target.characterLimit, target.text))
			{
				keyboardOpen = true;
			}
			nextOpenTime = Time.unscaledTime + 0.2f;
		}
	}

	private void OnDismissed(GamepadTextInputDismissed_t data)
	{
		keyboardOpen = false;
		if (!data.m_bSubmitted)
		{
			return;
		}
		uint enteredGamepadTextLength = SteamUtils.GetEnteredGamepadTextLength();
		if (enteredGamepadTextLength != 0)
		{
			if (SteamUtils.GetEnteredGamepadTextInput(out var pchText, enteredGamepadTextLength))
			{
				target.text = pchText;
				target.caretPosition = target.text.Length;
				target.ForceLabelUpdate();
			}
			EventSystem.current?.SetSelectedGameObject(target.gameObject);
		}
	}
}
