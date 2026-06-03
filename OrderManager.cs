using System;
using System.Collections.Generic;
using System.Linq;
using DFTGames.Localization;
using UnityEngine;

public class OrderManager : SingletonBehaviour<OrderManager>
{
	public struct OrderData(ProductType type, int count, int arrivalTime, OrderReceivingArea receivingArea)
	{
		public ProductType type = type;

		public int count = count;

		public int arrivalTime = arrivalTime;

		public OrderReceivingArea receivingArea = receivingArea;
	}

	public enum OrderReceivingArea
	{
		STORE_FRONT,
		LOADING_DOCK_1,
		LOADING_DOCK_2,
		LOADING_DOCK_3,
		LOADING_DOCK_4,
		COUNT
	}

	[SerializeField]
	private Truck truck;

	private const string TIMED_ORDERS_KEY = "TIMED_ORDERS";

	private Dictionary<OrderReceivingArea, List<BuyPanel.CartSlot>> orderDictionary = new Dictionary<OrderReceivingArea, List<BuyPanel.CartSlot>>();

	private WaitForSeconds orderWaiter = new WaitForSeconds(1f);

	private string FIRST_ORDER_GIVEN = "firstOrder";

	private bool firstOrderGiven;

	private string ORDER_COUNT_FOR_DAY = "OrderCountForDayKey";

	private int orderCountForDay;

	private string makeOldSavesPallettedKey = "makeOldSavesPallettedKey";

	private string orderedAmountKey = "orderedAmountKey";

	private Dictionary<ProductType, int> lastSelectedProductAmounts = new Dictionary<ProductType, int>();

	private Dictionary<ProductType, int> orderedAmountDictionary = new Dictionary<ProductType, int>();

	public static Dictionary<ProductGroup, int> bulkDiscounts = new Dictionary<ProductGroup, int>
	{
		{
			ProductGroup.GROCERY,
			10
		},
		{
			ProductGroup.BAKERY,
			12
		},
		{
			ProductGroup.TOY,
			15
		},
		{
			ProductGroup.CLOTHING,
			15
		},
		{
			ProductGroup.ELECTRONICS,
			6
		},
		{
			ProductGroup.MUSIC,
			8
		},
		{
			ProductGroup.SPORTS,
			12
		},
		{
			ProductGroup.VENDING,
			10
		},
		{
			ProductGroup.FISH,
			10
		}
	};

	public Dictionary<ProductType, int> LastSelectedProductAmounts => lastSelectedProductAmounts;

	private new void Awake()
	{
		base.Awake();
		firstOrderGiven = GenericDataSerializer.LoadBool(FIRST_ORDER_GIVEN);
		orderCountForDay = GenericDataSerializer.LoadInt(ORDER_COUNT_FOR_DAY);
		orderedAmountDictionary = GenericDataSerializer.Load(orderedAmountKey, new Dictionary<ProductType, int>());
		if (!GenericDataSerializer.LoadBool(makeOldSavesPallettedKey))
		{
			GenericDataSerializer.SaveBool(makeOldSavesPallettedKey, value: true);
			for (int i = 0; i < 5; i++)
			{
				OrderReceivingArea orderReceivingArea = (OrderReceivingArea)i;
				if (orderReceivingArea != OrderReceivingArea.STORE_FRONT)
				{
					List<BuyPanel.CartSlot> list = GenericDataSerializer.Load("TIMED_ORDERS" + orderReceivingArea, new List<BuyPanel.CartSlot>());
					for (int j = 0; j < list.Count; j++)
					{
						list[j] = new BuyPanel.CartSlot(list[j].type, list[j].amount, true);
					}
					GenericDataSerializer.Save("TIMED_ORDERS" + orderReceivingArea, list);
				}
			}
		}
		for (int k = 0; k < 5; k++)
		{
			OrderReceivingArea key = (OrderReceivingArea)k;
			orderDictionary.Add(key, GenericDataSerializer.Load("TIMED_ORDERS" + key, new List<BuyPanel.CartSlot>()));
		}
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, delegate
		{
			orderCountForDay = 0;
			GenericDataSerializer.SaveInt(ORDER_COUNT_FOR_DAY, orderCountForDay);
		});
		EventManager.AddListener<OrderReceivingArea>(GameEvents.TRUCK_ARRIVED, SpawnTruckOrders);
		EventManager.AddListener<OrderReceivingArea>(GameEvents.TRUCK_DISAPPEARED, CheckAndUpdateOrder);
	}

	private void Start()
	{
		for (int i = 0; i < 5; i++)
		{
			OrderReceivingArea receivingArea = (OrderReceivingArea)i;
			CheckAndUpdateOrder(receivingArea);
		}
	}

	public List<BuyPanel.CartSlot> GetOrders(OrderReceivingArea receivingArea)
	{
		return orderDictionary[receivingArea];
	}

	public bool CanAddOrder(List<BuyPanel.CartSlot> orders)
	{
		for (int i = 0; i < orders.Count; i++)
		{
			ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(orders[i].type);
			if (anyProductData.maxAmountAllowed != -1)
			{
				int maxAmountAllowed = anyProductData.maxAmountAllowed;
				int num = (orderedAmountDictionary.ContainsKey(orders[i].type) ? (orderedAmountDictionary[orders[i].type] + orders[i].amount) : orders[i].amount);
				if (maxAmountAllowed < num)
				{
					string word = Locale.GetWord(anyProductData.type.ToString());
					SingletonBehaviour<TooltipUI>.Instance.ShowTimedErrorWithFullText(Locale.GetWord("limit_reached").Replace("{0}", word), base.transform);
					return false;
				}
			}
		}
		return true;
	}

	public void ProductRecycled(ProductType type)
	{
		if (orderedAmountDictionary.ContainsKey(type))
		{
			orderedAmountDictionary[type]--;
		}
		GenericDataSerializer.Save(orderedAmountKey, orderedAmountDictionary);
		EventManager.NotifyEvent(GameEvents.LIMITED_PRODUCT_COUNT_UPDATED, type);
	}

	public void AddOrder(List<BuyPanel.CartSlot> orders, OrderReceivingArea receivingArea)
	{
		for (int i = 0; i < orders.Count; i++)
		{
			if ((orders[i].type <= ProductType.WAREHOUSE_EQUIPMENT_START || orders[i].type >= ProductType.WAREHOUSE_EQUIPMENT_END) && SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(orders[i].type).maxAmountAllowed != -1)
			{
				if (orderedAmountDictionary.ContainsKey(orders[i].type))
				{
					orderedAmountDictionary[orders[i].type] += orders[i].amount;
				}
				else
				{
					orderedAmountDictionary.Add(orders[i].type, orders[i].amount);
				}
			}
			orderDictionary[receivingArea].Add(orders[i]);
			GenericDataSerializer.Save(orderedAmountKey, orderedAmountDictionary);
			EventManager.NotifyEvent(GameEvents.LIMITED_PRODUCT_COUNT_UPDATED, orders[i].type);
		}
		orderCountForDay += orders.Count;
		GenericDataSerializer.SaveInt(ORDER_COUNT_FOR_DAY, orderCountForDay);
		SaveOrders(receivingArea);
		CheckAndUpdateOrder(receivingArea);
	}

	public int GetOrderedAmount(ProductType type)
	{
		if (!orderedAmountDictionary.ContainsKey(type))
		{
			return 0;
		}
		return orderedAmountDictionary[type];
	}

	private void CheckAndUpdateOrder(OrderReceivingArea receivingArea)
	{
		if (orderDictionary[receivingArea].Count != 0)
		{
			Truck truck = SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea);
			if (!truck.IsActive)
			{
				truck.Activate();
				UpdateOrdersToBeSpawned(receivingArea);
			}
		}
	}

	public OrderReceivingArea GetMostAvailableOrderReceivingArea()
	{
		int num = int.MaxValue;
		OrderReceivingArea orderReceivingArea = OrderReceivingArea.LOADING_DOCK_1;
		for (int i = 0; i < 5; i++)
		{
			if (!SingletonBehaviour<StorageManager>.Instance.IsDockUnlocked((OrderReceivingArea)i) || i == 0)
			{
				continue;
			}
			OrderReceivingArea orderReceivingArea2 = (OrderReceivingArea)i;
			int count = orderDictionary[orderReceivingArea2].Count;
			if (count == num)
			{
				bool isActive = SingletonBehaviour<TruckManager>.Instance.GetTruck(orderReceivingArea).IsActive;
				if (!SingletonBehaviour<TruckManager>.Instance.GetTruck(orderReceivingArea2).IsActive && isActive)
				{
					orderReceivingArea = orderReceivingArea2;
				}
			}
			else if (count < num)
			{
				num = count;
				orderReceivingArea = orderReceivingArea2;
			}
		}
		return orderReceivingArea;
	}

	private void UpdateOrdersToBeSpawned(OrderReceivingArea receivingArea)
	{
		List<BuyPanel.CartSlot> list = orderDictionary[receivingArea];
		Truck obj = SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea);
		int count = obj.TargetPallets.Count;
		int targetBoxSlotCountPerPallet = obj.TargetBoxSlotCountPerPallet;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].isPallet)
			{
				num++;
			}
			else
			{
				num2 += list[i].amount;
			}
			int num3 = (num2 + targetBoxSlotCountPerPallet - 1) / targetBoxSlotCountPerPallet;
			if (num + num3 > count || (i > 0 && list[i].type > ProductType.WAREHOUSE_EQUIPMENT_START))
			{
				break;
			}
			if (i == 0 && list[i].type > ProductType.WAREHOUSE_EQUIPMENT_START)
			{
				BuyPanel.CartSlot value = list[i];
				value.isInTruck = true;
				orderDictionary[receivingArea][i] = value;
				break;
			}
			BuyPanel.CartSlot value2 = list[i];
			value2.isInTruck = true;
			orderDictionary[receivingArea][i] = value2;
		}
		EventManager.NotifyEvent(GameEvents.ORDERS_UPDATED, receivingArea);
	}

	private void SpawnTruckOrders(OrderReceivingArea receivingArea)
	{
		List<BuyPanel.CartSlot> list = orderDictionary[receivingArea];
		if (receivingArea == OrderReceivingArea.STORE_FRONT)
		{
			List<Transform> spawnPositions = SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea).SpawnPositions;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				BuyPanel.CartSlot cartSlot = list[i];
				int num3 = 0;
				for (int j = 0; j < cartSlot.amount; j++)
				{
					if (num >= spawnPositions.Count)
					{
						break;
					}
					SpawnOrder(cartSlot.type, spawnPositions[num]);
					num++;
					num3++;
				}
				if (num3 == cartSlot.amount)
				{
					num2++;
				}
				else if (num3 > 0)
				{
					list[i] = new BuyPanel.CartSlot(cartSlot.type, cartSlot.amount - num3);
					break;
				}
				if (num == spawnPositions.Count)
				{
					break;
				}
			}
			if (num2 > 0)
			{
				orderDictionary[receivingArea].RemoveRange(0, num2);
			}
		}
		else
		{
			EventLogger.FirstDockDelivered();
			EventLogger.LogEvent("c_order_received_" + receivingArea);
			int num4 = 0;
			Truck truck = SingletonBehaviour<TruckManager>.Instance.GetTruck(receivingArea);
			List<Pallet> targetPallets = truck.TargetPallets;
			int num5 = 0;
			List<BuyPanel.CartSlot> list2 = new List<BuyPanel.CartSlot>();
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].isInTruck)
				{
					list2.Add(list[k]);
				}
			}
			list2 = list2.OrderByDescending((BuyPanel.CartSlot s) => s.isPallet).ToList();
			for (int num6 = 0; num6 < list2.Count; num6++)
			{
				BuyPanel.CartSlot cartSlot2 = list2[num6];
				if (cartSlot2.type > ProductType.WAREHOUSE_EQUIPMENT_START)
				{
					Transform vehicleSpawnPoint = truck.VehicleSpawnPoint;
					VehichleInteractable vehicleTransform = SingletonBehaviour<VehicleManager>.Instance.GetVehicleTransform(cartSlot2.type);
					vehicleTransform.SetPosition(vehicleSpawnPoint.position, vehicleSpawnPoint.eulerAngles);
					vehicleTransform.gameObject.SetActive(value: true);
					num5++;
					break;
				}
				ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(cartSlot2.type);
				List<Vector3> localPositions = SingletonBehaviour<PalletManager>.Instance.GetBoxPositions(anyProductData.boxType).localPositions;
				List<Vector3> localEulers = SingletonBehaviour<PalletManager>.Instance.GetBoxPositions(anyProductData.boxType).localEulers;
				if (cartSlot2.isPallet)
				{
					Pallet pallet = SingletonBehaviour<PalletManager>.Instance.SpawnPallet(PalletType.BLUE_PALLET, targetPallets[num6].transform.position, targetPallets[num6].transform.rotation.eulerAngles);
					for (int num7 = 0; num7 < cartSlot2.amount; num7++)
					{
						if (num7 < localPositions.Count)
						{
							Box box = SpawnOrder(cartSlot2.type, localPositions[num7], localEulers[num7]);
							box.SaveLocation();
							pallet.AddBoxInstant(box);
						}
					}
					pallet.SaveLocation(checkForRestockingZone: true);
					num4 += this.truck.TargetBoxSlotCountPerPallet;
				}
				else
				{
					for (int num8 = 0; num8 < cartSlot2.amount; num8++)
					{
						Transform transform = truck.SpawnPositions[num4];
						SpawnOrder(cartSlot2.type, transform.position, transform.eulerAngles).SaveLocation(checkForRestockingZone: true);
						num4++;
					}
				}
				num5++;
			}
			if (num5 == 0)
			{
				return;
			}
			orderDictionary[receivingArea].RemoveRange(0, num5);
		}
		SaveOrders(receivingArea);
	}

	private Box SpawnOrder(ProductType productType, Vector3 position, Vector3 rotation)
	{
		if (SingletonBehaviour<ProductPool>.Instance.HasConsumableData(productType))
		{
			return SingletonBehaviour<BoxManager>.Instance.SpawnBox(productType, position, rotation);
		}
		if (productType >= ProductType.FURNITURE_START)
		{
			return SingletonBehaviour<BoxManager>.Instance.SpawnBox(ProductPool.ProductTypeToFurnitureType[productType], position, rotation);
		}
		return SingletonBehaviour<BoxManager>.Instance.SpawnBox(ProductPool.ProductTypeToPlaceableType[productType], position, rotation);
	}

	private void SpawnOrder(ProductType productType, Transform spawnTransform, int offset = 0)
	{
		if (productType >= ProductType.FURNITURE_START)
		{
			SingletonBehaviour<BoxManager>.Instance.SpawnBox(ProductPool.ProductTypeToFurnitureType[productType], spawnTransform, offset);
		}
		else if (productType >= ProductType.NONCONSUMABLE_START)
		{
			SingletonBehaviour<BoxManager>.Instance.SpawnBox(ProductPool.ProductTypeToPlaceableType[productType], spawnTransform, offset);
		}
		else
		{
			SingletonBehaviour<BoxManager>.Instance.SpawnBox(productType, spawnTransform, offset);
		}
	}

	public static int EpochTime()
	{
		return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
	}

	private void SaveOrders(OrderReceivingArea orderReceivingArea)
	{
		GenericDataSerializer.Save("TIMED_ORDERS" + orderReceivingArea, orderDictionary[orderReceivingArea]);
		EventManager.NotifyEvent(GameEvents.ORDERS_UPDATED, orderReceivingArea);
	}
}
