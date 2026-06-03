using UnityEngine;
using UnityEngine.EventSystems;

public class VariableJoystick : Joystick
{
	[SerializeField]
	private float moveThreshold = 1f;

	[SerializeField]
	private JoystickType joystickType;

	private Vector2 fixedPosition = Vector2.zero;

	private Vector3 startPosition;

	public float MoveThreshold
	{
		get
		{
			return moveThreshold;
		}
		set
		{
			moveThreshold = Mathf.Abs(value);
		}
	}

	public void SetMode(JoystickType joystickType)
	{
		this.joystickType = joystickType;
		startPosition = background.anchoredPosition;
		if (joystickType == JoystickType.Fixed)
		{
			background.anchoredPosition = fixedPosition;
		}
	}

	protected override void Start()
	{
		base.Start();
		fixedPosition = background.anchoredPosition;
		SetMode(joystickType);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (joystickType != JoystickType.Fixed)
		{
			background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
		}
		base.OnPointerDown(eventData);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		background.anchoredPosition = startPosition;
		base.OnPointerUp(eventData);
	}

	protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
	{
		if (joystickType == JoystickType.Dynamic && magnitude > moveThreshold)
		{
			Vector2 vector = normalised * (magnitude - moveThreshold) * radius;
			background.anchoredPosition += vector;
		}
		base.HandleInput(magnitude, normalised, radius, cam);
	}
}
