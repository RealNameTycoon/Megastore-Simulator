using UnityEngine;

public class Rotate : MonoBehaviour
{
	public float speedRotate = 3f;

	public bool rotate;

	public void SetRotate(bool rotate)
	{
		this.rotate = rotate;
	}

	private void Update()
	{
		if (rotate && Time.timeScale != 0f)
		{
			base.transform.Rotate(Vector3.up * speedRotate);
		}
	}
}
