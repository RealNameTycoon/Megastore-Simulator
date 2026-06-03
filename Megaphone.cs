using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Megaphone : HotkeyClickable
{
	[SerializeField]
	private Transform usingPosition;

	[SerializeField]
	private AudioSource megaphoneAudioSource;

	[SerializeField]
	private AudioSource megaphoneNonSpeakingAudioSource;

	private string micDevice;

	private bool isUsing;

	private WaitForSeconds waitToEndUse = new WaitForSeconds(ButtonsWindow.CLICK_HOLD_CALLBACK_FREQUENCY + 0.1f);

	private Coroutine endUseCoroutine;

	private Coroutine micStartCoroutine;

	private Transform megaphoneParent;

	public static Megaphone Instance { get; protected set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		megaphoneParent = base.transform.parent;
		base.transform.position = putDownPosition.position;
		base.transform.eulerAngles = putDownPosition.eulerAngles;
		base.gameObject.SetActive(value: false);
		RefreshMicDevice();
	}

	private void RefreshMicDevice()
	{
		try
		{
			if (Microphone.devices != null && Microphone.devices.Length != 0)
			{
				micDevice = Microphone.devices[0];
			}
			else
			{
				micDevice = null;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Megaphone: Microphone.devices failed: " + ex.Message);
			micDevice = null;
		}
	}

	private bool CanUseMic()
	{
		if (megaphoneAudioSource == null)
		{
			Debug.LogWarning("Megaphone: megaphoneAudioSource is not assigned.");
			return false;
		}
		RefreshMicDevice();
		if (string.IsNullOrEmpty(micDevice))
		{
			Debug.LogWarning("Megaphone: No microphone device available.");
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("no_microphone_device_available", base.transform);
			return false;
		}
		return true;
	}

	public override void PickUp()
	{
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
			endUseCoroutine = null;
		}
		isUsing = false;
		base.PickUp();
	}

	private void MoveToPlayerHolding()
	{
		base.transform.DOKill();
		base.transform.DOLocalMove(pickUpPosition.localPosition, GetPickUpSpeed()).SetSpeedBased(isSpeedBased: true);
		base.transform.DOLocalRotate(pickUpPosition.localEulerAngles, GetPickUpSpeedRotation()).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
		{
		});
	}

	public override void OnPickedUp()
	{
		UpdateMenu();
		EventManager.NotifyEvent(GameEvents.MEGAPHONE_PICKED);
	}

	public void UpdateMenu()
	{
		if (base.IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("hold_use", delegate
				{
					Use();
				})
			} });
		}
	}

	public override void RepaintButtonsForEndHover()
	{
		UpdateMenu();
	}

	public void Use()
	{
		if (!isUsing)
		{
			isUsing = true;
			base.transform.DOKill();
			base.transform.DOLocalMove(usingPosition.localPosition, GetPickUpSpeed()).SetSpeedBased(isSpeedBased: true);
			base.transform.DOLocalRotate(usingPosition.localEulerAngles, GetPickUpSpeedRotation()).SetSpeedBased(isSpeedBased: true);
			StartMegaphoneMic();
			megaphoneNonSpeakingAudioSource?.PlayOneShot(SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.MEGAPHONE_USE_START));
		}
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
		}
		endUseCoroutine = StartCoroutine(EndUseCoroutine());
	}

	private void StartMegaphoneMic()
	{
		if (micStartCoroutine != null)
		{
			StopCoroutine(micStartCoroutine);
			micStartCoroutine = null;
		}
		if (CanUseMic())
		{
			micStartCoroutine = StartCoroutine(StartMicAndPlayCoroutine());
		}
	}

	private IEnumerator StartMicAndPlayCoroutine()
	{
		SafeStopMic(immediate: true);
		string device = micDevice;
		int frequency = 44100;
		AudioClip clip;
		try
		{
			clip = Microphone.Start(device, loop: true, 1, frequency);
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("megaphone_speak", base.transform);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Megaphone: Microphone.Start failed for '" + device + "': " + ex.Message);
			yield break;
		}
		if (clip == null)
		{
			Debug.LogWarning("Megaphone: Microphone.Start returned null clip for '" + device + "'.");
			yield break;
		}
		float t = 0f;
		int pos = 0;
		while (t < 1f)
		{
			if (!isUsing)
			{
				try
				{
					Microphone.End(device);
					yield break;
				}
				catch
				{
					yield break;
				}
			}
			try
			{
				pos = Microphone.GetPosition(device);
			}
			catch
			{
				pos = 0;
			}
			if (pos > 0)
			{
				break;
			}
			t += Time.unscaledDeltaTime;
			yield return null;
		}
		if (pos <= 0)
		{
			Debug.LogWarning("Megaphone: Mic start timeout for '" + device + "'.");
			try
			{
				Microphone.End(device);
				yield break;
			}
			catch
			{
				yield break;
			}
		}
		megaphoneAudioSource.clip = clip;
		megaphoneAudioSource.loop = true;
		megaphoneAudioSource.DOKill();
		megaphoneAudioSource.volume = 1f;
		try
		{
			megaphoneAudioSource.Play();
		}
		catch (Exception ex2)
		{
			Debug.LogWarning("Megaphone: AudioSource.Play failed: " + ex2.Message);
			try
			{
				Microphone.End(device);
				yield break;
			}
			catch
			{
				yield break;
			}
		}
		micStartCoroutine = null;
	}

	private IEnumerator EndUseCoroutine()
	{
		yield return waitToEndUse;
		StopUse();
	}

	public override void OnPutDown()
	{
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
			endUseCoroutine = null;
		}
		EventManager.NotifyEvent(GameEvents.MEGAPHONE_RELEASED);
		isUsing = false;
		megaphoneAudioSource.DOKill();
		megaphoneAudioSource.DOFade(0f, 0.5f).OnComplete(delegate
		{
			megaphoneAudioSource.Stop();
			Microphone.End(micDevice);
		});
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	private void SafeStopMic(bool immediate)
	{
		if (micStartCoroutine != null)
		{
			StopCoroutine(micStartCoroutine);
			micStartCoroutine = null;
		}
		if (megaphoneAudioSource == null)
		{
			return;
		}
		megaphoneAudioSource.DOKill();
		if (immediate)
		{
			try
			{
				megaphoneAudioSource.Stop();
			}
			catch
			{
			}
			if (string.IsNullOrEmpty(micDevice))
			{
				return;
			}
			try
			{
				Microphone.End(micDevice);
				return;
			}
			catch
			{
				return;
			}
		}
		megaphoneAudioSource.DOFade(0f, 0.5f).OnComplete(delegate
		{
			try
			{
				megaphoneAudioSource.Stop();
			}
			catch
			{
			}
			if (!string.IsNullOrEmpty(micDevice))
			{
				try
				{
					Microphone.End(micDevice);
				}
				catch
				{
				}
			}
		});
	}

	private void StopUse(bool immediate = false)
	{
		if (!isUsing)
		{
			return;
		}
		isUsing = false;
		MoveToPlayerHolding();
		megaphoneAudioSource.DOKill();
		megaphoneNonSpeakingAudioSource.PlayOneShot(SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.MEGAPHONE_USE_END));
		if (immediate)
		{
			megaphoneAudioSource.Stop();
			Microphone.End(micDevice);
		}
		else
		{
			megaphoneAudioSource.DOFade(0f, 0.5f).OnComplete(delegate
			{
				megaphoneAudioSource.Stop();
				Microphone.End(micDevice);
			});
		}
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	public override void Reset()
	{
		base.Reset();
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
			endUseCoroutine = null;
		}
		StopUse(immediate: true);
	}

	public override LayerMask GetInteractableLayers()
	{
		return 0;
	}
}
