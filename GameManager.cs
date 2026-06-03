using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
	public static bool isDemo = false;

	public static bool isPlaytest = false;

	[SerializeField]
	private Camera fpsCamera;

	[SerializeField]
	private GameObject tppCamera;

	[SerializeField]
	private GameObject character;

	[SerializeField]
	private GameObject crossPromoSign;

	public bool isTpp;

	public const string SAVE_VERSION_KEY = "SAVE_VERSION_KEY";

	public static int COLD_STORAGE_VERSION = 3;

	public static int GetSaveVersion()
	{
		return GenericDataSerializer.LoadInt("SAVE_VERSION_KEY");
	}
}
