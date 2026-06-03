using UnityEngine;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour
{
	private Transform _tr;

	private Vector3 curPos;

	private void Awake()
	{
		curPos = base.transform.position;
		_tr = base.transform;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			SceneManager.LoadScene("level1");
		}
	}
}
