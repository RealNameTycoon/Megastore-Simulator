using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WalkPath : MonoBehaviour
{
	[Tooltip("Objects of motion / Объекты движения")]
	public GameObject[] peoplePrefabs;

	[Tooltip("Number of paths / Количество путей")]
	public int numberOfWays;

	[Tooltip("Space between paths / Пространство между путями")]
	public float lineSpacing;

	[Tooltip("Density of movement of objects / Плотность движения объектов")]
	[Range(0.01f, 0.5f)]
	public float Density = 0.2f;

	[Tooltip("Distance between objects / Дистанция между объектами")]
	[Range(1f, 10f)]
	public float _minimalObjectLength = 1f;

	[Tooltip("Make the path closed in the ring / Сделать путь замкнутым в кольцо")]
	public bool loopPath;

	protected float[] _distances;

	[HideInInspector]
	public List<Vector3> pathPoint = new List<Vector3>();

	[HideInInspector]
	public List<GameObject> pathPointTransform = new List<GameObject>();

	[HideInInspector]
	public Vector3[,] points;

	[HideInInspector]
	public List<Vector3> CalcPoint = new List<Vector3>();

	[HideInInspector]
	public int[] pointLength = new int[10];

	[HideInInspector]
	public bool disableLineDraw;

	[HideInInspector]
	public bool[] _forward;

	[HideInInspector]
	public GameObject par;

	[HideInInspector]
	public PathType pathType;

	[Tooltip("Radius of the sphere-scraper [m] / Радиус сферы-стёрки [м]")]
	[Range(0.1f, 25f)]
	public float eraseRadius = 2f;

	[Tooltip("The minimum distance from the cursor to the line at which you can add a new point to the path [m] / Минимальное расстояние от курсора до линии, при котором можно добавить новую точку в путь [м]")]
	[Range(0.5f, 10f)]
	public float addPointDistance = 2f;

	[Tooltip("Adjust the spawn of cars to the nearest surface. This option will be useful if there are bridges in the scene / Регулировка спавна людей к ближайшей поверхности. Этот параметор будет полезен, если в сцене есть мосты.")]
	public float highToSpawn = 1f;

	[Range(0f, 5f)]
	[Tooltip("Offset from the line along the X axis / Смещение от линии по оси X")]
	public float randXPos = 0.1f;

	[Range(0f, 5f)]
	[Tooltip("Offset from the line along the Z axis / Смещение от линии по оси Z")]
	public float randZPos = 0.1f;

	[HideInInspector]
	public bool newPointCreation;

	[HideInInspector]
	public bool oldPointDeleting;

	[HideInInspector]
	public Vector3 mousePosition = Vector3.zero;

	private int deletePointIndex = -1;

	private int firstPointIndex = -1;

	private int secondPointIndex = -1;

	public Vector3 getNextPoint(int w, int index)
	{
		return points[w, index];
	}

	public Vector3 getStartPoint(int w)
	{
		return points[w, 1];
	}

	public int getPointsTotal(int w)
	{
		return pointLength[w];
	}

	private void Awake()
	{
		DrawCurved(withDraw: false);
	}

	public virtual void SpawnOnePeople(int w, bool forward, float walkSpeed, float runSpeed)
	{
	}

	public virtual void SpawnPeople()
	{
	}

	public virtual void DrawCurved(bool withDraw)
	{
	}

	protected Vector3 GetRoutePosition(Vector3[] pointArray, float distance, int pointCount, bool loopPath)
	{
		int i = 0;
		float length = _distances[_distances.Length - 1];
		for (distance = Mathf.Repeat(distance, length); _distances[i] < distance; i++)
		{
		}
		int num = (i - 1 + pointCount) % pointCount;
		int num2 = i;
		float t = Mathf.InverseLerp(_distances[num], _distances[num2], distance);
		return Vector3.Lerp(pointArray[num], pointArray[num2], t);
	}

	protected int GetRoutePoint(float distance, int wayIndex, int pointCount, bool forward, bool loopPath)
	{
		int i = 0;
		float length = _distances[_distances.Length - 1];
		for (distance = Mathf.Repeat(distance, length); _distances[i] < distance; i++)
		{
		}
		return i;
	}

	private bool PointWithSphereCollision(Vector3 colisionSpherePosition, Vector3 pointPosition)
	{
		return Vector3.Magnitude(colisionSpherePosition - pointPosition) < eraseRadius;
	}

	private bool PointWithLineCollision(Vector3 lineStartPosition, Vector3 lineEndPosition, Vector3 pointPosition)
	{
		return Distance(lineStartPosition, lineEndPosition, pointPosition) < addPointDistance;
	}

	private float Distance(Vector3 lineStartPosition, Vector3 lineEndPosition, Vector3 pointPosition)
	{
		float num = Vector3.SqrMagnitude(lineEndPosition - lineStartPosition);
		if (num == 0f)
		{
			return Vector3.Distance(pointPosition, lineStartPosition);
		}
		float num2 = Mathf.Max(0f, Mathf.Min(1f, Vector3.Dot(pointPosition - lineStartPosition, lineEndPosition - lineStartPosition) / num));
		Vector3 b = lineStartPosition + num2 * (lineEndPosition - lineStartPosition);
		return Vector3.Distance(pointPosition, b);
	}

	public void AddPoint()
	{
		if (firstPointIndex != -1 || secondPointIndex != firstPointIndex)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(GameObject.Find("Population System").GetComponent<PopulationSystemManager>().pointPrefab, mousePosition, Quaternion.identity);
			gameObject.name = "p+";
			gameObject.transform.parent = pathPointTransform[firstPointIndex].transform.parent;
			pathPointTransform.Insert(firstPointIndex + 1, gameObject);
			pathPoint.Insert(firstPointIndex + 1, gameObject.transform.position);
		}
	}

	public void DeletePoint()
	{
		if (deletePointIndex != -1)
		{
			UnityEngine.Object.DestroyImmediate(pathPointTransform[deletePointIndex]);
			pathPointTransform.RemoveAt(deletePointIndex);
			pathPoint.RemoveAt(deletePointIndex);
		}
	}
}
