using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMove : SingletonBehaviour<PlayerMove>
{
	[SerializeField]
	private CapsuleCollider playerCapsule;

	[SerializeField]
	private BoxCollider cameraCoverCollider;

	[SerializeField]
	private string horizontalInputName;

	[SerializeField]
	private string verticalInputName;

	[SerializeField]
	private float walkSpeed;

	[SerializeField]
	private float runSpeed;

	[SerializeField]
	private float extraFallGravity;

	[SerializeField]
	private float runBuildUpSpeed;

	[SerializeField]
	private KeyCode runKey;

	private float movementSpeed;

	[SerializeField]
	private float slopeForce;

	[SerializeField]
	private float slopeForceRayLength;

	private CharacterController charController;

	[SerializeField]
	private AnimationCurve jumpFallOff;

	[SerializeField]
	private float jumpMultiplier = 8f;

	[SerializeField]
	private float jumpForce = 15f;

	[SerializeField]
	private KeyCode jumpKey;

	[SerializeField]
	private Joystick joystick;

	[SerializeField]
	private Transform charParent;

	[SerializeField]
	private Transform boxParent;

	[SerializeField]
	private Transform handTruckParent;

	[SerializeField]
	private Transform platformCartParent;

	[SerializeField]
	private Rigidbody rigidBody;

	[SerializeField]
	private float forceMultiplier = 6f;

	[SerializeField]
	private GameObject stepRayUpper;

	[SerializeField]
	private GameObject stepRayLower;

	[SerializeField]
	private Transform playerBottom;

	[SerializeField]
	private float stepSmooth = 2f;

	[SerializeField]
	private Collider boxCollider;

	[SerializeField]
	private SerializedDictionary<BoxType, BoxCollider> boxColliders;

	[SerializeField]
	private BoxCollider handTruckCollider;

	[SerializeField]
	private BoxCollider handTruckColliderTarget;

	[SerializeField]
	private BoxCollider platformCartCollider;

	[SerializeField]
	private BoxCollider palletTruckCollider;

	[SerializeField]
	private Transform cameraParent;

	[SerializeField]
	private Transform palletJackParent;

	[SerializeField]
	private Transform palletParent;

	private float stepHeight = 0.4f;

	public LayerMask groundLayer;

	public LayerMask playerCollidableLayers;

	[SerializeField]
	private float fallGravityMultiplier = 2.5f;

	[SerializeField]
	private float maxFallSpeed = -35f;

	private bool isJumping;

	private bool movementLocked;

	public const float FOOTSTEP_FREQUENCY = 0.5f;

	public const float INITIAL_FOOTSTEP_DELAY = 0.25f;

	private const float CHAR_RADIUS_DEFAULT = 0.3f;

	private const float CHAR_RADIUS_BOX = 0.57f;

	private static Vector3 CHAR_CENTER_DEFAULT = new Vector3(0f, 0f, 0.3f);

	private static Vector3 CHAR_CENTER_BOX = new Vector3(0f, 0f, 0.57f);

	private const float GROUND_DRAG = 20f;

	private const float AIR_DRAG = 2f;

	private static float COLLISION_AVOIDANCE_DISTANCE = 0.3f;

	private float footstepTimer = 0.25f;

	private Tweener rotationTweener;

	private Tweener boxTweenerRotation;

	private Tweener boxTweenerMovement;

	private Tweener handTruckRotation;

	private Tweener platformCartRotation;

	public const int TUTORIAL_ID = 2;

	private bool isWalking;

	private bool grounded;

	private Vector3 collisionNormal;

	private bool isTouchingWall;

	private float groundCheckDistance = 0.4f;

	private float initialHandTruckColliderCenterZ;

	private float initialHandTruckColliderSizeZ;

	private bool wasOnStep;

	private bool jumpedNotLanded;

	private const int FRAME_COUNT_TO_START_FALL = 3;

	private int wasOnStepCount;

	public Vector3 CameraParentEuler => cameraParent.eulerAngles;

	public Transform PlayerBottom => playerBottom;

	public Transform PalletJackParent => palletJackParent;

	public Transform PalletParent => palletParent;

	public Bounds Bounds => playerCapsule.bounds;

	public Vector3 PlacementStartPosition => base.transform.position;

	public bool MovementLocked
	{
		get
		{
			return movementLocked;
		}
		set
		{
			movementLocked = value;
			playerCapsule.enabled = !value;
			cameraCoverCollider.enabled = !value;
			rigidBody.useGravity = !value;
			rigidBody.interpolation = ((!value) ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
			rigidBody.isKinematic = value;
			if (boxTweenerMovement != null && boxTweenerMovement.IsActive())
			{
				KillTweensAndReset();
			}
			EventManager.NotifyEvent(GameEvents.PLAYER_MOVEMENT_SET);
		}
	}

	private void KillTweensAndReset()
	{
		rotationTweener.Kill();
		boxTweenerMovement.Kill();
		boxTweenerRotation.Kill();
		handTruckRotation.Kill();
		platformCartRotation.Kill();
		charParent.DOKill();
		boxParent.DOKill();
		charParent.DOLocalRotate(Vector3.zero, 0.2f);
		boxParent.DOLocalRotate(Vector3.zero, 0.2f);
		boxParent.DOLocalMove(Vector3.zero, 0.2f);
		handTruckParent.DOLocalRotate(Vector3.zero, 0.2f);
		platformCartParent.DOLocalRotate(Vector3.zero, 0.2f);
	}

	private new void Awake()
	{
		base.Awake();
		charController = GetComponent<CharacterController>();
		stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepRayLower.transform.position.y + stepHeight, stepRayUpper.transform.position.z);
		rigidBody.freezeRotation = true;
		initialHandTruckColliderCenterZ = handTruckCollider.center.z;
		initialHandTruckColliderSizeZ = handTruckCollider.size.z;
	}

	public void EnableBoxCollider(BoxType type)
	{
		boxColliders[type].enabled = true;
	}

	public void EnableHandTruckCollider()
	{
		handTruckCollider.enabled = true;
	}

	public void EnablePlatformCartCollider()
	{
		platformCartCollider.enabled = true;
	}

	public void EnablPalletTruckCollider()
	{
		palletTruckCollider.enabled = true;
	}

	public void UpdateRadius(bool objectPicked)
	{
		handTruckCollider.enabled = false;
		palletTruckCollider.enabled = false;
		platformCartCollider.enabled = false;
		foreach (KeyValuePair<BoxType, BoxCollider> boxCollider in boxColliders)
		{
			boxCollider.Value.enabled = false;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (Vector3.Dot(contactPoint.normal, Vector3.up) < 0.5f)
			{
				isTouchingWall = true;
				collisionNormal = contactPoint.normal;
				return;
			}
		}
		isTouchingWall = false;
	}

	private void OnCollisionExit(Collision collision)
	{
		isTouchingWall = false;
	}

	private void Update()
	{
		JumpInput();
	}

	private void FixedUpdate()
	{
		PlayerMovement();
	}

	private void PlayerMovement()
	{
		float num = SingletonBehaviour<InputManager>.Instance.MovementInput.Item1;
		float num2 = SingletonBehaviour<InputManager>.Instance.MovementInput.Item2;
		if (movementLocked || !SingletonBehaviour<UIManager>.Instance.AllWindowsClosed())
		{
			num = 0f;
			num2 = 0f;
		}
		Vector3 vector = cameraParent.forward * num2;
		Vector3 vector2 = cameraParent.right * num;
		Vector3 vector3 = Vector3.ClampMagnitude(vector + vector2, 1f) * 20f * forceMultiplier * movementSpeed;
		if (isTouchingWall && Vector3.Dot(vector3, collisionNormal) < 0f)
		{
			vector3 = Vector3.ProjectOnPlane(vector3, collisionNormal);
		}
		SetMovementSpeed();
		if (new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z).magnitude <= movementSpeed)
		{
			rigidBody.AddForce(vector3, ForceMode.Force);
		}
		bool flag = IsOnSurface();
		if (num2 != 0f || num != 0f)
		{
			footstepTimer += Time.fixedDeltaTime;
			if (!isWalking && SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle)
			{
				SingletonBehaviour<VehicleManager>.Instance.TurnVehicleWheels();
			}
			if (footstepTimer >= 0.5f)
			{
				if (boxTweenerMovement == null || !boxTweenerMovement.IsActive())
				{
					boxParent.DOKill();
					charParent.DOKill();
					rotationTweener = charParent.DOLocalRotate(Vector3.forward * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? 0.3f : 0f), 0.5f).SetEase(Ease.Linear).OnComplete(delegate
					{
						rotationTweener = charParent.DOLocalRotate(Vector3.forward * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? (-0.3f) : 0f), 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
						rotationTweener.Play();
					});
					boxTweenerMovement = boxParent.DOLocalMove(SingletonWindow<SettingsWindow>.Instance.HeadBob ? (Vector3.up * 0.025f + Vector3.forward * 0.0125f) : Vector3.zero, 0.25f).SetEase(Ease.Linear).OnComplete(delegate
					{
						boxTweenerMovement = boxParent.DOLocalMove(SingletonWindow<SettingsWindow>.Instance.HeadBob ? (Vector3.up * -0.025f - Vector3.forward * 0.0125f) : Vector3.zero, 0.25f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
						boxTweenerMovement.Play();
					});
					boxTweenerRotation = boxParent.DOLocalRotate(Vector3.forward * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? 2f : 0f), 0.5f).SetEase(Ease.Linear).OnComplete(delegate
					{
						boxTweenerRotation = boxParent.DOLocalRotate(Vector3.forward * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? (-2f) : 0f), 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
						boxTweenerRotation.Play();
					});
					handTruckRotation = handTruckParent.DOLocalRotate(Vector3.right * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? 1.5f : 0f), 0.5f).SetEase(Ease.Linear).OnComplete(delegate
					{
						handTruckRotation = handTruckParent.DOLocalRotate(Vector3.right * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? (-1.5f) : 0f), 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
						handTruckRotation.Play();
					});
					platformCartRotation = platformCartParent.DOLocalRotate(new Vector3(1f, 0.5f, 0f) * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? 0.2f : 0f), 0.125f).SetEase(Ease.Linear).OnComplete(delegate
					{
						platformCartRotation = platformCartParent.DOLocalRotate(new Vector3(1f, 0.5f, 0f) * (SingletonWindow<SettingsWindow>.Instance.HeadBob ? (-0.2f) : 0f), 0.125f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
						platformCartRotation.Play();
					});
				}
				if (flag)
				{
					SingletonBehaviour<AudioManager>.Instance.PlayAudio(AudioManager.AudioTypes.FOOT_STEPS);
				}
				footstepTimer = 0f;
			}
		}
		else
		{
			if (boxTweenerMovement != null && boxTweenerMovement.IsActive())
			{
				KillTweensAndReset();
			}
			if (SingletonBehaviour<VehicleManager>.Instance.IsAnimating)
			{
				SingletonBehaviour<VehicleManager>.Instance.StopVehicleWheels();
			}
			footstepTimer = 0.25f;
		}
		bool flag2 = false;
		if (num2 != 0f || num != 0f)
		{
			flag2 = StepClimb(vector + vector2);
			if (flag2)
			{
				wasOnStep = true;
			}
		}
		if (flag || flag2 || isJumping)
		{
			return;
		}
		if (!wasOnStep)
		{
			float num3 = Physics.gravity.y * fallGravityMultiplier;
			Vector3 linearVelocity = rigidBody.linearVelocity;
			linearVelocity.y += num3 * Time.fixedDeltaTime;
			if (linearVelocity.y < maxFallSpeed)
			{
				linearVelocity.y = maxFallSpeed;
			}
			rigidBody.linearVelocity = linearVelocity;
		}
		else
		{
			wasOnStepCount++;
			if (wasOnStepCount > 3)
			{
				wasOnStep = false;
				wasOnStepCount = 0;
			}
		}
	}

	public void BrakeSpeed(Vector3 targetEuler)
	{
		rigidBody.linearVelocity = new Vector3(0f, rigidBody.linearVelocity.y, 0f);
		rigidBody.rotation = Quaternion.Euler(targetEuler);
		rigidBody.angularVelocity = Vector3.zero;
	}

	private void ClampSpeed()
	{
		Vector3 vector = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
		if (vector.magnitude > movementSpeed)
		{
			Vector3 vector2 = vector.normalized * movementSpeed;
			rigidBody.linearVelocity = new Vector3(vector2.x, rigidBody.linearVelocity.y, vector2.z);
		}
	}

	private void SetMovementSpeed()
	{
		float num = runSpeed * SingletonBehaviour<VehicleManager>.Instance.GetSpeedMultiplier();
		float num2 = walkSpeed * SingletonBehaviour<VehicleManager>.Instance.GetSpeedMultiplier();
		if (SingletonBehaviour<InputManager>.Instance.IsPressingRun && (SingletonBehaviour<VehicleManager>.Instance.TakenVehicle == null || SingletonBehaviour<VehicleManager>.Instance.TakenVehicle.CanSprint))
		{
			if (movementSpeed > num)
			{
				movementSpeed = num;
			}
			movementSpeed = Mathf.Lerp(movementSpeed, num, Time.fixedDeltaTime * runBuildUpSpeed);
		}
		else
		{
			if (movementSpeed > num2)
			{
				movementSpeed = num2;
			}
			movementSpeed = Mathf.Lerp(movementSpeed, num2, Time.fixedDeltaTime * runBuildUpSpeed);
		}
	}

	private bool OnSlope()
	{
		if (isJumping)
		{
			return false;
		}
		if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, playerCapsule.height / 2f * slopeForceRayLength) && hitInfo.normal != Vector3.up)
		{
			return true;
		}
		return false;
	}

	private void JumpInput()
	{
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.JumpActionRef) && IsOnSurface() && !isJumping && !SingletonBehaviour<VehicleManager>.Instance.IsOnVehicle && !SingletonBehaviour<UIStackManager>.Instance.IsAnyWindowOpen() && !SingletonBehaviour<UIStackManager>.Instance.WindowClosedThisFrame)
		{
			StartCoroutine(JumpEvent());
		}
	}

	private IEnumerator JumpEvent()
	{
		isJumping = true;
		float elapsed = 0f;
		float jumpDuration = 0.2f;
		Vector3 linearVelocity = rigidBody.linearVelocity;
		linearVelocity.y = 0f;
		rigidBody.linearVelocity = linearVelocity;
		rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		while (elapsed < jumpDuration)
		{
			float time = elapsed / jumpDuration;
			float num = jumpFallOff.Evaluate(time);
			rigidBody.AddForce(Vector3.up * num * jumpMultiplier, ForceMode.Acceleration);
			elapsed += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		jumpedNotLanded = true;
		isJumping = false;
	}

	private bool IsOnSurface()
	{
		return Physics.Raycast(stepRayUpper.transform.position, Vector3.down, groundCheckDistance + 0.2f, playerCollidableLayers);
	}

	private bool StepClimb(Vector3 moveDirection)
	{
		if (isJumping)
		{
			return false;
		}
		if (Physics.Raycast(stepRayLower.transform.position, moveDirection, out var _, playerCapsule.radius + 0.3f, groundLayer) && !Physics.Raycast(stepRayUpper.transform.position, moveDirection, out var _, playerCapsule.radius + 0.4f, groundLayer))
		{
			rigidBody.position -= new Vector3(0f, (0f - stepSmooth) * Time.fixedDeltaTime, 0f);
			return true;
		}
		return false;
	}
}
