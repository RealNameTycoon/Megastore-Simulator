using System;
using System.Collections.Generic;
using DFTGames.Localization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class VehicleManager : SingletonBehaviour<VehicleManager>
{
	[SerializeField]
	private PlayerMove playerMove;

	[SerializeField]
	private Image dotImage;

	[SerializeField]
	private LayerMask palletCarrierVehicleOverlapLayers;

	[SerializeField]
	private LayerMask boxCarrierVehicleOverlapLayers;

	[SerializeField]
	private SerializedDictionary<ProductType, VehichleInteractable> productTypeToTransformMap;

	private const string VEHICLE_PURCHASED_KEY = "VEHICLE_PURCHASED_KEY";

	private VehichleInteractable takenVehicle;

	public bool isOnHandtruck;

	public bool isOnPallettruck;

	private Dictionary<VehicleType, int> vehicleIDToPriceMap = new Dictionary<VehicleType, int>
	{
		{
			VehicleType.HAND_TRUCK,
			1000
		},
		{
			VehicleType.PLATFORM_CART,
			2500
		},
		{
			VehicleType.PALLET_JACK,
			2000
		},
		{
			VehicleType.FORKLIFT,
			15000
		},
		{
			VehicleType.REACH_TRUCK,
			25000
		},
		{
			VehicleType.ELECTRIC_PALLET_JACK,
			8000
		},
		{
			VehicleType.PALLET_ROBOT,
			75000
		},
		{
			VehicleType.PALLET_STACKER,
			10000
		}
	};

	private Dictionary<VehicleType, int> vehicleTypeToRequiredLevelMap = new Dictionary<VehicleType, int>
	{
		{
			VehicleType.HAND_TRUCK,
			0
		},
		{
			VehicleType.PLATFORM_CART,
			0
		},
		{
			VehicleType.PALLET_JACK,
			0
		},
		{
			VehicleType.FORKLIFT,
			5
		},
		{
			VehicleType.REACH_TRUCK,
			10
		},
		{
			VehicleType.ELECTRIC_PALLET_JACK,
			5
		},
		{
			VehicleType.PALLET_ROBOT,
			15
		},
		{
			VehicleType.PALLET_STACKER,
			5
		}
	};

	private Dictionary<VehicleType, ProductType> vehicleTypeToDeliveryProductMap = new Dictionary<VehicleType, ProductType>
	{
		{
			VehicleType.HAND_TRUCK,
			ProductType.HAND_TRUCK
		},
		{
			VehicleType.PLATFORM_CART,
			ProductType.PLATFORM_CART
		},
		{
			VehicleType.PALLET_JACK,
			ProductType.PALLET_JACK
		},
		{
			VehicleType.FORKLIFT,
			ProductType.FORKLIFT
		},
		{
			VehicleType.REACH_TRUCK,
			ProductType.REACH_TRUCK
		},
		{
			VehicleType.ELECTRIC_PALLET_JACK,
			ProductType.ELECTRIC_PALLET_JACK
		},
		{
			VehicleType.PALLET_ROBOT,
			ProductType.PALLET_ROBOT
		},
		{
			VehicleType.PALLET_STACKER,
			ProductType.PALLET_STACKER
		}
	};

	private Dictionary<VehicleType, int> vehicleTypeToResetDockMap = new Dictionary<VehicleType, int>
	{
		{
			VehicleType.HAND_TRUCK,
			-1
		},
		{
			VehicleType.PALLET_JACK,
			0
		},
		{
			VehicleType.FORKLIFT,
			1
		},
		{
			VehicleType.REACH_TRUCK,
			2
		},
		{
			VehicleType.ELECTRIC_PALLET_JACK,
			0
		},
		{
			VehicleType.PALLET_ROBOT,
			0
		},
		{
			VehicleType.PALLET_STACKER,
			1
		},
		{
			VehicleType.PLATFORM_CART,
			-1
		}
	};

	public bool IsOnVehicle => takenVehicle != null;

	public bool IsOnHandTruck => isOnHandtruck;

	public bool IsOnPlatformCart
	{
		get
		{
			if (takenVehicle != null)
			{
				return takenVehicle.VehicleType == VehicleType.PLATFORM_CART;
			}
			return false;
		}
	}

	public bool IsAnimating
	{
		get
		{
			if (takenVehicle != null)
			{
				return takenVehicle.IsAnimating();
			}
			return false;
		}
	}

	public VehichleInteractable TakenVehicle => takenVehicle;

	public LayerMask PalletCarrierVehicleOverlapLayers => palletCarrierVehicleOverlapLayers;

	public LayerMask BoxCarrierVehicleOverlapLayers => boxCarrierVehicleOverlapLayers;

	private new void Awake()
	{
		base.Awake();
		foreach (KeyValuePair<ProductType, VehichleInteractable> item in productTypeToTransformMap)
		{
			item.Value.SetResetPosition();
		}
	}

	private void ActivatePurchasedVehicles()
	{
		foreach (KeyValuePair<VehicleType, ProductType> item in vehicleTypeToDeliveryProductMap)
		{
			if (IsVehiclePurchased(item.Key, 0))
			{
				ProductType key = vehicleTypeToDeliveryProductMap[item.Key];
				productTypeToTransformMap[key].gameObject.SetActive(value: true);
			}
		}
	}

	public void ActivatePalletJack()
	{
		ProductType key = vehicleTypeToDeliveryProductMap[VehicleType.PALLET_JACK];
		productTypeToTransformMap[key].gameObject.SetActive(value: true);
	}

	public void UpdateMenu()
	{
		if (takenVehicle != null)
		{
			takenVehicle.RefreshButtons();
		}
	}

	public void UpdateMenuBoxHovered()
	{
		if (takenVehicle != null)
		{
			takenVehicle.RefreshButtonsBoxHovered();
		}
	}

	public void UpdateMenuPalletHovered(Action palletTakeAction)
	{
		if (takenVehicle != null)
		{
			takenVehicle.RefreshButtonsPalletHovered(palletTakeAction);
		}
	}

	private void Start()
	{
		EventManager.AddListener<VehichleInteractable>(GameEvents.VEHICLE_TAKEN, OnVehicleEquipped);
		EventManager.AddListener(GameEvents.VEHICLE_RELEASED, OnVehicleUnequipped);
		ActivatePurchasedVehicles();
	}

	private void OnVehicleEquipped(VehichleInteractable takenVehicle)
	{
		this.takenVehicle = takenVehicle;
		isOnHandtruck = takenVehicle.VehicleType == VehicleType.HAND_TRUCK;
		isOnPallettruck = takenVehicle.VehicleType == VehicleType.PALLET_JACK;
		if (isOnHandtruck)
		{
			playerMove.EnableHandTruckCollider();
		}
		if (takenVehicle.VehicleType == VehicleType.PLATFORM_CART)
		{
			playerMove.EnablePlatformCartCollider();
		}
		if (isOnPallettruck || takenVehicle.VehicleType == VehicleType.ELECTRIC_PALLET_JACK || takenVehicle.VehicleType == VehicleType.PALLET_STACKER)
		{
			playerMove.EnablPalletTruckCollider();
		}
	}

	private void OnVehicleUnequipped()
	{
		takenVehicle = null;
		isOnHandtruck = false;
		isOnPallettruck = false;
		playerMove.UpdateRadius(objectPicked: false);
	}

	public void TurnVehicleWheels()
	{
		if (takenVehicle != null)
		{
			takenVehicle.StartAnimation();
		}
	}

	public void StopVehicleWheels()
	{
		if (takenVehicle != null)
		{
			takenVehicle.StopAnimation();
		}
	}

	public bool IsBoxContainedInVehicle(int boxID)
	{
		if (takenVehicle != null)
		{
			return takenVehicle.ContainsBox(boxID);
		}
		return false;
	}

	public bool PlaceBoxToVehicle(Box box)
	{
		if (takenVehicle != null)
		{
			return takenVehicle.PlaceBoxByClick(box);
		}
		return false;
	}

	public Transform GetVehicleTurningPoint()
	{
		if (takenVehicle == null)
		{
			return null;
		}
		return takenVehicle.GetTurningAnchor();
	}

	public float GetSpeedMultiplier()
	{
		if (takenVehicle == null)
		{
			return 1f;
		}
		return takenVehicle.SpeedMultiplier();
	}

	public bool IsVehiclePurchased(VehicleType vehicleType, int vehicleID)
	{
		bool value = false;
		if (vehicleType == VehicleType.HAND_TRUCK || vehicleType == VehicleType.PALLET_JACK)
		{
			value = true;
		}
		return GenericDataSerializer.LoadBool("VEHICLE_PURCHASED_KEY" + vehicleType.ToString() + vehicleID, value);
	}

	public int GetVehiclePrice(VehicleType vehicleType)
	{
		return vehicleIDToPriceMap[vehicleType];
	}

	public int GetVehicleRequiredLevel(VehicleType vehicleType)
	{
		return vehicleTypeToRequiredLevelMap[vehicleType];
	}

	public void PurchaseVehicle(VehicleType vehicleType, int vehicleID)
	{
		GenericDataSerializer.SaveBool("VEHICLE_PURCHASED_KEY" + vehicleType.ToString() + vehicleID, value: true);
	}

	public bool CanSellVehicle(VehicleType vehicleType, int vehicleID)
	{
		VehichleInteractable vehicleTransform = GetVehicleTransform(GetDeliveryProduct(vehicleType));
		if (!vehicleTransform.CanSell())
		{
			return false;
		}
		bool activeSelf = vehicleTransform.gameObject.activeSelf;
		if (!activeSelf)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("vehicle_not_available", base.transform);
		}
		return activeSelf;
	}

	public void SellVehicle(VehicleType vehicleType, int vehicleID)
	{
		VehichleInteractable vehicleTransform = GetVehicleTransform(GetDeliveryProduct(vehicleType));
		GenericDataSerializer.Save("VEHICLE_PURCHASED_KEY" + vehicleType.ToString() + vehicleID, dataToSave: false);
		vehicleTransform.gameObject.SetActive(value: false);
		vehicleTransform.OnSell();
	}

	public void ResetVehicle(VehicleType vehicleType, int vehicleID)
	{
		if (takenVehicle != null && takenVehicle.VehicleType == vehicleType && takenVehicle.VehicleID == vehicleID)
		{
			return;
		}
		VehichleInteractable vehicleTransform = GetVehicleTransform(GetDeliveryProduct(vehicleType));
		if (vehicleTransform.gameObject.activeSelf)
		{
			vehicleTransform.ResetToInitialPosition();
			int num = vehicleTypeToResetDockMap[vehicleType];
			string word = Locale.GetWord(vehicleType.ToString());
			if (num == -1)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltipWithFullText(Locale.GetWord("vehicle_reset_store_front_n").Replace("{0}", word).ToString(), base.transform);
			}
			else
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedTooltipWithFullText(Locale.GetWord("vehicle_reset_info_n").Replace("{0}", word).Replace("{1}", (num + 1).ToString()), vehicleTransform.transform);
			}
		}
	}

	public void ResetVehicleToPosition(VehicleType vehicleType, int vehicleID, Vector3 position = default(Vector3), Vector3 eulerAngles = default(Vector3))
	{
		Transform transform = GetVehicleTransform(GetDeliveryProduct(vehicleType)).transform;
		if (vehicleType == VehicleType.FORKLIFT || vehicleType == VehicleType.REACH_TRUCK)
		{
			transform.position = position;
			transform.eulerAngles = eulerAngles;
		}
	}

	public ProductType GetDeliveryProduct(VehicleType vehicleType)
	{
		return vehicleTypeToDeliveryProductMap[vehicleType];
	}

	public VehichleInteractable GetVehicleTransform(ProductType productType)
	{
		return productTypeToTransformMap[productType];
	}
}
