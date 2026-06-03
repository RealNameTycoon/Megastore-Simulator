using UnityEngine;

public interface MoveableObjectInterface
{
	void SwitchLook(bool toSolidObject);

	void SavePosition();

	void OnPlacementEnded();

	Transform GetTransform();

	bool CanPack();

	bool PlacedBefore();

	bool IsCancelable()
	{
		return true;
	}

	float GetPlacementRadius()
	{
		return PlacementManager.PLACEMENT_RADIUS;
	}
}
