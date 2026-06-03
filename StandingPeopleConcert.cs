using System;
using System.Collections.Generic;
using UnityEngine;

public class StandingPeopleConcert : MonoBehaviour
{
	public enum TestEnum
	{
		Rectangle,
		Circle
	}

	[HideInInspector]
	public GameObject planePrefab;

	[HideInInspector]
	public GameObject circlePrefab;

	[HideInInspector]
	public GameObject surface;

	[HideInInspector]
	public Vector2 planeSize = new Vector2(1f, 1f);

	[Tooltip("People prefabs / Префабы людей")]
	public GameObject[] peoplePrefabs = new GameObject[0];

	[HideInInspector]
	private List<Vector3> spawnPoints = new List<Vector3>();

	[HideInInspector]
	public GameObject target;

	[HideInInspector]
	public int peopleCount;

	[HideInInspector]
	public bool isCircle;

	[HideInInspector]
	public float circleDiametr = 1f;

	[HideInInspector]
	public bool showSurface = true;

	[Tooltip("Type of surface / Тип поверхности")]
	public TestEnum SurfaceType;

	[HideInInspector]
	public GameObject par;

	[HideInInspector]
	public bool looking;

	[HideInInspector]
	public float damping = 5f;

	[HideInInspector]
	public float highToSpawn;

	public void OnDrawGizmos()
	{
		if (!isCircle)
		{
			surface.transform.localScale = new Vector3(planeSize.x, 1f, planeSize.y);
		}
		else
		{
			surface.transform.localScale = new Vector3(circleDiametr, 1f, circleDiametr);
		}
	}

	public void SpawnRectangleSurface()
	{
		if (surface != null)
		{
			UnityEngine.Object.DestroyImmediate(surface);
		}
		GameObject gameObject = (surface = UnityEngine.Object.Instantiate(planePrefab, base.transform.position, Quaternion.identity));
		isCircle = false;
		gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);
		gameObject.transform.position += new Vector3(0f, 0.01f, 0f);
		gameObject.transform.parent = base.transform;
		gameObject.name = "surface";
	}

	public void SpawnCircleSurface()
	{
		if (surface != null)
		{
			UnityEngine.Object.DestroyImmediate(surface);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(circlePrefab, base.transform.position, Quaternion.identity);
		isCircle = true;
		gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);
		gameObject.transform.position += new Vector3(0f, 0.01f, 0f);
		gameObject.transform.parent = base.transform;
		gameObject.name = "surface";
		surface = gameObject;
	}

	public void RemoveButton()
	{
		if (par != null)
		{
			UnityEngine.Object.DestroyImmediate(par);
		}
	}

	public void PopulateButton()
	{
		RemoveButton();
		GameObject gameObject = (par = new GameObject());
		gameObject.transform.parent = base.gameObject.transform;
		gameObject.name = "people";
		spawnPoints.Clear();
		SpawnPeople(peopleCount);
	}

	private void SpawnPeople(int _peopleCount)
	{
		int[] randomPrefabIndexes = CommonUtils.GetRandomPrefabIndexes(_peopleCount, ref peoplePrefabs);
		for (int i = 0; i < _peopleCount; i++)
		{
			Vector3 vector = (isCircle ? RandomCirclePosition() : RandomRectanglePosition());
			if (!(vector != Vector3.zero))
			{
				continue;
			}
			GameObject gameObject = peoplePrefabs[randomPrefabIndexes[i]];
			GameObject gameObject2 = null;
			if (!Physics.Raycast(vector + Vector3.up * highToSpawn, Vector3.down, out var hitInfo, float.PositiveInfinity))
			{
				continue;
			}
			gameObject2 = UnityEngine.Object.Instantiate(gameObject, new Vector3(vector.x, hitInfo.point.y, vector.z), Quaternion.Euler(gameObject.transform.rotation.x, base.transform.eulerAngles.y, gameObject.transform.rotation.z));
			PeopleController peopleController = gameObject2.AddComponent<PeopleController>();
			spawnPoints.Add(gameObject2.transform.position);
			if (target != null)
			{
				peopleController.SetTarget(target.transform.position);
				if (looking)
				{
					peopleController.target = target.transform;
					peopleController.damping = damping;
				}
			}
			peopleController.animNames = new string[4] { "idle1", "idle2", "cheer", "claphands" };
			gameObject2.transform.parent = par.transform;
		}
	}

	private Vector3 RandomRectanglePosition()
	{
		Vector3 vector = new Vector3(0f, 0f, 0f);
		for (int i = 0; i < 10; i++)
		{
			vector.x = surface.transform.position.x - GetRealPlaneSize().x / 2f + UnityEngine.Random.Range(0f, GetRealPlaneSize().x - 0.6f);
			vector.z = surface.transform.position.z - GetRealPlaneSize().y / 2f + UnityEngine.Random.Range(0f, GetRealPlaneSize().y - 0.6f);
			vector.y = surface.transform.position.y;
			if (IsRandomPositionFree(vector))
			{
				return vector;
			}
		}
		return Vector3.zero;
	}

	private Vector3 RandomCirclePosition()
	{
		Vector3 position = surface.transform.position;
		float num = GetRealPlaneSize().x / 2f;
		Vector3 vector = default(Vector3);
		for (int i = 0; i < 10; i++)
		{
			float num2 = UnityEngine.Random.value * num;
			float num3 = UnityEngine.Random.value * 360f;
			vector.x = position.x + num2 * Mathf.Sin(num3 * (MathF.PI / 180f));
			vector.y = position.y;
			vector.z = position.z + num2 * Mathf.Cos(num3 * (MathF.PI / 180f));
			if (IsRandomPositionFree(vector))
			{
				return vector;
			}
		}
		return Vector3.zero;
	}

	private bool IsRandomPositionFree(Vector3 pos)
	{
		for (int i = 0; i < spawnPoints.Count; i++)
		{
			if (spawnPoints[i].x - 0.6f < pos.x && spawnPoints[i].x + 1f > pos.x && spawnPoints[i].z - 0.5f < pos.z && spawnPoints[i].z + 0.6f > pos.z)
			{
				return false;
			}
		}
		return true;
	}

	private Vector2 GetRealPlaneSize()
	{
		Vector3 size = surface.GetComponent<MeshRenderer>().bounds.size;
		return new Vector2(size.x, size.z);
	}

	private Vector2 GetRealPeopleModelSize()
	{
		Vector3 size = peoplePrefabs[1].GetComponent<MeshRenderer>().bounds.size;
		return new Vector2(size.x, size.z);
	}
}
