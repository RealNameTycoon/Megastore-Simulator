using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnpaidWagesWindow : UIWindow
{
	[SerializeField]
	private List<TextMeshProUGUI> employeeNames;

	[SerializeField]
	private RectTransform parentRect;

	private bool wasCursorLocked;

	private bool wasMovementLocked;

	public void OpenEditor()
	{
		Open(new List<string> { "John Doe", "Jane Smith", "Jim Beam" });
	}

	public void Open(List<string> employeeNames)
	{
		StartCoroutine(OpenRoutine(employeeNames));
	}

	private IEnumerator OpenRoutine(List<string> employeeNames)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < this.employeeNames.Count; i++)
		{
			if (i < employeeNames.Count)
			{
				this.employeeNames[i].gameObject.SetActive(value: true);
				this.employeeNames[i].text = employeeNames[i];
			}
			else
			{
				this.employeeNames[i].gameObject.SetActive(value: false);
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
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
		EventManager.NotifyEvent(SupermarketEvents.EMPLOYEE_PAYMENTS_DONE);
	}
}
