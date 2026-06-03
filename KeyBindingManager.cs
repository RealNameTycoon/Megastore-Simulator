using System;
using System.Collections.Generic;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindingManager : SingletonBehaviour<KeyBindingManager>
{
	private struct RowEntry
	{
		public InputActionReference inputActionReference;

		public int keyboardBindingIndex;

		public int gamepadBindingIndex;

		public string labelKey;
	}

	private const string KEY_BINDING_OVERRIDES = "key_binding_overrides";

	[Header("UI List")]
	[SerializeField]
	private Button controlTabButton;

	[SerializeField]
	private ScrollRect scrollViewRows;

	[SerializeField]
	private Transform rowsParent;

	[SerializeField]
	private KeybindingRow rowPrefab;

	[SerializeField]
	private InputActionAsset inputActionAsset;

	[SerializeField]
	private Button resetToDefaultsButton;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private CanvasGroup warningCanvasGroup;

	private InputActionMap gameplay;

	private readonly List<KeybindingRow> _rows = new List<KeybindingRow>();

	private InputActionRebindingExtensions.RebindingOperation _rebindOp;

	private List<KeyValuePair<InputActionReference, string>> _actionRefs = new List<KeyValuePair<InputActionReference, string>>();

	public KeyCode InteractKey { get; private set; } = KeyCode.E;

	public KeyCode LeaveKey { get; private set; } = KeyCode.Escape;

	private new void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
		_actionRefs = SingletonBehaviour<InputManager>.Instance.GetAllActionPairs();
		LoadOverrides();
		BuildOrRefreshUI(_actionRefs);
		resetToDefaultsButton.onClick.AddListener(ResetOverrides);
	}

	public void BuildOrRefreshUI(List<KeyValuePair<InputActionReference, string>> actionRefs)
	{
		if (rowsParent == null || rowPrefab == null)
		{
			Debug.LogWarning("KeyBindingManager: rowsParent/rowPrefab not set.");
			return;
		}
		List<RowEntry> list = new List<RowEntry>(128);
		foreach (KeyValuePair<InputActionReference, string> actionRef in actionRefs)
		{
			InputAction inputAction = actionRef.Key?.action;
			if (inputAction == null)
			{
				continue;
			}
			if (string.Equals(inputAction.name, "Move", StringComparison.OrdinalIgnoreCase))
			{
				int num = FindCompositePartIndex(inputAction, "WASD", "up");
				int num2 = FindCompositePartIndex(inputAction, "WASD", "down");
				int num3 = FindCompositePartIndex(inputAction, "WASD", "left");
				int num4 = FindCompositePartIndex(inputAction, "WASD", "right");
				int gamepadBindingIndex = -1;
				if (num >= 0)
				{
					list.Add(new RowEntry
					{
						inputActionReference = actionRef.Key,
						keyboardBindingIndex = num,
						gamepadBindingIndex = gamepadBindingIndex,
						labelKey = "controls_move_up"
					});
				}
				if (num2 >= 0)
				{
					list.Add(new RowEntry
					{
						inputActionReference = actionRef.Key,
						keyboardBindingIndex = num2,
						gamepadBindingIndex = gamepadBindingIndex,
						labelKey = "controls_move_down"
					});
				}
				if (num3 >= 0)
				{
					list.Add(new RowEntry
					{
						inputActionReference = actionRef.Key,
						keyboardBindingIndex = num3,
						gamepadBindingIndex = gamepadBindingIndex,
						labelKey = "controls_move_left"
					});
				}
				if (num4 >= 0)
				{
					list.Add(new RowEntry
					{
						inputActionReference = actionRef.Key,
						keyboardBindingIndex = num4,
						gamepadBindingIndex = gamepadBindingIndex,
						labelKey = "controls_move_right"
					});
				}
			}
			else
			{
				int num5 = FindFirstSimpleBinding(inputAction);
				int gamepadBindingIndex2 = FindFirstSimpleBinding(inputAction, "Gamepad");
				if (num5 >= 0)
				{
					list.Add(new RowEntry
					{
						inputActionReference = actionRef.Key,
						keyboardBindingIndex = num5,
						gamepadBindingIndex = gamepadBindingIndex2,
						labelKey = actionRef.Value
					});
				}
			}
		}
		while (_rows.Count < list.Count)
		{
			KeybindingRow item = UnityEngine.Object.Instantiate(rowPrefab, rowsParent);
			_rows.Add(item);
		}
		for (int i = 0; i < _rows.Count; i++)
		{
			bool flag = i < list.Count;
			_rows[i].gameObject.SetActive(flag);
			if (i != 0 && i != _rows.Count - 1)
			{
				_rows[i].SetUpAndDownSelectables(_rows[i - 1].RebindButton.gameObject, _rows[i + 1].RebindButton.gameObject, _rows[i - 1].RebindGamepadButton.gameObject, _rows[i + 1].RebindGamepadButton.gameObject);
			}
			if (flag)
			{
				RowEntry rowEntry = list[i];
				_rows[i].Repaint(rowEntry.inputActionReference, rowEntry.keyboardBindingIndex, rowEntry.gamepadBindingIndex, rowEntry.labelKey);
			}
		}
		_rows[0].SetUpAndDownSelectables(null, _rows[1].RebindButton.gameObject, null, _rows[1].RebindGamepadButton.gameObject);
		_rows[_rows.Count - 1].SetUpAndDownSelectables(_rows[_rows.Count - 2].RebindButton.gameObject, resetToDefaultsButton.gameObject, _rows[_rows.Count - 2].RebindGamepadButton.gameObject, resetToDefaultsButton.gameObject);
		Navigation navigation = controlTabButton.navigation;
		controlTabButton.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = navigation.selectOnUp,
			selectOnDown = _rows[0].RebindButton,
			selectOnLeft = navigation.selectOnLeft,
			selectOnRight = navigation.selectOnRight
		};
		Navigation navigation2 = resetToDefaultsButton.navigation;
		resetToDefaultsButton.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = _rows[_rows.Count - 1].RebindButton,
			selectOnDown = navigation2.selectOnDown,
			selectOnLeft = navigation2.selectOnLeft,
			selectOnRight = navigation2.selectOnRight
		};
	}

	public void StartRebind(InputActionReference actionRef, int bindingIndex, Action onComplete, Action onCancel)
	{
		if (!(actionRef == null) && actionRef.action != null)
		{
			CancelActiveRebind();
			InputAction action = actionRef.action;
			action.Disable();
			string obj = action.bindings[bindingIndex].groups ?? "";
			bool flag = obj.Contains("Gamepad");
			obj.Contains("KeyboardMouse");
			InputActionRebindingExtensions.RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(bindingIndex).WithControlsExcluding("<Mouse>/position").WithControlsExcluding("<Mouse>/delta")
				.OnMatchWaitForAnother(0.05f);
			if (flag)
			{
				rebindingOperation.WithControlsExcluding("<Keyboard>");
				rebindingOperation.WithControlsExcluding("<Mouse>");
				rebindingOperation.WithCancelingThrough("<Gamepad>/buttonEast");
			}
			else
			{
				rebindingOperation.WithControlsExcluding("<Gamepad>");
				rebindingOperation.WithCancelingThrough("<Keyboard>/escape");
			}
			_rebindOp = rebindingOperation.OnCancel(delegate
			{
				CleanupAfterRebind(action);
				onCancel?.Invoke();
			}).OnComplete(delegate
			{
				SaveOverrides();
				CheckAndWarnConflicts(actionRef, bindingIndex);
				CleanupAfterRebind(action);
				onComplete?.Invoke();
			});
			_rebindOp.Start();
		}
	}

	public void CancelActiveRebind()
	{
		if (_rebindOp != null)
		{
			_rebindOp.Cancel();
			_rebindOp = null;
		}
	}

	private void CleanupAfterRebind(InputAction action)
	{
		try
		{
			_rebindOp?.Dispose();
		}
		finally
		{
			_rebindOp = null;
			action.Enable();
		}
	}

	public void SaveOverrides()
	{
		string value = inputActionAsset.SaveBindingOverridesAsJson();
		PlayerPrefs.SetString("key_binding_overrides", value);
		PlayerPrefs.Save();
		Debug.Log("KeyBindingManager: Saved overrides to PlayerPrefs");
	}

	public void LoadOverrides()
	{
		Debug.Log("KeyBindingManager: Loading overrides from PlayerPrefs");
		if (PlayerPrefs.HasKey("key_binding_overrides"))
		{
			string text = PlayerPrefs.GetString("key_binding_overrides", string.Empty);
			Debug.Log("KeyBindingManager: Loaded overrides from PlayerPrefs: " + text);
			if (!string.IsNullOrEmpty(text))
			{
				inputActionAsset.LoadBindingOverridesFromJson(text);
			}
		}
	}

	public void ResetOverrides()
	{
		PlayerPrefs.DeleteKey("key_binding_overrides");
		inputActionAsset.RemoveAllBindingOverrides();
		BuildOrRefreshUI(_actionRefs);
	}

	private static int FindCompositePartIndex(InputAction action, string compositeName, string partName, string groupFilter = "KeyboardMouse")
	{
		for (int i = 0; i < action.bindings.Count; i++)
		{
			InputBinding inputBinding = action.bindings[i];
			if (!inputBinding.isComposite || !string.Equals(inputBinding.name, compositeName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			for (int j = i + 1; j < action.bindings.Count; j++)
			{
				InputBinding inputBinding2 = action.bindings[j];
				if (!inputBinding2.isPartOfComposite)
				{
					break;
				}
				if ((string.IsNullOrEmpty(groupFilter) || (!string.IsNullOrEmpty(inputBinding2.groups) && inputBinding2.groups.Contains(groupFilter))) && string.Equals(inputBinding2.name, partName, StringComparison.OrdinalIgnoreCase))
				{
					return j;
				}
			}
		}
		return -1;
	}

	private static int FindFirstSimpleBinding(InputAction action, string groupFilter = "KeyboardMouse")
	{
		for (int i = 0; i < action.bindings.Count; i++)
		{
			InputBinding inputBinding = action.bindings[i];
			if (!inputBinding.isComposite && !inputBinding.isPartOfComposite && !string.IsNullOrEmpty(inputBinding.effectivePath) && (string.IsNullOrEmpty(groupFilter) || (!string.IsNullOrEmpty(inputBinding.groups) && inputBinding.groups.Contains(groupFilter))))
			{
				return i;
			}
		}
		return -1;
	}

	private void CheckAndWarnConflicts(InputActionReference changedRef, int changedBindingIndex)
	{
		InputAction inputAction = changedRef?.action;
		if (inputAction == null || changedBindingIndex < 0 || changedBindingIndex >= inputAction.bindings.Count)
		{
			return;
		}
		InputBinding inputBinding = inputAction.bindings[changedBindingIndex];
		string effectivePath = inputBinding.effectivePath;
		if (string.IsNullOrEmpty(effectivePath))
		{
			return;
		}
		string bindingDisplay = KeybindingRow.GetBindingDisplay(changedRef, changedBindingIndex);
		List<string> list = new List<string>();
		HashSet<InputAction> hashSet = new HashSet<InputAction>();
		foreach (KeyValuePair<InputActionReference, string> actionRef in _actionRefs)
		{
			InputAction inputAction2 = actionRef.Key?.action;
			if (inputAction2 == null || !hashSet.Add(inputAction2))
			{
				continue;
			}
			for (int i = 0; i < inputAction2.bindings.Count; i++)
			{
				if (inputAction2 != inputAction || i != changedBindingIndex)
				{
					InputBinding inputBinding2 = inputAction2.bindings[i];
					if (!inputBinding2.isComposite && GroupsOverlap(inputBinding.groups, inputBinding2.groups) && string.Equals(inputBinding2.effectivePath, effectivePath, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(Locale.GetWord(actionRef.Value));
					}
				}
			}
		}
		if (list.Count > 0)
		{
			string newValue = string.Join(", ", list);
			ShowWarning(Locale.GetWord("warning_key_conflict").Replace("{0}", bindingDisplay).Replace("{1}", newValue));
		}
	}

	private static bool GroupsOverlap(string groupsA, string groupsB)
	{
		if (string.IsNullOrEmpty(groupsA) || string.IsNullOrEmpty(groupsB))
		{
			return true;
		}
		string[] array = groupsA.Split(';');
		string[] array2 = groupsB.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				if (string.Equals(array[i], array2[j], StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ShowWarning(string msg)
	{
		warningText.text = msg;
		warningText.enabled = true;
		warningCanvasGroup.alpha = 1f;
		warningCanvasGroup.DOKill();
		warningCanvasGroup.DOFade(0f, 0.5f).SetDelay(5f).SetUpdate(isIndependentUpdate: true);
	}
}
