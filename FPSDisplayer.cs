using TMPro;
using UnityEngine;

public class FPSDisplayer : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI FPSText;

	private float frameTotalTime;

	private int frameCount;

	private void Update()
	{
		frameTotalTime += Time.unscaledDeltaTime;
		frameCount++;
		if (frameTotalTime >= 1f)
		{
			FPSText.text = frameCount.ToString();
			frameCount = 0;
			frameTotalTime = 0f;
		}
	}
}
