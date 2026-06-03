using System;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour, MoveableObjectInterface, MoveableHolderInterface
{
	[SerializeField]
	protected FurnitureType type;

	[SerializeField]
	protected Moveable moveable;

	[SerializeField]
	private GameObject solidObject;

	[SerializeField]
	private Transform staffInteractionPoint;

	private const string FURNITURE_POSITION_KEY = "FurniturePosition";

	private const string FURNITURE_ROTATION_KEY = "FurnitureRotation";

	[SerializeField]
	private int furnitureID = -1;

	private Action onPlacementEnded;

	protected const string DISPLAYED_ID_KEY = "displayed_id_";

	public Transform StaffInteractionPoint => staffInteractionPoint;

	public FurnitureType Type => type;

	public int FurnitureID => furnitureID;

	public Moveable Moveable => moveable;

	public int GetDisplayedID()
	{
		return GenericDataSerializer.LoadInt("displayed_id_" + Type.ToString() + FurnitureID, FurnitureID + 1);
	}

	public void SetDisplayedID(int id)
	{
		GenericDataSerializer.SaveInt("displayed_id_" + Type.ToString() + FurnitureID, id);
	}

	public virtual void StartNewPlacement(Action onPlacementEnded = null, Action onCancelPlacement = null)
	{
		this.onPlacementEnded = onPlacementEnded;
		SingletonBehaviour<PlacementManager>.Instance.StartPlacement(moveable, furnitureID == -1, null, onCancelPlacement);
	}

	private void DisplayMoveable(bool display)
	{
		moveable.gameObject.SetActive(display);
		solidObject.gameObject.SetActive(!display);
	}

	public bool IsPlacementValid()
	{
		return moveable.IsValid;
	}

	public virtual void InitializeOldFurniture(int id, bool isPacked = false)
	{
		furnitureID = id;
		if (!isPacked)
		{
			if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.BAKERY && type == FurnitureType.OVEN && !GenericDataSerializer.HasKey("FurniturePosition" + type.ToString() + id))
			{
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialOvenTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialOvenTransform.rotation;
			}
			if (type == FurnitureType.CHECKOUT_DESK && id == 0 && !GenericDataSerializer.HasKey("FurniturePosition" + type.ToString() + id))
			{
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialCheckoutDeskTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialCheckoutDeskTransform.rotation;
			}
			if (GameManager.GetSaveVersion() >= 3 && type == FurnitureType.WALK_IN_FREEZER_SMALL && !GenericDataSerializer.HasKey("FurniturePosition" + type.ToString() + id))
			{
				base.transform.position = SingletonBehaviour<SpawnManager>.Instance.InitialWalkInFreezerTransform.position;
				base.transform.rotation = SingletonBehaviour<SpawnManager>.Instance.InitialWalkInFreezerTransform.rotation;
			}
			else if (GenericDataSerializer.HasKey("FurniturePosition" + type.ToString() + id))
			{
				base.transform.position = GenericDataSerializer.Load("FurniturePosition" + type.ToString() + id);
				base.transform.rotation = GenericDataSerializer.LoadQuaternion("FurnitureRotation" + type.ToString() + id);
			}
			else if (GenericDataSerializer.HasKey("FurniturePosition" + type.ToString() + id) || type != FurnitureType.BOX_SHELF)
			{
				_ = type;
				_ = 10;
			}
			SetFloorLayers();
		}
	}

	public virtual void InitializeNewFurniture(int id)
	{
		furnitureID = id;
	}

	public virtual void SetFloorLayers()
	{
		LayerMask placeableFloorLayers = 1 << PlacementManager.FLOOR_LAYER;
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
	}

	public virtual void SwitchLook(bool toSolidObject)
	{
		DisplayMoveable(!toSolidObject);
	}

	public virtual void OnPlacementEnded()
	{
		onPlacementEnded?.Invoke();
	}

	public void SavePosition()
	{
		GenericDataSerializer.Save("FurniturePosition" + type.ToString() + furnitureID, base.transform.position);
		GenericDataSerializer.Save("FurnitureRotation" + type.ToString() + furnitureID, base.transform.rotation);
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public Moveable GetMoveable()
	{
		return moveable;
	}

	public virtual List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		if (type == FurnitureType.FITTING_ROOM || type == FurnitureType.MANNEQUIN_SPORT)
		{
			return new List<(KeyCode, (string, Action))> { (KeyCode.F, ("pack", delegate
			{
				Pack();
			})) };
		}
		return null;
	}

	public virtual bool CanPack()
	{
		if (type != FurnitureType.FITTING_ROOM)
		{
			return type == FurnitureType.MANNEQUIN_SPORT;
		}
		return true;
	}

	public virtual void Pack()
	{
		if (CanPack())
		{
			SingletonBehaviour<SpawnManager>.Instance.PackFurniture(this);
		}
	}

	public BoxCollider SnappableBoxCollider()
	{
		return null;
	}

	public bool PlacedBefore()
	{
		return furnitureID != -1;
	}

	public virtual float GetPlacementRadius()
	{
		return PlacementManager.PLACEMENT_RADIUS;
	}
}
