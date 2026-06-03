using ToolBox.Serialization;
using UnityEngine;

public static class GenericDataSerializer
{
	public static void Save(string key, Vector3 dataToSave)
	{
		DataSerializer.Save(key, dataToSave);
	}

	public static void MigrateToPlayerPrefs(string key, float defaultValue)
	{
		if (HasKey(key))
		{
			float value = Load(key, defaultValue);
			PlayerPrefs.SetFloat(key, value);
			PlayerPrefs.Save();
			DeleteKey(key);
		}
	}

	public static Vector3 Load(string key)
	{
		return DataSerializer.Load<Vector3>(key);
	}

	public static void Save(string key, Quaternion dataToSave)
	{
		DataSerializer.Save(key, dataToSave);
	}

	public static Quaternion LoadQuaternion(string key)
	{
		return DataSerializer.Load<Quaternion>(key);
	}

	public static void Save<T>(string key, T dataToSave)
	{
		DataSerializer.Save(key, dataToSave);
	}

	public static void SaveInt(string key, int value)
	{
		DataSerializer.Save(key, value);
	}

	public static int LoadInt(string key, int defaultValue = 0)
	{
		return DataSerializer.Load(key, defaultValue);
	}

	public static void SaveString(string key, string value)
	{
		DataSerializer.Save(key, value);
	}

	public static string LoadString(string key, string value = "")
	{
		return DataSerializer.Load(key, value);
	}

	public static void SaveFloat(string key, float value)
	{
		DataSerializer.Save(key, value);
	}

	public static float LoadFloat(string key, float value = 0f)
	{
		return DataSerializer.Load(key, value);
	}

	public static void SaveBool(string key, bool value)
	{
		DataSerializer.Save(key, value);
	}

	public static bool LoadBool(string key, bool value = false)
	{
		return DataSerializer.Load(key, value);
	}

	public static T Load<T>(string key)
	{
		return DataSerializer.Load<T>(key);
	}

	public static T Load<T>(string key, T defaultValue)
	{
		return DataSerializer.Load(key, defaultValue);
	}

	public static bool HasKey(string key)
	{
		return DataSerializer.HasKey(key);
	}

	public static void DeleteKey(string key)
	{
		DataSerializer.DeleteKey(key);
	}

	public static void DeleteData()
	{
		DataSerializer.DeleteData();
		DataSerializer.DeleteAll();
	}
}
