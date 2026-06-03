using System;
using System.Collections;
using System.Collections.Generic;
using DFTGames.Localization;
using TinyGiantStudio.Layout;
using TinyGiantStudio.Text;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class SupermarketNameWindow : SingletonWindow<SupermarketNameWindow>
{
	[SerializeField]
	private Modular3DText supermarketName3DText;

	[SerializeField]
	private VolumeLayoutGroup volumeLayoutGroup;

	[SerializeField]
	private Modular3DText supermarketName3DTextSide;

	[SerializeField]
	private Modular3DText supermarketName3DTextInside;

	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private UnityEngine.UI.Button okButton;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private List<Material> styleMaterials;

	[SerializeField]
	private UnityEngine.UI.Button nextStyle;

	[SerializeField]
	private TextMeshProUGUI styleText;

	[SerializeField]
	private UnityEngine.UI.Button previousStyle;

	[SerializeField]
	private UnityEngine.UI.Toggle leftAlignmentToggle;

	[SerializeField]
	private UnityEngine.UI.Toggle rightAlignmentToggle;

	[SerializeField]
	private UnityEngine.UI.Toggle centerAlignmentToggle;

	[SerializeField]
	private ColorPicker textColorPicker;

	[SerializeField]
	private Image textColorPreviewImage;

	[SerializeField]
	private ColorPicker signColorPicker;

	[SerializeField]
	private Image signColorPreviewImage;

	[SerializeField]
	private UnityEngine.UI.Button increaseFontSizeButton;

	[SerializeField]
	private UnityEngine.UI.Button decreaseFontSizeButton;

	[SerializeField]
	private TextMeshProUGUI fontSizeText;

	[SerializeField]
	private UnityEngine.UI.Toggle hideMegastoreToggle;

	[SerializeField]
	private List<GameObject> megastorePostfixes;

	[SerializeField]
	private UnityEngine.UI.Button signColorButton;

	[SerializeField]
	private UnityEngine.UI.Button textColorButton;

	[SerializeField]
	private Canvas signColorPanel;

	[SerializeField]
	private Canvas textColorPanel;

	[SerializeField]
	private UnityEngine.UI.Button signColorOkButton;

	[SerializeField]
	private UnityEngine.UI.Button textColorOkButton;

	[SerializeField]
	private List<MeshRenderer> signMeshRenderers;

	[SerializeField]
	private List<MeshRenderer> textMeshRenderers;

	private int currentFontSize = 6;

	private Action onCloseAction;

	private int currentStyleIndex;

	private string currentName = "";

	private string lastAppliedName = "";

	private Alignment currentAlignment = Alignment.MiddleCenter;

	private const string CURRENT_ALIGNMENT_KEY = "currentAlignmentKey";

	private const string CURRENT_TEXT_COLOR_KEY = "currentTextColorKey";

	private const string CURRENT_SIGN_COLOR_KEY = "currentSignColorKey";

	private const string TEXT_STYLE_KEY = "textStyleKey";

	private const string CURRENT_FONT_SIZE_KEY = "currentFontSizeKey";

	private const string HIDE_POSTFIXES_KEY = "hidePostfixesKey";

	private Color currentTextColor = Color.white;

	private Color currentSignColor = Color.white;

	private bool hidePostfixes;

	private static int NEON_STYLE_INDEX = 1;

	private const float emissionIntensity = 6f;

	private Dictionary<int, string> textStyleNames = new Dictionary<int, string>
	{
		{ 0, "text_normal" },
		{ 1, "text_neon" }
	};

	private int MAX_FONT_SIZE = 9;

	private int MIN_FONT_SIZE = 3;

	private float postfixVisibleHeight = 1.65f;

	private float postfixHiddenHeight = 3.3f;

	private Color defaultSignColor = new Color(0.4f, 0.4f, 0.4f);

	private void Start()
	{
		currentName = GenericDataSerializer.LoadString(StartWindow.supermarketNameKey);
		inputField.text = currentName;
		ApplyName(currentName);
		inputField.onValueChanged.AddListener(OnValueChanged);
		okButton.onClick.AddListener(RenameAndClose);
		nextStyle.onClick.AddListener(NextStyle);
		previousStyle.onClick.AddListener(PreviousStyle);
		signColorButton.onClick.AddListener(OnSignColorButtonClicked);
		textColorButton.onClick.AddListener(OnTextColorButtonClicked);
		currentStyleIndex = GenericDataSerializer.LoadInt("textStyleKey");
		currentAlignment = (Alignment)GenericDataSerializer.Load("currentAlignmentKey", 4);
		leftAlignmentToggle.isOn = currentAlignment == Alignment.MiddleLeft;
		rightAlignmentToggle.isOn = currentAlignment == Alignment.MiddleRight;
		centerAlignmentToggle.isOn = currentAlignment == Alignment.MiddleCenter;
		leftAlignmentToggle.onValueChanged.AddListener(AlignLeft);
		rightAlignmentToggle.onValueChanged.AddListener(AlignRight);
		centerAlignmentToggle.onValueChanged.AddListener(AlignCenter);
		currentTextColor = GenericDataSerializer.Load("currentTextColorKey", Color.white);
		currentSignColor = GenericDataSerializer.Load("currentSignColorKey", defaultSignColor);
		currentFontSize = GenericDataSerializer.LoadInt("currentFontSizeKey", 6);
		fontSizeText.text = currentFontSize.ToString();
		increaseFontSizeButton.onClick.AddListener(IncreaseFontSize);
		decreaseFontSizeButton.onClick.AddListener(DecreaseFontSize);
		hidePostfixes = GenericDataSerializer.LoadBool("hidePostfixesKey");
		hideMegastoreToggle.isOn = hidePostfixes;
		hideMegastoreToggle.onValueChanged.AddListener(OnHidePostfixesChanged);
		textColorPicker.color = currentTextColor;
		textColorPicker.onColorChanged += OnTextColorChanged;
		signColorPicker.color = currentSignColor;
		signColorPicker.onColorChanged += OnSignColorChanged;
		OnHidePostfixesChanged(hidePostfixes);
		UpdateFontSize();
		UpdateLayout();
		UpdateStyle();
		OnTextColorChanged(currentTextColor);
		OnSignColorChanged(currentSignColor);
	}

	private void OnSignColorButtonClicked()
	{
		signColorPanel.enabled = true;
		SingletonBehaviour<InputManager>.Instance.SelectElement(signColorOkButton.gameObject);
		if (textColorPanel.enabled)
		{
			textColorPanel.enabled = false;
			ApplyTextColor();
		}
	}

	private void OnTextColorButtonClicked()
	{
		textColorPanel.enabled = true;
		SingletonBehaviour<InputManager>.Instance.SelectElement(textColorOkButton.gameObject);
		if (signColorPanel.enabled)
		{
			signColorPanel.enabled = false;
		}
	}

	private void IncreaseFontSize()
	{
		currentFontSize++;
		currentFontSize = Mathf.Clamp(currentFontSize, MIN_FONT_SIZE, MAX_FONT_SIZE);
		fontSizeText.text = currentFontSize.ToString();
		GenericDataSerializer.SaveInt("currentFontSizeKey", currentFontSize);
		UpdateFontSize();
	}

	private void DecreaseFontSize()
	{
		currentFontSize--;
		currentFontSize = Mathf.Clamp(currentFontSize, MIN_FONT_SIZE, MAX_FONT_SIZE);
		fontSizeText.text = currentFontSize.ToString();
		GenericDataSerializer.SaveInt("currentFontSizeKey", currentFontSize);
		UpdateFontSize();
	}

	private void UpdateFontSize()
	{
		supermarketName3DText.FontSize = new Vector3(currentFontSize, currentFontSize, supermarketName3DText.FontSize.z);
	}

	private void OnHidePostfixesChanged(bool isOn)
	{
		hidePostfixes = isOn;
		GenericDataSerializer.SaveBool("hidePostfixesKey", hidePostfixes);
		if (hidePostfixes)
		{
			volumeLayoutGroup.Height = postfixHiddenHeight;
		}
		else
		{
			volumeLayoutGroup.Height = postfixVisibleHeight;
		}
		foreach (GameObject megastorePostfix in megastorePostfixes)
		{
			megastorePostfix.SetActive(!hidePostfixes);
		}
		UpdateLayout();
	}

	private void OnTextColorChanged(Color color)
	{
		currentTextColor = color;
		StartCoroutine(UpdateForTextColorNextFrame());
	}

	private IEnumerator UpdateForTextColorNextFrame()
	{
		yield return new WaitForEndOfFrame();
		textColorPreviewImage.color = currentTextColor;
		UpdateForColor();
	}

	public void ApplyTextColor()
	{
		GenericDataSerializer.Save("currentTextColorKey", currentTextColor);
		textColorPanel.enabled = false;
		SingletonBehaviour<InputManager>.Instance.SelectElement(base.DefaultSelectedElement);
	}

	public void CancelTextColor()
	{
		currentTextColor = GenericDataSerializer.Load("currentTextColorKey", Color.white);
		textColorPicker.color = currentTextColor;
		textColorPanel.enabled = false;
		SingletonBehaviour<InputManager>.Instance.SelectElement(base.DefaultSelectedElement);
	}

	private void UpdateForColor()
	{
		textColorPreviewImage.color = currentTextColor;
		if (currentStyleIndex == NEON_STYLE_INDEX)
		{
			Color value = currentTextColor * 6f;
			styleMaterials[currentStyleIndex].SetColor("_EmissionColor", value);
			styleMaterials[currentStyleIndex].color = currentTextColor;
		}
		else
		{
			styleMaterials[currentStyleIndex].color = currentTextColor;
		}
	}

	private void SetTextColor()
	{
	}

	private void OnSignColorChanged(Color color)
	{
		currentSignColor = color;
		StartCoroutine(UpdateForSignColorNextFrame());
	}

	private IEnumerator UpdateForSignColorNextFrame()
	{
		yield return new WaitForEndOfFrame();
		UpdateForSignColor();
	}

	public void ApplySignColor()
	{
		GenericDataSerializer.Save("currentSignColorKey", currentSignColor);
		signColorPanel.enabled = false;
		SingletonBehaviour<InputManager>.Instance.SelectElement(base.DefaultSelectedElement);
	}

	public void CancelSignColor()
	{
		currentSignColor = GenericDataSerializer.Load("currentSignColorKey", defaultSignColor);
		signColorPicker.color = currentSignColor;
		signColorPanel.enabled = false;
		SingletonBehaviour<InputManager>.Instance.SelectElement(base.DefaultSelectedElement);
	}

	private void UpdateForSignColor()
	{
		signColorPreviewImage.color = currentSignColor;
		foreach (MeshRenderer signMeshRenderer in signMeshRenderers)
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			signMeshRenderer.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetColor("_Tint", currentSignColor);
			signMeshRenderer.SetPropertyBlock(materialPropertyBlock);
		}
	}

	private void UpdateLayout()
	{
		volumeLayoutGroup.Anchor = currentAlignment;
		volumeLayoutGroup.UpdateLayout();
	}

	private void NextStyle()
	{
		currentStyleIndex++;
		currentStyleIndex %= styleMaterials.Count;
		GenericDataSerializer.SaveInt("textStyleKey", currentStyleIndex);
		UpdateStyle();
	}

	private void PreviousStyle()
	{
		currentStyleIndex--;
		if (currentStyleIndex < 0)
		{
			currentStyleIndex = styleMaterials.Count - 1;
		}
		GenericDataSerializer.SaveInt("textStyleKey", currentStyleIndex);
		UpdateStyle();
	}

	private void UpdateStyle()
	{
		supermarketName3DText.Material = styleMaterials[currentStyleIndex];
		supermarketName3DTextSide.Material = styleMaterials[currentStyleIndex];
		supermarketName3DTextInside.Material = styleMaterials[currentStyleIndex];
		styleText.text = Locale.GetWord(textStyleNames[currentStyleIndex]);
		UpdateForColor();
	}

	public bool IsNameSet()
	{
		return GenericDataSerializer.HasKey(StartWindow.supermarketNameKey);
	}

	public void SetOnCloseAction(Action onCloseAction)
	{
		this.onCloseAction = onCloseAction;
	}

	public override void Open()
	{
		inputField.enabled = true;
		inputField.interactable = true;
		base.Open();
		canvasGroup.alpha = 1f;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!canvas.enabled);
		SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
	}

	public override void Close()
	{
		inputField.enabled = false;
		inputField.interactable = false;
		EventManager.NotifyEvent(UIEvents.UI_WINDOW_CLOSED);
		base.Close();
		canvasGroup.alpha = 0f;
		onCloseAction?.Invoke();
		if (signColorPanel.enabled)
		{
			ApplySignColor();
		}
		if (textColorPanel.enabled)
		{
			ApplyTextColor();
		}
	}

	private void OnValueChanged(string newValue)
	{
		currentName = newValue;
		ApplyName(currentName);
	}

	private void ApplyName(string newValue)
	{
		if (!(lastAppliedName == newValue))
		{
			if (lastAppliedName == "")
			{
				lastAppliedName = newValue;
				supermarketName3DText.Text = newValue;
				supermarketName3DTextSide.Text = newValue;
				supermarketName3DTextInside.Text = newValue;
				supermarketName3DText.CleanUpdateText();
			}
			else
			{
				lastAppliedName = newValue;
				supermarketName3DText.Text = newValue;
				supermarketName3DTextSide.Text = newValue;
				supermarketName3DTextInside.Text = newValue;
			}
		}
	}

	private void AlignLeft(bool isOn)
	{
		if (isOn)
		{
			currentAlignment = Alignment.MiddleLeft;
			GenericDataSerializer.Save("currentAlignmentKey", (int)currentAlignment);
			UpdateLayout();
		}
	}

	private void AlignRight(bool isOn)
	{
		if (isOn)
		{
			currentAlignment = Alignment.MiddleRight;
			GenericDataSerializer.Save("currentAlignmentKey", (int)currentAlignment);
			UpdateLayout();
		}
	}

	private void AlignCenter(bool isOn)
	{
		if (isOn)
		{
			currentAlignment = Alignment.MiddleCenter;
			GenericDataSerializer.Save("currentAlignmentKey", (int)currentAlignment);
			UpdateLayout();
		}
	}

	public void RenameAndClose()
	{
		bool flag = !GenericDataSerializer.HasKey(StartWindow.supermarketNameKey);
		if (inputField.text.Length < 1)
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("supermarket_name_error", base.transform);
			return;
		}
		GenericDataSerializer.SaveString(StartWindow.supermarketNameKey, currentName);
		DataSerializer.SaveStoreName(currentName);
		if (flag)
		{
			EventManager.NotifyEvent(GameEvents.NAME_SET_FIRST_TIME);
		}
		Close();
		signColorPanel.enabled = false;
		textColorPanel.enabled = false;
	}
}
