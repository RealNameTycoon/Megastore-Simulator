using UnityEngine;
using UnityEngine.UI;

public class PrologueWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button wishlistButton;

	private bool wasCursorLocked;

	private bool wasMovementLocked;

	private void Awake()
	{
		closeButton.onClick.AddListener(Close);
		wishlistButton.onClick.AddListener(OnWishlistNow);
	}

	public bool IsOpen()
	{
		return canvas.enabled;
	}

	public void Open()
	{
		canvas.enabled = true;
		wasCursorLocked = !SingletonBehaviour<PlayerLook>.Instance.RotationLocked;
		wasMovementLocked = SingletonBehaviour<PlayerMove>.Instance.MovementLocked;
		if (wasCursorLocked)
		{
			SingletonBehaviour<PlayerLook>.Instance.LockCursor(!wasCursorLocked);
		}
		if (!wasMovementLocked)
		{
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
		}
	}

	public void Close()
	{
		canvas.enabled = false;
		if (wasCursorLocked)
		{
			SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
		}
		if (!wasMovementLocked)
		{
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
		}
	}

	public void OnWishlistNow()
	{
		Application.OpenURL("steam://store/3819640");
		Close();
	}
}
