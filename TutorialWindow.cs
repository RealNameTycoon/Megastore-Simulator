using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject parentObject;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI objectiveTitleText;

	[SerializeField]
	private TextMeshProUGUI descText;

	[SerializeField]
	private Transform targetTransform;

	private bool isOpen;

	private Vector3 initialPosition;

	private string key;

	private string param;

	private void Start()
	{
		initialPosition = base.transform.position;
		LocalizeBase.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		if (param != "")
		{
			descText.text = Locale.GetWord(key).Replace("{0}", param);
		}
		else if (key != "")
		{
			descText.text = Locale.GetWord(key);
		}
	}

	public void SwitchToObjective()
	{
		titleText.enabled = false;
		objectiveTitleText.enabled = true;
	}

	public void Open(string descKey)
	{
		key = descKey;
		param = "";
		if (isOpen)
		{
			Refresh(descKey);
			return;
		}
		parentObject.SetActive(value: true);
		descText.text = Locale.GetWord(descKey);
		isOpen = true;
		base.transform.DOKill();
		base.transform.DOMove(targetTransform.position, 0.25f);
	}

	public void OpenWithKeyAndParam(string key, string param)
	{
		this.key = key;
		this.param = param;
		if (isOpen)
		{
			RefreshWithString(Locale.GetWord(key).Replace("{0}", param));
			return;
		}
		parentObject.SetActive(value: true);
		descText.text = Locale.GetWord(key).Replace("{0}", param);
		isOpen = true;
		base.transform.DOKill();
		base.transform.DOMove(targetTransform.position, 0.25f);
	}

	public void UpdateText(string key, string param)
	{
		this.key = key;
		this.param = param;
		descText.text = Locale.GetWord(key).Replace("{0}", param);
	}

	private void Refresh(string descKey)
	{
		if (isOpen)
		{
			base.transform.DOKill();
			base.transform.DOMove(initialPosition, 0.2f).OnComplete(delegate
			{
				descText.text = Locale.GetWord(descKey);
				base.transform.DOMove(targetTransform.position, 0.2f);
			});
		}
	}

	private void RefreshWithString(string text)
	{
		if (isOpen)
		{
			base.transform.DOKill();
			base.transform.DOMove(initialPosition, 0.2f).OnComplete(delegate
			{
				descText.text = text;
				base.transform.DOMove(targetTransform.position, 0.2f);
			});
		}
	}

	public bool IsKeyActive(string key)
	{
		if (key == this.key)
		{
			return isOpen;
		}
		return false;
	}

	public void Close()
	{
		isOpen = false;
		base.transform.DOKill();
		base.transform.DOMove(initialPosition, 0.2f).OnComplete(delegate
		{
			parentObject.SetActive(value: false);
		});
	}
}
