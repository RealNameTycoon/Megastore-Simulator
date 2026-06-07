using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class ProductPool : SingletonBehaviour<ProductPool>
{
	[Serializable]
	public class ProductDictionary : UnitySerializedDictionary<ProductType, ListContainer>
	{
	}

	[Serializable]
	public class ShelfDictionary : UnitySerializedDictionary<PlaceableType, ProductData>
	{
	}

	[Serializable]
	public class FurnitureDictionary : UnitySerializedDictionary<FurnitureType, ProductData>
	{
	}

	[SerializeField]
	private ProductDictionary productPool;

	[SerializeField]
	private ShelfDictionary shelfData;

	[SerializeField]
	private FurnitureDictionary furnitureData;

	[SerializeField]
	private List<Transform> licenseContainers;

	[SerializeField]
	private StockManager stockManager;

	[SerializeField]
	private VendingStockManager vendingStockManager;

	[SerializeField]
	private SerializedDictionary<ProductType, Material> spoilableMaterials = new SerializedDictionary<ProductType, Material>();

	[SerializeField]
	private SerializedDictionary<ProductType, Material> originalMaterials = new SerializedDictionary<ProductType, Material>();

	private Dictionary<ProductType, int> variatedProductToCountMap = new Dictionary<ProductType, int>();

	public static Dictionary<ProductType, PlaceableType> ProductTypeToPlaceableType = new Dictionary<ProductType, PlaceableType>
	{
		{
			ProductType.SMALL_SHELF,
			PlaceableType.SMALL_SHELF
		},
		{
			ProductType.VEGETABLE_SHELF,
			PlaceableType.VEGETABLE_SHELF
		},
		{
			ProductType.WIDE_SHELF,
			PlaceableType.WIDE_SHELF
		},
		{
			ProductType.FRIDGE,
			PlaceableType.FRIDGE
		},
		{
			ProductType.FREEZER,
			PlaceableType.FREEZER
		},
		{
			ProductType.WIDE_FRIDGE,
			PlaceableType.WIDE_FRIGE
		},
		{
			ProductType.VENDING_MACHINE,
			PlaceableType.VENDING_MACHINE
		},
		{
			ProductType.STORAGE_SHELF,
			PlaceableType.STORAGE_SHELF
		},
		{
			ProductType.WALL_SHELF,
			PlaceableType.WALL_SHELF
		},
		{
			ProductType.BAKERY_SHELF,
			PlaceableType.BAKERY_SHELF
		},
		{
			ProductType.BAKERY_SHELF_WIDE,
			PlaceableType.BAKERY_SHELF_WIDE
		},
		{
			ProductType.FOLDED_CLOTH_RACK_1,
			PlaceableType.FOLDED_CLOTH_RACK_1
		},
		{
			ProductType.FOLDED_CLOTH_RACK_2,
			PlaceableType.FOLDED_CLOTH_RACK_2
		},
		{
			ProductType.FOLDED_CLOTH_RACK_3,
			PlaceableType.FOLDED_CLOTH_RACK_3
		},
		{
			ProductType.HANGER_CLOTH_RACK_1,
			PlaceableType.HANGER_CLOTH_RACK_1
		},
		{
			ProductType.HANGER_CLOTH_RACK_2,
			PlaceableType.HANGER_CLOTH_RACK_2
		},
		{
			ProductType.HANGER_CLOTH_RACK_3,
			PlaceableType.HANGER_CLOTH_RACK_3
		},
		{
			ProductType.HANGER_CLOTH_RACK_4,
			PlaceableType.HANGER_CLOTH_RACK_4
		},
		{
			ProductType.HANGER_CLOTH_RACK_5,
			PlaceableType.HANGER_CLOTH_RACK_5
		},
		{
			ProductType.CIRCULAR_SHELF,
			PlaceableType.CIRCULAR_SHELF
		},
		{
			ProductType.SPORTS_SHELF_TWO_RACK,
			PlaceableType.SPORTS_SHELF_TWO_RACK
		},
		{
			ProductType.SPORTS_SHELF_SHORT_HANGER,
			PlaceableType.SPORTS_SHELF_SHORT_HANGER
		},
		{
			ProductType.SPORTS_SHELF_LONG_HANGER,
			PlaceableType.SPORTS_SHELF_LONG_HANGER
		},
		{
			ProductType.MUSIC_SHELF_HANGER,
			PlaceableType.MUSIC_SHELF_HANGER
		},
		{
			ProductType.MUSIC_SHELF_TWO_RACK,
			PlaceableType.MUSIC_SHELF_TWO_RACK
		},
		{
			ProductType.MUSIC_SHELF_LONG_HANGER,
			PlaceableType.MUSIC_SHELF_LONG_HANGER
		},
		{
			ProductType.ELECTRONICS_SHELF_TABLE,
			PlaceableType.ELECTRONICS_SHELF_TABLE
		},
		{
			ProductType.ELECTRONICS_SHELF_CIRCULAR,
			PlaceableType.ELECTRONICS_SHELF_CIRCULAR
		},
		{
			ProductType.ELECTRONICS_SHELF_HANGER,
			PlaceableType.ELECTRONICS_SHELF_HANGER
		},
		{
			ProductType.ELECTRONICS_SHELF_CUPBOARD,
			PlaceableType.ELECTRONICS_SHELF_CUPBOARD
		},
		{
			ProductType.CANDY_STAND,
			PlaceableType.CANDY_STAND
		},
		{
			ProductType.PRODUCE_SHELF_SMALL,
			PlaceableType.PRODUCE_SHELF_SMALL
		},
		{
			ProductType.PRODUCE_SHELF_WIDE,
			PlaceableType.PRODUCE_SHELF_WIDE
		},
		{
			ProductType.PRODUCE_SHELF_INSIDE_CORNER,
			PlaceableType.PRODUCE_SHELF_INSIDE_CORNER
		},
		{
			ProductType.PRODUCE_SHELF_OUTSIDE_CORNER,
			PlaceableType.PRODUCE_SHELF_OUTSIDE_CORNER
		},
		{
			ProductType.FISH_STAND,
			PlaceableType.FISH_STAND
		},
		{
			ProductType.LOBSTER_TANK,
			PlaceableType.LOBSTER_TANK
		},
		{
			ProductType.WOODEN_PRODUCE_SHELF,
			PlaceableType.WOODEN_PRODUCE_SHELF
		},
		{
			ProductType.WOODEN_PRODUCE_SHELF_CIRCULAR,
			PlaceableType.WOODEN_PRODUCE_SHELF_CIRCULAR
		},
		{
			ProductType.TOY_HANGER_SHELF,
			PlaceableType.TOY_HANGER_SHELF
		},
		{
			ProductType.TOY_WIDE_SHELF,
			PlaceableType.TOY_WIDE_SHELF
		},
		{
			ProductType.TOY_SHELF,
			PlaceableType.TOY_SHELF
		},
		{
			ProductType.TOY_BALL_SHELF,
			PlaceableType.TOY_BALL_SHELF
		},
		{
			ProductType.FREEZER_ISLAND,
			PlaceableType.FREEZER_ISLAND
		},
		{
			ProductType.FREEZER_ISLAND_CORNER,
			PlaceableType.FREEZER_ISLAND_CORNER
		},
		{
			ProductType.BEACH_SHELF,
			PlaceableType.BEACH_SHELF
		},
		{
			ProductType.BEACH_BASKET_SHELF,
			PlaceableType.BEACH_BASKET_SHELF
		},
		{
			ProductType.TOWEL_STAND_TALL,
			PlaceableType.TOWEL_STAND_TALL
		},
		{
			ProductType.TOWEL_STAND_SHORT,
			PlaceableType.TOWEL_STAND_SHORT
		}
	};

	public static Dictionary<ProductType, FurnitureType> ProductTypeToFurnitureType = new Dictionary<ProductType, FurnitureType>
	{
		{
			ProductType.OVEN,
			FurnitureType.OVEN
		},
		{
			ProductType.CHECKOUT_DESK,
			FurnitureType.CHECKOUT_DESK
		},
		{
			ProductType.SELF_CHECKOUT,
			FurnitureType.SELF_CHECKOUT
		},
		{
			ProductType.PALLET_RACK,
			FurnitureType.PALLET_RACK
		},
		{
			ProductType.TRAY_STATION,
			FurnitureType.TRAY_STATION
		},
		{
			ProductType.FITTING_ROOM,
			FurnitureType.FITTING_ROOM
		},
		{
			ProductType.MANNEQUIN_SPORT,
			FurnitureType.MANNEQUIN_SPORT
		},
		{
			ProductType.INFLATABLE_POOL,
			FurnitureType.INFLATABLE_POOL
		},
		{
			ProductType.TRASH_FURNITURE,
			FurnitureType.TRASH_FURNITURE
		},
		{
			ProductType.RECYCLE_BIN,
			FurnitureType.RECYCLE_BIN
		},
		{
			ProductType.BOX_SHELF,
			FurnitureType.BOX_SHELF
		},
		{
			ProductType.BOX_SHELF_SMALL,
			FurnitureType.BOX_SHELF_SMALL
		},
		{
			ProductType.WALK_IN_FREEZER_SMALL,
			FurnitureType.WALK_IN_FREEZER_SMALL
		},
		{
			ProductType.WALK_IN_FREEZER_MEDIUM,
			FurnitureType.WALK_IN_FREEZER_MEDIUM
		},
		{
			ProductType.WALK_IN_FREEZER_LARGE,
			FurnitureType.WALK_IN_FREEZER_LARGE
		},
		{
			ProductType.SMALL_PALLET_RACK,
			FurnitureType.SMALL_PALLET_RACK
		}
	};

	private new void Awake()
	{
		base.Awake();
		stockManager.Initialize();
		vendingStockManager.Initialize();
	}

	public ProductData GetAnyProductData(ProductType type)
	{
		if (type >= ProductType.NONCONSUMABLE_START && type < ProductType.FURNITURE_START)
		{
			return GetShelfData(ProductTypeToPlaceableType[type]);
		}
		if (type >= ProductType.FURNITURE_START)
		{
			return GetFurnitureData(ProductTypeToFurnitureType[type]);
		}
		return GetProductData(type);
	}

	public bool HasSpoilableMaterial(ProductType type)
	{
		if (spoilableMaterials.ContainsKey(type))
		{
			return originalMaterials.ContainsKey(type);
		}
		return false;
	}

	public Material GetSpoilableMaterial(ProductType type)
	{
		return spoilableMaterials[type];
	}

	public Material GetOriginalMaterial(ProductType type)
	{
		return originalMaterials[type];
	}

	public ProductData GetShelfData(PlaceableType type)
	{
		if (!shelfData.ContainsKey(type))
		{
			Debug.LogError("Shelf data for type " + type.ToString() + " is null.");
			return null;
		}
		return shelfData[type];
	}

	public ProductData GetFurnitureData(FurnitureType type)
	{
		return furnitureData[type];
	}

	public bool HasConsumableData(ProductType type)
	{
		if (productPool.ContainsKey(type) && productPool[type] != null && productPool[type].objects != null && productPool[type].objects.Count > 0)
		{
			return productPool[type].objects[0] != null;
		}
		return false;
	}

	public bool HasProductData(ProductType type)
	{
		if (type >= ProductType.NONCONSUMABLE_START && type < ProductType.FURNITURE_START)
		{
			return shelfData.ContainsKey(ProductTypeToPlaceableType[type]);
		}
		if (type >= ProductType.FURNITURE_START && type < ProductType.WAREHOUSE_EQUIPMENT_START)
		{
			return furnitureData.ContainsKey(ProductTypeToFurnitureType[type]);
		}
		return HasConsumableData(type);
	}

	public void MakeAllItemsForOneBox()
	{
		ProductDictionary productDictionary = new ProductDictionary();
		foreach (ListContainer value in productPool.Values)
		{
			List<Product> objects = value.objects;
			if (objects.Count < objects[0].Data.GetMaxProductCount() * 5)
			{
				for (int i = objects.Count; i < objects[0].Data.GetMaxProductCount() * 5; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(objects[0].gameObject, base.transform);
					objects.Add(gameObject.GetComponent<Product>());
				}
			}
			ListContainer listContainer = new ListContainer();
			listContainer.objects = objects;
			productDictionary.Add(objects[0].Data.type, listContainer);
		}
		productPool = productDictionary;
	}

	public void DetectMeshColliders()
	{
		for (int i = 0; i < productPool.Values.ToList().Count; i++)
		{
			Product product = productPool.Values.ToList()[i].objects[0];
			if (product == null || productPool.Values.ToList()[i] == null)
			{
				MonoBehaviour.print("no item exist:" + productPool.Keys.ToList()[i]);
				continue;
			}
			MeshCollider componentInChildren = product.gameObject.GetComponentInChildren<MeshCollider>();
			if (componentInChildren != null)
			{
				MonoBehaviour.print("mesh collider found:" + componentInChildren.gameObject.name);
			}
		}
	}

	public void DetectMissingSprites()
	{
		List<ProductType> list = productPool.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (productPool[list[i]].objects[0].Data.productSprite == null)
			{
				MonoBehaviour.print("missing sprite: " + list[i]);
			}
		}
		foreach (KeyValuePair<PlaceableType, ProductData> shelfDatum in shelfData)
		{
			if (shelfDatum.Value.productSprite == null)
			{
				MonoBehaviour.print("missing sprite: " + shelfDatum.Key);
			}
		}
		foreach (KeyValuePair<FurnitureType, ProductData> furnitureDatum in furnitureData)
		{
			if (furnitureDatum.Value.productSprite == null)
			{
				MonoBehaviour.print("missing sprite: " + furnitureDatum.Key);
			}
		}
	}

	public void DetectMissingVariants()
	{
		List<ProductType> list = productPool.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			List<Product> objects = productPool[list[i]].objects;
			if (objects.Count == 0)
			{
				MonoBehaviour.print("no item exist:" + list[i]);
			}
			else if (objects[0].Data.variantCount != objects.Count)
			{
				MonoBehaviour.print("missin variants: " + list[i]);
			}
		}
	}

	public void DetectOffSideProductLocations()
	{
		List<ProductType> list = productPool.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (GetProductData(list[i]).boxType == BoxType.WARDROBE_BOX)
			{
				MonoBehaviour.print(list[i].ToString() + " is wardrobe box");
			}
		}
	}

	public void DeleteExtraItems()
	{
		ProductDictionary productDictionary = new ProductDictionary();
		foreach (ListContainer value in productPool.Values)
		{
			List<Product> objects = value.objects;
			if (objects.Count > 1)
			{
				Product item = objects[0];
				for (int i = 1; i < objects.Count; i++)
				{
					if (objects[i] != null)
					{
						UnityEngine.Object.DestroyImmediate(objects[i].gameObject);
					}
				}
				objects.Clear();
				objects.Add(item);
			}
			ListContainer listContainer = new ListContainer();
			listContainer.objects = objects;
			productDictionary.Add(objects[0].Data.type, listContainer);
		}
		productPool = productDictionary;
	}

	public Product GetProduct(ProductType type)
	{
		List<Product> objects = productPool[type].objects;
		int variantCount = objects[0].Data.variantCount;
		if (variantCount > 1 && !variatedProductToCountMap.ContainsKey(type))
		{
			variatedProductToCountMap.Add(type, 0);
		}
		for (int i = variantCount; i < objects.Count; i++)
		{
			if (objects[i].gameObject.activeSelf || objects[i].isReserved)
			{
				continue;
			}
			if (variantCount > 1)
			{
				int num = variatedProductToCountMap[type] % variantCount;
				if (objects[i].VariantIndex != num)
				{
					continue;
				}
				variatedProductToCountMap[type]++;
			}
			objects[i].EnableOutline(state: false);
			if (objects[i].Data.boxType == BoxType.FRUIT_BOX)
			{
				objects[i].EnableRenderers(enable: true);
			}
			objects[i].gameObject.SetActive(value: true);
			return objects[i];
		}
		int index = 0;
		if (variantCount > 1)
		{
			index = variatedProductToCountMap[type] % variantCount;
			variatedProductToCountMap[type]++;
		}
		Product product = UnityEngine.Object.Instantiate(objects[index]);
		product.transform.position = Vector3.zero;
		product.transform.localScale = Vector3.one;
		product.isReserved = false;
		product.EnableOutline(state: false);
		if (product.Data.boxType == BoxType.FRUIT_BOX)
		{
			product.EnableRenderers(enable: true);
		}
		productPool[type].objects.Add(product);
		if (!product.gameObject.activeSelf)
		{
			product.gameObject.SetActive(value: true);
		}
		return product;
	}

	public ProductData GetProductData(ProductType type)
	{
		if (!productPool.ContainsKey(type) || productPool[type] == null || productPool[type].objects.Count == 0 || productPool[type].objects[0] == null)
		{
			return productPool[ProductType.COFFEE].objects[0].Data;
		}
		return productPool[type].objects[0].Data;
	}

	public void PutBackToPool(Product usedProduct)
	{
		usedProduct.gameObject.SetActive(value: false);
		usedProduct.isReserved = false;
		usedProduct.EnableOutline(state: false);
		usedProduct.transform.SetParent(base.transform);
		usedProduct.transform.localScale = Vector3.one;
		usedProduct.transform.localPosition = Vector3.zero;
		usedProduct.TempPrice = 0f;
		usedProduct.ResetProduct();
	}
}
