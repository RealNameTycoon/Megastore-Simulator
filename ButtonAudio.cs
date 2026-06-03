using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAudio : MonoBehaviour, IPointerUpHandler, IEventSystemHandler
{
	[SerializeField]
	private AudioManager.UIAudioTypes type;

	private Button parentButton;

	private void Start()
	{
		parentButton = base.gameObject.GetComponent<Button>();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!(parentButton != null) || parentButton.interactable)
		{
			SingletonBehaviour<AudioManager>.Instance.PlayUIAudio(type);
		}
	}
}
