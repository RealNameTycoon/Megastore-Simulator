using System;
using UnityEngine;

public class VehichleInteractable : Interactable
{
	[SerializeField]
	private Transform turningAnchor;

	[SerializeField]
	private VehicleType vehicleType;

	[SerializeField]
	private int vehicleID;

	[SerializeField]
	private float speedMultiplier = 1f;

	[SerializeField]
	private bool canSprint = true;

	protected bool isDriving;

	private const string POSITION_KEY = "Rotation";

	private const string ROTATION_KEY = "Position";

	protected Vector3 initialPosition;

	protected Vector3 initialEulerAngles;

	private bool initialPositionSet;

	public bool CanSprint => canSprint;

	public int VehicleID => vehicleID;

	public VehicleType VehicleType => vehicleType;

	protected virtual void Awake()
	{
		SetResetPosition();
	}

	public void SetResetPosition()
	{
		if (!initialPositionSet)
		{
			initialPosition = base.transform.position;
			initialEulerAngles = base.transform.localEulerAngles;
			initialPositionSet = true;
		}
	}

	public virtual string GetPositionKey()
	{
		return vehicleType.ToString() + "Rotation" + vehicleID;
	}

	public virtual string GetRotationKey()
	{
		return vehicleType.ToString() + "Position" + vehicleID;
	}

	public bool IsDriving()
	{
		return isDriving;
	}

	public Transform GetTurningAnchor()
	{
		return turningAnchor;
	}

	public virtual void StartAnimation()
	{
	}

	public virtual void StopAnimation()
	{
	}

	public virtual bool IsAnimating()
	{
		return false;
	}

	public virtual bool ContainsBox(int id)
	{
		return false;
	}

	public virtual bool PlaceBoxByClick(Box box)
	{
		return false;
	}

	public virtual void RefreshButtons()
	{
	}

	public virtual void RefreshButtonsBoxHovered()
	{
	}

	public virtual void RefreshButtonsPalletHovered(Action palletTakeAction)
	{
	}

	public virtual void SaveLocation()
	{
		GenericDataSerializer.Save(GetPositionKey(), base.transform.position);
		GenericDataSerializer.Save(GetRotationKey(), base.transform.localEulerAngles.y);
	}

	public new virtual LayerMask GetInteractableLayers()
	{
		int num = 1 << RayShooter.PICKABLE_LAYER;
		int num2 = 1 << RayShooter.VEHICLE_CLICKABLE_LAYER;
		return num | num2;
	}

	public virtual float SpeedMultiplier()
	{
		return speedMultiplier;
	}

	public virtual void ResetToInitialPosition()
	{
		base.transform.position = initialPosition;
		base.transform.localEulerAngles = initialEulerAngles;
		SaveLocation();
	}

	public virtual void SetPosition(Vector3 position, Vector3 eulerAngles)
	{
		base.transform.position = position;
		base.transform.localEulerAngles = eulerAngles;
		SaveLocation();
	}

	public virtual bool CanSell()
	{
		return true;
	}

	public virtual void OnSell()
	{
	}
}
