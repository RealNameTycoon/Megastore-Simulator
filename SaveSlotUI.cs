using System;
using DFTGames.Localization;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI storeNameAndDayText;

	[SerializeField]
	private TextMeshProUGUI lastPlayedTimeText;

	[SerializeField]
	private TextMeshProUGUI emptySlotText;

	[SerializeField]
	private Image selectionOutline;

	[SerializeField]
	private GameObject emptySlotObject;

	private bool isEmpty = true;

	private int profileIndex;

	public bool IsEmpty => isEmpty;

	public int ProfileIndex => profileIndex;

	public void Initialize(int profileIndex)
	{
		this.profileIndex = profileIndex;
		emptySlotText.text = Locale.GetWord("empty_slot_n").Replace("{0}", (profileIndex + 1).ToString());
	}

	public bool IsSelected()
	{
		return selectionOutline.enabled;
	}

	public void Refresh()
	{
		storeNameAndDayText.enabled = true;
		lastPlayedTimeText.enabled = true;
		int num = DataSerializer.LoadStoreDay(profileIndex);
		string text = Locale.GetWord("day_n").Replace("{0}", num.ToString());
		storeNameAndDayText.text = DataSerializer.LoadStoreName(profileIndex) + " - " + text;
		string newValue = GetLastPlayedTimeText(DataSerializer.LoadLastPlayedTime(profileIndex));
		lastPlayedTimeText.text = Locale.GetWord("last_played_time_n").Replace("{0}", newValue);
		isEmpty = false;
		emptySlotObject.SetActive(value: false);
	}

	private string GetLastPlayedTimeText(string lastPlayedTime)
	{
		if (!long.TryParse(lastPlayedTime, out var result))
		{
			return "";
		}
		return new DateTime(result, DateTimeKind.Utc).ToLocalTime().ToString("dd MMM yyyy HH:mm");
	}

	public void RefreshEmpty()
	{
		storeNameAndDayText.enabled = false;
		lastPlayedTimeText.enabled = false;
		isEmpty = true;
		emptySlotObject.SetActive(value: true);
	}

	public void SetSelected(bool isSelected)
	{
		selectionOutline.enabled = isSelected;
	}
}
