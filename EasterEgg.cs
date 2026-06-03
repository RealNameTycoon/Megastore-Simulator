using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class EasterEgg : MonoBehaviour
{
	[SerializeField]
	private Transform door;

	[SerializeField]
	private GameObject clickableMoney;

	[SerializeField]
	private TextMeshPro wallText;

	private Vector3 doorClosedEuler = Vector3.zero;

	private Vector3 doorOpenedEuler = new Vector3(0f, 100f, 0f);

	private const string EASTER_EGG_TAKEN_KEY = "EASTER_EGG_TAKEN";

	private bool isOpen;

	private bool easterEggTaken;

	private void Start()
	{
		easterEggTaken = GenericDataSerializer.Load("EASTER_EGG_TAKEN", defaultValue: false);
		if (easterEggTaken)
		{
			clickableMoney.SetActive(value: false);
		}
		if (easterEggTaken)
		{
			wallText.text = Locale.GetWord("easter_egg_stop");
		}
		else
		{
			wallText.text = Locale.GetWord("easter_egg_description");
		}
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		if (easterEggTaken)
		{
			wallText.text = Locale.GetWord("easter_egg_stop");
		}
		else
		{
			wallText.text = Locale.GetWord("easter_egg_description");
		}
	}

	public void OnDoorClicked()
	{
		if (isOpen)
		{
			door.DOKill();
			door.DOLocalRotate(doorClosedEuler, 180f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
		}
		else
		{
			door.DOKill();
			door.DOLocalRotate(doorOpenedEuler, 180f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear);
		}
		isOpen = !isOpen;
	}

	public void OnMoneyClicked()
	{
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, 20f);
		GenericDataSerializer.Save("EASTER_EGG_TAKEN", dataToSave: true);
		clickableMoney.SetActive(value: false);
	}

	private void Update()
	{
	}
}
