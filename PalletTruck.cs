using DG.Tweening;
using UnityEngine;

public class PalletTruck : PalletCarrierVehicle
{
	[SerializeField]
	private AudioSource wheelSource;

	private float initialVolume;

	private const float PALLET_SPEED_EFFECT = 0.6f;

	protected override void Awake()
	{
		base.Awake();
		initialVolume = wheelSource.volume;
	}

	public override void StopAnimation()
	{
		base.StopAnimation();
		if (wheelSource.isPlaying)
		{
			wheelSource.DOKill();
			wheelSource.DOFade(0f, 0.3f).OnComplete(delegate
			{
				wheelSource.Stop();
			});
		}
	}

	public override void StartAnimation()
	{
		base.StartAnimation();
		if (!wheelSource.isPlaying || DOTween.IsTweening(wheelSource))
		{
			wheelSource.DOKill();
			wheelSource.volume = initialVolume;
			wheelSource.Play();
		}
	}

	public override float SpeedMultiplier()
	{
		if (base.VehicleType == VehicleType.PALLET_JACK && base.ContainsPallet)
		{
			return base.SpeedMultiplier() * 0.6f;
		}
		return base.SpeedMultiplier();
	}
}
