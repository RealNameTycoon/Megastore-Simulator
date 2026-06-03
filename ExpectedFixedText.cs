using ArabicSupport;
using UnityEngine;

public class ExpectedFixedText : MonoBehaviour
{
	[TextArea]
	public string Unfixed;

	[TextArea]
	public string Expected;

	public bool ShowTashkeel;

	public bool UseHinduNumbers = true;

	public string Fixed { get; private set; }

	public void Fix()
	{
		Fixed = ArabicFixer.Fix(Unfixed, ShowTashkeel, UseHinduNumbers);
	}
}
