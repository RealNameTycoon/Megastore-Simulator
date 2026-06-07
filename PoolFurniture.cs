using DG.Tweening;
using UnityEngine;

public class PoolFurniture : Furniture
{
	[SerializeField]
	private Transform flamingoInflatable;

	[SerializeField]
	private Transform flamingoInflatable2;

	[SerializeField]
	private Transform ballInflatable;

	[SerializeField]
	private Transform circleInflatable;

	private void OnEnable()
	{
		PlayFloatingAnimation(flamingoInflatable, new Vector3(0f, 0f, 7f), 2.4f, 0f);
		PlayFloatingAnimation(flamingoInflatable2, new Vector3(0f, 0f, 7f), 2.4f, 0f);
		PlayFloatingAnimation(ballInflatable, new Vector3(4f, 0f, -6f), 1.9f, 0.3f);
		PlayFloatingAnimation(circleInflatable, new Vector3(-3f, 0f, 5f), 2.1f, 0.5f);
	}

	private void OnDisable()
	{
		flamingoInflatable.transform.DOKill();
		ballInflatable.transform.DOKill();
		circleInflatable.transform.DOKill();
		flamingoInflatable2.transform.DOKill();
	}

	public override void SetFloorLayers()
	{
		LayerMask placeableFloorLayers = (1 << PlacementManager.FLOOR_LAYER) | (1 << PlacementManager.SERVICE_ROOM_FLOOR_LAYER) | (1 << PlacementManager.STORAGE_FLOOR_LAYER) | (1 << PlacementManager.VEHICLE_FLOOR_LAYER) | (1 << PlacementManager.AROUND_STORE_LAYER);
		moveable.SetPlaceableFloorLayers(placeableFloorLayers);
	}

	private void PlayFloatingAnimation(Transform target, Vector3 rotationOffset, float duration, float delay)
	{
		if (!(target == null))
		{
			target.DOKill();
			Vector3 localEulerAngles = target.localEulerAngles;
			target.DOLocalRotate(localEulerAngles + rotationOffset, duration).SetEase(Ease.InOutSine).SetDelay(delay)
				.SetLoops(-1, LoopType.Yoyo);
		}
	}
}
