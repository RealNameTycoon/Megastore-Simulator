using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Oven : Furniture
{
	[SerializeField]
	private List<Tray> trays;

	[SerializeField]
	private List<TrayShelf> trayShelf;

	[SerializeField]
	private List<TrayShelf> storageTrayShelf;

	[SerializeField]
	private List<Transform> traySlots;

	[SerializeField]
	private List<PlacementStarter> placementStarters;

	[SerializeField]
	private Outline ovenOutline;

	[SerializeField]
	private Transform knobTransform;

	[SerializeField]
	private AudioSource source;

	[SerializeField]
	private AudioClip dingClip;

	[SerializeField]
	private AudioClip ovenWorking;

	[SerializeField]
	private Light pointLight;

	private bool cooking;

	private float initialAudioVolume;

	[SerializeField]
	private int reservedForEmployeeID = -1;

	private Action onCookingFinished;

	public bool Cooking => cooking;

	public bool IsReservedToStaff => reservedForEmployeeID != -1;

	public void Reserve(int employeeID)
	{
		reservedForEmployeeID = employeeID;
	}

	public void Release()
	{
		reservedForEmployeeID = -1;
		onCookingFinished = null;
	}

	private void Start()
	{
		initialAudioVolume = source.volume;
		source.volume = SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier * initialAudioVolume;
		for (int i = 0; i < placementStarters.Count; i++)
		{
			placementStarters[i].SetMouseHoverAction(OnMouseHovered);
			placementStarters[i].SetMouseHoverEndedAction(OnMouseHoverEnded);
		}
		EventManager.AddListener(UIEvents.AUDIO_MULTIPLIER_CHANGED, OnAudioMultiplierChanged);
		EventManager.AddListener(GameEvents.GAME_PAUSED, StopSound);
		EventManager.AddListener(GameEvents.GAME_RESUMED, ResumeSound);
	}

	private void OnMouseHovered()
	{
		ovenOutline.enabled = true;
		if (!AllTraysEmpty() && !AnyTrayCooked() && !cooking && SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.F,
					("pack", delegate
					{
						Pack();
					})
				},
				{
					KeyCode.E,
					("turn_on", delegate
					{
						TurnOn();
					})
				},
				{
					KeyCode.Mouse1,
					("place_move", delegate
					{
						StartNewPlacement();
					})
				}
			}, base.transform);
		}
	}

	private void OnMouseHoverEnded()
	{
		ovenOutline.enabled = false;
		if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
	}

	private void StopSound()
	{
		source.Pause();
	}

	private void ResumeSound()
	{
		source.UnPause();
	}

	private void OnAudioMultiplierChanged()
	{
		source.volume = SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier * initialAudioVolume;
	}

	public void TurnOn(Action onComplete = null, bool isNPC = false)
	{
		if (!isNPC && IsReservedToStaff)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("oven_reserved_staff_error", base.transform);
		}
		else
		{
			if (cooking)
			{
				return;
			}
			onCookingFinished = onComplete;
			for (int i = 0; i < trayShelf.Count; i++)
			{
				if (trayShelf[i].ContainedTrayID != -1 && !trayShelf[i].ContainedTray.IsCooked)
				{
					trayShelf[i].ContainedTray.StartCooking(OnCookingFinished);
				}
			}
			pointLight.intensity = 0f;
			pointLight.enabled = true;
			pointLight.DOIntensity(1f, 0.3f).OnComplete(delegate
			{
				pointLight.DOIntensity(2.2f, (CookableProduct.COOK_DURATION - 1f) / 8f).SetLoops(8, LoopType.Yoyo).SetEase(Ease.InOutSine)
					.OnComplete(delegate
					{
						pointLight.DOIntensity(0f, 0.3f);
					});
			});
			source.PlayOneShot(ovenWorking);
			knobTransform.localEulerAngles = new Vector3(90f, 0f, 0f);
			source.clip = dingClip;
			source.PlayDelayed(CookableProduct.COOK_DURATION - 4.2f);
			knobTransform.DOLocalRotate(new Vector3(90f, 0f, 360f), CookableProduct.COOK_DURATION, RotateMode.FastBeyond360);
			cooking = true;
			if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
			{
				SingletonBehaviour<ButtonsWindow>.Instance.Close();
			}
			EventManager.NotifyEvent(GameEvents.OVEN_TURNED_ON);
		}
	}

	public void OnDeactivateInstant()
	{
		pointLight.DOKill();
		pointLight.intensity = 0f;
		pointLight.enabled = true;
		source.DOKill();
		source.Stop();
		knobTransform.DOKill();
		cooking = false;
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].ContainedTray != null)
			{
				trayShelf[i].ContainedTray.OnDeactivateInstant();
			}
		}
	}

	public bool AnyTrayCooked()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].IsTrayCooked())
			{
				MonoBehaviour.print(trays[i].gameObject.name + " is cooked");
				return true;
			}
		}
		return false;
	}

	public bool AllTraysEmpty()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (!trayShelf[i].IsTrayEmpty())
			{
				return false;
			}
		}
		return true;
	}

	private void OnCookingFinished()
	{
		if (cooking)
		{
			cooking = false;
			onCookingFinished?.Invoke();
			onCookingFinished = null;
		}
		EventManager.NotifyEvent(GameEvents.OVEN_FINISHED_COOKING);
	}

	public override void InitializeOldFurniture(int id, bool isPacked = false)
	{
		base.InitializeOldFurniture(id, isPacked);
		if (SingletonBehaviour<SpawnManager>.Instance.InitialProductGroup == ProductGroup.BAKERY && base.FurnitureID == 0 && !trayShelf[0].HasInitializedBefore())
		{
			for (int i = 0; i < trayShelf.Count; i++)
			{
				trayShelf[i].InitializeNew();
			}
			for (int j = 0; j < storageTrayShelf.Count; j++)
			{
				storageTrayShelf[j].InitializeNew();
			}
			SingletonBehaviour<BakerManager>.Instance.RegisterOven(this);
		}
		else
		{
			for (int k = 0; k < trayShelf.Count; k++)
			{
				trayShelf[k].InitializeOld();
				storageTrayShelf[k].InitializeOld();
			}
			SingletonBehaviour<BakerManager>.Instance.RegisterOven(this);
		}
	}

	public override void InitializeNewFurniture(int id)
	{
		base.InitializeNewFurniture(id);
		for (int i = 0; i < trayShelf.Count; i++)
		{
			trayShelf[i].InitializeNew();
		}
		for (int j = 0; j < storageTrayShelf.Count; j++)
		{
			storageTrayShelf[j].InitializeNew();
		}
		SingletonBehaviour<BakerManager>.Instance.RegisterOven(this);
	}

	public void PutTrayBack(Tray tray)
	{
		int index = trays.IndexOf(tray);
		tray.transform.DOKill();
		tray.transform.SetParent(traySlots[index]);
		tray.transform.localPosition = Vector3.zero;
		tray.transform.localEulerAngles = Vector3.zero;
	}

	public void PutTrayBackAnimated(Tray tray)
	{
		int index = trays.IndexOf(tray);
		tray.transform.DOKill();
		tray.transform.SetParent(traySlots[index]);
		SingletonBehaviour<AudioManager>.Instance.PlayAudio((AudioManager.AudioTypes)(9 + UnityEngine.Random.Range(0, 2)));
		tray.transform.DOLocalMove(Vector3.zero, 0.3f);
		tray.transform.DOLocalRotate(Vector3.zero, 0.3f);
	}

	public void SetTraysItemFinishedAction(Action onItemsFinished)
	{
		for (int i = 0; i < trays.Count; i++)
		{
			Tray tray = trays[i];
			tray.OnItemsFinished = (Action)Delegate.Combine(tray.OnItemsFinished, onItemsFinished);
		}
	}

	public override void StartNewPlacement(Action onPlacementEnded = null, Action onCancelPlacement = null)
	{
		if (cooking)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_oven_cooking", base.transform);
		}
		else
		{
			base.StartNewPlacement(onPlacementEnded, onCancelPlacement);
		}
	}

	public override List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		if (AllTraysEmpty() || AnyTrayCooked())
		{
			if (SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
			{
				return new List<(KeyCode, (string, Action))> { (KeyCode.F, ("pack", delegate
				{
					Pack();
				})) };
			}
			return null;
		}
		if (!cooking && SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
		{
			return new List<(KeyCode, (string, Action))>
			{
				(KeyCode.F, ("pack", delegate
				{
					Pack();
				})),
				(KeyCode.E, ("turn_on", delegate
				{
					TurnOn();
				}))
			};
		}
		return null;
	}

	public bool CanCook()
	{
		if (!AllTraysEmpty() && !AnyTrayCooked())
		{
			return !cooking;
		}
		return false;
	}

	public bool HasEmptyCookingSlot()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].ContainedTrayID == -1 || trayShelf[i].ContainedTray.IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyCookingTrayTaken()
	{
		if (SingletonBehaviour<TrayManager>.Instance.IsPicked)
		{
			for (int i = 0; i < trayShelf.Count; i++)
			{
				if (trayShelf[i].ContainedTrayID != -1 && SingletonBehaviour<TrayManager>.Instance.PickedTray.TrayID == trayShelf[i].ContainedTrayID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public TrayShelf GetEmptyCookingSlot()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].ContainedTrayID == -1 || trayShelf[i].ContainedTray.IsEmpty())
			{
				return trayShelf[i];
			}
		}
		return null;
	}

	public bool OvenIsEmpty()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].ContainedTrayID != -1)
			{
				return false;
			}
		}
		return true;
	}

	public List<TrayShelf> GetCookingShelves()
	{
		return trayShelf;
	}

	public int GetEmptyCookingSlotCount()
	{
		int num = 0;
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].ContainedTrayID == -1 || trayShelf[i].ContainedTray.IsEmpty())
			{
				num++;
			}
		}
		return num;
	}

	public new void Pack()
	{
		if (CanPack())
		{
			SingletonBehaviour<BakerManager>.Instance.UnregisterOven(this);
			SingletonBehaviour<SpawnManager>.Instance.PackFurniture(this);
		}
	}

	public override bool CanPack()
	{
		if (reservedForEmployeeID != -1)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("oven_reserved_staff_error", base.transform);
			return false;
		}
		if (!AllTraysEmpty())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_oven_not_empty", base.transform);
			return false;
		}
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (!storageTrayShelf[i].IsTrayEmpty())
			{
				SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_oven_not_empty", base.transform);
				return false;
			}
		}
		return true;
	}
}
