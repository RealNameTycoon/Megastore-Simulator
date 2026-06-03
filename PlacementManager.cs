using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlacementManager : SingletonBehaviour<PlacementManager>
{
	private Placeable placeableToMove;

	private Moveable objectToMove;

	public static int FLOOR_LAYER = 10;

	public static int SERVICE_ROOM_FLOOR_LAYER = 18;

	public static int STORAGE_FLOOR_LAYER = 19;

	public static int VEHICLE_FLOOR_LAYER = 21;

	public static int AROUND_STORE_LAYER = 3;

	public static float PLACEMENT_RADIUS = 4.5f;

	public static float BIG_ITEM_PLACEMENT_RADIUS = 9f;

	private bool placingObject;

	private bool willPlaceObject;

	private float placeableY;

	private float rotationTargetY;

	private Tweener rotationTween;

	private static float MOVEMENT_SPEED = 12f;

	private static float DEFAULT_ROTATION_STEP = 45f;

	private static float WHEEL_ROTATION_STEP = 5f;

	private List<int> extraLayers = new List<int>();

	private Vector3 bottomPointDelta = Vector3.up * 0.1f;

	private bool isPinned;

	private Vector3 lastPosition = Vector3.zero;

	private Vector3 lastSize = Vector3.zero;

	private Vector3 initialPlacementPosition = Vector3.zero;

	private Quaternion initialPlacementRotation = Quaternion.identity;

	private Action onCancelPlacement;

	public bool PlacingObject
	{
		get
		{
			if (!placingObject)
			{
				return willPlaceObject;
			}
			return true;
		}
	}

	public void SetWillPlaceObject(bool willPlaceObject)
	{
		this.willPlaceObject = willPlaceObject;
	}

	public void StartPlacement(Placeable placeable, Action onCancelPlacement = null)
	{
		this.onCancelPlacement = onCancelPlacement;
		initialPlacementPosition = placeable.transform.position;
		initialPlacementRotation = placeable.transform.localRotation;
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
		{
			{
				KeyCode.R,
				("cancel", CancelPlacement)
			},
			{
				KeyCode.LeftShift,
				("place_hold_snap", null)
			},
			{
				KeyCode.Mouse2,
				("rotate", RotateWheel)
			},
			{
				KeyCode.D,
				("rotate_left", delegate
				{
					TurnLeft(DEFAULT_ROTATION_STEP);
				})
			},
			{
				KeyCode.Q,
				("rotate_right", delegate
				{
					TurnRight(DEFAULT_ROTATION_STEP);
				})
			},
			{
				KeyCode.Mouse0,
				("place", Place)
			}
		});
		placeableToMove = placeable;
		rotationTargetY = placeableToMove.transform.localRotation.eulerAngles.y;
		placeableY = placeable.transform.position.y;
		if (placeable.PlaceableID == -1)
		{
			FacePlayer();
		}
		placingObject = true;
		if (placeable.Type == PlaceableType.VENDING_MACHINE)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("tooltip_place_aroundstore", base.transform);
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("tooltip_place_furniture", base.transform);
		}
		EventManager.NotifyEvent(PlaceableEvents.PLACEMENT_STARTED);
	}

	private void CancelPlacement()
	{
		if (placeableToMove != null)
		{
			if (!placeableToMove.PlacedBefore())
			{
				placingObject = false;
				willPlaceObject = false;
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
				isPinned = false;
				SingletonBehaviour<TooltipUI>.Instance.Close();
				SingletonBehaviour<ButtonsWindow>.Instance.Close();
				if (extraLayers != null)
				{
					extraLayers.Clear();
				}
				placeableToMove.Moveable.ResetCollidedEntities();
				placeableToMove = null;
				onCancelPlacement?.Invoke();
			}
			else
			{
				placeableToMove.transform.position = initialPlacementPosition;
				placeableToMove.transform.localRotation = initialPlacementRotation;
				PlacePlaceable(isCanceled: true);
			}
		}
		else if (objectToMove != null)
		{
			if (!objectToMove.MovedObject.PlacedBefore())
			{
				placingObject = false;
				willPlaceObject = false;
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
				isPinned = false;
				SingletonBehaviour<TooltipUI>.Instance.Close();
				SingletonBehaviour<ButtonsWindow>.Instance.Close();
				if (extraLayers != null)
				{
					extraLayers.Clear();
				}
				objectToMove.ResetCollidedEntities();
				objectToMove = null;
				onCancelPlacement?.Invoke();
			}
			else
			{
				objectToMove.MovedObject.GetTransform().position = initialPlacementPosition;
				objectToMove.MovedObject.GetTransform().localRotation = initialPlacementRotation;
				PlaceMoveable(isCanceled: true);
			}
		}
		onCancelPlacement = null;
	}

	public void RotateWheel()
	{
		float scrollWheelInput = SingletonBehaviour<InputManager>.Instance.ScrollWheelInput;
		if (scrollWheelInput > 0f)
		{
			TurnRight(WHEEL_ROTATION_STEP);
		}
		else if (scrollWheelInput < 0f)
		{
			TurnLeft(WHEEL_ROTATION_STEP);
		}
	}

	public void StartPlacement(Moveable objectToMove, bool facePlayer, List<int> extraLayers = null, Action onCancelPlacement = null)
	{
		this.extraLayers = extraLayers;
		objectToMove.EnableGhost();
		this.onCancelPlacement = onCancelPlacement;
		initialPlacementPosition = objectToMove.MovedObject.GetTransform().position;
		initialPlacementRotation = objectToMove.MovedObject.GetTransform().localRotation;
		if (objectToMove.MovedObject.IsCancelable())
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.R,
					("cancel", CancelPlacement)
				},
				{
					KeyCode.LeftShift,
					("place_hold_snap", null)
				},
				{
					KeyCode.Mouse2,
					("rotate", RotateWheel)
				},
				{
					KeyCode.D,
					("rotate_left", delegate
					{
						TurnLeft(DEFAULT_ROTATION_STEP);
					})
				},
				{
					KeyCode.Q,
					("rotate_right", delegate
					{
						TurnRight(DEFAULT_ROTATION_STEP);
					})
				},
				{
					KeyCode.Mouse0,
					("place", Place)
				}
			});
		}
		else
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.LeftShift,
					("place_hold_snap", null)
				},
				{
					KeyCode.Mouse2,
					("rotate", RotateWheel)
				},
				{
					KeyCode.D,
					("rotate_left", delegate
					{
						TurnLeft(DEFAULT_ROTATION_STEP);
					})
				},
				{
					KeyCode.Q,
					("rotate_right", delegate
					{
						TurnRight(DEFAULT_ROTATION_STEP);
					})
				},
				{
					KeyCode.Mouse0,
					("place", Place)
				}
			});
		}
		this.objectToMove = objectToMove;
		rotationTargetY = objectToMove.MovedObject.GetTransform().localRotation.eulerAngles.y;
		placeableY = objectToMove.MovedObject.GetTransform().position.y;
		if (facePlayer)
		{
			FacePlayer();
		}
		placingObject = true;
		SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("tooltip_place_object", base.transform);
		EventManager.NotifyEvent(PlaceableEvents.OBJECT_PLACEMENT_STARTED);
	}

	private void TurnLeft(float degree)
	{
		rotationTween.Kill();
		rotationTargetY += degree;
		rotationTargetY %= 360f;
		Transform targetPlaceable = GetTargetPlaceable();
		rotationTween = targetPlaceable.transform.DOLocalRotate(new Vector3(targetPlaceable.transform.localRotation.eulerAngles.x, rotationTargetY, targetPlaceable.transform.localRotation.eulerAngles.z), 180f).SetSpeedBased(isSpeedBased: true);
	}

	private void Place()
	{
		if (placingObject)
		{
			if (placeableToMove != null && placeableToMove.IsPlacementValid())
			{
				PlacePlaceable();
			}
			else if (objectToMove != null && objectToMove.IsValid)
			{
				PlaceMoveable();
			}
		}
	}

	private void PlacePlaceable(bool isCanceled = false)
	{
		StartCoroutine(DisablePlacementRoutine());
		isPinned = false;
		SingletonBehaviour<TooltipUI>.Instance.Close();
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		if (extraLayers != null)
		{
			extraLayers.Clear();
		}
		if (placeableToMove.Type == PlaceableType.FISH_STAND)
		{
			SingletonWindow<TutorialVideoWindowUI>.Instance.Open(TutorialVideoWindowType.FISH_STAND_TUTORIAL);
		}
		placeableToMove.OnPlacementEnded(isCanceled);
		placeableToMove = null;
	}

	private void PlaceMoveable(bool isCanceled = false)
	{
		StartCoroutine(DisablePlacementRoutine());
		isPinned = false;
		SingletonBehaviour<TooltipUI>.Instance.Close();
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		if (extraLayers != null)
		{
			extraLayers.Clear();
		}
		objectToMove.OnPlacementEnded(isCanceled);
		objectToMove = null;
	}

	private IEnumerator DisablePlacementRoutine()
	{
		yield return new WaitForEndOfFrame();
		placingObject = false;
		willPlaceObject = false;
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		EventManager.NotifyEvent(PlaceableEvents.PLACEMENT_FINISHED);
	}

	private void TurnRight(float degree)
	{
		rotationTween.Kill();
		rotationTargetY -= degree;
		rotationTargetY %= 360f;
		Transform targetPlaceable = GetTargetPlaceable();
		rotationTween = targetPlaceable.transform.DOLocalRotate(new Vector3(targetPlaceable.transform.localRotation.eulerAngles.x, rotationTargetY, targetPlaceable.transform.localRotation.eulerAngles.z), 180f).SetSpeedBased(isSpeedBased: true);
	}

	private void FacePlayer()
	{
		Transform targetPlaceable = GetTargetPlaceable();
		float y = SingletonBehaviour<PlayerMove>.Instance.CameraParentEuler.y;
		y = Mathf.Round(y / 90f) * 90f + 180f;
		rotationTargetY = y % 360f;
		targetPlaceable.transform.localEulerAngles = new Vector3(targetPlaceable.localRotation.eulerAngles.x, rotationTargetY, targetPlaceable.localRotation.eulerAngles.z);
	}

	private Transform GetTargetPlaceable()
	{
		if (placeableToMove != null)
		{
			return placeableToMove.transform;
		}
		if (objectToMove != null)
		{
			return objectToMove.MovedObject.GetTransform();
		}
		return null;
	}

	private void Update()
	{
		if (!placingObject)
		{
			return;
		}
		int num = 1 << FLOOR_LAYER;
		int num2 = 1 << VEHICLE_FLOOR_LAYER;
		int num3 = 1 << STORAGE_FLOOR_LAYER;
		int num4 = 1 << SERVICE_ROOM_FLOOR_LAYER;
		int num5 = 1 << AROUND_STORE_LAYER;
		int num6 = num | num2 | num3 | num4 | num5;
		if (extraLayers != null)
		{
			for (int i = 0; i < extraLayers.Count; i++)
			{
				num6 |= 1 << extraLayers[i];
			}
		}
		if (Physics.Raycast(base.transform.position, base.transform.TransformDirection(Vector3.forward), out var hitInfo, GetPlacementRadius() + 1f, num6))
		{
			Transform targetPlaceable = GetTargetPlaceable();
			if (targetPlaceable == null)
			{
				return;
			}
			if (!IsStationary() && Mathf.Approximately(targetPlaceable.position.y, hitInfo.point.y))
			{
				SetStationary(stationary: true);
			}
			float num7 = Vector3.Distance(base.transform.position, hitInfo.point);
			if (num7 < GetPlacementRadius() + 0.5f)
			{
				Vector3 vector2;
				if (num7 > GetPlacementRadius())
				{
					Vector3 vector = base.transform.position + GetPlacementRadius() * base.transform.forward;
					vector2 = new Vector3(vector.x, hitInfo.point.y, vector.z);
				}
				else
				{
					vector2 = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
				}
				_ = (vector2 - targetPlaceable.position).normalized;
				TryMoveGhost(targetPlaceable, vector2);
				if (Physics.Raycast(targetPlaceable.position + bottomPointDelta, Vector3.down, out hitInfo, float.PositiveInfinity, num6))
				{
					SetHitLayer(hitInfo.collider.gameObject.layer);
				}
				else
				{
					SetHitLayer(-1);
				}
			}
			else
			{
				float y = targetPlaceable.position.y;
				if (Physics.Raycast(targetPlaceable.position + bottomPointDelta, Vector3.down, out hitInfo, float.PositiveInfinity, num6))
				{
					y = hitInfo.point.y;
					SetHitLayer(hitInfo.collider.gameObject.layer);
				}
				else
				{
					SetHitLayer(-1);
				}
				Vector3 vector3 = base.transform.position + GetPlacementRadius() * base.transform.forward;
				Vector3 positionToMove = new Vector3(vector3.x, y, vector3.z);
				TryMoveGhost(targetPlaceable, positionToMove);
			}
			return;
		}
		Transform targetPlaceable2 = GetTargetPlaceable();
		if (!(targetPlaceable2 == null))
		{
			Vector3 positionToMove2 = base.transform.position + GetPlacementRadius() * base.transform.forward;
			TryMoveGhost(targetPlaceable2, positionToMove2);
			if (IsStationary())
			{
				SetStationary(stationary: false);
			}
		}
	}

	private void TryMoveGhost(Transform ghost, Vector3 positionToMove)
	{
		float maxDistanceDelta = MOVEMENT_SPEED * Time.deltaTime;
		Vector3 position = Vector3.MoveTowards(ghost.position, positionToMove, maxDistanceDelta);
		if (SingletonBehaviour<InputManager>.Instance.IsActionPressed(SingletonBehaviour<InputManager>.Instance.ModifierActionRef))
		{
			BoxCollider boxCollider = GetMoveable().BoxCollider;
			Vector3 localScale = GetMoveable().transform.localScale;
			Collider[] array = Physics.OverlapBox(positionToMove + Vector3.up * boxCollider.center.y * localScale.y, new Vector3(boxCollider.size.x / 2f * localScale.x, boxCollider.size.y / 2f * localScale.y, boxCollider.size.z / 2f * localScale.z), layerMask: (1 << RayShooter.PLACEABLE_LAYER) | (1 << RayShooter.FURNITURE_LAYER) | (1 << RayShooter.PALLET_LAYER) | (1 << RayShooter.PALLET_SHELF_LAYER), orientation: boxCollider.transform.rotation);
			if (array.Length != 0 && GetMoveable().IsStationary)
			{
				BoxCollider moveableCollider = GetMoveableCollider(array);
				if (moveableCollider != null)
				{
					Vector3[] bottomCorners = GetBottomCorners(moveableCollider);
					Vector3 closest = GetClosest(bottomCorners, positionToMove);
					closest.y = positionToMove.y;
					Vector3[] bottomCorners2 = GetBottomCorners(boxCollider);
					Vector3 closest2 = GetClosest(bottomCorners2, closest);
					closest2.y = positionToMove.y;
					Vector3 target = ghost.position + closest - closest2;
					Vector3 position2 = Vector3.MoveTowards(ghost.position, target, maxDistanceDelta);
					ghost.position = position2;
					return;
				}
			}
			ghost.position = position;
		}
		else
		{
			ghost.position = position;
		}
	}

	private BoxCollider GetMoveableCollider(Collider[] overlapColliders)
	{
		int num = 0;
		BoxCollider result = null;
		for (int i = 0; i < overlapColliders.Length; i++)
		{
			Shelf component = overlapColliders[i].GetComponent<Shelf>();
			if (num < 3)
			{
				SnappableObject component2 = overlapColliders[i].GetComponent<SnappableObject>();
				if (component2 != null)
				{
					num = 3;
					result = ((!(component2.SnappableBoxCollider() != null)) ? component2.GetMoveable().BoxCollider : component2.SnappableBoxCollider());
					continue;
				}
			}
			if (num < 2)
			{
				MoveableHolderInterface component3 = overlapColliders[i].GetComponent<MoveableHolderInterface>();
				if (component3 != null)
				{
					num = 2;
					result = ((!(component3.SnappableBoxCollider() != null)) ? component3.GetMoveable().BoxCollider : component3.SnappableBoxCollider());
					continue;
				}
			}
			if (num < 1 && component != null)
			{
				num = 1;
				result = component.ParentPlaceable.Moveable.BoxCollider;
			}
		}
		return result;
	}

	private Vector3 GetClosest(Vector3[] edges, Vector3 target)
	{
		int num = -1;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < edges.Length; i++)
		{
			float num3 = Vector3.Distance(edges[i], target);
			if (num3 < num2)
			{
				num = i;
				num2 = num3;
			}
		}
		return edges[num];
	}

	private void PinToPoint()
	{
	}

	private void SetStationary(bool stationary)
	{
		GetMoveable().SetStationary(stationary);
	}

	private void SetHitLayer(int layer)
	{
		GetMoveable().SetHitLayer(layer);
	}

	private bool IsStationary()
	{
		return GetMoveable().IsStationary;
	}

	private Moveable GetMoveable()
	{
		if (placeableToMove != null)
		{
			return placeableToMove.Moveable;
		}
		return objectToMove;
	}

	public float GetPlacementRadius()
	{
		if (placeableToMove != null)
		{
			return placeableToMove.GetPlacementRadius();
		}
		if (objectToMove != null)
		{
			return objectToMove.MovedObject.GetPlacementRadius();
		}
		return PLACEMENT_RADIUS;
	}

	public Vector3[] GetBottomCorners(BoxCollider box)
	{
		Vector3[] array = new Vector3[4];
		Vector3 center = box.center;
		Vector3 vector = box.size * 0.5f;
		Vector3[] array2 = new Vector3[4]
		{
			new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z),
			new Vector3(vector.x, 0f - vector.y, 0f - vector.z),
			new Vector3(0f - vector.x, 0f - vector.y, vector.z),
			new Vector3(vector.x, 0f - vector.y, vector.z)
		};
		for (int i = 0; i < 4; i++)
		{
			Vector3 position = center + array2[i];
			array[i] = box.transform.TransformPoint(position);
		}
		return array;
	}

	private void OnDrawGizmos()
	{
		if (lastPosition != Vector3.zero && lastSize != Vector3.zero)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(lastPosition, lastSize);
		}
	}
}
