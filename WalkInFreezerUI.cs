using DFTGames.Localization;
using TMPro;
using UnityEngine;

public class WalkInFreezerUI : UIWindow
{
	[SerializeField]
	private TextMeshProUGUI temperatureText;

	[SerializeField]
	private TextMeshProUGUI temperatureModeText;

	[SerializeField]
	private TextMeshProUGUI temperatureSymbolText;

	private const string freezerModePrefix = "freezer_mode_";

	private const string celsiusSymbol = "℃";

	private const string fahrenheitSymbol = "℉";

	public void SetTemperature(float temperature)
	{
		temperatureText.text = GetTemperatureText(temperature);
		StorageRequirement storageRequirement = GetStorageRequirement(temperature);
		temperatureModeText.text = Locale.GetWord("freezer_mode_" + storageRequirement);
		temperatureSymbolText.text = (SingletonWindow<SettingsWindow>.Instance.UseFahrenheit ? "℉" : "℃");
	}

	private string GetTemperatureText(float temperature)
	{
		if (!SingletonWindow<SettingsWindow>.Instance.UseFahrenheit)
		{
			return ((int)temperature).ToString();
		}
		return ((int)temperature * 9 / 5 + 32).ToString();
	}

	private StorageRequirement GetStorageRequirement(float temperature)
	{
		for (int i = 0; i < 4; i++)
		{
			StorageRequirement storageRequirement = (StorageRequirement)i;
			if (storageRequirement != StorageRequirement.None)
			{
				int num = BoxManager.storageRequirementToMinDegree[storageRequirement];
				int num2 = BoxManager.storageRequirementToMaxDegree[storageRequirement];
				if (temperature >= (float)num && temperature <= (float)num2)
				{
					return storageRequirement;
				}
			}
		}
		return StorageRequirement.None;
	}
}
