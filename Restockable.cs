using UnityEngine;

public interface Restockable
{
	Transform PickupPoint();

	bool HasProductLabel();

	int GetProductCount();

	Transform GetTransform();

	int GetCapacityForProduct(ProductData productData);

	void PlaceProduct(Box box, bool instant = false);

	void PlaceProduct(Tray tray);

	void SetReservedToStaff(bool value);

	void ClearReservedToStaff();

	bool IsReservedToStaff();

	ProductGroup GetProductGroup();

	ProductType ContainedProductType();

	ProductType PreviousContainedProductType();

	string RestocableID();

	bool IsBakeryPlaceable();

	bool IsUnloaderPlaceable();

	bool IsAvailableForRestocking();

	Transform StaffInteractionPoint();

	bool IsCookedShelf();

	Transform ParentTransform();

	bool IsPlayerReserved();

	void OnPacked();
}
