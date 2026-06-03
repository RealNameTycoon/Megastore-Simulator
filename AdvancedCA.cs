using UnityEngine;

[ExecuteInEditMode]
public class AdvancedCA : MonoBehaviour
{
	public enum ColorSet
	{
		RedBlue,
		RedGreen
	}

	private Shader curShader;

	public float DispersionAmount = 1f;

	public ColorSet Colors;

	private Material curMaterial;

	private Material material
	{
		get
		{
			if (curMaterial == null)
			{
				curMaterial = new Material(curShader);
				curMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return curMaterial;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else
		{
			curShader = Shader.Find("Custom/AdvancedCA");
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (curShader != null)
		{
			material.SetFloat("_Amount", DispersionAmount);
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}

	private void Update()
	{
		if (Colors == ColorSet.RedBlue)
		{
			Shader.EnableKeyword("REDBLUE");
			Shader.DisableKeyword("REDGREEN");
		}
		else if (ColorSet.RedGreen == Colors)
		{
			Shader.EnableKeyword("REDGREEN");
			Shader.DisableKeyword("REDBLUE");
		}
	}

	private void OnDisable()
	{
		if ((bool)curMaterial)
		{
			Object.DestroyImmediate(curMaterial);
		}
	}
}
