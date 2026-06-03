using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
	public GameObject ghostPrefab;

	public GameObject placementPrefab;

	public GameObject ghostParent;

	public bool placingObject;

	public Material ghostMaterialValid;

	public Material ghostMaterialInvalid;

	private bool wasValidPosition;

	private int floorLayer = 14;

	private MeshRenderer[] renderers;

	private SkinnedMeshRenderer[] sRenderers;

	private GameObject ghostInstance;

	private void Awake()
	{
		if (placingObject)
		{
			startPlacing();
		}
	}

	public void startPlacing()
	{
		ghostInstance = Object.Instantiate(ghostPrefab, ghostParent.transform);
		renderers = ghostInstance.transform.GetComponentsInChildren<MeshRenderer>();
		sRenderers = ghostInstance.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
		if (wasValidPosition)
		{
			setGhostMaterial(ghostMaterialValid);
		}
		else
		{
			setGhostMaterial(ghostMaterialInvalid);
		}
	}

	private void setGhostMaterial(Material newMaterial)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] array = new Material[renderers[i].materials.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = newMaterial;
			}
			renderers[i].materials = array;
		}
		for (int k = 0; k < sRenderers.Length; k++)
		{
			Material[] array2 = new Material[sRenderers[k].materials.Length];
			for (int l = 0; l < array2.Length; l++)
			{
				array2[l] = newMaterial;
			}
			sRenderers[k].materials = array2;
		}
	}

	public void stopPlacing()
	{
		placingObject = false;
		Object.Destroy(ghostInstance);
	}

	private void performInteract()
	{
		if (wasValidPosition && placingObject)
		{
			Object.Instantiate(placementPrefab, ghostInstance.transform.position, ghostInstance.transform.rotation);
		}
	}

	private void FixedUpdate()
	{
		if (!placingObject)
		{
			return;
		}
		int layerMask = 1 << floorLayer;
		if (Physics.Raycast(base.transform.position, base.transform.TransformDirection(Vector3.forward), out var hitInfo, 3f, layerMask))
		{
			ghostInstance.transform.position = hitInfo.point;
			ghostInstance.transform.rotation = Quaternion.identity;
			Collider[] array = Physics.OverlapSphere(new Vector3(ghostInstance.transform.position.x, ghostInstance.transform.position.y + 0.6f, ghostInstance.transform.position.z), 0.3f);
			bool flag = true;
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (!nameMatchesPrefab(collider.name) && !collider.isTrigger)
				{
					flag = false;
				}
			}
			if (flag)
			{
				if (!wasValidPosition)
				{
					setGhostMaterial(ghostMaterialValid);
					wasValidPosition = true;
				}
				Debug.DrawRay(base.transform.position, base.transform.TransformDirection(Vector3.forward) * hitInfo.distance, Color.blue);
			}
			else
			{
				if (wasValidPosition)
				{
					setGhostMaterial(ghostMaterialInvalid);
					wasValidPosition = false;
				}
				Debug.DrawRay(base.transform.position, base.transform.TransformDirection(Vector3.forward) * hitInfo.distance, Color.magenta);
			}
		}
		else
		{
			if (wasValidPosition)
			{
				setGhostMaterial(ghostMaterialInvalid);
				ghostInstance.transform.localPosition = Vector3.zero;
				wasValidPosition = false;
			}
			Debug.DrawRay(base.transform.position, base.transform.TransformDirection(Vector3.forward) * 3f, Color.red);
		}
		bool nameMatchesPrefab(string colliderName)
		{
			bool result = false;
			if (colliderName.StartsWith(ghostPrefab.name))
			{
				result = true;
			}
			Collider[] componentsInChildren = ghostPrefab.GetComponentsInChildren<Collider>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (colliderName == componentsInChildren[j].gameObject.name)
				{
					result = true;
				}
			}
			return result;
		}
	}
}
