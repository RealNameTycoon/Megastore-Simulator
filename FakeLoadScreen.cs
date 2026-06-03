using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FakeLoadScreen : SingletonBehaviour<FakeLoadScreen>
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Slider progressSlider;

	[SerializeField]
	private StartWindow startWindow;

	[SerializeField]
	private GameObject monitor;

	[SerializeField]
	private GameObject checkoutTable;

	[SerializeField]
	private GameObject reviewTable;

	public const string FIRST_TIME_KEY = "FIRST_TIME_KEY";

	private bool isInitialized;

	public static float FAKE_LOAD_DURATION = 1f;

	public bool IsInitialized => isInitialized;

	private new void Awake()
	{
		base.Awake();
		canvas.enabled = true;
	}

	private void Start()
	{
		progressSlider.DOValue(1f, FAKE_LOAD_DURATION).OnComplete(InitializeGame);
	}

	private void InitializeGame()
	{
		isInitialized = true;
		SingletonBehaviour<AudioManager>.Instance.Initialize();
		startWindow.Initialize();
		SingletonBehaviour<EmployeeManager>.Instance.Initialize();
		if (!GenericDataSerializer.HasKey("FIRST_TIME_KEY"))
		{
			GenericDataSerializer.Save("FIRST_TIME_KEY", dataToSave: true);
		}
		canvas.enabled = false;
		EventManager.NotifyEvent(UIEvents.FAKE_LOADING_FINISHED);
	}
}
