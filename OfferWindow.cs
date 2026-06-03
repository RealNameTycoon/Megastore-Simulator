using DG.Tweening;
using UnityEngine;

public class OfferWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Transform content;

	[SerializeField]
	private Transform contentTarget;

	private Vector3 initialPosition;

	private void Start()
	{
		initialPosition = content.position;
	}

	public bool IsOpen()
	{
		return canvas.enabled;
	}

	public void Open()
	{
		content.position = initialPosition;
		content.DOMove(contentTarget.position, 0.2f);
		canvas.enabled = true;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!canvas.enabled);
	}

	public void Close()
	{
		canvas.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!canvas.enabled);
		content.position = initialPosition;
	}

	private void Update()
	{
	}
}
