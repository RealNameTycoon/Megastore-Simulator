using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HotKeyManager : SingletonBehaviour<HotKeyManager>
{
	[SerializeField]
	private List<RectTransform> hotkeySlot;

	[SerializeField]
	private List<Image> selectedHotkeyOutlines;

	[SerializeField]
	private List<Image> deselectedHotkeyOutlines;

	[SerializeField]
	private List<HotkeyClickable> hotkeyClickables;

	[SerializeField]
	private List<UIEffect> hotkeyEffects;

	[SerializeField]
	private CanvasGroup hotkeyCanvasGroup;

	private int selectedHotkeyIndex = -1;

	private const float SELECTED_HOTKEY_SIZE = 95f;

	private const float DESELECTED_HOTKEY_SIZE = 85f;

	private bool hotkeysEnabled = true;

	public int SelectedHotkeyIndex => selectedHotkeyIndex;

	private void Start()
	{
		EventManager.AddListener(GameEvents.PLAYER_ROTATION_SET, RefreshEnablity);
		EventManager.AddListener(GameEvents.PLAYER_MOVEMENT_SET, RefreshEnablity);
		EventManager.AddListener<VehichleInteractable>(GameEvents.VEHICLE_TAKEN, delegate
		{
			RefreshEnablity();
		});
		EventManager.AddListener(GameEvents.VEHICLE_RELEASED, RefreshEnablity);
		EventManager.AddListener<int>(GameEvents.PALLET_TAKEN, delegate
		{
			RefreshEnablity();
		});
		EventManager.AddListener(GameEvents.BOX_THROWN, RefreshEnablity);
		EventManager.AddListener(GameEvents.PALLET_THROWN, RefreshEnablity);
		EventManager.AddListener<int>(GameEvents.PALLET_TRASHED, delegate
		{
			RefreshEnablity();
		});
		EventManager.AddListener(PlaceableEvents.PLACEMENT_STARTED, RefreshEnablity);
		EventManager.AddListener(PlaceableEvents.OBJECT_PLACEMENT_STARTED, RefreshEnablity);
		EventManager.AddListener(PlaceableEvents.PLACEMENT_FINISHED, RefreshEnablity);
		EventManager.AddListener(GameEvents.PALLET_STACKED_BOX, RefreshEnablity);
		EventManager.AddListener(GameEvents.STORAGE_UNIT_STACKED_BOX, RefreshEnablity);
		EventManager.AddListener(GameEvents.PICKED_BOX_TRIGGERED_TRASH, RefreshEnablity);
	}

	public void RefreshEnablity()
	{
		bool flag = !SingletonBehaviour<PlayerLook>.Instance.RotationLocked && !SingletonBehaviour<PlayerMove>.Instance.MovementLocked && !SingletonBehaviour<PlacementManager>.Instance.PlacingObject && !SingletonBehaviour<BoxManager>.Instance.IsBoxPicked && !SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle && !SingletonBehaviour<TrayManager>.Instance.IsPicked && !SingletonBehaviour<PalletManager>.Instance.IsPalletPicked;
		if (flag != hotkeysEnabled)
		{
			hotkeysEnabled = flag;
			RepaintHotkeys();
		}
	}

	private void RepaintHotkeys()
	{
		hotkeyCanvasGroup.DOKill();
		hotkeyCanvasGroup.DOFade(hotkeysEnabled ? 1f : 0.6f, 0.2f).SetEase(Ease.OutBack);
		for (int i = 0; i < hotkeySlot.Count; i++)
		{
			hotkeyEffects[i].enabled = hotkeysEnabled;
		}
	}

	public void RepaintButtonsForHover(Interactable interactable)
	{
		if (selectedHotkeyIndex != -1)
		{
			hotkeyClickables[selectedHotkeyIndex].RepaintButtonsForInteractable(interactable);
		}
	}

	public void RepaintButtonsForEndHover()
	{
		if (selectedHotkeyIndex != -1)
		{
			hotkeyClickables[selectedHotkeyIndex].RepaintButtonsForEndHover();
		}
	}

	public void SelectButton(int index)
	{
		if (!hotkeysEnabled)
		{
			return;
		}
		if (index == selectedHotkeyIndex)
		{
			DeselectButton(index);
			selectedHotkeyIndex = -1;
			return;
		}
		if (selectedHotkeyIndex != -1)
		{
			DeselectButton(selectedHotkeyIndex);
		}
		selectedHotkeyOutlines[index].enabled = true;
		deselectedHotkeyOutlines[index].enabled = false;
		selectedHotkeyIndex = index;
		hotkeyClickables[index].PickUp();
		hotkeySlot[index].DOKill();
		hotkeySlot[index].DOSizeDelta(new Vector2(95f, 95f), 0.2f).SetEase(Ease.OutBack);
	}

	private void DeselectButton(int index)
	{
		selectedHotkeyOutlines[index].enabled = false;
		deselectedHotkeyOutlines[index].enabled = true;
		hotkeyClickables[index].Reset();
		hotkeyClickables[index].PutDown();
		hotkeySlot[index].DOKill();
		hotkeySlot[index].DOSizeDelta(new Vector2(85f, 85f), 0.2f).SetEase(Ease.OutBack);
	}

	private void Update()
	{
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.Hotkey1ActionRef) && hotkeyClickables.Count > 0 && hotkeyClickables[0] != null)
		{
			SelectButton(0);
		}
		else if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.Hotkey2ActionRef) && hotkeyClickables.Count > 1 && hotkeyClickables[1] != null)
		{
			SelectButton(1);
		}
		else if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.Hotkey3ActionRef) && hotkeyClickables.Count > 2 && hotkeyClickables[2] != null)
		{
			SelectButton(2);
		}
		else if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.Hotkey4ActionRef) && hotkeyClickables.Count > 3 && hotkeyClickables[3] != null)
		{
			SelectButton(3);
		}
	}
}
