using System;
using BugsnagUnity;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseWindow : UIWindow
{
	[SerializeField]
	private Button saveButton;

	[SerializeField]
	private TextMeshProUGUI saveText;

	private bool isCursorLocked;

	private string lastSaveKey = "save";

	public override void Open()
	{
		if (!IsOpen())
		{
			saveText.text = Locale.CurrentLanguageStrings["save"];
			lastSaveKey = "save";
			saveButton.interactable = true;
			EventManager.NotifyEvent(GameEvents.GAME_PAUSED);
			SingletonBehaviour<AudioManager>.Instance.OnGamePaused();
			Time.timeScale = 0f;
			base.Open();
			if (SingletonBehaviour<PlayerLook>.Instance != null)
			{
				isCursorLocked = !SingletonBehaviour<PlayerLook>.Instance.RotationLocked;
				SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
			}
			AudioListener.pause = true;
		}
	}

	public override void Close()
	{
		base.Close();
		Time.timeScale = 1f;
		SingletonBehaviour<AudioManager>.Instance.OnGameResumed();
		if (SingletonBehaviour<PlayerLook>.Instance != null)
		{
			SingletonBehaviour<PlayerLook>.Instance.LockCursor(isCursorLocked);
		}
		AudioListener.pause = false;
		EventManager.NotifyEvent(GameEvents.GAME_RESUMED);
	}

	public void Hide()
	{
		canvas.enabled = false;
	}

	public void UnHide()
	{
		canvas.enabled = true;
	}

	private void Start()
	{
		EventManager.AddListener(UIEvents.SETTING_WINDOW_CLOSED, UnHide);
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		saveText.text = Locale.CurrentLanguageStrings[lastSaveKey];
	}

	public void OnSave()
	{
		DataSerializer.SaveFile(UpdateSaveText);
	}

	private void UpdateSaveText()
	{
		saveText.text = Locale.CurrentLanguageStrings["saved"];
		lastSaveKey = "saved";
		saveButton.interactable = false;
	}

	public void OnSettings()
	{
		SingletonWindow<SettingsWindow>.Instance.Open();
		Hide();
	}

	public void OnExitToMenu()
	{
		Time.timeScale = 1f;
		SingletonBehaviour<AudioManager>.Instance.OnBackToMenu();
		try
		{
			SingletonBehaviour<EmployeeManager>.Instance.DeactivateAllEmployeesInstant();
		}
		catch (Exception exception)
		{
			Bugsnag.Notify(exception);
		}
		if (saveButton.interactable)
		{
			DataSerializer.SaveFile(delegate
			{
				DOTween.KillAll();
				EventManager.RemoveAllListeners();
				SingletonBehaviour<RayShooter>.Instance.RemoveAllListeners();
				SceneManager.LoadScene("LoadingSceneCrazy");
			});
		}
		else
		{
			DOTween.KillAll();
			EventManager.RemoveAllListeners();
			SingletonBehaviour<RayShooter>.Instance.RemoveAllListeners();
			SceneManager.LoadScene("LoadingSceneCrazy");
		}
	}

	public void OnExitToDesktop()
	{
		Time.timeScale = 1f;
		try
		{
			SingletonBehaviour<EmployeeManager>.Instance.DeactivateAllEmployeesInstant();
		}
		catch (Exception exception)
		{
			Bugsnag.Notify(exception);
		}
		if (saveButton.interactable)
		{
			DataSerializer.SaveFile(delegate
			{
				DOTween.KillAll();
				Application.Quit();
			});
		}
		else
		{
			DOTween.KillAll();
			Application.Quit();
		}
	}
}
