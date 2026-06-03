using System.Collections.Generic;
using UnityEngine;

public class ProducePlaceble : Placeable
{
	[SerializeField]
	private List<ParticleSystem> waterSprayParticleSystems;

	[SerializeField]
	private AudioSource waterSprayAudioSource;

	private const float WATER_SPRAY_DISTANCE = 20f;

	private void Awake()
	{
		EventManager.AddListener(PlaceableEvents.WATER_SPRAY_STARTED, OnWaterSprayStarted);
		EventManager.AddListener(PlaceableEvents.WATER_SPRAY_ENDED, OnWaterSprayEnded);
	}

	private void OnWaterSprayStarted()
	{
		if (!IsPlacing() && !AllShelvesAreEmpty())
		{
			if (Vector3.Distance(base.transform.position, SingletonBehaviour<PlayerMove>.Instance.transform.position) < 20f)
			{
				waterSprayAudioSource.Play();
			}
			for (int i = 0; i < waterSprayParticleSystems.Count; i++)
			{
				waterSprayParticleSystems[i].Play();
			}
		}
	}

	private void OnWaterSprayEnded()
	{
		waterSprayAudioSource.Stop();
		for (int i = 0; i < waterSprayParticleSystems.Count; i++)
		{
			waterSprayParticleSystems[i].Stop();
		}
	}
}
