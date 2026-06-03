using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThiefManager : SingletonBehaviour<ThiefManager>
{
	[SerializeField]
	private Camera thiefCamera;

	[SerializeField]
	private DOTweenPath pathAnimation;

	[SerializeField]
	private GameObject thief;

	[SerializeField]
	private GameObject bag1;

	[SerializeField]
	private GameObject bag2;

	[SerializeField]
	private Transform lookPosition1;

	[SerializeField]
	private Transform lookPosition2;

	[SerializeField]
	private RobbedWindow robbedWindow;

	[SerializeField]
	private TextMeshProUGUI thiefProtectionOwnedText;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private Button purchaseButton;

	private const int TURNING_WAYPOINT_INDEX = 5;

	private int currentIndex;

	private List<Placeable> placeables = new List<Placeable>();

	private List<Shelf> shelves = new List<Shelf>();

	private List<Product> products = new List<Product>();

	private List<ProductData> productDatas = new List<ProductData>();

	private List<int> productCounts = new List<int>();

	private bool thiefProtectionPurchased;

	private string THIEF_PROTECTION_PURCHASED_KEY = "THIEF_PROTECTION_PURCHASED_KEY";

	public bool ThiefProtectionPurchased => thiefProtectionPurchased;

	private void Start()
	{
		thiefProtectionPurchased = GenericDataSerializer.LoadBool(THIEF_PROTECTION_PURCHASED_KEY);
		if (thiefProtectionPurchased)
		{
			thiefProtectionOwnedText.enabled = true;
			purchaseButton.gameObject.SetActive(value: false);
			priceText.enabled = false;
		}
	}

	public void OnProtectionPurchased()
	{
		thiefProtectionPurchased = true;
		GenericDataSerializer.SaveBool(THIEF_PROTECTION_PURCHASED_KEY, value: true);
		thiefProtectionOwnedText.enabled = true;
		purchaseButton.gameObject.SetActive(value: false);
		priceText.enabled = false;
	}

	public void OnStepComplete()
	{
		currentIndex++;
		if (currentIndex == 5)
		{
			bag1.SetActive(value: true);
			bag2.SetActive(value: true);
		}
	}

	public void PathCompleted()
	{
		pathAnimation.DOPause();
		thief.gameObject.SetActive(value: false);
		for (int i = 0; i < GetProductCount(); i++)
		{
			Placeable randomAvailablePlaceable = SingletonBehaviour<SpawnManager>.Instance.GetRandomAvailablePlaceable();
			if (randomAvailablePlaceable == null)
			{
				break;
			}
			Shelf randomAvailableShelf = randomAvailablePlaceable.GetRandomAvailableShelf();
			Product randomProduct = randomAvailableShelf.GetRandomProduct();
			if (productDatas.Contains(randomProduct.Data))
			{
				productCounts[productDatas.IndexOf(randomProduct.Data)]++;
			}
			else
			{
				if (productDatas.Count >= 3)
				{
					continue;
				}
				productDatas.Add(randomProduct.Data);
				productCounts.Add(1);
			}
			placeables.Add(randomAvailablePlaceable);
			shelves.Add(randomAvailableShelf);
			products.Add(randomProduct);
			randomProduct.isReserved = true;
		}
		robbedWindow.Open(productDatas, productCounts);
	}

	public void RemoveAllProducts()
	{
		if (shelves.Count != 0)
		{
			for (int i = 0; i < shelves.Count; i++)
			{
				shelves[i].RemoveProduct(products[i]);
			}
			placeables.Clear();
			shelves.Clear();
			for (int j = 0; j < products.Count; j++)
			{
				SingletonBehaviour<ProductPool>.Instance.PutBackToPool(products[j]);
			}
			products.Clear();
			productDatas.Clear();
			productCounts.Clear();
		}
	}

	public void ReleaseAllProducts()
	{
		if (shelves.Count != 0)
		{
			placeables.Clear();
			shelves.Clear();
			for (int i = 0; i < products.Count; i++)
			{
				products[i].isReserved = false;
			}
			products.Clear();
			productDatas.Clear();
			productCounts.Clear();
		}
	}

	private int GetProductCount()
	{
		return Random.Range(4, 8);
	}

	public void StartRobery()
	{
		pathAnimation.DORestart();
		currentIndex = 0;
		bag1.SetActive(value: false);
		bag2.SetActive(value: false);
		thief.gameObject.SetActive(value: true);
		StartCoroutine(WaitAndActivateBags());
	}

	private IEnumerator WaitAndActivateBags()
	{
		float fullDistance = 0f;
		for (int i = 0; i < pathAnimation.wps.Count; i++)
		{
			fullDistance += pathAnimation.path.wpLengths[i];
		}
		for (int j = 0; j < pathAnimation.wps.Count; j++)
		{
			yield return new WaitForSeconds(pathAnimation.duration / fullDistance * pathAnimation.path.wpLengths[j]);
			if (j == 2)
			{
				thiefCamera.DOFieldOfView(40f, 2f);
			}
			if (j == 4)
			{
				pathAnimation.DOPause();
				yield return new WaitForSeconds(1f);
				pathAnimation.DOPlay();
				bag1.SetActive(value: true);
				bag2.SetActive(value: true);
			}
			if (j == 5)
			{
				thiefCamera.DOFieldOfView(60f, 2f);
			}
		}
	}

	private void Update()
	{
	}
}
