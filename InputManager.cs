using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;

public class InputManager : SingletonBehaviour<InputManager>
{
	[SerializeField]
	private EventSystem eventSystem;

	[SerializeField]
	private GameObject lastSelectedElement;

	[SerializeField]
	private InputActionReference uiSubmitAction;

	[Header("Movement Actions")]
	[SerializeField]
	private InputActionReference moveAction;

	[SerializeField]
	private InputActionReference lookAction;

	[SerializeField]
	private InputActionReference jumpAction;

	[SerializeField]
	private InputActionReference runAction;

	[Header("Primary Actions")]
	[SerializeField]
	private InputActionReference primaryAction;

	[SerializeField]
	private InputActionReference secondaryAction;

	[SerializeField]
	private InputActionReference middleClickAction;

	[SerializeField]
	private InputActionReference scrollWheelAction;

	[Header("Gameplay Actions")]
	[SerializeField]
	private InputActionReference interactAction;

	[SerializeField]
	private InputActionReference cancelAction;

	[SerializeField]
	private InputActionReference closeAction;

	[SerializeField]
	private InputActionReference putDownAction;

	[SerializeField]
	private InputActionReference rotateLeftAction;

	[SerializeField]
	private InputActionReference rotateRightAction;

	[SerializeField]
	private InputActionReference setPriceAction;

	[SerializeField]
	private InputActionReference endDayAction;

	[Header("UI Actions")]
	[SerializeField]
	private InputActionReference switchTabAction;

	[SerializeField]
	private InputActionReference pauseAction;

	[SerializeField]
	private InputActionReference modifierAction;

	[SerializeField]
	private InputActionReference modifierAltAction;

	[Header("Hotkey Actions")]
	[SerializeField]
	private InputActionReference hotkey1Action;

	[SerializeField]
	private InputActionReference hotkey2Action;

	[SerializeField]
	private InputActionReference hotkey3Action;

	[SerializeField]
	private InputActionReference hotkey4Action;

	[Header("Forklift Actions")]
	[SerializeField]
	private InputActionReference liftAction;

	[SerializeField]
	private InputActionReference lowerAction;

	[SerializeField]
	private InputActionReference reverseAction;

	[SerializeField]
	private InputActionReference forwardAction;

	[SerializeField]
	private InputActionReference steeringAction;

	[Header("Pack Actions")]
	[SerializeField]
	private InputActionReference packAction;

	[SerializeField]
	private InputActionReference palletInteractionAction;

	[SerializeField]
	private InputActionReference clearCartAction;

	[SerializeField]
	private SerializedDictionary<GamepadGlyph, Sprite> xboxGlyphSprites;

	[SerializeField]
	private SerializedDictionary<GamepadGlyph, Sprite> psGlyphSprites;

	[Header("Mobile Controls")]
	private int m_LastTouchId = -1;

	private Vector2 m_LastTouchPosition;

	private bool m_CameraRotating;

	private Vector2 m_InitialMousePosition;

	private Vector2 m_MoveDifference;

	public Action leftMouseDownAction;

	public Action leftMouseUpAction;

	public Action rightMouseDownAction;

	private Dictionary<KeyCode, InputActionReference> keyCodeToActionMap;

	public InputActionReference UISubmitActionRef => uiSubmitAction;

	public InputActionReference MoveActionRef => moveAction;

	public InputActionReference LookActionRef => lookAction;

	public InputActionReference JumpActionRef => jumpAction;

	public InputActionReference RunActionRef => runAction;

	public InputActionReference PrimaryActionRef => primaryAction;

	public InputActionReference SecondaryActionRef => secondaryAction;

	public InputActionReference MiddleClickActionRef => middleClickAction;

	public InputActionReference ScrollWheelActionRef => scrollWheelAction;

	public InputActionReference InteractActionRef => interactAction;

	public InputActionReference CancelActionRef => cancelAction;

	public InputActionReference CloseActionRef => closeAction;

	public InputActionReference ClearCartActionRef => clearCartAction;

	public InputActionReference PutDownActionRef => putDownAction;

	public InputActionReference RotateLeftActionRef => rotateLeftAction;

	public InputActionReference RotateRightActionRef => rotateRightAction;

	public InputActionReference SetPriceActionRef => setPriceAction;

	public InputActionReference EndDayActionRef => endDayAction;

	public InputActionReference SwitchTabActionRef => switchTabAction;

	public InputActionReference PauseActionRef => pauseAction;

	public InputActionReference ModifierActionRef => modifierAction;

	public InputActionReference ModifierAltActionRef => modifierAltAction;

	public InputActionReference LiftActionRef => liftAction;

	public InputActionReference LowerActionRef => lowerAction;

	public InputActionReference ReverseActionRef => reverseAction;

	public InputActionReference ForwardActionRef => forwardAction;

	public InputActionReference SteeringActionRef => steeringAction;

	public InputActionReference Hotkey1ActionRef => hotkey1Action;

	public InputActionReference Hotkey2ActionRef => hotkey2Action;

	public InputActionReference Hotkey3ActionRef => hotkey3Action;

	public InputActionReference Hotkey4ActionRef => hotkey4Action;

	public bool IsPressingJump => jumpAction?.action.IsPressed() ?? false;

	public bool IsPressingRun => runAction?.action.IsPressed() ?? false;

	public bool IsPressingPrimary => primaryAction?.action.IsPressed() ?? false;

	public bool IsPressingSecondary => secondaryAction?.action.IsPressed() ?? false;

	public bool IsPressingModifier => modifierAction?.action.IsPressed() ?? false;

	public bool IsPressingModifierAlt => modifierAltAction?.action.IsPressed() ?? false;

	public bool IsPressingLift => liftAction?.action.IsPressed() ?? false;

	public bool IsPressingLower => lowerAction?.action.IsPressed() ?? false;

	public (float, float) MovementInput => (moveAction?.action.ReadValue<Vector2>().x ?? 0f, moveAction?.action.ReadValue<Vector2>().y ?? 0f);

	public float HorizontalRotationInput => lookAction?.action.ReadValue<Vector2>().x ?? 0f;

	public float VerticalRotationInput => lookAction?.action.ReadValue<Vector2>().y ?? 0f;

	public float ScrollWheelInput => scrollWheelAction?.action.ReadValue<float>() ?? 0f;

	private new void Awake()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public InputActionReference GetInputActionForKeyCode(KeyCode keyCode)
	{
		if (keyCodeToActionMap == null)
		{
			InitializeKeyCodeMap();
		}
		keyCodeToActionMap.TryGetValue(keyCode, out var value);
		return value;
	}

	public string GetBindingDisplayString(InputActionReference actionRef, bool useGamepad = false)
	{
		if (actionRef == null || actionRef.action == null)
		{
			return "";
		}
		int num = (useGamepad ? 1 : 0);
		if (actionRef.action.bindings.Count > num)
		{
			return actionRef.action.GetBindingDisplayString(num);
		}
		return actionRef.action.GetBindingDisplayString();
	}

	public bool WasActionTriggeredThisFrame(InputActionReference actionRef)
	{
		return actionRef?.action.WasPressedThisFrame() ?? false;
	}

	public bool WasActionReleasedThisFrame(InputActionReference actionRef)
	{
		return actionRef?.action.WasReleasedThisFrame() ?? false;
	}

	public bool IsActionPressed(InputActionReference actionRef)
	{
		return actionRef?.action.IsPressed() ?? false;
	}

	private void InitializeKeyCodeMap()
	{
		keyCodeToActionMap = new Dictionary<KeyCode, InputActionReference>
		{
			{
				KeyCode.Mouse0,
				primaryAction
			},
			{
				KeyCode.Mouse1,
				secondaryAction
			},
			{
				KeyCode.Mouse2,
				scrollWheelAction
			},
			{
				KeyCode.E,
				interactAction
			},
			{
				KeyCode.R,
				cancelAction
			},
			{
				KeyCode.C,
				closeAction
			},
			{
				KeyCode.V,
				clearCartAction
			},
			{
				KeyCode.G,
				putDownAction
			},
			{
				KeyCode.D,
				rotateLeftAction
			},
			{
				KeyCode.Q,
				rotateRightAction
			},
			{
				KeyCode.T,
				setPriceAction
			},
			{
				KeyCode.LeftShift,
				modifierAction
			},
			{
				KeyCode.LeftControl,
				modifierAltAction
			},
			{
				KeyCode.Tab,
				switchTabAction
			},
			{
				KeyCode.Escape,
				pauseAction
			},
			{
				KeyCode.UpArrow,
				liftAction
			},
			{
				KeyCode.DownArrow,
				lowerAction
			},
			{
				KeyCode.Alpha1,
				hotkey1Action
			},
			{
				KeyCode.Alpha2,
				hotkey2Action
			},
			{
				KeyCode.Alpha3,
				hotkey3Action
			},
			{
				KeyCode.Alpha4,
				hotkey4Action
			},
			{
				KeyCode.F,
				packAction
			},
			{
				KeyCode.Space,
				palletInteractionAction
			}
		};
	}

	private void OnEnable()
	{
		EnableAllActions();
	}

	private void EnableAllActions()
	{
		moveAction?.action.Enable();
		lookAction?.action.Enable();
		jumpAction?.action.Enable();
		runAction?.action.Enable();
		primaryAction?.action.Enable();
		secondaryAction?.action.Enable();
		middleClickAction?.action.Enable();
		scrollWheelAction?.action.Enable();
		interactAction?.action.Enable();
		cancelAction?.action.Enable();
		closeAction?.action.Enable();
		clearCartAction?.action.Enable();
		putDownAction?.action.Enable();
		rotateLeftAction?.action.Enable();
		rotateRightAction?.action.Enable();
		setPriceAction?.action.Enable();
		endDayAction?.action.Enable();
		switchTabAction?.action.Enable();
		pauseAction?.action.Enable();
		modifierAction?.action.Enable();
		modifierAltAction?.action.Enable();
		hotkey1Action?.action.Enable();
		hotkey2Action?.action.Enable();
		hotkey3Action?.action.Enable();
		hotkey4Action?.action.Enable();
		packAction?.action.Enable();
		palletInteractionAction?.action.Enable();
	}

	private void DisableAllActions()
	{
		moveAction?.action.Disable();
		lookAction?.action.Disable();
		jumpAction?.action.Disable();
		runAction?.action.Disable();
		primaryAction?.action.Disable();
		secondaryAction?.action.Disable();
		middleClickAction?.action.Disable();
		scrollWheelAction?.action.Disable();
		interactAction?.action.Disable();
		cancelAction?.action.Disable();
		closeAction?.action.Disable();
		clearCartAction?.action.Disable();
		putDownAction?.action.Disable();
		rotateLeftAction?.action.Disable();
		rotateRightAction?.action.Disable();
		setPriceAction?.action.Disable();
		endDayAction?.action.Disable();
		switchTabAction?.action.Disable();
		pauseAction?.action.Disable();
		modifierAction?.action.Disable();
		modifierAltAction?.action.Disable();
		hotkey1Action?.action.Disable();
		hotkey2Action?.action.Disable();
		hotkey3Action?.action.Disable();
		hotkey4Action?.action.Disable();
		packAction?.action.Disable();
		palletInteractionAction?.action.Disable();
	}

	private void Update()
	{
		if (WasActionTriggeredThisFrame(primaryAction))
		{
			leftMouseDownAction?.Invoke();
		}
		if (WasActionTriggeredThisFrame(secondaryAction))
		{
			rightMouseDownAction?.Invoke();
		}
		if (WasActionReleasedThisFrame(primaryAction))
		{
			leftMouseUpAction?.Invoke();
		}
		if ((bool)eventSystem)
		{
			if ((bool)eventSystem.currentSelectedGameObject && lastSelectedElement != eventSystem.currentSelectedGameObject)
			{
				lastSelectedElement = eventSystem.currentSelectedGameObject;
			}
			if (!eventSystem.currentSelectedGameObject && (bool)lastSelectedElement)
			{
				eventSystem.SetSelectedGameObject(lastSelectedElement);
			}
		}
	}

	public bool IsLastTouchOnUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}

	public List<InputActionReference> GetAllActionRefs()
	{
		if (GameManager.isDemo)
		{
			return new List<InputActionReference>
			{
				moveAction, jumpAction, runAction, interactAction, closeAction, clearCartAction, putDownAction, rotateLeftAction, rotateRightAction, setPriceAction,
				endDayAction, pauseAction, modifierAction, hotkey1Action, hotkey2Action, hotkey3Action, hotkey4Action, packAction, palletInteractionAction
			};
		}
		return new List<InputActionReference>
		{
			moveAction, jumpAction, runAction, interactAction, closeAction, clearCartAction, putDownAction, rotateLeftAction, rotateRightAction, setPriceAction,
			endDayAction, switchTabAction, pauseAction, modifierAction, liftAction, lowerAction, hotkey1Action, hotkey2Action, hotkey3Action, packAction,
			palletInteractionAction, reverseAction, forwardAction
		};
	}

	public List<KeyValuePair<InputActionReference, string>> GetAllActionPairs()
	{
		if (GameManager.isDemo)
		{
			return new List<KeyValuePair<InputActionReference, string>>
			{
				new KeyValuePair<InputActionReference, string>(moveAction, "controls_move"),
				new KeyValuePair<InputActionReference, string>(jumpAction, "controls_jump"),
				new KeyValuePair<InputActionReference, string>(runAction, "controls_run"),
				new KeyValuePair<InputActionReference, string>(interactAction, "controls_interact"),
				new KeyValuePair<InputActionReference, string>(closeAction, "controls_close"),
				new KeyValuePair<InputActionReference, string>(clearCartAction, "controls_clear_cart"),
				new KeyValuePair<InputActionReference, string>(putDownAction, "controls_put_down"),
				new KeyValuePair<InputActionReference, string>(rotateLeftAction, "controls_rotate_left"),
				new KeyValuePair<InputActionReference, string>(rotateRightAction, "controls_rotate_right"),
				new KeyValuePair<InputActionReference, string>(setPriceAction, "controls_set_price"),
				new KeyValuePair<InputActionReference, string>(endDayAction, "controls_end_day"),
				new KeyValuePair<InputActionReference, string>(pauseAction, "controls_pause"),
				new KeyValuePair<InputActionReference, string>(modifierAction, "controls_modifier"),
				new KeyValuePair<InputActionReference, string>(hotkey1Action, "controls_hotkey_1"),
				new KeyValuePair<InputActionReference, string>(hotkey2Action, "controls_hotkey_2"),
				new KeyValuePair<InputActionReference, string>(hotkey3Action, "controls_hotkey_3"),
				new KeyValuePair<InputActionReference, string>(packAction, "controls_pack"),
				new KeyValuePair<InputActionReference, string>(palletInteractionAction, "controls_pallet_interaction")
			};
		}
		return new List<KeyValuePair<InputActionReference, string>>
		{
			new KeyValuePair<InputActionReference, string>(moveAction, "controls_move"),
			new KeyValuePair<InputActionReference, string>(jumpAction, "controls_jump"),
			new KeyValuePair<InputActionReference, string>(runAction, "controls_run"),
			new KeyValuePair<InputActionReference, string>(interactAction, "controls_interact"),
			new KeyValuePair<InputActionReference, string>(closeAction, "controls_close"),
			new KeyValuePair<InputActionReference, string>(clearCartAction, "controls_clear_cart"),
			new KeyValuePair<InputActionReference, string>(putDownAction, "controls_put_down"),
			new KeyValuePair<InputActionReference, string>(rotateLeftAction, "controls_rotate_left"),
			new KeyValuePair<InputActionReference, string>(rotateRightAction, "controls_rotate_right"),
			new KeyValuePair<InputActionReference, string>(setPriceAction, "controls_set_price"),
			new KeyValuePair<InputActionReference, string>(endDayAction, "controls_end_day"),
			new KeyValuePair<InputActionReference, string>(switchTabAction, "controls_switch_tab"),
			new KeyValuePair<InputActionReference, string>(pauseAction, "controls_pause"),
			new KeyValuePair<InputActionReference, string>(modifierAction, "controls_modifier"),
			new KeyValuePair<InputActionReference, string>(liftAction, "controls_lift"),
			new KeyValuePair<InputActionReference, string>(lowerAction, "controls_lower"),
			new KeyValuePair<InputActionReference, string>(hotkey1Action, "controls_hotkey_1"),
			new KeyValuePair<InputActionReference, string>(hotkey2Action, "controls_hotkey_2"),
			new KeyValuePair<InputActionReference, string>(hotkey3Action, "controls_hotkey_3"),
			new KeyValuePair<InputActionReference, string>(hotkey4Action, "controls_hotkey_4"),
			new KeyValuePair<InputActionReference, string>(packAction, "controls_pack"),
			new KeyValuePair<InputActionReference, string>(palletInteractionAction, "controls_pallet_interaction"),
			new KeyValuePair<InputActionReference, string>(forwardAction, "controls_forward"),
			new KeyValuePair<InputActionReference, string>(reverseAction, "controls_reverse")
		};
	}

	public void SelectElement(GameObject gameObject)
	{
		if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
		{
			eventSystem.SetSelectedGameObject(gameObject);
		}
	}

	public void ClearSelection()
	{
		lastSelectedElement = null;
		eventSystem.SetSelectedGameObject(null);
	}

	public GameObject GetSelectedElement()
	{
		return eventSystem.currentSelectedGameObject;
	}

	public Sprite GetGamepadGlyphSprite(GamepadGlyph glyph, bool isXbox)
	{
		if (SingletonBehaviour<LastInputDeviceTracker>.Instance.UseGamepad)
		{
			if (!isXbox)
			{
				return psGlyphSprites[glyph];
			}
			return xboxGlyphSprites[glyph];
		}
		return null;
	}

	public Sprite GetGamepadGlyphSprite(InputActionReference actionRef)
	{
		string bindingPath = GetBindingPath(actionRef, useGamepad: true);
		return GetGamepadSprite(bindingPath);
	}

	private string GetBindingPath(InputActionReference actionRef, bool useGamepad)
	{
		if (actionRef?.action == null)
		{
			return "";
		}
		ReadOnlyArray<InputBinding> bindings = actionRef.action.bindings;
		string value = (useGamepad ? "Gamepad" : "KeyboardMouse");
		for (int i = 0; i < bindings.Count; i++)
		{
			InputBinding inputBinding = bindings[i];
			if (!inputBinding.isComposite && !inputBinding.isPartOfComposite && (string.IsNullOrEmpty(inputBinding.groups) || inputBinding.groups.Contains(value)))
			{
				return inputBinding.effectivePath;
			}
		}
		for (int j = 0; j < bindings.Count; j++)
		{
			if (!bindings[j].isComposite && !bindings[j].isPartOfComposite)
			{
				return bindings[j].effectivePath;
			}
		}
		return "";
	}

	public Sprite GetGamepadSprite(string bindingPath)
	{
		bindingPath = bindingPath.ToLower();
		if (bindingPath.Contains("buttonsouth"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.South, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("buttoneast"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.East, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("buttonwest"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.West, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("buttonnorth"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.North, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("lefttrigger"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.LeftTrigger, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("righttrigger"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.RightTrigger, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("leftshoulder"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.LeftBumper, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("rightshoulder"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.RightBumper, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("select"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.Select, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("start"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.Start, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("dpad/left"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.DpadLeft, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("dpad/right"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.DpadRight, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("scroll"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.DpadVertical, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("leftstickpress"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.LeftStickPress, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		if (bindingPath.Contains("rightstickpress"))
		{
			return GetGamepadGlyphSprite(GamepadGlyph.RightStickPress, !SingletonBehaviour<LastInputDeviceTracker>.Instance.IsPSController());
		}
		return null;
	}
}
