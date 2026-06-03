using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Forklift : PalletCarrierVehicle
{
	[SerializeField]
	private AudioSource engineSource;

	[SerializeField]
	private AudioSource reverseSource;

	[SerializeField]
	private AudioSource liftSource;

	[SerializeField]
	private NewCarController m_Car;

	public Transform fork;

	public Transform mast;

	public float speedTranslate;

	public Transform forkTopPosition;

	public Transform forkBottomPosition;

	public Transform mastTopPosition;

	public Transform mastBottomPosition;

	public Transform handle;

	private const float HANDLE_TARGET_X = 15f;

	private bool mastMoveTrue = true;

	private Vector3 handleForwardEuler;

	private Vector3 handleBackwardEuler;

	private const float MAX_ENGINE_VOLUME = 0.348f;

	private const float MIN_ENGINE_VOLUME = 0f;

	private float liftVolume;

	private float epsilon = 0.01f;

	private float steeringInput;

	private float steeringSpeed = 4f;

	protected override void Awake()
	{
		base.Awake();
		engineSource.clip = SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.FORKLIFT_ELECTRIC_ENGINE);
		engineSource.loop = true;
		reverseSource.clip = SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.REVERSE_BEEP);
		reverseSource.loop = true;
		liftSource.clip = SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.HYDRAULIC_LIFT);
		liftSource.loop = true;
		handleForwardEuler = new Vector3(15f, handle.localEulerAngles.y, handle.localEulerAngles.z);
		handleBackwardEuler = new Vector3(345f, handle.localEulerAngles.y, handle.localEulerAngles.z);
		liftVolume = liftSource.volume;
	}

	private void FixedUpdate()
	{
		if (isDriving)
		{
			float target = SingletonBehaviour<InputManager>.Instance.SteeringActionRef.action.ReadValue<float>();
			steeringInput = Mathf.MoveTowards(steeringInput, target, steeringSpeed * Time.deltaTime);
			float num = SingletonBehaviour<InputManager>.Instance.ForwardActionRef.action.ReadValue<float>();
			float num2 = SingletonBehaviour<InputManager>.Instance.ReverseActionRef.action.ReadValue<float>();
			float num3 = num - num2;
			if (!engineSource.isPlaying && m_Car.CurrentSpeed > 0.1f)
			{
				engineSource.Play();
			}
			bool flag = m_Car.GoingForward();
			if (!reverseSource.isPlaying && !flag && m_Car.CurrentSpeed > 5f)
			{
				reverseSource.Play();
			}
			else if (flag && reverseSource.isPlaying)
			{
				reverseSource.Stop();
			}
			float num4 = Input.GetAxis("Jump");
			if (Mathf.Approximately(num3, 0f))
			{
				num4 = 1f;
			}
			m_Car.Move(steeringInput, num3, num4, num4);
		}
		else if (!Mathf.Approximately(m_Car.CurrentSpeed, 0f))
		{
			m_Car.Move(0f, 0f, 0f, 1f);
		}
		if (m_Car.CurrentSpeed < 0.005f && reverseSource.isPlaying)
		{
			reverseSource.Stop();
		}
		engineSource.volume = Mathf.Lerp(0f, 0.348f, m_Car.CurrentSpeed / m_Car.MaxSpeed);
		engineSource.pitch = Mathf.Lerp(1f, 1.5f, m_Car.CurrentSpeed / m_Car.MaxSpeed);
		if (engineSource.volume < 0.005f && engineSource.isPlaying)
		{
			engineSource.Stop();
		}
	}

	public override bool CanUnload()
	{
		if (base.CanUnload())
		{
			if (Mathf.InverseLerp(forkBottomPosition.localPosition.y, forkTopPosition.localPosition.y, fork.transform.localPosition.y) < epsilon || interactedPalletShelfs.Count > 0)
			{
				return true;
			}
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("cant_unload_pallet_forklift", base.transform);
			return false;
		}
		return false;
	}

	public void Update()
	{
		if (!isDriving)
		{
			return;
		}
		if (fork.transform.localPosition.y >= mastTopPosition.localPosition.y && fork.transform.localPosition.y < forkTopPosition.localPosition.y)
		{
			mastMoveTrue = true;
		}
		else
		{
			mastMoveTrue = false;
		}
		if (fork.transform.localPosition.y <= mastTopPosition.localPosition.y)
		{
			mastMoveTrue = false;
		}
		if (SingletonBehaviour<InputManager>.Instance.IsPressingLift)
		{
			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition, forkTopPosition.localPosition, speedTranslate * Time.deltaTime);
			float num = Mathf.InverseLerp(forkBottomPosition.localPosition.y, forkTopPosition.localPosition.y, fork.transform.localPosition.y);
			if (mastMoveTrue)
			{
				mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition, mastTopPosition.localPosition, speedTranslate * Time.deltaTime);
			}
			if (num > 1f - epsilon && liftSource.isPlaying)
			{
				liftSource.DOKill();
				liftSource.DOFade(0f, 0.5f).OnComplete(delegate
				{
					liftSource.Stop();
				});
			}
			else if (num < 1f - epsilon && (!liftSource.isPlaying || DOTween.IsTweening(liftSource)))
			{
				liftSource.DOKill();
				liftSource.volume = liftVolume;
				liftSource.Play();
			}
		}
		if (SingletonBehaviour<InputManager>.Instance.IsPressingLower)
		{
			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition, forkBottomPosition.localPosition, speedTranslate * Time.deltaTime);
			float num2 = Mathf.InverseLerp(forkBottomPosition.localPosition.y, forkTopPosition.localPosition.y, fork.transform.localPosition.y);
			if (mastMoveTrue)
			{
				mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition, mastBottomPosition.localPosition, speedTranslate * Time.deltaTime);
			}
			if (num2 < epsilon && liftSource.isPlaying)
			{
				liftSource.DOFade(0f, 0.5f).OnComplete(delegate
				{
					liftSource.Stop();
				});
			}
			else if (num2 > epsilon && (!liftSource.isPlaying || DOTween.IsTweening(liftSource)))
			{
				liftSource.DOKill();
				liftSource.volume = liftVolume;
				liftSource.Play();
			}
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionReleasedThisFrame(SingletonBehaviour<InputManager>.Instance.LowerActionRef) || SingletonBehaviour<InputManager>.Instance.WasActionReleasedThisFrame(SingletonBehaviour<InputManager>.Instance.LiftActionRef))
		{
			handle.DOKill();
			handle.DOLocalRotate(new Vector3(0f, handle.localEulerAngles.y, handle.localEulerAngles.z), 45f).SetSpeedBased(isSpeedBased: true);
			if (liftSource.isPlaying)
			{
				liftSource.DOFade(0f, 0.5f).OnComplete(delegate
				{
					liftSource.Stop();
				});
			}
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.LowerActionRef))
		{
			handle.DOKill();
			handle.DOLocalRotate(handleBackwardEuler, 45f).SetSpeedBased(isSpeedBased: true);
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.LiftActionRef))
		{
			handle.DOKill();
			handle.DOLocalRotate(handleForwardEuler, 45f).SetSpeedBased(isSpeedBased: true);
		}
	}

	public override Dictionary<KeyCode, (string, Action)> GetExtraButtonActions()
	{
		return new Dictionary<KeyCode, (string, Action)>
		{
			{
				KeyCode.UpArrow,
				("forklift_up_hold", null)
			},
			{
				KeyCode.DownArrow,
				("forklift_down_hold", null)
			}
		};
	}

	public override void OnLeft()
	{
		base.OnLeft();
		if (liftSource.isPlaying)
		{
			liftSource.DOKill();
			liftSource.DOFade(0f, 0.5f).OnComplete(delegate
			{
				liftSource.Stop();
			});
		}
	}

	public override void ResetToInitialPosition()
	{
		m_Car.DisableMovement();
		m_Car.enabled = false;
		StartCoroutine(ResetToInitialPositionCoroutine(initialPosition, initialEulerAngles));
	}

	private IEnumerator ResetToInitialPositionCoroutine(Vector3 position, Vector3 eulerAngles)
	{
		yield return null;
		yield return new WaitForFixedUpdate();
		base.transform.position = position;
		base.transform.localEulerAngles = eulerAngles;
		SaveLocation();
		m_Car.enabled = true;
		m_Car.EnableMovement();
	}
}
