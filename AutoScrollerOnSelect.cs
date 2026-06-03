using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScrollerOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform scrollTargetOverride;

	public void OnSelect(BaseEventData eventData)
	{
		if (!(scrollRect == null) && SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
		{
			RectTransform rectTransform = ((scrollTargetOverride != null) ? scrollTargetOverride : GetComponentInParent<RectTransform>());
			if (!(rectTransform == null))
			{
				scrollRect.KeepInDeadZone(rectTransform);
			}
		}
	}
}
