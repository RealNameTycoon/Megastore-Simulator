using TMPro;
using UnityEngine;

public class FontChanger : MonoBehaviour
{
	public delegate void ChildHandler(GameObject child);

	[SerializeField]
	private TMP_FontAsset fontAssetToUse;

	public void ChangeFont()
	{
		foreach (Transform item in base.gameObject.transform)
		{
			ChangeFontInChild(item.gameObject);
		}
	}

	public void ChangeFontInChild(GameObject child)
	{
		if (child == null)
		{
			return;
		}
		TextMeshProUGUI component = child.GetComponent<TextMeshProUGUI>();
		if (component != null)
		{
			component.font = fontAssetToUse;
		}
		foreach (Transform item in child.transform)
		{
			ChangeFontInChild(item.gameObject);
		}
	}

	public void IterateChildren(GameObject gameObject, ChildHandler childHandler, bool recursive)
	{
		DoIterate(gameObject, childHandler, recursive);
	}

	private void DoIterate(GameObject gameObject, ChildHandler childHandler, bool recursive)
	{
	}
}
