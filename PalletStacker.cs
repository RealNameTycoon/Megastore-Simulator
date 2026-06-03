using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PalletStacker : PalletTruck
{
	[SerializeField]
	private AudioSource liftSource;

	public Transform fork;

	public Transform mast;

	public float speedTranslate;

	public Transform forkTopPosition;

	public Transform forkBottomPosition;

	public Transform mastTopPosition;

	public Transform mastBottomPosition;

	private bool mastMoveTrue = true;

	private float liftVolume;

	private float epsilon = 0.01f;

	protected override void Awake()
	{
		base.Awake();
		liftSource.clip = SingletonBehaviour<AudioManager>.Instance.GetAudioClip(AudioManager.AudioTypes.HYDRAULIC_LIFT);
		liftSource.loop = true;
		liftVolume = liftSource.volume;
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
		if ((SingletonBehaviour<InputManager>.Instance.WasActionReleasedThisFrame(SingletonBehaviour<InputManager>.Instance.LowerActionRef) || SingletonBehaviour<InputManager>.Instance.WasActionReleasedThisFrame(SingletonBehaviour<InputManager>.Instance.LiftActionRef)) && liftSource.isPlaying)
		{
			liftSource.DOFade(0f, 0.5f).OnComplete(delegate
			{
				liftSource.Stop();
			});
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
}
