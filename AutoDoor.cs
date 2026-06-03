using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AutoDoor : MonoBehaviour
{
	[SerializeField]
	private BoxCollider trigger;

	[Header("Door Parts")]
	[SerializeField]
	private Transform leftDoor;

	[SerializeField]
	private Transform leftDoorOpenTarget;

	[SerializeField]
	private Transform rightDoor;

	[SerializeField]
	private Transform rightDoorOpenTarget;

	[Header("Timing & Easing")]
	[SerializeField]
	private Ease openEase = Ease.InOutSine;

	[SerializeField]
	private Ease closeEase = Ease.InOutSine;

	[Header("Audio (optional)")]
	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip openClip;

	[SerializeField]
	private AudioClip closeClip;

	[Header("Options")]
	[Tooltip("How close is considered 'at endpoint' for playing sounds.")]
	[SerializeField]
	private float endpointTolerance = 0.0025f;

	[SerializeField]
	private bool ignorePlayer;

	[SerializeField]
	private bool animateRotation;

	[SerializeField]
	private TemperatureZone temperatureZone;

	private Vector3 leftClosedPos;

	private Vector3 rightClosedPos;

	private Vector3 leftOpenPos;

	private Vector3 rightOpenPos;

	private Vector3 leftClosedRot;

	private Vector3 rightClosedRot;

	private Vector3 leftOpenRot;

	private Vector3 rightOpenRot;

	private bool isOpen;

	public static string CustomerTag = "Customer";

	public static string PlayerTag = "Player";

	public const int IgnoreRaycastLayer = 2;

	private HashSet<int> occupantIDs = new HashSet<int>();

	private float initialVolume;

	public TemperatureZone TemperatureZone => temperatureZone;

	public bool IsOpen => isOpen;

	private void Awake()
	{
		if (audioSource != null)
		{
			initialVolume = audioSource.volume;
			audioSource.volume = SingletonBehaviour<AudioManager>.Instance.MasterAudioMultiplier * initialVolume;
		}
		if (leftDoor != null)
		{
			leftClosedPos = leftDoor.position;
			leftOpenPos = leftDoorOpenTarget.position;
			leftClosedRot = leftDoor.localEulerAngles;
			leftOpenRot = leftDoorOpenTarget.localEulerAngles;
		}
		if (rightDoor != null)
		{
			rightOpenPos = rightDoorOpenTarget.position;
			rightClosedPos = rightDoor.position;
			rightClosedRot = rightDoor.localEulerAngles;
			rightOpenRot = rightDoorOpenTarget.localEulerAngles;
		}
		EventManager.AddListener(GameEvents.NEW_DAY_STARTED, OnNewDayStarted);
	}

	private void OnNewDayStarted()
	{
		Vector3 center = trigger.bounds.center;
		Vector3 extents = trigger.bounds.extents;
		Quaternion rotation = trigger.transform.rotation;
		occupantIDs.Clear();
		Collider[] array = Physics.OverlapBox(center, extents, rotation, 4, QueryTriggerInteraction.Ignore);
		foreach (Collider collider in array)
		{
			if (collider.CompareTag(PlayerTag))
			{
				occupantIDs.Add(collider.gameObject.GetInstanceID());
				return;
			}
		}
		CloseDoorInstant();
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((ignorePlayer && other.gameObject.tag == PlayerTag) || (other.gameObject.tag != PlayerTag && other.gameObject.tag != CustomerTag))
		{
			return;
		}
		if (temperatureZone != null && other.gameObject.tag == CustomerTag)
		{
			EscalatorRider component = other.GetComponent<EscalatorRider>();
			if (component != null && !component.ShoulOpenTheDoor(this))
			{
				return;
			}
		}
		if (!occupantIDs.Contains(other.gameObject.GetInstanceID()))
		{
			occupantIDs.Add(other.gameObject.GetInstanceID());
			if (!isOpen)
			{
				OpenDoor();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((ignorePlayer && other.gameObject.tag == PlayerTag) || (other.gameObject.tag != PlayerTag && other.gameObject.tag != CustomerTag))
		{
			return;
		}
		if (temperatureZone != null && other.gameObject.tag == CustomerTag)
		{
			EscalatorRider component = other.GetComponent<EscalatorRider>();
			if ((component != null && !component.ShouldCloseTheDoor(this)) || SingletonBehaviour<TemperatureZoneManager>.Instance.GetTemperatureZoneAtPosition(SingletonBehaviour<PlayerMove>.Instance.transform.position) == temperatureZone)
			{
				return;
			}
		}
		if (occupantIDs.Contains(other.gameObject.GetInstanceID()))
		{
			occupantIDs.Remove(other.gameObject.GetInstanceID());
			if (occupantIDs.Count == 0 && isOpen)
			{
				CloseDoor();
			}
		}
	}

	private void OpenDoor()
	{
		if ((bool)audioSource && (bool)openClip && AtClosedEndpoint())
		{
			audioSource.PlayOneShot(openClip);
		}
		isOpen = true;
		if (leftDoor != null)
		{
			leftDoor.DOKill();
			if (animateRotation)
			{
				leftDoor.DOLocalRotate(leftOpenRot, 0.8f).SetEase(openEase);
			}
			else
			{
				leftDoor.DOMove(leftOpenPos, 1.2f).SetEase(openEase);
			}
		}
		if (rightDoor != null)
		{
			rightDoor.DOKill();
			if (animateRotation)
			{
				rightDoor.DOLocalRotate(rightOpenRot, 0.8f).SetEase(openEase);
			}
			else
			{
				rightDoor.DOMove(rightOpenPos, 1.2f).SetEase(openEase);
			}
		}
	}

	private void CloseDoor()
	{
		if ((bool)audioSource && (bool)closeClip && AtOpenEndpoint() && !animateRotation)
		{
			audioSource.PlayOneShot(closeClip);
		}
		isOpen = false;
		if (leftDoor != null)
		{
			leftDoor.DOKill();
			if (animateRotation)
			{
				leftDoor.DOLocalRotate(leftClosedRot, 0.8f).SetEase(closeEase).OnComplete(delegate
				{
					if ((bool)audioSource && (bool)closeClip)
					{
						audioSource.PlayOneShot(closeClip);
					}
				});
			}
			else
			{
				leftDoor.DOMove(leftClosedPos, 1.2f).SetEase(closeEase);
			}
		}
		if (rightDoor != null)
		{
			rightDoor.DOKill();
			if (animateRotation)
			{
				rightDoor.DOLocalRotate(rightClosedRot, 0.8f).SetEase(closeEase);
			}
			else
			{
				rightDoor.DOMove(rightClosedPos, 1.2f).SetEase(closeEase);
			}
		}
	}

	public void OpenOrCloseDoor()
	{
		if (isOpen)
		{
			CloseDoor();
		}
		else
		{
			OpenDoor();
		}
	}

	private void CloseDoorInstant()
	{
		isOpen = false;
		if (leftDoor != null)
		{
			leftDoor.DOKill();
			if (animateRotation)
			{
				leftDoor.localEulerAngles = leftClosedRot;
			}
			else
			{
				leftDoor.transform.position = leftClosedPos;
			}
		}
		if (rightDoor != null)
		{
			rightDoor.DOKill();
			if (animateRotation)
			{
				rightDoor.localEulerAngles = rightClosedRot;
			}
			else
			{
				rightDoor.transform.position = rightClosedPos;
			}
		}
	}

	private bool AtClosedEndpoint()
	{
		if (animateRotation)
		{
			bool num = leftDoor == null || (leftDoor.localEulerAngles - leftClosedRot).sqrMagnitude <= endpointTolerance * endpointTolerance;
			bool flag = rightDoor == null || (rightDoor.localEulerAngles - rightClosedRot).sqrMagnitude <= endpointTolerance * endpointTolerance;
			return num && flag;
		}
		if ((leftDoor.position - leftClosedPos).sqrMagnitude <= endpointTolerance * endpointTolerance)
		{
			return (rightDoor.position - rightClosedPos).sqrMagnitude <= endpointTolerance * endpointTolerance;
		}
		return false;
	}

	private bool AtOpenEndpoint()
	{
		if (animateRotation)
		{
			bool num = leftDoor == null || (leftDoor.localEulerAngles - leftOpenRot).sqrMagnitude <= endpointTolerance * endpointTolerance;
			bool flag = rightDoor == null || (rightDoor.localEulerAngles - rightOpenRot).sqrMagnitude <= endpointTolerance * endpointTolerance;
			return num && flag;
		}
		bool num2 = leftDoor == null || (leftDoor.position - leftOpenPos).sqrMagnitude <= endpointTolerance * endpointTolerance;
		bool flag2 = rightDoor == null || (rightDoor.position - rightOpenPos).sqrMagnitude <= endpointTolerance * endpointTolerance;
		return num2 && flag2;
	}
}
