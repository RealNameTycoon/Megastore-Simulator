using System.Collections;
using UnityEngine;

public class ObjectiveManager : SingletonBehaviour<ObjectiveManager>
{
	[SerializeField]
	private TutorialWindow tutorialWindow;

	private string lastCompletedStepKey = "lastCompletedObjectiveKey";

	private int lastCompletedStep = -2;

	private const int SERVE_10_OBJECTIVE_ID = 0;

	public const int LICENSE_OBJECTIVE_ID = 1;

	public const int NEW_FURNITURE_OBJECTIVE_ID = 2;

	public const int BUY_GROWTH_OBJECTIVE_ID = 3;

	private const int NEEDED_CHECKOUTCOUNT_FOR_OBJECTIVE = 10;

	public int LastCompletedStep => lastCompletedStep;

	public void Initialize()
	{
		EventManager.AddListener<int>(TutorialEvents.OBJECTIVE_STEP_DONE, OnObjectiveStepCompleted);
		EventManager.AddListener<float>(PaymentEvents.CASH_PAYMENT_DONE, OnPaymentDone);
		EventManager.AddListener<float>(PaymentEvents.POS_PAYMENT_DONE, OnPaymentDone);
		EventManager.AddListener<int, ProductGroup>(ProductEvents.PRODUCT_LICENSE_PURCHASED, OnLicensePurchased);
		EventManager.AddListener<PlaceableType>(PlaceableEvents.NEW_PLACEABLE_PLACED, OnNewPlaceablePlaced);
		EventManager.AddListener<FurnitureType>(PlaceableEvents.NEW_FURNITURE_PLACED, OnNewFurniturePlaced);
		EventManager.AddListener<int>(SupermarketEvents.GROWTH_PURCHASED, OnGrowthPurchased);
		lastCompletedStep = GenericDataSerializer.LoadInt(lastCompletedStepKey, -1);
		StartCoroutine(InitializeRoutine());
	}

	private void Start()
	{
		if (SingletonBehaviour<TutorialManager>.Instance.TutorialDone())
		{
			Initialize();
		}
	}

	private void OnObjectiveStepCompleted(int objectiveStep)
	{
		lastCompletedStep = objectiveStep;
		GenericDataSerializer.SaveInt(lastCompletedStepKey, lastCompletedStep);
		EventLogger.LogEvent("c_objective_completed_" + objectiveStep);
		StartCoroutine(SwitchObjective());
	}

	private IEnumerator SwitchObjective()
	{
		SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(AudioManager.UIAudioTypes.TUTORIAL_STEP_DONE);
		DeactivateObjective(lastCompletedStep);
		yield return new WaitForSeconds(0.5f);
		ActivateObjective(lastCompletedStep + 1);
	}

	private IEnumerator InitializeRoutine()
	{
		yield return new WaitForEndOfFrame();
		tutorialWindow.SwitchToObjective();
		ActivateObjective(lastCompletedStep + 1);
	}

	private void DeactivateObjective(int objectiveStep)
	{
		if (objectiveStep == 3)
		{
			tutorialWindow.Close();
		}
	}

	private void OnPaymentDone(float payment)
	{
		if (IsObjectiveActive(0))
		{
			tutorialWindow.UpdateText("tutorial_perform_checkout10", SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount.ToString());
			if (SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount == 10)
			{
				EventManager.NotifyEvent(TutorialEvents.OBJECTIVE_STEP_DONE, 0);
			}
		}
	}

	private void OnLicensePurchased(int newLicense, ProductGroup group)
	{
		if (IsObjectiveActive(1))
		{
			EventManager.NotifyEvent(TutorialEvents.OBJECTIVE_STEP_DONE, 1);
		}
	}

	private void OnNewPlaceablePlaced(PlaceableType type)
	{
		if (IsObjectiveActive(2) && type != PlaceableType.PRODUCE_SHELF_SMALL)
		{
			EventManager.NotifyEvent(TutorialEvents.OBJECTIVE_STEP_DONE, 2);
		}
	}

	private void OnNewFurniturePlaced(FurnitureType type)
	{
		if (IsObjectiveActive(2))
		{
			EventManager.NotifyEvent(TutorialEvents.OBJECTIVE_STEP_DONE, 2);
		}
	}

	private void OnGrowthPurchased(int growthLevel)
	{
		if (IsObjectiveActive(3))
		{
			EventManager.NotifyEvent(TutorialEvents.OBJECTIVE_STEP_DONE, 3);
		}
	}

	public bool IsObjectiveActive(int objectiveID)
	{
		return objectiveID == lastCompletedStep + 1;
	}

	private void SkipObjective()
	{
		lastCompletedStep++;
		EventLogger.LogEvent("c_objective_skipped_" + lastCompletedStep);
		GenericDataSerializer.SaveInt(lastCompletedStepKey, lastCompletedStep);
		ActivateObjective(lastCompletedStep + 1);
	}

	private void ActivateObjective(int objectiveStep)
	{
		switch (objectiveStep)
		{
		case 0:
			if (SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount >= 10)
			{
				SkipObjective();
			}
			else
			{
				tutorialWindow.OpenWithKeyAndParam("tutorial_perform_checkout10", SingletonBehaviour<CheckoutDeskManager>.Instance.CheckedOutCustomerCount.ToString());
			}
			break;
		case 1:
			if (ProductLicenseManager.AnyLicensePurchased())
			{
				SkipObjective();
			}
			else
			{
				tutorialWindow.Open("objective_buy_license");
			}
			break;
		case 2:
			tutorialWindow.Open("objective_buy_furniture");
			break;
		case 3:
			tutorialWindow.Open("objective_buy_growth");
			break;
		}
	}
}
