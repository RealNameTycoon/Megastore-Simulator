using System;
using DG.Tweening;
using UnityEngine;

public class CookableProduct : Product
{
	[SerializeField]
	private Transform scaleParent;

	[SerializeField]
	private Renderer[] renderers;

	[SerializeField]
	private ParticleSystem particleSystem;

	private Color cookedColor = new Color(0.5019608f, 0.34509805f, 0.23921569f);

	private Color rawColor = new Color(0.5019608f, 0.34509805f, 0.23921569f);

	public static float COOK_DURATION = 12f;

	private Vector3 rawScale = new Vector3(0.8f, 0.3943f, 0.8f);

	private Tween cookTween;

	public void AssignRenderers()
	{
		renderers = GetComponentsInChildren<Renderer>();
		if (renderers.Length == 0)
		{
			Debug.LogError("No renderers found in children of " + base.gameObject.name);
		}
	}

	public void StartCooking(Action<CookableProduct> onComplete = null)
	{
		if (particleSystem != null)
		{
			particleSystem.Play();
		}
		float cookProgress = 0f;
		cookTween = DOTween.To(() => cookProgress, delegate(float x)
		{
			SetCookProgress(x);
			cookProgress = x;
		}, 1f, COOK_DURATION).SetEase(Ease.Linear);
		scaleParent.DOScale(Vector3.one, COOK_DURATION).SetEase(Ease.Linear).OnComplete(delegate
		{
			if (particleSystem != null)
			{
				particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			onComplete?.Invoke(this);
		});
	}

	public void FinishCookingInstant()
	{
		if (cookTween != null)
		{
			cookTween.Kill();
		}
		CookInstantly();
	}

	public void CookInstantly()
	{
		SetCookProgress(1f);
		scaleParent.localScale = Vector3.one;
	}

	public void TurnRaw()
	{
		SetCookProgress(0f);
		scaleParent.localScale = rawScale;
	}

	public override void ResetProduct()
	{
		TurnRaw();
	}

	private void SetCookProgress(float amount)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			renderers[i].GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat("_CookProgress", amount);
			renderers[i].SetPropertyBlock(materialPropertyBlock);
		}
	}
}
