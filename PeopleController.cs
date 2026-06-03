using UnityEngine;

public class PeopleController : MonoBehaviour
{
	[HideInInspector]
	public float timer;

	[HideInInspector]
	public string[] animNames;

	[HideInInspector]
	public float damping;

	[HideInInspector]
	public Transform target;

	private void Start()
	{
		Tick();
	}

	private void Tick()
	{
		timer = 0f;
		int num = Random.Range(0, animNames.Length);
		SetAnimClip(animNames[num]);
		timer = Random.Range(3f, 5f);
	}

	public void SetTarget(Vector3 _target)
	{
		Vector3 worldPosition = new Vector3(_target.x, base.transform.position.y, _target.z);
		base.transform.LookAt(worldPosition);
	}

	private void Update()
	{
		if (timer >= 0f)
		{
			timer -= Time.deltaTime;
		}
		else
		{
			Tick();
		}
		if (target != null)
		{
			Vector3 forward = target.position - base.transform.position;
			forward.y = 0f;
			Quaternion b = Quaternion.LookRotation(forward);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * damping);
		}
	}

	public void SetAnimClip(string animName)
	{
		GetComponent<Animator>().CrossFade(animName, 0.1f, 0, Random.Range(0f, 1f));
	}
}
