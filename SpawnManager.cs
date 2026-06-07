using System.Collections.Generic;
using DFTGames.Localization;
using UnityEngine;
using UnityEngine.Rendering;

public class SpawnManager : SingletonBehaviour<SpawnManager>
{
	[SerializeField]
	private Transform initialShelfTransform;

	[SerializeField]
	private Transform initialVegetableShelfTransform;

	[SerializeField]
	private Transform initialOvenTransform;

	[SerializeField]
	private Transform initialBakeryShelfTransform;

	[SerializeField]
	private Transform initialToyShelfTransform;

	[SerializeField]
	private Transform initialFoldedRackTransform;

	[SerializeField]
	private Transform initialWalkInFreezerTransform;

	[SerializeField]
	private Transform initialCheckoutDeskTransform;

	private string PLACEABLE_COUNT_KEY = "PLACEABLE_COUNT";

	private string FURNITURE_COUNT_KEY = "FURNITURE_COUNT";

	public const string INITIAL_PRODUCTGROUP_KEY = "INITIAL_PRDUCTGROUP_KEY";

	public const string PACKED_KEY = "FURNITURE_PACKED_KEY";

	public const string DELETED_KEY = "FURNITURE_DELETED_KEY";

	public const string VEGETABLE_SHELF_REFUNDED = "VEGETABLE_SHELF_REFUNDED";

	public const string FREE_FREEZER_GIVEN = "FREE_FREEZER_GIVEN";

	private List<Placeable> placeables = new List<Placeable>();

	private List<Furniture> furnitures = new List<Furniture>();

	[SerializeField]
	private SerializedDictionary<FurnitureType, List<Furniture>> furnituresDictionary = new SerializedDictionary<FurnitureType, List<Furniture>>();

	[SerializeField]
	private List<Placeable> bakeryShelfPlaceables = new List<Placeable>();

	[SerializeField]
	private List<ChoppingStandPlaceable> fishStandPlaceables = new List<ChoppingStandPlaceable>();

	public const int TUTORIAL_ID = 4;

	private ProductGroup initialProductGroup = ProductGroup.NONE;

	private float vegetableShelfRefundAmount;

	private static Dictionary<ProductType, int> MAX_ALLOWED_PRODUCT_AMOUNT_DICTIONARY = new Dictionary<ProductType, int>
	{
		{
			ProductType.CHECKOUT_DESK,
			10
		},
		{
			ProductType.SELF_CHECKOUT,
			6
		}
	};

	public Transform InitialShelfTransform => initialShelfTransform;

	public Transform InitialVegetableShelfTransform => initialVegetableShelfTransform;

	public Transform InitialOvenTransform => initialOvenTransform;

	public Transform InitialBakeryShelfTransform => initialBakeryShelfTransform;

	public Transform InitialToyShelfTransform => initialToyShelfTransform;

	public Transform InitialWalkInFreezerTransform => initialWalkInFreezerTransform;

	public Transform InitialCheckoutDeskTransform => initialCheckoutDeskTransform;

	public Transform InitialFoldedRackTransform => initialFoldedRackTransform;

	public List<Placeable> BakeryShelfPlaceables => bakeryShelfPlaceables;

	public List<ChoppingStandPlaceable> FishStandPlaceables => fishStandPlaceables;

	public ProductGroup InitialProductGroup => initialProductGroup;

	public float VegetableShelfRefundAmount => vegetableShelfRefundAmount;

	public List<Placeable> Placeables => placeables;

	public static int GetMaxAllowedProductAmount(ProductType type)
	{
		if (MAX_ALLOWED_PRODUCT_AMOUNT_DICTIONARY.ContainsKey(type))
		{
			return MAX_ALLOWED_PRODUCT_AMOUNT_DICTIONARY[type];
		}
		return -1;
	}

	private new void Awake()
	{
		base.Awake();
		initialProductGroup = GenericDataSerializer.Load("INITIAL_PRDUCTGROUP_KEY", ProductGroup.NONE);
		EventManager.AddListener(StartupEvents.PALLETS_INITIALIZED, TryInitialize);
	}

	private void TryInitialize()
	{
		if (GenericDataSerializer.HasKey("INITIAL_PRDUCTGROUP_KEY"))
		{
			Initialize();
		}
	}

	public void PackFurniture(Furniture furniture)
	{
		GenericDataSerializer.Save("FURNITURE_PACKED_KEY" + furniture.Type.ToString() + furniture.FurnitureID, dataToSave: true);
		FurnitureType type = furniture.Type;
		furnitures.Remove(furniture);
		furnituresDictionary[type].Remove(furniture);
		SingletonBehaviour<FurniturePool>.Instance.RemoveFurniture(furniture);
		furniture.gameObject.SetActive(value: false);
		Box box = SingletonBehaviour<BoxManager>.Instance.SpawnBox(type, furniture.transform.position, furniture.transform.eulerAngles);
		box.SaveLocation();
		box.SetPackableID(furniture.FurnitureID);
		SingletonBehaviour<BoxManager>.Instance.PickUpBox(box);
		EventManager.NotifyEvent(PlaceableEvents.FURNITURE_REMOVED, furniture.Type);
	}

	public void PackFurnitureWithoutBox(Furniture furniture)
	{
		GenericDataSerializer.Save("FURNITURE_PACKED_KEY" + furniture.Type.ToString() + furniture.FurnitureID, dataToSave: true);
		FurnitureType type = furniture.Type;
		furnitures.Remove(furniture);
		furnituresDictionary[type].Remove(furniture);
		SingletonBehaviour<FurniturePool>.Instance.RemoveFurniture(furniture);
		furniture.gameObject.SetActive(value: false);
		EventManager.NotifyEvent(PlaceableEvents.FURNITURE_REMOVED, furniture.Type);
	}

	public void PackPlaceable(Placeable placeable)
	{
		GenericDataSerializer.Save("FURNITURE_PACKED_KEY" + placeable.Type.ToString() + placeable.PlaceableID, dataToSave: true);
		placeables.Remove(placeable);
		SingletonBehaviour<PlaceablePool>.Instance.RemovePlaceable(placeable);
		placeable.gameObject.SetActive(value: false);
		Box box = SingletonBehaviour<BoxManager>.Instance.SpawnBox(placeable.Type, placeable.transform.position, placeable.transform.eulerAngles);
		box.SaveLocation();
		box.SetPackableID(placeable.PlaceableID);
		SingletonBehaviour<BoxManager>.Instance.PickUpBox(box);
		if (placeable.Type == PlaceableType.FISH_STAND)
		{
			fishStandPlaceables.Remove(placeable as ChoppingStandPlaceable);
		}
		if (placeable.Type == PlaceableType.BAKERY_SHELF)
		{
			bakeryShelfPlaceables.Remove(placeable);
		}
		EventManager.NotifyEvent(PlaceableEvents.PLACEABLE_REMOVED, placeable.Type);
	}

	public void UnpackFurniture(Furniture furniture)
	{
		GenericDataSerializer.DeleteKey("FURNITURE_PACKED_KEY" + furniture.Type.ToString() + furniture.FurnitureID);
	}

	public void UnpackPlaceable(Placeable placeable)
	{
		GenericDataSerializer.DeleteKey("FURNITURE_PACKED_KEY" + placeable.Type.ToString() + placeable.PlaceableID);
	}

	public void DeleteFurniture(FurnitureType type, int id)
	{
		GenericDataSerializer.Save("FURNITURE_DELETED_KEY" + type.ToString() + id, dataToSave: true);
	}

	public void DeletePlaceable(PlaceableType type, int id)
	{
		GenericDataSerializer.Save("FURNITURE_DELETED_KEY" + type.ToString() + id, dataToSave: true);
	}

	public void Initialize()
	{
		initialProductGroup = GenericDataSerializer.Load<ProductGroup>("INITIAL_PRDUCTGROUP_KEY");
		InitializePlaceables();
		InitializeFurnitures();
		EventManager.NotifyEvent(StartupEvents.SPAWN_MANAGER_INITIALIZED);
		EventManager.NotifyEvent(StartupEvents.TEMPERATURE_ZONES_INITIALIZED);
		bool num = GenericDataSerializer.LoadBool("VEGETABLE_SHELF_REFUNDED");
		bool flag = GenericDataSerializer.LoadBool("FREE_FREEZER_GIVEN");
		bool flag2 = false;
		if (!num)
		{
			float num2 = vegetableShelfRefundAmount + SingletonBehaviour<BoxManager>.Instance.VegetableShelfRefundAmount;
			if (num2 > 0f)
			{
				string refundAmount = Locale.GetWord("refund_description").Replace("{0}", num2.ToString());
				SingletonWindow<RefundWindow>.Instance.Open(refundAmount);
				flag2 = true;
				GenericDataSerializer.SaveBool("VEGETABLE_SHELF_REFUNDED", value: true);
				EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, num2);
			}
		}
		if (GameManager.GetSaveVersion() < GameManager.COLD_STORAGE_VERSION && !flag)
		{
			GenericDataSerializer.SaveBool("FREE_FREEZER_GIVEN", value: true);
			if (!flag2)
			{
				string word = Locale.GetWord("free_walkin_description");
				SingletonWindow<RefundWindow>.Instance.Open(word, Locale.GetWord("free_walkin_title"));
				flag2 = true;
			}
			SingletonBehaviour<BoxManager>.Instance.SpawnFreeWalkInFreezer();
		}
		SingletonBehaviour<PlayerLook>.Instance.Initialize();
	}

	public Placeable GetSupportedPlaceable(ShelfType type)
	{
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].TopShelf.IsSupported(type) && !placeables[i].IsReserved)
			{
				return placeables[i];
			}
		}
		return null;
	}

	public Placeable GetRandomAvailablePlaceable()
	{
		List<Placeable> list = new List<Placeable>();
		for (int i = 0; i < placeables.Count; i++)
		{
			if (!placeables[i].IsReserved && placeables[i].Type != PlaceableType.VENDING_MACHINE && placeables[i].Type != PlaceableType.STORAGE_SHELF && placeables[i].Type != PlaceableType.OVEN && placeables[i].HasAvailableProduct())
			{
				list.Add(placeables[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	public Placeable GetRandomAvailableVender()
	{
		List<Placeable> list = new List<Placeable>();
		for (int i = 0; i < placeables.Count; i++)
		{
			if (!placeables[i].IsReserved && placeables[i].Type == PlaceableType.VENDING_MACHINE && placeables[i].HasAvailableProduct())
			{
				list.Add(placeables[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	public Placeable GetPlaceableById(PlaceableType type, int id)
	{
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].Type == type && placeables[i].PlaceableID == id)
			{
				return placeables[i];
			}
		}
		return null;
	}

	public int TotalContainedProduct()
	{
		int num = 0;
		for (int i = 0; i < placeables.Count; i++)
		{
			num += placeables[i].ContainedProductCount();
		}
		return num;
	}

	private void InitializePlaceables()
	{
		bool flag = GenericDataSerializer.LoadBool("VEGETABLE_SHELF_REFUNDED");
		for (int i = 0; i < 58; i++)
		{
			PlaceableType placeableType = (PlaceableType)i;
			int placeableCount = GetPlaceableCount(placeableType);
			for (int j = 0; j < placeableCount; j++)
			{
				bool num = GenericDataSerializer.LoadBool("FURNITURE_PACKED_KEY" + placeableType.ToString() + j);
				bool flag2 = GenericDataSerializer.LoadBool("FURNITURE_DELETED_KEY" + placeableType.ToString() + j);
				if (num || flag2)
				{
					continue;
				}
				if (placeableType == PlaceableType.VEGETABLE_SHELF)
				{
					if (!flag)
					{
						vegetableShelfRefundAmount += 250f;
					}
					continue;
				}
				Placeable placeable = SingletonBehaviour<PlaceablePool>.Instance.GetPlaceable(placeableType);
				placeable.InitializeOldPlaceable(j);
				if (!placeable.IsSavePositionCorrupted)
				{
					placeables.Add(placeable);
					if (placeable.IsBakeryPlaceable())
					{
						bakeryShelfPlaceables.Add(placeable);
					}
					if (placeable.Type == PlaceableType.FISH_STAND)
					{
						fishStandPlaceables.Add(placeable as ChoppingStandPlaceable);
					}
				}
			}
		}
		int placeableCount2 = GetPlaceableCount(PlaceableType.STORAGE_SHELF);
		for (int k = 0; k < placeableCount2; k++)
		{
			Placeable placeable2 = SingletonBehaviour<PlaceablePool>.Instance.GetPlaceable(PlaceableType.STORAGE_SHELF);
			placeable2.InitializeOldPlaceable(k);
			placeables.Add(placeable2);
			if (placeable2.IsBakeryPlaceable())
			{
				bakeryShelfPlaceables.Add(placeable2);
			}
			if (placeable2.Type == PlaceableType.FISH_STAND)
			{
				fishStandPlaceables.Add(placeable2 as ChoppingStandPlaceable);
			}
		}
	}

	private void InitializeFurnitures()
	{
		for (int i = 0; i < 16; i++)
		{
			FurnitureType type = (FurnitureType)i;
			int furnitureCount = GetFurnitureCount(type);
			for (int j = 0; j < furnitureCount; j++)
			{
				bool flag = GenericDataSerializer.LoadBool("FURNITURE_DELETED_KEY" + type.ToString() + j);
				if (!(GenericDataSerializer.LoadBool("FURNITURE_PACKED_KEY" + type.ToString() + j) || flag))
				{
					Furniture furniture = SingletonBehaviour<FurniturePool>.Instance.GetFurniture(type);
					furniture.InitializeOldFurniture(j);
					furnitures.Add(furniture);
					AddFurnitureToDictionary(furniture);
					if (furniture.Type == FurnitureType.CHECKOUT_DESK || furniture.Type == FurnitureType.SELF_CHECKOUT)
					{
						SingletonBehaviour<CheckoutDeskManager>.Instance.AddNewCheckoutManager(furniture as CheckoutManager);
					}
				}
			}
		}
	}

	public Furniture GetFirstOven()
	{
		for (int i = 0; i < furnitures.Count; i++)
		{
			if (furnitures[i].Type == FurnitureType.OVEN)
			{
				return furnitures[i];
			}
		}
		return null;
	}

	public Placeable GetFirstBakeryShelf()
	{
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].Type == PlaceableType.BAKERY_SHELF)
			{
				return placeables[i];
			}
		}
		return null;
	}

	private Placeable GetFirstWallShelf()
	{
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].Type == PlaceableType.WALL_SHELF)
			{
				return placeables[i];
			}
		}
		return null;
	}

	private Placeable GetFirstVegetableShelf()
	{
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].Type == PlaceableType.PRODUCE_SHELF_SMALL)
			{
				return placeables[i];
			}
		}
		return null;
	}

	private Placeable GetFirstClothingShelf()
	{
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].Type == PlaceableType.FOLDED_CLOTH_RACK_2)
			{
				return placeables[i];
			}
		}
		return null;
	}

	public (Transform, float) GetTopInitialShelf()
	{
		float num = 2f;
		Transform item;
		switch (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup)
		{
		case ProductGroup.GROCERY:
			item = SingletonBehaviour<SpawnManager>.Instance.GetFirstVegetableShelf().transform;
			break;
		case ProductGroup.BAKERY:
			item = SingletonBehaviour<SpawnManager>.Instance.GetFirstOven().transform;
			break;
		case ProductGroup.TOY:
			item = SingletonBehaviour<SpawnManager>.Instance.GetFirstWallShelf().transform;
			num += 0.5f;
			break;
		case ProductGroup.CLOTHING:
			item = SingletonBehaviour<SpawnManager>.Instance.GetFirstClothingShelf().transform;
			break;
		default:
			item = SingletonBehaviour<SpawnManager>.Instance.GetFirstVegetableShelf().transform;
			break;
		}
		return (item, num);
	}

	public Placeable GetRandomPlaceableToBurn()
	{
		List<Placeable> list = new List<Placeable>();
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].IsBurnable)
			{
				list.Add(placeables[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	public bool HasBurnable()
	{
		for (int i = 0; i < placeables.Count; i++)
		{
			if (placeables[i].IsBurnable)
			{
				return true;
			}
		}
		return false;
	}

	public int GetPlaceableCount(PlaceableType type)
	{
		if (initialProductGroup == ProductGroup.GROCERY && (type == PlaceableType.PRODUCE_SHELF_SMALL || type == PlaceableType.WIDE_SHELF))
		{
			if (type == PlaceableType.PRODUCE_SHELF_SMALL)
			{
				if (GameManager.GetSaveVersion() >= 2)
				{
					return GenericDataSerializer.LoadInt(PLACEABLE_COUNT_KEY + type, 1);
				}
				return GenericDataSerializer.LoadInt(PLACEABLE_COUNT_KEY + type);
			}
			return GenericDataSerializer.LoadInt(PLACEABLE_COUNT_KEY + type, 1);
		}
		if (initialProductGroup == ProductGroup.BAKERY && type == PlaceableType.BAKERY_SHELF)
		{
			return GenericDataSerializer.LoadInt(PLACEABLE_COUNT_KEY + type, 1);
		}
		if (initialProductGroup == ProductGroup.TOY && type == PlaceableType.WALL_SHELF)
		{
			return GenericDataSerializer.LoadInt(PLACEABLE_COUNT_KEY + type, 1);
		}
		if (initialProductGroup == ProductGroup.CLOTHING && type == PlaceableType.FOLDED_CLOTH_RACK_2)
		{
			return GenericDataSerializer.LoadInt(PLACEABLE_COUNT_KEY + type, 1);
		}
		return GenericDataSerializer.LoadInt(PLACEABLE_COUNT_KEY + type);
	}

	public int GetFurnitureCount(FurnitureType type)
	{
		if (initialProductGroup == ProductGroup.BAKERY && type == FurnitureType.OVEN)
		{
			return GenericDataSerializer.LoadInt(FURNITURE_COUNT_KEY + type, 1);
		}
		if (type == FurnitureType.CHECKOUT_DESK)
		{
			return GenericDataSerializer.LoadInt(FURNITURE_COUNT_KEY + type, 1);
		}
		if (GameManager.GetSaveVersion() >= 3 && type == FurnitureType.WALK_IN_FREEZER_SMALL)
		{
			return GenericDataSerializer.LoadInt(FURNITURE_COUNT_KEY + type, 1);
		}
		return GenericDataSerializer.LoadInt(FURNITURE_COUNT_KEY + type);
	}

	public void AddNewPlaceable(Placeable newPlaceable)
	{
		placeables.Add(newPlaceable);
		if (newPlaceable.IsBakeryPlaceable())
		{
			bakeryShelfPlaceables.Add(newPlaceable);
		}
		if (newPlaceable.Type == PlaceableType.FISH_STAND)
		{
			fishStandPlaceables.Add(newPlaceable as ChoppingStandPlaceable);
		}
		GenericDataSerializer.SaveInt(PLACEABLE_COUNT_KEY + newPlaceable.Type, GetPlaceableCount(newPlaceable.Type) + 1);
	}

	public void AddOldPlaceable(Placeable placeable)
	{
		placeables.Add(placeable);
		if (placeable.IsBakeryPlaceable())
		{
			bakeryShelfPlaceables.Add(placeable);
		}
		if (placeable.Type == PlaceableType.FISH_STAND)
		{
			fishStandPlaceables.Add(placeable as ChoppingStandPlaceable);
		}
	}

	public void AddOldFurniture(Furniture furniture)
	{
		furnitures.Add(furniture);
		AddFurnitureToDictionary(furniture);
		if (furniture.Type == FurnitureType.CHECKOUT_DESK || furniture.Type == FurnitureType.SELF_CHECKOUT)
		{
			SingletonBehaviour<CheckoutDeskManager>.Instance.AddNewCheckoutManager(furniture as CheckoutManager);
		}
	}

	public void AddNewFurniture(Furniture newFurniture)
	{
		furnitures.Add(newFurniture);
		AddFurnitureToDictionary(newFurniture);
		if (newFurniture.Type == FurnitureType.CHECKOUT_DESK || newFurniture.Type == FurnitureType.SELF_CHECKOUT)
		{
			SingletonBehaviour<CheckoutDeskManager>.Instance.AddNewCheckoutManager(newFurniture as CheckoutManager);
		}
		GenericDataSerializer.SaveInt(FURNITURE_COUNT_KEY + newFurniture.Type, GetFurnitureCount(newFurniture.Type) + 1);
	}

	public void SetPlaceableCount(PlaceableType type, int count)
	{
		GenericDataSerializer.SaveInt(PLACEABLE_COUNT_KEY + type, count);
	}

	private void AddFurnitureToDictionary(Furniture furniture)
	{
		if (!furnituresDictionary.ContainsKey(furniture.Type))
		{
			furnituresDictionary[furniture.Type] = new List<Furniture>();
		}
		furnituresDictionary[furniture.Type].Add(furniture);
		furnituresDictionary[furniture.Type].Sort((Furniture a, Furniture b) => a.GetDisplayedID().CompareTo(b.GetDisplayedID()));
	}

	public int GetLowestAvailableDisplayedID(FurnitureType type)
	{
		int num = int.MaxValue;
		if (!furnituresDictionary.ContainsKey(type))
		{
			return 1;
		}
		List<Furniture> list = furnituresDictionary[type];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GetDisplayedID() != i + 1)
			{
				num = i + 1;
				break;
			}
		}
		if (num == int.MaxValue)
		{
			num = list.Count + 1;
		}
		return num;
	}
}
