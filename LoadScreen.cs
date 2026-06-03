using System;
using System.Collections;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScreen : UIWindow
{
	[SerializeField]
	private LoadGameWindow loadGameWindow;

	[SerializeField]
	private Slider loadingBar;

	[SerializeField]
	private RectTransform wishlistLabel;

	[SerializeField]
	private GameObject continueButton;

	[SerializeField]
	private Canvas deleteConfirmationWindow;

	[SerializeField]
	private GameObject titleWindowContent;

	[SerializeField]
	private GameObject loadingContent;

	[SerializeField]
	private TextMeshProUGUI versionText;

	[SerializeField]
	private Image logoImage;

	[SerializeField]
	private Image logoDemoImage;

	[SerializeField]
	private GameObject discussionLink;

	private Vector3 wishlistLabelTargetRotation = new Vector3(0f, 0f, -15f);

	private int stepCount;

	private static string oldSaveFixedKey = "OLD_SAVE_FIXED";

	private static string oldSaveDeletedKey = "OLD_SAVE_DELETED";

	public static string returnedMoneyKey = "RETURNED_MONEY";

	private void Start()
	{
		if (GameManager.isDemo)
		{
			wishlistLabel.gameObject.SetActive(value: true);
			discussionLink.SetActive(value: true);
			AnimateWishlistLabel();
		}
		logoImage.enabled = !GameManager.isDemo;
		logoDemoImage.enabled = GameManager.isDemo;
		versionText.text = "v" + Application.version;
		Open();
		loadGameWindow.InitializeWindow();
	}

	private void AnimateWishlistLabel()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(wishlistLabel.DOLocalRotate(wishlistLabelTargetRotation, 1.3f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InBack)
			.OnComplete(delegate
			{
			})).SetLoops(-1).AppendInterval(5f);
		sequence.Play();
	}

	private IEnumerator LoadGameScene()
	{
		DOTween.KillAll();
		titleWindowContent.SetActive(value: false);
		loadingContent.SetActive(value: true);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		AsyncOperation loadGame = SceneManager.LoadSceneAsync("GameScenePC");
		while (!loadGame.isDone)
		{
			loadingBar.value = loadGame.progress / 2f;
			yield return null;
		}
	}

	public void OnNewGame()
	{
		if (GenericDataSerializer.HasKey("FIRST_TIME_KEY"))
		{
			deleteConfirmationWindow.enabled = true;
			return;
		}
		GenericDataSerializer.SaveInt("SAVE_VERSION_KEY", LoadGameWindow.CURRENT_SAVE_VERSION);
		StartCoroutine(LoadGameScene());
	}

	public void OnWishlistButtonClicked()
	{
		Application.OpenURL("steam://store/3819640");
	}

	public void OnDiscordButtonClicked()
	{
		Application.OpenURL("https://discord.gg/ewGHrHnZPn");
	}

	public void OnMailClicked()
	{
		OpenMailApp();
	}

	public void OnFeedbacksButtonClicked()
	{
		Application.OpenURL("https://steamcommunity.com/app/3819640/discussions/0/737036097948905061/");
	}

	private void OpenMailApp()
	{
		string text = "hbekmezci@yologames.co";
		string escapeUri = GetEscapeUri(Locale.CurrentLanguageStrings["support_title"]);
		string escapeUri2 = GetEscapeUri(Locale.CurrentLanguageStrings["support_desc"] + "\n\n\n\n\nApplication Version: " + Application.version + "\n");
		Application.OpenURL("mailto:" + text + "?subject=" + escapeUri + "&body=" + escapeUri2);
	}

	private string GetEscapeUri(string url)
	{
		return Uri.EscapeUriString(url);
	}

	public void OnDeleteProgressAndLoad()
	{
		GenericDataSerializer.DeleteData();
		GenericDataSerializer.SaveInt("SAVE_VERSION_KEY", LoadGameWindow.CURRENT_SAVE_VERSION);
		StartCoroutine(LoadGameScene());
	}

	public void OnContinueButtonClicked()
	{
		StartCoroutine(LoadGameScene());
	}

	public void OnLoadButtonClicked()
	{
		StartCoroutine(LoadGameScene());
	}

	public void OnQuit()
	{
		Application.Quit();
	}

	public void OnSettingsPressed()
	{
		SingletonWindow<SettingsWindow>.Instance.Open();
	}
}
