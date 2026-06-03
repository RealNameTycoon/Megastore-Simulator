using System;
using System.Collections.Generic;
using UnityEngine;

public class AudiencePath : WalkPath
{
	public enum Angle
	{
		zero,
		minus90,
		plus90
	}

	[Tooltip("Type of rotation / Вариант поворота")]
	[SerializeField]
	private Angle angle = Angle.plus90;

	[Range(-180f, 180f)]
	[Tooltip("Rotation of people / Поворот человека")]
	[SerializeField]
	private float peopleRotation;

	[Tooltip("Look for target / Слежение за таргетом")]
	[HideInInspector]
	[SerializeField]
	private bool looking;

	[Tooltip("Target / Цель")]
	[HideInInspector]
	[SerializeField]
	private Transform target;

	[Tooltip("Speed rotation (smooth) / Скорость поворота (смягчение)")]
	[HideInInspector]
	[SerializeField]
	private float damping = 5f;

	public override void DrawCurved(bool withDraw)
	{
		if (numberOfWays < 1)
		{
			numberOfWays = 1;
		}
		if (lineSpacing < 0.6f)
		{
			lineSpacing = 0.6f;
		}
		_forward = new bool[numberOfWays];
		for (int i = 0; i < numberOfWays; i++)
		{
			_forward[i] = true;
		}
		if (pathPoint.Count < 2)
		{
			return;
		}
		points = new Vector3[numberOfWays, pathPoint.Count + 2];
		pointLength[0] = pathPoint.Count + 2;
		for (int j = 0; j < pathPointTransform.Count; j++)
		{
			Vector3 vector;
			Vector3 vector2;
			if (j == 0)
			{
				vector = ((!loopPath) ? Vector3.zero : (pathPointTransform[pathPointTransform.Count - 1].transform.position - pathPointTransform[j].transform.position));
				vector2 = pathPointTransform[j].transform.position - pathPointTransform[j + 1].transform.position;
			}
			else if (j == pathPointTransform.Count - 1)
			{
				vector = pathPointTransform[j - 1].transform.position - pathPointTransform[j].transform.position;
				vector2 = ((!loopPath) ? Vector3.zero : (pathPointTransform[j].transform.position - pathPointTransform[0].transform.position));
			}
			else
			{
				vector = pathPointTransform[j - 1].transform.position - pathPointTransform[j].transform.position;
				vector2 = pathPointTransform[j].transform.position - pathPointTransform[j + 1].transform.position;
			}
			Vector3 vector3 = Vector3.Normalize(Quaternion.Euler(0f, 90f, 0f) * (vector + vector2));
			points[0, j + 1] = ((numberOfWays % 2 == 1) ? pathPointTransform[j].transform.position : (pathPointTransform[j].transform.position + vector3 * lineSpacing / 2f));
			if (numberOfWays > 1)
			{
				points[1, j + 1] = points[0, j + 1] - vector3 * lineSpacing;
			}
			for (int k = 1; k < numberOfWays; k++)
			{
				points[k, j + 1] = points[0, j + 1] + vector3 * lineSpacing * (float)Math.Pow(-1.0, k) * ((k + 1) / 2);
			}
		}
		for (int l = 0; l < numberOfWays; l++)
		{
			points[l, 0] = points[l, 1];
			points[l, pointLength[0] - 1] = points[l, pointLength[0] - 2];
		}
		if (!withDraw)
		{
			return;
		}
		for (int m = 0; m < numberOfWays; m++)
		{
			if (loopPath)
			{
				Gizmos.color = (_forward[m] ? Color.green : Color.red);
				Gizmos.DrawLine(points[m, 0], points[m, pathPoint.Count]);
			}
			for (int n = 1; n < pathPoint.Count; n++)
			{
				Gizmos.color = (_forward[m] ? Color.green : Color.red);
				Gizmos.DrawLine(points[m, n + 1], points[m, n]);
			}
		}
	}

	public override void SpawnPeople()
	{
		List<GameObject> list = new List<GameObject>(peoplePrefabs);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == null)
			{
				list.RemoveAt(num);
			}
		}
		peoplePrefabs = list.ToArray();
		if (points == null)
		{
			DrawCurved(withDraw: false);
		}
		if (par == null)
		{
			par = new GameObject();
			par.transform.parent = base.gameObject.transform;
			par.name = "walkingObjects";
		}
		int num2 = (loopPath ? (pointLength[0] - 1) : (pointLength[0] - 2));
		if (num2 < 2)
		{
			return;
		}
		int num3 = (loopPath ? (pointLength[0] - 1) : (pointLength[0] - 2));
		for (int i = 0; i < numberOfWays; i++)
		{
			_distances = new float[num3];
			float num4 = 0f;
			for (int j = 1; j < num3; j++)
			{
				num4 += ((!loopPath || j != num3 - 1) ? (points[i, j + 1] - points[i, j]) : (points[i, 1] - points[i, num3])).magnitude;
				_distances[j] = num4;
			}
			int num5 = Mathf.FloorToInt(Density * num4 / _minimalObjectLength);
			float num6 = _minimalObjectLength + (num4 - (float)num5 * _minimalObjectLength) / (float)num5;
			int[] randomPrefabIndexes = CommonUtils.GetRandomPrefabIndexes(num5, ref peoplePrefabs);
			Vector3[] array = new Vector3[_distances.Length];
			for (int k = 1; k < _distances.Length; k++)
			{
				array[k - 1] = points[i, k];
			}
			array[_distances.Length - 1] = (loopPath ? points[i, 1] : points[i, _distances.Length]);
			for (int l = 0; l < num5; l++)
			{
				GameObject gameObject = base.gameObject;
				float num7 = UnityEngine.Random.Range((0f - num6) / 3f, num6 / 3f) + (float)i * num6;
				float distance = (float)(l + 1) * num6 + num7;
				Vector3 routePosition = GetRoutePosition(array, distance, num3, loopPath);
				routePosition = new Vector3(routePosition.x, routePosition.y, routePosition.z);
				if (Physics.Raycast(new Vector3(routePosition.x, routePosition.y + highToSpawn, routePosition.z), Vector3.down, out var hitInfo, float.PositiveInfinity))
				{
					routePosition.y = hitInfo.point.y;
					gameObject = UnityEngine.Object.Instantiate(peoplePrefabs[randomPrefabIndexes[l]], routePosition, Quaternion.identity);
					gameObject.transform.parent = par.transform;
					PeopleController peopleController = gameObject.AddComponent<PeopleController>();
					peopleController.animNames = new string[4] { "idle1", "idle2", "cheer", "claphands" };
					if (looking)
					{
						peopleController.target = target;
						peopleController.damping = damping;
					}
					MovePath movePath = gameObject.AddComponent<MovePath>();
					movePath.walkPath = base.gameObject;
					movePath.MyStart(i, GetRoutePoint((float)(l + 1) * num6 + num7, i, num3, forward: true, loopPath), "", loopPath, _forward: true, 0f, 0f);
					Vector3 worldPosition = new Vector3(movePath.finishPos.x, gameObject.transform.position.y, movePath.finishPos.z);
					UnityEngine.Object.DestroyImmediate(movePath);
					gameObject.transform.LookAt(worldPosition);
					if (angle == Angle.zero)
					{
						gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y + peopleRotation, gameObject.transform.eulerAngles.z);
					}
					else
					{
						gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y + (float)((angle == Angle.plus90) ? 90 : (-90)) + peopleRotation, gameObject.transform.eulerAngles.z);
					}
					gameObject.transform.position += gameObject.transform.forward * UnityEngine.Random.Range(0f - randZPos, randZPos);
					gameObject.transform.position += gameObject.transform.right * UnityEngine.Random.Range(0f - randXPos, randXPos);
					if (Physics.Raycast(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + highToSpawn, gameObject.transform.position.z), Vector3.down, out hitInfo, float.PositiveInfinity))
					{
						gameObject.transform.position = new Vector3(gameObject.transform.position.x, hitInfo.point.y, gameObject.transform.position.z);
					}
				}
			}
		}
	}
}
