using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputerTable : MonoBehaviour, MoveableObjectInterface
{
	[SerializeField]
	private ComputerUI computerUI;

	[SerializeField]
	private Canvas computerCanvas;

	[SerializeField]
	private Image computerCanvasBlocker;

	[SerializeField]
	private CanvasGroup computerCanvasBlockerCanvasGroup;

	[SerializeField]
	private Moveable moveable;

	[SerializeField]
	private GameObject solidObject;

	[SerializeField]
	private int tableID;

	private const string COMPUTER_TABLE_POSITION_KEY = "COMPUTER_TABLE_POSITION_KEY";

	private const string COMPUTER_TABLE_ROTATION_KEY = "COMPUTER_TABLE_ROTATION_KEY";

	private Action onPlacementEnded;

	private void Awake()
	{
		List<(KeyCode, (string, Action))> list = new List<(KeyCode, (string, Action))>();
		list.Add((KeyCode.Mouse1, ("move", delegate
		{
			StartNewPlacement();
		})));
		computerUI.SetAdditionalActions(list);
		if (GenericDataSerializer.HasKey("COMPUTER_TABLE_POSITION_KEY" + tableID))
		{
			Vector3 position = GenericDataSerializer.Load("COMPUTER_TABLE_POSITION_KEY" + tableID);
			Quaternion rotation = GenericDataSerializer.LoadQuaternion("COMPUTER_TABLE_ROTATION_KEY" + tableID);
			base.transform.position = position;
			base.transform.rotation = rotation;
		}
		LayerMask placeableFloorLayers = (1 << PlacementManager.FLOOR_LAYER) | (1 << PlacementManager.SERVICE_ROOM_FLOOR_LAYER) | (1 << PlacementManager.STORAGE_FLOOR_LAYER);
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
	}

	private void DisplayMoveable(bool display)
	{
		moveable.gameObject.SetActive(display);
		solidObject.gameObject.SetActive(!display);
	}

	public void SwitchLook(bool toSolidObject)
	{
		DisplayMoveable(!toSolidObject);
	}

	public void OnPlacementEnded()
	{
		StartCoroutine(EnableClickablesRoutine());
		onPlacementEnded?.Invoke();
	}

	private IEnumerator EnableClickablesRoutine()
	{
		computerCanvasBlockerCanvasGroup.alpha = 1f;
		yield return new WaitForEndOfFrame();
		computerUI.gameObject.SetActive(value: true);
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	public void SavePosition()
	{
		GenericDataSerializer.Save("COMPUTER_TABLE_POSITION_KEY" + tableID, base.transform.position);
		GenericDataSerializer.Save("COMPUTER_TABLE_ROTATION_KEY" + tableID, base.transform.rotation);
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public void StartNewPlacement(Action onPlacementEnded = null)
	{
		this.onPlacementEnded = onPlacementEnded;
		computerCanvasBlockerCanvasGroup.alpha = 0f;
		computerUI.gameObject.SetActive(value: false);
		SingletonBehaviour<PlacementManager>.Instance.StartPlacement(moveable, facePlayer: false);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public bool CanPack()
	{
		return false;
	}

	public bool PlacedBefore()
	{
		return true;
	}
}
