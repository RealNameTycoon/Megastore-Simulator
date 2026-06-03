using System;
using System.Collections;
using UnityEngine;

public static class CoroutineExtensions
{
	public static void WaitAndPerform(this MonoBehaviour gameObject, float delay, Action action)
	{
		gameObject.StartCoroutine(WaitAndPerform(delay, action));
	}

	public static void WaitAndPerformRealtime(this MonoBehaviour gameObject, float delay, Action action)
	{
		gameObject.StartCoroutine(WaitAndPerformRealtime(delay, action));
	}

	public static IEnumerator WaitAndPerform(float delay, Action action)
	{
		yield return new WaitForSeconds(delay);
		action?.Invoke();
	}

	public static void WaitFrameAndPerform(this MonoBehaviour gameObject, Action action)
	{
		gameObject.StartCoroutine(WaitFrameAndPerform(action));
	}

	public static IEnumerator WaitFrameAndPerform(Action action)
	{
		yield return new WaitForEndOfFrame();
		action?.Invoke();
	}

	public static IEnumerator WaitAndPerformRealtime(float delay, Action action)
	{
		yield return new WaitForSecondsRealtime(delay);
		action?.Invoke();
	}
}
