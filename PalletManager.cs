using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PalletManager : SingletonBehaviour<PalletManager>
{
	[Serializable]
	public class BoxPalletPositionDictionary : UnitySerializedDictionary<BoxType, PositionContainer>
	{
	}

	[SerializeField]
	private BoxPalletPositionDictionary boxPositionDictionary;

	private const string SPAWNED_PALLET_COUNT_KEY = "SpawnedPalletCount";

	private const string SPAWNED_PALLET_IDS_KEY = "SPAWNED_PALLET_IDS";

	private const string SPAWNED_PALLET_TYPES_KEY = "SPAWNED_PALLET_TYPES";

	private List<PalletType> spawnedPalletTypes;

	private Dictionary<int, Pallet> spawnedPallets = new Dictionary<int, Pallet>();

	private List<int> spawnedPalletIDs;

	private int spawnedPalletCount;

	private Pallet pickedPallet;

	public static float PALLET_DEPOSIT_AMOUNT = 10f;

	public Pallet PickedPallet => pickedPallet;

	public bool IsPalletPicked => pickedPallet != null;

	public bool IsPalletOnAir
	{
		get
		{
			if (pickedPallet != null && (pickedPallet.gameObject.layer != RayShooter.PALLET_LAYER || DOTween.IsTweening(pickedPallet)))
			{
				return true;
			}
			return false;
		}
	}

	public PositionContainer GetBoxPositions(BoxType type)
	{
		return boxPositionDictionary[type];
	}

	public int GetPalletCapacity(BoxType type)
	{
		return boxPositionDictionary[type].localPositions.Count;
	}

	public int GetPalletCapacity(ProductType type)
	{
		return GetPalletCapacity(SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(type).boxType);
	}

	private new void Awake()
	{
		base.Awake();
		spawnedPalletCount = GenericDataSerializer.LoadInt("SpawnedPalletCount");
		spawnedPalletIDs = GenericDataSerializer.Load("SPAWNED_PALLET_IDS", new List<int>());
		spawnedPalletTypes = GenericDataSerializer.Load("SPAWNED_PALLET_TYPES", new List<PalletType>());
		EventManager.AddListener(StartupEvents.BOXES_INITIALIZED, Initialize);
		EventManager.AddListener(StartupEvents.TEMPERATURE_ZONES_INITIALIZED, RegisterTemperatureZones);
	}

	private void Initialize()
	{
		for (int i = 0; i < spawnedPalletIDs.Count; i++)
		{
			Pallet pallet = SingletonBehaviour<PalletPool>.Instance.GetPallet(spawnedPalletTypes[i]);
			pallet.InitializeOldPallet(spawnedPalletIDs[i]);
			spawnedPallets.Add(spawnedPalletIDs[i], pallet);
		}
		EventManager.NotifyEvent(StartupEvents.PALLETS_INITIALIZED);
	}

	private void RegisterTemperatureZones()
	{
		foreach (KeyValuePair<int, Pallet> spawnedPallet in spawnedPallets)
		{
			spawnedPallet.Value.CheckAndUpdateTemperatureZone();
		}
	}

	public void PickUpPallet(Pallet pallet)
	{
		if (pallet.IsReservedToStaff())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("pallet_reserved_staff_error", base.transform);
			return;
		}
		pickedPallet = pallet;
		SingletonBehaviour<HotKeyManager>.Instance.RefreshEnablity();
		pickedPallet.transform.SetParent(SingletonBehaviour<PlayerMove>.Instance.PalletParent);
		pickedPallet.transform.DOLocalMove(Vector3.zero, BoxManager.BOX_PICKUP_DURATION);
		EventManager.NotifyEvent(GameEvents.PALLET_PICK_STARTED);
		EventManager.NotifyEvent(GameEvents.PALLET_TAKEN, pallet.PalletID);
		pickedPallet.EnableCollider(enable: false);
		_ = pickedPallet;
		pickedPallet.transform.DOLocalRotate(Vector3.zero, BoxManager.BOX_PICKUP_DURATION).OnComplete(delegate
		{
			UpdateMenu();
			EventManager.NotifyEvent(GameEvents.PALLET_PICKED_UP);
		});
	}

	public void UpdateMenu()
	{
		if (pickedPallet != null && !IsPalletOnAir)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.G,
				("box_put", delegate
				{
					PutPallet();
				})
			} }, base.transform);
		}
	}

	public void UpdateMenuForTrash()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
		{
			KeyCode.Mouse0,
			("box_throw", delegate
			{
				TrashPallet();
			})
		} });
	}

	public void TrashPallet()
	{
		if (!(pickedPallet == null))
		{
			pickedPallet.transform.SetParent(null);
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
			int palletID = pickedPallet.PalletID;
			pickedPallet = null;
			EventManager.NotifyEvent(GameEvents.PALLET_TRASHED, palletID);
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_THROW);
			SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		}
	}

	public void ThrowPallet()
	{
		if (!(pickedPallet == null))
		{
			pickedPallet.transform.SetParent(null);
			pickedPallet.RigidBody.linearVelocity = pickedPallet.transform.right * 9f;
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
			pickedPallet = null;
			SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.BOX_THROW);
			EventManager.NotifyEvent(GameEvents.PALLET_THROWN);
			SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		}
	}

	public void PutPallet()
	{
		if (!(pickedPallet == null))
		{
			pickedPallet.transform.SetParent(null);
			Vector3 localEulerAngles = pickedPallet.transform.localEulerAngles;
			pickedPallet.transform.localEulerAngles = new Vector3(0f, localEulerAngles.y, 0f);
			pickedPallet.Moveable.SetStationary(stationary: false);
			pickedPallet.StartNewPlacement(delegate
			{
				pickedPallet.EnableCollider(enable: true);
				pickedPallet = null;
				EventManager.NotifyEvent(GameEvents.PALLET_THROWN);
				SingletonBehaviour<RayShooter>.Instance.ImitateHover();
			});
		}
	}

	public Pallet GetPallet(int palletID)
	{
		if (!spawnedPallets.ContainsKey(palletID))
		{
			return null;
		}
		return spawnedPallets[palletID];
	}

	public Pallet SpawnPallet(PalletType palletType, Vector3 position, Vector3 eulerRotation)
	{
		Pallet pallet = SingletonBehaviour<PalletPool>.Instance.GetPallet(palletType);
		pallet.Initialize(spawnedPalletCount);
		pallet.transform.position = position;
		pallet.transform.eulerAngles = eulerRotation;
		pallet.SaveLocation();
		spawnedPallets.Add(spawnedPalletCount, pallet);
		spawnedPalletIDs.Add(spawnedPalletCount);
		spawnedPalletTypes.Add(palletType);
		spawnedPalletCount++;
		SavePalletData();
		return pallet;
	}

	public void DeletePallet(int palletID)
	{
		int index = spawnedPalletIDs.IndexOf(palletID);
		Pallet pallet = spawnedPallets[palletID];
		spawnedPalletIDs.RemoveAt(index);
		spawnedPalletTypes.RemoveAt(index);
		spawnedPallets.Remove(palletID);
		pallet.DeleteData();
		SavePalletData();
		SingletonBehaviour<PalletPool>.Instance.PutBackToPool(pallet);
	}

	private void SavePalletData()
	{
		GenericDataSerializer.SaveInt("SpawnedPalletCount", spawnedPalletCount);
		GenericDataSerializer.Save("SPAWNED_PALLET_IDS", spawnedPalletIDs);
		GenericDataSerializer.Save("SPAWNED_PALLET_TYPES", spawnedPalletTypes);
	}
}
