using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class Product : MonoBehaviour
{
	[SerializeField]
	private Outline outline;

	[SerializeField]
	private ProductData data;

	[SerializeField]
	private int variantIndex;

	[SerializeField]
	protected MeshRenderer[] meshRenderers;

	[SerializeField]
	protected BoxCollider boxCollider;

	private bool renderersEnabled = true;

	private bool renderersCulled;

	public bool isReserved;

	protected float tempPrice;

	private Action onClickAction;

	public int VariantIndex => variantIndex;

	public ProductData Data => data;

	public MeshRenderer PrimaryRenderer => meshRenderers[0];

	public Vector3 BoxColliderSize => boxCollider.size;

	public Vector3 BoxColliderCenter => boxCollider.center;

	public float TempPrice
	{
		get
		{
			return tempPrice;
		}
		set
		{
			tempPrice = value;
		}
	}

	public void SetData(ProductData newData)
	{
		data = newData;
	}

	public void SetOnClickAction(Action onClickAction)
	{
		this.onClickAction = onClickAction;
	}

	public virtual void OnBeforePlace(PlaceableType placeableType, bool isStart)
	{
	}

	public virtual void OnAfterPlace()
	{
	}

	public virtual void OnBeforeTake()
	{
		if (data.type != ProductType.FOOD_TRAY)
		{
			tempPrice = SingletonBehaviour<PriceManager>.Instance.GetUnitPrice(data.type);
		}
	}

	public virtual void OnRemovedFromShelf()
	{
		if (renderersCulled && renderersEnabled)
		{
			EnableRenderers(enable: true);
		}
		renderersCulled = false;
	}

	public void EnableOutline(bool state)
	{
		outline.enabled = state;
	}

	public void OnScan()
	{
		if (outline.enabled)
		{
			outline.enabled = false;
			onClickAction?.Invoke();
			onClickAction = null;
		}
	}

	public void SetMaterial(Material material, MaterialPropertyBlock materialPropertyBlock)
	{
		for (int i = 0; i < meshRenderers.Length; i++)
		{
			Material[] sharedMaterials = meshRenderers[i].sharedMaterials;
			sharedMaterials[data.spoilableMaterialIndex] = material;
			meshRenderers[i].sharedMaterials = sharedMaterials;
			meshRenderers[i].SetPropertyBlock(materialPropertyBlock, data.spoilableMaterialIndex);
		}
	}

	public virtual void ResetProduct()
	{
	}

	public void DisableCastShadow()
	{
		for (int i = 0; i < meshRenderers.Length; i++)
		{
			meshRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
		}
	}

	public void FixLODDistances()
	{
		foreach (Transform item in base.transform)
		{
			if (item.GetComponent<MeshRenderer>() != null || item.GetComponent<SkinnedMeshRenderer>() != null)
			{
				continue;
			}
			MeshRenderer[] componentsInChildren = item.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				int num = componentsInChildren.Length;
				LODGroup lODGroup = item.GetComponent<LODGroup>();
				if (lODGroup == null)
				{
					lODGroup = item.gameObject.AddComponent<LODGroup>();
				}
				LOD[] array = new LOD[num];
				for (int i = 0; i < num; i++)
				{
					Renderer[] renderers = new Renderer[1] { componentsInChildren[i] };
					componentsInChildren[i].transform.localScale = Vector3.one;
					componentsInChildren[i].transform.localPosition = Vector3.zero;
					componentsInChildren[i].transform.localEulerAngles = Vector3.zero;
					array[i] = new LOD(num switch
					{
						3 => i switch
						{
							0 => 0.1f, 
							1 => 0.05f, 
							_ => 0f, 
						}, 
						0 => 40f, 
						1 => 15f, 
						_ => 5f, 
					}, renderers);
				}
				lODGroup.SetLODs(array);
				lODGroup.RecalculateBounds();
				Debug.Log($"LODGroup created on '{item.name}' with {num} LOD(s).");
			}
		}
	}

	public void MoveMeshSoParentIsBottomCenter()
	{
		if (meshRenderers == null || meshRenderers.Length == 0)
		{
			Debug.LogWarning("No MeshRenderers assigned.");
			return;
		}
		MeshRenderer meshRenderer = meshRenderers[0];
		if (meshRenderer == null)
		{
			Debug.LogWarning("No MeshRenderer found.");
			return;
		}
		MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			Debug.LogWarning("No MeshFilter / mesh on MeshRenderer.");
			return;
		}
		Vector3[] vertices = component.sharedMesh.vertices;
		if (vertices == null || vertices.Length == 0)
		{
			Debug.LogWarning("Mesh has no vertices.");
			return;
		}
		Transform transform = base.transform;
		Transform transform2 = meshRenderer.transform;
		Vector3 zero = Vector3.zero;
		float num = float.PositiveInfinity;
		int num2 = vertices.Length;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 position = transform2.TransformPoint(vertices[i]);
			Vector3 vector = transform.InverseTransformPoint(position);
			zero.x += vector.x;
			zero.z += vector.z;
			if (vector.y < num)
			{
				num = vector.y;
			}
		}
		Vector3 vector2 = new Vector3(zero.x / (float)num2, num, zero.z / (float)num2);
		if (transform2.GetComponent<LODGroup>() != null)
		{
			transform2.transform.parent.localPosition -= vector2;
		}
		else
		{
			transform2.localPosition -= vector2;
		}
		Debug.Log("Moved " + transform2.name + " so parent pivot is at bottom center.");
	}

	public void FixRotation()
	{
		if (meshRenderers == null || meshRenderers.Length == 0)
		{
			Debug.LogWarning("No MeshRenderers assigned.");
			return;
		}
		MeshRenderer meshRenderer = meshRenderers[0];
		if (meshRenderer == null)
		{
			return;
		}
		MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			return;
		}
		Mesh sharedMesh = component.sharedMesh;
		Vector3[] vertices = sharedMesh.vertices;
		int[] triangles = sharedMesh.triangles;
		if (vertices == null || vertices.Length == 0 || triangles == null || triangles.Length < 3)
		{
			return;
		}
		Transform transform = meshRenderer.transform;
		Vector3 vector = Vector3.zero;
		float num = -1f;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int num2 = triangles[i];
			int num3 = triangles[i + 1];
			int num4 = triangles[i + 2];
			Vector3 vector2 = transform.TransformPoint(vertices[num2]);
			Vector3 vector3 = transform.TransformPoint(vertices[num3]);
			Vector3 vector4 = Vector3.Cross(rhs: transform.TransformPoint(vertices[num4]) - vector2, lhs: vector3 - vector2);
			float num5 = vector4.magnitude * 0.5f;
			if (!(num5 < 1E-06f))
			{
				Vector3 normalized = vector4.normalized;
				if (num5 > num)
				{
					num = num5;
					vector = normalized;
				}
			}
		}
		if (!(num <= 0f) && !(vector == Vector3.zero))
		{
			if (Vector3.Dot(vector, Vector3.up) < 0f)
			{
				vector = -vector;
			}
			Quaternion quaternion = Quaternion.FromToRotation(vector, Vector3.up);
			transform.rotation = quaternion * transform.rotation;
		}
	}

	public void FixRotationRight()
	{
		if (meshRenderers == null || meshRenderers.Length == 0)
		{
			Debug.LogWarning("FixRotationRight: No MeshRenderers assigned.");
			return;
		}
		MeshRenderer meshRenderer = meshRenderers[0];
		if (meshRenderer == null)
		{
			return;
		}
		MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			return;
		}
		Vector3[] normals = component.sharedMesh.normals;
		if (normals == null || normals.Length == 0)
		{
			return;
		}
		Transform transform = meshRenderer.transform;
		Vector3 up = Vector3.up;
		Vector3 vector = Vector3.zero;
		float num = float.MaxValue;
		for (int i = 0; i < normals.Length; i++)
		{
			Vector3 normalized = transform.TransformDirection(normals[i]).normalized;
			float num2 = Mathf.Abs(Vector3.Dot(normalized, up));
			if (num2 < num)
			{
				num = num2;
				vector = normalized;
			}
		}
		if (!(vector == Vector3.zero))
		{
			Vector3 vector2 = Vector3.ProjectOnPlane(vector, up);
			if (!(vector2.sqrMagnitude < 1E-06f))
			{
				vector2.Normalize();
				Quaternion quaternion = Quaternion.AngleAxis(Vector3.SignedAngle(vector2, Vector3.right, up), up);
				transform.rotation = quaternion * transform.rotation;
			}
		}
	}

	private static string ToPascalNoUnderscore(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		string[] array = s.Split('_');
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (char.IsDigit(text[0]))
			{
				stringBuilder.Append(text);
				continue;
			}
			stringBuilder.Append(char.ToUpperInvariant(text[0]));
			if (text.Length > 1)
			{
				stringBuilder.Append(text.Substring(1).ToLowerInvariant());
			}
		}
		return stringBuilder.ToString();
	}

	public void FixPosition()
	{
		GameObject gameObject = FindChildByName(base.transform, "GameObject").gameObject.GetComponentInChildren<MeshRenderer>().gameObject;
		BoxCollider component = gameObject.GetComponent<BoxCollider>();
		if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
		else
		{
			MeshCollider component2 = gameObject.GetComponent<MeshCollider>();
			if (component2 != null)
			{
				UnityEngine.Object.DestroyImmediate(component2);
			}
		}
		component = gameObject.AddComponent<BoxCollider>();
		gameObject.transform.localPosition = Vector3.zero + Vector3.up * component.size.y * gameObject.transform.localScale.y / 2f;
		UnityEngine.Object.DestroyImmediate(component);
	}

	public void AssignMeshRenderers()
	{
		meshRenderers = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
	}

	public void EnableRenderers(bool enable)
	{
		if (meshRenderers == null || meshRenderers.Length == 0)
		{
			Debug.LogWarning("EnableRenderers: No MeshRenderers assigned." + data.type);
			return;
		}
		for (int i = 0; i < meshRenderers.Length; i++)
		{
			if (meshRenderers[i] == null)
			{
				Debug.LogWarning("EnableRenderers: MeshRenderer is null." + data.type);
			}
			else
			{
				meshRenderers[i].enabled = enable;
			}
		}
		renderersEnabled = enable;
	}

	public void CullRenderers(bool cull)
	{
		if (meshRenderers == null || meshRenderers.Length == 0)
		{
			Debug.LogWarning("CullRenderers: No MeshRenderers assigned." + data.type);
			return;
		}
		for (int i = 0; i < meshRenderers.Length; i++)
		{
			if (meshRenderers[i] == null)
			{
				Debug.LogWarning("CullRenderers: MeshRenderer is null." + data.type);
			}
			else
			{
				meshRenderers[i].enabled = !cull;
			}
		}
		renderersCulled = cull;
	}

	private Transform FindChildByName(Transform parent, string childName)
	{
		foreach (Transform item in parent)
		{
			if (item.name == childName)
			{
				return item;
			}
			Transform transform2 = FindChildByName(item, childName);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}

	private BoxPool FindActiveBoxPool()
	{
		GameObject gameObject = GameObject.Find("BoxPool");
		if ((bool)gameObject && gameObject.activeInHierarchy)
		{
			BoxPool component = gameObject.GetComponent<BoxPool>();
			if ((bool)component)
			{
				return component;
			}
		}
		if ((bool)SingletonBehaviour<BoxPool>.Instance)
		{
			return SingletonBehaviour<BoxPool>.Instance;
		}
		return UnityEngine.Object.FindObjectsByType<BoxPool>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault((BoxPool x) => x.isActiveAndEnabled);
	}

	private PlaceablePool FindActivePlaceablePool()
	{
		GameObject gameObject = GameObject.Find("PlaceablePool");
		if ((bool)gameObject && gameObject.activeInHierarchy)
		{
			PlaceablePool component = gameObject.GetComponent<PlaceablePool>();
			if ((bool)component)
			{
				return component;
			}
		}
		if ((bool)SingletonBehaviour<PlaceablePool>.Instance)
		{
			return SingletonBehaviour<PlaceablePool>.Instance;
		}
		return UnityEngine.Object.FindObjectsByType<PlaceablePool>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault((PlaceablePool x) => x.isActiveAndEnabled);
	}
}
