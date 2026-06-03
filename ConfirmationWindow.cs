using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationWindow : UIWindow
{
	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private Button cancelButton;

	private Action onConfirmAction;

	private void Awake()
	{
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(OnCancelButtonClicked);
	}

	public void Open(string description, Action onConfirmAction)
	{
		descriptionText.text = description;
		this.onConfirmAction = onConfirmAction;
		Open();
	}

	private void OnConfirmButtonClicked()
	{
		onConfirmAction?.Invoke();
		Close();
	}

	private void OnCancelButtonClicked()
	{
		Close();
	}
}
