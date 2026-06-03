using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CarAI : MonoBehaviour
{
	public Rigidbody rigidBody;

	[Header("Car Wheels (Wheel Collider)")]
	public WheelCollider frontLeft;

	public WheelCollider frontRight;

	public WheelCollider backLeft;

	public WheelCollider backRight;

	[Header("Car Wheels (Transform)")]
	public Transform wheelFL;

	public Transform wheelFR;

	public Transform wheelBL;

	public Transform wheelBR;

	[Header("Car Front (Transform)")]
	public Transform carFront;

	[Header("General Parameters")]
	public int MaxSteeringAngle = 45;

	public int MaxRPM = 150;

	[Header("Debug")]
	public bool ShowGizmos;

	public bool Debugger;

	[Header("Destination Parameters")]
	public bool Patrol = true;

	public Transform CustomDestination;

	[HideInInspector]
	public bool move;

	[SerializeField]
	private AudioSource carSource;

	[SerializeField]
	private AudioClip carEngineSound;

	[SerializeField]
	private List<AudioClip> carHorns;

	[SerializeField]
	private AudioClip hornLong;

	[SerializeField]
	private AudioClip engineStop;

	[SerializeField]
	private bool isFastCar;

	public float steeringSmoothing = 5f;

	private float currentSteeringAngle;

	private WaitForSeconds secondWaiter = new WaitForSeconds(2f);

	private Vector3 PostionToFollow = Vector3.zero;

	private int currentWayPoint;

	private float AIFOV = 60f;

	private bool allowMovement;

	private int NavMeshLayerBite;

	private List<Vector3> waypoints = new List<Vector3>();

	private float LocalMaxSpeed;

	private int Fails;

	private float MovementTorque = 1f;

	private Action stopAction;

	private float carVolume;

	public float reverseSpeedMultiplier = 0.6f;

	private bool isReversing;

	public bool IsMoving => move;

	public bool IsReversing => isReversing;

	private void Awake()
	{
		carVolume = carSource.volume;
		rigidBody.centerOfMass = Vector3.zero;
		EventManager.AddListener(GameEvents.SOUND_MUTED, UpdateSound);
		EventManager.AddListener(GameEvents.SOUND_UNMUTED, UpdateSound);
		EventManager.AddListener(GameEvents.GAME_PAUSED, MuteSound);
		EventManager.AddListener(GameEvents.GAME_RESUMED, UpdateSound);
		EventManager.AddListener(UIEvents.AUDIO_MULTIPLIER_CHANGED, OnAudioMultiplierChanged);
		carSource.volume = carVolume * SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier;
		UpdateSound();
	}

	private void Start()
	{
		ConfigureWheel(frontLeft);
		ConfigureWheel(frontRight);
		ConfigureWheel(backLeft);
		ConfigureWheel(backRight);
	}

	private void ConfigureWheel(WheelCollider wheel)
	{
		wheel.ConfigureVehicleSubsteps(5f, 12, 15);
	}

	public void Move(List<Vector3> wps, Action onStopAction = null, bool reverse = false)
	{
		currentWayPoint = 0;
		allowMovement = true;
		move = true;
		isReversing = reverse;
		carSource.DOKill();
		carSource.volume = carVolume * SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier;
		carSource.clip = carEngineSound;
		carSource.loop = true;
		carSource.Play();
		stopAction = onStopAction;
		waypoints = wps;
	}

	public void Stop()
	{
		move = false;
		allowMovement = false;
		ApplyBrakes();
		carSource.Stop();
	}

	private void OnAudioMultiplierChanged()
	{
		carSource.volume = carVolume * SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier;
	}

	private void MuteSound()
	{
		carSource.Pause();
	}

	private void UpdateSound()
	{
		if (SingletonBehaviour<AudioManager>.Instance.IsSoundOn)
		{
			carSource.UnPause();
		}
		else
		{
			carSource.Pause();
		}
		if (Time.timeScale == 0f)
		{
			carSource.Pause();
		}
		carSource.volume = carVolume * SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier;
	}

	private void FixedUpdate()
	{
		UpdateWheels();
		ApplySteering();
		PathProgress();
	}

	private void PathProgress()
	{
		if (currentWayPoint >= waypoints.Count && allowMovement)
		{
			allowMovement = false;
			StartCoroutine(DeaccelerationRoutine());
		}
		else if (currentWayPoint < waypoints.Count)
		{
			PostionToFollow = waypoints[currentWayPoint];
			allowMovement = true;
			Vector3 a;
			if (isReversing)
			{
				float num = Vector3.Distance(base.transform.position, carFront.position);
				a = base.transform.position - base.transform.forward * num;
			}
			else
			{
				a = carFront.position;
			}
			float num2 = Vector3.Distance(a, PostionToFollow);
			float num3 = (isReversing ? 4f : 2.5f);
			if (num2 < num3)
			{
				currentWayPoint++;
			}
			else if (HasPassedWaypoint())
			{
				currentWayPoint++;
			}
		}
		Movement();
		if (currentWayPoint > 5 && waypoints.Count > 30)
		{
			waypoints.RemoveAt(0);
			currentWayPoint--;
		}
	}

	private bool HasPassedWaypoint()
	{
		if (currentWayPoint >= waypoints.Count)
		{
			return false;
		}
		Vector3 vector = (isReversing ? base.transform.position : carFront.position);
		Vector3 vector2 = PostionToFollow - vector;
		Vector3 rhs = (isReversing ? (-base.transform.forward) : carFront.forward);
		return Vector3.Dot(vector2.normalized, rhs) < -0.3f;
	}

	private IEnumerator DeaccelerationRoutine()
	{
		yield return secondWaiter;
		carSource.clip = null;
		carSource.loop = false;
		carSource.Stop();
		if (engineStop != null)
		{
			carSource.PlayOneShot(engineStop);
		}
		carSource.DOFade(0f, 2f).OnComplete(delegate
		{
			carSource.volume = carVolume * SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier;
			if (carHorns.Count != 0)
			{
				carSource.PlayOneShot(carHorns.GetRandomElement());
			}
		});
		stopAction?.Invoke();
	}

	private void ApplyBrakes()
	{
		frontLeft.brakeTorque = 2500f;
		frontRight.brakeTorque = 2500f;
		backLeft.brakeTorque = 2500f;
		backRight.brakeTorque = 2500f;
	}

	private void UpdateWheels()
	{
		ApplyRotationAndPostion(frontLeft, wheelFL);
		ApplyRotationAndPostion(frontRight, wheelFR);
		ApplyRotationAndPostion(backLeft, wheelBL);
		ApplyRotationAndPostion(backRight, wheelBR);
	}

	private void ApplyRotationAndPostion(WheelCollider targetWheel, Transform wheel)
	{
		targetWheel.GetWorldPose(out var pos, out var quat);
		wheel.position = pos;
		wheel.rotation = quat;
	}

	private void ApplySteering()
	{
		Vector3 vector = base.transform.InverseTransformPoint(PostionToFollow);
		float b = vector.x / vector.magnitude * (float)MaxSteeringAngle;
		currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, b, Time.fixedDeltaTime * steeringSmoothing);
		float num = Mathf.Abs(currentSteeringAngle) / (float)MaxSteeringAngle;
		float num2 = MaxRPM;
		num2 = Mathf.Lerp(MaxRPM, (float)MaxRPM * 0.2f, num * num);
		if (isReversing)
		{
			num2 *= reverseSpeedMultiplier;
		}
		LocalMaxSpeed = Mathf.Lerp(LocalMaxSpeed, num2, Time.fixedDeltaTime * 2f);
		frontLeft.steerAngle = currentSteeringAngle;
		frontRight.steerAngle = currentSteeringAngle;
	}

	private void Movement()
	{
		if (move && allowMovement)
		{
			allowMovement = true;
		}
		else
		{
			allowMovement = false;
		}
		int value = (int)((frontLeft.rpm + frontRight.rpm + backLeft.rpm + backRight.rpm) / 4f);
		if (allowMovement)
		{
			frontLeft.brakeTorque = 0f;
			frontRight.brakeTorque = 0f;
			backLeft.brakeTorque = 0f;
			backRight.brakeTorque = 0f;
			float num = (isReversing ? (-1f) : 1f);
			if ((float)Mathf.Abs(value) < LocalMaxSpeed)
			{
				int num2 = (isFastCar ? 1200 : 400);
				backRight.motorTorque = (float)num2 * MovementTorque * num;
				backLeft.motorTorque = (float)num2 * MovementTorque * num;
				frontRight.motorTorque = (float)num2 * MovementTorque * num;
				frontLeft.motorTorque = (float)num2 * MovementTorque * num;
			}
			else if ((float)Mathf.Abs(value) < LocalMaxSpeed + LocalMaxSpeed * 1f / 4f)
			{
				backRight.motorTorque = 0f;
				backLeft.motorTorque = 0f;
				frontRight.motorTorque = 0f;
				frontLeft.motorTorque = 0f;
			}
			else
			{
				ApplyBrakes();
			}
		}
		else
		{
			ApplyBrakes();
		}
		float num3 = (isReversing ? 0.8f : 1f);
		carSource.pitch = Mathf.Lerp(1f, 1.5f, Mathf.Abs(rigidBody.linearVelocity.magnitude) / 7f) * num3;
	}

	private void OnDrawGizmos()
	{
		if (!ShowGizmos)
		{
			return;
		}
		for (int i = 0; i < waypoints.Count; i++)
		{
			if (i == currentWayPoint)
			{
				Gizmos.color = Color.blue;
			}
			else if (i > currentWayPoint)
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = Color.green;
			}
			Gizmos.DrawWireSphere(waypoints[i], 2f);
		}
		CalculateFOV();
		void CalculateFOV()
		{
			Gizmos.color = Color.white;
			float num = AIFOV * 2f;
			float num2 = 10f;
			float num3 = num / 2f;
			Quaternion quaternion = Quaternion.AngleAxis(0f - num3, Vector3.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(num3, Vector3.up);
			Vector3 vector = quaternion * base.transform.forward;
			Vector3 vector2 = quaternion2 * base.transform.forward;
			Gizmos.DrawRay(carFront.position, vector * num2);
			Gizmos.DrawRay(carFront.position, vector2 * num2);
		}
	}
}
