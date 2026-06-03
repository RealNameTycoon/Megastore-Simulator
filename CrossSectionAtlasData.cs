using UnityEngine;

[CreateAssetMenu(fileName = "CrossSectionAtlasData", menuName = "Choppable/Cross Section Atlas Data")]
public class CrossSectionAtlasData : ScriptableObject
{
	public string productTypeName;

	public int sliceCount;

	public int sliceResolution;

	public int atlasGrid;

	public Vector3 localBoundsMin;

	public Vector3 localBoundsMax;

	public Texture2D atlasTexture;
}
