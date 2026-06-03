using UnityEngine;

public class GDPRWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas gdprCanvas;

	[SerializeField]
	private StartWindow startWindow;

	public void OnPrivacyPolicy()
	{
		Application.OpenURL("https://yologamesstudio.blogspot.com/2024/07/of-use-terms-of-this-agreement-terms-of.html");
	}

	public void Open()
	{
		gdprCanvas.enabled = true;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!gdprCanvas.enabled);
	}

	public void Close()
	{
		gdprCanvas.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!gdprCanvas.enabled);
	}

	private void Update()
	{
	}
}
