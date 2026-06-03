using System;
using UnityEngine;

public class EconomyManager : SingletonBehaviour<EconomyManager>
{
	[SerializeField]
	public bool isTest;

	[SerializeField]
	private GameObject levelUpButton;

	private float softCurrency;

	private int hardCurrency;

	private string softCurrencyKey = "softCurrencyKey";

	private string hardCurrencyKey = "hardCurrencyKey";

	public float SoftCurrency => softCurrency;

	public int HardCurrency => hardCurrency;

	public void DebugAddSoftCurrency(float amount)
	{
		EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, amount);
	}

	public void DebugAddHardCurrency(int amount)
	{
		EventManager.NotifyEvent(EconomyEvents.ADD_HARD_CURRENCY, amount);
	}

	public void PrintSellingPrices()
	{
		for (int i = 0; i < 20; i++)
		{
			CalculateSellingPrice(0.5f * (float)i + 1f, 0);
		}
	}

	public static double CalculateSellingPrice(double buyingPrice, int licenseLevel)
	{
		double num = 1.5 * Math.Exp(-0.35 * buyingPrice);
		double num2 = buyingPrice * (1.0 + num);
		double num3 = (double)licenseLevel * 0.02;
		return Math.Round(num2 * (1.0 + num3), 2);
	}

	private new void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(this);
		EventManager.AddListener<float>(EconomyEvents.ADD_SOFT_CURRENCY, AddSoftCurrency);
		EventManager.AddListener<int>(EconomyEvents.ADD_HARD_CURRENCY, AddHardCurrency);
		EventManager.AddListener<float>(EconomyEvents.REMOVE_SOFT_CURRENCY, RemoveSoftCurrency);
		EventManager.AddListener<int>(EconomyEvents.REMOVE_HARD_CURRENCY, RemoveHardCurrency);
		if (isTest)
		{
			levelUpButton.gameObject.SetActive(isTest);
			softCurrency = GenericDataSerializer.LoadFloat(softCurrencyKey, 9999999f);
			hardCurrency = GenericDataSerializer.LoadInt(hardCurrencyKey, 99999);
		}
		else
		{
			levelUpButton.gameObject.SetActive(value: false);
			softCurrency = GenericDataSerializer.LoadFloat(softCurrencyKey, 400f);
			hardCurrency = GenericDataSerializer.LoadInt(hardCurrencyKey, 100);
		}
	}

	public bool HasEnoughSoftCurrency(float amount)
	{
		return amount <= softCurrency;
	}

	public bool HasEnoughHardCurrency(int amount)
	{
		return amount <= hardCurrency;
	}

	public void AddSoftCurrency(float amount)
	{
		if (GameManager.isPlaytest)
		{
			amount *= 5f;
		}
		softCurrency += amount;
		if (softCurrency <= 0f)
		{
			softCurrency = 0f;
		}
		SaveSoftCurrency();
		if (softCurrency >= 1000f)
		{
			EventLogger.ReachBalance1k();
		}
		if (softCurrency >= 5000f)
		{
			EventLogger.ReachBalance5k();
		}
		if (softCurrency >= 25000f)
		{
			EventLogger.ReachBalance25k();
		}
	}

	public void AddHardCurrency(int amount)
	{
		hardCurrency += amount;
		SaveHardCurrency();
	}

	public void RemoveSoftCurrency(float amount)
	{
		softCurrency -= amount;
		SaveSoftCurrency();
	}

	public void RemoveHardCurrency(int amount)
	{
		hardCurrency -= amount;
		SaveHardCurrency();
	}

	private void SaveSoftCurrency()
	{
		GenericDataSerializer.SaveFloat(softCurrencyKey, softCurrency);
	}

	private void SaveHardCurrency()
	{
		GenericDataSerializer.SaveInt(hardCurrencyKey, hardCurrency);
	}
}
