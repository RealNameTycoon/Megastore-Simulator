using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomerManager : SingletonBehaviour<CustomerManager>
{
	[SerializeField]
	private List<Customer> customers;

	[SerializeField]
	private List<Customer> vendingCustomers;

	[SerializeField]
	private List<Customer> bazaarCustomers;

	[SerializeField]
	private List<Customer> customersReview;

	[SerializeField]
	private Transform startPosition;

	[SerializeField]
	private Transform endPosition;

	[SerializeField]
	private Transform startPosition2;

	[SerializeField]
	private Transform endPosition2;

	[SerializeField]
	private Transform poolTransform;

	[SerializeField]
	private Transform doorFront;

	[SerializeField]
	private TextMeshProUGUI customerCountText;

	[SerializeField]
	private List<string> customerNames;

	private bool customerShopping;

	private bool vendingCustomerShopping;

	private int shoppingCustomerCount;

	public Transform PoolTransform => poolTransform;

	private List<Customer> Customers => customers;

	private new void Awake()
	{
		base.Awake();
		EventManager.AddListener(StartupEvents.SPAWN_MANAGER_INITIALIZED, delegate
		{
			UpdateCustomerCountText();
		});
	}

	private void Start()
	{
		EventManager.AddListener(CustomerEvents.CUSTOMER_LEAVE_STARTED, delegate
		{
			int count = shoppingCustomerCount - 1;
			UpdateShoppingCustomerCount(count);
			UpdateCustomerCountText();
		});
		EventManager.AddListener(CustomerEvents.VENDING_CUSTOMER_LEAVE_STARTED, CheckLastCustomerOfDay);
		EventManager.AddListener(CustomerEvents.LEAVE_STARTED_COMPLAINER, delegate
		{
			int count = shoppingCustomerCount - 1;
			UpdateShoppingCustomerCount(count);
			UpdateCustomerCountText();
		});
		EventManager.AddListener(CustomerEvents.VENDING_LEAVE_STARTED_COMPLAINER, CheckLastCustomerOfDay);
		EventManager.AddListener(CustomerEvents.CUSTOMER_SHOP_OVER, delegate
		{
			TrySpawnCustomer();
		});
		EventManager.AddListener(CustomerEvents.CUSTOMER_SHOP_STARTED, delegate
		{
			int count = shoppingCustomerCount + 1;
			UpdateShoppingCustomerCount(count);
			customerShopping = true;
			UpdateCustomerCountText();
		});
		EventManager.AddListener(CustomerEvents.VENDING_CUSTOMER_SHOP_OVER, delegate
		{
			vendingCustomerShopping = false;
			TrySpawnVendingCustomer();
		});
		EventManager.AddListener(SupermarketEvents.FIRE_STARTED, delegate
		{
			customerShopping = false;
			vendingCustomerShopping = false;
		});
		EventManager.AddListener(SupermarketEvents.CUSTOMER_CAPACITY_CHANGED, delegate
		{
			UpdateCustomerCountText();
		});
		StartCoroutine(SpawnRoutine());
		StartCoroutine(SpawnVendingRoutine());
		EventManager.AddListener(CustomerEvents.SHOP_OPENED, OnShopOpened);
	}

	public void TestCustomerSpawn(int count)
	{
		for (int i = 0; i < count; i++)
		{
			Transform transform;
			Transform transform2;
			if (Random.Range(0, 2) == 0)
			{
				transform = startPosition;
				transform2 = endPosition;
			}
			else
			{
				transform = startPosition2;
				transform2 = endPosition2;
			}
			Customer randomAvailableCustomer = GetRandomAvailableCustomer();
			randomAvailableCustomer.transform.position = transform.position;
			randomAvailableCustomer.transform.rotation = transform.rotation;
			randomAvailableCustomer.Activate(transform2);
			customerShopping = true;
			Customer randomAvailableCustomer2 = GetRandomAvailableCustomer();
			randomAvailableCustomer2.transform.position = transform.position;
			randomAvailableCustomer2.transform.rotation = transform.rotation;
			randomAvailableCustomer2.Activate(transform2);
			customerShopping = true;
			randomAvailableCustomer.DeactivateReleaseAll();
			randomAvailableCustomer2.DeactivateReleaseAll();
		}
	}

	private void OnShopOpened()
	{
		if (ShoppingCustomerCount() == 0)
		{
			TrySpawnCustomer();
		}
	}

	private void UpdateShoppingCustomerCount(int count)
	{
		shoppingCustomerCount = count;
		if (shoppingCustomerCount == 0)
		{
			CheckLastCustomerOfDay();
		}
	}

	private void CheckLastCustomerOfDay()
	{
		if (shoppingCustomerCount == 0 && ShoppingVendingCustomerCount() == 0 && !SingletonBehaviour<TimeManager>.Instance.CanSpawnCustomer())
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
			SingletonBehaviour<TimeManager>.Instance.ShowNextDayUI();
		}
	}

	public int ShoppingCustomerCount()
	{
		return shoppingCustomerCount;
	}

	public int ShoppingVendingCustomerCount()
	{
		int num = 0;
		for (int i = 0; i < vendingCustomers.Count; i++)
		{
			if (vendingCustomers[i].gameObject.activeSelf && !vendingCustomers[i].IsLeaving)
			{
				num++;
			}
		}
		return num;
	}

	public int ShoppingBazaarCustomerCount()
	{
		int num = 0;
		for (int i = 0; i < bazaarCustomers.Count; i++)
		{
			if (bazaarCustomers[i].gameObject.activeSelf && !bazaarCustomers[i].IsLeaving)
			{
				num++;
			}
		}
		return num;
	}

	public void DeactivateAllCustomers()
	{
		for (int i = 0; i < Customers.Count; i++)
		{
			if (Customers[i].gameObject.activeSelf)
			{
				Customers[i].Deactivate();
			}
		}
	}

	private bool CanSpawn()
	{
		if (!SingletonBehaviour<StockManager>.Instance.CanSpawnCustomer() || SingletonBehaviour<FireManager>.Instance.IsFireActive)
		{
			return false;
		}
		if (GetActiveCustomerCount() >= SingletonBehaviour<CheckoutDeskManager>.Instance.GetCustomerCapacity())
		{
			return false;
		}
		if (!SingletonBehaviour<OpenCloseLabel>.Instance.IsOpen)
		{
			return false;
		}
		if (!SingletonBehaviour<TimeManager>.Instance.CanSpawnCustomer())
		{
			return false;
		}
		return true;
	}

	public int GetActiveCustomerCount()
	{
		int num = 0;
		for (int i = 0; i < Customers.Count; i++)
		{
			if (Customers[i].gameObject.activeSelf)
			{
				num++;
			}
		}
		return num;
	}

	private bool CanSpawnVendingCustomer()
	{
		if (vendingCustomerShopping || SingletonBehaviour<SpawnManager>.Instance.GetRandomAvailableVender() == null || SingletonBehaviour<FireManager>.Instance.IsFireActive)
		{
			return false;
		}
		if (!SingletonBehaviour<VendingStockManager>.Instance.CanSpawnCustomer())
		{
			return false;
		}
		if (!SingletonBehaviour<OpenCloseLabel>.Instance.IsOpen)
		{
			return false;
		}
		if (!SingletonBehaviour<TimeManager>.Instance.CanSpawnVendingCustomer())
		{
			return false;
		}
		return true;
	}

	private bool CanSpawnBazaarCustomer()
	{
		if (!SingletonBehaviour<TimeManager>.Instance.CanSpawnCustomer())
		{
			return false;
		}
		if (!SingletonBehaviour<MerchantCheckoutManager>.Instance.HasProducts())
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < bazaarCustomers.Count; i++)
		{
			if (bazaarCustomers[i].gameObject.activeSelf)
			{
				num++;
			}
		}
		if (num == 4)
		{
			return false;
		}
		return true;
	}

	public Customer GetRandomAvailableCustomer()
	{
		List<Customer> list = new List<Customer>();
		for (int i = 0; i < Customers.Count; i++)
		{
			if (!Customers[i].gameObject.activeSelf)
			{
				list.Add(Customers[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	private Customer GetRandomAvailableVendingCustomer()
	{
		List<Customer> list = new List<Customer>();
		for (int i = 0; i < vendingCustomers.Count; i++)
		{
			if (!vendingCustomers[i].gameObject.activeSelf)
			{
				list.Add(vendingCustomers[i]);
			}
		}
		return list.GetRandomElement();
	}

	private Customer GetRandomAvailableBazaarCustomer()
	{
		List<Customer> list = new List<Customer>();
		for (int i = 0; i < bazaarCustomers.Count; i++)
		{
			if (!bazaarCustomers[i].gameObject.activeSelf)
			{
				list.Add(bazaarCustomers[i]);
			}
		}
		return list.GetRandomElement();
	}

	public void TrySpawnCustomer()
	{
		if (!CanSpawn())
		{
			return;
		}
		if (SingletonBehaviour<CarCustomerManager>.Instance.HasWaitingCar())
		{
			SingletonBehaviour<CarCustomerManager>.Instance.TrySpawnWaitingCar();
			return;
		}
		int currentLevel = SingletonBehaviour<ExperienceManager>.Instance.CurrentLevel;
		int num = ShoppingCustomerCount();
		if (currentLevel > 2 && num != 0 && num != 0 && Random.Range(0, 100) < currentLevel switch
		{
			3 => 10, 
			4 => 20, 
			5 => 30, 
			6 => 40, 
			_ => 50, 
		})
		{
			SingletonBehaviour<CarCustomerManager>.Instance.SpawnCarCustomer();
			return;
		}
		Transform transform;
		Transform transform2;
		if (Random.Range(0, 2) == 0)
		{
			transform = startPosition;
			transform2 = endPosition;
		}
		else
		{
			transform = startPosition2;
			transform2 = endPosition2;
		}
		Customer randomAvailableCustomer = GetRandomAvailableCustomer();
		if (!(randomAvailableCustomer == null))
		{
			randomAvailableCustomer.transform.position = transform.position;
			randomAvailableCustomer.transform.rotation = transform.rotation;
			EventManager.NotifyEvent(StatisticsEvents.CUSTOMER_SPAWNED);
			randomAvailableCustomer.Activate(transform2);
			customerShopping = true;
		}
	}

	public bool TrySpawnCarCustomer(CustomerCar car)
	{
		if (CanSpawn())
		{
			Transform driverDoorFront = car.DriverDoorFront;
			Customer randomAvailableCustomer = GetRandomAvailableCustomer();
			if (randomAvailableCustomer == null)
			{
				return false;
			}
			randomAvailableCustomer.transform.position = driverDoorFront.position;
			randomAvailableCustomer.transform.rotation = driverDoorFront.rotation;
			EventManager.NotifyEvent(StatisticsEvents.CUSTOMER_SPAWNED);
			randomAvailableCustomer.ActivateWithCar(car.DriverDoorFront, car);
			customerShopping = true;
			return true;
		}
		return false;
	}

	public void TrySpawnVendingCustomer()
	{
		if (CanSpawnVendingCustomer())
		{
			Transform transform;
			Transform transform2;
			if (Random.Range(0, 2) == 0)
			{
				transform = startPosition;
				transform2 = endPosition;
			}
			else
			{
				transform = startPosition2;
				transform2 = endPosition2;
			}
			Customer randomAvailableVendingCustomer = GetRandomAvailableVendingCustomer();
			randomAvailableVendingCustomer.transform.position = transform.position;
			randomAvailableVendingCustomer.transform.rotation = transform.rotation;
			randomAvailableVendingCustomer.Activate(transform2);
			EventManager.NotifyEvent(StatisticsEvents.VENDING_CUSTOMER_SPAWNED);
			vendingCustomerShopping = true;
		}
	}

	private IEnumerator SpawnVendingRoutine()
	{
		while (true)
		{
			float customerCountMultiplier = SingletonBehaviour<VendingStockManager>.Instance.GetCustomerCountMultiplier();
			yield return new WaitForSeconds(Random.Range(3f / customerCountMultiplier, 6f / customerCountMultiplier));
			TrySpawnVendingCustomer();
		}
	}

	private IEnumerator SpawnRoutine()
	{
		while (true)
		{
			float customerCountMultiplier = SingletonBehaviour<StockManager>.Instance.GetCustomerCountMultiplier();
			yield return new WaitForSeconds(Random.Range(3f / customerCountMultiplier, 6f / customerCountMultiplier));
			TrySpawnCustomer();
		}
	}

	private void UpdateCustomerCountText()
	{
		int customerCapacity = SingletonBehaviour<CheckoutDeskManager>.Instance.GetCustomerCapacity();
		if (shoppingCustomerCount == customerCapacity)
		{
			customerCountText.color = UIManager.RedColor;
		}
		else
		{
			customerCountText.color = UIManager.WhiteColor;
		}
		customerCapacity = Mathf.Min(customers.Count, customerCapacity);
		customerCountText.text = shoppingCustomerCount + " / " + customerCapacity;
	}
}
