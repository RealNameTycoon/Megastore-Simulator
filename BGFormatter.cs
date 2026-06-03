using UnityEngine;
using UnityEngine.UI;

public class BGFormatter : MonoBehaviour
{
	[SerializeField]
	private Image phoneBG;

	[SerializeField]
	private Image tabletBG;

	[SerializeField]
	private int referencePhoneHeight;

	[SerializeField]
	private int referencePhoneWidth;

	private static float aspect;

	private static float referenceAspect;

	private void Start()
	{
		aspect = (float)Screen.width / (float)Screen.height;
		referenceAspect = (float)referencePhoneWidth / (float)referencePhoneHeight;
		if (referenceAspect / aspect <= 1f)
		{
			phoneBG.enabled = true;
			tabletBG.enabled = false;
		}
		else
		{
			phoneBG.enabled = false;
			tabletBG.enabled = true;
		}
	}

	public static bool IsTablet()
	{
		return referenceAspect / aspect > 1f;
	}
}
