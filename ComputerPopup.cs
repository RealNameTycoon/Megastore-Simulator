using System;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerPopup : UIWindow
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private TextMeshProUGUI closeButtonText;

	private Action onConfirmAction;

	private void Awake()
	{
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
	}

	public void Open(string title, string description, Action onConfirmAction = null)
	{
		titleText.text = title;
		descriptionText.text = description;
		this.onConfirmAction = onConfirmAction;
		confirmButton.gameObject.SetActive(onConfirmAction != null);
		if (onConfirmAction == null)
		{
			closeButtonText.text = Locale.GetWord("ok_capital");
		}
		else
		{
			closeButtonText.text = Locale.GetWord("cancel");
		}
		base.Open();
		canvas.enabled = true;
	}

	private void OnConfirmButtonClicked()
	{
		onConfirmAction?.Invoke();
	}

	public override void Close()
	{
		base.Close();
		canvas.enabled = false;
	}
}
