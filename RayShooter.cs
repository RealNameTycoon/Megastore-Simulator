using System;
using System.Collections.Generic;
using UnityEngine;

public class RayShooter : SingletonBehaviour<RayShooter>
{
	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private Camera tppCamera;

	public static int PLACEABLE_LAYER = 6;

	public static int PRODUCT_LAYER = 13;

	public static int TRASH_LAYER = 14;

	public static int FURNITURE_LAYER = 11;

	public static int CLICKABLE_LAYER = 7;

	public static int PICKABLE_LAYER = 15;

	public static int VEHICLE_CLICKABLE_LAYER = 26;

	private static int CHECKOUT_CLICKABLE_LAYER = 28;

	public static int PALLET_LAYER = 29;

	public static int PALLET_SHELF_LAYER = 30;

	public static int WALL_LAYER = 9;

	public static int FLOOR_LAYER = 10;

	private static int STORAGE_FLOOR_LAYER = 19;

	private static int VEHICLE_FLOOR_LAYER = 21;

	public static int SERVICE_ROOM_FLOOR_LAYER = 18;

	public static int TRAY_INTERACTABLE_LAYER = 23;

	public static string PRICE_LABEL_TAG = "priceLabel";

	public static string CHOPPING_STAND_TAG = "choppingStandInteractable";

	public static string CHECKOUT_CLICKABLE_TAG = "checkoutInteractable";

	private Interactable hoveredInteractable;

	public static float DEFAULT_INTERACTION_DETECTION_DISTANCE = 4f;

	public static float CHECKING_INTERACTION_DETECTION_DISTANCE = 12f;

	private static LayerMask groundLayerMask = (1 << FLOOR_LAYER) | (1 << STORAGE_FLOOR_LAYER) | (1 << VEHICLE_FLOOR_LAYER) | (1 << SERVICE_ROOM_FLOOR_LAYER) | (1 << PlacementManager.AROUND_STORE_LAYER);

	public static LayerMask GroundLayerMask => groundLayerMask;

	public Camera MainCamera => mainCamera;

	private float DetectionDistance => CHECKING_INTERACTION_DETECTION_DISTANCE;

	private new void Awake()
	{
		base.Awake();
		EventManager.AddListener(GameEvents.BOX_OPENED, ImitateHover);
		EventManager.AddListener(GameEvents.BOX_PICKED_UP, ImitateHover);
		EventManager.AddListener(GameEvents.PALLET_PICKED_UP, ImitateHover);
		EventManager.AddListener(GameEvents.TRAY_PICKED_UP, ImitateHover);
		EventManager.AddListener(GameEvents.GENERIC_BOX_PICKED_UP, ImitateHover);
		EventManager.AddListener(PlaceableEvents.PLACEMENT_STARTED, OnPlacementStarted);
		EventManager.AddListener(PlaceableEvents.FURNITURE_PLACEMENT_STARTED, OnPlacementStarted);
		EventManager.AddListener(UIEvents.INTERACTION_CLICKED, OnInteractionClicked);
	}

	private void OnInteractionClicked()
	{
		if (ShouldDetectClickUI() && hoveredInteractable != null)
		{
			hoveredInteractable.OnMouseButtonDown();
		}
	}

	private void OnPlacementStarted()
	{
		if (hoveredInteractable != null)
		{
			hoveredInteractable.OnMouseHoverEnded();
			hoveredInteractable = null;
		}
	}

	private bool IsInInteractionDistance(Interactable interactable, Vector3 hitPoint)
	{
		return Vector3.Distance(MainCamera.transform.position, hitPoint) <= interactable.GetInteractionDistance();
	}

	private int GetAllLayers()
	{
		int num = 1 << PLACEABLE_LAYER;
		int num2 = 1 << FURNITURE_LAYER;
		int num3 = 1 << CLICKABLE_LAYER;
		int num4 = 1 << PICKABLE_LAYER;
		int num5 = 1 << TRASH_LAYER;
		int num6 = 1 << VEHICLE_CLICKABLE_LAYER;
		int num7 = 1 << PALLET_LAYER;
		int num8 = 1 << PALLET_SHELF_LAYER;
		return num | num2 | num3 | num4 | num5 | num6 | num7 | num8;
	}

	private void Start()
	{
		InputManager instance = SingletonBehaviour<InputManager>.Instance;
		instance.leftMouseDownAction = (Action)Delegate.Combine(instance.leftMouseDownAction, new Action(OnLeftMouseDown));
		InputManager instance2 = SingletonBehaviour<InputManager>.Instance;
		instance2.leftMouseUpAction = (Action)Delegate.Combine(instance2.leftMouseUpAction, new Action(OnLeftMouseUp));
		InputManager instance3 = SingletonBehaviour<InputManager>.Instance;
		instance3.rightMouseDownAction = (Action)Delegate.Combine(instance3.rightMouseDownAction, new Action(OnRightMouseDown));
	}

	public void RemoveAllListeners()
	{
		InputManager instance = SingletonBehaviour<InputManager>.Instance;
		instance.leftMouseDownAction = (Action)Delegate.Remove(instance.leftMouseDownAction, new Action(OnLeftMouseDown));
		InputManager instance2 = SingletonBehaviour<InputManager>.Instance;
		instance2.leftMouseUpAction = (Action)Delegate.Remove(instance2.leftMouseUpAction, new Action(OnLeftMouseUp));
		InputManager instance3 = SingletonBehaviour<InputManager>.Instance;
		instance3.rightMouseDownAction = (Action)Delegate.Remove(instance3.rightMouseDownAction, new Action(OnRightMouseDown));
	}

	private void OnLeftMouseDown()
	{
		if (ShouldDetectHover())
		{
			Interactable clickedInteractable = GetClickedInteractable();
			if (clickedInteractable != null)
			{
				clickedInteractable.OnMouseButtonDown();
			}
		}
	}

	private void OnLeftMouseUp()
	{
		if (ShouldDetectHover())
		{
			Interactable clickedInteractable = GetClickedInteractable();
			if (clickedInteractable != null)
			{
				clickedInteractable.OnMouseButtonUp();
			}
		}
	}

	private void OnRightMouseDown()
	{
		if (ShouldDetectHover())
		{
			Interactable clickedInteractable = GetClickedInteractable();
			if (clickedInteractable != null)
			{
				clickedInteractable.OnMouseRMBDown();
			}
		}
	}

	private Interactable GetClickedInteractable()
	{
		if (!ShouldDetectClick())
		{
			return null;
		}
		int num = 1 << PLACEABLE_LAYER;
		_ = FURNITURE_LAYER;
		_ = CLICKABLE_LAYER;
		int num2 = 1 << CHECKOUT_CLICKABLE_LAYER;
		int num3 = 1 << PICKABLE_LAYER;
		int num4 = 1 << TRASH_LAYER;
		int num5 = 1 << VEHICLE_CLICKABLE_LAYER;
		int num6 = 1 << PALLET_LAYER;
		int num7 = 1 << TRAY_INTERACTABLE_LAYER;
		List<string> list = null;
		int layerMask;
		if (FireExtinguisher.Instance.IsPicked)
		{
			layerMask = num;
		}
		else if (Megaphone.Instance.IsPicked)
		{
			layerMask = Megaphone.Instance.GetInteractableLayers();
		}
		else if (HandScanner.Instance.IsPicked)
		{
			layerMask = HandScanner.Instance.GetInteractableLayers();
			list = HandScanner.Instance.GetInteractableTags();
		}
		else if (PaintRoller.Instance.IsPicked)
		{
			layerMask = PaintRoller.Instance.GetInteractableLayers();
		}
		else if (SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			layerMask = num7 | num;
		}
		else if (GenericBox.Instance.IsPicked)
		{
			layerMask = num;
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			layerMask = num | num4 | num6 | num5;
		}
		else if (SingletonBehaviour<PlayerSittingManager>.Instance.IsSittingOnCheckoutDesk)
		{
			layerMask = num2;
			list = SingletonBehaviour<PlayerSittingManager>.Instance.GetCheckoutInteractableTags();
		}
		else if (!SingletonBehaviour<PlayerSittingManager>.Instance.IsSittingOnChoppingStand)
		{
			layerMask = ((!SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle) ? GetAllLayers() : ((int)SingletonBehaviour<VehicleManager>.Instance.TakenVehicle.GetInteractableLayers()));
		}
		else
		{
			layerMask = num2;
			list = SingletonBehaviour<PlayerSittingManager>.Instance.GetChoppingStandInteractableTags();
		}
		Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hitInfo, DetectionDistance, layerMask))
		{
			if (list != null && !list.Contains(hitInfo.collider.gameObject.tag))
			{
				return null;
			}
			Interactable component = hitInfo.collider.gameObject.GetComponent<Interactable>();
			if (component == null || !IsInInteractionDistance(component, hitInfo.point))
			{
				return null;
			}
			if (hitInfo.collider.isTrigger && !SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
			{
				layerMask = num3;
				if (Physics.Raycast(ray, out var hitInfo2, DetectionDistance, layerMask))
				{
					hitInfo = hitInfo2;
				}
			}
			return component;
		}
		return null;
	}

	private void FixedUpdate()
	{
		if (!ShouldDetectHover())
		{
			return;
		}
		int num = 1 << PLACEABLE_LAYER;
		_ = FURNITURE_LAYER;
		_ = CLICKABLE_LAYER;
		int num2 = 1 << CHECKOUT_CLICKABLE_LAYER;
		int num3 = 1 << PICKABLE_LAYER;
		int num4 = 1 << TRASH_LAYER;
		int num5 = 1 << VEHICLE_CLICKABLE_LAYER;
		int num6 = 1 << PALLET_LAYER;
		int num7 = 1 << TRAY_INTERACTABLE_LAYER;
		List<string> list = null;
		int layerMask;
		if (FireExtinguisher.Instance.IsPicked)
		{
			layerMask = num;
		}
		else if (Megaphone.Instance.IsPicked)
		{
			layerMask = Megaphone.Instance.GetInteractableLayers();
		}
		else if (PaintRoller.Instance.IsPicked)
		{
			layerMask = PaintRoller.Instance.GetInteractableLayers();
		}
		else if (HandScanner.Instance.IsPicked)
		{
			layerMask = HandScanner.Instance.GetInteractableLayers();
			list = HandScanner.Instance.GetInteractableTags();
		}
		else if (SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			layerMask = num7 | num;
		}
		else if (GenericBox.Instance.IsPicked)
		{
			layerMask = num;
		}
		else if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			layerMask = num | num4 | num6 | num5;
		}
		else if (SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
		{
			layerMask = num4;
		}
		else if (SingletonBehaviour<PlayerSittingManager>.Instance.IsSittingOnCheckoutDesk)
		{
			layerMask = num2;
			list = SingletonBehaviour<PlayerSittingManager>.Instance.GetCheckoutInteractableTags();
		}
		else if (!SingletonBehaviour<PlayerSittingManager>.Instance.IsSittingOnChoppingStand)
		{
			layerMask = ((!SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle) ? GetAllLayers() : ((int)SingletonBehaviour<VehicleManager>.Instance.TakenVehicle.GetInteractableLayers()));
		}
		else
		{
			layerMask = num2;
			list = SingletonBehaviour<PlayerSittingManager>.Instance.GetChoppingStandInteractableTags();
		}
		if (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.TransformDirection(Vector3.forward), out var hitInfo, DetectionDistance, layerMask))
		{
			if (list != null && !list.Contains(hitInfo.collider.gameObject.tag))
			{
				FinishHover();
				return;
			}
			Interactable component = hitInfo.collider.gameObject.GetComponent<Interactable>();
			if (component == null)
			{
				FinishHover();
				return;
			}
			if (!IsInInteractionDistance(component, hitInfo.point))
			{
				FinishHover();
				return;
			}
			if (hitInfo.collider.isTrigger && !SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<PalletManager>.Instance.IsPalletPicked)
			{
				layerMask = num3;
				if (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.TransformDirection(Vector3.forward), out var hitInfo2, DetectionDistance, layerMask))
				{
					hitInfo = hitInfo2;
				}
			}
			if (!(hoveredInteractable != null) || !hitInfo.collider.gameObject.Equals(hoveredInteractable.gameObject))
			{
				if (hoveredInteractable != null)
				{
					hoveredInteractable.OnMouseHoverEnded();
				}
				hoveredInteractable = component;
				hoveredInteractable.OnMouseHoverStarted();
			}
		}
		else
		{
			FinishHover();
		}
	}

	private void FinishHover()
	{
		if (hoveredInteractable != null)
		{
			hoveredInteractable.OnMouseHoverEnded();
			hoveredInteractable = null;
			if (SingletonBehaviour<HotKeyManager>.Instance.SelectedHotkeyIndex != -1)
			{
				SingletonBehaviour<HotKeyManager>.Instance.RepaintButtonsForEndHover();
			}
		}
	}

	public void ImitateHover()
	{
		if (hoveredInteractable != null && ShouldDetectHover())
		{
			hoveredInteractable.OnMouseHoverStarted();
		}
	}

	private bool ShouldDetectHover()
	{
		if (SingletonBehaviour<UIManager>.Instance.AllWindowsClosed())
		{
			return !SingletonBehaviour<PlacementManager>.Instance.PlacingObject;
		}
		return false;
	}

	private bool ShouldDetectClick()
	{
		if (ShouldDetectClickUI())
		{
			return !SingletonBehaviour<InputManager>.Instance.IsLastTouchOnUI();
		}
		return false;
	}

	private bool ShouldDetectClickUI()
	{
		if (ShouldDetectHover() && !SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			return !GenericBox.Instance.IsPicked;
		}
		return false;
	}
}
