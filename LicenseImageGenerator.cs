using System.Collections.Generic;
using UnityEngine;

public class LicenseImageGenerator : MonoBehaviour
{
	[SerializeField]
	private List<Transform> transforms;

	private List<Product> usedProducts = new List<Product>();

	public void CreateLicenseUI(ProductData[] productData)
	{
		foreach (Product usedProduct in usedProducts)
		{
			SingletonBehaviour<ProductPool>.Instance.PutBackToPool(usedProduct);
		}
		int num = 0;
		foreach (ProductData productData2 in productData)
		{
			Product product = SingletonBehaviour<ProductPool>.Instance.GetProduct(productData2.type);
			product.transform.SetParent(transforms[num]);
			product.transform.localPosition = Vector3.zero;
			product.transform.localEulerAngles = Vector3.zero;
			usedProducts.Add(product);
			num++;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
