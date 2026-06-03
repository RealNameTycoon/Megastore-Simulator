using System;
using System.Collections.Generic;
using DFTGames.Localization;
using UnityEngine;
using UnityEngine.UI;

public class LanguageChanger : MonoBehaviour
{
	[SerializeField]
	private Dropdown dropdown;

	private List<Dropdown.OptionData> optionDatas;

	private Dictionary<int, CustomSystemLanguage> intToLanguage;

	private Dictionary<CustomSystemLanguage, int> languageToInt;

	private void Start()
	{
		intToLanguage = new Dictionary<int, CustomSystemLanguage>();
		languageToInt = new Dictionary<CustomSystemLanguage, int>();
		dropdown.ClearOptions();
		optionDatas = new List<Dropdown.OptionData>();
		Enum.GetNames(typeof(CustomSystemLanguage));
		Enum.GetValues(typeof(CustomSystemLanguage));
		List<CustomSystemLanguage> list = Locale.AvailableLanguages();
		List<string> list2 = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(list[i].ToString());
			intToLanguage.Add(i, list[i]);
			languageToInt.Add(list[i], i);
		}
		dropdown.AddOptions(list2);
		dropdown.onValueChanged.AddListener(LanguageChanged);
		dropdown.value = languageToInt[Locale.PlayerLanguage];
	}

	private void LanguageChanged(int newLanguage)
	{
		LocalizeBase.SetCurrentLanguage(intToLanguage[newLanguage]);
	}
}
