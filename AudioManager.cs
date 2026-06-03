using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DFTGames.Localization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonBehaviour<AudioManager>
{
	[Serializable]
	public class AudioDictionary : UnitySerializedDictionary<AudioTypes, AudioClipArray>
	{
	}

	[Serializable]
	public class UIAudioDictionary : UnitySerializedDictionary<UIAudioTypes, AudioClip>
	{
	}

	[Serializable]
	public class StingerAudioDictionary : UnitySerializedDictionary<StingerTypes, AudioClip>
	{
	}

	[Serializable]
	public class OccasionalAudioDictionary : UnitySerializedDictionary<OccasionalAudioTypes, AudioClipArray>
	{
	}

	[Serializable]
	public enum UIAudioTypes
	{
		MOUSE_CLICK,
		TUTORIAL_STEP_DONE,
		CLICK_GENERIC,
		ERROR_TOOLTIP,
		TOGGLE,
		MONITOR_OPENED,
		POS_APPEAR,
		HANDSCANNER_SWITCH,
		ELECTRICAL_BEEP
	}

	[Serializable]
	public enum AudioTypes
	{
		CASH_REGISTER_OPEN,
		GIVE_COIN_1,
		PAPER_SLIDE_1,
		PAPER_SLIDE_2,
		PAPER_SLIDE_3,
		PAPER_SLIDE_4,
		PAYMENT_DONE,
		SCANNER_BEEP,
		WHOOSH_QUICK,
		WHOOSH_SHORT_1,
		WHOOSH_SHORT_2,
		FOOT_STEPS,
		BOX_THROW,
		BOX_APPEAR,
		POS_CLICK,
		BOX_OPEN,
		BOX_CLOSE,
		MAN_YES,
		WOMAN_YES,
		OLD_MAN_NO,
		WOMAN_NO,
		OLD_WOMAN_YES,
		OLD_MAN_YES,
		MAN_NO,
		BOX_GARBAGE,
		RECEIPT_PRINT_1,
		RECEIPT_PRINT_2,
		RECEIPT_PRINT_3,
		POS_APPEAR,
		CHECKOUT_BARRIER_OPEN,
		CHECKOUT_BARRIER_CLOSE,
		FORKLIFT_ELECTRIC_ENGINE,
		REVERSE_BEEP,
		HYDRAULIC_LIFT,
		PALLET_TAKE,
		PALLET_TRUCK_ROLL,
		MEGAPHONE_USE_START,
		MEGAPHONE_USE_END,
		PLACEMENT_ENDED_WOOD,
		PLACEMENT_STARTED_WOOD,
		HAND_SCANNER_BEEP,
		CHOP_SOUND,
		PACK_PLASTIC,
		FREEZER_DOOR_OPEN,
		FREEZER_DOOR_CLOSE
	}

	[Serializable]
	public enum OccasionalAudioTypes
	{
		CHECKOUT_LONG_WAIT_FEMALE,
		CHECKOUT_LONG_WAIT_MALE,
		REGISTER_GREETING_FEMALE,
		REGISTER_GREETING_MALE,
		PRICE_ADJUSTMENT_WAITING_FEMALE,
		PRICE_ADJUSTMENT_WAITING_MALE,
		BAKERY_FRESHNESS_CONCERN_FEMALE,
		BAKERY_FRESHNESS_CONCERN_MALE,
		ITEM_OUT_OF_STOCK_FEMALE,
		ITEM_OUT_OF_STOCK_MALE,
		PRICE_COMPLAINT_FEMALE,
		PRICE_COMPLAINT_MALE
	}

	[Serializable]
	public enum StingerTypes
	{
		WIN,
		LOSE,
		HORN
	}

	[Serializable]
	public struct AudioClipArray
	{
		public AudioClip[] clips;
	}

	[SerializeField]
	private AudioMixer mainMixer;

	[SerializeField]
	private AudioSource m_MusicSource;

	[SerializeField]
	private AudioSource m_UISource;

	[SerializeField]
	private AudioSource gameplaySource;

	[SerializeField]
	private AudioSource gameSource;

	[SerializeField]
	private AudioSource ambienceSource;

	[SerializeField]
	private AudioClip ambienceMusic;

	[SerializeField]
	private AudioClip startMusic;

	[SerializeField]
	private AudioClip menuMusic;

	[SerializeField]
	private AudioClip battleMusic;

	[SerializeField]
	private AudioClip ambienceMorning;

	[SerializeField]
	private AudioClip ambienceNight;

	[SerializeField]
	private AudioClip gameMusic;

	[SerializeField]
	private AudioDictionary audioClips;

	[SerializeField]
	private UIAudioDictionary UIAudioClips;

	[SerializeField]
	private StingerAudioDictionary StingerAudioClips;

	[SerializeField]
	private OccasionalAudioDictionary occasionalAudioClips;

	[SerializeField]
	private bool debug;

	private Dictionary<OccasionalAudioTypes, List<int>> occasionalAudioPlayTimes = new Dictionary<OccasionalAudioTypes, List<int>>();

	private float m_CurrentAudioVolume;

	public static float DISTANCE_BUFFER = 3f;

	private bool isStartMusic = true;

	[SerializeField]
	private float musicVolume;

	[SerializeField]
	private float soundVolume = 1f;

	private const string musicMutedKey = "MusicMuted";

	private const string soundMutedKey = "SoundMuted";

	private const string masterAudioVolume = "masterAudioVolume";

	private const string musicAudioVolume = "musicAudioVolume";

	private const string dialogueAudioVolume = "dialogueAudioVolume";

	private const string sfxAudioVolume = "sfxAudioVolume";

	private const string ambientAudioVolume = "ambientAudioVolume";

	private bool musicMuted;

	private bool soundMuted;

	private const float AMBIENCE_SOUND_VOLUME = 0.07f;

	private bool insideSupermarket;

	private float masterAudioMultiplier = 1f;

	private float musicAudioMultiplier = -1f;

	private float speechAudioMultiplier = 1f;

	private float sfxAudioMultiplier = 1f;

	private float ambienceAudioMultiplier = 1f;

	private WaitForSeconds complaintFrequency = new WaitForSeconds(30f);

	private bool canPlaySpeech = true;

	private const string MASTER_VOLUME = "MasterVolume";

	private const string MUSIC_VOLUME = "MusicVolume";

	private const string SFX_VOLUME = "SFXVolume";

	private const string DIALOGUE_VOLUME = "DialogueVolume";

	private const string AMBIENCE_VOLUME = "AmbienceVolume";

	public bool IsVolumeOn => AudioListener.volume > 0f;

	public bool IsMusicOn => !musicMuted;

	public bool IsSoundOn => !soundMuted;

	public float MasterAudioMultiplier => masterAudioMultiplier;

	public float MusicAudioMultiplier => musicAudioMultiplier;

	public float SpeechAudioMultiplier => speechAudioMultiplier;

	public float SFXAudioMultiplier => sfxAudioMultiplier;

	public float AmbienceAudioMultiplier => ambienceAudioMultiplier;

	private new void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(this);
		SceneManager.sceneLoaded += OnSceneLoaded;
		musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
		soundMuted = false;
		masterAudioMultiplier = PlayerPrefs.GetFloat("masterAudioVolume", 1f);
		if (!PlayerPrefs.HasKey("musicAudioVolume") && GenericDataSerializer.HasKey("musicAudioVolume"))
		{
			musicAudioMultiplier = GenericDataSerializer.LoadFloat("musicAudioVolume", 0.5f);
			GenericDataSerializer.DeleteKey("musicAudioVolume");
			PlayerPrefs.SetFloat("musicAudioVolume", musicAudioMultiplier);
		}
		else
		{
			musicAudioMultiplier = PlayerPrefs.GetFloat("musicAudioVolume", 0.5f);
		}
		speechAudioMultiplier = PlayerPrefs.GetFloat("dialogueAudioVolume", 0.7f);
		sfxAudioMultiplier = PlayerPrefs.GetFloat("sfxAudioVolume", 1f);
		ambienceAudioMultiplier = PlayerPrefs.GetFloat("ambientAudioVolume", 1f);
		m_MusicSource.volume = (musicMuted ? 0f : musicVolume);
		m_UISource.volume = (soundMuted ? 0f : soundVolume);
		m_UISource.ignoreListenerPause = true;
		m_MusicSource.ignoreListenerPause = true;
		m_MusicSource.Play();
	}

	private void Start()
	{
		SetMasterAudioMultiplier(masterAudioMultiplier);
		SetMusicMultiplier(musicAudioMultiplier);
		SetDialogueMultiplier(speechAudioMultiplier);
		SetSFXMultiplier(sfxAudioMultiplier);
		SetAmbienceMultiplier(ambienceAudioMultiplier);
		LoadLanguageAssets();
		LocalizeBase.OnLanguageChanged += delegate
		{
			LoadLanguageAssets();
		};
	}

	private void LoadLanguageAssets()
	{
		StartCoroutine(LoadLanguage());
	}

	private IEnumerator LoadLanguage()
	{
		string languageCode = Locale.GetWord("KEY");
		if (debug)
		{
			Debug.Log("Loading language assets for: " + languageCode);
		}
		occasionalAudioPlayTimes.Clear();
		foreach (OccasionalAudioTypes type in Enum.GetValues(typeof(OccasionalAudioTypes)))
		{
			string path = Path.Combine(Application.streamingAssetsPath, "Audio/OccasionalSounds", languageCode, type.ToString());
			if (!Directory.Exists(path))
			{
				path = Path.Combine(Application.streamingAssetsPath, "Audio/OccasionalSounds", "EN", type.ToString());
			}
			List<AudioClip> clips = new List<AudioClip>();
			string[] files = Directory.GetFiles(path, "*.mp3");
			string[] files2 = Directory.GetFiles(path, "*.wav");
			List<string> list = new List<string>();
			list.AddRange(files);
			list.AddRange(files2);
			foreach (string file in list)
			{
				string text = Path.GetExtension(file).ToLowerInvariant();
				AudioType audioType;
				if (!(text == ".mp3"))
				{
					if (!(text == ".wav"))
					{
						continue;
					}
					audioType = AudioType.WAV;
				}
				else
				{
					audioType = AudioType.MPEG;
				}
				using UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(file, audioType);
				yield return req.SendWebRequest();
				if (req.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError("Audio load failed: " + file);
					continue;
				}
				AudioClip content = DownloadHandlerAudioClip.GetContent(req);
				content.name = Path.GetFileName(file);
				clips.Add(content);
			}
			if (clips.Count > 0)
			{
				occasionalAudioClips[type] = new AudioClipArray
				{
					clips = clips.ToArray()
				};
			}
		}
		if (!debug)
		{
			yield break;
		}
		Debug.Log("Language assets loaded for: " + languageCode);
		Debug.Log("Occasional audio clips: " + occasionalAudioClips.Count);
		Debug.Log("Occasional audio play times: " + occasionalAudioPlayTimes.Count);
		foreach (OccasionalAudioTypes value in Enum.GetValues(typeof(OccasionalAudioTypes)))
		{
			Debug.Log("Occasional audio clip: " + value.ToString() + " - " + occasionalAudioClips[value].clips.Length);
		}
	}

	public void SetMasterAudioMultiplier(float multiplier)
	{
		PlayerPrefs.SetFloat("masterAudioVolume", multiplier);
		masterAudioMultiplier = multiplier;
		gameplaySource.volume = soundVolume * masterAudioMultiplier;
		m_UISource.volume = soundVolume * masterAudioMultiplier;
		gameSource.volume = soundVolume * masterAudioMultiplier;
		ambienceSource.volume = (insideSupermarket ? (0.07f * masterAudioMultiplier / 4f) : (0.07f * masterAudioMultiplier * 0.75f));
		mainMixer.SetFloat("MasterVolume", LinearToDb(multiplier));
		EventManager.NotifyEvent(UIEvents.AUDIO_MULTIPLIER_CHANGED);
	}

	public void SetMusicMultiplier(float multiplier)
	{
		PlayerPrefs.SetFloat("musicAudioVolume", multiplier);
		musicAudioMultiplier = multiplier;
		Debug.Log("Music multiplier set to: " + multiplier);
		mainMixer.SetFloat("MusicVolume", LinearToDb(multiplier));
	}

	public void SetDialogueMultiplier(float multiplier)
	{
		PlayerPrefs.SetFloat("dialogueAudioVolume", multiplier);
		speechAudioMultiplier = multiplier;
		mainMixer.SetFloat("DialogueVolume", LinearToDb(multiplier));
		EventManager.NotifyEvent(UIEvents.DIALOGUE_MULTIPLIER_CHANGED);
	}

	public void SetSFXMultiplier(float multiplier)
	{
		PlayerPrefs.SetFloat("sfxAudioVolume", multiplier);
		sfxAudioMultiplier = multiplier;
		mainMixer.SetFloat("SFXVolume", LinearToDb(multiplier));
	}

	public void SetAmbienceMultiplier(float multiplier)
	{
		PlayerPrefs.SetFloat("ambientAudioVolume", multiplier);
		ambienceAudioMultiplier = multiplier;
		mainMixer.SetFloat("AmbienceVolume", LinearToDb(multiplier));
	}

	private float LinearToDb(float multiplier)
	{
		if (multiplier <= 0.0001f)
		{
			return -80f;
		}
		multiplier = Mathf.Clamp(multiplier, 0.0001f, 1f);
		return Mathf.Log10(multiplier) * 20f;
	}

	public void OnGamePaused()
	{
		ambienceSource.Pause();
	}

	public void OnGameResumed()
	{
		ambienceSource.UnPause();
	}

	public void OnNewDayStarted()
	{
		if (!soundMuted)
		{
			ambienceSource.DOKill();
			ambienceSource.DOFade(0f, 0.07f * masterAudioMultiplier).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
			{
				ambienceSource.clip = ambienceMorning;
				ambienceSource.loop = true;
				ambienceSource.Play();
				ambienceSource.DOFade(insideSupermarket ? (0.07f * masterAudioMultiplier / 5.5f) : (0.07f * masterAudioMultiplier), 0.07f).SetSpeedBased(isSpeedBased: true);
			});
		}
		else
		{
			ambienceSource.clip = ambienceMorning;
			ambienceSource.loop = true;
			ambienceSource.Play();
		}
	}

	public void OnSunDowned()
	{
		if (!soundMuted)
		{
			ambienceSource.DOKill();
			ambienceSource.DOFade(0f, 0.07f).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
			{
				ambienceSource.clip = ambienceNight;
				ambienceSource.loop = true;
				ambienceSource.Play();
				ambienceSource.DOFade(insideSupermarket ? (0.07f * masterAudioMultiplier / 4f) : (0.07f * masterAudioMultiplier), 0.07f).SetSpeedBased(isSpeedBased: true);
			});
		}
		else
		{
			ambienceSource.clip = ambienceNight;
			ambienceSource.loop = true;
			ambienceSource.Play();
		}
	}

	public void FadeInAmbience()
	{
		ambienceSource.DOKill();
		ambienceSource.DOFade(0.07f * masterAudioMultiplier, 0.07f).SetSpeedBased(isSpeedBased: true);
	}

	public void FadeOutAmbience()
	{
		ambienceSource.DOKill();
		ambienceSource.DOFade(0.07f * masterAudioMultiplier / 4f, 0.07f).SetSpeedBased(isSpeedBased: true);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			insideSupermarket = true;
			if (!soundMuted)
			{
				FadeOutAmbience();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			insideSupermarket = false;
			if (!soundMuted)
			{
				FadeInAmbience();
			}
		}
	}

	public void Initialize()
	{
		m_MusicSource.DOFade(0f, 1f).OnComplete(delegate
		{
			float endValue = (musicMuted ? 0f : musicVolume);
			m_MusicSource.clip = startMusic;
			m_MusicSource.Play();
			m_MusicSource.DOFade(endValue, 2f);
		});
		if (soundMuted)
		{
			gameplaySource.volume = 0f;
			m_UISource.volume = 0f;
			gameSource.volume = 0f;
			ambienceSource.volume = 0f;
		}
		else
		{
			gameplaySource.volume = soundVolume * masterAudioMultiplier;
			m_UISource.volume = soundVolume * masterAudioMultiplier;
			gameSource.volume = soundVolume * masterAudioMultiplier;
			ambienceSource.volume = 0.07f * masterAudioMultiplier;
		}
		if (SingletonBehaviour<TimeManager>.Instance.CurrentMin >= 750)
		{
			ambienceSource.clip = ambienceNight;
		}
		else
		{
			ambienceSource.clip = ambienceMorning;
		}
		ambienceSource.Play();
	}

	public void MuteSound()
	{
		gameplaySource.volume = 0f;
		m_UISource.volume = 0f;
		gameSource.volume = 0f;
		m_MusicSource.volume = 0f;
		ambienceSource.volume = 0f;
		ambienceSource.DOKill();
	}

	public void UnmuteSound()
	{
		m_MusicSource.volume = (musicMuted ? 0f : musicVolume);
		ambienceSource.DOKill();
		if (soundMuted)
		{
			gameplaySource.volume = 0f;
			m_UISource.volume = 0f;
			gameSource.volume = 0f;
			ambienceSource.volume = 0f;
		}
		else
		{
			gameplaySource.volume = soundVolume;
			m_UISource.volume = soundVolume;
			gameSource.volume = soundVolume;
			ambienceSource.volume = 0.07f * masterAudioMultiplier;
		}
	}

	public void ToggleMusic()
	{
		if (musicMuted)
		{
			m_MusicSource.volume = musicVolume;
		}
		else
		{
			m_MusicSource.volume = 0f;
		}
		musicMuted = !musicMuted;
		PlayerPrefs.SetInt("MusicMuted", musicMuted ? 1 : 0);
	}

	public void ToggleSound()
	{
		if (soundMuted)
		{
			gameplaySource.volume = soundVolume;
			m_UISource.volume = soundVolume;
			gameSource.volume = soundVolume;
			ambienceSource.volume = (insideSupermarket ? (0.07f * masterAudioMultiplier / 4f) : (0.07f * masterAudioMultiplier));
		}
		else
		{
			gameplaySource.volume = 0f;
			m_UISource.volume = 0f;
			gameSource.volume = 0f;
			ambienceSource.volume = 0f;
		}
		soundMuted = !soundMuted;
		PlayerPrefs.SetInt("SoundMuted", soundMuted ? 1 : 0);
	}

	public void ChangeMusicToMenu(float delay)
	{
		if (isStartMusic && Mathf.Approximately(m_MusicSource.volume, musicVolume))
		{
			m_MusicSource.DOKill();
			m_MusicSource.DOFade(0f, 1f).OnComplete(delegate
			{
				m_MusicSource.Stop();
				m_MusicSource.clip = menuMusic;
				isStartMusic = false;
				m_MusicSource.loop = true;
				m_MusicSource.Play();
				m_MusicSource.DOFade(musicVolume, 1f);
			}).SetDelay(delay);
		}
	}

	public void OnBackToMenu()
	{
		m_MusicSource.Stop();
		ambienceSource.Stop();
		insideSupermarket = false;
		AudioListener.pause = false;
	}

	public AudioClip GetAudioClip(AudioTypes soundType)
	{
		return audioClips[soundType].clips.GetRandomElement();
	}

	public AudioClip GetUIAudioClip(UIAudioTypes soundType)
	{
		return UIAudioClips[soundType];
	}

	public AudioClip GetOccasionalAudioClip(OccasionalAudioTypes soundType)
	{
		if (!occasionalAudioClips.ContainsKey(soundType))
		{
			return null;
		}
		if (!occasionalAudioPlayTimes.ContainsKey(soundType))
		{
			occasionalAudioPlayTimes[soundType] = new List<int>();
			for (int i = 0; i < GetOccasionalAudioCount(soundType); i++)
			{
				occasionalAudioPlayTimes[soundType].Add(0);
			}
			int num = UnityEngine.Random.Range(0, occasionalAudioPlayTimes[soundType].Count);
			occasionalAudioPlayTimes[soundType][num]++;
			return occasionalAudioClips[soundType].clips[num];
		}
		int num2 = int.MaxValue;
		for (int j = 0; j < occasionalAudioPlayTimes[soundType].Count; j++)
		{
			if (occasionalAudioPlayTimes[soundType][j] < num2)
			{
				num2 = occasionalAudioPlayTimes[soundType][j];
			}
		}
		List<int> list = new List<int>();
		for (int k = 0; k < occasionalAudioPlayTimes[soundType].Count; k++)
		{
			if (occasionalAudioPlayTimes[soundType][k] == num2)
			{
				list.Add(k);
			}
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		int num3 = list[index];
		occasionalAudioPlayTimes[soundType][num3]++;
		return occasionalAudioClips[soundType].clips[num3];
	}

	public int GetOccasionalAudioCount(OccasionalAudioTypes soundType)
	{
		if (occasionalAudioClips.ContainsKey(soundType))
		{
			return occasionalAudioClips[soundType].clips.Length;
		}
		return 0;
	}

	public void PlayUIAudio(UIAudioTypes soundType)
	{
		m_UISource.PlayOneShot(UIAudioClips[soundType]);
	}

	public void PlayAudio(AudioTypes type)
	{
		AudioSource audioSource = gameplaySource;
		if (audioSource != null && audioClips.ContainsKey(type))
		{
			audioSource.PlayOneShot(audioClips[type].clips.GetRandomElement());
		}
	}

	public void PlayCheckoutAudio(AudioTypes type)
	{
		AudioSource audioSource = gameSource;
		if (audioSource != null && audioClips.ContainsKey(type))
		{
			audioSource.PlayOneShot(audioClips[type].clips.GetRandomElement());
		}
	}

	public void PlayAudio(AudioClip clip)
	{
		AudioSource audioSource = gameplaySource;
		if (audioSource != null)
		{
			audioSource.PlayOneShot(clip);
		}
	}

	public bool CanPlaySpeech()
	{
		if (canPlaySpeech)
		{
			canPlaySpeech = false;
			StartCoroutine(ResetCanPlaySpeech());
			return true;
		}
		return false;
	}

	private IEnumerator ResetCanPlaySpeech()
	{
		yield return complaintFrequency;
		canPlaySpeech = true;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
	{
		if (scene.buildIndex == 0)
		{
			AudioListener.pause = false;
		}
	}
}
