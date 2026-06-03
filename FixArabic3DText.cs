using ArabicSupport;
using UnityEngine;

public class FixArabic3DText : MonoBehaviour
{
	public bool showTashkeel = true;

	public bool useHinduNumbers = true;

	private void Start()
	{
		string text = ArabicFixer.Fix(base.gameObject.GetComponent<TextMesh>().text, showTashkeel, useHinduNumbers);
		base.gameObject.GetComponent<TextMesh>().text = text;
		Debug.Log(text);
	}
}
