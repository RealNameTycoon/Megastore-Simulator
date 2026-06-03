using UnityEngine;

public class tazo_UVScroller : MonoBehaviour
{
	public int targetMaterialSlot;

	private Renderer myrender;

	public float speedY = 0.5f;

	public float speedX;

	private float timeWentX;

	private float timeWentY;

	private void Start()
	{
		myrender = GetComponent<Renderer>();
	}

	private void Update()
	{
		timeWentY += Time.deltaTime * speedY;
		timeWentX += Time.deltaTime * speedX;
		myrender.material.SetTextureOffset("_MainTex", new Vector2(timeWentX, timeWentY));
	}
}
