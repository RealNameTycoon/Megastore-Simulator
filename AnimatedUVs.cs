using UnityEngine;

public class AnimatedUVs : MonoBehaviour
{
	public float speedY = 0.5f;

	public float speedx;

	private float offsety;

	private float offsetx;

	private Renderer rend;

	private void Start()
	{
		rend = GetComponent<Renderer>();
	}

	private void Update()
	{
		offsety += Time.deltaTime * speedY;
		offsetx += Time.deltaTime * speedx;
		rend.material.SetTextureOffset("_MainTex", new Vector2(offsetx, offsety));
	}
}
