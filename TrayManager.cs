using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TrayManager : SingletonBehaviour<TrayManager>
{
	[SerializeField]
	private Transform trayParent;

	[SerializeField]
	private Transform trayTarget;

	private static string TRAY_COUNT_KEY = "TRAY_COUNT_KEY";

	private Tray pickedTray;

	private float PICKUP_DURATION = 0.5f;

	private bool isPicked;

	public bool IsPicked => isPicked;

	public Tray PickedTray => pickedTray;

	public int SpawnNewTrayID()
	{
		int num = GenericDataSerializer.LoadInt(TRAY_COUNT_KEY);
		GenericDataSerializer.SaveInt(TRAY_COUNT_KEY, num + 1);
		return num;
	}

	public void PickUpTray(Tray tray)
	{
		pickedTray = tray;
		isPicked = true;
		SingletonBehaviour<HotKeyManager>.Instance.RefreshEnablity();
		pickedTray.transform.DOKill();
		pickedTray.transform.SetParent(trayParent);
		pickedTray.ContainerShelf.EnableClickableCollider(enable: true);
		pickedTray.transform.DOLocalMove(trayTarget.localPosition, PICKUP_DURATION);
		pickedTray.transform.DOLocalRotate(trayTarget.localEulerAngles, PICKUP_DURATION).OnComplete(delegate
		{
			UpdateMenu();
			EventManager.NotifyEvent(GameEvents.TRAY_PICKED_UP);
			SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		});
	}

	public void UpdateMenu()
	{
		if (IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.R,
				("finish", delegate
				{
					CancelPutDown();
				})
			} });
		}
	}

	public void UpdateMenuWithPut()
	{
		if (IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("put", delegate
					{
						PlaceTray();
					})
				},
				{
					KeyCode.R,
					("finish", delegate
					{
						CancelPutDown();
					})
				}
			});
		}
	}

	public void UpdateMenuWithTake()
	{
		if (IsPicked)
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
					("finish", delegate
					{
						CancelPutDown();
					})
				}
			});
		}
	}

	public void UpdateMenuWithPlace(bool takeAvailable = false)
	{
		if (!(pickedTray != null))
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
					KeyCode.R,
					("finish", delegate
					{
						CancelPutDown();
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
					("finish", delegate
					{
						CancelPutDown();
					})
				}
			});
		}
	}

	private void TakeProduct()
	{
		EventManager.NotifyEvent(PlaceableEvents.TAKE_PRODUCT);
	}

	private void Place()
	{
		if (!pickedTray.IsEmpty())
		{
			EventManager.NotifyEvent(PlaceableEvents.PLACE_PRODUCT);
		}
	}

	private void PlaceTray()
	{
		EventManager.NotifyEvent(GameEvents.TRAY_PUTDOWN);
		pickedTray.ReleaseAnimated();
		Release();
	}

	private void CancelPutDown()
	{
		EventManager.NotifyEvent(GameEvents.TRAY_CANCEL_PUTDOWN);
		pickedTray.Release();
		Release();
	}

	private void Release()
	{
		isPicked = false;
		pickedTray = null;
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		SingletonBehaviour<HotKeyManager>.Instance.RefreshEnablity();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
