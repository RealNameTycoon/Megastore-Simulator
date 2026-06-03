using UnityEngine;

public class ProductCullingManager : MonoBehaviour
{
	[SerializeField]
	private bool enableCustomOcculision = true;

	private const float LOWER_FLOOR_DISTANCE = 3f;

	public void CullPlaceables()
	{
		Vector3 position = SingletonBehaviour<PlayerMove>.Instance.transform.position;
		for (int i = 0; i < SingletonBehaviour<SpawnManager>.Instance.Placeables.Count; i++)
		{
			Placeable placeable = SingletonBehaviour<SpawnManager>.Instance.Placeables[i];
			if (placeable.CustomOcculisionEnabled)
			{
				float num = position.y - placeable.transform.position.y;
				Vector3 vector = placeable.transform.position - position;
				bool flag = num > 3f;
				bool num2 = Vector3.Angle(vector, placeable.transform.forward) < placeable.CustomOcculisionAngle / 2f;
				bool flag2 = Vector3.Distance(position, placeable.transform.position) > placeable.MinDistanceToStartOcculision;
				if (num2 && flag2 && !flag)
				{
					placeable.CullAllProducts(cull: true);
				}
				else
				{
					placeable.CullAllProducts(cull: false);
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (enableCustomOcculision)
		{
			CullPlaceables();
		}
	}
}
