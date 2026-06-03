using UnityEngine;

public class ProductLicenseManager : MonoBehaviour
{
	private const string PRODUCT_LICENSE_KEY = "PRODUCT_LICENSE_KEY";

	private const string LICENSE_PURCHASED_KEY = "LICENSE_PURCHASED";

	private const string ANY_LICENSE_PURCHASED = "ANY_LICENSE_PURCHASED";

	public static bool LicensePurchasedOld(int licenseLevel)
	{
		return GenericDataSerializer.LoadBool("LICENSE_PURCHASED" + licenseLevel);
	}

	public static void RemoveLicense(int licenseLevel)
	{
		if (GenericDataSerializer.HasKey("LICENSE_PURCHASED" + licenseLevel))
		{
			GenericDataSerializer.DeleteKey("LICENSE_PURCHASED" + licenseLevel);
		}
	}

	public static bool LicensePurchased(int licenseLevel, ProductGroup productGroup)
	{
		return GenericDataSerializer.LoadBool("LICENSE_PURCHASED" + productGroup.ToString() + licenseLevel);
	}

	public static void PurchaseLicense(int licenseLevel, ProductGroup productGroup, bool defaultPurchase = false)
	{
		if (!defaultPurchase && !GenericDataSerializer.LoadBool("ANY_LICENSE_PURCHASED"))
		{
			GenericDataSerializer.SaveBool("ANY_LICENSE_PURCHASED", value: true);
		}
		GenericDataSerializer.SaveBool("LICENSE_PURCHASED" + productGroup.ToString() + licenseLevel, value: true);
	}

	public static bool AnyLicensePurchased()
	{
		return GenericDataSerializer.LoadBool("ANY_LICENSE_PURCHASED");
	}
}
