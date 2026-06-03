using System;
using System.Collections.Generic;
using PixelPlay.OffScreenIndicator;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class OffScreenIndicator : MonoBehaviour
{
	[Range(0.5f, 0.9f)]
	[Tooltip("Distance offset of the indicators from the centre of the screen")]
	[SerializeField]
	private float screenBoundOffset = 0.9f;

	private Camera mainCamera;

	private Vector3 screenCentre;

	private Vector3 screenBounds;

	private List<Target> targets = new List<Target>();

	public static Action<Target, bool> TargetStateChanged;

	private void Awake()
	{
		mainCamera = Camera.main;
		screenCentre = new Vector3(Screen.width, Screen.height, 0f) / 2f;
		screenBounds = screenCentre * screenBoundOffset;
		TargetStateChanged = (Action<Target, bool>)Delegate.Combine(TargetStateChanged, new Action<Target, bool>(HandleTargetStateChanged));
	}

	private void LateUpdate()
	{
		DrawIndicators();
	}

	private void DrawIndicators()
	{
		foreach (Target target in targets)
		{
			Vector3 screenPosition = OffScreenIndicatorCore.GetScreenPosition(mainCamera, target.transform.position);
			bool flag = OffScreenIndicatorCore.IsTargetVisible(screenPosition);
			float distanceText = (target.NeedDistanceText ? target.GetDistanceFromCamera(mainCamera.transform.position) : float.MinValue);
			Indicator indicator = null;
			if (target.NeedBoxIndicator && flag)
			{
				screenPosition.z = 0f;
				indicator = GetIndicator(ref target.indicator, IndicatorType.BOX);
			}
			else if (target.NeedArrowIndicator && !flag)
			{
				float angle = float.MinValue;
				OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref screenPosition, ref angle, screenCentre, screenBounds);
				indicator = GetIndicator(ref target.indicator, IndicatorType.ARROW);
				indicator.transform.rotation = Quaternion.Euler(0f, 0f, angle * 57.29578f);
			}
			if ((bool)indicator)
			{
				indicator.SetImageColor(target.TargetColor);
				indicator.SetDistanceText(distanceText);
				indicator.transform.position = screenPosition;
				indicator.SetTextRotation(Quaternion.identity);
			}
		}
	}

	private void HandleTargetStateChanged(Target target, bool active)
	{
		if (active)
		{
			targets.Add(target);
			return;
		}
		target.indicator?.Activate(value: false);
		target.indicator = null;
		targets.Remove(target);
	}

	private Indicator GetIndicator(ref Indicator indicator, IndicatorType type)
	{
		if (indicator != null)
		{
			if (indicator.Type != type)
			{
				indicator.Activate(value: false);
				indicator = ((type == IndicatorType.BOX) ? BoxObjectPool.current.GetPooledObject() : ArrowObjectPool.current.GetPooledObject());
				indicator.Activate(value: true);
			}
		}
		else
		{
			indicator = ((type == IndicatorType.BOX) ? BoxObjectPool.current.GetPooledObject() : ArrowObjectPool.current.GetPooledObject());
			indicator.Activate(value: true);
		}
		return indicator;
	}

	private void OnDestroy()
	{
		TargetStateChanged = (Action<Target, bool>)Delegate.Remove(TargetStateChanged, new Action<Target, bool>(HandleTargetStateChanged));
	}
}
