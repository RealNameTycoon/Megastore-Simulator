using System;
using System.Collections;
using System.IO;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialVideoWindowUI : SingletonWindow<TutorialVideoWindowUI>
{
	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	private TMP_Text titleText;

	[SerializeField]
	private RawImage videoRawImage;

	[SerializeField]
	private TMP_Text descriptionText;

	[SerializeField]
	private Button okButton;

	[SerializeField]
	private VideoPlayer videoPlayer;

	private static string tutorialShownKey = "tutorialShownKey";

	public event Action OnClosed;

	private new void Awake()
	{
		base.Awake();
		EventManager.AddListener<int>(GameEvents.PALLET_TAKEN, delegate
		{
			Open(TutorialVideoWindowType.PALLET_DISPOSAL);
		});
		EventManager.AddListener<PlaceableType>(PlaceableEvents.NEW_PLACEABLE_PLACED, OnNewPlaceablePlaced);
		EventManager.AddListener<int>(TutorialEvents.TUTORIAL_STEP_DONE, OnTutorialStepDone);
		okButton?.onClick.AddListener(Close);
		if (videoPlayer != null)
		{
			videoPlayer.playOnAwake = false;
			videoPlayer.isLooping = true;
			videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
		}
	}

	private void OnTutorialStepDone(int tutorialStep)
	{
		if (tutorialStep == 5)
		{
			StartCoroutine(OpenTutorialRoutine(TutorialVideoWindowType.HANDTRUCK_TUTORIAL));
		}
	}

	private void OnNewPlaceablePlaced(PlaceableType type)
	{
		if (type == PlaceableType.VENDING_MACHINE)
		{
			Open(TutorialVideoWindowType.VENDING_COLLECT_MONEY);
		}
	}

	private IEnumerator OpenTutorialRoutine(TutorialVideoWindowType type)
	{
		yield return new WaitForSeconds(1f);
		Open(type);
	}

	public void Open(TutorialVideoWindowType type)
	{
		if (!SingletonBehaviour<UIStackManager>.Instance.IsAnyWindowOpen() && !GenericDataSerializer.HasKey(tutorialShownKey + type))
		{
			GenericDataSerializer.SaveBool(tutorialShownKey + type, value: true);
			titleText.text = Locale.GetWord(type.ToString() + "_title");
			descriptionText.text = Locale.GetWord(type.ToString() + "_desc");
			if (videoPlayer != null)
			{
				string url = Path.Combine(Path.Combine(Application.streamingAssetsPath, "TutorialVideos"), type.ToString() + ".mp4");
				videoPlayer.source = VideoSource.Url;
				videoPlayer.url = url;
				videoPlayer.Prepare();
				videoPlayer.prepareCompleted += OnPrepared;
			}
		}
	}

	private void OnPrepared(VideoPlayer vp)
	{
		vp.prepareCompleted -= OnPrepared;
		vp.Play();
		base.Open();
		raycaster.enabled = true;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
	}

	public override void Close()
	{
		base.Close();
		if (videoPlayer != null)
		{
			videoPlayer.prepareCompleted -= OnPrepared;
			videoPlayer.Stop();
			videoPlayer.url = "";
		}
		this.OnClosed?.Invoke();
		raycaster.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
	}
}
