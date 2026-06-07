using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class StockManager : SingletonBehaviour<StockManager>
{
	[Serializable]
	public class ShelfDictionary : UnitySerializedDictionary<ProductType, ShelvesList>
	{
	}

	[Serializable]
	public class ShelvesList
	{
		public List<Shelf> shelves;
	}

	[SerializeField]
	private ShelfDictionary shelfDictionary;

	[SerializeField]
	private ShelfDictionary availableShelfDictionary;

	[SerializeField]
	private SerializedDictionary<ProductType, int> stockDictionary = new SerializedDictionary<ProductType, int>();

	[SerializeField]
	private SerializedDictionary<ProductType, int> availableStockDictionary;

	[SerializeField]
	private SerializedDictionary<ProductType, int> boxStockDictionary;

	[SerializeField]
	private SerializedDictionary<ProductType, float> scoreDictionary = new SerializedDictionary<ProductType, float>();

	[SerializeField]
	private SerializedDictionary<ProductType, float> availableScoreDictionary = new SerializedDictionary<ProductType, float>();

	[SerializeField]
	private float totalScore;

	[SerializeField]
	private float availableTotalScore;

	[SerializeField]
	private List<ProductType> outOfStockProducts;

	[SerializeField]
	private List<ProductType> purchasableProducts;

	private const float BASE_POPULARITY = 0.001f;

	private const float MIN_BUYING_CHANCE = 0f;

	public const float BASE_SCORE_FOR_COMPLAINT = 0.3f;

	private const float PURCHASE_SCORE_AT_MARKET_PRICE = 0.5f;

	private static float testTotalProfit = 0f;

	private static Dictionary<ProductGroup, float> priceElasticityForDepartments = new Dictionary<ProductGroup, float>
	{
		{
			ProductGroup.GROCERY,
			2.7f
		},
		{
			ProductGroup.TOY,
			2.4f
		},
		{
			ProductGroup.CLOTHING,
			2.6f
		},
		{
			ProductGroup.BAKERY,
			2.5f
		},
		{
			ProductGroup.SPORTS,
			3.2f
		},
		{
			ProductGroup.MUSIC,
			4f
		},
		{
			ProductGroup.ELECTRONICS,
			5.3f
		},
		{
			ProductGroup.VENDING,
			2.7f
		},
		{
			ProductGroup.FISH,
			2.7f
		},
		{
			ProductGroup.BEACH,
			2.7f
		}
	};

	private static Dictionary<ProductGroup, float> priceAsymmetryForDepartments = new Dictionary<ProductGroup, float>
	{
		{
			ProductGroup.GROCERY,
			1.3f
		},
		{
			ProductGroup.TOY,
			1.2f
		},
		{
			ProductGroup.CLOTHING,
			1.25f
		},
		{
			ProductGroup.BAKERY,
			1.3f
		},
		{
			ProductGroup.SPORTS,
			1.3f
		},
		{
			ProductGroup.MUSIC,
			1.6f
		},
		{
			ProductGroup.ELECTRONICS,
			1.9f
		},
		{
			ProductGroup.VENDING,
			1.3f
		},
		{
			ProductGroup.FISH,
			1.3f
		},
		{
			ProductGroup.BEACH,
			1.3f
		}
	};

	public List<ProductType> PurchasableProducts => purchasableProducts;

	public void Initialize()
	{
		MonoBehaviour.print("Initializing Stock Manager");
		EventManager.AddListener<Shelf, ProductType>(PlaceableEvents.PRODUCT_ADDED, OnProductAdded);
		EventManager.AddListener<Shelf, ProductType>(PlaceableEvents.PRODUCT_REMOVED, OnProductRemoved);
		EventManager.AddListener<int, ProductGroup>(ProductEvents.PRODUCT_LICENSE_PURCHASED, OnLicensePurchased);
		EventManager.AddListener<ProductType, float>(ProductEvents.PRODUCT_PRICE_CHANGED, OnProductPriceChanged);
		EventManager.AddListener<ProductType, int>(GameEvents.PRODUCT_ADDED_TO_BOX, OnProductAddedToBox);
		EventManager.AddListener<ProductType, int>(GameEvents.PRODUCT_REMOVED_FROM_BOX, OnProductRemovedFromBox);
		outOfStockProducts = new List<ProductType>();
		for (int i = 0; i < purchasableProducts.Count; i++)
		{
			ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(purchasableProducts[i]);
			if (ProductLicenseManager.LicensePurchased(productData.requiredLicense, productData.productGroup))
			{
				outOfStockProducts.Add(productData.type);
			}
		}
	}

	public int GetAvailableStockOnShelves(ProductType type)
	{
		if (SingletonBehaviour<ProductPool>.Instance.GetProductData(type).productGroup == ProductGroup.VENDING)
		{
			return SingletonBehaviour<VendingStockManager>.Instance.GetAvailableStockOnShelves(type);
		}
		if (stockDictionary.ContainsKey(type))
		{
			return stockDictionary[type];
		}
		return 0;
	}

	public int GetAvailableStockInBoxes(ProductType type)
	{
		if (boxStockDictionary.ContainsKey(type))
		{
			return boxStockDictionary[type];
		}
		return 0;
	}

	public bool IsProductOutStock(ProductType type)
	{
		return !stockDictionary.ContainsKey(type);
	}

	public bool CanSpawnCustomer()
	{
		if (availableScoreDictionary.Count > 0)
		{
			return availableTotalScore > Mathf.Epsilon;
		}
		return false;
	}

	private void OnLicensePurchased(int newLicense, ProductGroup group)
	{
		if (group == ProductGroup.VENDING)
		{
			return;
		}
		for (int i = 0; i < purchasableProducts.Count; i++)
		{
			ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(SingletonBehaviour<StockManager>.Instance.PurchasableProducts[i]);
			if (productData.requiredLicense == newLicense && productData.productGroup == group)
			{
				outOfStockProducts.Add(productData.type);
			}
		}
	}

	private void OnProductAdded(Shelf shelf, ProductType type)
	{
		if (SingletonBehaviour<ProductPool>.Instance.GetProductData(type).productGroup == ProductGroup.VENDING)
		{
			return;
		}
		if (!shelfDictionary.ContainsKey(type))
		{
			shelfDictionary.Add(type, new ShelvesList
			{
				shelves = new List<Shelf>()
			});
			if (outOfStockProducts.Contains(type))
			{
				outOfStockProducts.Remove(type);
			}
		}
		if (!shelfDictionary[type].shelves.Contains(shelf))
		{
			shelfDictionary[type].shelves.Add(shelf);
		}
		if (!shelf.ParentPlaceable.IsReserved)
		{
			if (!availableShelfDictionary.ContainsKey(type))
			{
				availableShelfDictionary.Add(type, new ShelvesList
				{
					shelves = new List<Shelf>()
				});
			}
			if (!availableShelfDictionary[type].shelves.Contains(shelf))
			{
				availableShelfDictionary[type].shelves.Add(shelf);
			}
			UpdateAvailableStocks(type);
		}
		UpdateStocks(type);
	}

	private void OnProductAddedToBox(ProductType type, int count)
	{
		if (!boxStockDictionary.ContainsKey(type))
		{
			boxStockDictionary.Add(type, 0);
		}
		boxStockDictionary[type] += count;
		EventManager.NotifyEvent(GameEvents.STOCKS_UPDATED, type);
	}

	private void OnProductRemovedFromBox(ProductType type, int count)
	{
		if (boxStockDictionary.ContainsKey(type))
		{
			boxStockDictionary[type] -= count;
			EventManager.NotifyEvent(GameEvents.STOCKS_UPDATED, type);
		}
	}

	private void OnProductRemoved(Shelf shelf, ProductType type)
	{
		if (SingletonBehaviour<ProductPool>.Instance.GetProductData(type).productGroup == ProductGroup.VENDING)
		{
			return;
		}
		if (shelfDictionary[type].shelves.Contains(shelf))
		{
			if (shelf.GetProductCount() == 0)
			{
				shelfDictionary[type].shelves.Remove(shelf);
				if (shelfDictionary[type].shelves.Count == 0)
				{
					shelfDictionary.Remove(type);
					outOfStockProducts.Add(type);
				}
			}
		}
		else if (shelf.GetProductCount() == 0)
		{
			shelfDictionary[type].shelves.Remove(shelf);
			if (shelfDictionary[type].shelves.Count == 0)
			{
				shelfDictionary.Remove(type);
				outOfStockProducts.Add(type);
			}
		}
		if (!shelf.ParentPlaceable.IsReserved)
		{
			if (availableShelfDictionary[type].shelves.Contains(shelf))
			{
				if (shelf.GetProductCount() == 0)
				{
					availableShelfDictionary[type].shelves.Remove(shelf);
					if (availableShelfDictionary[type].shelves.Count == 0)
					{
						availableShelfDictionary.Remove(type);
					}
				}
			}
			else if (shelf.GetProductCount() == 0)
			{
				availableShelfDictionary[type].shelves.Remove(shelf);
				if (availableShelfDictionary[type].shelves.Count == 0)
				{
					availableShelfDictionary.Remove(type);
				}
			}
			UpdateAvailableStocks(type);
		}
		UpdateStocks(type);
	}

	private void UpdateStocks(ProductType type)
	{
		int num = 0;
		if (shelfDictionary.ContainsKey(type))
		{
			for (int i = 0; i < shelfDictionary[type].shelves.Count; i++)
			{
				num += shelfDictionary[type].shelves[i].GetProductCount();
			}
			UpdateStocks(type, num);
		}
		else
		{
			UpdateStocks(type, num);
		}
	}

	private void UpdateStocks(ProductType type, int count)
	{
		if (count == 0)
		{
			totalScore -= scoreDictionary[type];
			stockDictionary.Remove(type);
			scoreDictionary.Remove(type);
		}
		else if (stockDictionary.ContainsKey(type))
		{
			stockDictionary[type] = count;
		}
		else
		{
			stockDictionary.Add(type, count);
			scoreDictionary.Add(type, CalculatePurchaseScore(type));
			totalScore += scoreDictionary[type];
		}
	}

	private void UpdateAvailableStocks(ProductType type)
	{
		int num = 0;
		if (availableShelfDictionary.ContainsKey(type))
		{
			for (int i = 0; i < availableShelfDictionary[type].shelves.Count; i++)
			{
				num += availableShelfDictionary[type].shelves[i].GetProductCount();
			}
			UpdateAvailableStocks(type, num);
		}
		else
		{
			UpdateAvailableStocks(type, num);
		}
		EventManager.NotifyEvent(GameEvents.STOCKS_UPDATED, type);
	}

	private void UpdateAvailableStocks(ProductType type, int count)
	{
		if (SingletonBehaviour<ProductPool>.Instance.GetProductData(type).productGroup != ProductGroup.VENDING)
		{
			if (count == 0)
			{
				availableTotalScore -= availableScoreDictionary[type];
				availableStockDictionary.Remove(type);
				availableScoreDictionary.Remove(type);
			}
			else if (availableStockDictionary.ContainsKey(type))
			{
				availableStockDictionary[type] = count;
			}
			else
			{
				availableStockDictionary.Add(type, count);
				availableScoreDictionary.Add(type, CalculatePurchaseScore(type));
				availableTotalScore += availableScoreDictionary[type];
			}
		}
	}

	private void OnProductPriceChanged(ProductType type, float newPrice)
	{
		if (SingletonBehaviour<ProductPool>.Instance.GetProductData(type).productGroup != ProductGroup.VENDING)
		{
			if (scoreDictionary.ContainsKey(type))
			{
				totalScore -= scoreDictionary[type];
				scoreDictionary[type] = CalculatePurchaseScore(type);
				totalScore += scoreDictionary[type];
			}
			if (availableScoreDictionary.ContainsKey(type))
			{
				availableTotalScore -= availableScoreDictionary[type];
				availableScoreDictionary[type] = CalculatePurchaseScore(type);
				availableTotalScore += availableScoreDictionary[type];
			}
		}
	}

	private float CalculatePurchaseScore(ProductType type, float elasticity = 8f, float asymmetry = 1.6f, float basePopularity = 0.1f)
	{
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(type);
		float unitMarketPrice = productData.unitMarketPrice;
		return CalculatePurchaseScore(SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(productData.type), unitMarketPrice, type);
	}

	public float CalculatePurchaseScore(float currentPrice, float marketPrice, ProductType type, float elasticity = 3f, float asymmetry = 1.2f, float basePopularity = 0.001f)
	{
		if (marketPrice <= 0f)
		{
			return 0.001f;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(type);
		elasticity = priceElasticityForDepartments[productData.productGroup];
		float num = (currentPrice - marketPrice) / marketPrice;
		float num2 = ((num >= 0f) ? (elasticity * asymmetry) : (elasticity / asymmetry));
		float num3 = 1f / (1f + Mathf.Exp(num2 * num));
		return Mathf.Clamp01(0.001f + 0.999f * num3);
	}

	public float GetScoreOnMarketPrice(ProductType type)
	{
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(type);
		return CalculatePurchaseScore(productData.unitMarketPrice, productData.unitMarketPrice, type);
	}

	public float GetCustomerCountMultiplier()
	{
		if (Mathf.Approximately(totalScore, 0f))
		{
			return 0.4f;
		}
		return Mathf.Lerp(1f, 3f, totalScore / 200f);
	}

	public bool AssignCustomerPurchaseParameters(Customer customer, bool isTest = false)
	{
		List<Placeable> list = new List<Placeable>();
		List<CustomerBehavior> list2 = new List<CustomerBehavior>();
		List<Shelf> list3 = new List<Shelf>();
		List<Product> list4 = new List<Product>();
		if (UnityEngine.Random.Range(0, 100) < 5 && outOfStockProducts.Count > 0)
		{
			ProductType randomElement = outOfStockProducts.GetRandomElement();
			ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(randomElement);
			Placeable supportedPlaceable = SingletonBehaviour<SpawnManager>.Instance.GetSupportedPlaceable(productData.shelfType);
			if (supportedPlaceable == null)
			{
				return false;
			}
			if (!isTest)
			{
				SetPlaceableReserved(supportedPlaceable, customer, reserved: true);
			}
			list.Add(supportedPlaceable);
			list2.Add(CustomerBehavior.NO_STOCK_COMPLAINT);
			list4.Add(SingletonBehaviour<ProductPool>.Instance.GetProduct(randomElement));
			if (!isTest)
			{
				customer.SetPurchaseParameters(list, null, list4, list2);
			}
			return true;
		}
		int productCount = GetProductCount();
		float basketBudget = GetBasketBudget();
		float num = 0f;
		int num2 = 0;
		int num3 = productCount * 5;
		List<ProductType> list5 = new List<ProductType>();
		while (list4.Count < productCount && num2 < num3)
		{
			num2++;
			Shelf randomShelf = GetRandomShelf(customer);
			if (randomShelf == null)
			{
				continue;
			}
			Product product = randomShelf.GetRandomProduct();
			if (product == null || list5.Contains(product.Data.type))
			{
				continue;
			}
			float unitPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(product.Data.type);
			if (num + unitPrice > basketBudget)
			{
				continue;
			}
			list.Add(randomShelf.ParentPlaceable);
			list3.Add(randomShelf);
			CustomerBehavior customerBehavior = CustomerBehavior.PURCHASE;
			float value = scoreDictionary[product.Data.type];
			float num4 = GetScoreOnMarketPrice(product.Data.type) * 0.6f;
			float num5 = Mathf.InverseLerp(num4, 0.001f, value) * 100f;
			if (scoreDictionary[product.Data.type] < num4 && (float)UnityEngine.Random.Range(0, 100) < num5)
			{
				customerBehavior = CustomerBehavior.OVER_PRICE_COMPLAINT;
				list5.Add(product.Data.type);
				product = SingletonBehaviour<ProductPool>.Instance.GetProduct(product.Data.type);
			}
			if (customerBehavior == CustomerBehavior.PURCHASE)
			{
				num += unitPrice;
			}
			product.isReserved = true;
			list4.Add(product);
			list2.Add(customerBehavior);
			if (list4.Count < productCount && IsBoostedProduct(product.Data) && customerBehavior == CustomerBehavior.PURCHASE && UnityEngine.Random.Range(0, 100) < 50)
			{
				Product randomProduct = randomShelf.GetRandomProduct();
				if (randomProduct != null)
				{
					num += unitPrice;
					list.Add(randomShelf.ParentPlaceable);
					list3.Add(randomShelf);
					randomProduct.isReserved = true;
					list4.Add(randomProduct);
					list2.Add(CustomerBehavior.PURCHASE);
				}
			}
		}
		if (isTest)
		{
			string text = "";
			float num6 = 0f;
			for (int i = 0; i < list4.Count; i++)
			{
				if (list2[i] != CustomerBehavior.OVER_PRICE_COMPLAINT)
				{
					num6 += SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(list4[i].Data.type) - list4[i].Data.cost;
					text = text + list4[i].Data.type.ToString() + ", ";
				}
			}
			testTotalProfit += num6;
		}
		if (!isTest)
		{
			for (int j = 0; j < list.Count; j++)
			{
				SetPlaceableReserved(list[j], customer, reserved: true);
			}
		}
		_ = list4.Count;
		if (!isTest)
		{
			customer.SetPurchaseParameters(list, list3, list4, list2);
		}
		return true;
	}

	private bool IsBoostedProduct(ProductData data)
	{
		if (data.productGroup == ProductGroup.GROCERY || data.productGroup == ProductGroup.BAKERY || data.productGroup == ProductGroup.BEACH)
		{
			return true;
		}
		return false;
	}

	public void SetPlaceableReserved(Placeable placeable, Customer customer, bool reserved)
	{
		if (reserved)
		{
			if (placeable.isReserved && placeable.GetCurrentCustomer() == customer)
			{
				return;
			}
			for (int i = 0; i < placeable.Shelves.Count; i++)
			{
				Shelf shelf = placeable.Shelves[i];
				if (shelf.ContainedType != ProductType.NONE && availableShelfDictionary.ContainsKey(shelf.ContainedType))
				{
					availableShelfDictionary[shelf.ContainedType].shelves.Remove(shelf);
					if (availableShelfDictionary[shelf.ContainedType].shelves.Count == 0)
					{
						availableShelfDictionary.Remove(shelf.ContainedType);
					}
					UpdateAvailableStocks(shelf.ContainedType);
				}
			}
		}
		else
		{
			for (int j = 0; j < placeable.Shelves.Count; j++)
			{
				Shelf shelf2 = placeable.Shelves[j];
				if (shelf2.ContainedType != ProductType.NONE)
				{
					if (!availableShelfDictionary.ContainsKey(shelf2.ContainedType))
					{
						availableShelfDictionary.Add(shelf2.ContainedType, new ShelvesList
						{
							shelves = new List<Shelf>()
						});
					}
					if (!availableShelfDictionary[shelf2.ContainedType].shelves.Contains(shelf2))
					{
						availableShelfDictionary[shelf2.ContainedType].shelves.Add(shelf2);
					}
					UpdateAvailableStocks(shelf2.ContainedType);
				}
			}
		}
		placeable.SetReserved(customer, reserved);
	}

	private int GetProductCount()
	{
		float t = Mathf.Clamp01(totalScore / 100f);
		int num = Mathf.RoundToInt(Mathf.Lerp(5f, 10f, t));
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			float num2 = Mathf.Abs((float)(i + 1) - (float)(num + 1) * 0.5f);
			array[i] = 1f / (1f + num2);
			array[i] = Mathf.Max(array[i], 0.15f);
		}
		float num3 = 0f;
		for (int j = 0; j < num; j++)
		{
			num3 += array[j];
		}
		for (int k = 0; k < num; k++)
		{
			array[k] /= num3;
		}
		float value = UnityEngine.Random.value;
		float num4 = 0f;
		for (int l = 0; l < num; l++)
		{
			num4 += array[l];
			if (value <= num4)
			{
				return l + 1;
			}
		}
		return num;
	}

	private float GetBasketBudget()
	{
		float t = Mathf.Clamp01(totalScore / 60f);
		float minInclusive = Mathf.Lerp(60f, 120f, t);
		float maxInclusive = Mathf.Lerp(200f, 350f, t);
		float minInclusive2 = Mathf.Lerp(200f, 350f, t);
		float maxInclusive2 = Mathf.Lerp(600f, 900f, t);
		float minInclusive3 = Mathf.Lerp(600f, 900f, t);
		float maxInclusive3 = Mathf.Lerp(1400f, 2000f, t);
		float value = UnityEngine.Random.value;
		if (value < 0.5f)
		{
			return UnityEngine.Random.Range(minInclusive, maxInclusive);
		}
		if (value < 0.8f)
		{
			return UnityEngine.Random.Range(minInclusive2, maxInclusive2);
		}
		return UnityEngine.Random.Range(minInclusive3, maxInclusive3);
	}

	public Shelf GetRandomShelf(Customer customer)
	{
		ProductType randomProductType = GetRandomProductType();
		if (randomProductType == ProductType.NONE)
		{
			MonoBehaviour.print("No product type found");
			return null;
		}
		for (int i = 0; i < shelfDictionary[randomProductType].shelves.Count; i++)
		{
			Shelf shelf = shelfDictionary[randomProductType].shelves[i];
			if ((!shelf.ParentPlaceable.IsReserved || shelf.ParentPlaceable.GetCurrentCustomer().Equals(customer)) && shelfDictionary[randomProductType].shelves[i].IsProductAvailable())
			{
				return shelfDictionary[randomProductType].shelves[i];
			}
		}
		MonoBehaviour.print("No shelf found for product type: " + randomProductType);
		return null;
	}

	private ProductType GetRandomProductType()
	{
		if (availableTotalScore <= 0f)
		{
			return ProductType.NONE;
		}
		float num = UnityEngine.Random.Range(0f, availableTotalScore);
		float num2 = 0f;
		foreach (KeyValuePair<ProductType, float> item in availableScoreDictionary)
		{
			num2 += item.Value;
			if (num < num2 + Mathf.Epsilon)
			{
				return item.Key;
			}
		}
		if (availableScoreDictionary.Count > 0)
		{
			return availableScoreDictionary.Last().Key;
		}
		return ProductType.NONE;
	}
}
