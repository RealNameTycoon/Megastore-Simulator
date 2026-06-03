using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StorageShelf : Shelf
{
	[SerializeField]
	private SpriteRenderer productImage;

	private const string SHELF_BOX_LIST_KEY = "BoxList";

	private List<int> containedBoxIDs = new List<int>();

	protected override void Start()
	{
		base.Start();
		EventManager.AddListener(GameEvents.BOX_PICKED_UP, OnBoxPickedUp);
		EventManager.AddListener<Placeable>(PlaceableEvents.PLACEMENT_ENDED, OnPlacementEnded);
	}

	public override int GetProductCount()
	{
		int num = 0;
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] != -1)
			{
				num++;
			}
		}
		return num;
	}

	private void RemoveBox(Box box)
	{
		containedBoxIDs[containedBoxIDs.IndexOf(box.BoxID)] = -1;
		if (GetProductCount() == 0)
		{
			containedType = ProductType.NONE;
			base.PriceTag.gameObject.SetActive(value: false);
		}
		SaveShelfContent();
	}

	protected override void OnPlaceProduct()
	{
		if (!isHovered)
		{
			return;
		}
		Box box = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		ProductData containedProduct = box.GetContainedProduct();
		if (containedProduct == null || containedProduct.boxType == BoxType.XL_BOX)
		{
			return;
		}
		BoxType boxType = containedProduct.boxType;
		int num = ((boxType == BoxType.WIDE) ? 2 : 3);
		if ((containedType == ProductType.NONE || containedType == containedProduct.type) && GetProductCount() < num)
		{
			int num2 = 1;
			int num3 = num;
			if (containedType == ProductType.NONE)
			{
				containedBoxIDs = new List<int>();
				for (int i = 0; i < num2 * num3; i++)
				{
					containedBoxIDs.Add(-1);
				}
			}
			List<Vector3> list = MathUtils.CalculatePositions(base.Corner1, base.Corner2, num2, num3, isVertical: false);
			int firstEmptyIndex = GetFirstEmptyIndex();
			SingletonBehaviour<BoxManager>.Instance.OnBoxPut();
			if (box.IsOpen())
			{
				box.Close();
			}
			box.transform.SetParent(base.transform);
			box.transform.DOLocalRotate(base.transform.forward, 0.5f);
			box.transform.DoCurvedLocalMove(list[firstEmptyIndex] + BoxManager.BoxTypeToYOffset[boxType], 0.5f, 2f).OnComplete(delegate
			{
				box.OnBoxPut();
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
			});
			HapticController.Vibrate(PresetType.LightImpact);
			containedBoxIDs[firstEmptyIndex] = box.BoxID;
			if (containedType == ProductType.NONE)
			{
				containedType = containedProduct.type;
			}
			SaveShelfContent();
			CloseInteractionElements();
			RepaintLabel();
		}
		else if (containedType != containedProduct.type && GetProductCount() < num)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_different_product", base.transform);
		}
		else
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowError("error_shelf_full", base.transform);
		}
	}

	private void RepaintLabel()
	{
		if (containedType == ProductType.NONE)
		{
			return;
		}
		base.PriceTag.gameObject.SetActive(value: true);
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType);
		productImage.sprite = productData.productSprite;
		int num = 0;
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] != -1)
			{
				num += SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]).ProductCount;
			}
		}
		base.PriceText.text = num.ToString();
	}

	public override int GetFirstEmptyIndex()
	{
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] == -1)
			{
				return i;
			}
		}
		return 0;
	}

	public override void Initialize()
	{
		containedBoxIDs = GenericDataSerializer.Load(base.ParentPlaceable.PlaceableID + "|" + base.ShelfID + "BoxList", new List<int>());
		containedType = GenericDataSerializer.Load(base.ParentPlaceable.Type.ToString() + base.ParentPlaceable.PlaceableID + "|" + base.ShelfID + "ShelfProduct", ProductType.NONE);
		RepaintLabel();
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] != -1)
			{
				SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]).transform.SetParent(base.transform);
			}
		}
	}

	private void OnBoxPickedUp()
	{
		Box pickedBox = SingletonBehaviour<BoxManager>.Instance.GetPickedBox();
		if (containedBoxIDs.Contains(pickedBox.BoxID))
		{
			RemoveBox(pickedBox);
			RepaintLabel();
		}
	}

	private void OnPlacementEnded(Placeable placeable)
	{
		if (!placeable.Equals(base.ParentPlaceable))
		{
			return;
		}
		for (int i = 0; i < containedBoxIDs.Count; i++)
		{
			if (containedBoxIDs[i] != -1)
			{
				SingletonBehaviour<BoxManager>.Instance.GetBox(containedBoxIDs[i]).SaveLocation();
			}
		}
	}

	private void SaveShelfContent()
	{
		GenericDataSerializer.Save(base.ParentPlaceable.PlaceableID + "|" + base.ShelfID + "BoxList", containedBoxIDs);
		GenericDataSerializer.Save(base.ParentPlaceable.Type.ToString() + base.ParentPlaceable.PlaceableID + "|" + base.ShelfID + "ShelfProduct", containedType);
	}

	public override void OnMouseHoverStarted()
	{
		if (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<BoxManager>.Instance.IsBoxOnAir && !SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsEmpty() && !SingletonBehaviour<BoxManager>.Instance.GetPickedBox().IsPlaceableBox())
		{
			base.ParentPlaceable.Outline.enabled = true;
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("tooltip_shelf_place", base.transform);
			SingletonBehaviour<BoxManager>.Instance.UpdateMenuForStorageShelf();
			isHovered = true;
		}
		else if (!SingletonBehaviour<BoxManager>.Instance.IsBoxPicked)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTooltip("tooltip_shelf_move", base.transform);
		}
	}
}
