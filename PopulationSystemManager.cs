using UnityEngine;

public class PopulationSystemManager : MonoBehaviour
{
	[SerializeField]
	private GameObject planePrefab;

	[SerializeField]
	private GameObject circlePrefab;

	public GameObject pointPrefab;

	[HideInInspector]
	public bool isConcert;

	[HideInInspector]
	public bool isStreet;

	[HideInInspector]
	public Vector3 mousePos;

	public void Concert(Vector3 pos)
	{
		isConcert = false;
		GameObject obj = new GameObject();
		obj.transform.position = pos;
		obj.name = "Audience";
		obj.AddComponent<StandingPeopleConcert>();
		StandingPeopleConcert component = obj.GetComponent<StandingPeopleConcert>();
		component.planePrefab = planePrefab;
		component.circlePrefab = circlePrefab;
		component.SpawnRectangleSurface();
	}

	public void Street(Vector3 pos)
	{
		isStreet = false;
		GameObject obj = new GameObject();
		obj.transform.position = pos;
		obj.name = "Talking people";
		obj.AddComponent<StandingPeopleStreet>();
		StandingPeopleStreet component = obj.GetComponent<StandingPeopleStreet>();
		component.planePrefab = planePrefab;
		component.circlePrefab = circlePrefab;
		component.SpawnRectangleSurface();
	}
}
