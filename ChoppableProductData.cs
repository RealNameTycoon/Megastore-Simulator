using UnityEngine;

[CreateAssetMenu(fileName = "ChoppableProductData", menuName = "ScriptableObjects/ChoppableProductData", order = 1)]
public class ChoppableProductData : ScriptableObject
{
	public Vector3[] localPositions;

	public Vector3[] eulerAngles;

	public void SetPositions(Transform[] localTransforms)
	{
		localPositions = new Vector3[localTransforms.Length];
		eulerAngles = new Vector3[localTransforms.Length];
		for (int i = 0; i < localTransforms.Length; i++)
		{
			localPositions[i] = localTransforms[i].localPosition;
			eulerAngles[i] = localTransforms[i].localEulerAngles;
		}
	}
}
