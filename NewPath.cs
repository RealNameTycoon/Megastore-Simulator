using System.Collections.Generic;
using UnityEngine;

public class NewPath : MonoBehaviour
{
	private List<Vector3> points = new List<Vector3>();

	public int pointLenght;

	public Vector3 mousePos;

	public string pathName;

	public bool errors;

	public bool exit;

	public GameObject par;

	[HideInInspector]
	[SerializeField]
	public PathType PathType;

	public List<Vector3> PointsGet()
	{
		return points;
	}

	public void PointSet(int index, Vector3 pos)
	{
		points.Add(pos);
		if (par == null)
		{
			par = new GameObject();
			par.name = "New path points";
			par.transform.parent = base.gameObject.transform;
		}
		GameObject obj = Object.Instantiate(GameObject.Find("Population System").GetComponent<PopulationSystemManager>().pointPrefab, pos, Quaternion.identity);
		obj.name = "p" + index;
		obj.transform.parent = par.transform;
	}
}
