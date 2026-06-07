using DG.Tweening;
using UnityEngine;

public class InflatableProduct : Product
{
	[SerializeField]
	private Transform scaleParent;

	public static float INFLATE_DURATION = 0.6f;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void OnBeforePlace(PlaceableType placeableType, bool isStart)
	{
		base.OnBeforePlace(placeableType, isStart);
		scaleParent.transform.DOKill();
		scaleParent.transform.DOScale(base.Data.inflatedScale, INFLATE_DURATION).SetEase(Ease.Linear);
	}

	public override void OnRemovedFromShelf()
	{
		base.OnRemovedFromShelf();
		scaleParent.transform.DOKill();
		scaleParent.transform.DOScale(base.Data.deflatedScale, INFLATE_DURATION).SetEase(Ease.Linear);
	}

	public void InflateInstant()
	{
		scaleParent.localScale = base.Data.inflatedScale;
	}

	public void DeflateInstant()
	{
		scaleParent.localScale = base.Data.deflatedScale;
	}
}
