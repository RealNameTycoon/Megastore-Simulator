using UnityEngine;

public class SingletonWindow<T> : UIWindow where T : SingletonWindow<T>
{
	public static T Instance { get; protected set; }

	protected virtual void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Instance = (T)this;
		}
	}
}
