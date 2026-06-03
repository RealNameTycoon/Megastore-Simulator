using UnityEngine;

public class Hoverable : MonoBehaviour
{
	[SerializeField]
	private string toolTipKey;

	public void SetTooltip(string key)
	{
		toolTipKey = key;
	}

	protected virtual string GetToolTip()
	{
		return toolTipKey;
	}
}
