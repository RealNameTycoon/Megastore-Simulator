using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsWindow : SingletonWindow<SettingsWindow>
{
	[SerializeField]
	private RectTransform window;

	[SerializeField]
	private RectTransform target;

	[SerializeField]
	private List<Button> tabButtons;

	[SerializeField]
	private List<Canvas> tabContents;

	[SerializeField]
	private Slider soundSlider;

	[SerializeField]
	private Slider musicSlider;

	[SerializeField]
	private Slider dialogueSlider;

	[SerializeField]
	private Slider sfxSlider;

	[SerializeField]
	private Slider ambientSlider;

	[SerializeField]
	private Slider rotationSlider;

	[SerializeField]
	private Slider fieldOfViewSlider;

	[SerializeField]
	private Slider brightnessSlider;

	[SerializeField]
	private Slider productDrawDistanceSlider;

	[SerializeField]
	private TextMeshProUGUI soundValueText;

	[SerializeField]
	private TextMeshProUGUI musicValueText;

	[SerializeField]
	private TextMeshProUGUI dialogueValueText;

	[SerializeField]
	private TextMeshProUGUI sfxValueText;

	[SerializeField]
	private TextMeshProUGUI ambientValueText;

	[SerializeField]
	private TextMeshProUGUI rotationValueText;

	[SerializeField]
	private TextMeshProUGUI fieldOfViewValueText;

	[SerializeField]
	private TextMeshProUGUI brightnessValueText;

	[SerializeField]
	private TextMeshProUGUI productDrawDistanceValueText;

	[SerializeField]
	private TextMeshProUGUI languageText;

	[SerializeField]
	private Button languageNext;

	[SerializeField]
	private Button languagePrevious;

	[SerializeField]
	private TMP_Dropdown resolutionDropdown;

	[SerializeField]
	private TMP_Dropdown qualityDropdown;

	[SerializeField]
	private Toggle fullscreenToggle;

	[SerializeField]
	private Toggle vsyncToggle;

	[SerializeField]
	private Toggle invertYToggle;

	[SerializeField]
	private Toggle headBobToggle;

	[SerializeField]
	private Volume volume;

	[SerializeField]
	private ScrollRect scrollRect;

	private ColorAdjustments colorAdjustments;

	private Vector3 initialPosition;

	private float sliderValue = 1f;

	private int currentLanguageIndex;

	private Resolution[] resolutions;

	private List<Resolution> filteredResolutions = new List<Resolution>();

	private const string IS_FULLSCREEN_KEY = "IS_FULLSCREEN_KEY";

	private const string IS_VSYNC_KEY = "IS_VSYNC_KEY";

	private const string RESOLUTION_INDEX_KEY = "RESOLUTION_INDEX_KEY";

	private const string QUALITY_INDEX_KEY = "QUALITY_INDEX_KEY";

	private const string INVERT_Y_KEY = "INVERT_Y_KEY";

	private const string HEAD_BOB_KEY = "HEAD_BOB_KEY";

	private const string FIELD_OF_VIEW_KEY = "FIELD_OF_VIEW_KEY";

	private const string BRIGHTNESS_KEY = "BRIGHTNESS_KEY";

	private const string PRODUCT_DRAW_DISTANCE_KEY = "PRODUCT_DRAW_DISTANCE_KEY";

	private int selectedTabIndex = -1;

	private Color selectedTabColor = new Color(12f / 85f, 12f / 85f, 12f / 85f, 200f);

	private Color deselectedTabColor = new Color(0.29411766f, 0.4117647f, 0.5137255f);

	private const float tresholdAspectRatio = 1.78f;

	private const float maxAspectRatio = 3.2f;

	private float fieldOfView = 90f;

	private float productDrawDistance = 50f;

	private bool useFahrenheit;

	private Dictionary<CustomSystemLanguage, string> languageDisplayNames = new Dictionary<CustomSystemLanguage, string>
	{
		{
			CustomSystemLanguage.Arabic,
			"العربية"
		},
		{
			CustomSystemLanguage.ChineseSimplified,
			"简体中文"
		},
		{
			CustomSystemLanguage.ChineseTraditional,
			"繁體中文"
		},
		{
			CustomSystemLanguage.Czech,
			"Čeština"
		},
		{
			CustomSystemLanguage.Danish,
			"Dansk"
		},
		{
			CustomSystemLanguage.Dutch,
			"Nederlands"
		},
		{
			CustomSystemLanguage.English,
			"English"
		},
		{
			CustomSystemLanguage.Finnish,
			"Suomi"
		},
		{
			CustomSystemLanguage.French,
			"Français"
		},
		{
			CustomSystemLanguage.German,
			"Deutsch"
		},
		{
			CustomSystemLanguage.Hungarian,
			"Magyar"
		},
		{
			CustomSystemLanguage.Indonesian,
			"Bahasa Indonesia"
		},
		{
			CustomSystemLanguage.Italian,
			"Italiano"
		},
		{
			CustomSystemLanguage.Japanese,
			"日本語"
		},
		{
			CustomSystemLanguage.Korean,
			"한국어"
		},
		{
			CustomSystemLanguage.Malay,
			"Bahasa Melayu"
		},
		{
			CustomSystemLanguage.Norwegian,
			"Norsk (Bokmål)"
		},
		{
			CustomSystemLanguage.Polish,
			"Polski"
		},
		{
			CustomSystemLanguage.Portuguese,
			"Português"
		},
		{
			CustomSystemLanguage.Romanian,
			"Română"
		},
		{
			CustomSystemLanguage.Russian,
			"Русский"
		},
		{
			CustomSystemLanguage.Spanish,
			"Español"
		},
		{
			CustomSystemLanguage.Swedish,
			"Svenska"
		},
		{
			CustomSystemLanguage.Thai,
			"ไทย"
		},
		{
			CustomSystemLanguage.Turkish,
			"Türkçe"
		}
	};

	public bool UseFahrenheit => useFahrenheit;

	public float RotationMultiplier => rotationSlider.value;

	public float ProductDrawDistance => productDrawDistance;

	public float FieldOfView => fieldOfView;

	public bool InvertY => invertYToggle.isOn;

	public bool HeadBob => headBobToggle.isOn;

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
		initialPosition = window.anchoredPosition;
		languageNext.onClick.AddListener(OnNextLanguage);
		languagePrevious.onClick.AddListener(OnPreviousLanguage);
		float masterAudioMultiplier = SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier;
		soundSlider.value = masterAudioMultiplier;
		soundValueText.text = (masterAudioMultiplier * 100f).ToString("F0");
		soundSlider.onValueChanged.AddListener(delegate(float num5)
		{
			SingletonBehaviour<AudioManager>.Instance.SetMasterAudioMultiplier(num5);
			soundValueText.text = (num5 * 100f).ToString("F0");
		});
		float musicAudioMultiplier = SingletonBehaviour<AudioManager>.Instance.MusicAudioMultiplier;
		musicSlider.value = musicAudioMultiplier;
		musicValueText.text = (musicAudioMultiplier * 100f).ToString("F0");
		musicSlider.onValueChanged.AddListener(delegate(float num5)
		{
			SingletonBehaviour<AudioManager>.Instance.SetMusicMultiplier(num5);
			musicValueText.text = (num5 * 100f).ToString("F0");
		});
		float speechAudioMultiplier = SingletonBehaviour<AudioManager>.Instance.SpeechAudioMultiplier;
		dialogueSlider.value = speechAudioMultiplier;
		dialogueValueText.text = (speechAudioMultiplier * 100f).ToString("F0");
		dialogueSlider.onValueChanged.AddListener(delegate(float num5)
		{
			SingletonBehaviour<AudioManager>.Instance.SetDialogueMultiplier(num5);
			dialogueValueText.text = (num5 * 100f).ToString("F0");
		});
		float sFXAudioMultiplier = SingletonBehaviour<AudioManager>.Instance.SFXAudioMultiplier;
		sfxSlider.value = sFXAudioMultiplier;
		sfxValueText.text = (sFXAudioMultiplier * 100f).ToString("F0");
		sfxSlider.onValueChanged.AddListener(delegate(float num5)
		{
			SingletonBehaviour<AudioManager>.Instance.SetSFXMultiplier(num5);
			sfxValueText.text = (num5 * 100f).ToString("F0");
		});
		float ambienceAudioMultiplier = SingletonBehaviour<AudioManager>.Instance.AmbienceAudioMultiplier;
		ambientSlider.value = ambienceAudioMultiplier;
		ambientValueText.text = (ambienceAudioMultiplier * 100f).ToString("F0");
		ambientSlider.onValueChanged.AddListener(delegate(float num5)
		{
			SingletonBehaviour<AudioManager>.Instance.SetAmbienceMultiplier(num5);
			ambientValueText.text = (num5 * 100f).ToString("F0");
		});
		float num = PlayerPrefs.GetFloat(PlayerLook.ROTATION_MULTIPLIER_KEY, 1f);
		rotationSlider.value = num;
		rotationValueText.text = Mathf.RoundToInt(num * 100f).ToString();
		rotationSlider.onValueChanged.AddListener(delegate(float num5)
		{
			rotationValueText.text = Mathf.RoundToInt(num5 * 100f).ToString();
		});
		fieldOfView = PlayerPrefs.GetFloat("FIELD_OF_VIEW_KEY", DefaultFOV());
		fieldOfViewSlider.value = fieldOfView;
		fieldOfViewValueText.text = fieldOfView.ToString();
		fieldOfViewSlider.onValueChanged.AddListener(delegate(float num5)
		{
			fieldOfView = num5;
			fieldOfViewValueText.text = fieldOfView.ToString();
			EventManager.NotifyEvent(UIEvents.FIELD_OF_VIEW_CHANGED, fieldOfView);
		});
		float num2 = PlayerPrefs.GetFloat("BRIGHTNESS_KEY", 1f);
		brightnessSlider.value = num2;
		brightnessValueText.text = Mathf.RoundToInt(num2 * 100f) + "%";
		float value = Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(brightnessSlider.minValue, brightnessSlider.maxValue, num2));
		colorAdjustments.postExposure.value = value;
		brightnessSlider.onValueChanged.AddListener(delegate(float num5)
		{
			brightnessValueText.text = Mathf.RoundToInt(num5 * 100f) + "%";
			float value2 = Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(brightnessSlider.minValue, brightnessSlider.maxValue, num5));
			colorAdjustments.postExposure.value = value2;
		});
		productDrawDistance = PlayerPrefs.GetFloat("PRODUCT_DRAW_DISTANCE_KEY", productDrawDistance);
		productDrawDistanceSlider.value = productDrawDistance;
		productDrawDistanceValueText.text = productDrawDistance.ToString();
		productDrawDistanceSlider.onValueChanged.AddListener(delegate(float num5)
		{
			productDrawDistanceValueText.text = num5.ToString();
		});
		bool isOn = PlayerPrefs.GetInt("INVERT_Y_KEY", 0) == 1;
		invertYToggle.isOn = isOn;
		bool isOn2 = PlayerPrefs.GetInt("HEAD_BOB_KEY", 1) == 1;
		headBobToggle.isOn = isOn2;
		for (int num3 = 0; num3 < Locale.AvailableLanguages().Count; num3++)
		{
			if (Locale.AvailableLanguages()[num3] == Locale.PlayerLanguage)
			{
				currentLanguageIndex = num3;
			}
		}
		languageText.text = languageDisplayNames[Locale.PlayerLanguage];
		InitializeResolutionDropdown();
		QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QUALITY_INDEX_KEY", GetDefaultQualityIndex()));
		InitializeQualityDropdown();
		bool flag = PlayerPrefs.GetInt("IS_FULLSCREEN_KEY", 1) == 1;
		fullscreenToggle.isOn = flag;
		Screen.fullScreenMode = (flag ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
		fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
		bool flag2 = PlayerPrefs.GetInt("IS_VSYNC_KEY", 1) == 1;
		vsyncToggle.isOn = flag2;
		QualitySettings.vSyncCount = (flag2 ? 1 : 0);
		vsyncToggle.onValueChanged.AddListener(ToggleVsync);
		for (int num4 = 0; num4 < tabButtons.Count; num4++)
		{
			int index = num4;
			tabButtons[index].onClick.AddListener(delegate
			{
				SelectTab(index);
			});
		}
		useFahrenheit = IsUsRegion();
	}

	private int DefaultFOV()
	{
		float num = (float)Screen.width / (float)Screen.height;
		if (num > 1.78f)
		{
			return Mathf.RoundToInt(Mathf.Lerp(90f, 125f, (num - 1.78f) / 1.4200001f));
		}
		return 90;
	}

	private void SelectTab(int index)
	{
		if (selectedTabIndex != -1)
		{
			DeselectTab(selectedTabIndex);
		}
		for (int i = 0; i < tabContents.Count; i++)
		{
			tabContents[i].enabled = i == index;
		}
		tabButtons[index].image.color = selectedTabColor;
		selectedTabIndex = index;
	}

	private void DeselectTab(int index)
	{
		tabButtons[index].image.color = deselectedTabColor;
	}

	private int GetDefaultResolutionIndex()
	{
		return 0;
	}

	private int GetDefaultQualityIndex()
	{
		int graphicsMemorySize = SystemInfo.graphicsMemorySize;
		int systemMemorySize = SystemInfo.systemMemorySize;
		if (graphicsMemorySize < 2000 || systemMemorySize < 4000)
		{
			return 1;
		}
		if (graphicsMemorySize < 4000 || systemMemorySize < 8000)
		{
			return 2;
		}
		if (graphicsMemorySize >= 7000)
		{
			_ = 12000;
		}
		return 2;
	}

	private void InitializeResolutionDropdown()
	{
		resolutions = Screen.resolutions;
		resolutionDropdown.ClearOptions();
		int value = 0;
		List<string> list = new List<string>();
		HashSet<string> hashSet = new HashSet<string>();
		for (int num = resolutions.Length - 1; num > 0; num--)
		{
			string item = resolutions[num].width + "x" + resolutions[num].height;
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item);
				filteredResolutions.Add(resolutions[num]);
				list.Add(item);
				if (resolutions[num].width == Screen.currentResolution.width && resolutions[num].height == Screen.currentResolution.height)
				{
					value = filteredResolutions.Count - 1;
				}
			}
		}
		resolutionDropdown.AddOptions(list);
		resolutionDropdown.value = value;
		resolutionDropdown.RefreshShownValue();
		resolutionDropdown.onValueChanged.AddListener(SetResolution);
	}

	private void InitializeQualityDropdown()
	{
		qualityDropdown.ClearOptions();
		string[] names = QualitySettings.names;
		qualityDropdown.AddOptions(new List<string>(names));
		qualityDropdown.value = QualitySettings.GetQualityLevel();
		qualityDropdown.RefreshShownValue();
		qualityDropdown.onValueChanged.AddListener(SetQuality);
	}

	public void SetResolution(int resolutionIndex)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.CLICK_GENERIC);
		Resolution resolution = filteredResolutions[resolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
	}

	public void SetQuality(int qualityIndex)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.CLICK_GENERIC);
		PlayerPrefs.SetInt("QUALITY_INDEX_KEY", qualityIndex);
		QualitySettings.SetQualityLevel(qualityIndex);
	}

	public void ToggleFullscreen(bool isFullscreen)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.CLICK_GENERIC);
		PlayerPrefs.SetInt("IS_FULLSCREEN_KEY", fullscreenToggle.isOn ? 1 : 0);
		Screen.fullScreenMode = (isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
	}

	public void ToggleVsync(bool isVsync)
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.CLICK_GENERIC);
		PlayerPrefs.SetInt("IS_VSYNC_KEY", vsyncToggle.isOn ? 1 : 0);
		QualitySettings.vSyncCount = (vsyncToggle.isOn ? 1 : 0);
	}

	public void OnNextLanguage()
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.CLICK_GENERIC);
		currentLanguageIndex++;
		currentLanguageIndex %= Locale.AvailableLanguages().Count;
		LocalizeBase.SetCurrentLanguage(Locale.AvailableLanguages()[currentLanguageIndex]);
		languageText.text = languageDisplayNames[Locale.AvailableLanguages()[currentLanguageIndex]];
	}

	public void OnPreviousLanguage()
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.CLICK_GENERIC);
		currentLanguageIndex--;
		if (currentLanguageIndex < 0)
		{
			currentLanguageIndex += Locale.AvailableLanguages().Count;
		}
		currentLanguageIndex %= Locale.AvailableLanguages().Count;
		LocalizeBase.SetCurrentLanguage(Locale.AvailableLanguages()[currentLanguageIndex]);
		languageText.text = languageDisplayNames[Locale.AvailableLanguages()[currentLanguageIndex]];
	}

	public override void Open()
	{
		SelectTab(0);
		StartCoroutine(ScrollToBottom());
		window.DOKill();
		window.DOAnchorPos(target.anchoredPosition, 0.2f).SetUpdate(isIndependentUpdate: true);
		base.Open();
		sliderValue = rotationSlider.value;
	}

	private IEnumerator ScrollToBottom()
	{
		yield return new WaitForEndOfFrame();
		scrollRect.verticalNormalizedPosition = 1f;
	}

	public override void Close()
	{
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		if (!Mathf.Approximately(sliderValue, rotationSlider.value))
		{
			PlayerPrefs.SetFloat(PlayerLook.ROTATION_MULTIPLIER_KEY, rotationSlider.value);
			EventManager.NotifyEvent(UIEvents.ROTATION_MULTIPLIER_CHANGED, rotationSlider.value);
		}
		PlayerPrefs.SetFloat("FIELD_OF_VIEW_KEY", fieldOfViewSlider.value);
		PlayerPrefs.SetInt("INVERT_Y_KEY", invertYToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("HEAD_BOB_KEY", headBobToggle.isOn ? 1 : 0);
		if (!Mathf.Approximately(productDrawDistanceSlider.value, productDrawDistance))
		{
			EventManager.NotifyEvent(UIEvents.PRODUCT_DRAW_DISTANCE_CHANGED, productDrawDistanceSlider.value);
			productDrawDistance = productDrawDistanceSlider.value;
			PlayerPrefs.SetFloat("PRODUCT_DRAW_DISTANCE_KEY", productDrawDistanceSlider.value);
		}
		PlayerPrefs.SetFloat("BRIGHTNESS_KEY", brightnessSlider.value);
		PlayerPrefs.Save();
		window.DOKill();
		window.DOAnchorPos(initialPosition, 0.2f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			base.Close();
			EventManager.NotifyEvent(UIEvents.SETTING_WINDOW_CLOSED);
		});
	}

	public static bool IsUsRegion()
	{
		try
		{
			return RegionInfo.CurrentRegion.TwoLetterISORegionName == "US";
		}
		catch
		{
			return false;
		}
	}
}
