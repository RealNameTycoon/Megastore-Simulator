using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class ReverbManager : SingletonBehaviour<ReverbManager>
{
	public AudioMixer mixer;

	private Coroutine transitionRoutine;

	private new void Awake()
	{
		base.Awake();
		Object.DontDestroyOnLoad(this);
		SetArea(ReverbAreaType.OUTSIDE);
	}

	public void SetArea(ReverbAreaType area)
	{
		if (transitionRoutine != null)
		{
			StopCoroutine(transitionRoutine);
		}
		Debug.Log("Current Reverb Area: " + area);
		transitionRoutine = StartCoroutine(Transition(area));
	}

	private IEnumerator Transition(ReverbAreaType area)
	{
		float targetReverb;
		float targetDecay;
		float targetDry;
		switch (area)
		{
		case ReverbAreaType.SUPERMARKET:
			targetReverb = -600f;
			targetDecay = 3.3f;
			targetDry = -400f;
			break;
		case ReverbAreaType.WAREHOUSE:
			targetReverb = -600f;
			targetDecay = 2.4f;
			targetDry = -800f;
			break;
		case ReverbAreaType.SERVICE_ROOM:
			targetReverb = -2200f;
			targetDecay = 1.1f;
			targetDry = -600f;
			break;
		default:
			targetReverb = -10000f;
			targetDecay = 0.8f;
			targetDry = 0f;
			break;
		}
		mixer.GetFloat("ReverbLevel", out var currentReverb);
		mixer.GetFloat("DecayTime", out var currentDecay);
		mixer.GetFloat("DryLevel", out var currentDry);
		Debug.Log($"Reverb Get: {currentReverb} {targetReverb}");
		Debug.Log($"Decay Get: {currentDecay} {targetDecay}");
		Debug.Log($"Dry Get: {currentDry} {targetDry}");
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * 1.8f;
			mixer.SetFloat("ReverbLevel", Mathf.Lerp(currentReverb, targetReverb, t));
			mixer.SetFloat("DecayTime", Mathf.Lerp(currentDecay, targetDecay, t));
			mixer.SetFloat("DryLevel", Mathf.Lerp(currentDry, targetDry, t));
			yield return null;
		}
	}
}
