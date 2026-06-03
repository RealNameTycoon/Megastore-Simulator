using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
	[SerializeField]
	private IndicatorType indicatorType;

	private Image indicatorImage;

	private Text distanceText;

	public bool Active => base.transform.gameObject.activeInHierarchy;

	public IndicatorType Type => indicatorType;

	private void Awake()
	{
		indicatorImage = base.transform.GetComponent<Image>();
		distanceText = base.transform.GetComponentInChildren<Text>();
	}

	public void SetImageColor(Color color)
	{
		indicatorImage.color = color;
	}

	public void SetDistanceText(float value)
	{
		distanceText.text = ((value >= 0f) ? (Mathf.Floor(value) + " m") : "");
	}

	public void SetTextRotation(Quaternion rotation)
	{
		distanceText.rectTransform.rotation = rotation;
	}

	public void Activate(bool value)
	{
		base.transform.gameObject.SetActive(value);
	}
}
