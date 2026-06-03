using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
	private Vector3 startPosition;

	protected override void Start()
	{
		base.Start();
		startPosition = background.anchoredPosition;
		background.gameObject.SetActive(value: false);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
		base.OnPointerDown(eventData);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		background.anchoredPosition = startPosition;
		base.OnPointerUp(eventData);
	}
}
