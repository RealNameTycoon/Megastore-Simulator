using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LightSwitch : Clickable
{
	[SerializeField]
	private Transform lightSwitch;

	[SerializeField]
	private GameObject lightParent;

	[SerializeField]
	private Material lightEmmisiveMaterial;

	[SerializeField]
	private List<ReflectionProbe> reflectionProbes;

	private bool isOn = true;

	public bool IsOn => isOn;

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		isOn = !isOn;
		SingletonBehaviour<TimeManager>.Instance.RepaintLights(IsOn);
		lightSwitch.DOKill();
		lightSwitch.DOLocalRotate(isOn ? (Vector3.forward * 7f) : (Vector3.forward * -7f), 0.3f);
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TOGGLE);
	}
}
