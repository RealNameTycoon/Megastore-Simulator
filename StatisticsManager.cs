using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DFTGames.Localization;
using UnityEngine;

public class StatisticsManager : SingletonBehaviour<StatisticsManager>
{
	private struct DailyStats
	{
		public int day;

		public int satisfiedCustomers;

		public int notFoundProducts;

		public int foundExpensiveProducts;

		public int overpaidCustomers;

		public int totalCustomers;

		public int totalSoldProducts;

		public Dictionary<ProductType, int> soldItemsDictionary;

		public float salesProfit;

		public float totalSales;

		public float supplyCost;

		public float upgradeCost;

		public float hiringCost;

		public float loanPayments;

		public float loanTaken;

		public float balance;
	}

	[SerializeField]
	private StatisticsWindow statisticsWindow;

	[SerializeField]
	private ProductStatsWindow productStatsWindow;

	private const string SATISFIED_CUSTOMERS_KEY = "SATISFIED_CUSTOMERS_KEY";

	private const string NOT_FOUND_PRODUCTS_KEY = "NOT_FOUND_PRODUCTS_KEY";

	private const string FOUND_EXPENSIVE_PRODUCTS_KEY = "FOUND_EXPENSIVE_PRODUCTS_KEY";

	private const string OVERPAID_CUSTOMERS_KEY = "OVERPAID_CUSTOMERS_KEY";

	private const string TOTAL_CUSTOMERS_KEY = "TOTAL_CUSTOMERS_KEY";

	private const string TOTAL_SOLD_PRODUCTS_KEY = "TOTAL_SOLD_PRODUCTS_KEY";

	private const string SOLD_ITEMS_DICTIONARY_KEY = "SOLD_ITEMS_DICTIONARY_KEY";

	private const string SALES_PROFIT_KEY = "SALES_PROFIT_KEY";

	private const string TOTAL_SALES_KEY = "TOTAL_SALES_KEY";

	private const string SUPPLY_COST_KEY = "SUPPLY_COST_KEY";

	private const string UPGRADE_COST_KEY = "UPGRADE_COST_KEY";

	private const string HIRING_COST_KEY = "HIRING_COST_KEY";

	private const string DAILY_STATS_KEY = "DAILY_STATS_KEY";

	private const string LOAN_PAYMENTS_KEY = "LOAN_PAYMENTS_KEY";

	private const string LOAN_TAKEN_KEY = "LOAN_TAKEN_KEY";

	private List<DailyStats> dailyStatsList = new List<DailyStats>();

	private const int MAX_TOP_SOLD_PRODUCTS_COUNT = 10;

	private const int MaxDailyStatsCount = 60;

	private int satisfiedCustomers;

	private int notFoundProducts;

	private int foundExpensiveProducts;

	private int overpaidCustomers;

	private int totalCustomers;

	private int totalSoldProducts;

	private float salesProfit;

	private float totalSales;

	private float supplyCost;

	private float upgradeCost;

	private float hiringCost;

	private float loanPayments;

	private float loanTaken;

	private Dictionary<ProductType, int> soldItemsDictionary = new Dictionary<ProductType, int>();

	private Dictionary<ProductType, int> soldItemsDictionaryLastDay = new Dictionary<ProductType, int>();

	private Dictionary<ProductType, int> soldItemsDictionary7D = new Dictionary<ProductType, int>();

	private Dictionary<ProductType, int> soldItemsDictionary30D = new Dictionary<ProductType, int>();

	public int SatisfiedCustomers => satisfiedCustomers;

	public int StoreExperience => (SatisfiedCustomers + overpaidCustomers) * 5;

	public int NotFoundProducts => notFoundProducts;

	public int FoundExpensiveProducts => foundExpensiveProducts;

	public int OverpaidCustomers => overpaidCustomers;

	public int TotalCustomers => totalCustomers;

	public int TotalSoldProducts => totalSoldProducts;

	public float AveragePurchase => totalSales / (float)(satisfiedCustomers + overpaidCustomers);

	public float SalesProfit => salesProfit;

	public float TotalSales => totalSales;

	public float Revenue => totalSales;

	public float SupplyCost => supplyCost;

	public float UpgradeCost => upgradeCost;

	public float LoanPayments => loanPayments;

	public float LoanTaken => loanTaken;

	public float HiringCost => hiringCost;

	public float TotalProfit => totalSales - supplyCost - upgradeCost - loanPayments;

	public float Balance => SingletonBehaviour<EconomyManager>.Instance.SoftCurrency;

	public Dictionary<ProductType, int> SoldItemsDictionaryLastDay => soldItemsDictionaryLastDay;

	public Dictionary<ProductType, int> SoldItemsDictionary7D => soldItemsDictionary7D;

	public Dictionary<ProductType, int> SoldItemsDictionary30D => soldItemsDictionary30D;

	public ProductType MostSoldProduct
	{
		get
		{
			if (soldItemsDictionary.Count == 0)
			{
				return ProductType.NONE;
			}
			ProductType result = ProductType.NONE;
			int num = 0;
			foreach (KeyValuePair<ProductType, int> item in soldItemsDictionary)
			{
				if (item.Value > num)
				{
					result = item.Key;
					num = item.Value;
				}
			}
			return result;
		}
	}

	public int GetOneDaySoldAmount(ProductType type)
	{
		if (soldItemsDictionaryLastDay.ContainsKey(type))
		{
			return soldItemsDictionaryLastDay[type];
		}
		return 0;
	}

	public int Get7DaysSoldAmount(ProductType type)
	{
		if (soldItemsDictionary7D.ContainsKey(type))
		{
			return soldItemsDictionary7D[type];
		}
		return 0;
	}

	public int Get30DaysSoldAmount(ProductType type)
	{
		if (soldItemsDictionary30D.ContainsKey(type))
		{
			return soldItemsDictionary30D[type];
		}
		return 0;
	}

	private new void Awake()
	{
		base.Awake();
		satisfiedCustomers = GenericDataSerializer.LoadInt("SATISFIED_CUSTOMERS_KEY");
		notFoundProducts = GenericDataSerializer.LoadInt("NOT_FOUND_PRODUCTS_KEY");
		foundExpensiveProducts = GenericDataSerializer.LoadInt("FOUND_EXPENSIVE_PRODUCTS_KEY");
		overpaidCustomers = GenericDataSerializer.LoadInt("OVERPAID_CUSTOMERS_KEY");
		totalCustomers = GenericDataSerializer.LoadInt("TOTAL_CUSTOMERS_KEY");
		totalSoldProducts = GenericDataSerializer.LoadInt("TOTAL_SOLD_PRODUCTS_KEY");
		salesProfit = GenericDataSerializer.LoadFloat("SALES_PROFIT_KEY");
		totalSales = GenericDataSerializer.LoadFloat("TOTAL_SALES_KEY");
		supplyCost = GenericDataSerializer.LoadFloat("SUPPLY_COST_KEY");
		upgradeCost = GenericDataSerializer.LoadFloat("UPGRADE_COST_KEY");
		hiringCost = GenericDataSerializer.LoadFloat("HIRING_COST_KEY");
		loanPayments = GenericDataSerializer.LoadFloat("LOAN_PAYMENTS_KEY");
		loanTaken = GenericDataSerializer.LoadFloat("LOAN_TAKEN_KEY");
		dailyStatsList = GenericDataSerializer.Load("DAILY_STATS_KEY", new List<DailyStats>());
		soldItemsDictionary = GenericDataSerializer.Load("SOLD_ITEMS_DICTIONARY_KEY", new Dictionary<ProductType, int>());
		EventManager.AddListener(StatisticsEvents.CUSTOMER_SATISFIED, OnCustomerSatisfied);
		EventManager.AddListener(StatisticsEvents.VENDING_CUSTOMER_SATISFIED, OnCustomerSatisfied);
		EventManager.AddListener(StatisticsEvents.PRODUCT_NOT_FOUND, OnProductNotFound);
		EventManager.AddListener(StatisticsEvents.PRODUCT_FOUND_EXPENSIVE, OnProductFoundExpensive);
		EventManager.AddListener(StatisticsEvents.EXTRA_CHANGE_GIVEN, OnExtraChangeGiven);
		EventManager.AddListener(StatisticsEvents.CUSTOMER_SPAWNED, OnCustomerSpawned);
		EventManager.AddListener(StatisticsEvents.VENDING_CUSTOMER_SPAWNED, OnCustomerSpawned);
		EventManager.AddListener<List<Product>>(StatisticsEvents.VENDING_PRODUCTS_SOLD, OnProductsSold);
		EventManager.AddListener<List<Product>>(StatisticsEvents.PRODUCTS_SOLD, OnProductsSold);
		EventManager.AddListener<float>(StatisticsEvents.PROFIT_MADE, OnProfitMade);
		EventManager.AddListener<float>(StatisticsEvents.VENDING_PROFIT_MADE, OnProfitMade);
		EventManager.AddListener<float>(PaymentEvents.POS_PAYMENT_DONE, OnPaymentFinished);
		EventManager.AddListener<float>(PaymentEvents.VENDING_PAYMENT_DONE, OnPaymentFinished);
		EventManager.AddListener<float>(PaymentEvents.CASH_PAYMENT_DONE, OnPaymentFinished);
		EventManager.AddListener<float>(PaymentEvents.CART_PUCHASED, OnCartPurchased);
		EventManager.AddListener<float>(StatisticsEvents.UPGRADE_COST, OnUpgradePurchase);
		EventManager.AddListener<float>(StatisticsEvents.HIRING_COST, OnHireCost);
		EventManager.AddListener<float>(PaymentEvents.LOAN_PAYMENT_DONE, OnLoanPaymentFinished);
		EventManager.AddListener<float>(PaymentEvents.LOAN_TAKEN, OnLoanTaken);
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
		UpdateSoldItemsDictionaries();
	}

	private void UpdateSoldItemsDictionaries()
	{
		soldItemsDictionaryLastDay.Clear();
		soldItemsDictionary7D.Clear();
		soldItemsDictionary30D.Clear();
		int num = dailyStatsList.Count - 1;
		while (num >= 0 && num >= dailyStatsList.Count - 30)
		{
			_ = dailyStatsList[num];
			foreach (KeyValuePair<ProductType, int> item in dailyStatsList[num].soldItemsDictionary)
			{
				if (num == dailyStatsList.Count - 1)
				{
					if (soldItemsDictionaryLastDay.ContainsKey(item.Key))
					{
						soldItemsDictionaryLastDay[item.Key] += item.Value;
					}
					else
					{
						soldItemsDictionaryLastDay.Add(item.Key, item.Value);
					}
				}
				if (num >= dailyStatsList.Count - 7)
				{
					if (soldItemsDictionary7D.ContainsKey(item.Key))
					{
						soldItemsDictionary7D[item.Key] += item.Value;
					}
					else
					{
						soldItemsDictionary7D.Add(item.Key, item.Value);
					}
				}
				if (num >= dailyStatsList.Count - 30)
				{
					if (soldItemsDictionary30D.ContainsKey(item.Key))
					{
						soldItemsDictionary30D[item.Key] += item.Value;
					}
					else
					{
						soldItemsDictionary30D.Add(item.Key, item.Value);
					}
				}
			}
			num--;
		}
	}

	private void OnLoanPaymentFinished(float amount)
	{
		loanPayments += amount;
		GenericDataSerializer.SaveFloat("LOAN_PAYMENTS_KEY", loanPayments);
	}

	private void OnLoanTaken(float amount)
	{
		loanTaken += amount;
		GenericDataSerializer.SaveFloat("LOAN_TAKEN_KEY", loanTaken);
	}

	private void OnCustomerSatisfied()
	{
		satisfiedCustomers++;
		GenericDataSerializer.SaveInt("SATISFIED_CUSTOMERS_KEY", satisfiedCustomers);
	}

	private void OnProductNotFound()
	{
		notFoundProducts++;
		GenericDataSerializer.SaveInt("NOT_FOUND_PRODUCTS_KEY", notFoundProducts);
	}

	private void OnProductFoundExpensive()
	{
		foundExpensiveProducts++;
		GenericDataSerializer.SaveInt("FOUND_EXPENSIVE_PRODUCTS_KEY", foundExpensiveProducts);
	}

	private void OnExtraChangeGiven()
	{
		overpaidCustomers++;
		GenericDataSerializer.SaveInt("OVERPAID_CUSTOMERS_KEY", overpaidCustomers);
	}

	private void OnCustomerSpawned()
	{
		totalCustomers++;
		GenericDataSerializer.SaveInt("TOTAL_CUSTOMERS_KEY", totalCustomers);
	}

	private void OnProductsSold(List<Product> products)
	{
		totalSoldProducts += products.Count;
		GenericDataSerializer.SaveInt("TOTAL_SOLD_PRODUCTS_KEY", totalSoldProducts);
		for (int i = 0; i < products.Count; i++)
		{
			Product product = products[i];
			ProductType productType = product.Data.type;
			int num = 1;
			if (productType == ProductType.COOKABLE_PACK)
			{
				productType = (product as CookablePackageProduct).ContainedType;
			}
			if (productType == ProductType.FOOD_TRAY)
			{
				FoodTrayProduct obj = product as FoodTrayProduct;
				productType = obj.ContainedType;
				num = obj.ProductCount;
			}
			if (soldItemsDictionary.ContainsKey(productType))
			{
				soldItemsDictionary[productType] += num;
			}
			else
			{
				soldItemsDictionary.Add(productType, num);
			}
		}
		GenericDataSerializer.Save("SOLD_ITEMS_DICTIONARY_KEY", soldItemsDictionary);
	}

	private void OnProfitMade(float amount)
	{
		salesProfit += amount;
		GenericDataSerializer.SaveFloat("SALES_PROFIT_KEY", salesProfit);
	}

	private void OnPaymentFinished(float amount)
	{
		totalSales += amount;
		GenericDataSerializer.SaveFloat("TOTAL_SALES_KEY", totalSales);
	}

	private void OnCartPurchased(float amount)
	{
		supplyCost += amount;
		GenericDataSerializer.SaveFloat("SUPPLY_COST_KEY", supplyCost);
	}

	private void OnUpgradePurchase(float amount)
	{
		upgradeCost += amount;
		GenericDataSerializer.SaveFloat("UPGRADE_COST_KEY", upgradeCost);
	}

	private void OnHireCost(float amount)
	{
		hiringCost += amount;
		GenericDataSerializer.SaveFloat("HIRING_COST_KEY", hiringCost);
	}

	private void OnNewDayStarted()
	{
		DailyStats item = new DailyStats
		{
			day = SingletonBehaviour<TimeManager>.Instance.CurrentDay - 1,
			satisfiedCustomers = satisfiedCustomers,
			notFoundProducts = notFoundProducts,
			foundExpensiveProducts = foundExpensiveProducts,
			overpaidCustomers = overpaidCustomers,
			totalCustomers = totalCustomers,
			totalSoldProducts = totalSoldProducts,
			salesProfit = salesProfit,
			totalSales = totalSales,
			supplyCost = supplyCost,
			upgradeCost = upgradeCost,
			hiringCost = hiringCost,
			loanPayments = loanPayments,
			loanTaken = loanTaken,
			balance = SingletonBehaviour<EconomyManager>.Instance.SoftCurrency,
			soldItemsDictionary = new Dictionary<ProductType, int>(soldItemsDictionary)
		};
		dailyStatsList.Add(item);
		while (dailyStatsList.Count > 60)
		{
			dailyStatsList.RemoveAt(0);
		}
		GenericDataSerializer.Save("DAILY_STATS_KEY", dailyStatsList);
		satisfiedCustomers = 0;
		notFoundProducts = 0;
		foundExpensiveProducts = 0;
		overpaidCustomers = 0;
		totalCustomers = 0;
		totalSoldProducts = 0;
		salesProfit = 0f;
		totalSales = 0f;
		supplyCost = 0f;
		upgradeCost = 0f;
		hiringCost = 0f;
		loanPayments = 0f;
		loanTaken = 0f;
		soldItemsDictionary.Clear();
		GenericDataSerializer.SaveInt("SATISFIED_CUSTOMERS_KEY", satisfiedCustomers);
		GenericDataSerializer.SaveInt("NOT_FOUND_PRODUCTS_KEY", notFoundProducts);
		GenericDataSerializer.SaveInt("FOUND_EXPENSIVE_PRODUCTS_KEY", foundExpensiveProducts);
		GenericDataSerializer.SaveInt("OVERPAID_CUSTOMERS_KEY", overpaidCustomers);
		GenericDataSerializer.SaveInt("TOTAL_CUSTOMERS_KEY", totalCustomers);
		GenericDataSerializer.SaveInt("TOTAL_SOLD_PRODUCTS_KEY", totalSoldProducts);
		GenericDataSerializer.SaveFloat("SALES_PROFIT_KEY", salesProfit);
		GenericDataSerializer.SaveFloat("TOTAL_SALES_KEY", totalSales);
		GenericDataSerializer.SaveFloat("SUPPLY_COST_KEY", supplyCost);
		GenericDataSerializer.SaveFloat("UPGRADE_COST_KEY", upgradeCost);
		GenericDataSerializer.SaveFloat("HIRING_COST_KEY", hiringCost);
		GenericDataSerializer.SaveFloat("LOAN_PAYMENTS_KEY", loanPayments);
		GenericDataSerializer.SaveFloat("LOAN_TAKEN_KEY", loanTaken);
		GenericDataSerializer.Save("SOLD_ITEMS_DICTIONARY_KEY", soldItemsDictionary);
		UpdateSoldItemsDictionaries();
		statisticsWindow.OnNewDayStarted();
		productStatsWindow.OnNewDayStarted();
	}

	public float GetAverageProfit(int lastDaysCount)
	{
		if (dailyStatsList.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		int num2 = Mathf.Min(lastDaysCount, dailyStatsList.Count);
		for (int i = 0; i < num2; i++)
		{
			int num3 = dailyStatsList.Count - i - 1;
			if (num3 < 0)
			{
				break;
			}
			num += dailyStatsList[num3].salesProfit - dailyStatsList[num3].hiringCost - SingletonBehaviour<FinanceManager>.Instance.DailyPayment();
		}
		return num / (float)lastDaysCount;
	}

	private void Update()
	{
	}

	public LineChartUI.ChartData GetChartData(ProductType type, int lastDaysCount)
	{
		LineChartUI.ChartData result = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		for (int i = 0; i < lastDaysCount; i++)
		{
			int num = dailyStatsList.Count - lastDaysCount + i;
			if (num >= 0)
			{
				int num2 = SingletonBehaviour<TimeManager>.Instance.CurrentDay - lastDaysCount + i;
				int num3 = (dailyStatsList[num].soldItemsDictionary.ContainsKey(type) ? dailyStatsList[num].soldItemsDictionary[type] : 0);
				list.Add(new LineChartUI.ChartEntry
				{
					axisIndexLabelString = num2.ToString(),
					value = num3
				});
			}
		}
		result.title = Locale.GetWord("product_sales_chart_title_n").Replace("{0}", Locale.GetWord(type.ToString()));
		result.entries = list;
		result.xAxisName = Locale.GetWord("day_axisname");
		result.yAxisName = Locale.GetWord("units_sold");
		return result;
	}

	public LineChartUI.ChartData GetDailySalesChartData(int timeFrame)
	{
		LineChartUI.ChartData result = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		for (int i = 0; i < timeFrame; i++)
		{
			int num = dailyStatsList.Count - timeFrame + i;
			if (num >= 0)
			{
				int num2 = SingletonBehaviour<TimeManager>.Instance.CurrentDay - timeFrame + i;
				list.Add(new LineChartUI.ChartEntry
				{
					axisIndexLabelString = num2.ToString(),
					value = dailyStatsList[num].totalSales
				});
			}
		}
		result.entries = list;
		result.xAxisName = GetDayAxisName();
		return result;
	}

	private string GetDayAxisName()
	{
		string word = Locale.GetWord("axisname_day");
		int num = 3;
		if (string.IsNullOrEmpty(word))
		{
			return "";
		}
		if (word.Length <= num)
		{
			return word;
		}
		return word.Substring(0, num) + ".";
	}

	public LineChartUI.ChartData GetDailySoldProductsChartData(int timeFrame)
	{
		LineChartUI.ChartData result = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		for (int i = 0; i < timeFrame; i++)
		{
			int num = dailyStatsList.Count - timeFrame + i;
			if (num >= 0)
			{
				int num2 = SingletonBehaviour<TimeManager>.Instance.CurrentDay - timeFrame + i;
				list.Add(new LineChartUI.ChartEntry
				{
					axisIndexLabelString = num2.ToString(),
					value = dailyStatsList[num].totalSoldProducts
				});
			}
		}
		result.entries = list;
		result.xAxisName = GetDayAxisName();
		return result;
	}

	public LineChartUI.ChartData GetDailySalesByDepartmentChartData(int timeFrame)
	{
		LineChartUI.ChartData result = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		Dictionary<ProductGroup, int> dictionary = new Dictionary<ProductGroup, int>();
		for (int i = 0; i < timeFrame; i++)
		{
			int num = dailyStatsList.Count - timeFrame + i;
			if (num < 0)
			{
				continue;
			}
			foreach (KeyValuePair<ProductType, int> item in dailyStatsList[num].soldItemsDictionary)
			{
				ProductGroup productGroup = SingletonBehaviour<ProductPool>.Instance.GetProductData(item.Key).productGroup;
				if (dictionary.ContainsKey(productGroup))
				{
					dictionary[productGroup] += item.Value;
				}
				else
				{
					dictionary.Add(productGroup, item.Value);
				}
			}
		}
		foreach (KeyValuePair<ProductGroup, int> item2 in dictionary)
		{
			list.Add(new LineChartUI.ChartEntry("", Locale.GetWord(item2.Key.ToString().ToLower(CultureInfo.InvariantCulture)), item2.Value));
		}
		list.Sort((LineChartUI.ChartEntry a, LineChartUI.ChartEntry b) => b.value.CompareTo(a.value));
		list.Reverse();
		result.entries = list;
		return result;
	}

	public LineChartUI.ChartData GetDailyTopSoldProductsChartData(int timeFrame)
	{
		LineChartUI.ChartData result = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		Dictionary<ProductType, int> dictionary = new Dictionary<ProductType, int>();
		for (int i = 0; i < timeFrame; i++)
		{
			int num = dailyStatsList.Count - timeFrame + i;
			if (num < 0)
			{
				continue;
			}
			foreach (KeyValuePair<ProductType, int> item in dailyStatsList[num].soldItemsDictionary)
			{
				if (dictionary.ContainsKey(item.Key))
				{
					dictionary[item.Key] += item.Value;
				}
				else
				{
					dictionary.Add(item.Key, item.Value);
				}
			}
		}
		List<KeyValuePair<ProductType, int>> list2 = dictionary.OrderByDescending((KeyValuePair<ProductType, int> kvp) => kvp.Value).ToList();
		for (int num2 = 0; num2 < 10 && num2 < list2.Count; num2++)
		{
			list.Add(new LineChartUI.ChartEntry("", Locale.GetWord(list2[num2].Key.ToString()), list2[num2].Value));
		}
		list.Reverse();
		result.entries = list;
		return result;
	}

	public LineChartUI.ChartData GetDailyAverageSpendPerCustomerChartData(int timeFrame)
	{
		LineChartUI.ChartData result = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		for (int i = 0; i < timeFrame; i++)
		{
			int num = dailyStatsList.Count - timeFrame + i;
			if (num >= 0)
			{
				int num2 = SingletonBehaviour<TimeManager>.Instance.CurrentDay - timeFrame + i;
				if (dailyStatsList[num].totalCustomers == 0)
				{
					list.Add(new LineChartUI.ChartEntry("", num2.ToString()));
				}
				else
				{
					list.Add(new LineChartUI.ChartEntry("", num2.ToString(), dailyStatsList[num].totalSales / (float)dailyStatsList[num].totalCustomers));
				}
			}
		}
		result.entries = list;
		result.xAxisName = GetDayAxisName();
		return result;
	}

	public (float, float) GetCurrentAndPreviousDayRevenue(int timeFrame)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < timeFrame; i++)
		{
			int num3 = dailyStatsList.Count - timeFrame + i;
			int num4 = num3 - timeFrame;
			if (num3 >= 0)
			{
				num += dailyStatsList[num3].totalSales;
				if (num4 >= 0)
				{
					num2 += dailyStatsList[num4].totalSales;
				}
			}
		}
		return (num, num2);
	}

	public (float, float) GetCurrentAndPreviousDayProfit(int timeFrame)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < timeFrame; i++)
		{
			int num3 = dailyStatsList.Count - timeFrame + i;
			int num4 = num3 - timeFrame;
			if (num3 >= 0)
			{
				num += dailyStatsList[num3].salesProfit;
				if (num4 >= 0)
				{
					num2 += dailyStatsList[num4].salesProfit;
				}
			}
		}
		return (num, num2);
	}

	public (int, int) GetCurrentAndPreviousDayCustomers(int timeFrame)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < timeFrame; i++)
		{
			int num3 = dailyStatsList.Count - timeFrame + i;
			int num4 = num3 - timeFrame;
			if (num3 >= 0)
			{
				num += dailyStatsList[num3].totalCustomers;
				if (num4 >= 0)
				{
					num2 += dailyStatsList[num4].totalCustomers;
				}
			}
		}
		return (num, num2);
	}

	public (int, int) GetCurrentAndPreviousDaySoldProducts(int timeFrame)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < timeFrame; i++)
		{
			int num3 = dailyStatsList.Count - timeFrame + i;
			int num4 = num3 - timeFrame;
			if (num3 >= 0)
			{
				num += dailyStatsList[num3].totalSoldProducts;
				if (num4 >= 0)
				{
					num2 += dailyStatsList[num4].totalSoldProducts;
				}
			}
		}
		return (num, num2);
	}

	public (LineChartUI.ChartData, LineChartUI.ChartData) GetDailyPreviousAndCurrentCustomerIssuesChartData(int timeFrame)
	{
		LineChartUI.ChartData item = default(LineChartUI.ChartData);
		LineChartUI.ChartData item2 = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		List<LineChartUI.ChartEntry> list2 = new List<LineChartUI.ChartEntry>();
		string word = Locale.GetWord("unavailable_products");
		string word2 = Locale.GetWord("found_expensive_products");
		string word3 = Locale.GetWord("overpaid_customers");
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		for (int i = 0; i < timeFrame; i++)
		{
			int num7 = dailyStatsList.Count - timeFrame + i;
			if (num7 >= 0)
			{
				num += dailyStatsList[num7].notFoundProducts;
				num2 += dailyStatsList[num7].foundExpensiveProducts;
				num3 += dailyStatsList[num7].overpaidCustomers;
				int num8 = num7 - timeFrame;
				if (num8 >= 0)
				{
					num4 += dailyStatsList[num8].notFoundProducts;
					num5 += dailyStatsList[num8].foundExpensiveProducts;
					num6 += dailyStatsList[num8].overpaidCustomers;
				}
			}
		}
		list.Add(new LineChartUI.ChartEntry("", word, num));
		list.Add(new LineChartUI.ChartEntry("", word2, num2));
		list.Add(new LineChartUI.ChartEntry("", word3, num3));
		list2.Add(new LineChartUI.ChartEntry("", word, num4));
		list2.Add(new LineChartUI.ChartEntry("", word2, num5));
		list2.Add(new LineChartUI.ChartEntry("", word3, num6));
		item.entries = list;
		item2.entries = list2;
		return (item, item2);
	}

	public (LineChartUI.ChartData, LineChartUI.ChartData) GetDailyRevenueVsExpensesChartData(int timeFrame)
	{
		LineChartUI.ChartData item = default(LineChartUI.ChartData);
		LineChartUI.ChartData item2 = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		List<LineChartUI.ChartEntry> list2 = new List<LineChartUI.ChartEntry>();
		for (int i = 0; i < timeFrame; i++)
		{
			int num = dailyStatsList.Count - timeFrame + i;
			if (num >= 0)
			{
				int num2 = SingletonBehaviour<TimeManager>.Instance.CurrentDay - timeFrame + i;
				list.Add(new LineChartUI.ChartEntry("", num2.ToString(), dailyStatsList[num].totalSales));
				list2.Add(new LineChartUI.ChartEntry("", num2.ToString(), dailyStatsList[num].supplyCost + dailyStatsList[num].upgradeCost + dailyStatsList[num].hiringCost + dailyStatsList[num].loanPayments));
			}
		}
		item.entries = list;
		item2.entries = list2;
		item.xAxisName = GetDayAxisName();
		item2.xAxisName = GetDayAxisName();
		return (item, item2);
	}

	public LineChartUI.ChartData GetDailyBalanceOverTimeChartData(int timeFrame)
	{
		LineChartUI.ChartData result = default(LineChartUI.ChartData);
		List<LineChartUI.ChartEntry> list = new List<LineChartUI.ChartEntry>();
		for (int i = 0; i < timeFrame; i++)
		{
			int num = dailyStatsList.Count - timeFrame + i;
			if (num >= 0)
			{
				list.Add(new LineChartUI.ChartEntry("", (SingletonBehaviour<TimeManager>.Instance.CurrentDay - timeFrame + i).ToString(), dailyStatsList[num].balance));
			}
		}
		result.entries = list;
		result.xAxisName = GetDayAxisName();
		return result;
	}
}
