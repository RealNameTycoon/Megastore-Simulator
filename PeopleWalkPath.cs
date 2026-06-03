using System;
using System.Collections.Generic;
using UnityEngine;

public class PeopleWalkPath : WalkPath
{
	public enum EnumMove
	{
		Walk,
		Run
	}

	public enum EnumDir
	{
		Forward,
		Backward,
		HugLeft,
		HugRight,
		WeaveLeft,
		WeaveRight
	}

	[HideInInspector]
	[Tooltip("Type of movement / Тип движения")]
	[SerializeField]
	private EnumMove _moveType;

	[Tooltip("Direction of movement / Направление движения. Левостороннее, правостороннее, итд.")]
	[SerializeField]
	private EnumDir direction;

	[HideInInspector]
	[Tooltip("Speed of walk / Скорость ходьбы")]
	[SerializeField]
	private float walkSpeed = 1f;

	[HideInInspector]
	[Tooltip("Speed of run / Скорость бега")]
	[SerializeField]
	private float runSpeed = 4f;

	[HideInInspector]
	public bool isWalk;

	[HideInInspector]
	[SerializeField]
	[Tooltip("Set your animation speed? / Установить свою скорость анимации?")]
	private bool _overrideDefaultAnimationMultiplier = true;

	[HideInInspector]
	[SerializeField]
	[Tooltip("Speed animation of walking / Скорость анимации ходьбы")]
	private float _customWalkAnimationMultiplier = 1.1f;

	[HideInInspector]
	[SerializeField]
	[Tooltip("Running animation speed / Скорость анимации бега")]
	private float _customRunAnimationMultiplier = 0.3f;

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
		isWalk = _moveType.ToString() == "Walk";
		for (int i = 0; i < numberOfWays; i++)
		{
			if (direction.ToString() == "Forward")
			{
				_forward[i] = true;
			}
			else if (direction.ToString() == "Backward")
			{
				_forward[i] = false;
			}
			else if (direction.ToString() == "HugLeft")
			{
				if ((i + 2) % 2 == 0)
				{
					_forward[i] = true;
				}
				else
				{
					_forward[i] = false;
				}
			}
			else if (direction.ToString() == "HugRight")
			{
				if ((i + 2) % 2 == 0)
				{
					_forward[i] = false;
				}
				else
				{
					_forward[i] = true;
				}
			}
			else if (direction.ToString() == "WeaveLeft")
			{
				if (i == 1 || i == 2 || (i - 1) % 4 == 0 || (i - 2) % 4 == 0)
				{
					_forward[i] = false;
				}
				else
				{
					_forward[i] = true;
				}
			}
			else if (direction.ToString() == "WeaveRight")
			{
				if (i == 1 || i == 2 || (i - 1) % 4 == 0 || (i - 2) % 4 == 0)
				{
					_forward[i] = true;
				}
				else
				{
					_forward[i] = false;
				}
			}
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

	public override void SpawnOnePeople(int w, bool forward, float walkSpeed, float runSpeed)
	{
		int num = UnityEngine.Random.Range(0, peoplePrefabs.Length);
		GameObject gameObject = base.gameObject;
		gameObject = (forward ? UnityEngine.Object.Instantiate(peoplePrefabs[num], points[w, 1], Quaternion.identity) : UnityEngine.Object.Instantiate(peoplePrefabs[num], points[w, pointLength[0] - 2], Quaternion.identity));
		MovePath movePath = gameObject.AddComponent<MovePath>();
		movePath.randXFinish = UnityEngine.Random.Range(0f - randXPos, randXPos);
		movePath.randZFinish = UnityEngine.Random.Range(0f - randZPos, randZPos);
		gameObject.transform.parent = par.transform;
		movePath.walkPath = base.gameObject;
		string anim = ((!isWalk) ? "run" : "walk");
		movePath.InitializeAnimation(_overrideDefaultAnimationMultiplier, _customWalkAnimationMultiplier, _customRunAnimationMultiplier);
		if (!forward)
		{
			movePath.MyStart(w, pointLength[0] - 2, anim, loopPath, forward, walkSpeed, runSpeed);
			gameObject.transform.LookAt(points[w, pointLength[0] - 3]);
		}
		else
		{
			movePath.MyStart(w, 1, anim, loopPath, forward, walkSpeed, runSpeed);
			gameObject.transform.LookAt(points[w, 2]);
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
			par.name = "people";
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
			bool forward = false;
			switch (direction.ToString())
			{
			case "Forward":
				forward = true;
				break;
			case "Backward":
				forward = false;
				break;
			case "HugLeft":
				forward = (i + 2) % 2 == 0;
				break;
			case "HugRight":
				forward = (i + 2) % 2 != 0;
				break;
			case "WeaveLeft":
				forward = i != 1 && i != 2 && (i - 1) % 4 != 0 && (i - 2) % 4 != 0;
				break;
			case "WeaveRight":
				forward = i == 1 || i == 2 || (i - 1) % 4 == 0 || (i - 2) % 4 == 0;
				break;
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
				float num8 = UnityEngine.Random.Range(0f - randXPos, randXPos);
				float num9 = UnityEngine.Random.Range(0f - randZPos, randZPos);
				routePosition = new Vector3(routePosition.x + num8, routePosition.y, routePosition.z + num9);
				Vector3 origin = new Vector3(routePosition.x, routePosition.y + 10000f, routePosition.z);
				RaycastHit[] array2 = Physics.RaycastAll(origin, Vector3.down, float.PositiveInfinity);
				float num10 = 0f;
				int num11 = 0;
				origin = new Vector3(routePosition.x, routePosition.y + 10000f, routePosition.z);
				array2 = Physics.RaycastAll(origin, Vector3.down, float.PositiveInfinity);
				for (int m = 0; m < array2.Length; m++)
				{
					if (num10 < Vector3.Distance(array2[0].point, origin))
					{
						num11 = m;
						num10 = Vector3.Distance(array2[0].point, origin);
					}
				}
				if (array2.Length != 0)
				{
					routePosition.y = array2[num11].point.y;
				}
				gameObject = UnityEngine.Object.Instantiate(peoplePrefabs[randomPrefabIndexes[l]], routePosition, Quaternion.identity);
				MovePath movePath = gameObject.AddComponent<MovePath>();
				movePath.randXFinish = num8;
				movePath.randZFinish = num9;
				gameObject.transform.parent = par.transform;
				movePath.walkPath = base.gameObject;
				movePath.MyStart(anim: (!isWalk) ? "run" : "walk", _w: i, _i: GetRoutePoint((float)(l + 1) * num6 + num7, i, num3, forward, loopPath), _loop: loopPath, _forward: forward, _walkSpeed: walkSpeed, _runSpeed: runSpeed);
				movePath.InitializeAnimation(_overrideDefaultAnimationMultiplier, _customWalkAnimationMultiplier, _customRunAnimationMultiplier);
				movePath.SetLookPosition();
			}
		}
	}
}
