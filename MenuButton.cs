using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private GameObject selectedBG;

	[SerializeField]
	private TextMeshProUGUI secondaryButtonText;

	private Color secondaryButtonTextColor;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		buttonText.color = Color.black;
		SetVisual(selected: true);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		SetVisual(selected: false);
	}

	public void OnSelect(BaseEventData eventData)
	{
		SetVisual(selected: true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		SetVisual(selected: false);
	}

	private void SetVisual(bool selected)
	{
		if (selectedBG != null)
		{
			selectedBG.SetActive(selected);
		}
		if (buttonText != null)
		{
			buttonText.color = (selected ? Color.black : Color.white);
		}
		if (secondaryButtonText != null)
		{
			secondaryButtonText.color = (selected ? Color.black : secondaryButtonTextColor);
		}
	}

	private void Start()
	{
		if (secondaryButtonText != null)
		{
			secondaryButtonTextColor = secondaryButtonText.color;
		}
	}

	private void Update()
	{
	}
}
