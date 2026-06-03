using System.Collections.Generic;
using UnityEngine;

public class DecorationManager : SingletonBehaviour<DecorationManager>
{
	[SerializeField]
	private List<FloorClickable> floorClickables;

	[SerializeField]
	private List<FloorClickable> wallClickables;

	[SerializeField]
	private List<Material> floorMaterials;

	[SerializeField]
	private List<Material> wallMaterials;

	[SerializeField]
	private List<MeshRenderer> walls;

	[SerializeField]
	private MeshRenderer floor;

	[SerializeField]
	private List<GameObject> floorClickableParents;

	[SerializeField]
	private List<GameObject> wallClickableParents;

	[SerializeField]
	private Material defaultWallMaterial;

	[SerializeField]
	private Material defaultFloorMaterial;

	private const string lastUsedDecorationTypeKey = "lastUsedDecorationTypeKey";

	private const string lastUsedDecorationIndexKey = "lastUsedDecorationIndexKey";

	private int usedFloorDecoration;

	private int usedWallDecoration;

	public DecorationUI.DecorationType lastUsedDecorationType;

	public int lastUsedDecorationIndex;

	private static readonly int URP_BaseMap = Shader.PropertyToID("_BaseMap");

	private static readonly int URP_BumpMap = Shader.PropertyToID("_BumpMap");

	private static readonly int URP_BaseColor = Shader.PropertyToID("_BaseColor");

	public Material DefaultWallMaterial => defaultWallMaterial;

	public Material DefaultFloorMaterial => defaultFloorMaterial;

	public int UsedFloorDecoration => usedFloorDecoration;

	public int UsedWallDecoration => usedWallDecoration;

	private new void Awake()
	{
		base.Awake();
		lastUsedDecorationType = GenericDataSerializer.Load("lastUsedDecorationTypeKey", DecorationUI.DecorationType.FLOOR);
		lastUsedDecorationIndex = GenericDataSerializer.LoadInt("lastUsedDecorationIndexKey");
		UseDecoration(lastUsedDecorationType, lastUsedDecorationIndex);
	}

	public void PurchaseDecoration(DecorationUI.DecorationType type, int id)
	{
		GenericDataSerializer.SaveBool(type.ToString() + id, value: true);
		EventManager.NotifyEvent(DecorationEvents.DECORATION_PURCHASED, type, id);
	}

	public bool IsDecorationPurchased(DecorationUI.DecorationType type, int id)
	{
		if (id == 0)
		{
			return true;
		}
		if (type == DecorationUI.DecorationType.WALL && id < 3)
		{
			return true;
		}
		return GenericDataSerializer.HasKey(type.ToString() + id);
	}

	public void UseDecoration(DecorationUI.DecorationType type, int id)
	{
		lastUsedDecorationType = type;
		lastUsedDecorationIndex = id;
		GenericDataSerializer.Save("lastUsedDecorationTypeKey", type);
		GenericDataSerializer.SaveInt("lastUsedDecorationIndexKey", id);
		EventManager.NotifyEvent(DecorationEvents.DECORATION_USED, type, id);
	}

	public Material GetMaterial(DecorationUI.DecorationType type, int id)
	{
		switch (type)
		{
		case DecorationUI.DecorationType.FLOOR:
			if (id > floorMaterials.Count - 1)
			{
				id = 0;
			}
			return floorMaterials[id];
		case DecorationUI.DecorationType.WALL:
			if (id > wallMaterials.Count - 1)
			{
				id = 0;
			}
			return wallMaterials[id];
		default:
			return floorMaterials[id];
		}
	}

	public Texture GetTexture(DecorationUI.DecorationType type, int id)
	{
		switch (type)
		{
		case DecorationUI.DecorationType.FLOOR:
			if (id > floorMaterials.Count - 1)
			{
				id = 0;
			}
			if (floorMaterials[id].HasProperty(URP_BaseMap))
			{
				return floorMaterials[id].GetTexture(URP_BaseMap);
			}
			return null;
		case DecorationUI.DecorationType.WALL:
			if (id > wallMaterials.Count - 1)
			{
				id = 0;
			}
			if (wallMaterials[id].HasProperty(URP_BaseMap))
			{
				return wallMaterials[id].GetTexture(URP_BaseMap);
			}
			return null;
		default:
			return null;
		}
	}

	public Texture GetNormalTexture(DecorationUI.DecorationType type, int id)
	{
		switch (type)
		{
		case DecorationUI.DecorationType.FLOOR:
			if (id > floorMaterials.Count - 1)
			{
				id = 0;
			}
			if (floorMaterials[id].HasProperty(URP_BumpMap))
			{
				return floorMaterials[id].GetTexture(URP_BumpMap);
			}
			return null;
		case DecorationUI.DecorationType.WALL:
			if (id > wallMaterials.Count - 1)
			{
				id = 0;
			}
			if (wallMaterials[id].HasProperty(URP_BumpMap))
			{
				return wallMaterials[id].GetTexture(URP_BumpMap);
			}
			return null;
		default:
			return null;
		}
	}

	public Color GetColor(DecorationUI.DecorationType type, int id)
	{
		switch (type)
		{
		case DecorationUI.DecorationType.FLOOR:
		{
			if (id > floorMaterials.Count - 1)
			{
				id = 0;
			}
			Material material2 = floorMaterials[id];
			if (material2 != null && material2.HasProperty(URP_BaseColor))
			{
				return material2.GetColor(URP_BaseColor);
			}
			break;
		}
		case DecorationUI.DecorationType.WALL:
		{
			if (id > wallMaterials.Count - 1)
			{
				id = 0;
			}
			Material material = wallMaterials[id];
			if (material != null && material.HasProperty(URP_BaseColor))
			{
				return material.GetColor(URP_BaseColor);
			}
			break;
		}
		}
		return Color.white;
	}

	public Texture FlatNormalTexture()
	{
		return floorMaterials[0].GetTexture(URP_BumpMap);
	}
}
