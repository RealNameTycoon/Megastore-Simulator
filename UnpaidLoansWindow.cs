using System.Collections;
using DFTGames.Localization;
using TMPro;
using UnityEngine;

public class UnpaidLoansWindow : UIWindow
{
	[SerializeField]
	private TextMeshProUGUI descriptionText;

	private bool wasCursorLocked;

	private bool wasMovementLocked;

	public void OpenWithFullText(string description)
	{
		descriptionText.text = description;
		StartCoroutine(OpenRoutine());
	}

	public void Open(string descriptionKey)
	{
		descriptionText.text = Locale.GetWord(descriptionKey);
		StartCoroutine(OpenRoutine());
	}

	private IEnumerator OpenRoutine()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		base.Open();
		wasCursorLocked = !SingletonBehaviour<PlayerLook>.Instance.RotationLocked;
		wasMovementLocked = SingletonBehaviour<PlayerMove>.Instance.MovementLocked;
		if (wasCursorLocked)
		{
			SingletonBehaviour<PlayerLook>.Instance.LockCursor(!wasCursorLocked);
		}
		if (!wasMovementLocked)
		{
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = true;
		}
	}

	public override void Close()
	{
		base.Close();
		if (wasCursorLocked)
		{
			SingletonBehaviour<PlayerLook>.Instance.LockCursor(state: true);
		}
		if (!wasMovementLocked)
		{
			SingletonBehaviour<PlayerMove>.Instance.MovementLocked = false;
		}
	}
}
