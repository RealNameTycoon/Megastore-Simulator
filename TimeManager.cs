using System;
using System.Collections;
using System.Collections.Generic;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TimeManager : SingletonBehaviour<TimeManager>
{
	[SerializeField]
	private Light directionalLight;

	[SerializeField]
	private Button endDayButton;

	[SerializeField]
	private TextMeshProUGUI endDayTooltipText;

	[SerializeField]
	private TextMeshProUGUI endDayTooltipTextGamepad;

	[SerializeField]
	private Image gamepadSprite;

	[SerializeField]
	private TextMeshProUGUI dayText;

	[SerializeField]
	private TextMeshProUGUI hourText;

	[SerializeField]
	private TextMeshProUGUI hourTextComputer;

	[SerializeField]
	private CanvasGroup blinkMask;

	[SerializeField]
	private List<GameObject> streetLights;

	[SerializeField]
	private Material[] lightEmmisiveMaterialsOutdoor;

	[SerializeField]
	private Material[] lightEmmisiveMaterialsIndoor;

	[SerializeField]
	private List<ReflectionProbe> reflectionProbes;

	[SerializeField]
	private LightSwitch lightSwitch;

	private Color lightColor = new Color(0.75f, 0.75f, 0.75f);

	private string activeLightingScenario = "";

	private const float ON_REFLECTION_INTENTITY = 1f;

	private const float OFF_REFLECTION_INTENSITY = 0.3f;

	private List<float> initialReflectionProbeIntensities;

	private const string onName = "ON";

	private const string offName = "OFF";

	private const string dayPostfix = "_DAY";

	private const string nightPostfix = "_NIGHT";

	private const string currentDayKey = "CurrentDay";

	private const string currentMinsKey = "CurrentHour";

	private WaitForSeconds point1MinWaiter = new WaitForSeconds(0.066f);

	private int dayCount;

	private float mins;

	private int quarters;

	private const int TOTAL_DAY_MINS_VENDING = 900;

	private const int TWENTY_FOUR_HOURS_MINS = 1440;

	private const int TOTAL_DAY_MINS = 960;

	public const int NIGHT_MINS = 480;

	private Color ambientColor = new Color(0.88235295f, 0.88235295f, 0.88235295f);

	private Color morningColorTillSundown = new Color(0.8156863f, 77f / 85f, 1f);

	private Color sundownColor = new Color(0.77254903f, 48f / 85f, 0.7058824f);

	private Color nightColor = new Color(0.8f, 0.8f, 0.8f);

	private float MORNING_SKYBOX_ROTATION = 92f;

	private float ENDDAY_SKYBOX_ROTATION = -90f;

	private float MIDDAY_LIGHT_INTENSITY = 4f;

	private float MORNING_LIGHT_INTENSITY = 2.5f;

	private float ENDDDAY_LIGHT_INTENSITY = 1.2f;

	private float MAX_SKYBOX_EXPOSURE = 0.7f;

	private float MIN_SKYBOX_EXPOSURE = 0.2f;

	private float Y_ANGLE_MORNING = 272f;

	private float Y_ANGLE_ENDDAY = 63.6f;

	private float X_ANGLE_MORNING = 128f;

	private float X_ANGLE_MIDDAY = 98f;

	private float X_ANGLE_ENDDAY = 158f;

	private float MORNING_KELVIN = 8000f;

	private float MID_DAY_KELVIN = 5000f;

	private float END_DAY_KELVIN = 12000f;

	private const int SUNDOWN_START_MINS_17 = 660;

	public const int SUNDOWN_END_MINS_1830 = 750;

	public const int OUTDOOR_LIGHT_ON_MIN = 780;

	private const int SUNDOWN_NIGHT_MINS_21 = 900;

	private const int TEMPERATURE_MAX_MINUTES_14 = 480;

	private const int TEMPERATURE_MIN_MINUTES_21 = 900;

	private const int MID_DAY_MINS_12 = 360;

	private const int EARLY_SHIFT_END_MINUTE = 480;

	private const int TIME_TO_STOP_HEATING_1HOUR = 60;

	private const int INCOME_BOOST_OFFER_MINS = 360;

	private const int LAST_WANDERER_SPAWN_TIME = 840;

	public const int FIRE_MINS_START = 360;

	public const int FIRE_MINS_END = 720;

	public static Action OnMinPassed;

	private Coroutine lightScenarioSwitchRoutine;

	private ProbeReferenceVolume probeRefVolume;

	private const float BLENDING_FACTOR = 1f;

	private const int CELLS_BLENDED_PER_FRAME = 1000;

	public int CurrentDay => dayCount;

	public int CurrentMin => quarters / 10;

	public void ChangeTime(int hour)
	{
		mins = hour * 60 - 360;
		quarters = (int)mins * 10;
	}

	public void EndDay()
	{
		mins = 958f;
		quarters = (int)mins * 10;
	}

	public bool DayOver()
	{
		return mins == 960f;
	}

	private new void Awake()
	{
		base.Awake();
		probeRefVolume = ProbeReferenceVolume.instance;
		probeRefVolume.SetNumberOfCellsLoadedPerFrame(5);
		initialReflectionProbeIntensities = new List<float>();
		foreach (ReflectionProbe reflectionProbe in reflectionProbes)
		{
			initialReflectionProbeIntensities.Add(reflectionProbe.intensity);
		}
		RepaintLights(enable: true);
		EventManager.AddListener(GameEvents.ROBERY_FINISHED, OnRoberyFinished);
		dayCount = GenericDataSerializer.LoadInt("CurrentDay", 1);
		mins = GenericDataSerializer.LoadInt("CurrentHour");
		quarters = (int)(mins * 10f);
	}

	private void Start()
	{
		endDayButton.onClick.AddListener(delegate
		{
			OnEndDay();
		});
		RepaintHourText();
		RepaintDay();
		if (mins == 960f)
		{
			ShowNextDayUI();
			EventManager.NotifyEvent(GameEvents.DAY_ENDED_NO_CUSTOMERS_LEFT);
			RepaintSun();
		}
		else if (mins < 360f)
		{
			RepaintSun();
		}
		else
		{
			RepaintSun();
		}
		StartCoroutine(DayRoutine());
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	public bool IsEarlyShiftOver()
	{
		return mins >= 480f;
	}

	public bool IsEndDayButtonActive()
	{
		return endDayButton.gameObject.activeSelf;
	}

	private void OnLanguageChanged()
	{
		endDayTooltipText.text = Locale.GetWord("press_e").Replace("{0}", SingletonBehaviour<InputManager>.Instance.GetBindingDisplayString(SingletonBehaviour<InputManager>.Instance.EndDayActionRef));
		RepaintDay();
	}

	public bool CanSpawnWanderer()
	{
		return mins < 840f;
	}

	public bool CanSpawnVendingCustomer()
	{
		return mins < 900f;
	}

	public bool CanSpawnCustomer()
	{
		return mins < 960f;
	}

	private IEnumerator DayRoutine()
	{
		while (true)
		{
			if (mins == 0f && !SingletonBehaviour<OpenCloseLabel>.Instance.IsOpen)
			{
				yield return point1MinWaiter;
				continue;
			}
			if (mins < 960f)
			{
				mins = (float)quarters / 10f;
				quarters++;
				GenericDataSerializer.SaveInt("CurrentHour", CurrentMin);
				RepaintSun();
				if (quarters % 10 == 0)
				{
					if (mins == 360f)
					{
						EventManager.NotifyEvent(UIEvents.OPEN_INCOME_OFFER);
					}
					if (quarters == 7500)
					{
						SingletonBehaviour<AudioManager>.Instance.OnSunDowned();
					}
					if (quarters == 8400)
					{
						EventManager.NotifyEvent(GameEvents.SPAWN_TIME_OVER);
					}
					if (quarters == 4800)
					{
						EventManager.NotifyEvent(GameEvents.EARLY_SHIFT_OVER);
						EventManager.NotifyEvent(GameEvents.LATE_SHIFT_STARTED);
					}
					RepaintHourText();
					if (quarters == 9600)
					{
						SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("end_day_tooltip", base.transform);
						if (SingletonBehaviour<CustomerManager>.Instance.ShoppingCustomerCount() == 0 && SingletonBehaviour<CustomerManager>.Instance.ShoppingVendingCustomerCount() == 0)
						{
							ShowNextDayUI();
						}
					}
					OnMinPassed?.Invoke();
				}
			}
			yield return point1MinWaiter;
		}
	}

	private void RepaintSun()
	{
		directionalLight.colorTemperature = GetTemperature();
		directionalLight.intensity = GetIntensity();
		directionalLight.transform.eulerAngles = GetLightRotation();
		RenderSettings.skybox.SetFloat("_Exposure", GetExposure());
		RenderSettings.skybox.SetFloat("_Rotation", GetRotation());
		RenderSettings.skybox.SetColor("_Tint", GetColor());
		RenderSettings.ambientLight = GetAmbientIntensity() * ambientColor;
		if (CurrentMin >= 780 && activeLightingScenario != GetNeededLightningScenario())
		{
			RepaintLights(lightSwitch.IsOn);
		}
		else if (CurrentMin < 780 && activeLightingScenario != GetNeededLightningScenario())
		{
			RepaintLights(lightSwitch.IsOn);
		}
	}

	private void EnableStreetLights(bool enable)
	{
		for (int i = 0; i < streetLights.Count; i++)
		{
			streetLights[i].SetActive(enable);
		}
	}

	private float GetTemperature()
	{
		if (mins < 480f)
		{
			return Mathf.Lerp(MORNING_KELVIN, MID_DAY_KELVIN, mins / 480f);
		}
		return Mathf.Lerp(MID_DAY_KELVIN, END_DAY_KELVIN, (mins - 480f) / 420f);
	}

	private float GetIntensity()
	{
		if (mins < 480f)
		{
			return Mathf.Lerp(MORNING_LIGHT_INTENSITY, MIDDAY_LIGHT_INTENSITY, mins / 360f);
		}
		return Mathf.Lerp(MIDDAY_LIGHT_INTENSITY, ENDDDAY_LIGHT_INTENSITY, (mins - 480f) / 420f);
	}

	private float GetExposure()
	{
		if (mins >= 480f)
		{
			return Mathf.Lerp(MAX_SKYBOX_EXPOSURE, MIN_SKYBOX_EXPOSURE, (mins - 480f) / 420f);
		}
		return MAX_SKYBOX_EXPOSURE;
	}

	private float GetRotation()
	{
		return Mathf.Lerp(MORNING_SKYBOX_ROTATION, ENDDAY_SKYBOX_ROTATION, mins / 960f);
	}

	private Color GetColor()
	{
		if (mins < 660f)
		{
			return morningColorTillSundown;
		}
		if (mins >= 660f && mins <= 750f)
		{
			return Color.Lerp(morningColorTillSundown, sundownColor, (mins - 660f) / 90f);
		}
		return Color.Lerp(sundownColor, nightColor, (mins - 750f) / 150f);
	}

	private float GetAmbientIntensity()
	{
		if (mins < 660f)
		{
			return 1f;
		}
		return Mathf.Lerp(1f, 0.5f, (mins - 660f) / 240f);
	}

	private Vector3 GetLightRotation()
	{
		float x = directionalLight.transform.eulerAngles.x;
		float y = Mathf.Lerp(Y_ANGLE_MORNING, Y_ANGLE_ENDDAY, mins / 960f);
		float num = 420f;
		if (mins < num)
		{
			x = Mathf.Lerp(X_ANGLE_MORNING, X_ANGLE_MIDDAY, mins / num);
		}
		else if (mins >= num)
		{
			x = Mathf.Lerp(X_ANGLE_MIDDAY, X_ANGLE_ENDDAY, (mins - num) / (960f - num));
		}
		return new Vector3(x, y, directionalLight.transform.eulerAngles.z);
	}

	public void ShowNextDayUI()
	{
		SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("end_day_tooltip", base.transform);
		endDayButton.gameObject.SetActive(value: true);
		if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
		{
			endDayTooltipText.enabled = false;
			endDayTooltipTextGamepad.enabled = true;
			gamepadSprite.enabled = true;
			gamepadSprite.sprite = SingletonBehaviour<InputManager>.Instance.GetGamepadGlyphSprite(SingletonBehaviour<InputManager>.Instance.EndDayActionRef);
		}
		else
		{
			endDayTooltipText.enabled = true;
			endDayTooltipTextGamepad.enabled = false;
			gamepadSprite.enabled = false;
			endDayTooltipText.text = Locale.GetWord("press_e").Replace("{0}", SingletonBehaviour<InputManager>.Instance.GetBindingDisplayString(SingletonBehaviour<InputManager>.Instance.EndDayActionRef, SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad));
		}
	}

	public void OnEndDay()
	{
		if (SingletonBehaviour<OpenCloseLabel>.Instance.IsOpen)
		{
			SingletonBehaviour<OpenCloseLabel>.Instance.OnLabelClicked();
		}
		endDayButton.gameObject.SetActive(value: false);
		EventManager.NotifyEvent(GameEvents.DAY_ENDED);
		DataSerializer.SaveFile();
	}

	public void StartTheNewDay()
	{
		SingletonBehaviour<CustomerManager>.Instance.DeactivateAllCustomers();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		if (IsThiefDay())
		{
			SingletonBehaviour<UIManager>.Instance.SwitchToThiefCamera(state: true);
			RenderSettings.skybox.SetFloat("_Exposure", MAX_SKYBOX_EXPOSURE);
			directionalLight.intensity = MIDDAY_LIGHT_INTENSITY;
			EventManager.NotifyEvent(GameEvents.ROBERY_STARTED);
			SingletonBehaviour<ThiefManager>.Instance.StartRobery();
			return;
		}
		RenderSettings.skybox.SetFloat("_Exposure", MAX_SKYBOX_EXPOSURE);
		directionalLight.intensity = MIDDAY_LIGHT_INTENSITY;
		dayCount++;
		mins = 0f;
		quarters = 0;
		RepaintSun();
		RepaintHourText();
		GenericDataSerializer.SaveInt("CurrentHour", CurrentMin);
		GenericDataSerializer.SaveInt("CurrentDay", dayCount);
		DataSerializer.SaveStoreDay(dayCount);
		EventManager.NotifyEvent(GameEvents.NEW_DAY_STARTED);
		SingletonBehaviour<AudioManager>.Instance.OnNewDayStarted();
		RepaintDay();
	}

	public void RepaintLights(bool enable)
	{
		if (lightScenarioSwitchRoutine != null)
		{
			StopCoroutine(lightScenarioSwitchRoutine);
		}
		lightScenarioSwitchRoutine = StartCoroutine(RepaintLightsRoutine(enable));
	}

	private IEnumerator RepaintLightsRoutine(bool enable)
	{
		bool outdoorMaterialEmission = mins >= 750f;
		activeLightingScenario = GetNeededLightningScenario();
		if (enable)
		{
			SetIndoorMaterialEmission(shiny: true);
			probeRefVolume.lightingScenario = activeLightingScenario;
			for (int i = 0; i < initialReflectionProbeIntensities.Count; i++)
			{
				if (initialReflectionProbeIntensities.Count > i)
				{
					reflectionProbes[i].intensity = initialReflectionProbeIntensities[i];
				}
				else
				{
					reflectionProbes[i].intensity = 1f;
				}
				reflectionProbes[i].timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
				reflectionProbes[i].RenderProbe();
			}
		}
		else
		{
			SetIndoorMaterialEmission(shiny: false);
			probeRefVolume.lightingScenario = activeLightingScenario;
			foreach (ReflectionProbe reflectionProbe in reflectionProbes)
			{
				reflectionProbe.intensity = 0.3f;
				reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
				reflectionProbe.RenderProbe();
			}
		}
		SetOutdoorMaterialEmission(outdoorMaterialEmission);
		lightScenarioSwitchRoutine = null;
		yield return null;
	}

	public void SetIndoorMaterialEmission(bool shiny)
	{
		for (int i = 0; i < lightEmmisiveMaterialsIndoor.Length; i++)
		{
			lightEmmisiveMaterialsIndoor[i].SetColor("_EmissionColor", shiny ? (lightColor * 7.5f) : (lightColor * 1.5f));
		}
	}

	public void SetOutdoorMaterialEmission(bool shiny)
	{
		for (int i = 0; i < lightEmmisiveMaterialsOutdoor.Length; i++)
		{
			lightEmmisiveMaterialsOutdoor[i].SetColor("_EmissionColor", shiny ? (lightColor * 32f) : (lightColor * 1.5f));
		}
	}

	private string GetNeededLightningScenario()
	{
		string text = ((mins >= 750f) ? "_NIGHT" : "_DAY");
		if (lightSwitch.IsOn)
		{
			return "ON" + text;
		}
		return "OFF" + text;
	}

	private void OnRoberyFinished()
	{
		blinkMask.DOFade(1f, 0.33f).OnComplete(delegate
		{
			dayCount++;
			mins = 0f;
			quarters = 0;
			GenericDataSerializer.SaveInt("CurrentHour", CurrentMin);
			GenericDataSerializer.SaveInt("CurrentDay", dayCount);
			DataSerializer.SaveStoreDay(dayCount);
			EventManager.NotifyEvent(GameEvents.NEW_DAY_STARTED);
			SingletonBehaviour<UIManager>.Instance.SwitchToThiefCamera(state: false);
			RepaintDay();
			blinkMask.DOFade(0f, 0.33f).OnComplete(delegate
			{
				blinkMask.gameObject.SetActive(value: false);
			});
		});
	}

	private bool IsThiefDay()
	{
		return false;
	}

	private void RepaintHourText()
	{
		string text = GetHourText(CurrentMin);
		hourText.text = text;
		hourTextComputer.text = text;
	}

	public string GetHourText(int mins)
	{
		int num = 360 + mins;
		int num2 = num / 60;
		int num3 = num % 60;
		if (num2 == 0)
		{
			num2 = 12;
		}
		return $"{num2:D2}:{num3:D2}";
	}

	private void RepaintDay()
	{
		dayText.text = Locale.GetWord("day_n").Replace("{0}", dayCount.ToString());
	}
}
