using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameWindow : UIWindow
{
	[SerializeField]
	private List<SaveSlotUI> saveSlots;

	[SerializeField]
	private Button newGameButton;

	[SerializeField]
	private Button deleteButton;

	[SerializeField]
	private Button loadButton;

	[SerializeField]
	private ConfirmationWindow confirmationWindow;

	[SerializeField]
	private LoadScreen loadScreen;

	[SerializeField]
	private Button continueMenuButton;

	[SerializeField]
	private Button loadGameMenuButton;

	[SerializeField]
	private Button newGameMenuButton;

	[SerializeField]
	private TextMeshProUGUI continueGameDescriptionText;

	[SerializeField]
	private UIWindow prologueCarryOverWindow;

	[SerializeField]
	private Button prologueCarryOverButton;

	[SerializeField]
	private Button cancelPrologueCarryOverButton;

	private int currentProfileIndex;

	private string prologueCarryOverKey = "PROLOGUE_CARRY_OVER_KEY";

	public static int CURRENT_SAVE_VERSION = 3;

	private SaveSlotUI selectedSaveSlot;

	public void InitializeWindow()
	{
		DataSerializer.LoadAllSlots();
		if (!PlayerPrefs.HasKey(prologueCarryOverKey) && DataSerializer.HasPrologueSave() && !HasAnySave())
		{
			prologueCarryOverWindow.Open();
			prologueCarryOverButton.onClick.AddListener(OnPrologueCarryOverButtonClicked);
			cancelPrologueCarryOverButton.onClick.AddListener(OnCancelPrologueCarryOverButtonClicked);
		}
		else
		{
			Initialize();
		}
	}

	private void SetUpNavigation()
	{
		Navigation navigation = deleteButton.navigation;
		deleteButton.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = navigation.selectOnUp,
			selectOnDown = navigation.selectOnDown,
			selectOnLeft = (newGameButton.gameObject.activeSelf ? newGameButton : loadButton),
			selectOnRight = (newGameButton.gameObject.activeSelf ? newGameButton : loadButton)
		};
	}

	private void Initialize()
	{
		for (int i = 0; i < saveSlots.Count; i++)
		{
			saveSlots[i].Initialize(i);
		}
		currentProfileIndex = GetApplicableContinueIndex();
		continueMenuButton.gameObject.SetActive(currentProfileIndex != -1);
		loadGameMenuButton.gameObject.SetActive(HasAnySave());
		Debug.Log("Current profile index: " + currentProfileIndex);
		if (currentProfileIndex != -1)
		{
			RepaintContinueGameDescription(currentProfileIndex);
		}
		DataSerializer.ChangeProfile((currentProfileIndex != -1) ? currentProfileIndex : 0);
		RepaintSaveSlots();
		OnSaveSlotClicked(saveSlots[(currentProfileIndex != -1) ? currentProfileIndex : 0]);
		newGameButton.onClick.AddListener(OnNewGameButtonClicked);
		deleteButton.onClick.AddListener(OnDeleteButtonClicked);
		loadButton.onClick.AddListener(OnLoadButtonClicked);
		continueMenuButton.onClick.AddListener(OnContinueMenuButtonClicked);
		loadGameMenuButton.onClick.AddListener(OnLoadGameMenuButtonClicked);
		newGameMenuButton.onClick.AddListener(OnNewGameMenuButtonClicked);
	}

	private void OnCancelPrologueCarryOverButtonClicked()
	{
		PlayerPrefs.SetString(prologueCarryOverKey, "false");
		PlayerPrefs.Save();
		prologueCarryOverWindow.Close();
		Initialize();
	}

	private void OnPrologueCarryOverButtonClicked()
	{
		PlayerPrefs.SetString(prologueCarryOverKey, "true");
		PlayerPrefs.Save();
		DataSerializer.CarryPrologueSave();
		Initialize();
		prologueCarryOverWindow.Close();
	}

	private int GetApplicableContinueIndex()
	{
		int num = DataSerializer.LoadLastProfileIndex();
		int result = -1;
		long num2 = -1L;
		if (num != -1)
		{
			return num;
		}
		for (int i = 0; i < saveSlots.Count; i++)
		{
			if (DataSerializer.HasSave(i))
			{
				long result2;
				long num3 = (long.TryParse(DataSerializer.LoadLastPlayedTime(i), out result2) ? result2 : 0);
				Debug.Log("Save time: " + result2 + " for profile index: " + i);
				if (result2 > num2)
				{
					result = i;
					num2 = num3;
				}
			}
		}
		return result;
	}

	private bool HasAnySave()
	{
		for (int i = 0; i < saveSlots.Count; i++)
		{
			if (DataSerializer.HasSave(i))
			{
				return true;
			}
		}
		return false;
	}

	private void OnNewGameButtonClicked()
	{
		OnSecondClick(selectedSaveSlot);
	}

	private void OnDeleteButtonClicked()
	{
		confirmationWindow.Open(Locale.GetWord("delete_game_desc").Replace("{0}", (selectedSaveSlot.ProfileIndex + 1).ToString()), DeleteGameOnTheSelectedSlot);
	}

	private void DeleteGameOnTheSelectedSlot()
	{
		DataSerializer.ChangeProfile(selectedSaveSlot.ProfileIndex);
		DataSerializer.DeleteData(selectedSaveSlot.ProfileIndex);
		DataSerializer.DeleteAll();
		RepaintSaveSlots();
		RepaintButtons();
	}

	private void OnLoadButtonClicked()
	{
		OnSecondClick(selectedSaveSlot);
	}

	public void OnSaveSlotClicked(SaveSlotUI saveSlot)
	{
		if (saveSlot.IsSelected())
		{
			OnSecondClick(saveSlot);
			return;
		}
		selectedSaveSlot?.SetSelected(isSelected: false);
		selectedSaveSlot = saveSlot;
		selectedSaveSlot.SetSelected(isSelected: true);
		RepaintButtons();
	}

	private void RepaintButtons()
	{
		newGameButton.gameObject.SetActive(selectedSaveSlot.IsEmpty);
		deleteButton.interactable = !selectedSaveSlot.IsEmpty;
		loadButton.gameObject.SetActive(!selectedSaveSlot.IsEmpty);
		int applicableContinueIndex = GetApplicableContinueIndex();
		Debug.Log("Applicable continue index: " + applicableContinueIndex);
		continueMenuButton.gameObject.SetActive(applicableContinueIndex != -1);
		if (applicableContinueIndex != -1)
		{
			RepaintContinueGameDescription(applicableContinueIndex);
		}
		loadGameMenuButton.gameObject.SetActive(HasAnySave());
		SetUpNavigation();
	}

	private void RepaintContinueGameDescription(int profileIndex)
	{
		string text = Locale.GetWord("day_n").Replace("{0}", DataSerializer.LoadStoreDay(profileIndex).ToString());
		continueGameDescriptionText.text = DataSerializer.LoadStoreName(profileIndex) + " - " + text;
	}

	private void OnSecondClick(SaveSlotUI saveSlot)
	{
		if (saveSlot.IsEmpty)
		{
			confirmationWindow.Open(Locale.GetWord("new_game_desc").Replace("{0}", (saveSlot.ProfileIndex + 1).ToString()), NewGameOnTheSelectedSlot);
		}
		else
		{
			confirmationWindow.Open(Locale.GetWord("load_game_desc").Replace("{0}", (saveSlot.ProfileIndex + 1).ToString()), LoadGameOnTheSelectedSlot);
		}
	}

	private void NewGameOnTheSelectedSlot()
	{
		DataSerializer.ChangeProfile(selectedSaveSlot.ProfileIndex);
		GenericDataSerializer.SaveInt("SAVE_VERSION_KEY", CURRENT_SAVE_VERSION);
		loadScreen.OnLoadButtonClicked();
	}

	private void LoadGameOnTheSelectedSlot()
	{
		DataSerializer.ChangeProfile(selectedSaveSlot.ProfileIndex);
		loadScreen.OnLoadButtonClicked();
	}

	private void RepaintSaveSlots()
	{
		for (int i = 0; i < saveSlots.Count; i++)
		{
			if (DataSerializer.HasSave(i))
			{
				saveSlots[i].Refresh();
			}
			else
			{
				saveSlots[i].RefreshEmpty();
			}
		}
	}

	private void OnContinueMenuButtonClicked()
	{
		DataSerializer.ChangeProfile(currentProfileIndex);
		loadScreen.OnLoadButtonClicked();
	}

	private void OnLoadGameMenuButtonClicked()
	{
		Open();
	}

	private void OnNewGameMenuButtonClicked()
	{
		if (!HasAnySave())
		{
			loadScreen.OnNewGame();
		}
		else
		{
			Open();
		}
	}
}
