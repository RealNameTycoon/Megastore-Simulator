using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefundWindow : SingletonWindow<RefundWindow>
{
	[SerializeField]
	private TextMeshProUGUI refundAmountText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Button closeButton;

	private bool wasCursorLocked;

	private bool wasMovementLocked;

	private new void Awake()
	{
		base.Awake();
		closeButton.onClick.AddListener(Close);
	}

	public override bool IsOpen()
	{
		return canvas.enabled;
	}

	public void Open(string refundAmount)
	{
		refundAmountText.text = refundAmount;
		base.Open();
		wasCursorLocked = !SingletonBehaviour<PlayerLook>.Instance.RotationLocked;
		wasMovementLocked = SingletonBehaviour<PlayerMove>.Instance.MovementLocked;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
		if (!wasMovementLocked)
		{
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
		}
	}

	public void Open(string refundAmount, string title)
	{
		titleText.text = title;
		titleText.gameObject.SetActive(value: true);
		Open(refundAmount);
	}

	public override void Close()
	{
		base.Close();
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
