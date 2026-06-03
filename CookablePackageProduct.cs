using UnityEngine;

public class CookablePackageProduct : Product
{
	[SerializeField]
	private SpriteRenderer productIcon;

	private ProductType containedType;

	public ProductType ContainedType => containedType;

	public void SetProduct(ProductType type, float tempPrice)
	{
		containedType = type;
		base.tempPrice = tempPrice;
		SetSpriteKeepSize(productIcon, SingletonBehaviour<ProductPool>.Instance.GetProductData(containedType).productSprite);
	}

	private void SetSpriteKeepSize(SpriteRenderer sr, Sprite newSprite)
	{
		Sprite sprite = sr.sprite;
		Vector3 vector = (sprite ? sprite.bounds.size : newSprite.bounds.size);
		sr.sprite = newSprite;
		Vector3 size = newSprite.bounds.size;
		Transform obj = sr.transform;
		Vector3 localScale = obj.localScale;
		if (size.x != 0f)
		{
			localScale.x *= vector.x / size.x;
		}
		if (size.y != 0f)
		{
			localScale.y *= vector.y / size.y;
		}
		obj.localScale = localScale;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
