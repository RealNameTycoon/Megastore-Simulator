using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class BoxManager : SingletonBehaviour<BoxManager>
{
	[Serializable]
	public class BoxShelfBoxPositionDictionary : UnitySerializedDictionary<BoxType, PositionContainer>
	{
	}

	[SerializeField]
	private BoxShelfBoxPositionDictionary boxShelfBoxPositionDictionary;

	[SerializeField]
	private Transform boxTargetTransform;

	[SerializeField]
	private Transform boxTargetTablet;

	[SerializeField]
	private Transform boxTargetFruitTransform;

	[SerializeField]
	private Transform boxTargetFruitTablet;

	[SerializeField]
	private Transform boxParent;

	[SerializeField]
	private Transform boxSpawnTransform;

	[SerializeField]
	private Transform instantSpawnTransform;

	[SerializeField]
	private Transform tutorialSpawnTransform;

	[SerializeField]
	private PlayerMove playerMove;

	[SerializeField]
	private PlayerLook playerLook;

	[SerializeField]
	private SerializedDictionary<BoxType, Material> boxOriginalMaterials;

	[SerializeField]
	private SerializedDictionary<BoxType, Material> boxPerishableMaterials;

	public static float BOX_PICKUP_DURATION = 0.5f;

	private Box pickedBox;

	private List<int> spawnedBoxIDs;

	private List<BoxType> spawnedBoxTypes;

	private Dictionary<int, Box> spawnedBoxes = new Dictionary<int, Box>();

	private Dictionary<int, Box> activeBoxes = new Dictionary<int, Box>();

	private int spawnedBoxCount;

	private const string SPAWNED_BOX_COUNT_KEY = "SpawnedBoxCount";

	private const string SPAWNED_BOX_IDS_KEY = "SPAWNED_BOX_IDS";

	private const string SPAWNED_BOX_TYPES_KEY = "SPAWNED_BOX_TYPES";

	public const int TUTORIAL_ID = 3;

	private float BOX_WIDTH_OFFSET = 0.5f;

	private Vector3 boxTargetLocalPosition;

	private Vector3 fruitBoxTargetLocalPosition;

	public static int BOX_LAYER = 15;

	public static int FLYING_BOX_LAYER = 16;

	public static int NOT_COLLIDING_BOX_LAYER = 24;

	public static int NOT_RAYCASTING_BOX_LAYER = 25;

	public Vector3 tutorialBoxPosition = Vector3.zero;

	private float vegetableShelfRefundAmount;

	public static Dictionary<BoxType, Vector3> BoxTypeToYOffset = new Dictionary<BoxType, Vector3>
	{
		{
			BoxType.SMALL_BOX,
			-0.159f * Vector3.up
		},
		{
			BoxType.MEDIUM_BOX,
			Vector3.zero
		},
		{
			BoxType.LARGE_BOX,
			Vector3.zero
		},
		{
			BoxType.FRUIT_BOX,
			-0.035f * Vector3.up
		},
		{
			BoxType.XL_BOX,
			-0.03f * Vector3.forward
		},
		{
			BoxType.WIDE,
			Vector3.zero
		},
		{
			BoxType.SQUARE,
			0.085f * Vector3.up
		},
		{
			BoxType.HIGH_SQUARE,
			0.2793f * Vector3.up
		},
		{
			BoxType.WARDROBE_BOX,
			1f * Vector3.up
		}
	};

	public static readonly Dictionary<StorageSensitivity, int> storageSensitivityToHighTemperatureSpoilageMinutes = new Dictionary<StorageSensitivity, int>
	{
		{
			StorageSensitivity.None,
			0
		},
		{
			StorageSensitivity.Low,
			11520
		},
		{
			StorageSensitivity.Medium,
			5760
		},
		{
			StorageSensitivity.High,
			2880
		},
		{
			StorageSensitivity.Critical,
			1440
		}
	};

	public static readonly Dictionary<StorageSensitivity, int> storageSensitivityToLowTemperatureSpoilageMinutes = new Dictionary<StorageSensitivity, int>
	{
		{
			StorageSensitivity.None,
			0
		},
		{
			StorageSensitivity.Low,
			4320
		},
		{
			StorageSensitivity.Medium,
			2880
		},
		{
			StorageSensitivity.High,
			1440
		},
		{
			StorageSensitivity.Critical,
			720
		}
	};

	public static readonly Dictionary<StorageRequirement, int> storageRequirementToMinDegree = new Dictionary<StorageRequirement, int>
	{
		{
			StorageRequirement.Cool,
			6
		},
		{
			StorageRequirement.Fridge,
			0
		},
		{
			StorageRequirement.Freezer,
			-18
		}
	};

	public static readonly Dictionary<StorageRequirement, int> storageRequirementToMaxDegree = new Dictionary<StorageRequirement, int>
	{
		{
			StorageRequirement.Cool,
			12
		},
		{
			StorageRequirement.Fridge,
			5
		},
		{
			StorageRequirement.Freezer,
			-1
		}
	};

	private Dictionary<ProductType, int> productTypeToStockCount = new Dictionary<ProductType, int>();

	private int lastSpawn;

	public Transform TutorialBoxTransform => tutorialSpawnTransform;

	public float VegetableShelfRefundAmount => vegetableShelfRefundAmount;

	private Transform BoxParent => boxParent;

	public bool IsBoxPicked => pickedBox != null;

	public bool IsBoxOnAir
	{
		get
		{
			if (pickedBox != null && (pickedBox.gameObject.layer != BOX_LAYER || DOTween.IsTweening(pickedBox)))
			{
				return true;
			}
			return false;
		}
	}

	public Vector3 GetInstantSpawnPosition(Box box)
	{
		if (tutorialBoxPosition == Vector3.zero)
		{
			tutorialBoxPosition = instantSpawnTransform.position;
		}
		tutorialBoxPosition += box.GetHeight() * Vector3.up;
		return tutorialBoxPosition;
	}

	public void DebugSpawnBox(FurnitureType productType, int offset = 0)
	{
		SpawnBox(productType, tutorialSpawnTransform);
	}

	public void DebugSpawnProductBox(ProductType productType, int offset = 0)
	{
		SpawnBox(productType, tutorialSpawnTransform);
	}

	public void DebugSpawnPlaceableBox(PlaceableType placeableType, int offset = 0)
	{
		SpawnBox(placeableType, tutorialSpawnTransform);
	}

	public bool ContainsPerishableMaterial(BoxType type)
	{
		return boxPerishableMaterials.ContainsKey(type);
	}

	public Material GetBoxOriginalMaterial(BoxType type)
	{
		return boxOriginalMaterials[type];
	}

	public Material GetBoxPerishableMaterial(BoxType type)
	{
		return boxPerishableMaterials[type];
	}

	public PositionContainer GetBoxPositions(BoxType type)
	{
		return boxShelfBoxPositionDictionary[type];
	}

	public int GetBoxCapacity(BoxType type)
	{
		return boxShelfBoxPositionDictionary[type].localPositions.Count;
	}

	public bool NoContainerPicked()
	{
		if (!IsBoxPicked && !SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			return !GenericBox.Instance.IsPicked;
		}
		return false;
	}

	public static bool IsBoxLayer(int layer)
	{
		if (layer != BOX_LAYER && layer != NOT_COLLIDING_BOX_LAYER)
		{
			return layer == FLYING_BOX_LAYER;
		}
		return true;
	}

	private new void Awake()
	{
		base.Awake();
		spawnedBoxCount = GenericDataSerializer.LoadInt("SpawnedBoxCount");
		spawnedBoxIDs = GenericDataSerializer.Load("SPAWNED_BOX_IDS", new List<int>());
		spawnedBoxTypes = GenericDataSerializer.Load("SPAWNED_BOX_TYPES", new List<BoxType>());
		EventManager.AddListener(StartupEvents.RESTOCK_ZONES_INITIALIZED, Initialize);
		EventManager.AddListener(StartupEvents.TEMPERATURE_ZONES_INITIALIZED, RegisterTemperatureZones);
	}

	private void Start()
	{
		boxTargetLocalPosition = boxTargetTransform.localPosition;
		float y = Mathf.Lerp(boxTargetLocalPosition.y, boxTargetTablet.localPosition.y, (playerLook.GetFov() - 60f) / 25f);
		boxTargetLocalPosition.y = y;
		fruitBoxTargetLocalPosition = boxTargetFruitTransform.localPosition;
		float y2 = Mathf.Lerp(boxTargetFruitTransform.localPosition.y, boxTargetFruitTablet.localPosition.y, (playerLook.GetFov() - 60f) / 25f);
		fruitBoxTargetLocalPosition.y = y2;
		lastSpawn = GenericDataSerializer.LoadInt("lastspawn");
	}

	public void SpawnTutorialBox()
	{
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(ProductType.APPLE);
		if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.GROCERY)
		{
			productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(ProductType.APPLE);
		}
		else if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.BAKERY)
		{
			productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(ProductType.BAGETTE_V1);
		}
		else if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.TOY)
		{
			productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(ProductType.AVACADO_PLUSHIE);
		}
		else if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.CLOTHING)
		{
			productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(ProductType.T_SHIRT_FEMALE_1);
		}
		Box box = SingletonBehaviour<BoxPool>.Instance.GetBox(productData.boxType);
		box.Initialize(spawnedBoxCount);
		box.FillItems(productData, productData.GetMaxProductCount());
		box.transform.position = tutorialSpawnTransform.position;
		box.transform.rotation = tutorialSpawnTransform.rotation;
		box.SaveLocation();
		spawnedBoxIDs.Add(spawnedBoxCount);
		spawnedBoxTypes.Add(productData.boxType);
		spawnedBoxes.Add(spawnedBoxCount, box);
		spawnedBoxCount++;
		SaveBoxData();
	}

	public Box GetTutorialBox()
	{
		if (spawnedBoxes.Count == 0 || !spawnedBoxes.ContainsKey(0))
		{
			return null;
		}
		return spawnedBoxes[0];
	}

	public Box GetBox(int boxID)
	{
		if (!spawnedBoxes.ContainsKey(boxID))
		{
			return null;
		}
		return spawnedBoxes[boxID];
	}

	private void Update()
	{
	}

	private void IncrementLastSpawn()
	{
		lastSpawn++;
		ProductType productType = (ProductType)lastSpawn;
		MonoBehaviour.print("new spawnType: " + productType);
		GenericDataSerializer.SaveInt("lastspawn", lastSpawn);
	}

	private void DecrementLastSpawn()
	{
		lastSpawn--;
		ProductType productType = (ProductType)lastSpawn;
		MonoBehaviour.print("new spawnType: " + productType);
		GenericDataSerializer.SaveInt("lastspawn", lastSpawn);
	}

	private void Initialize()
	{
		bool flag = GenericDataSerializer.LoadBool("VEGETABLE_SHELF_REFUNDED");
		for (int i = 0; i < spawnedBoxIDs.Count; i++)
		{
			if (spawnedBoxTypes[i] != BoxType.WARDROBE_BOX)
			{
				PlaceableType placeableType = GenericDataSerializer.Load("boxPlaceableType" + spawnedBoxIDs[i], PlaceableType.NONE);
				if (!flag && placeableType == PlaceableType.VEGETABLE_SHELF)
				{
					vegetableShelfRefundAmount += 250f;
					GenericDataSerializer.Save("boxPlaceableType" + spawnedBoxIDs[i], PlaceableType.NONE);
				}
				Box box = SingletonBehaviour<BoxPool>.Instance.GetBox(spawnedBoxTypes[i]);
				box.InitializeOldBox(spawnedBoxIDs[i]);
				spawnedBoxes.Add(spawnedBoxIDs[i], box);
			}
		}
		EventManager.NotifyEvent(StartupEvents.BOXES_INITIALIZED);
	}

	private void RegisterTemperatureZones()
	{
		foreach (KeyValuePair<int, Box> activeBox in activeBoxes)
		{
			activeBox.Value.CheckAndUpdateTemperatureZone();
		}
	}

	public void PickUpBox(Box box)
	{
		playerMove.EnableBoxCollider(box.Type);
		pickedBox = box;
		SingletonBehaviour<HotKeyManager>.Instance.RefreshEnablity();
		pickedBox.transform.SetParent(BoxParent);
		pickedBox.RigidBody.constraints = RigidbodyConstraints.FreezeAll;
		pickedBox.gameObject.layer = NOT_COLLIDING_BOX_LAYER;
		WakeUpBoxesAbove(pickedBox);
		Vector3 endValue = (box.HasNoCover ? fruitBoxTargetLocalPosition : (boxTargetLocalPosition + Vector3.down * box.GetHeight() / 1.25f * box.transform.localScale.y + Vector3.up * 0.35f + Vector3.forward * box.GetWidth() / 1.25f / 7f));
		MonoBehaviour.print("moving box to target: " + endValue.ToString());
		pickedBox.transform.DOKill();
		pickedBox.transform.DOLocalMove(endValue, BOX_PICKUP_DURATION);
		Vector3 endValue2 = (box.HasNoCover ? boxTargetFruitTransform.localEulerAngles : boxTargetTransform.localEulerAngles);
		EventManager.NotifyEvent(GameEvents.BOX_PICK_STARTED);
		Box pickedUpBox = pickedBox;
		UnregisterBox(pickedBox);
		pickedBox.transform.DOLocalRotate(endValue2, BOX_PICKUP_DURATION).OnComplete(delegate
		{
			pickedUpBox.gameObject.layer = BOX_LAYER;
			UpdateMenu();
			EventManager.NotifyEvent(GameEvents.BOX_PICKED_UP);
			if (SingletonBehaviour<TutorialManager>.Instance.TutorialDone() && pickedUpBox.GetContainedProduct() != null && pickedUpBox.GetContainedProduct().storageRequirement != StorageRequirement.None)
			{
				SingletonWindow<TutorialVideoWindowUI>.Instance.Open(TutorialVideoWindowType.WALKIN_FREEZER_TUTORIAL);
			}
		});
		if (SingletonBehaviour<TutorialManager>.Instance.IsTutorialActive(3))
		{
			EventManager.NotifyEvent(TutorialEvents.TUTORIAL_STEP_DONE, 3);
		}
	}

	public void WakeUpBoxesAbove(Box box)
	{
		float y = box.Collider.bounds.size.y;
		Vector3 center = box.transform.position + Vector3.up * y * 0.6f;
		Vector3 halfExtents = new Vector3(0.5f, y * 0.6f, 0.5f);
		Collider[] array = Physics.OverlapBox(center, halfExtents, Quaternion.identity, 1 << BOX_LAYER);
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody attachedRigidbody = array[i].attachedRigidbody;
			if (attachedRigidbody != null)
			{
				attachedRigidbody.WakeUp();
				attachedRigidbody.linearVelocity += Vector3.down * 0.05f;
			}
		}
	}

	public void SpawnBox(ProductType productType, Transform spawnTransform, int offset = 0)
	{
		SpawnBox(productType, spawnTransform.position, spawnTransform.eulerAngles, offset);
	}

	public Box SpawnBox(ProductType productType, Vector3 position, Vector3 eulerRotation, int offset = 0)
	{
		Debug.Log("spawning box for product type: " + productType);
		ProductData productData = SingletonBehaviour<ProductPool>.Instance.GetProductData(productType);
		Box box = SingletonBehaviour<BoxPool>.Instance.GetBox(productData.boxType);
		box.Initialize(spawnedBoxCount, productData.storageRequirement != StorageRequirement.None);
		Debug.Log("max product count: " + productData.GetMaxProductCount());
		box.FillItems(productData, productData.GetMaxProductCount());
		box.transform.position = position;
		box.transform.eulerAngles = eulerRotation;
		box.SaveLocation();
		spawnedBoxIDs.Add(spawnedBoxCount);
		spawnedBoxTypes.Add(productData.boxType);
		spawnedBoxes.Add(spawnedBoxCount, box);
		spawnedBoxCount++;
		SaveBoxData();
		return box;
	}

	public void SpawnFreeWalkInFreezer()
	{
		SpawnBox(FurnitureType.WALK_IN_FREEZER_SMALL, tutorialSpawnTransform);
	}

	public Box SpawnBox(PlaceableType type, Transform spawnTransform, int offset = 0)
	{
		return SpawnBox(type, spawnTransform.position, spawnTransform.eulerAngles, offset);
	}

	public Box SpawnBox(PlaceableType type, Vector3 position, Vector3 eulerRotation, int offset = 0)
	{
		Box box = SingletonBehaviour<BoxPool>.Instance.GetBox(BoxType.XL_BOX);
		box.Initialize(spawnedBoxCount);
		box.FillWithPlaceable(type);
		box.transform.position = position;
		box.transform.eulerAngles = eulerRotation;
		spawnedBoxIDs.Add(spawnedBoxCount);
		spawnedBoxTypes.Add(BoxType.XL_BOX);
		spawnedBoxes.Add(spawnedBoxCount, box);
		spawnedBoxCount++;
		SaveBoxData();
		return box;
	}

	public Box SpawnBox(FurnitureType type, Transform spawnTransform, int offset = 0)
	{
		return SpawnBox(type, spawnTransform.position, spawnTransform.eulerAngles, offset);
	}

	public Box SpawnBox(FurnitureType type, Vector3 position, Vector3 eulerRotation, int offset = 0)
	{
		Box box = SingletonBehaviour<BoxPool>.Instance.GetBox(BoxType.XL_BOX);
		box.Initialize(spawnedBoxCount);
		box.FillWithFurniture(type);
		box.transform.position = position;
		box.transform.eulerAngles = eulerRotation;
		box.SaveLocation();
		spawnedBoxIDs.Add(spawnedBoxCount);
		spawnedBoxTypes.Add(BoxType.XL_BOX);
		spawnedBoxes.Add(spawnedBoxCount, box);
		spawnedBoxCount++;
		SaveBoxData();
		return box;
	}

	public void ClearAllEmptyBoxes()
	{
		for (int i = 0; i < spawnedBoxIDs.Count; i++)
		{
			if (spawnedBoxes[spawnedBoxIDs[i]].IsEmpty())
			{
				DeleteBox(spawnedBoxIDs[i]);
				i--;
			}
		}
	}

	public void DeleteBox(int boxID)
	{
		int num = spawnedBoxIDs.IndexOf(boxID);
		if (num != -1)
		{
			EventManager.NotifyEvent(GameEvents.BOX_DELETED, boxID);
			Box box = spawnedBoxes[boxID];
			UnregisterBox(box);
			if (pickedBox != null && box.BoxID == pickedBox.BoxID)
			{
				playerMove.UpdateRadius(objectPicked: false);
				pickedBox.RigidBody.constraints = RigidbodyConstraints.None;
				SingletonBehaviour<ButtonsWindow>.Instance.Close();
				pickedBox = null;
				EventManager.NotifyEvent(GameEvents.PICKED_BOX_TRIGGERED_TRASH);
			}
			spawnedBoxIDs.RemoveAt(num);
			spawnedBoxTypes.RemoveAt(num);
			spawnedBoxes.Remove(boxID);
			box.DeleteAllData();
			SaveBoxData();
			SingletonBehaviour<BoxPool>.Instance.PutBackToPool(box);
		}
	}

	public void DeleteBoxData(int boxID)
	{
		int num = spawnedBoxIDs.IndexOf(boxID);
		if (num != -1)
		{
			Box box = spawnedBoxes[boxID];
			spawnedBoxIDs.RemoveAt(num);
			spawnedBoxTypes.RemoveAt(num);
			spawnedBoxes.Remove(boxID);
			box.DeleteAllData();
			SaveBoxData();
		}
	}

	public void DeleteBoxNPC(Box box)
	{
		SingletonBehaviour<BoxPool>.Instance.PutBackToPool(box);
	}

	private void SaveBoxData()
	{
		GenericDataSerializer.SaveInt("SpawnedBoxCount", spawnedBoxCount);
		GenericDataSerializer.Save("SPAWNED_BOX_IDS", spawnedBoxIDs);
		GenericDataSerializer.Save("SPAWNED_BOX_TYPES", spawnedBoxTypes);
	}

	public void UpdateMenuForTrash()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			KeyCode.Mouse0,
			("box_throw", delegate
			{
				ThrowBox();
			})
		} });
	}

	public void UpdateMenuForBoxContainer()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			KeyCode.Mouse0,
			("box_throw", delegate
			{
				ThrowBox();
			})
		} });
	}

	public void UpdateMenu()
	{
		if (!(pickedBox != null) || IsBoxOnAir)
		{
			return;
		}
		if (pickedBox.HasNoCover)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.R,
					("box_throw", delegate
					{
						ThrowBox();
					})
				},
				{
					KeyCode.G,
					("box_put", delegate
					{
						PutBox();
					})
				}
			}, base.transform);
		}
		else if (pickedBox.IsOpen())
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.C,
					("box_close", delegate
					{
						CloseBox();
					})
				},
				{
					KeyCode.R,
					("box_throw", delegate
					{
						ThrowBox();
					})
				},
				{
					KeyCode.G,
					("box_put", delegate
					{
						PutBox();
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
					("box_open", delegate
					{
						OpenBox();
					})
				},
				{
					KeyCode.R,
					("box_throw", delegate
					{
						ThrowBox();
					})
				},
				{
					KeyCode.G,
					("box_put", delegate
					{
						PutBox();
					})
				}
			});
		}
	}

	public void UpdateMenuWithStack()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
		{
			{
				KeyCode.Mouse0,
				("box_store", delegate
				{
					StoreBox();
				})
			},
			{
				KeyCode.R,
				("box_throw", delegate
				{
					ThrowBox();
				})
			},
			{
				KeyCode.G,
				("box_put", delegate
				{
					PutBox();
				})
			}
		});
	}

	public void UpdateMenuWithTake()
	{
		if (!(pickedBox != null) || IsBoxOnAir)
		{
			return;
		}
		if (pickedBox.HasNoCover)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse1,
					("box_take_product", delegate
					{
						TakeProduct();
					})
				},
				{
					KeyCode.R,
					("box_throw", delegate
					{
						ThrowBox();
					})
				},
				{
					KeyCode.G,
					("box_put", delegate
					{
						PutBox();
					})
				}
			}, base.transform);
		}
		else if (pickedBox.IsOpen())
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse1,
					("box_take_product", delegate
					{
						TakeProduct();
					})
				},
				{
					KeyCode.C,
					("box_close", delegate
					{
						CloseBox();
					})
				},
				{
					KeyCode.R,
					("box_throw", delegate
					{
						ThrowBox();
					})
				},
				{
					KeyCode.G,
					("box_put", delegate
					{
						PutBox();
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
					("box_open", delegate
					{
						OpenBox();
					})
				},
				{
					KeyCode.R,
					("box_throw", delegate
					{
						ThrowBox();
					})
				},
				{
					KeyCode.G,
					("box_put", delegate
					{
						PutBox();
					})
				}
			});
		}
	}

	public void UpdateMenuWithPlace(bool takeAvailable = false)
	{
		if (!(pickedBox != null) || IsBoxOnAir)
		{
			return;
		}
		if (pickedBox.HasNoCover)
		{
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
						KeyCode.R,
						("box_throw", delegate
						{
							ThrowBox();
						})
					},
					{
						KeyCode.G,
						("box_put", delegate
						{
							PutBox();
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
						KeyCode.R,
						("box_throw", delegate
						{
							ThrowBox();
						})
					},
					{
						KeyCode.G,
						("box_put", delegate
						{
							PutBox();
						})
					}
				});
			}
		}
		else if (pickedBox.IsOpen())
		{
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
						KeyCode.C,
						("box_close", delegate
						{
							CloseBox();
						})
					},
					{
						KeyCode.R,
						("box_throw", delegate
						{
							ThrowBox();
						})
					},
					{
						KeyCode.G,
						("box_put", delegate
						{
							PutBox();
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
						KeyCode.C,
						("box_close", delegate
						{
							CloseBox();
						})
					},
					{
						KeyCode.R,
						("box_throw", delegate
						{
							ThrowBox();
						})
					},
					{
						KeyCode.G,
						("box_put", delegate
						{
							PutBox();
						})
					}
				});
			}
		}
		else
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("box_open", delegate
					{
						OpenBox();
					})
				},
				{
					KeyCode.R,
					("box_throw", delegate
					{
						ThrowBox();
					})
				},
				{
					KeyCode.G,
					("box_put", delegate
					{
						PutBox();
					})
				}
			});
		}
	}

	public void UpdateMenuForStorageShelf()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
		{
			{
				KeyCode.Mouse0,
				("box_place_storage", delegate
				{
					Place();
				})
			},
			{
				KeyCode.R,
				("box_throw", delegate
				{
					ThrowBox();
				})
			},
			{
				KeyCode.G,
				("box_put", delegate
				{
					PutBox();
				})
			}
		});
	}

	private void Place()
	{
		EventManager.NotifyEvent(PlaceableEvents.PLACE_PRODUCT);
	}

	private void TakeProduct()
	{
		EventManager.NotifyEvent(PlaceableEvents.TAKE_PRODUCT);
	}

	private void StoreBox()
	{
		EventManager.NotifyEvent(PlaceableEvents.STORE_BOX);
	}

	public void ThrowBox()
	{
		if (!(pickedBox == null))
		{
			playerMove.UpdateRadius(objectPicked: false);
			RegisterBox(pickedBox);
			pickedBox.OnBeforeThrow();
			pickedBox.transform.SetParent(null);
			pickedBox.RigidBody.constraints = RigidbodyConstraints.None;
			pickedBox.RigidBody.linearVelocity = boxTargetTransform.right * -9f;
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
			pickedBox = null;
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_THROW);
			EventManager.NotifyEvent(GameEvents.BOX_THROWN);
			SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		}
	}

	public void PutBox()
	{
		if (!(pickedBox == null))
		{
			playerMove.UpdateRadius(objectPicked: false);
			pickedBox.transform.SetParent(null);
			Vector3 localEulerAngles = pickedBox.transform.localEulerAngles;
			pickedBox.transform.localEulerAngles = new Vector3(0f, localEulerAngles.y, 0f);
			pickedBox.RigidBody.isKinematic = true;
			pickedBox.StartNewPlacement(delegate
			{
				pickedBox.RigidBody.isKinematic = false;
				pickedBox.RigidBody.constraints = RigidbodyConstraints.None;
				RegisterBox(pickedBox);
				pickedBox = null;
				EventManager.NotifyEvent(GameEvents.BOX_THROWN);
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
			});
		}
	}

	public void OnBoxPut()
	{
		playerMove.UpdateRadius(objectPicked: false);
		pickedBox.OnBeforeThrow();
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		pickedBox = null;
		SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_THROW);
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	public void OnBoxPutWithoutThrow()
	{
		playerMove.UpdateRadius(objectPicked: false);
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		pickedBox = null;
		SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_THROW);
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	public void OpenBox()
	{
		pickedBox.Open();
		UpdateMenu();
		EventManager.NotifyEvent(GameEvents.BOX_OPENED);
	}

	public void CloseBox()
	{
		pickedBox.Close();
		UpdateMenu();
	}

	public Box GetPickedBox()
	{
		return pickedBox;
	}

	public void RegisterBox(Box box)
	{
		if (!activeBoxes.ContainsKey(box.BoxID))
		{
			activeBoxes.Add(box.BoxID, box);
		}
	}

	public void UnregisterBox(Box box)
	{
		if (activeBoxes.ContainsKey(box.BoxID))
		{
			activeBoxes.Remove(box.BoxID);
		}
	}

	private void FixedUpdate()
	{
		foreach (KeyValuePair<int, Box> activeBox in activeBoxes)
		{
			activeBox.Value.PhysicsUpdate();
		}
	}

	public bool HasBox(int boxID)
	{
		return spawnedBoxes.ContainsKey(boxID);
	}
}
