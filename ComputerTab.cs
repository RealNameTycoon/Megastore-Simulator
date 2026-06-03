using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ComputerTab : MonoBehaviour
{
	[SerializeField]
	private Button tabButton;

	[SerializeField]
	private Image tabBackground;

	[SerializeField]
	private TabbedPanel tabbedPanel;

	private static Color selectedTabColor = new Color(7f / 85f, 0.34509805f, 53f / 85f);

	private static Color deselectedTabColor = new Color(0.21960784f, 0.59607846f, 0.99215686f);

	private bool isSelected;

	private Vector3 initialScale = Vector3.one;

	public bool IsSelected => isSelected;

	public Button TabButton => tabButton;

	public TabbedPanel TabWindow => tabbedPanel;

	public void Select()
	{
		tabbedPanel.transform.DOKill();
		tabBackground.color = selectedTabColor;
		isSelected = true;
		if (!base.gameObject.activeSelf)
		{
			tabbedPanel.transform.localScale = 0.2f * Vector3.one;
			tabbedPanel.Repaint();
			base.gameObject.transform.SetAsLastSibling();
			base.gameObject.SetActive(value: true);
		}
		tabbedPanel.transform.DOScale(initialScale, 0.15f);
		tabbedPanel.Open();
	}

	public void SelectInstant()
	{
		tabbedPanel.transform.DOKill();
		tabbedPanel.transform.localScale = Vector3.one;
		tabBackground.color = selectedTabColor;
		isSelected = true;
		if (!base.gameObject.activeSelf)
		{
			tabbedPanel.Repaint();
			base.gameObject.transform.SetAsLastSibling();
			base.gameObject.SetActive(value: true);
		}
		tabbedPanel.Open();
	}

	public void Deselect()
	{
		tabbedPanel.RemoveFocus();
		tabbedPanel.transform.DOKill();
		tabbedPanel.transform.DOScale(0.15f, 0.25f).OnComplete(delegate
		{
			tabbedPanel.Close();
		});
		isSelected = false;
		tabBackground.color = deselectedTabColor;
		tabButton.interactable = true;
	}

	public void DeselectInstant()
	{
		tabbedPanel.transform.DOKill();
		tabbedPanel.Close();
		isSelected = false;
		tabBackground.color = deselectedTabColor;
		tabButton.interactable = true;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
