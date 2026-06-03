using System;
using System.Collections.Generic;
using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Entities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class GenericBox : HotkeyClickable
{
	[SerializeField]
	private BoxCollider boxCollider;

	[SerializeField]
	private BoxContentWindow boxContentWindow;

	[SerializeField]
	private SerializedDictionary<ProductType, List<Product>> productMap = new SerializedDictionary<ProductType, List<Product>>();

	[SerializeField]
	private List<ProductType> containedProductTypes = new List<ProductType>();

	[SerializeField]
	private List<int> productCounts = new List<int>();

	private const string CONTAINED_PRODUCTS_KEY = "GENERIC_BOX_CONTAINED_PRODUCTS";

	private const string CONTAINED_PRODUCT_COUNTS_KEY = "GENERIC_BOX_CONTAINED_PRODUCT_COUNTS";

	[SerializeField]
	private int selectedProductIndex;

	private AlgorithmPackingResult latestResult;

	private ProductType itemType;

	private int totalProductCount;

	public static GenericBox Instance { get; protected set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		containedProductTypes = GenericDataSerializer.Load("GENERIC_BOX_CONTAINED_PRODUCTS", new List<ProductType>());
		productCounts = GenericDataSerializer.Load("GENERIC_BOX_CONTAINED_PRODUCT_COUNTS", new List<int>());
		for (int i = 0; i < containedProductTypes.Count; i++)
		{
			productMap[containedProductTypes[i]] = new List<Product>();
			for (int j = 0; j < productCounts[i]; j++)
			{
				Product product = SingletonBehaviour<ProductPool>.Instance.GetProduct(containedProductTypes[i]);
				product.transform.SetParent(base.transform);
				product.transform.localPosition = Vector3.zero;
				productMap[containedProductTypes[i]].Add(product);
				totalProductCount++;
			}
			EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, containedProductTypes[i], productCounts[i]);
		}
		RebuildLayout(instant: true);
		base.transform.position = putDownPosition.position;
		base.transform.eulerAngles = putDownPosition.eulerAngles;
		base.gameObject.SetActive(value: false);
	}

	public bool CanTakeProduct(Product productToTake)
	{
		float x = boxCollider.gameObject.transform.lossyScale.x;
		List<Container> list = new List<Container>();
		list.Add(new Container(0, boxCollider.size.x * x, boxCollider.size.z * x, boxCollider.size.y * x));
		List<Item> list2 = new List<Item>();
		int num = 0;
		bool flag = false;
		foreach (KeyValuePair<ProductType, List<Product>> item in productMap)
		{
			List<Product> value = item.Value;
			for (int i = 0; i < value.Count; i++)
			{
				Product product = value[i];
				list2.Add(new Item(num, product.BoxColliderSize.x, product.BoxColliderSize.y, product.BoxColliderSize.z, 1));
				num++;
				if (item.Key == productToTake.Data.type && i == value.Count - 1)
				{
					flag = true;
					list2.Add(new Item(num, productToTake.BoxColliderSize.x, productToTake.BoxColliderSize.y, productToTake.BoxColliderSize.z, 1));
					num++;
				}
			}
		}
		if (!flag)
		{
			list2.Add(new Item(num, productToTake.BoxColliderSize.x, productToTake.BoxColliderSize.y, productToTake.BoxColliderSize.z, 1));
			num++;
		}
		if (PackingService.FailsVolumeCheck(list[0], list2))
		{
			Debug.LogError("Fails volume check");
			return false;
		}
		List<int> list3 = new List<int>();
		list3.Add(1);
		AlgorithmPackingResult algorithmPackingResult = PackingService.Pack(list, list2, list3)[0].AlgorithmPackingResults[0];
		if (!algorithmPackingResult.IsCompletePack)
		{
			return false;
		}
		latestResult = algorithmPackingResult;
		itemType = productToTake.Data.type;
		return true;
	}

	private bool RebuildLayout(bool instant = false, Product addedProduct = null)
	{
		boxContentWindow.Repaint(containedProductTypes, productCounts, selectedProductIndex);
		if (totalProductCount == 0)
		{
			return false;
		}
		float x = boxCollider.gameObject.transform.lossyScale.x;
		List<Product> list = new List<Product>();
		int num = 0;
		AlgorithmPackingResult algorithmPackingResult;
		if (latestResult != null && addedProduct.Data.type == itemType)
		{
			algorithmPackingResult = latestResult;
			foreach (KeyValuePair<ProductType, List<Product>> item3 in productMap)
			{
				List<Product> value = item3.Value;
				for (int i = 0; i < value.Count; i++)
				{
					Product item = value[i];
					list.Add(item);
					num++;
				}
			}
		}
		else
		{
			List<Container> list2 = new List<Container>();
			list2.Add(new Container(0, boxCollider.size.x * x, boxCollider.size.z * x, boxCollider.size.y * x));
			List<Item> list3 = new List<Item>();
			foreach (KeyValuePair<ProductType, List<Product>> item4 in productMap)
			{
				List<Product> value2 = item4.Value;
				for (int j = 0; j < value2.Count; j++)
				{
					Product product = value2[j];
					list.Add(product);
					list3.Add(new Item(num, product.BoxColliderSize.x, product.BoxColliderSize.y, product.BoxColliderSize.z, 1));
					num++;
				}
			}
			List<int> list4 = new List<int>();
			list4.Add(1);
			algorithmPackingResult = PackingService.Pack(list2, list3, list4)[0].AlgorithmPackingResults[0];
		}
		latestResult = null;
		Vector3 packerOriginInBoxLocal = boxCollider.gameObject.transform.localPosition + boxCollider.center - boxCollider.size * 0.5f;
		Sequence s = null;
		if (!instant)
		{
			s = DOTween.Sequence();
		}
		foreach (KeyValuePair<ProductType, List<Product>> item5 in productMap)
		{
			_ = item5.Value;
			for (int k = 0; k < algorithmPackingResult.PackedItems.Count; k++)
			{
				Item item2 = algorithmPackingResult.PackedItems[k];
				int iD = item2.ID;
				Product product2 = list[iD];
				Quaternion packedRotation = GetPackedRotation(product2.BoxColliderSize, item2);
				Vector3 packedLocalPosition = GetPackedLocalPosition(product2, item2, packerOriginInBoxLocal, x, packedRotation);
				if (instant)
				{
					product2.transform.localPosition = packedLocalPosition;
					product2.transform.localRotation = packedRotation;
				}
				else if (Vector3.Distance(product2.transform.localPosition, packedLocalPosition) > Mathf.Epsilon)
				{
					product2.transform.DOKill();
					if (product2 == addedProduct)
					{
						s.Join(product2.transform.DoCurvedLocalMove(packedLocalPosition, 0.15f, 0.2f));
					}
					else
					{
						s.Join(product2.transform.DOLocalMove(packedLocalPosition, 0.15f));
					}
					s.Join(product2.transform.DOLocalRotate(packedRotation.eulerAngles, 0.15f));
				}
			}
		}
		return true;
	}

	private static Vector3 GetPackedLocalPosition(Product product, Item packedItem, Vector3 packerOriginInBoxLocal, float boxScale, Quaternion rotation)
	{
		Vector3 vector = new Vector3(packedItem.CoordX + packedItem.PackDimX * 0.5f, packedItem.CoordY + packedItem.PackDimY * 0.5f, packedItem.CoordZ + packedItem.PackDimZ * 0.5f);
		Vector3 vector2 = packerOriginInBoxLocal + vector / boxScale;
		Vector3 vector3 = rotation * Vector3.Scale(product.transform.localScale, product.BoxColliderCenter);
		return vector2 - vector3;
	}

	private static Quaternion GetPackedRotation(Vector3 originalSize, Item packedItem)
	{
		float a = packedItem.PackDimX;
		float a2 = packedItem.PackDimY;
		float a3 = packedItem.PackDimZ;
		float x = originalSize.x;
		float y = originalSize.y;
		float z = originalSize.z;
		if (Eq(a, x) && Eq(a2, y) && Eq(a3, z))
		{
			return Quaternion.identity;
		}
		if (Eq(a, y) && Eq(a2, x) && Eq(a3, z))
		{
			return Quaternion.Euler(0f, 0f, 90f);
		}
		if (Eq(a, x) && Eq(a2, z) && Eq(a3, y))
		{
			return Quaternion.Euler(90f, 0f, 0f);
		}
		if (Eq(a, z) && Eq(a2, y) && Eq(a3, x))
		{
			return Quaternion.Euler(0f, 90f, 0f);
		}
		if (Eq(a, z) && Eq(a2, x) && Eq(a3, y))
		{
			return Quaternion.Euler(0f, 0f, 90f) * Quaternion.Euler(90f, 0f, 0f);
		}
		if (Eq(a, y) && Eq(a2, z) && Eq(a3, x))
		{
			return Quaternion.Euler(0f, 0f, 90f) * Quaternion.Euler(0f, 90f, 0f);
		}
		return Quaternion.identity;
		static bool Eq(float num, float b)
		{
			return Mathf.Abs(num - b) < 0.001f;
		}
	}

	public override void PickUp()
	{
		base.PickUp();
		EventManager.NotifyEvent(GameEvents.GENERIC_BOX_PICKED_UP);
		boxContentWindow.Open();
		UpdateMenu();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	public override void PutDown()
	{
		base.PutDown();
		EventManager.NotifyEvent(GameEvents.GENERIC_BOX_PUTDOWN);
		boxContentWindow.Close();
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpen())
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
			SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		}
	}

	public void AddItem(Product product)
	{
		if (!(product == null) && !(product.Data == null))
		{
			EventManager.NotifyEvent(GameEvents.PRODUCT_ADDED_TO_BOX, product.Data.type, 1);
			product.transform.SetParent(base.transform);
			if (productMap.ContainsKey(product.Data.type))
			{
				productMap[product.Data.type].Add(product);
				productCounts[containedProductTypes.IndexOf(product.Data.type)]++;
			}
			else
			{
				productMap.Add(product.Data.type, new List<Product> { product });
				containedProductTypes.Add(product.Data.type);
				productCounts.Add(1);
			}
			totalProductCount++;
			SaveContent();
			RebuildLayout(instant: false, product);
		}
	}

	public bool HasSpace()
	{
		return true;
	}

	public bool IsEmpty()
	{
		return totalProductCount == 0;
	}

	public void UpdateMenuWithPlace(bool takeAvailable = false)
	{
		if (!base.IsPicked)
		{
			return;
		}
		if (takeAvailable)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("box_place", delegate
					{
						Place();
					})
				},
				{
					KeyCode.Mouse1,
					("box_take_product", delegate
					{
						TakeProduct();
					})
				},
				{
					KeyCode.Mouse2,
					("box_switch_product", delegate
					{
						RotateWheel();
					})
				}
			});
		}
		else
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("box_place", delegate
					{
						Place();
					})
				},
				{
					KeyCode.Mouse2,
					("box_switch_product", delegate
					{
						RotateWheel();
					})
				}
			});
		}
	}

	public void UpdateMenuWithTake()
	{
		if (base.IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse1,
				("box_take_product", delegate
				{
					TakeProduct();
				})
			} });
		}
	}

	public void UpdateMenu()
	{
		if (base.IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse2,
				("box_switch_product", delegate
				{
					RotateWheel();
				})
			} });
		}
	}

	private void TakeProduct()
	{
		EventManager.NotifyEvent(PlaceableEvents.TAKE_PRODUCT);
	}

	private void Place()
	{
		EventManager.NotifyEvent(PlaceableEvents.PLACE_PRODUCT);
	}

	private void SaveContent()
	{
		GenericDataSerializer.Save("GENERIC_BOX_CONTAINED_PRODUCTS", containedProductTypes);
		GenericDataSerializer.Save("GENERIC_BOX_CONTAINED_PRODUCT_COUNTS", productCounts);
	}

	public ProductData GetContainedProduct()
	{
		if (containedProductTypes.Count <= selectedProductIndex)
		{
			return null;
		}
		return SingletonBehaviour<ProductPool>.Instance.GetProductData(containedProductTypes[selectedProductIndex]);
	}

	public Product RemoveAndGetItem()
	{
		ProductType productType = containedProductTypes[selectedProductIndex];
		Product product = productMap[productType][productMap[productType].Count - 1];
		productMap[productType].Remove(product);
		productCounts[selectedProductIndex]--;
		EventManager.NotifyEvent(GameEvents.PRODUCT_REMOVED_FROM_BOX, product.Data.type, 1);
		if (productMap[productType].Count == 0)
		{
			productMap.Remove(productType);
			containedProductTypes.Remove(productType);
			productCounts.RemoveAt(selectedProductIndex);
			if (selectedProductIndex >= containedProductTypes.Count)
			{
				selectedProductIndex = Mathf.Max(0, containedProductTypes.Count - 1);
			}
		}
		totalProductCount--;
		RebuildLayout();
		SaveContent();
		return product;
	}

	protected override float GetPickUpSpeed()
	{
		return PICKUP_SPEED * 2f;
	}

	protected override float GetPickUpSpeedRotation()
	{
		return PICKUP_SPEED_ROTATION * 2f;
	}

	public void RotateWheel()
	{
		float scrollWheelInput = SingletonBehaviour<InputManager>.Instance.ScrollWheelInput;
		if (scrollWheelInput > 0f)
		{
			IncreaseSelectedProductIndex();
		}
		else if (scrollWheelInput < 0f)
		{
			DecreaseSelectedProductIndex();
		}
		boxContentWindow.Repaint(containedProductTypes, productCounts, selectedProductIndex);
	}

	private void IncreaseSelectedProductIndex()
	{
		selectedProductIndex++;
		if (selectedProductIndex >= containedProductTypes.Count)
		{
			selectedProductIndex = containedProductTypes.Count - 1;
		}
	}

	private void DecreaseSelectedProductIndex()
	{
		selectedProductIndex--;
		if (selectedProductIndex < 0)
		{
			selectedProductIndex = 0;
		}
	}
}
