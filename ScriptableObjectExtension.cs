using UnityEngine;

public static class ScriptableObjectExtension
{
	public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
	{
		if (scriptableObject == null)
		{
			Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
			return (T)ScriptableObject.CreateInstance(typeof(T));
		}
		T val = Object.Instantiate(scriptableObject);
		val.name = scriptableObject.name;
		return val;
	}
}
