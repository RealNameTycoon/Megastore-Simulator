using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditLabelWindow : SingletonWindow<EditLabelWindow>
{
	[SerializeField]
	private TMP_InputField labelInputField;

	[SerializeField]
	private TextMeshProUGUI uptoCharNText;

	[SerializeField]
	private Button okButton;

	[SerializeField]
	private GraphicRaycaster raycaster;

	private PalletRack palletRack;

	private const int MAX_CHAR_COUNT = 4;

	private void Start()
	{
		okButton.onClick.AddListener(OnOk);
	}

	private void OnOk()
	{
		if (palletRack != null)
		{
			palletRack.SetRackLabel(labelInputField.text);
		}
		Close();
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
		raycaster.enabled = false;
	}

	public void Open(PalletRack palletRack)
	{
		this.palletRack = palletRack;
		string rackLabel = palletRack.GetRackLabel();
		labelInputField.text = rackLabel;
		uptoCharNText.text = Locale.GetWord("upto_char_n").Replace("{0}", 4.ToString());
		base.Open();
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: false);
		raycaster.enabled = true;
	}
}
