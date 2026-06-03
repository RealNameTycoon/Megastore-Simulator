using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviewUpperUI : SingletonBehaviour<ReviewUpperUI>
{
	[SerializeField]
	private Canvas upperCanvas;

	[SerializeField]
	private Canvas upperCanvasReview;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private Canvas levelEndCanvas;

	[SerializeField]
	private CanvasGroup levelEndCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI congratsText;

	[SerializeField]
	private TextMeshProUGUI level_n_text;

	[SerializeField]
	private TextMeshProUGUI earnedText;

	[SerializeField]
	private Button nextLevelButton;

	private int spawnedCustomersInLevel;

	private int currentLevel;

	private const string currentLevelKey = "review_current_level";

	private bool canSpawn = true;

	private int servedCustomerCount;

	public void Initialize()
	{
		currentLevel = GenericDataSerializer.LoadInt("review_current_level", 1);
		levelText.text = "LEVEL " + currentLevel;
		upperCanvas.enabled = false;
		upperCanvasReview.enabled = true;
		EventManager.AddListener(ReviewEvents.PAYMENTS_FAILED_REVIEW, delegate
		{
			OnPaymentDone(success: false);
		});
		EventManager.AddListener(ReviewEvents.PAYMENTS_SUCCESS_REVIEW, delegate
		{
			OnPaymentDone(success: true);
		});
		nextLevelButton.onClick.AddListener(OnNextLevelClicked);
	}

	private void OnPaymentDone(bool success)
	{
		servedCustomerCount++;
		if (servedCustomerCount == GetCustomerCount())
		{
			OpenLevelEndScreen();
		}
	}

	public int GetCustomerCount()
	{
		return 3 + currentLevel / 2;
	}

	private void OpenLevelEndScreen()
	{
		StartCoroutine(OpenRoutine());
	}

	private IEnumerator OpenRoutine()
	{
		congratsText.transform.localScale = Vector3.zero;
		level_n_text.transform.localScale = Vector3.zero;
		earnedText.transform.localScale = Vector3.zero;
		nextLevelButton.gameObject.SetActive(value: false);
		level_n_text.text = "Level " + currentLevel + " finished";
		levelEndCanvasGroup.alpha = 0f;
		levelEndCanvas.enabled = true;
		levelEndCanvasGroup.DOFade(1f, 0.2f);
		congratsText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(0.25f);
		level_n_text.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(0.25f);
		earnedText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine).OnComplete(delegate
		{
			nextLevelButton.gameObject.SetActive(value: true);
		});
	}

	private void OnNextLevelClicked()
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TUTORIAL_STEP_DONE);
		levelEndCanvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			levelEndCanvas.enabled = false;
		});
		currentLevel++;
		levelText.text = "LEVEL " + currentLevel;
		GenericDataSerializer.SaveInt("review_current_level", currentLevel);
		canSpawn = true;
		spawnedCustomersInLevel = 0;
		servedCustomerCount = 0;
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, 20f);
		SingletonBehaviour<CustomerManager>.Instance.TrySpawnCustomer();
	}

	public bool IsLastCustomer()
	{
		return servedCustomerCount == GetCustomerCount();
	}

	public bool CanSpawnCustomer()
	{
		return canSpawn;
	}

	public void OnCustomerSpawned()
	{
		spawnedCustomersInLevel++;
		if (spawnedCustomersInLevel == GetCustomerCount())
		{
			canSpawn = false;
		}
	}
}
