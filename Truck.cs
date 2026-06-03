using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Truck : MonoBehaviour
{
	[SerializeField]
	private BoxCollider truckOverlapTrigger;

	[SerializeField]
	private LayerMask truckOverlapLayers;

	[SerializeField]
	private Collider doorRight;

	[SerializeField]
	private Collider doorLeft;

	[SerializeField]
	private AudioSource truckBackAudioSource;

	[SerializeField]
	private AudioClip doorOpenClip;

	[SerializeField]
	private AudioClip doorCloseClip;

	[SerializeField]
	private CarAI carAI;

	[SerializeField]
	private List<Transform> spawnPositions;

	[SerializeField]
	private Transform elevator;

	[SerializeField]
	private Transform elevatorTarget;

	[SerializeField]
	private Rigidbody rigidBody;

	[SerializeField]
	private OrderManager.OrderReceivingArea receivingArea;

	[SerializeField]
	private List<Pallet> targetPallets;

	[SerializeField]
	private Transform vehicleSpawnPoint;

	[SerializeField]
	private Collider doorBlockCollider;

	public DOTweenPath truckArrivalPath;

	public DOTweenPath truckDeparturePath;

	private Vector3 closedDoorEulerAngle = Vector3.zero;

	private Vector3 openRightDoorEulerAngle = new Vector3(0f, -135f, 0f);

	private Vector3 openLeftDoorEulerAngle = new Vector3(0f, 135f, 0f);

	private const float DOOR_ANIMATION_SPEED = 135f;

	private List<Collider> collidingEntities = new List<Collider>();

	private List<Collider> collidingPickables = new List<Collider>();

	private bool isEmpty = true;

	private const string TRUCK_POSITION_KEY = "TRUCK_POSITION_KEY";

	private const string TRUCK_ROTATION_KEY = "TRUCK_ROTATION_KEY";

	private const string TRUCK_EMPTY_KEY = "TRUCK_EMPTY_KEY";

	private const string TRUCK_ACTIVE_KEY = "TRUCK_ACTIVE_KEY";

	private const string TRUCK_ARRIVED_KEY = "TRUCK_ARRIVED_KEY";

	private Vector3 truckStartPosition;

	private Vector3 truckStartRotation;

	private Vector3 elevatorLocalStartPosition;

	private Vector3 elevatorLocalStartRotation;

	private Vector3 elevatorTargetRotation = new Vector3(-152.3f, 0f, 0f);

	private float initialBackAudioVolume;

	private bool doorsAnimating;

	private bool truckActive;

	private int halfSecondsPassed;

	private int shipmentDuration;

	private WaitForSeconds waiter = new WaitForSeconds(0.5f);

	private bool arrived;

	private WaitForSeconds leaveDelay = new WaitForSeconds(20f);

	private Coroutine leaveCoroutine;

	private bool isDoorOpen;

	public Transform VehicleSpawnPoint => vehicleSpawnPoint;

	public bool IsActive => base.gameObject.activeSelf;

	public List<Transform> SpawnPositions => spawnPositions;

	public List<Pallet> TargetPallets => targetPallets;

	public int TargetBoxSlotCountPerPallet => 14;

	public bool TruckActive => truckActive;

	public bool Arrived => arrived;

	public float ShipmentProgress => (float)halfSecondsPassed / (float)shipmentDuration;

	public bool IsMoving => carAI.IsMoving;

	public bool IsDoorEnabled
	{
		get
		{
			if (doorRight.enabled)
			{
				return doorLeft.enabled;
			}
			return false;
		}
	}

	public bool IsDoorOpen => isDoorOpen;

	private void Awake()
	{
		truckStartPosition = base.transform.position;
		truckStartRotation = base.transform.localEulerAngles;
		truckActive = GenericDataSerializer.Load("TRUCK_ACTIVE_KEY" + receivingArea, defaultValue: false);
		arrived = GenericDataSerializer.Load("TRUCK_ARRIVED_KEY" + receivingArea, defaultValue: false);
		elevatorLocalStartPosition = elevator.localPosition;
		elevatorLocalStartRotation = elevator.localEulerAngles;
		initialBackAudioVolume = truckBackAudioSource.volume;
		truckBackAudioSource.volume = SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier * initialBackAudioVolume;
		GenericDataSerializer.Load("TRUCK_EMPTY_KEY", defaultValue: true);
		if (!arrived)
		{
			base.gameObject.SetActive(value: false);
		}
		EventManager.AddListener(UIEvents.AUDIO_MULTIPLIER_CHANGED, OnAudioMultiplierChanged);
	}

	private void OnAudioMultiplierChanged()
	{
		truckBackAudioSource.volume = SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier * initialBackAudioVolume;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "HandTruckCollider" || other.gameObject.name == "BoxCollider")
		{
			return;
		}
		collidingEntities.Add(other);
		if (BoxManager.IsBoxLayer(other.gameObject.layer))
		{
			collidingPickables.Add(other);
			if (isEmpty)
			{
				isEmpty = false;
				GenericDataSerializer.Save("TRUCK_EMPTY_KEY", isEmpty);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (IsEmpty())
		{
			EventManager.NotifyEvent(GameEvents.TRUCK_UNLOADED, receivingArea);
			if (other.CompareTag(AutoDoor.CustomerTag))
			{
				EventManager.NotifyEvent(GameEvents.TRUCK_UNLOADED_BY_STAFF, receivingArea);
			}
		}
		if (collidingEntities.Count == 0)
		{
			isEmpty = true;
			GenericDataSerializer.Save("TRUCK_EMPTY_KEY", isEmpty);
		}
	}

	public void Activate()
	{
		if (IsStationary())
		{
			truckActive = true;
			GenericDataSerializer.Save("TRUCK_ACTIVE_KEY" + receivingArea, truckActive);
			base.gameObject.SetActive(value: true);
			shipmentDuration = Random.Range(20, 60);
			halfSecondsPassed = 0;
			EventManager.NotifyEvent(GameEvents.TRUCK_INCOMING, receivingArea);
			StartCoroutine(TruckArrivalRoutine());
		}
		else
		{
			base.transform.position = truckStartPosition;
			base.transform.localEulerAngles = truckStartRotation;
			EnableDoors(state: false);
			base.gameObject.SetActive(value: true);
			carAI.Move(truckArrivalPath.wps, OnArrivedSupermarket);
			EventManager.NotifyEvent(GameEvents.TRUCK_INCOMING, receivingArea);
		}
	}

	private IEnumerator TruckArrivalRoutine()
	{
		while (halfSecondsPassed < shipmentDuration)
		{
			yield return waiter;
			halfSecondsPassed++;
		}
		EventManager.NotifyEvent(GameEvents.TRUCK_ARRIVED, receivingArea);
		GenericDataSerializer.Save("TRUCK_ARRIVED_KEY" + receivingArea, dataToSave: true);
	}

	private void OnArrivedSupermarket()
	{
		rigidBody.isKinematic = true;
		elevator.DOLocalMove(elevatorTarget.transform.localPosition, 1f).SetEase(Ease.InSine);
		elevator.DOLocalRotate(elevatorTargetRotation, 1f).SetEase(Ease.InSine).OnComplete(delegate
		{
			EnableDoors(state: true);
			EventManager.NotifyEvent(GameEvents.TRUCK_ARRIVED, receivingArea);
			GenericDataSerializer.Save("TRUCK_POSITION_KEY", base.transform.position);
			GenericDataSerializer.Save("TRUCK_ROTATION_KEY", base.transform.localEulerAngles);
		});
	}

	public void Leave()
	{
		if (IsStationary())
		{
			truckActive = false;
			GenericDataSerializer.Save("TRUCK_ACTIVE_KEY" + receivingArea, truckActive);
			EventManager.NotifyEvent(GameEvents.TRUCK_LEAVING, receivingArea);
			base.gameObject.SetActive(value: false);
			EventManager.NotifyEvent(GameEvents.TRUCK_DISAPPEARED, receivingArea);
			GenericDataSerializer.Save("TRUCK_ARRIVED_KEY" + receivingArea, dataToSave: false);
			return;
		}
		rigidBody.isKinematic = false;
		doorRight.enabled = false;
		doorLeft.enabled = false;
		if (receivingArea == OrderManager.OrderReceivingArea.STORE_FRONT && doorBlockCollider != null)
		{
			doorBlockCollider.enabled = true;
		}
		GenericDataSerializer.DeleteKey("TRUCK_POSITION_KEY");
		GenericDataSerializer.DeleteKey("TRUCK_ROTATION_KEY");
		EventManager.NotifyEvent(GameEvents.TRUCK_LEAVING, receivingArea);
		elevator.DOLocalMove(elevatorLocalStartPosition, 1f).SetEase(Ease.InSine).SetDelay(1f);
		elevator.DOLocalRotate(elevatorLocalStartRotation, 1f).SetEase(Ease.InSine).SetDelay(1f)
			.OnComplete(delegate
			{
				carAI.Move(truckDeparturePath.wps, delegate
				{
					base.gameObject.SetActive(value: false);
					EventManager.NotifyEvent(GameEvents.TRUCK_DISAPPEARED, receivingArea);
					if (leaveCoroutine != null)
					{
						StopCoroutine(leaveCoroutine);
					}
				});
			});
		leaveCoroutine = StartCoroutine(LeaveDelayRoutine());
	}

	private IEnumerator LeaveDelayRoutine()
	{
		yield return leaveDelay;
		carAI.Stop();
		base.gameObject.SetActive(value: false);
		EventManager.NotifyEvent(GameEvents.TRUCK_DISAPPEARED, receivingArea);
	}

	private void EnableDoors(bool state)
	{
		doorRight.enabled = state;
		doorLeft.enabled = state;
		if (receivingArea == OrderManager.OrderReceivingArea.STORE_FRONT && doorBlockCollider != null)
		{
			doorBlockCollider.enabled = !state;
		}
	}

	public void OnDoorClicked()
	{
		if (!doorsAnimating)
		{
			if (isDoorOpen)
			{
				isDoorOpen = !isDoorOpen;
				CloseDoors();
			}
			else
			{
				isDoorOpen = !isDoorOpen;
				OpenDoors();
			}
		}
	}

	private void CloseDoors()
	{
		doorsAnimating = true;
		doorRight.transform.DOLocalRotate(closedDoorEulerAngle, 270f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.OutQuad)
			.OnComplete(delegate
			{
				if (IsEmpty())
				{
					Leave();
				}
				else
				{
					SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("truck_unload_warning", base.transform);
				}
				doorsAnimating = false;
			});
		doorLeft.transform.DOLocalRotate(closedDoorEulerAngle, 270f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.OutQuad);
		StartCoroutine(PlayCloseClipDelayed(0.35f));
	}

	public void SellPalletsInsideTruck()
	{
		Collider[] objectsInsideTruck = GetObjectsInsideTruck();
		if (objectsInsideTruck.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < objectsInsideTruck.Length; i++)
		{
			if (objectsInsideTruck[i].gameObject.layer == RayShooter.PALLET_LAYER)
			{
				Pallet component = objectsInsideTruck[i].GetComponent<Pallet>();
				if (component != null)
				{
					int palletID = component.PalletID;
					component.gameObject.SetActive(value: false);
					component.EnableCollider(enable: true);
					SingletonBehaviour<PalletManager>.Instance.DeletePallet(palletID);
					component.OnMouseHoverEnded();
					EventManager.NotifyEvent(EconomyEvents.ADD_SOFT_CURRENCY, PalletManager.PALLET_DEPOSIT_AMOUNT);
					flag = true;
				}
			}
		}
		if (flag)
		{
			PlayAudioIfInRange(truckBackAudioSource, AudioManager.AudioTypes.PAYMENT_DONE);
		}
	}

	private void PlayAudioIfInRange(AudioSource source, AudioManager.AudioTypes type)
	{
		if (!(source == null) && Vector3.Distance(base.transform.position, SingletonBehaviour<RayShooter>.Instance.MainCamera.transform.position) < source.maxDistance + AudioManager.DISTANCE_BUFFER)
		{
			source.PlayOneShot(SingletonBehaviour<AudioManager>.Instance.GetAudioClip(type));
		}
	}

	public bool IsEmpty()
	{
		Collider[] objectsInsideTruck = GetObjectsInsideTruck();
		bool flag = true;
		if (objectsInsideTruck.Length != 0)
		{
			new List<Pallet>();
			for (int i = 0; i < objectsInsideTruck.Length; i++)
			{
				if (objectsInsideTruck[i].gameObject.layer == RayShooter.CLICKABLE_LAYER && objectsInsideTruck[i].gameObject.tag == RayShooter.PRICE_LABEL_TAG)
				{
					continue;
				}
				if (objectsInsideTruck[i].gameObject.layer == RayShooter.PALLET_LAYER)
				{
					Pallet component = objectsInsideTruck[i].GetComponent<Pallet>();
					if (component != null && !component.IsEmpty)
					{
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
			}
		}
		return objectsInsideTruck.Length == 0 || flag;
	}

	private Collider[] GetObjectsInsideTruck()
	{
		Vector3 position = truckOverlapTrigger.transform.position;
		Vector3 halfExtents = truckOverlapTrigger.size * 0.5f * truckOverlapTrigger.transform.lossyScale.x;
		Quaternion rotation = truckOverlapTrigger.transform.rotation;
		return Physics.OverlapBox(position, halfExtents, rotation, truckOverlapLayers, QueryTriggerInteraction.Ignore);
	}

	private IEnumerator PlayCloseClipDelayed(float delay)
	{
		yield return new WaitForSeconds(delay);
		truckBackAudioSource.PlayOneShot(doorCloseClip);
	}

	private void OpenDoors()
	{
		doorsAnimating = true;
		doorRight.transform.DOLocalRotate(openRightDoorEulerAngle, 135f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.InSine)
			.OnComplete(delegate
			{
				doorsAnimating = false;
			});
		doorLeft.transform.DOLocalRotate(openLeftDoorEulerAngle, 135f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.InSine);
		truckBackAudioSource.PlayOneShot(doorOpenClip);
	}

	public bool IsStationary()
	{
		return receivingArea != OrderManager.OrderReceivingArea.STORE_FRONT;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
