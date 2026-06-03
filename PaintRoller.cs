using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PaintRoller : HotkeyClickable
{
	[SerializeField]
	private Transform usingPosition;

	[SerializeField]
	private MeshRenderer rollerMeshRenderer;

	private bool isUsing;

	private Coroutine endUseCoroutine;

	private WaitForSeconds waitToEndUse = new WaitForSeconds(ButtonsWindow.CLICK_HOLD_CALLBACK_FREQUENCY + 0.1f);

	private FloorClickable currentFloorHover;

	private DecorationUI.DecorationType currentDecorationType;

	private float rollerYRotation;

	private float rollerZRotation;

	public static PaintRoller Instance { get; protected set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		rollerYRotation = rollerMeshRenderer.transform.localEulerAngles.y;
		rollerZRotation = rollerMeshRenderer.transform.localEulerAngles.z;
		base.transform.position = putDownPosition.position;
		base.transform.eulerAngles = putDownPosition.eulerAngles;
		EventManager.AddListener<DecorationUI.DecorationType, int>(DecorationEvents.DECORATION_USED, OnDecorationUsed);
		OnDecorationUsed(SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationType, SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex);
		base.gameObject.SetActive(value: false);
		EventManager.AddListener(DecorationEvents.FLOOR_HOVER_ENDED, OnFloorHoverEnded);
		EventManager.AddListener<FloorClickable>(DecorationEvents.FLOOR_HOVER_STARTED, OnFloorHoverStarted);
	}

	private void OnFloorHoverStarted(FloorClickable floorClickable)
	{
		currentFloorHover = floorClickable;
		UpdateMenu();
	}

	private void OnFloorHoverEnded()
	{
		SingletonBehaviour<ButtonsWindow>.Instance.StopLeftClickHold();
		StopUse();
		currentFloorHover = null;
		UpdateMenu();
	}

	public override LayerMask GetInteractableLayers()
	{
		if (currentDecorationType == DecorationUI.DecorationType.FLOOR)
		{
			return (1 << RayShooter.FLOOR_LAYER) | (1 << RayShooter.SERVICE_ROOM_FLOOR_LAYER);
		}
		if (currentDecorationType == DecorationUI.DecorationType.WALL)
		{
			return 1 << RayShooter.WALL_LAYER;
		}
		return 0;
	}

	private void OnDecorationUsed(DecorationUI.DecorationType type, int id)
	{
		currentDecorationType = type;
		rollerMeshRenderer.sharedMaterial = SingletonBehaviour<DecorationManager>.Instance.GetMaterial(type, id);
	}

	public override void OnPickedUp()
	{
		UpdateMenu();
	}

	public void UpdateMenu()
	{
		if (base.IsPicked && currentFloorHover != null && currentFloorHover.CurrentDecorationIndex != SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.Mouse0,
				("hold_use", delegate
				{
					Use();
				})
			} }, base.transform);
		}
		else if (SingletonBehaviour<ButtonsWindow>.Instance.IsOpenedBy(base.transform))
		{
			SingletonBehaviour<ButtonsWindow>.Instance.Close();
		}
	}

	public override void RepaintButtonsForEndHover()
	{
		UpdateMenu();
	}

	private void Use()
	{
		if (!isUsing)
		{
			isUsing = true;
			base.transform.DOKill();
			rollerMeshRenderer.transform.DOKill();
			rollerMeshRenderer.transform.localEulerAngles = new Vector3(0f, rollerYRotation, rollerZRotation);
			base.transform.DOLocalMove(usingPosition.localPosition, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
			base.transform.DOLocalRotate(usingPosition.localEulerAngles, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine)
				.OnComplete(delegate
				{
					UpdateMenu();
				});
			rollerMeshRenderer.transform.DOLocalRotate(new Vector3(360f, rollerYRotation, rollerZRotation), 0.25f, RotateMode.FastBeyond360).SetLoops(4, LoopType.Incremental).SetEase(Ease.Linear);
		}
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
		}
		endUseCoroutine = StartCoroutine(EndUseCoroutine());
	}

	private IEnumerator EndUseCoroutine()
	{
		yield return waitToEndUse;
		StopUse();
	}

	private void StopUse(bool immediate = false)
	{
		if (isUsing)
		{
			isUsing = false;
			MoveToPlayerHolding();
		}
	}

	private void MoveToPlayerHolding()
	{
		base.transform.DOKill();
		rollerMeshRenderer.transform.DOKill();
		rollerMeshRenderer.transform.localEulerAngles = new Vector3(0f, rollerYRotation, rollerZRotation);
		base.transform.DOLocalMove(pickUpPosition.localPosition, GetPickUpSpeed()).SetSpeedBased(isSpeedBased: true);
		base.transform.DOLocalRotate(pickUpPosition.localEulerAngles, GetPickUpSpeedRotation()).SetSpeedBased(isSpeedBased: true).OnComplete(delegate
		{
		});
	}

	public override void OnPutDown()
	{
		base.OnPutDown();
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
			endUseCoroutine = null;
		}
	}

	public override void Reset()
	{
		base.Reset();
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
			endUseCoroutine = null;
		}
		StopUse(immediate: true);
	}

	public override void PickUp()
	{
		if (endUseCoroutine != null)
		{
			StopCoroutine(endUseCoroutine);
			endUseCoroutine = null;
		}
		isUsing = false;
		base.PickUp();
	}
}
