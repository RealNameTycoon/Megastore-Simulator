using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IncomeBoostUI : MonoBehaviour
{
	[SerializeField]
	private Transform contentTarget;

	[SerializeField]
	private Transform content;

	[SerializeField]
	private GameObject remainingTimeUI;

	[SerializeField]
	private TextMeshProUGUI remainingTimeText;

	[SerializeField]
	private Button energyButton;

	[SerializeField]
	private Button rewardedButton;

	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	private DOTweenAnimation animation;

	[SerializeField]
	private Button upperUIRewardedButton;

	private Vector3 initialPosition;

	private const int ENERGY_COST = 10;

	private const int INCOME_BOOST_DURATION = 300;

	private const string INCOME_END_SEC_KEY = "INCOME_END_SEC";

	private int remainingTime = -1;

	private Coroutine closeRoutine;

	private WaitForSeconds waiter;

	private void Start()
	{
		EventManager.AddListener<int>(TutorialEvents.TUTORIAL_STEP_DONE, OnTutorialStepDone);
	}

	private void OnTutorialStepDone(int stepID)
	{
		if (stepID == 14)
		{
			Initialize();
		}
	}

	public float BoostMultiplier()
	{
		if (BoostActive())
		{
			return 1.2f;
		}
		return 1f;
	}

	private bool BoostActive()
	{
		return remainingTime > 0;
	}

	public void Initialize()
	{
		waiter = new WaitForSeconds(1f);
		initialPosition = content.position;
		energyButton.onClick.AddListener(OnEnergy);
		rewardedButton.onClick.AddListener(OnRewarded);
		if (GenericDataSerializer.HasKey("INCOME_END_SEC"))
		{
			remainingTime = GenericDataSerializer.LoadInt("INCOME_END_SEC") - OrderManager.EpochTime();
		}
		if (remainingTime > 0)
		{
			RepaintRemainingTime();
			remainingTimeUI.SetActive(value: true);
			StartCoroutine(BoostRoutine());
		}
	}

	private void AnimateUpperUIRewarded()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(upperUIRewardedButton.transform.DOScale(1.1f, 0.4f).SetDelay(2f));
		sequence.Append(upperUIRewardedButton.transform.DOScale(1f, 0.4f).OnComplete(delegate
		{
		}));
		sequence.SetLoops(-1, LoopType.Restart).Play();
	}

	private void OnEnergy()
	{
		if (SingletonBehaviour<EconomyManager>.Instance.HasEnoughHardCurrency(10))
		{
			EventLogger.LogEvent("c_income_energy");
			EventManager.NotifyEvent(EconomyEvents.REMOVE_HARD_CURRENCY, 10);
			BoostIncome();
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("not_enough_energy", base.transform);
		}
	}

	private void OnRewarded()
	{
		AdManager.Instance.ShowRewarded(delegate
		{
			EventLogger.LogEvent("c_rewarded_income");
			BoostIncome();
		});
	}

	private void BoostIncome()
	{
		GenericDataSerializer.SaveInt("INCOME_END_SEC", OrderManager.EpochTime() + 300);
		remainingTime = 300;
		RepaintRemainingTime();
		StartCoroutine(BoostRoutine());
		remainingTimeUI.SetActive(value: true);
		upperUIRewardedButton.gameObject.SetActive(value: false);
	}

	private IEnumerator BoostRoutine()
	{
		while (remainingTime > 0)
		{
			yield return new WaitForSeconds(1f);
			remainingTime--;
			RepaintRemainingTime();
			if (remainingTime == 0)
			{
				remainingTimeUI.SetActive(value: false);
			}
		}
	}

	private void RepaintRemainingTime()
	{
		int num = remainingTime / 60;
		int num2 = remainingTime % 60;
		remainingTimeText.text = "+%20 | " + num.ToString("00") + ":" + num2.ToString("00");
	}
}
