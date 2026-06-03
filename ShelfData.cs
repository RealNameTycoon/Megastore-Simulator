using UnityEngine;

[CreateAssetMenu(fileName = "ShelfData", menuName = "ScriptableObjects/ShelfData", order = 1)]
public class ShelfData : ScriptableObject
{
	public Sprite productSprite;

	public ProductType type;

	public BoxType boxType;

	public float cost;

	public float unitMarketPrice;

	public string productNameKey;

	public int shelfRowCount;

	public int shelfColumnCount;

	public int boxRowCount;

	public int boxColumnCount;

	public ShelfType shelfType;

	public string brand;

	public int requiredLicense;
}
