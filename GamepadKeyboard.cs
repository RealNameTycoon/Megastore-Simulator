using Steamworks;
using UnityEngine;

public static class GamepadKeyboard
{
	public static bool OpenSteam(string description, int maxChars, string existingText = "")
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		int num = Mathf.RoundToInt((float)Screen.width * 0.7f);
		int nTextFieldHeight = 120;
		int nTextFieldXPosition = (Screen.width - num) / 2;
		int nTextFieldYPosition = Screen.height - 150;
		return SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, nTextFieldXPosition, nTextFieldYPosition, num, nTextFieldHeight);
	}

	public static bool OpenSteamFloatingBottomNumeric()
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		int num = Mathf.RoundToInt((float)Screen.width * 0.55f);
		int nTextFieldHeight = 100;
		int nTextFieldXPosition = (Screen.width - num) / 2;
		int nTextFieldYPosition = Screen.height - 130;
		return SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeNumeric, nTextFieldXPosition, nTextFieldYPosition, num, nTextFieldHeight);
	}
}
