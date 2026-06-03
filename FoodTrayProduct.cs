using System.Collections.Generic;
using UnityEngine;

public class FoodTrayProduct : Product
{
	[SerializeField]
	private MeshRenderer coverRenderer;

	private List<GameObject> choppedItems = new List<GameObject>();

	[SerializeField]
	private List<Product> products = new List<Product>();

	private ProductType containedType;

	public ProductType ContainedType => containedType;

	public bool HasProducts => products.Count > 0;

	public bool IsEmpty
	{
		get
		{
			if (products.Count == 0)
			{
				return choppedItems.Count == 0;
			}
			return false;
		}
	}

	public int ProductCount
	{
		get
		{
			if (IsEmpty)
			{
				return 0;
			}
			if (products.Count == 0)
			{
				return 1;
			}
			return products.Count;
		}
	}

	public void SetProduct(ProductType type, float tempPrice)
	{
		containedType = type;
		base.tempPrice = tempPrice;
	}

	public void AddChoppedItem(GameObject item)
	{
		choppedItems.Add(item);
	}

	public void AddProduct(Product product)
	{
		products.Add(product);
	}

	public void RemoveChoppedItem(GameObject item)
	{
		choppedItems.Remove(item);
	}

	public override void ResetProduct()
	{
		foreach (GameObject choppedItem in choppedItems)
		{
			Object.Destroy(choppedItem);
		}
		choppedItems.Clear();
		coverRenderer.gameObject.SetActive(value: false);
		foreach (Product product in products)
		{
			SingletonBehaviour<ProductPool>.Instance.PutBackToPool(product);
		}
		products.Clear();
	}

	public void EnableCover()
	{
		coverRenderer.gameObject.SetActive(value: true);
	}
}
