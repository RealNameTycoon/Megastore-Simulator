using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Box : Clickable, MoveableObjectInterface
{
	[SerializeField]
	private BoxCollider collider;

	[SerializeField]
	private Rigidbody rigidBody;

	[SerializeField]
	private Transform corner1;

	[SerializeField]
	private Transform corner2;

	[SerializeField]
	private bool hasNoCover;

	[SerializeField]
	private Animation openCloseAnimation;

	[SerializeField]
	private BoxType type;

	[SerializeField]
	private GameObject productsCover;

	[SerializeField]
	private MeshRenderer productsCoverMeshRenderer;

	[SerializeField]
	private Moveable moveable;

	[SerializeField]
	private GameObject solidObject;

	[SerializeField]
	private MeshRenderer boxMeshStaticClosed;

	[SerializeField]
	private SkinnedMeshRenderer boxMeshAnimated;

	[SerializeField]
	private Transform customLid;

	[SerializeField]
	private Transform customLidTargetOpen;

	[SerializeField]
	private ParticleSystem flyParticles;

	private RestockZone restockZone;

	[SerializeField]
	private TemperatureZone temperatureZone;

	private Action onPlacementEnded;

	[SerializeField]
	private int boxID = -1;

	private List<Vector3> productLocations;

	private const float COVER_ROTATION_SPEED_CLOSE = 1200f;

	private const float COVER_ROTATION_SPEED = 600f;

	private bool isOpen;

	private List<Product> containedProducts = new List<Product>();

	private PlaceableType containedPlaceableType = PlaceableType.NONE;

	private FurnitureType containedFurnitureType = FurnitureType.NONE;

	private Vector3 rigidBodyVelocity = Vector3.zero;

	private bool isMoving;

	private const string BOX_POSITION_KEY = "boxPosition";

	private const string BOX_ROTATION_KEY = "boxRotation";

	private const string BOX_PRODUCT_TYPE_KEY = "boxProductType";

	private const string BOX_PRODUCT_COUNT_KEY = "boxProductCount";

	public const string BOX_PLACEABLE_TYPE_KEY = "boxPlaceableType";

	private const string BOX_FURNITURE_TYPE_KEY = "boxFurnitureType";

	private const string BOX_IS_OPEN_KEY = "boxIsOpen";

	private const string BOX_IS_STORED = "boxIsStored";

	private const string BOX_IS_IN_RETURN_AREA = "boxIsInReturnArea";

	private const string RESERVED_EMPLOYEE_ID_KEY = "reservedEmployeeID";

	private const string PACKABLE_ID_KEY = "PACKABLE_ID_KEY";

	private const string FRESHNESS_KEY = "BOX_FRESHNESS_KEY";

	private const string FROZEN_DAMAGE_KEY = "BOX_FROZEN_DAMAGE_KEY";

	private const string IS_BOX_PERISHABLE_KEY = "IS_BOX_PERISHABLE_KEY";

	private const string IS_BOX_SPOILABLE = "IS_BOX_SPOILABLE";

	private float STEADY_BOX_SPEED = 0.95f;

	private static readonly int SpoilageAmountId = Shader.PropertyToID("_SpoilageAmount");

	private static readonly int FreezeDamageAmountId = Shader.PropertyToID("_FreezeDamageAmount");

	private WaitForFixedUpdate fixedUpdateWaiter = new WaitForFixedUpdate();

	public Action OnItemPlaced;

	private bool stored;

	private bool isInReturnArea;

	private Coroutine EnableRendererRoutine;

	[SerializeField]
	private int reservedForEmployeeID = -1;

	private int containedPalletID = -1;

	[SerializeField]
	private BoxStorageUnit containedBoxStorageUnit;

	private bool isRenderersEnabled = true;

	private Vector3 customLidStartPosition;

	private Vector3 customLidStartRotation;

	private const float CUSTOM_LID_OPEN_ROTATION = -180f;

	[SerializeField]
	private float freshnessSpoilageProgress;

	[SerializeField]
	private float frozenDamageProgress;

	private bool isSpoilable;

	public RestockZone RestockZone => restockZone;

	public bool HasNoCover => hasNoCover;

	public BoxType Type => type;

	public int BoxID => boxID;

	public Rigidbody RigidBody => rigidBody;

	public BoxCollider Collider => collider;

	public PlaceableType ContainedPlaceableType => containedPlaceableType;

	public FurnitureType ContainedFurnitureType => containedFurnitureType;

	public bool Stored => stored;

	public bool IsInReturnArea => isInReturnArea;

	public int ReservedForEmployeeID => reservedForEmployeeID;

	public bool IsReservedForRestocking => reservedForEmployeeID != -1;

	public int ContainedPalletID => containedPalletID;

	public BoxStorageUnit ContainedBoxStorageUnit => containedBoxStorageUnit;

	public float FreshnessSpoilageProgress => freshnessSpoilageProgress;

	public float FrozenDamageProgress => frozenDamageProgress;

	public int ProductCount => GetProductCount();

	public void SetPackableID(int productID)
	{
		GenericDataSerializer.SaveInt("PACKABLE_ID_KEY" + boxID, productID);
	}

	public int GetPackableID()
	{
		return GenericDataSerializer.LoadInt("PACKABLE_ID_KEY" + boxID, -1);
	}

	public void SetReservedForRestocking(int employeeID)
	{
		reservedForEmployeeID = employeeID;
		GenericDataSerializer.SaveInt("reservedEmployeeID" + boxID, employeeID);
	}

	public void ReleaseBoxReservation()
	{
		if (reservedForEmployeeID != -1)
		{
			reservedForEmployeeID = -1;
			GenericDataSerializer.SaveInt("reservedEmployeeID" + boxID, -1);
		}
	}

	public void OnAddedToPallet(int palletID)
	{
		containedPalletID = palletID;
	}

	public void OnRemovedFromPallet()
	{
		containedPalletID = -1;
	}

	public void OnAddedToBoxStorageUnit(BoxStorageUnit boxStorageUnit)
	{
		containedBoxStorageUnit = boxStorageUnit;
	}

	public void OnRemovedFromBoxStorageUnit()
	{
		containedBoxStorageUnit = null;
	}

	private void Start()
	{
		LayerMask placeableFloorLayers = (1 << PlacementManager.FLOOR_LAYER) | (1 << PlacementManager.SERVICE_ROOM_FLOOR_LAYER) | (1 << PlacementManager.STORAGE_FLOOR_LAYER) | (1 << PlacementManager.VEHICLE_FLOOR_LAYER) | (1 << BoxManager.BOX_LAYER) | (1 << PlacementManager.AROUND_STORE_LAYER);
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
		if (customLid != null)
		{
			customLidStartRotation = customLid.localEulerAngles;
			customLidStartPosition = customLid.localPosition;
		}
	}

	public void WakeUpRigidBody()
	{
		rigidBody.WakeUp();
	}

	public void OnBeforeThrow()
	{
		base.gameObject.layer = BoxManager.FLYING_BOX_LAYER;
	}

	public void OnBoxPut()
	{
		if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
		{
			base.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
		}
		else
		{
			base.gameObject.layer = BoxManager.BOX_LAYER;
		}
		SetBoxStored();
	}

	public void OnBoxTakenByNPC()
	{
		UnregisterFromZones();
		if (stored)
		{
			stored = false;
			GenericDataSerializer.SaveBool("boxIsStored" + boxID, value: false);
		}
		SingletonBehaviour<BoxManager>.Instance.UnregisterBox(this);
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
			isMoving = false;
			rigidBody.isKinematic = true;
		}
		else
		{
			StartCoroutine(WaitAndDisablePhysics());
		}
	}

	private IEnumerator WaitAndDisablePhysics()
	{
		yield return fixedUpdateWaiter;
		yield return fixedUpdateWaiter;
		base.gameObject.layer = BoxManager.NOT_RAYCASTING_BOX_LAYER;
		isMoving = false;
		rigidBody.isKinematic = true;
	}

	public void SetBoxStored()
	{
		isMoving = false;
		SaveLocation();
		stored = true;
		rigidBody.isKinematic = true;
		SingletonBehaviour<BoxManager>.Instance.UnregisterBox(this);
		GenericDataSerializer.SaveBool("boxIsStored" + boxID, value: true);
	}

	public void InitializeOldBox(int boxID)
	{
		this.boxID = boxID;
		Vector3 vector;
		Quaternion rotation;
		if (!GenericDataSerializer.HasKey("boxPosition" + boxID))
		{
			vector = SingletonBehaviour<BoxManager>.Instance.GetInstantSpawnPosition(this);
			rotation = SingletonBehaviour<BoxManager>.Instance.TutorialBoxTransform.rotation;
		}
		else
		{
			vector = GenericDataSerializer.Load("boxPosition" + boxID);
			rotation = GenericDataSerializer.LoadQuaternion("boxRotation" + boxID);
			if (vector == Vector3.zero)
			{
				vector = SingletonBehaviour<BoxManager>.Instance.GetInstantSpawnPosition(this);
				rotation = SingletonBehaviour<BoxManager>.Instance.TutorialBoxTransform.rotation;
			}
		}
		int count = GenericDataSerializer.LoadInt("boxProductCount" + boxID);
		ProductType productType = GenericDataSerializer.Load("boxProductType" + boxID, ProductType.NONE);
		bool flag = productType != ProductType.NONE && !SingletonBehaviour<ProductPool>.Instance.HasProductData(productType);
		PlaceableType placeableType = GenericDataSerializer.Load("boxPlaceableType" + boxID, PlaceableType.NONE);
		FurnitureType furnitureType = GenericDataSerializer.Load("boxFurnitureType" + boxID, FurnitureType.NONE);
		isInReturnArea = GenericDataSerializer.LoadBool("boxIsInReturnArea" + boxID);
		GenericDataSerializer.LoadInt("reservedEmployeeID" + boxID, -1);
		stored = GenericDataSerializer.LoadBool("boxIsStored" + boxID);
		isSpoilable = GenericDataSerializer.LoadBool("IS_BOX_SPOILABLE" + boxID);
		if (stored)
		{
			rigidBody.isKinematic = true;
		}
		base.transform.position = vector;
		base.transform.rotation = rotation;
		if (productType != ProductType.NONE && !flag)
		{
			ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(productType);
			if (ShouldActivateProductsCover(productData, count))
			{
				productsCover.SetActive(value: true);
			}
			if (!GenericDataSerializer.HasKey("IS_BOX_PERISHABLE_KEY" + boxID))
			{
				GenericDataSerializer.SaveBool("IS_BOX_PERISHABLE_KEY" + boxID, productData.storageRequirement != StorageRequirement.None);
			}
			FillItems(productData, count);
		}
		else if (furnitureType != FurnitureType.NONE)
		{
			FillWithFurniture(furnitureType);
		}
		else if (placeableType != PlaceableType.NONE)
		{
			FillWithPlaceable(placeableType);
		}
		if (!stored && !isInReturnArea)
		{
			CheckAndUpdateRestockZone();
			SingletonBehaviour<BoxManager>.Instance.RegisterBox(this);
		}
		if (isInReturnArea)
		{
			SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.PlaceBox(this, instant: true);
		}
		freshnessSpoilageProgress = GenericDataSerializer.LoadFloat("BOX_FRESHNESS_KEY" + boxID);
		frozenDamageProgress = GenericDataSerializer.LoadFloat("BOX_FROZEN_DAMAGE_KEY" + boxID);
		bool isPerishable = GenericDataSerializer.LoadBool("IS_BOX_PERISHABLE_KEY" + boxID);
		ChangeBoxMaterial(isPerishable);
		if (freshnessSpoilageProgress > 1f)
		{
			ChangeProductsToSpoiled();
			TryEnableFlyParticles(enable: true);
		}
		if (frozenDamageProgress > 1f)
		{
			ChangeProductsToFrozen();
		}
	}

	private void ChangeBoxMaterial(bool isPerishable)
	{
		if (SingletonBehaviour<BoxManager>.Instance.ContainsPerishableMaterial(type))
		{
			if (isPerishable)
			{
				boxMeshStaticClosed.sharedMaterial = SingletonBehaviour<BoxManager>.Instance.GetBoxPerishableMaterial(type);
				boxMeshAnimated.sharedMaterial = SingletonBehaviour<BoxManager>.Instance.GetBoxPerishableMaterial(type);
			}
			else
			{
				boxMeshStaticClosed.sharedMaterial = SingletonBehaviour<BoxManager>.Instance.GetBoxOriginalMaterial(type);
				boxMeshAnimated.sharedMaterial = SingletonBehaviour<BoxManager>.Instance.GetBoxOriginalMaterial(type);
			}
		}
	}

	public float GetHeight()
	{
		return collider.size.y * 1.25f;
	}

	public float GetWidth()
	{
		return collider.size.x * 1.25f;
	}

	public float GetLength()
	{
		return collider.size.z * 1.25f;
	}

	public void Initialize(int boxID, bool isPerishable = false)
	{
		this.boxID = boxID;
		ChangeBoxMaterial(isPerishable);
		isSpoilable = true;
		GenericDataSerializer.SaveBool("IS_BOX_SPOILABLE" + boxID, isSpoilable);
		GenericDataSerializer.SaveBool("IS_BOX_PERISHABLE_KEY" + boxID, isPerishable);
	}

	public void SaveLocation(bool checkForRestockingZone = false)
	{
		if (checkForRestockingZone)
		{
			CheckAndUpdateRestockZone();
			CheckAndUpdateTemperatureZone();
		}
		GenericDataSerializer.Save("boxPosition" + boxID, base.transform.position);
		GenericDataSerializer.Save("boxRotation" + boxID, base.transform.rotation);
	}

	private void CheckAndUpdateRestockZone()
	{
		RestockZone restockZoneAtPosition = SingletonBehaviour<RestockZoneManager>.Instance.GetRestockZoneAtPosition(base.transform.position);
		if (!(restockZone == restockZoneAtPosition))
		{
			UnregisterFromRestockZone();
			restockZone = restockZoneAtPosition;
			if (restockZone != null)
			{
				restockZone.RegisterBox(this);
			}
		}
	}

	public void CheckAndUpdateTemperatureZone()
	{
		TemperatureZone temperatureZoneAtPosition = SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(base.transform.position);
		if (!(temperatureZone == temperatureZoneAtPosition))
		{
			UnregisterFromTemperatureZone();
			temperatureZone = temperatureZoneAtPosition;
			if (temperatureZone != null)
			{
				temperatureZone.RegisterBox(this);
			}
		}
	}

	public void RegisterToRestockZone(RestockZone restockZone)
	{
		UnregisterFromRestockZone();
		this.restockZone = restockZone;
		restockZone.RegisterBox(this);
	}

	public void UnregisterFromRestockZone()
	{
		if (restockZone != null)
		{
			restockZone.UnregisterBox(this);
			restockZone = null;
		}
	}

	public void RegisterToTemperatureZone(TemperatureZone temperatureZone)
	{
		UnregisterFromTemperatureZone();
		this.temperatureZone = temperatureZone;
		temperatureZone.RegisterBox(this);
	}

	public void UnregisterFromTemperatureZone()
	{
		if (temperatureZone != null)
		{
			temperatureZone.UnregisterBox(this);
			temperatureZone = null;
		}
	}

	private void UnregisterFromZones()
	{
		UnregisterFromRestockZone();
		UnregisterFromTemperatureZone();
		if (isInReturnArea)
		{
			SingletonBehaviour<RestockZoneManager>.Instance.UnregisterReturnAreaBox(this);
			SingletonBehaviour<RestockZoneManager>.Instance.ReturnArea.RemoveBox(this);
			isInReturnArea = false;
			GenericDataSerializer.SaveBool("boxIsInReturnArea" + boxID, value: false);
		}
	}

	public void TemperatureTick(float temperature, int minutes = 1)
	{
		if (!isSpoilable)
		{
			return;
		}
		ProductType productType = GenericDataSerializer.Load("boxProductType" + boxID, ProductType.NONE);
		if (productType == ProductType.NONE)
		{
			return;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(productType);
		if (productData.storageRequirement == StorageRequirement.None)
		{
			return;
		}
		int num = BoxManager.storageRequirementToMinDegree[productData.storageRequirement];
		int num2 = BoxManager.storageRequirementToMaxDegree[productData.storageRequirement];
		if (temperature < (float)num - Mathf.Epsilon && frozenDamageProgress < 1f)
		{
			frozenDamageProgress += (float)minutes / (float)BoxManager.storageSensitivityToLowTemperatureSpoilageMinutes[productData.storageSensitivity];
			if (frozenDamageProgress >= 1f)
			{
				ChangeProductsToFrozen();
			}
		}
		else if (temperature > (float)num2 + Mathf.Epsilon && freshnessSpoilageProgress < 1f)
		{
			freshnessSpoilageProgress += (float)minutes / (float)BoxManager.storageSensitivityToHighTemperatureSpoilageMinutes[productData.storageSensitivity];
			if (freshnessSpoilageProgress >= 1f)
			{
				TryEnableFlyParticles(enable: true);
				ChangeProductsToSpoiled();
			}
		}
		GenericDataSerializer.SaveFloat("BOX_FRESHNESS_KEY" + boxID, freshnessSpoilageProgress);
		GenericDataSerializer.SaveFloat("BOX_FROZEN_DAMAGE_KEY" + boxID, frozenDamageProgress);
	}

	private void ChangeProductsToSpoiled()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetFloat(SpoilageAmountId, 1f);
		materialPropertyBlock.SetFloat(FreezeDamageAmountId, 0f);
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				containedProducts[i].SetMaterial(SingletonBehaviour<ProductPool>.Instance.GetSpoilableMaterial(containedProducts[i].Data.type), materialPropertyBlock);
			}
		}
	}

	private void TryEnableFlyParticles(bool enable)
	{
		if (enable && IsBoxSpoiled())
		{
			flyParticles.Play();
		}
		else
		{
			flyParticles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	private void ChangeProductsToFrozen()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetFloat(SpoilageAmountId, 0f);
		materialPropertyBlock.SetFloat(FreezeDamageAmountId, 1f);
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				containedProducts[i].SetMaterial(SingletonBehaviour<ProductPool>.Instance.GetSpoilableMaterial(containedProducts[i].Data.type), materialPropertyBlock);
			}
		}
	}

	private void SaveContent()
	{
		int productCount = GetProductCount();
		GenericDataSerializer.SaveInt("boxProductCount" + boxID, productCount);
		if (productCount == 0)
		{
			freshnessSpoilageProgress = 0f;
			frozenDamageProgress = 0f;
			GenericDataSerializer.SaveFloat("BOX_FRESHNESS_KEY" + boxID, freshnessSpoilageProgress);
			GenericDataSerializer.SaveFloat("BOX_FROZEN_DAMAGE_KEY" + boxID, frozenDamageProgress);
		}
		if (!ContainsConsumableProduct())
		{
			GenericDataSerializer.Save("boxProductType" + boxID, ProductType.NONE);
		}
		else
		{
			GenericDataSerializer.Save("boxProductType" + boxID, containedProducts[GetFirstAvailableProductIndex()].Data.type);
		}
		GenericDataSerializer.Save("boxPlaceableType" + boxID, containedPlaceableType);
		GenericDataSerializer.Save("boxFurnitureType" + boxID, containedFurnitureType);
	}

	public virtual int GetProductCount()
	{
		int num = 0;
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		if (!base.Outline.enabled)
		{
			return;
		}
		if (IsReservedForRestocking)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("box_reserved_staff_error", base.transform);
		}
		else if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
		{
			if (SingletonBehaviour<VehicleManager>.Instance.PlaceBoxToVehicle(this))
			{
				UnregisterFromZones();
			}
		}
		else if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			UnregisterFromZones();
			SingletonBehaviour<BoxManager>.Instance.PickUpBox(this);
			RigidBody.isKinematic = false;
			if (stored)
			{
				Detach();
			}
		}
	}

	public void OnBoxRemovedFromVehicle()
	{
		Detach();
		SaveLocation(checkForRestockingZone: true);
	}

	public void OnBoxRemovedFromPallet()
	{
		if (type != BoxType.FRUIT_BOX)
		{
			return;
		}
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				containedProducts[i].EnableRenderers(enable: true);
			}
		}
	}

	private void Detach()
	{
		if (stored)
		{
			stored = false;
			GenericDataSerializer.SaveBool("boxIsStored" + boxID, value: false);
		}
		RigidBody.isKinematic = false;
		SingletonBehaviour<BoxManager>.Instance.RegisterBox(this);
	}

	public ProductData GetContainedProduct()
	{
		int firstAvailableProductIndex = GetFirstAvailableProductIndex();
		if (firstAvailableProductIndex == -1)
		{
			return null;
		}
		return containedProducts[firstAvailableProductIndex].Data;
	}

	public bool IsBoxSpoiled()
	{
		return freshnessSpoilageProgress >= 1f;
	}

	public bool IsBoxFrozen()
	{
		return frozenDamageProgress >= 1f;
	}

	public ProductData GetAnyContainedProductData()
	{
		if (containedPlaceableType != PlaceableType.NONE)
		{
			return SingletonBehaviour<ProductPool>.Instance.GetShelfData(containedPlaceableType);
		}
		if (containedFurnitureType != FurnitureType.NONE)
		{
			return SingletonBehaviour<ProductPool>.Instance.GetFurnitureData(containedFurnitureType);
		}
		return GetContainedProduct();
	}

	public void EnableRenderers(bool enable)
	{
		if (type != BoxType.FRUIT_BOX || isRenderersEnabled == enable)
		{
			return;
		}
		isRenderersEnabled = enable;
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				containedProducts[i].EnableRenderers(enable);
			}
		}
		if (productsCoverMeshRenderer != null)
		{
			productsCoverMeshRenderer.enabled = enable;
		}
	}

	public void FillItems(ProductData data, int count)
	{
		bool flag = data.boxLocalPositions != null && data.boxLocalPositions.Count > 0;
		productLocations = GetProductPositions(data);
		if (!ContainsConsumableProduct())
		{
			containedProducts.Clear();
			for (int i = 0; i < data.GetMaxProductCount(); i++)
			{
				containedProducts.Add(null);
			}
		}
		int maxProductCount = data.GetMaxProductCount();
		count = Mathf.Min(count, maxProductCount);
		for (int j = 0; j < count; j++)
		{
			int index = maxProductCount - 1 - j;
			Product product = SingletonBehaviour<ProductPool>.Instance.GetProduct(data.type);
			product.transform.SetParent(base.transform);
			product.transform.localPosition = productLocations[index];
			if (data.facePlayer && !flag)
			{
				product.transform.rotation = Quaternion.LookRotation(base.transform.right, base.transform.up);
			}
			else if (flag)
			{
				product.transform.localEulerAngles = data.boxEulerAngles[index];
			}
			else
			{
				product.transform.localEulerAngles = base.transform.forward;
			}
			containedProducts[index] = product;
			if (!hasNoCover)
			{
				product.EnableRenderers(enable: false);
			}
			if (!hasNoCover && customLid == null)
			{
				boxMeshStaticClosed.enabled = true;
				boxMeshAnimated.enabled = false;
			}
		}
		EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, data.type, count);
		if (ShouldActivateProductsCover(data, count))
		{
			productsCover.SetActive(value: true);
		}
		SaveContent();
	}

	private List<Vector3> GetProductPositions(ProductData data)
	{
		if (data.boxLocalPositions != null && data.boxLocalPositions.Count > 0)
		{
			return data.boxLocalPositions;
		}
		return MathUtils.CalculatePositionsForBox(corner1, corner2, data.boxRowCount, data.boxColumnCount, data.isVerticalBoxLayout, collider.size.y);
	}

	public void AddItem(Product product)
	{
		if (product == null || product.Data == null)
		{
			return;
		}
		if (!ContainsConsumableProduct())
		{
			containedProducts.Clear();
			for (int i = 0; i < product.Data.GetMaxProductCount(); i++)
			{
				containedProducts.Add(null);
			}
			productLocations = GetProductPositions(product.Data);
		}
		product.transform.DOKill();
		product.transform.SetParent(base.transform);
		int firstEmptyProductIndex = GetFirstEmptyProductIndex();
		containedProducts[firstEmptyProductIndex] = product;
		EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, product.Data.type, 1);
		Vector3 to = productLocations[firstEmptyProductIndex];
		product.transform.DoCurvedLocalMove(to, 0.5f, 2f);
		bool flag = product.Data.boxLocalPositions != null && product.Data.boxLocalPositions.Count > 0;
		ShortcutExtensions.DOLocalRotate(endValue: flag ? product.Data.boxEulerAngles[firstEmptyProductIndex] : ((!product.Data.facePlayer || flag) ? new Vector3(0f, 180f, 0f) : new Vector3(0f, 90f, 0f)), target: product.transform, duration: 0.5f);
		SaveContent();
	}

	private bool ShouldActivateProductsCover(ProductData data, int count)
	{
		if (productsCover != null && data.GetMaxProductCount() == count)
		{
			return data.IsCookable();
		}
		return false;
	}

	public void FillWithPlaceable(PlaceableType type)
	{
		if (SingletonBehaviour<ProductPool>.Instance.GetShelfData(type) == null)
		{
			containedPlaceableType = PlaceableType.NONE;
			SaveContent();
		}
		else
		{
			containedPlaceableType = type;
			SaveContent();
		}
	}

	public void FillWithFurniture(FurnitureType type)
	{
		containedFurnitureType = type;
		SaveContent();
	}

	public Product RemoveAndGetItem()
	{
		int firstAvailableProductIndex = GetFirstAvailableProductIndex();
		Product product = containedProducts[firstAvailableProductIndex];
		containedProducts[firstAvailableProductIndex] = null;
		EventManager.NotifyEvent(GameEvents.PRODUCT_REMOVED_FROM_BOX, product.Data.type, 1);
		SaveContent();
		OnItemPlaced?.Invoke();
		if (productsCover != null && productsCover.activeSelf)
		{
			productsCover.SetActive(value: false);
		}
		return product;
	}

	public void DisposeAllProducts()
	{
		bool flag = IsBoxSpoiled() || IsBoxFrozen();
		TryEnableFlyParticles(enable: false);
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				if (flag)
				{
					containedProducts[i].SetMaterial(SingletonBehaviour<ProductPool>.Instance.GetOriginalMaterial(containedProducts[i].Data.type), null);
				}
				SingletonBehaviour<ProductPool>.Instance.PutBackToPool(containedProducts[i]);
			}
		}
		containedProducts.Clear();
	}

	private int GetFirstAvailableProductIndex()
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetFirstEmptyProductIndex()
	{
		if (containedProducts == null || containedProducts.Count == 0)
		{
			return -1;
		}
		for (int num = containedProducts.Count - 1; num >= 0; num--)
		{
			if (containedProducts[num] == null)
			{
				return num;
			}
		}
		return -1;
	}

	private void ClearBoxContent()
	{
		bool flag = IsBoxSpoiled() || IsBoxFrozen();
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				if (flag)
				{
					containedProducts[i].SetMaterial(SingletonBehaviour<ProductPool>.Instance.GetOriginalMaterial(containedProducts[i].Data.type), null);
				}
				SingletonBehaviour<ProductPool>.Instance.PutBackToPool(containedProducts[i]);
			}
		}
		containedProducts.Clear();
		if (productLocations != null)
		{
			productLocations = null;
		}
		containedPlaceableType = PlaceableType.NONE;
		containedFurnitureType = FurnitureType.NONE;
		OnItemPlaced = null;
		if (isOpen)
		{
			CloseInstant();
		}
	}

	public void ResetBox()
	{
		ClearBoxContent();
		ResetVisualState();
		freshnessSpoilageProgress = 0f;
		frozenDamageProgress = 0f;
	}

	public void ResetVisualState()
	{
		if (!hasNoCover && customLid == null)
		{
			boxMeshStaticClosed.enabled = true;
			boxMeshAnimated.enabled = false;
		}
	}

	public void DeleteAllData()
	{
		DisposeAllProducts();
		GenericDataSerializer.DeleteKey("boxPosition" + boxID);
		GenericDataSerializer.DeleteKey("boxRotation" + boxID);
		GenericDataSerializer.DeleteKey("boxIsOpen" + boxID);
		GenericDataSerializer.DeleteKey("boxProductCount" + boxID);
		GenericDataSerializer.DeleteKey("boxProductType" + boxID);
		GenericDataSerializer.DeleteKey("boxPlaceableType" + boxID);
		GenericDataSerializer.DeleteKey("boxFurnitureType" + boxID);
		GenericDataSerializer.DeleteKey("boxIsInReturnArea" + boxID);
		GenericDataSerializer.DeleteKey("boxIsStored" + boxID);
		GenericDataSerializer.DeleteKey("reservedEmployeeID" + boxID);
		GenericDataSerializer.DeleteKey("PACKABLE_ID_KEY" + boxID);
		GenericDataSerializer.DeleteKey("BOX_FRESHNESS_KEY" + boxID);
		GenericDataSerializer.DeleteKey("BOX_FROZEN_DAMAGE_KEY" + boxID);
		GenericDataSerializer.DeleteKey("IS_BOX_PERISHABLE_KEY" + boxID);
		GenericDataSerializer.DeleteKey("IS_BOX_SPOILABLE" + boxID);
	}

	public void SetBoxInReturnArea(bool inReturnArea)
	{
		isInReturnArea = inReturnArea;
		ReleaseBoxReservation();
		base.gameObject.layer = BoxManager.BOX_LAYER;
		GenericDataSerializer.SaveBool("boxIsInReturnArea" + boxID, isInReturnArea);
	}

	public void Open(bool playSound = true)
	{
		if (hasNoCover || isOpen)
		{
			return;
		}
		isOpen = true;
		if (customLid == null)
		{
			if (openCloseAnimation.isPlaying)
			{
				openCloseAnimation.Stop();
			}
			openCloseAnimation.Play("Open");
		}
		else
		{
			customLid.DOKill();
			customLid.DOLocalRotate(Vector3.up * ((-180f - customLid.localEulerAngles.y) % 360f), 360f, RotateMode.LocalAxisAdd).SetSpeedBased(isSpeedBased: true).SetEase(Ease.OutSine)
				.OnComplete(delegate
				{
					customLid.DOLocalMove(customLidTargetOpen.localPosition, 1f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.OutSine);
				});
		}
		if (playSound)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_OPEN);
		}
		if (containedPlaceableType != PlaceableType.NONE)
		{
			ActivateAnimatedMesh(enable: true);
			StartCoroutine(WaitAndUnboxPlaceable());
		}
		else if (containedFurnitureType != FurnitureType.NONE)
		{
			ActivateAnimatedMesh(enable: true);
			StartCoroutine(WaitAndUnboxFurniture());
		}
		else
		{
			if (EnableRendererRoutine != null)
			{
				StopCoroutine(EnableRendererRoutine);
			}
			EnableProductRenderers(enable: true);
		}
		TryEnableFlyParticles(enable: true);
	}

	public void OpenInstantEditor()
	{
		if (customLid != null)
		{
			customLid.localEulerAngles = Vector3.up * -180f;
			customLid.localPosition = customLidStartPosition;
		}
		else if ((bool)openCloseAnimation)
		{
			AnimationState animationState = openCloseAnimation["Open"];
			if (!(animationState == null))
			{
				animationState.normalizedTime = 1f;
				openCloseAnimation.Play("Open");
				openCloseAnimation.Sample();
				openCloseAnimation.Stop();
				ActivateAnimatedMesh(enable: true);
			}
		}
	}

	private IEnumerator WaitAndUnboxPlaceable()
	{
		SingletonBehaviour<PlacementManager>.Instance.SetWillPlaceObject(willPlaceObject: true);
		yield return new WaitForSeconds(0.4f);
		UnBoxPlaceable();
	}

	private IEnumerator WaitAndUnboxFurniture()
	{
		SingletonBehaviour<PlacementManager>.Instance.SetWillPlaceObject(willPlaceObject: true);
		yield return new WaitForSeconds(0.4f);
		UnBoxFurniture();
	}

	private IEnumerator WaitAndEnableProducts(bool enable)
	{
		yield return new WaitForSeconds(0.8f);
		EnableProductRenderers(enable);
	}

	private void EnableProductRenderers(bool enable)
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				containedProducts[i].EnableRenderers(enable);
			}
		}
		if (!hasNoCover && customLid == null)
		{
			boxMeshStaticClosed.enabled = !enable;
			boxMeshAnimated.enabled = enable;
		}
	}

	private void ActivateAnimatedMesh(bool enable)
	{
		if (!hasNoCover && customLid == null)
		{
			boxMeshStaticClosed.enabled = !enable;
			boxMeshAnimated.enabled = enable;
		}
	}

	public void CloseInstant()
	{
		if (customLid != null)
		{
			isOpen = false;
			customLid.localEulerAngles = customLidStartRotation;
			customLid.localPosition = customLidStartPosition;
			return;
		}
		if (openCloseAnimation.isPlaying)
		{
			openCloseAnimation.Stop();
		}
		AnimationState animationState = openCloseAnimation["Close"];
		animationState.enabled = true;
		animationState.weight = 1f;
		animationState.time = animationState.length;
		openCloseAnimation.Sample();
		openCloseAnimation.Stop();
		isOpen = false;
		ResetVisualState();
	}

	public bool IsEmpty()
	{
		if (!ContainsConsumableProduct() && containedPlaceableType == PlaceableType.NONE)
		{
			return containedFurnitureType == FurnitureType.NONE;
		}
		return false;
	}

	public bool IsDisposable()
	{
		if (!IsEmpty() && !IsBoxSpoiled())
		{
			return IsBoxFrozen();
		}
		return true;
	}

	public float GetBoxPrice(float sellingPriceMultiplier)
	{
		float num = 0f;
		ProductData anyContainedProductData = GetAnyContainedProductData();
		if (anyContainedProductData == null)
		{
			return 0f;
		}
		num = anyContainedProductData.cost * sellingPriceMultiplier;
		if (ContainsConsumableProduct())
		{
			num *= (float)GetProductCount();
		}
		if (IsBoxSpoiled() || IsBoxFrozen())
		{
			num = 0f;
		}
		return num;
	}

	public bool ContainsConsumableProduct()
	{
		for (int i = 0; i < containedProducts.Count; i++)
		{
			if (containedProducts[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPlaceableBox()
	{
		return containedPlaceableType != PlaceableType.NONE;
	}

	public bool IsFurnitureBox()
	{
		return containedFurnitureType != FurnitureType.NONE;
	}

	private void UnBoxPlaceable()
	{
		SingletonBehaviour<BoxManager>.Instance.ThrowBox();
		base.gameObject.SetActive(value: false);
		int placeableID = GenericDataSerializer.LoadInt("PACKABLE_ID_KEY" + boxID, -1);
		Placeable newPlaceable = SingletonBehaviour<PlaceablePool>.Instance.GetPlaceable(containedPlaceableType);
		Vector3 initialScale = newPlaceable.transform.localScale;
		newPlaceable.transform.localScale = 0.1f * initialScale;
		newPlaceable.transform.DOScale(initialScale * 1.07f, 0.25f).SetEase(Ease.Linear).OnComplete(delegate
		{
			newPlaceable.transform.DOScale(initialScale, 0.1f).SetEase(Ease.Linear);
		})
			.OnKill(delegate
			{
				newPlaceable.transform.localScale = initialScale;
			});
		newPlaceable.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z);
		newPlaceable.SetFloorLayers();
		newPlaceable.StartNewPlacement(delegate
		{
			if (placeableID == -1)
			{
				int placeableCount = SingletonBehaviour<SpawnManager>.Instance.GetPlaceableCount(containedPlaceableType);
				newPlaceable.InitializeNewPlaceable(placeableCount);
				SingletonBehaviour<SpawnManager>.Instance.AddNewPlaceable(newPlaceable);
				EventManager.NotifyEvent(PlaceableEvents.NEW_PLACEABLE_PLACED, newPlaceable.Type);
			}
			else
			{
				newPlaceable.InitializeOldPlaceable(placeableID, isPacked: true);
				SingletonBehaviour<SpawnManager>.Instance.UnpackPlaceable(newPlaceable);
				SingletonBehaviour<SpawnManager>.Instance.AddOldPlaceable(newPlaceable);
				EventManager.NotifyEvent(PlaceableEvents.NEW_PLACEABLE_PLACED, newPlaceable.Type);
			}
			SingletonBehaviour<BoxManager>.Instance.DeleteBox(BoxID);
		}, delegate
		{
			base.gameObject.transform.position = newPlaceable.transform.position;
			base.gameObject.transform.eulerAngles = newPlaceable.transform.eulerAngles;
			newPlaceable.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
			CloseInstant();
			SingletonBehaviour<BoxManager>.Instance.PickUpBox(this);
		});
	}

	private void UnBoxFurniture()
	{
		SingletonBehaviour<BoxManager>.Instance.ThrowBox();
		base.gameObject.SetActive(value: false);
		int furnitureID = GenericDataSerializer.LoadInt("PACKABLE_ID_KEY" + boxID, -1);
		Furniture newFurniture = SingletonBehaviour<FurniturePool>.Instance.GetFurniture(containedFurnitureType);
		Vector3 initialScale = newFurniture.transform.localScale;
		newFurniture.transform.localScale = 0.1f * initialScale;
		newFurniture.transform.DOScale(initialScale * 1.07f, 0.25f).SetEase(Ease.Linear).OnComplete(delegate
		{
			newFurniture.transform.DOScale(initialScale, 0.1f).SetEase(Ease.Linear);
		})
			.OnKill(delegate
			{
				newFurniture.transform.localScale = initialScale;
			});
		newFurniture.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z);
		newFurniture.SetFloorLayers();
		newFurniture.StartNewPlacement(delegate
		{
			int furnitureCount = SingletonBehaviour<SpawnManager>.Instance.GetFurnitureCount(containedFurnitureType);
			if (furnitureID == -1)
			{
				newFurniture.InitializeNewFurniture(furnitureCount);
				SingletonBehaviour<SpawnManager>.Instance.AddNewFurniture(newFurniture);
				EventManager.NotifyEvent(PlaceableEvents.NEW_FURNITURE_PLACED, newFurniture.Type);
			}
			else
			{
				newFurniture.InitializeOldFurniture(furnitureID, isPacked: true);
				SingletonBehaviour<SpawnManager>.Instance.UnpackFurniture(newFurniture);
				SingletonBehaviour<SpawnManager>.Instance.AddOldFurniture(newFurniture);
				EventManager.NotifyEvent(PlaceableEvents.NEW_FURNITURE_PLACED, newFurniture.Type);
			}
			SingletonBehaviour<BoxManager>.Instance.DeleteBox(BoxID);
		}, delegate
		{
			base.gameObject.transform.position = newFurniture.transform.position;
			base.gameObject.transform.eulerAngles = newFurniture.transform.eulerAngles;
			newFurniture.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
			CloseInstant();
			SingletonBehaviour<BoxManager>.Instance.PickUpBox(this);
		});
	}

	public void Close(bool playSound = true)
	{
		isOpen = false;
		if (customLid == null)
		{
			if (openCloseAnimation.isPlaying)
			{
				openCloseAnimation.Stop();
			}
			openCloseAnimation.Play("Close");
		}
		else
		{
			customLid.DOKill();
			customLid.DOLocalMove(customLidStartPosition, 1f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.OutSine)
				.OnComplete(delegate
				{
					customLid.DOLocalRotate(Vector3.up * ((360f - customLid.localEulerAngles.y) % 450f), 360f, RotateMode.LocalAxisAdd).SetSpeedBased(isSpeedBased: true).SetEase(Ease.OutSine);
				});
		}
		if (playSound)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_CLOSE);
		}
		if (containedPlaceableType == PlaceableType.NONE && containedFurnitureType == FurnitureType.NONE)
		{
			EnableRendererRoutine = StartCoroutine(WaitAndEnableProducts(enable: false));
		}
	}

	public bool IsOpen()
	{
		return isOpen;
	}

	public void PhysicsUpdate()
	{
		rigidBodyVelocity = rigidBody.linearVelocity;
		float sqrMagnitude = rigidBodyVelocity.sqrMagnitude;
		if (isMoving && sqrMagnitude < STEADY_BOX_SPEED)
		{
			isMoving = false;
			base.gameObject.layer = BoxManager.BOX_LAYER;
			if (reservedForEmployeeID == -1)
			{
				SaveLocation(checkForRestockingZone: true);
			}
		}
		else if (!isMoving && sqrMagnitude > STEADY_BOX_SPEED)
		{
			isMoving = true;
		}
	}

	public override void OnMouseHoverStarted()
	{
		if (SingletonBehaviour<VehicleManager>.Instance.IsBoxContainedInVehicle(boxID))
		{
			return;
		}
		base.OnMouseHoverStarted();
		if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
		{
			SingletonBehaviour<VehicleManager>.Instance.UpdateMenuBoxHovered();
		}
		else
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("take", null)
			} }, base.transform);
		}
		if (containedPlaceableType != PlaceableType.NONE)
		{
			SingletonBehaviour<BoxInfoWindow>.Instance.Open(SingletonBehaviour<ProductPool>.Instance.GetShelfData(containedPlaceableType), 1);
			return;
		}
		if (containedFurnitureType != FurnitureType.NONE)
		{
			SingletonBehaviour<BoxInfoWindow>.Instance.Open(SingletonBehaviour<ProductPool>.Instance.GetFurnitureData(containedFurnitureType), 1);
			return;
		}
		if (IsEmpty())
		{
			SingletonBehaviour<BoxInfoWindow>.Instance.Open(type);
			return;
		}
		ProductData data = containedProducts[GetFirstAvailableProductIndex()].Data;
		if (data.storageRequirement != StorageRequirement.None)
		{
			float currentTemperature = ((temperatureZone != null) ? temperatureZone.Temperature : ((float)BoxManager.storageRequirementToMaxDegree[data.storageRequirement]));
			SingletonBehaviour<BoxInfoWindow>.Instance.Open(containedProducts[GetFirstAvailableProductIndex()].Data, GetProductCount(), freshnessSpoilageProgress, frozenDamageProgress, data.storageRequirement, currentTemperature);
		}
		else
		{
			SingletonBehaviour<BoxInfoWindow>.Instance.Open(containedProducts[GetFirstAvailableProductIndex()].Data, GetProductCount());
		}
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		CloseInteractionElements();
	}

	protected void CloseInteractionElements()
	{
		SingletonBehaviour<BoxInfoWindow>.Instance.Close();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		SingletonBehaviour<VehicleManager>.Instance.UpdateMenu();
	}

	public void StartNewPlacement(Action onPlacementEnded = null)
	{
		if (!hasNoCover)
		{
			CloseInstant();
		}
		EnableProductRenderers(enable: false);
		this.onPlacementEnded = onPlacementEnded;
		SingletonBehaviour<PlacementManager>.Instance.StartPlacement(moveable, facePlayer: true, new List<int> { BoxManager.BOX_LAYER });
	}

	private void DisplayMoveable(bool display)
	{
		moveable.gameObject.SetActive(display);
		collider.enabled = !display;
		solidObject.gameObject.SetActive(!display);
		if (!hasNoCover && customLid == null)
		{
			boxMeshStaticClosed.enabled = !display;
		}
	}

	public void SwitchLook(bool toSolidObject)
	{
		DisplayMoveable(!toSolidObject);
	}

	public void OnPlacementEnded()
	{
		onPlacementEnded?.Invoke();
		if (hasNoCover || isOpen)
		{
			EnableProductRenderers(enable: true);
		}
	}

	public void SavePosition()
	{
		SaveLocation(checkForRestockingZone: true);
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public new void EnableOutline(bool enable)
	{
		base.Outline.enabled = enable;
	}

	private void OnDrawGizmos()
	{
		if (corner1 != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(corner1.position, 0.05f);
		}
		if (corner2 != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(corner2.position, 0.05f);
		}
		if (corner1 != null && corner2 != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(corner1.position, corner2.position);
		}
	}

	public bool CanPack()
	{
		return false;
	}

	public bool PlacedBefore()
	{
		return boxID != -1;
	}

	public virtual bool IsCancelable()
	{
		return false;
	}
}
