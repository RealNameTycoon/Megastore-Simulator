using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ChoppingStandPlaceable : Placeable
{
	[SerializeField]
	private Transform playerSitTransform;

	[SerializeField]
	private Transform playerStandMoveTransform;

	[SerializeField]
	private Transform playerStandLookTransform;

	[SerializeField]
	private Transform productPreperationPoint;

	[SerializeField]
	private Clickable choppingWoodBlockClickable;

	[SerializeField]
	private Transform knifeTransform;

	[SerializeField]
	private Clickable foodTrayClickable;

	[SerializeField]
	private Transform topFoodTrayTransform;

	[SerializeField]
	private Transform disposalPoint;

	private bool isAutomated;

	private Customer awaitingCustomer;

	[SerializeField]
	private List<Product> currentProducts = new List<Product>();

	private FoodTrayProduct foodTrayProduct;

	private bool knifeAnimating;

	private Vector3 knifeStartPosition;

	private Vector3 knifeStartRotation;

	private List<GameObject> choppedFragments = new List<GameObject>();

	private int currentProductIndex;

	private bool playerSitting;

	public bool IsAutomated => isAutomated;

	public Customer AwaitingCustomer => awaitingCustomer;

	public List<Product> CurrentProducts => currentProducts;

	public bool PlayerSitting => playerSitting;

	private void Awake()
	{
		choppingWoodBlockClickable.SetHoverStartedAction(OnChoppingWoodBlockHoverStarted);
		choppingWoodBlockClickable.SetHoverEndedAction(RepaintButtonsForChoppingStand);
		foodTrayClickable.SetHoverStartedAction(OnTrayHoverStarted);
		foodTrayClickable.SetHoverEndedAction(RepaintButtonsForChoppingStand);
		knifeStartPosition = knifeTransform.localPosition;
		knifeStartRotation = knifeTransform.localEulerAngles;
	}

	public void EnableKnife(bool enable)
	{
		knifeTransform.gameObject.SetActive(enable);
	}

	public void JobStep()
	{
		if (awaitingCustomer != null)
		{
			if (ProductReadyToGive())
			{
				OnTrayClicked();
			}
			else
			{
				OnChoppingWoodBlockClicked();
			}
		}
	}

	public bool HasCustomer()
	{
		return awaitingCustomer != null;
	}

	private void OnChoppingWoodBlockHoverStarted()
	{
		if (SingletonBehaviour<PlayerSittingManager>.Instance.IsSittingOnChoppingStand)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("place_hold_chop", delegate
					{
						OnChoppingWoodBlockClicked();
					})
				},
				{
					SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
					("leave", delegate
					{
						OverChoppingStand();
					})
				}
			});
		}
	}

	private void OnTrayHoverStarted()
	{
		if (SingletonBehaviour<PlayerSittingManager>.Instance.IsSittingOnChoppingStand)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("wrap", delegate
					{
						OnTrayClicked();
					})
				},
				{
					SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
					("leave", delegate
					{
						OverChoppingStand();
					})
				}
			});
		}
	}

	private void OnChoppingWoodBlockClicked()
	{
		if (knifeAnimating || currentProductIndex >= currentProducts.Count || !(foodTrayProduct != null))
		{
			return;
		}
		ChoppableProduct choppableProduct = currentProducts[currentProductIndex] as ChoppableProduct;
		if (choppableProduct.IsFullyChopped || (!choppableProduct.Sliceable && foodTrayProduct.ProductCount == currentProducts.Count))
		{
			return;
		}
		if (!choppableProduct.Sliceable)
		{
			choppableProduct.transform.SetParent(foodTrayProduct.transform);
			foodTrayProduct.AddProduct(choppableProduct);
			choppableProduct.transform.DOLocalMove(choppableProduct.TrayLocalPositions[currentProductIndex], 0.5f);
			choppableProduct.transform.DOLocalRotate(choppableProduct.TrayEulerAngles[currentProductIndex], 0.5f);
			if (playerSitting)
			{
				SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
			}
			currentProductIndex++;
			return;
		}
		knifeAnimating = true;
		Vector3 nextChopPoint = choppableProduct.GetNextChopPoint();
		bool shouldDisposeNextCut = choppableProduct.ShouldDisposeNextCut();
		knifeTransform.DOKill();
		Vector3 zero = Vector3.zero;
		if (!choppableProduct.HasCustomCuttingPoints)
		{
			ShortcutExtensions.DOLocalRotate(endValue: new Vector3(knifeStartRotation.x, knifeStartRotation.y, knifeStartRotation.z - 90f), target: knifeTransform, duration: 0.25f);
		}
		else
		{
			Quaternion endValue = Quaternion.LookRotation(choppableProduct.GetNextCuttingPoint().right.normalized) * Quaternion.Euler(0f, 0f, -90f);
			knifeTransform.DORotateQuaternion(endValue, 0.2f);
		}
		knifeTransform.DOMove(nextChopPoint + Vector3.up * 0.1f, 0.3f).OnComplete(delegate
		{
			ShortcutExtensions.DOLocalRotate(endValue: new Vector3(knifeTransform.localEulerAngles.x + 30f, knifeTransform.localEulerAngles.y, knifeTransform.localEulerAngles.z), target: knifeTransform, duration: 0.25f).SetLoops(2, LoopType.Yoyo);
			int step = 0;
			GameObject choppedFragment = choppableProduct.Cut();
			choppedFragment.transform.DOScale(new Vector3(choppedFragment.transform.localScale.x, choppedFragment.transform.localScale.y * 0.8f, choppedFragment.transform.localScale.z * 1.05f), 0.125f).SetLoops(2, LoopType.Yoyo);
			if (playerSitting)
			{
				SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.CHOP_SOUND);
			}
			knifeTransform.DOLocalMoveY(productPreperationPoint.localPosition.y, 0.25f).SetLoops(2, LoopType.Yoyo).OnStepComplete(delegate
			{
				if (step == 0)
				{
					knifeAnimating = false;
					if (choppableProduct != null)
					{
						if (shouldDisposeNextCut)
						{
							DisposeFragment(choppedFragment);
						}
						else
						{
							if (choppedFragment == null || foodTrayProduct == null)
							{
								return;
							}
							choppedFragment.transform.SetParent(foodTrayProduct.transform);
							foodTrayProduct.AddChoppedItem(choppedFragment);
							choppedFragment.transform.DOLocalMove(choppableProduct.TrayLocalPositions[choppableProduct.ChoppingIndex - 1], 0.5f);
							choppedFragment.transform.DOLocalRotate(choppableProduct.TrayEulerAngles[choppableProduct.ChoppingIndex - 1], 0.5f);
							if (choppableProduct.IsFullyChopped)
							{
								if (choppableProduct.DisposeLastFragment)
								{
									DisposeFragment(choppableProduct.LastFragment);
								}
								else
								{
									GameObject lastFragment = choppableProduct.LastFragment;
									lastFragment.transform.SetParent(foodTrayProduct.transform);
									foodTrayProduct.AddChoppedItem(lastFragment);
									lastFragment.transform.DOLocalMove(choppableProduct.TrayLocalPositions[choppableProduct.ChoppingIndex], 0.5f);
									lastFragment.transform.DOLocalRotate(choppableProduct.TrayEulerAngles[choppableProduct.ChoppingIndex], 0.5f);
								}
							}
						}
					}
					else if (!choppableProduct.IsFullyChopped)
					{
					}
				}
				else if (step == 1)
				{
					knifeTransform.localPosition = knifeStartPosition;
					knifeTransform.localEulerAngles = knifeStartRotation;
				}
				step++;
			});
		});
	}

	public void ChopProductInstantly()
	{
		ChoppableProduct choppableProduct = currentProducts[currentProductIndex] as ChoppableProduct;
		if (choppableProduct.IsFullyChopped || (!choppableProduct.Sliceable && foodTrayProduct.ProductCount == currentProducts.Count))
		{
			return;
		}
		if (!choppableProduct.Sliceable)
		{
			choppableProduct.transform.SetParent(foodTrayProduct.transform);
			foodTrayProduct.AddProduct(choppableProduct);
			choppableProduct.transform.DOLocalMove(choppableProduct.TrayLocalPositions[currentProductIndex], 0.5f);
			choppableProduct.transform.DOLocalRotate(choppableProduct.TrayEulerAngles[currentProductIndex], 0.5f);
			currentProductIndex++;
			return;
		}
		bool flag = choppableProduct.ShouldDisposeNextCut();
		GameObject gameObject = choppableProduct.Cut();
		if (!(choppableProduct != null))
		{
			return;
		}
		if (flag)
		{
			DisposeFragment(gameObject);
		}
		else
		{
			if (gameObject == null || foodTrayProduct == null)
			{
				return;
			}
			gameObject.transform.SetParent(foodTrayProduct.transform);
			foodTrayProduct.AddChoppedItem(gameObject);
			gameObject.transform.DOLocalMove(choppableProduct.TrayLocalPositions[choppableProduct.ChoppingIndex - 1], 0.5f);
			gameObject.transform.DOLocalRotate(choppableProduct.TrayEulerAngles[choppableProduct.ChoppingIndex - 1], 0.5f);
			if (choppableProduct.IsFullyChopped)
			{
				if (choppableProduct.DisposeLastFragment)
				{
					DisposeFragment(choppableProduct.LastFragment);
					return;
				}
				GameObject lastFragment = choppableProduct.LastFragment;
				lastFragment.transform.SetParent(foodTrayProduct.transform);
				foodTrayProduct.AddChoppedItem(lastFragment);
				lastFragment.transform.DOLocalMove(choppableProduct.TrayLocalPositions[choppableProduct.ChoppingIndex], 0.5f);
				lastFragment.transform.DOLocalRotate(choppableProduct.TrayEulerAngles[choppableProduct.ChoppingIndex], 0.5f);
			}
		}
	}

	private void DisposeFragment(GameObject fragment)
	{
		fragment.transform.DOMove(disposalPoint.position, 0.5f).OnComplete(delegate
		{
			UnityEngine.Object.Destroy(fragment);
		});
	}

	private void OnTrayClicked()
	{
		if (ProductReadyToGive())
		{
			GivePreparedProduct(foodTrayProduct);
			choppedFragments.Clear();
		}
	}

	public bool ProductReadyToGive()
	{
		if (currentProducts.Count == 0)
		{
			return false;
		}
		if (foodTrayProduct == null || foodTrayProduct.IsEmpty)
		{
			return false;
		}
		ChoppableProduct choppableProduct = currentProducts[0] as ChoppableProduct;
		if (!choppableProduct.IsFullyChopped)
		{
			if (!choppableProduct.Sliceable)
			{
				return foodTrayProduct.ProductCount == currentProducts.Count;
			}
			return false;
		}
		return true;
	}

	public void GivePreparedProduct(FoodTrayProduct product)
	{
		if (awaitingCustomer != null && product != null)
		{
			if (!product.HasProducts)
			{
				SingletonBehaviour<ProductPool>.Instance.PutBackToPool(currentProducts[currentProductIndex]);
			}
			product.EnableCover();
			if (playerSitting)
			{
				SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.PACK_PLASTIC);
			}
			awaitingCustomer.GivePreparedProduct(product);
			awaitingCustomer = null;
			currentProducts.Clear();
			currentProductIndex = 0;
			foodTrayProduct = null;
		}
	}

	public override List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		return new List<(KeyCode, (string, Action))> { (KeyCode.Mouse0, ("interact", delegate
		{
			OnStandClicked();
		})) };
	}

	public void OnStandClicked()
	{
		if (!SingletonBehaviour<UIManager>.Instance.AllWindowsClosed())
		{
			return;
		}
		if (IsOccupiedByStaff())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("reserved_chopping_stand", base.transform);
			return;
		}
		playerSitting = true;
		EventManager.NotifyEvent(PlaceableEvents.SIT_ON_CHOPPING_STAND);
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
		SingletonBehaviour<PlayerMove>.Instance.transform.DOMove(playerSitTransform.position, 0.2f);
		SingletonBehaviour<PlayerLook>.Instance.transform.DORotate(playerSitTransform.eulerAngles, 0.2f).OnComplete(delegate
		{
			SingletonBehaviour<PlayerLook>.Instance.UpdateClamp(playerSitTransform.localEulerAngles.x);
			RepaintButtonsForChoppingStand();
		});
	}

	private void RepaintButtonsForChoppingStand()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			SingletonBehaviour<KeyBindingManager>.Instance.LeaveKey,
			("leave", delegate
			{
				OverChoppingStand();
			})
		} });
	}

	public override bool IsServingPlaceable()
	{
		return true;
	}

	private void OverChoppingStand()
	{
		if (SingletonBehaviour<TooltipUI>.Instance.HasOpened(base.transform))
		{
			SingletonBehaviour<TooltipUI>.Instance.Close();
		}
		SingletonBehaviour<PlayerMove>.Instance.transform.DOMove(playerStandMoveTransform.position, 0.2f);
		SingletonBehaviour<PlayerLook>.Instance.CameraParent.transform.DORotate(playerStandMoveTransform.rotation.eulerAngles, 0.2f).OnComplete(delegate
		{
			SingletonBehaviour<PlayerLook>.Instance.RotationLocked = false;
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
		});
		SingletonBehaviour<PlayerLook>.Instance.transform.DOLocalRotate(playerStandLookTransform.localEulerAngles, 0.2f).OnComplete(delegate
		{
			SingletonBehaviour<PlayerLook>.Instance.UpdateClamp(playerStandLookTransform.localEulerAngles.x);
		});
		playerSitting = false;
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		EventManager.NotifyEvent(PlaceableEvents.LEFT_CHOPPING_STAND);
	}

	public void AskForProduct(List<Product> products, Customer customer)
	{
		if (products.Count == 1)
		{
			Product product = products[0];
			ChoppableProduct choppableProduct = product as ChoppableProduct;
			product.transform.DOKill();
			product.transform.SetParent(productPreperationPoint);
			product.transform.DoCurvedLocalMove(Vector3.zero, 0.5f, 2f);
			Vector3 endValue = (choppableProduct.ForwardAlligned ? Vector3.zero : (Vector3.up * 90f));
			product.transform.DOLocalRotate(endValue, 0.5f).OnComplete(delegate
			{
				awaitingCustomer = customer;
				currentProducts = products;
				currentProductIndex = 0;
				choppableProduct.OnPlacedOnChoppingStand();
			});
		}
		else
		{
			for (int num = 0; num < products.Count; num++)
			{
				Product product2 = products[num];
				ChoppableProduct choppableProduct2 = product2 as ChoppableProduct;
				product2.transform.DOKill();
				product2.transform.SetParent(productPreperationPoint);
				product2.transform.DoCurvedLocalMove(choppableProduct2.TrayLocalPositions[num], 0.5f, 2f);
				if (num == products.Count - 1)
				{
					product2.transform.DOLocalRotate(choppableProduct2.TrayEulerAngles[num], 0.55f).OnComplete(delegate
					{
						awaitingCustomer = customer;
						currentProducts = products;
						currentProductIndex = 0;
						choppableProduct2.OnPlacedOnChoppingStand();
					});
				}
				else
				{
					product2.transform.DOLocalRotate(choppableProduct2.TrayEulerAngles[num], 0.5f);
				}
			}
		}
		Product product3 = SingletonBehaviour<ProductPool>.Instance.GetProduct(ProductType.FOOD_TRAY);
		foodTrayProduct = product3 as FoodTrayProduct;
		foodTrayProduct.SetProduct(products[0].Data.type, SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(products[0].Data.type) * (float)products.Count);
		foodTrayProduct.transform.position = topFoodTrayTransform.position;
		foodTrayProduct.transform.SetParent(topFoodTrayTransform);
		foodTrayProduct.transform.localPosition = Vector3.zero;
		foodTrayProduct.transform.localEulerAngles = Vector3.zero;
	}
}
