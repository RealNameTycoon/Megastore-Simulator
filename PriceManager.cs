public class PriceManager : SingletonBehaviour<PriceManager>
{
	private const string PRODUCT_PRICE_KEY = "ProductPrice";

	public float GetUnitPrice(ProductType productType)
	{
		if (productType == ProductType.NONE)
		{
			return -1f;
		}
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(productType);
		return GenericDataSerializer.LoadFloat("ProductPrice" + productType, productData.cost);
	}

	public void SetUnitPrice(ProductType productType, float newPrice)
	{
		GenericDataSerializer.SaveFloat("ProductPrice" + productType, newPrice);
	}
}
