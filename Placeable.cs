using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Placeable : MonoBehaviour
{
	[SerializeField]
	protected PlaceableType type;

	[SerializeField]
	private Moveable moveable;

	[SerializeField]
	private GameObject shelfParent;

	[SerializeField]
	private Outline outline;

	[SerializeField]
	private List<Shelf> shelves;

	[SerializeField]
	private Transform pickupPoint;

	[SerializeField]
	private ParticleSystem fireParticleSystem;

	[SerializeField]
	private Transform animationComponent;

	[SerializeField]
	private Transform animationComponentTarget;

	[SerializeField]
	private Transform dropPosition;

	[SerializeField]
	private Transform staffInteractionPoint;

	[SerializeField]
	private bool customOcculisionEnabled;

	[SerializeField]
	private bool cullPriceLabels;

	[SerializeField]
	private float customOcculisionAngle;

	[SerializeField]
	private float customOcculisionDistance;

	private Employee awaitingEmployee;

	private const string PLACEABLE_POSITION_KEY = "PlaceablePosition";

	private const string PLACEABLE_ROTATION_KEY = "PlaceableRotation";

	private int placeableID = -1;

	private Action onPlacementEnded;

	public bool isReserved;

	private Customer currentCustomer;

	protected bool isHovered;

	private bool productsCulled;

	private bool isSavePositionCorrupted;

	public Transform DropPosition => dropPosition;

	public bool CustomOcculisionEnabled => customOcculisionEnabled;

	public float CustomOcculisionAngle => customOcculisionAngle;

	public float MinDistanceToStartOcculision => customOcculisionDistance;

	public Transform StaffInteractionPoint => staffInteractionPoint;

	public bool IsBurnable => fireParticleSystem != null;

	public Shelf TopShelf => shelves[shelves.Count - 1];

	public Outline Outline => outline;

	public PlaceableType Type => type;

	public int PlaceableID => placeableID;

	public Transform PickupPoint => pickupPoint;

	public List<Shelf> Shelves => shelves;

	public bool IsReserved => isReserved;

	public Moveable Moveable => moveable;

	public bool IsSavePositionCorrupted => isSavePositionCorrupted;

	public void SetReserved(Customer customer, bool reserved)
	{
		isReserved = reserved;
		if (reserved)
		{
			currentCustomer = customer;
		}
		else
		{
			currentCustomer = null;
		}
	}

	public Customer GetCurrentCustomer()
	{
		return currentCustomer;
	}

	public void OccupyByStaff(Employee employee)
	{
		awaitingEmployee = employee;
	}

	public bool IsOccupiedByStaff()
	{
		return awaitingEmployee != null;
	}

	public virtual bool IsServingPlaceable()
	{
		return false;
	}

	public void StartNewPlacement(Action onPlacementEnded = null, Action onCancelPlacement = null)
	{
		DisplayMoveable(display: true);
		SingletonBehaviour<PlacementManager>.Instance.StartPlacement(this, onCancelPlacement);
		this.onPlacementEnded = onPlacementEnded;
	}

	public int ContainedProductCount()
	{
		int num = 0;
		for (int i = 0; i < shelves.Count; i++)
		{
			num += shelves[i].GetProductCount();
		}
		return num;
	}

	protected bool AllShelvesAreEmpty()
	{
		bool result = true;
		for (int i = 0; i < shelves.Count; i++)
		{
			if (shelves[i].ContainedProductType() != ProductType.NONE)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public Shelf GetRandomAvailableShelf()
	{
		List<Shelf> list = new List<Shelf>();
		for (int i = 0; i < shelves.Count; i++)
		{
			if (shelves[i].IsProductAvailable())
			{
				list.Add(shelves[i]);
			}
		}
		return list.GetRandomElement();
	}

	public bool HasAvailableProduct()
	{
		for (int i = 0; i < shelves.Count; i++)
		{
			if (shelves[i].IsProductAvailable())
			{
				return true;
			}
		}
		return false;
	}

	public void Animate()
	{
		if (!(animationComponent == null))
		{
			Vector3 localEulerAngles = animationComponentTarget.localEulerAngles;
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			animationComponent.DOLocalRotate(localEulerAngles, 0.7f).SetLoops(2, LoopType.Yoyo);
		}
	}

	public bool IsPlacementValid()
	{
		return moveable.IsValid;
	}

	public void OnPlacementEnded(bool isCanceled = false)
	{
		if (!isCanceled)
		{
			EventManager.NotifyEvent(PlaceableEvents.PLACEMENT_ENDED, this);
		}
		onPlacementEnded?.Invoke();
		DisplayMoveable(display: false);
		moveable.ResetCollidedEntities();
		SavePosition();
	}

	private void DisplayMoveable(bool display)
	{
		moveable.gameObject.SetActive(display);
		shelfParent.gameObject.SetActive(!display);
	}

	protected bool IsPlacing()
	{
		return moveable.gameObject.activeSelf;
	}

	public virtual void InitializeOldPlaceable(int id, bool isPacked = false)
	{
		placeableID = id;
		if (isPacked)
		{
			return;
		}
		if (!GenericDataSerializer.HasKey("PlaceablePosition" + type.ToString() + id))
		{
			if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.GROCERY && type == PlaceableType.WIDE_SHELF)
			{
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialShelfTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialShelfTransform.rotation;
			}
			else if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.GROCERY && type == PlaceableType.PRODUCE_SHELF_SMALL)
			{
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialVegetableShelfTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialVegetableShelfTransform.rotation;
			}
			else if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.BAKERY && type == PlaceableType.BAKERY_SHELF)
			{
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialBakeryShelfTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialBakeryShelfTransform.rotation;
			}
			else if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.TOY && type == PlaceableType.WALL_SHELF)
			{
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialToyShelfTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialToyShelfTransform.rotation;
			}
			else
			{
				if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup != ProductGroup.CLOTHING || type != PlaceableType.FOLDED_CLOTH_RACK_2)
				{
					isSavePositionCorrupted = true;
					return;
				}
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialFoldedRackTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialFoldedRackTransform.rotation;
			}
		}
		else
		{
			base.transform.position = GenericDataSerializer.Load("PlaceablePosition" + type.ToString() + id);
			base.transform.rotation = GenericDataSerializer.LoadQuaternion("PlaceableRotation" + type.ToString() + id);
			if (Mathf.Approximately(base.transform.position.x, 0f))
			{
				Mathf.Approximately(base.transform.position.z, 0f);
			}
		}
		for (int i = 0; i < shelves.Count; i++)
		{
			shelves[i].Initialize();
		}
		SetFloorLayers();
	}

	public virtual void InitializeNewPlaceable(int id)
	{
		placeableID = id;
		for (int i = 0; i < shelves.Count; i++)
		{
			shelves[i].RegisterRestockingJobs();
		}
	}

	public void SetFloorLayers()
	{
		moveable.SetPlaceableFloorLayers(GetPlaceableFloorLayers());
	}

	public virtual LayerMask GetPlaceableFloorLayers()
	{
		return 1 << PlacementManager.FLOOR_LAYER;
	}

	public void SavePosition()
	{
		GenericDataSerializer.Save("PlaceablePosition" + type.ToString() + placeableID, base.transform.position);
		GenericDataSerializer.Save("PlaceableRotation" + type.ToString() + placeableID, base.transform.rotation);
	}

	public void StartFire()
	{
		fireParticleSystem.gameObject.SetActive(value: true);
		fireParticleSystem.Play();
	}

	public void StopFire()
	{
		fireParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
	}

	public virtual void OnShelfHoverStarted()
	{
		isHovered = true;
	}

	public virtual void OnShelfHoverEnded()
	{
		isHovered = false;
	}

	public bool IsBakeryPlaceable()
	{
		if (type != PlaceableType.BAKERY_SHELF)
		{
			return type == PlaceableType.BAKERY_SHELF_WIDE;
		}
		return true;
	}

	public void Pack()
	{
		if (CanPack())
		{
			for (int i = 0; i < shelves.Count; i++)
			{
				shelves[i].OnPacked();
			}
			if (awaitingEmployee != null)
			{
				awaitingEmployee.OnOccupiedPlaceableRemoved();
				awaitingEmployee = null;
			}
			SingletonBehaviour<SpawnManager>.Instance.PackPlaceable(this);
		}
	}

	public virtual bool CanPack()
	{
		bool flag = true;
		for (int i = 0; i < shelves.Count; i++)
		{
			if (shelves[i].HasProductLabel())
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_product_label", base.transform);
		}
		if (isReserved)
		{
			flag = false;
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pack_error_reserved", base.transform);
		}
		return flag;
	}

	public virtual List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		return new List<(KeyCode, (string, Action))> { (KeyCode.F, ("pack", delegate
		{
			Pack();
		})) };
	}

	public void CullAllProducts(bool cull)
	{
		if (productsCulled != cull)
		{
			for (int i = 0; i < shelves.Count; i++)
			{
				shelves[i].CullAllProducts(cull, cullPriceLabels);
			}
			productsCulled = cull;
		}
	}

	public bool PlacedBefore()
	{
		return placeableID != -1;
	}

	public float GetPlacementRadius()
	{
		return PlacementManager.PLACEMENT_RADIUS;
	}
}
