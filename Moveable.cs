using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] renderers;

	[SerializeField]
	private SkinnedMeshRenderer[] skinnedMeshRenderers;

	[SerializeField]
	private Material validMaterial;

	[SerializeField]
	private Material invalidMaterial;

	[SerializeField]
	private MonoBehaviour movedObject;

	private LayerMask placeableFloorLayerMask;

	private MoveableObjectInterface movedObjectCache;

	private BoxCollider boxCollider;

	[SerializeField]
	private List<BoxCollider> boxColliders = new List<BoxCollider>();

	[SerializeField]
	private List<Collider> collidingEntities = new List<Collider>();

	private bool isValid = true;

	private bool isStationary;

	private bool isInValidLayer = true;

	public BoxCollider BoxCollider => boxCollider;

	public List<BoxCollider> BoxColliders => boxColliders;

	public MoveableObjectInterface MovedObject
	{
		get
		{
			if (movedObjectCache == null && movedObject != null)
			{
				movedObjectCache = movedObject as MoveableObjectInterface;
			}
			return movedObjectCache;
		}
	}

	public bool IsValid
	{
		get
		{
			if (isValid && isStationary)
			{
				return isInValidLayer;
			}
			return false;
		}
	}

	public bool IsStationary => isStationary;

	public void AssignMeshRenderers()
	{
		renderers = GetComponentsInChildren<MeshRenderer>(includeInactive: false);
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] == null)
			{
				Debug.LogWarning("AssignMeshRenderers: MeshRenderer is null." + base.gameObject.name);
			}
			else if (renderers[i].sharedMaterial != validMaterial)
			{
				renderers[i].sharedMaterial = validMaterial;
			}
		}
		skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		for (int j = 0; j < skinnedMeshRenderers.Length; j++)
		{
			if (skinnedMeshRenderers[j] == null)
			{
				Debug.LogWarning("AssignMeshRenderers: SkinnedMeshRenderer is null." + base.gameObject.name);
			}
		}
	}

	public void FixCollider()
	{
		MeshRenderer obj = renderers[0];
		Bounds bounds = obj.bounds;
		if (obj == null)
		{
			Debug.LogWarning("No MeshRenderer found in children.");
			return;
		}
		BoxCollider component = GetComponent<BoxCollider>();
		if (component != null)
		{
			Object.DestroyImmediate(component);
		}
		BoxCollider obj2 = base.gameObject.AddComponent<BoxCollider>();
		Vector3 center = base.transform.InverseTransformPoint(bounds.center);
		Vector3 extents = bounds.extents;
		Vector3 zero = Vector3.zero;
		zero.x = Vector3.Scale(base.transform.InverseTransformVector(new Vector3(extents.x, 0f, 0f)), Vector3.one).magnitude * 2f;
		zero.y = Vector3.Scale(base.transform.InverseTransformVector(new Vector3(0f, extents.y, 0f)), Vector3.one).magnitude * 2f;
		zero.z = Vector3.Scale(base.transform.InverseTransformVector(new Vector3(0f, 0f, extents.z)), Vector3.one).magnitude * 2f;
		obj2.center = center;
		obj2.size = zero;
		obj2.isTrigger = true;
		Debug.Log("Collider fixed on " + base.gameObject.name + " (fits first MeshRenderer in children).");
	}

	public void FixTriggerBox()
	{
		BoxCollider component = base.transform.parent.GetComponent<BoxCollider>();
		BoxCollider component2 = GetComponent<BoxCollider>();
		component2.center = component.center;
		component2.size = component.size;
		component2.isTrigger = true;
	}

	private void Awake()
	{
	}

	public void ResetCollidedEntities()
	{
		collidingEntities.Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("collider entered: " + other.gameObject.name);
		if (!IsInMask(placeableFloorLayerMask, other.gameObject.layer) && !other.isTrigger)
		{
			collidingEntities.Add(other);
			if (isValid)
			{
				isValid = false;
				UpdateMaterial(invalidMaterial);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (IsInMask(placeableFloorLayerMask, other.gameObject.layer) || other.isTrigger)
		{
			return;
		}
		Debug.Log("collider exited: " + other.gameObject.name);
		collidingEntities.Remove(other);
		if (collidingEntities.Count == 0)
		{
			if (isStationary)
			{
				UpdateMaterial(validMaterial);
			}
			isValid = true;
		}
	}

	private void UpdateMaterial(Material newMaterial)
	{
		if (renderers.Length == 0)
		{
			for (int i = 0; i < skinnedMeshRenderers.Length; i++)
			{
				Material[] array = new Material[skinnedMeshRenderers[i].materials.Length];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = newMaterial;
				}
				skinnedMeshRenderers[i].materials = array;
			}
			return;
		}
		for (int k = 0; k < renderers.Length; k++)
		{
			Material[] array2 = new Material[renderers[k].materials.Length];
			for (int l = 0; l < array2.Length; l++)
			{
				array2[l] = newMaterial;
			}
			renderers[k].materials = array2;
		}
	}

	public void EnableGhost()
	{
		MovedObject.SwitchLook(toSolidObject: false);
	}

	public void OnPlacementEnded(bool isCanceled = false)
	{
		if (!isCanceled)
		{
			EventManager.NotifyEvent(PlaceableEvents.OBJECT_PLACEMENT_ENDED, MovedObject);
		}
		MovedObject.OnPlacementEnded();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
		ResetCollidedEntities();
		MovedObject.SwitchLook(toSolidObject: true);
		MovedObject.SavePosition();
	}

	public virtual void ResetPosition()
	{
	}

	public void SetStationary(bool stationary)
	{
		isStationary = stationary;
		UpdateValidationLook();
	}

	public void SetHitLayer(int layer)
	{
		if (placeableFloorLayerMask.value != 0)
		{
			if (IsInMask(placeableFloorLayerMask, layer))
			{
				isInValidLayer = true;
			}
			else
			{
				isInValidLayer = false;
			}
			UpdateValidationLook();
		}
	}

	private void UpdateValidationLook()
	{
		if (ShouldBeValid())
		{
			UpdateMaterial(validMaterial);
		}
		else
		{
			UpdateMaterial(invalidMaterial);
		}
	}

	private bool ShouldBeValid()
	{
		if (isInValidLayer && collidingEntities.Count == 0)
		{
			return isStationary;
		}
		return false;
	}

	private bool IsInMask(LayerMask mask, int layer)
	{
		return (mask.value & (1 << layer)) != 0;
	}

	public void SetPlaceableFloorLayers(LayerMask layerMask)
	{
		placeableFloorLayerMask = layerMask;
		Initialize();
	}

	private void Initialize()
	{
		boxCollider = GetComponent<BoxCollider>();
		BoxCollider[] components = GetComponents<BoxCollider>();
		boxColliders.AddRange(components);
	}

	private void Update()
	{
	}
}
