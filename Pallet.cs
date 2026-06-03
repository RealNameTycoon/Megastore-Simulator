using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Pallet : Interactable, MoveableObjectInterface, MoveableHolderInterface
{
	[SerializeField]
	private Collider collider;

	[SerializeField]
	private Outline outline;

	[SerializeField]
	private Transform corner1;

	[SerializeField]
	private Transform corner2;

	[SerializeField]
	private GameObject palletLabelBack;

	[SerializeField]
	private GameObject palletLabelFront;

	[SerializeField]
	private SpriteRenderer productSpriteBack;

	[SerializeField]
	private SpriteRenderer productSpriteFront;

	[SerializeField]
	private TextMeshPro productCountTextBack;

	[SerializeField]
	private TextMeshPro productCountTextFront;

	[SerializeField]
	private Rigidbody rigidBody;

	[SerializeField]
	private Moveable moveable;

	[SerializeField]
	private GameObject solidObject;

	[SerializeField]
	private Clickable labelClickable1;

	[SerializeField]
	private Clickable labelClickable2;

	[SerializeField]
	private RestockZone restockZone;

	[SerializeField]
	private TemperatureZone temperatureZone;

	public static string PALLET_TAG = "Pallet";

	private static string PALLET_POSITION_KEY = "PALLET_POSITION_KEY";

	private static string PALLET_ROTATION_KEY = "PALLET_ROTATION_KEY";

	private static float PALLET_HEIGHT = 2f;

	private const string PALLET_BOXES = "PALLET_BOXES";

	protected const string PALLET_PRODUCT_TYPE = "PALLET_PRODUCT_TYPE";

	private List<int> containedBoxIDs = new List<int>();

	[SerializeField]
	private int palletID;

	private bool isHovered;

	private ProductType containedType = ProductType.NONE;

	private int productCount;

	private Action onPlacementEnded;

	private const int MAX_BOXES_TO_ENABLE_RENDERERS = 4;

	[SerializeField]
	private PalletShelf containerShelf;

	public int boxAmountToWaitForReturn;

	public RestockZone RestockZone => restockZone;

	public Moveable Moveable => moveable;

	public bool IsEmpty => containedBoxIDs.Count == 0;

	public int PalletID => palletID;

	public ProductType ContainedType => containedType;

	public int ProductCount => productCount;

	public int BoxCount => containedBoxIDs.Count;

	public Rigidbody RigidBody => rigidBody;

	public PalletShelf ContainerShelf => containerShelf;

	public int BoxAmountToWaitForReturn => boxAmountToWaitForReturn;

	public void SetContainerShelf(PalletShelf shelf)
	{
		containerShelf = shelf;
	}

	public void FitBoxes(Box box)
	{
		float surfaceWidth = Mathf.Abs(corner1.localPosition.x - corner2.localPosition.x);
		float surfaceLength = Mathf.Abs(corner1.localPosition.z - corner2.localPosition.z);
		PalletPacking.Result result = PalletPacking.Generate((corner1.localPosition + corner2.localPosition) / 2f, surfaceWidth, surfaceLength, PALLET_HEIGHT, box.GetWidth(), box.GetLength(), box.GetHeight());
		Debug.Log(result.positions.Count);
		for (int i = 0; i < result.positions.Count; i++)
		{
			Box box2 = UnityEngine.Object.Instantiate(box);
			box2.transform.SetParent(base.transform);
			box2.transform.localPosition = result.positions[i];
			box2.transform.localEulerAngles = result.eulerAngles[i];
		}
	}

	private void Start()
	{
		LayerMask placeableFloorLayers = (1 << PlacementManager.FLOOR_LAYER) | (1 << PlacementManager.SERVICE_ROOM_FLOOR_LAYER) | (1 << PlacementManager.STORAGE_FLOOR_LAYER) | (1 << PlacementManager.VEHICLE_FLOOR_LAYER) | (1 << PlacementManager.AROUND_STORE_LAYER);
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
		EventManager.AddListener(PlaceableEvents.STORE_BOX, OnStoreBox);
		EventManager.AddListener(GameEvents.BOX_PICK_STARTED, OnBoxPickStarted);
		EventManager.AddListener<Box>(GameEvents.VEHICLE_BOX_PICK_STARTED, OnVehicleBoxPickStarted);
		labelClickable1.onClickAction.AddListener(OnLabelClicked);
		labelClickable1.onRightClickAction.AddListener(OnLabelRightClicked);
		labelClickable1.SetHoverStartedAction(OnLabelHoverStarted);
		labelClickable2.onClickAction.AddListener(OnLabelClicked);
		labelClickable2.onRightClickAction.AddListener(OnLabelRightClicked);
		labelClickable2.SetHoverStartedAction(OnLabelHoverStarted);
	}

	private void OnLabelClicked()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = containedType;
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_ADDEDTO_CART, param);
		}
		else if (productCount == 0)
		{
			labelClickable1.gameObject.SetActive(value: false);
			labelClickable2.gameObject.SetActive(value: false);
			containedType = ProductType.NONE;
			SaveContent();
			if (containerShelf != null)
			{
				containerShelf.OnLabelRemoved();
			}
		}
	}

	private void OnLabelRightClicked()
	{
		if (HandScanner.Instance.IsPicked)
		{
			ProductType param = containedType;
			EventManager.NotifyEvent(ProductEvents.SCANNER_PRODUCT_REMOVEDFROM_CART, param);
		}
	}

	private void OnLabelHoverStarted()
	{
		ProductType param = containedType;
		EventManager.NotifyEvent(ProductEvents.PRICE_TAG_HOVER_STARTED, param);
	}

	public void Initialize(int palletID)
	{
		this.palletID = palletID;
	}

	private void OnStoreBox()
	{
		if (!isHovered)
		{
			return;
		}
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (!containedBoxIDs.Contains(pickedBox.BoxID) && !SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			ProductData anyContainedProductData = pickedBox.GetAnyContainedProductData();
			ProductType productType = ((anyContainedProductData == null) ? ProductType.NONE : anyContainedProductData.type);
			int count = SingletonBehaviour<PalletManager>.Instance.GetBoxPositions(pickedBox.Type).localPositions.Count;
			if (containedType != ProductType.NONE && containedType != productType && containedBoxIDs.Count < count && containedBoxIDs.Count > 0)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_different_product", base.transform);
			}
			else if ((containerShelf != null && containerShelf.IsReservedToStaff()) || IsReservedToRestocker())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pallet_reserved_staff_error", base.transform);
			}
			else if (containedBoxIDs.Count >= count)
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_full", base.transform);
			}
			else
			{
				PlaceBox(pickedBox);
			}
		}
	}

	public void PlaceReturnBox(Box box, bool instant = false)
	{
		boxAmountToWaitForReturn--;
		box.ReleaseBoxReservation();
		PlaceBox(box, isNPC: true, instant);
	}

	public void PlaceBox(Box box, bool isNPC = false, bool instant = false)
	{
		ProductData anyContainedProductData = box.GetAnyContainedProductData();
		ProductType productType = ((anyContainedProductData == null) ? ProductType.NONE : anyContainedProductData.type);
		PositionContainer boxPositions = SingletonBehaviour<PalletManager>.Instance.GetBoxPositions(box.Type);
		_ = boxPositions.localPositions.Count;
		if (containedType != ProductType.NONE && containedType != productType && containedBoxIDs.Count != 0)
		{
			return;
		}
		containedBoxIDs.Add(box.BoxID);
		box.OnAddedToPallet(palletID);
		if (restockZone != null)
		{
			box.RegisterToRestockZone(restockZone);
		}
		if (temperatureZone != null)
		{
			box.RegisterToTemperatureZone(temperatureZone);
		}
		if (!isNPC)
		{
			SingletonBehaviour<BoxManager>.Instance.OnBoxPut();
			box.Collider.enabled = false;
		}
		if (box.IsOpen())
		{
			box.Close();
		}
		box.transform.SetParent(base.transform);
		box.OnBoxPut();
		if (instant)
		{
			box.transform.localPosition = boxPositions.localPositions[containedBoxIDs.Count - 1];
			box.transform.localEulerAngles = boxPositions.localEulers[containedBoxIDs.Count - 1];
			RefreshBoxRenderers();
		}
		else
		{
			box.transform.DOLocalRotate(boxPositions.localEulers[containedBoxIDs.Count - 1], 0.4f);
			box.transform.DoCurvedLocalMove(boxPositions.localPositions[containedBoxIDs.Count - 1], 0.4f, 2f).OnComplete(delegate
			{
				if (containedBoxIDs.Contains(box.BoxID))
				{
					RefreshBoxRenderers();
				}
				if (!isNPC)
				{
					SingletonBehaviour<RayShooter>.Instance.ImitateHover();
				}
			});
		}
		if (containedType == ProductType.NONE || containedType != productType)
		{
			containedType = productType;
		}
		productCount += GetProductCount(box);
		SaveContent();
		if (!isNPC)
		{
			CloseInteractionElements();
		}
		RepaintLabel();
		if (containerShelf != null)
		{
			containerShelf.OnBoxPut();
		}
		EventManager.NotifyEvent(GameEvents.PALLET_STACKED_BOX);
	}

	public void AddBoxInstant(Box boxToAdd)
	{
		if (containedBoxIDs.Contains(boxToAdd.BoxID))
		{
			return;
		}
		ProductData anyContainedProductData = boxToAdd.GetAnyContainedProductData();
		ProductType productType = ((anyContainedProductData == null) ? ProductType.NONE : anyContainedProductData.type);
		PositionContainer boxPositions = SingletonBehaviour<PalletManager>.Instance.GetBoxPositions(boxToAdd.Type);
		int count = boxPositions.localPositions.Count;
		if ((containedType == ProductType.NONE || containedType == productType || containedBoxIDs.Count >= count || containedBoxIDs.Count <= 0) && containedBoxIDs.Count < count && (containedType == ProductType.NONE || containedType == productType || containedBoxIDs.Count == 0))
		{
			containedBoxIDs.Add(boxToAdd.BoxID);
			RefreshBoxRenderers();
			boxToAdd.OnAddedToPallet(palletID);
			boxToAdd.Collider.enabled = false;
			boxToAdd.transform.SetParent(base.transform);
			boxToAdd.transform.localPosition = boxPositions.localPositions[containedBoxIDs.Count - 1];
			boxToAdd.transform.localEulerAngles = boxPositions.localEulers[containedBoxIDs.Count - 1];
			boxToAdd.OnBoxPut();
			if (containedType == ProductType.NONE || containedType != productType)
			{
				containedType = productType;
			}
			productCount += GetProductCount(boxToAdd);
			SaveContent();
			RepaintLabel();
		}
	}

	public void StartNewPlacement(Action onPlacementEnded = null)
	{
		this.onPlacementEnded = onPlacementEnded;
		SingletonBehaviour<PlacementManager>.Instance.StartPlacement(moveable, facePlayer: true, new List<int> { RayShooter.PALLET_LAYER });
	}

	private void OnBoxPickStarted()
	{
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (containedBoxIDs.Contains(pickedBox.BoxID))
		{
			RemoveBox(pickedBox);
			pickedBox.SaveLocation();
			RepaintLabel();
		}
	}

	private void OnVehicleBoxPickStarted(Box pickedBox)
	{
		if (containedBoxIDs.Contains(pickedBox.BoxID))
		{
			RemoveBox(pickedBox);
			RepaintLabel();
		}
	}

	public void RemoveBox(Box box, bool willReturn = false)
	{
		containedBoxIDs.Remove(box.BoxID);
		box.OnRemovedFromPallet();
		productCount -= GetProductCount(box);
		box.OnBoxRemovedFromPallet();
		box.EnableOutline(enable: false);
		box.Collider.enabled = true;
		RefreshBoxRenderers();
		RepaintLabel();
		SaveContent();
		if (containerShelf != null && !willReturn)
		{
			containerShelf.OnBoxTaken();
		}
		if (containerShelf != null && willReturn)
		{
			boxAmountToWaitForReturn++;
		}
	}

	public void OnReturnBoxCanceled()
	{
		boxAmountToWaitForReturn--;
		if (containerShelf != null)
		{
			containerShelf.OnBoxTaken();
		}
	}

	private void SaveContent()
	{
		GenericDataSerializer.Save("PALLET_BOXES" + palletID, containedBoxIDs);
		GenericDataSerializer.Save("PALLET_PRODUCT_TYPE" + palletID, containedType);
	}

	private void RepaintLabel()
	{
		if (containedType != ProductType.NONE)
		{
			palletLabelBack.SetActive(value: true);
			palletLabelFront.SetActive(value: true);
			productCountTextBack.text = productCount.ToString();
			productCountTextFront.text = productCount.ToString();
			ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(containedType);
			SetSpriteKeepSize(productSpriteBack, anyProductData.productSprite);
			SetSpriteKeepSize(productSpriteFront, anyProductData.productSprite);
		}
		else
		{
			palletLabelBack.SetActive(value: false);
			palletLabelFront.SetActive(value: false);
		}
	}

	public bool HasProductLabel()
	{
		if (!palletLabelBack.activeSelf)
		{
			return palletLabelFront.activeSelf;
		}
		return true;
	}

	private void SetSpriteKeepSize(SpriteRenderer sr, Sprite newSprite)
	{
		Sprite sprite = sr.sprite;
		Vector3 vector = (sprite ? sprite.bounds.size : newSprite.bounds.size);
		sr.sprite = newSprite;
		Vector3 size = newSprite.bounds.size;
		Transform obj = sr.transform;
		Vector3 localScale = obj.localScale;
		if (size.x != 0f)
		{
			localScale.x *= vector.x / size.x;
		}
		if (size.y != 0f)
		{
			localScale.y *= vector.y / size.y;
		}
		obj.localScale = localScale;
	}

	public void EnableOutline(bool enable)
	{
		outline.enabled = enable;
		EnableBoxOutlines(enable);
	}

	public void EnableCollider(bool enable)
	{
		collider.enabled = enable;
	}

	public void InitializeOldPallet(int palletID)
	{
		this.palletID = palletID;
		if (GenericDataSerializer.HasKey(PALLET_POSITION_KEY + palletID))
		{
			Vector3 position = GenericDataSerializer.Load(PALLET_POSITION_KEY + palletID);
			Quaternion rotation = GenericDataSerializer.LoadQuaternion(PALLET_ROTATION_KEY + palletID);
			base.transform.position = position;
			base.transform.rotation = rotation;
		}
		containedBoxIDs = GenericDataSerializer.Load("PALLET_BOXES" + palletID, new List<int>());
		containedType = GenericDataSerializer.Load("PALLET_PRODUCT_TYPE" + palletID, ProductType.NONE);
		if (!SingletonBehaviour<ProductPool>.Instance.HasProductData(containedType))
		{
			containedType = ProductType.NONE;
		}
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] != -1)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
				if (box == null || box.IsInReturnArea)
				{
					list.Add(containedBoxIDs[i]);
					continue;
				}
				PositionContainer boxPositions = SingletonBehaviour<PalletManager>.Instance.GetBoxPositions(box.Type);
				box.OnAddedToPallet(palletID);
				box.transform.SetParent(base.transform);
				box.transform.localPosition = boxPositions.localPositions[num];
				box.transform.localEulerAngles = boxPositions.localEulers[num];
				box.Collider.enabled = false;
				productCount += GetProductCount(box);
				num++;
			}
		}
		if (list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				containedBoxIDs.Remove(list[j]);
			}
			Debug.LogWarning("fixing the deleted box IDs for pallet " + palletID + " ," + list.Count + " boxes deleted", this);
			SaveContent();
		}
		FixOldIssuesWithBoxes();
		CheckAndUpdateRestockZone();
		RefreshBoxRenderers();
		RepaintLabel();
	}

	private void FixOldIssuesWithBoxes()
	{
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
			if (box != null && !box.Stored)
			{
				box.UnregisterFromRestockZone();
				box.UnregisterFromTemperatureZone();
				box.SetBoxStored();
			}
		}
	}

	private void RefreshBoxRenderers()
	{
		int num = 0;
		for (int num2 = containedBoxIDs.Count - 1; num2 >= 0; num2--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num2]);
			if (box != null)
			{
				PositionContainer boxPositions = SingletonBehaviour<PalletManager>.Instance.GetBoxPositions(box.Type);
				box.DOKill();
				box.transform.localPosition = boxPositions.localPositions[num2];
				box.transform.localEulerAngles = boxPositions.localEulers[num2];
			}
			if (box != null && num < 4)
			{
				box.EnableRenderers(enable: true);
				num++;
			}
			else if (box != null)
			{
				box.EnableRenderers(enable: false);
			}
		}
	}

	private int GetProductCount(Box box)
	{
		if (box.IsEmpty())
		{
			return 0;
		}
		if (box.GetAnyContainedProductData().type >= ProductType.NONCONSUMABLE_START)
		{
			return 1;
		}
		return box.ProductCount;
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		outline.enabled = true;
		isHovered = true;
		if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
		{
			SingletonBehaviour<VehicleManager>.Instance.UpdateMenuPalletHovered(TakeLastBox);
			EnableBoxOutlines(enable: true);
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuWithStack();
			EnableBoxOutlines(enable: true);
		}
		else if (containedBoxIDs.Count > 0)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("take_box", TakeLastBox)
			} }, base.transform);
			EnableBoxOutlines(enable: true);
		}
		else if (containedBoxIDs.Count == 0 && !SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("take_pallet", delegate
				{
					SingletonBehaviour<PalletManager>.Instance.PickUpPallet(this);
				})
			} }, base.transform);
		}
		if (containedType != ProductType.NONE)
		{
			OpenBoxInfoWindow();
		}
	}

	private void OpenBoxInfoWindow()
	{
		ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(containedType);
		if (containedBoxIDs.Count == 0)
		{
			SingletonBehaviour<BoxInfoWindow>.Instance.Open(anyProductData, productCount);
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
			if (box != null)
			{
				num = Mathf.Min(num, box.FreshnessSpoilageProgress);
				num2 = Mathf.Max(num2, box.FreshnessSpoilageProgress);
				num3 = Mathf.Min(num3, box.FrozenDamageProgress);
				num4 = Mathf.Max(num4, box.FrozenDamageProgress);
			}
		}
		SingletonBehaviour<BoxInfoWindow>.Instance.Open(anyProductData, productCount, temperatureZone.Temperature, num, num2, num3, num4);
	}

	private void TakeLastBox()
	{
		if (containedBoxIDs.Count <= 0)
		{
			return;
		}
		Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[containedBoxIDs.Count - 1]);
		if (!(box == null))
		{
			if (box.ReservedForEmployeeID != -1 || (containerShelf != null && containerShelf.IsReservedToStaff()))
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pallet_reserved_staff_error", base.transform);
			}
			else
			{
				box.OnMouseButtonDown();
			}
		}
	}

	public bool IsReservedToStaff()
	{
		if (containedBoxIDs.Count > 0)
		{
			for (int i = 0; i < containedBoxIDs.Count; i++)
			{
				Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
				if (box != null && box.IsReservedForRestocking)
				{
					return true;
				}
			}
		}
		if ((containerShelf != null && containerShelf.IsReservedToStaff()) || IsReservedToRestocker())
		{
			return true;
		}
		return false;
	}

	public bool IsReservedToRestocker()
	{
		if (containerShelf != null)
		{
			return boxAmountToWaitForReturn > 0;
		}
		return false;
	}

	public Box GetLastBox()
	{
		if (containedBoxIDs.Count > 0)
		{
			return SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[containedBoxIDs.Count - 1]);
		}
		return null;
	}

	public Box GetLastUnreservedBox()
	{
		for (int num = containedBoxIDs.Count - 1; num >= 0; num--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null && !box.IsReservedForRestocking)
			{
				return box;
			}
		}
		return null;
	}

	public Box GetLowestStockBox()
	{
		Box result = null;
		int num = int.MaxValue;
		for (int num2 = containedBoxIDs.Count - 1; num2 >= 0; num2--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num2]);
			if (box != null && !box.IsReservedForRestocking)
			{
				int num3 = box.ProductCount;
				if (num3 != 0 && num3 < num)
				{
					result = box;
					num = num3;
				}
			}
		}
		return result;
	}

	public bool IsLastBox(Box box)
	{
		if (containedBoxIDs.Count > 0)
		{
			return containedBoxIDs[containedBoxIDs.Count - 1] == box.BoxID;
		}
		return false;
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		CloseInteractionElements();
	}

	public void ResetPallet()
	{
		containedBoxIDs.Clear();
		containedType = ProductType.NONE;
		productCount = 0;
		palletLabelBack.SetActive(value: false);
		palletLabelFront.SetActive(value: false);
	}

	protected void CloseInteractionElements()
	{
		SingletonBehaviour<BoxInfoWindow>.Instance.Close();
		if (outline != null)
		{
			outline.enabled = false;
		}
		EnableBoxOutlines(enable: false);
		isHovered = false;
		SingletonBehaviour<BoxManager>.Instance.UpdateMenu();
		FireExtinguisher.Instance.UpdateMenu();
		SingletonBehaviour<VehicleManager>.Instance.UpdateMenu();
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
	}

	private void EnableBoxOutlines(bool enable)
	{
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]);
			if (box != null)
			{
				box.EnableOutline(enable);
			}
		}
	}

	public void SaveLocation(bool checkForRestockingZone = false)
	{
		if (checkForRestockingZone)
		{
			CheckAndUpdateRestockZone();
			CheckAndUpdateTemperatureZone();
		}
		GenericDataSerializer.Save(PALLET_POSITION_KEY + palletID, base.transform.position);
		GenericDataSerializer.Save(PALLET_ROTATION_KEY + palletID, base.transform.rotation);
	}

	public void InteractionStarted()
	{
		outline.enabled = true;
	}

	public void InteractionEnded()
	{
		outline.enabled = false;
	}

	public void OnTaken()
	{
		UnregisterFromRestockZone();
		UnregisterFromTemperatureZone();
	}

	public void OnReleased()
	{
		SaveLocation(checkForRestockingZone: true);
	}

	private void DisplayMoveable(bool display)
	{
		moveable.gameObject.SetActive(display);
		collider.enabled = !display;
		solidObject.gameObject.SetActive(!display);
	}

	public void SwitchLook(bool toSolidObject)
	{
		DisplayMoveable(!toSolidObject);
	}

	public void SavePosition()
	{
		SaveLocation(checkForRestockingZone: true);
	}

	public void DeleteData()
	{
		GenericDataSerializer.DeleteKey(PALLET_POSITION_KEY + palletID);
		GenericDataSerializer.DeleteKey(PALLET_ROTATION_KEY + palletID);
		GenericDataSerializer.DeleteKey("PALLET_BOXES" + palletID);
		GenericDataSerializer.DeleteKey("PALLET_PRODUCT_TYPE" + palletID);
	}

	public void OnPlacementEnded()
	{
		onPlacementEnded?.Invoke();
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public Moveable GetMoveable()
	{
		return moveable;
	}

	public void CheckAndUpdateRestockZone()
	{
		restockZone = SingletonBehaviour<RestockZoneManager>.Instance.GetRestockZoneAtPosition(base.transform.position);
		if (!(restockZone != null))
		{
			return;
		}
		for (int num = containedBoxIDs.Count - 1; num >= 0; num--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null)
			{
				box.RegisterToRestockZone(restockZone);
			}
		}
	}

	public void CheckAndUpdateTemperatureZone()
	{
		temperatureZone = SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(base.transform.position);
		if (!(temperatureZone != null))
		{
			return;
		}
		if (containerShelf == null && !SingletonBehaviour<TemperatureZoneManager>.Instance.IsRoomTemperatureZone(temperatureZone))
		{
			base.transform.SetParent(temperatureZone.transform);
			temperatureZone.RegisterFreePallet(this);
		}
		for (int num = containedBoxIDs.Count - 1; num >= 0; num--)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[num]);
			if (box != null)
			{
				box.RegisterToTemperatureZone(temperatureZone);
			}
		}
	}

	public void UnregisterFromRestockZone()
	{
		if (!(restockZone != null))
		{
			return;
		}
		foreach (int containedBoxID in containedBoxIDs)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxID);
			if (box != null)
			{
				box.UnregisterFromRestockZone();
			}
		}
	}

	public void UnregisterFromTemperatureZone()
	{
		if (!(temperatureZone != null))
		{
			return;
		}
		if (containerShelf == null && !SingletonBehaviour<TemperatureZoneManager>.Instance.IsRoomTemperatureZone(temperatureZone))
		{
			base.transform.SetParent(null);
			temperatureZone.UnregisterFreePallet(this);
		}
		foreach (int containedBoxID in containedBoxIDs)
		{
			Box box = SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxID);
			if (box != null)
			{
				box.UnregisterFromTemperatureZone();
			}
		}
	}

	public bool CanPack()
	{
		return false;
	}

	public void DeleteAllData()
	{
		GenericDataSerializer.DeleteKey(PALLET_POSITION_KEY + palletID);
		GenericDataSerializer.DeleteKey(PALLET_ROTATION_KEY + palletID);
		GenericDataSerializer.DeleteKey("PALLET_BOXES" + palletID);
		GenericDataSerializer.DeleteKey("PALLET_PRODUCT_TYPE" + palletID);
	}

	public override float GetInteractionDistance()
	{
		return 6f;
	}

	public BoxCollider SnappableBoxCollider()
	{
		return null;
	}

	public bool PlacedBefore()
	{
		return true;
	}

	public virtual bool IsCancelable()
	{
		return false;
	}
}
