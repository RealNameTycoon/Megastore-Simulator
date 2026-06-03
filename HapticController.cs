using UnityEngine;

public class HapticController : MonoBehaviour
{
	private static string hapticEnabledKey = "hapticEnabledKey";

	private static bool hapticEnabled = false;

	public static bool HapticEnabled => hapticEnabled;

	private void Awake()
	{
		hapticEnabled = false;
	}

	public static void Vibrate(PresetType hapticType)
	{
	}

	public static void ToggleVibration()
	{
		hapticEnabled = !hapticEnabled;
		GenericDataSerializer.SaveBool(hapticEnabledKey, hapticEnabled);
	}
}
