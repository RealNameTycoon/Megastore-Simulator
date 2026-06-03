using System;
using System.Collections;
using System.Collections.Generic;
using DFTGames.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonsWindow : SingletonBehaviour<ButtonsWindow>
{
	[SerializeField]
	private List<UIButton> pcUIs;

	[SerializeField]
	private List<TextMeshProUGUI> pcUITexts;

	[SerializeField]
	private Canvas pcCanvas;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private PauseWindow pauseWindow;

	[SerializeField]
	private PrologueWindow prologueWindow;

	[SerializeField]
	private UnpaidWagesWindow unpaidWagesWindow;

	private Transform opener;

	private Action leftClickAction;

	private List<KeyCode> keyCodes = new List<KeyCode>();

	private List<Action> actions = new List<Action>();

	private List<InputActionReference> inputActionRefs = new List<InputActionReference>();

	public static float CLICK_HOLD_CALLBACK_FREQUENCY = 0.2f;

	private WaitForSeconds waiterLeftClick = new WaitForSeconds(CLICK_HOLD_CALLBACK_FREQUENCY);

	private WaitForSeconds waiterRightClick = new WaitForSeconds(CLICK_HOLD_CALLBACK_FREQUENCY);

	private Coroutine leftClickHoldCoroutine;

	private Coroutine rightClickHoldCoroutine;

	private int leftClickActionIndex = -1;

	private int rightClickActionIndex = -1;

	private int primaryActionIndex = -1;

	private int secondaryActionIndex = -1;

	private IEnumerator LeftClickHoldRoutine()
	{
		while (true)
		{
			yield return waiterLeftClick;
			if (Input.GetKey(KeyCode.Mouse0) && leftClickActionIndex != -1)
			{
				actions[leftClickActionIndex]?.Invoke();
			}
		}
	}

	private IEnumerator RightClickHoldRoutine()
	{
		while (true)
		{
			yield return waiterRightClick;
			if (Input.GetKey(KeyCode.Mouse1) && rightClickActionIndex != -1 && rightClickActionIndex < actions.Count - 1)
			{
				actions[rightClickActionIndex]?.Invoke();
			}
		}
	}

	private IEnumerator PrimaryActionHoldRoutine()
	{
		while (true)
		{
			yield return waiterLeftClick;
			if (SingletonBehaviour<InputManager>.Instance.IsPressingPrimary && primaryActionIndex != -1)
			{
				actions[primaryActionIndex]?.Invoke();
			}
		}
	}

	private IEnumerator SecondaryActionHoldRoutine()
	{
		while (true)
		{
			yield return waiterRightClick;
			if (SingletonBehaviour<InputManager>.Instance.IsPressingSecondary && secondaryActionIndex != -1 && secondaryActionIndex < actions.Count - 1)
			{
				actions[secondaryActionIndex]?.Invoke();
			}
		}
	}

	private void Start()
	{
		canvas.enabled = false;
	}

	private void Update()
	{
		UpdateInputActionsCheck();
	}

	private void UpdateInputActionsCheck()
	{
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.PauseActionRef) && !inputActionRefs.Contains(SingletonBehaviour<InputManager>.Instance.PauseActionRef))
		{
			if (SingletonWindow<SettingsWindow>.Instance.IsOpen())
			{
				SingletonWindow<SettingsWindow>.Instance.Close();
			}
			else if (pauseWindow.IsOpen())
			{
				pauseWindow.Close();
			}
			else if (prologueWindow.IsOpen())
			{
				prologueWindow.Close();
			}
			else if (unpaidWagesWindow.IsOpen())
			{
				unpaidWagesWindow.Close();
			}
			else
			{
				pauseWindow.Open();
			}
			EventManager.NotifyEvent(UIEvents.LEAVE_TRIGGERED);
			return;
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.EndDayActionRef) && !inputActionRefs.Contains(SingletonBehaviour<InputManager>.Instance.EndDayActionRef) && SingletonBehaviour<TimeManager>.Instance.IsEndDayButtonActive() && !inputActionRefs.Contains(SingletonBehaviour<InputManager>.Instance.EndDayActionRef) && !SingletonWindow<PriceWindow>.Instance.IsOpen() && !SingletonWindow<SupermarketNameWindow>.Instance.IsOpen() && !SingletonWindow<EditLabelWindow>.Instance.IsOpen())
		{
			SingletonBehaviour<TimeManager>.Instance.OnEndDay();
		}
		if (inputActionRefs.Count == 0)
		{
			return;
		}
		for (int i = 0; i < inputActionRefs.Count; i++)
		{
			if (inputActionRefs[i] != null && SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(inputActionRefs[i]))
			{
				actions[i]?.Invoke();
				break;
			}
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.PrimaryActionRef) && (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked || SingletonBehaviour<TrayManager>.Instance.IsPicked || GenericBox.Instance.IsPicked || Megaphone.Instance.IsPicked || PaintRoller.Instance.IsPicked || SingletonBehaviour<PlayerSittingManager>.Instance.IsSittingOnChoppingStand) && primaryActionIndex != -1 && !Mathf.Approximately(Time.timeScale, 0f))
		{
			if (leftClickHoldCoroutine != null)
			{
				StopCoroutine(leftClickHoldCoroutine);
			}
			leftClickHoldCoroutine = StartCoroutine(PrimaryActionHoldRoutine());
			if (rightClickHoldCoroutine != null)
			{
				StopCoroutine(rightClickHoldCoroutine);
			}
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionReleasedThisFrame(SingletonBehaviour<InputManager>.Instance.PrimaryActionRef) && leftClickHoldCoroutine != null)
		{
			StopCoroutine(leftClickHoldCoroutine);
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.SecondaryActionRef) && (SingletonBehaviour<BoxManager>.Instance.IsBoxPicked || SingletonBehaviour<TrayManager>.Instance.IsPicked || GenericBox.Instance.IsPicked) && secondaryActionIndex != -1 && !Mathf.Approximately(Time.timeScale, 0f))
		{
			if (rightClickHoldCoroutine != null)
			{
				StopCoroutine(rightClickHoldCoroutine);
			}
			rightClickHoldCoroutine = StartCoroutine(SecondaryActionHoldRoutine());
			if (leftClickHoldCoroutine != null)
			{
				StopCoroutine(leftClickHoldCoroutine);
			}
		}
		if (SingletonBehaviour<InputManager>.Instance.WasActionReleasedThisFrame(SingletonBehaviour<InputManager>.Instance.SecondaryActionRef) && rightClickHoldCoroutine != null)
		{
			StopCoroutine(rightClickHoldCoroutine);
		}
	}

	public void StopLeftClickHold()
	{
		if (leftClickHoldCoroutine != null)
		{
			StopCoroutine(leftClickHoldCoroutine);
		}
	}

	public void RepaintWithKeyCodes(Dictionary<KeyCode, (string description, Action action)> buttonInfo, Transform opener = null, bool localizeDescription = true)
	{
		Dictionary<InputActionReference, (string, Action)> dictionary = new Dictionary<InputActionReference, (string, Action)>();
		foreach (KeyValuePair<KeyCode, (string, Action)> item in buttonInfo)
		{
			InputActionReference inputActionForKeyCode = SingletonBehaviour<InputManager>.Instance.GetInputActionForKeyCode(item.Key);
			if (inputActionForKeyCode != null)
			{
				dictionary[inputActionForKeyCode] = item.Value;
			}
		}
		if (dictionary.Count > 0)
		{
			RepaintWithInputActions(dictionary, opener, SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad, localizeDescription);
		}
		else
		{
			RepaintWithKeyCodes(buttonInfo, opener);
		}
	}

	public void RepaintWithInputActions(Dictionary<InputActionReference, (string description, Action action)> buttonInfo, Transform opener = null, bool useGamepad = false, bool localizeDescription = true)
	{
		this.opener = opener;
		keyCodes.Clear();
		actions.Clear();
		inputActionRefs.Clear();
		leftClickActionIndex = -1;
		rightClickActionIndex = -1;
		primaryActionIndex = -1;
		secondaryActionIndex = -1;
		int i = 0;
		foreach (KeyValuePair<InputActionReference, (string, Action)> item in buttonInfo)
		{
			if (item.Key == SingletonBehaviour<InputManager>.Instance.PrimaryActionRef)
			{
				primaryActionIndex = i;
				leftClickActionIndex = i;
			}
			else if (item.Key == SingletonBehaviour<InputManager>.Instance.SecondaryActionRef)
			{
				secondaryActionIndex = i;
				rightClickActionIndex = i;
			}
			string actionDescription = (localizeDescription ? Locale.GetWord(item.Value.Item1) : item.Value.Item1);
			pcUIs[i].Repaint(item.Key, actionDescription, item.Value.Item2, useGamepad);
			pcUIs[i].gameObject.SetActive(value: true);
			inputActionRefs.Add(item.Key);
			actions.Add(item.Value.Item2);
			i++;
		}
		for (; i < pcUIs.Count; i++)
		{
			pcUIs[i].gameObject.SetActive(value: false);
		}
		pcCanvas.enabled = pcUIs[0].gameObject.activeSelf;
	}

	private void RefreshInputActionDisplay()
	{
		for (int i = 0; i < inputActionRefs.Count && i < pcUIs.Count; i++)
		{
			if (pcUIs[i].gameObject.activeSelf && inputActionRefs[i] != null)
			{
				_ = pcUIs[i].CurrentAction;
			}
		}
	}

	public void AddToMenu(string key, Action action)
	{
	}

	public bool IsOpen()
	{
		return pcUIs[0].gameObject.activeSelf;
	}

	public bool IsOpenedBy(Transform transform)
	{
		return transform.Equals(opener);
	}

	public void Close()
	{
		keyCodes.Clear();
		actions.Clear();
		inputActionRefs.Clear();
		for (int i = 0; i < pcUIs.Count; i++)
		{
			pcUIs[i].gameObject.SetActive(value: false);
		}
		pcCanvas.enabled = false;
		leftClickActionIndex = -1;
		rightClickActionIndex = -1;
		primaryActionIndex = -1;
		secondaryActionIndex = -1;
	}
}
