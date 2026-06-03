using System;
using DG.Tweening;
using UnityEngine;

public class PlayerLook : SingletonBehaviour<PlayerLook>
{
	[SerializeField]
	private Camera mainCam;

	[SerializeField]
	private string mouseXInputName;

	[SerializeField]
	private string mouseYInputName;

	[SerializeField]
	private float mouseSensitivity;

	[SerializeField]
	private Transform cameraParent;

	private bool rotationLocked;

	private const float DEFAULT_SENS_MULTIPLIER = 0.05f;

	private const float LOCK_ANIMATION_DURATION = 0.5f;

	private const float LOCK_ANIMATION_MOVEMENT_SPEED = 2.2f;

	private const float LOCK_ANIMATION_ROTATION_SPEED = 48f;

	public const int TUTORIAL_ID = 1;

	private float persistentInputX;

	private float persistentInputY;

	private Vector3 initialPosition;

	private Vector3 rotationBeforeLock;

	private float xAxisClamp;

	public static string ROTATION_MULTIPLIER_KEY = "ROTATION_MULTIPLIER_KEY";

	private float rotationMultiplier = 1f;

	private bool shouldShowCursor;

	public Transform CameraParent => cameraParent;

	public Camera MainCamera => mainCam;

	public bool RotationLocked
	{
		get
		{
			return rotationLocked;
		}
		set
		{
			rotationLocked = value;
			EventManager.NotifyEvent(GameEvents.PLAYER_ROTATION_SET);
		}
	}

	private new void Awake()
	{
		base.Awake();
		initialPosition = base.transform.localPosition;
		xAxisClamp = 0f;
		float fieldOfView = Camera.HorizontalToVerticalFieldOfView(SingletonWindow<SettingsWindow>.Instance.FieldOfView, mainCam.aspect);
		mainCam.fieldOfView = fieldOfView;
		OnProductDrawDistanceChanged(SingletonWindow<SettingsWindow>.Instance.ProductDrawDistance);
		rotationMultiplier = SingletonWindow<SettingsWindow>.Instance.RotationMultiplier;
		EventManager.AddListener<float>(UIEvents.ROTATION_MULTIPLIER_CHANGED, OnRotationMultiplierChanged);
		EventManager.AddListener<float>(UIEvents.FIELD_OF_VIEW_CHANGED, OnFieldOfViewChanged);
		EventManager.AddListener<float>(UIEvents.PRODUCT_DRAW_DISTANCE_CHANGED, OnProductDrawDistanceChanged);
		EventManager.AddListener<LastInputDeviceType>(UIEvents.INPUT_DEVICE_CHANGED, delegate
		{
			RefreshCursor();
		});
	}

	private void OnFieldOfViewChanged(float newFieldOfView)
	{
		mainCam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(newFieldOfView, mainCam.aspect);
	}

	private void OnProductDrawDistanceChanged(float newProductDrawDistance)
	{
		int num = LayerMask.NameToLayer("product");
		int num2 = LayerMask.NameToLayer("TransparentFX");
		float[] array = mainCam.layerCullDistances;
		if (array == null || array.Length != 32)
		{
			array = new float[32];
		}
		array[num] = newProductDrawDistance;
		array[num2] = Mathf.Min(newProductDrawDistance / 2f, 20f);
		mainCam.layerCullDistances = array;
		mainCam.layerCullSpherical = true;
	}

	public void Initialize()
	{
		if (!SingletonWindow<RefundWindow>.Instance.IsOpen())
		{
			LockCursor(state: true);
		}
	}

	public float GetFov()
	{
		return mainCam.fieldOfView;
	}

	private void OnRotationMultiplierChanged(float newMultiplier)
	{
		rotationMultiplier = newMultiplier;
	}

	public void LockCursor(bool state)
	{
		SingletonBehaviour<UIManager>.Instance.EnableDot(state);
		shouldShowCursor = !state;
		RefreshCursor();
		Cursor.lockState = (state ? CursorLockMode.Locked : CursorLockMode.None);
		RotationLocked = !state;
	}

	private void RefreshCursor()
	{
		Cursor.visible = shouldShowCursor && !SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad;
	}

	public void UnlockCamera()
	{
		LockCursor(state: true);
		base.transform.DOKill();
		base.transform.DOLocalMove(initialPosition, 2.2f).SetSpeedBased().SetEase(Ease.Linear);
		base.transform.DOLocalRotate(rotationBeforeLock, 48f).SetSpeedBased().SetEase(Ease.Linear);
		rotationBeforeLock = Vector3.zero;
		RotationLocked = false;
	}

	public void UnlockCamera(float duration, Action onUnlockedAction)
	{
		LockCursor(state: true);
		base.transform.DOKill();
		base.transform.DOLocalMove(initialPosition, duration).SetEase(Ease.Linear).OnComplete(delegate
		{
			onUnlockedAction?.Invoke();
		});
		base.transform.DOLocalRotate(rotationBeforeLock, duration).SetEase(Ease.Linear);
		rotationBeforeLock = Vector3.zero;
		RotationLocked = false;
	}

	public void LockToTransform(Transform transformToLock, Action onComplete = null)
	{
		rotationBeforeLock = base.transform.localEulerAngles;
		LockCursor(state: false);
		base.transform.DOKill();
		base.transform.DOMove(transformToLock.position, 2.2f).SetSpeedBased().SetEase(Ease.Linear)
			.OnComplete(delegate
			{
				onComplete?.Invoke();
			});
		base.transform.DORotate(transformToLock.eulerAngles, 48f).SetSpeedBased().SetEase(Ease.Linear);
	}

	public void LockToTransform(Transform transformToLock, float duration, Action onComplete = null)
	{
		rotationBeforeLock = base.transform.localEulerAngles;
		LockCursor(state: false);
		base.transform.DOKill();
		base.transform.DOMove(transformToLock.position, duration).SetEase(Ease.Linear).OnComplete(delegate
		{
			onComplete?.Invoke();
		});
		base.transform.DORotate(transformToLock.eulerAngles, duration).SetEase(Ease.Linear);
	}

	public void UpdateClamp(float rotationX)
	{
		xAxisClamp = 0f - CurrentXRotation();
	}

	public float CurrentXRotation()
	{
		float x = base.transform.localEulerAngles.x;
		x %= 360f;
		if (x > 180f)
		{
			x -= 360f;
		}
		if (x < -180f)
		{
			x += 360f;
		}
		return x;
	}

	private void Update()
	{
		CameraRotation();
	}

	private void CameraRotation()
	{
		if (!rotationLocked)
		{
			float b = SingletonBehaviour<InputManager>.Instance.HorizontalRotationInput * 0.05f * rotationMultiplier;
			int num = ((!SingletonWindow<SettingsWindow>.Instance.InvertY) ? 1 : (-1));
			float b2 = SingletonBehaviour<InputManager>.Instance.VerticalRotationInput * (float)num * 0.05f * rotationMultiplier;
			persistentInputY = Mathf.Lerp(persistentInputY, b2, 40f * Time.deltaTime);
			xAxisClamp += persistentInputY;
			if (xAxisClamp > 90f)
			{
				xAxisClamp = 90f;
				b2 = 0f;
				ClampXAxisRotationToValue(270f);
			}
			else if (xAxisClamp < -90f)
			{
				xAxisClamp = -90f;
				b2 = 0f;
				ClampXAxisRotationToValue(90f);
			}
			if (SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
			{
				base.transform.Rotate(Vector3.left * persistentInputY);
				persistentInputX = Mathf.Lerp(persistentInputX, b, 40f * Time.deltaTime);
				cameraParent.Rotate(Vector3.up * persistentInputX);
			}
			else
			{
				base.transform.Rotate(Vector3.left * persistentInputY);
				persistentInputX = Mathf.Lerp(persistentInputX, b, 40f * Time.deltaTime);
				cameraParent.Rotate(Vector3.up * persistentInputX);
			}
		}
	}

	private void ClampXAxisRotationToValue(float value)
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.x = value;
		base.transform.eulerAngles = eulerAngles;
	}
}
