using UnityEngine;

public class ArabicSupportTester : MonoBehaviour
{
	public ExpectedFixedText[] ExpectedTexts;

	private void Start()
	{
		ExpectedTexts = Object.FindObjectsOfType(typeof(ExpectedFixedText)) as ExpectedFixedText[];
		ExpectedFixedText[] expectedTexts = ExpectedTexts;
		for (int i = 0; i < expectedTexts.Length; i++)
		{
			expectedTexts[i].Fix();
		}
	}
}
