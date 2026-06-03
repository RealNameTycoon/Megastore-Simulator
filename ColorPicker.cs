using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class ColorPicker : UIBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler, IPointerUpHandler, IMaterialModifier
{
	private enum PointerDownLocation
	{
		HueCircle,
		SVSquare,
		Outside
	}

	private const float Recip2Pi = 1f / (2f * MathF.PI);

	private const string ColorPickerShaderName = "UI/ColorPicker";

	private static readonly int _HSV = Shader.PropertyToID("_HSV");

	private static readonly int _AspectRatio = Shader.PropertyToID("_AspectRatio");

	private static readonly int _HueCircleInner = Shader.PropertyToID("_HueCircleInner");

	private static readonly int _HueSelectorInner = Shader.PropertyToID("_HueSelectorInner");

	private static readonly int _SVSquareSize = Shader.PropertyToID("_SVSquareSize");

	[SerializeField]
	[Range(0f, 0.5f)]
	private float _hueCircleInnerRadius = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _hueSelectorInnerRadius = 0.8f;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float _saturationValueSquareSize = 0.25f;

	[SerializeField]
	[FormerlySerializedAs("colorPickerShader")]
	private Shader _colorPickerShader;

	private Material _generatedMaterial;

	private PointerDownLocation _pointerDownLocation = PointerDownLocation.Outside;

	private float _h;

	private float _s;

	private float _v;

	private RectTransform _rectTransform => (RectTransform)base.transform;

	public Color color
	{
		get
		{
			return Color.HSVToRGB(_h, _s, _v);
		}
		set
		{
			Color.RGBToHSV(value, out _h, out _s, out _v);
			ApplyColor();
		}
	}

	public event Action<Color> onColorChanged;

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		UpdateAspectRatio();
	}

	private void UpdateAspectRatio()
	{
		if (_generatedMaterial != null)
		{
			Rect rect = _rectTransform.rect;
			_generatedMaterial.SetFloat(_AspectRatio, rect.width / rect.height);
		}
	}

	public Material GetModifiedMaterial(Material baseMaterial)
	{
		if (_generatedMaterial == null)
		{
			_generatedMaterial = new Material(_colorPickerShader);
			_generatedMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		UpdateAspectRatio();
		ApplyColor();
		ApplySizesOfElements();
		return _generatedMaterial;
	}

	public void ApplySizesOfElements()
	{
		if (_generatedMaterial != null)
		{
			_generatedMaterial.SetFloat(_HueCircleInner, _hueCircleInnerRadius);
			_generatedMaterial.SetFloat(_HueSelectorInner, _hueSelectorInnerRadius);
			_generatedMaterial.SetFloat(_SVSquareSize, _saturationValueSquareSize);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!(_generatedMaterial == null))
		{
			Vector2 relativePosition = GetRelativePosition(eventData);
			if (_pointerDownLocation == PointerDownLocation.HueCircle)
			{
				_h = (Mathf.Atan2(relativePosition.y, relativePosition.x) * (1f / (2f * MathF.PI)) + 1f) % 1f;
				ApplyColor();
			}
			if (_pointerDownLocation == PointerDownLocation.SVSquare)
			{
				float num = _generatedMaterial.GetFloat(_SVSquareSize);
				_s = Mathf.InverseLerp(0f - num, num, relativePosition.x);
				_v = Mathf.InverseLerp(0f - num, num, relativePosition.y);
				ApplyColor();
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_generatedMaterial == null)
		{
			return;
		}
		Vector2 relativePosition = GetRelativePosition(eventData);
		float magnitude = relativePosition.magnitude;
		if (magnitude < 0.5f && magnitude > _generatedMaterial.GetFloat(_HueCircleInner))
		{
			_pointerDownLocation = PointerDownLocation.HueCircle;
			_h = (Mathf.Atan2(relativePosition.y, relativePosition.x) * (1f / (2f * MathF.PI)) + 1f) % 1f;
			ApplyColor();
			return;
		}
		float num = _generatedMaterial.GetFloat(_SVSquareSize);
		if (relativePosition.x >= 0f - num && relativePosition.x <= num && relativePosition.y >= 0f - num && relativePosition.y <= num)
		{
			_pointerDownLocation = PointerDownLocation.SVSquare;
			_s = Mathf.InverseLerp(0f - num, num, relativePosition.x);
			_v = Mathf.InverseLerp(0f - num, num, relativePosition.y);
			ApplyColor();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_pointerDownLocation = PointerDownLocation.Outside;
	}

	private void ApplyColor()
	{
		if (!(_generatedMaterial == null))
		{
			_generatedMaterial.SetVector(_HSV, new Vector3(_h, _s, _v));
			this.onColorChanged?.Invoke(color);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_generatedMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(_generatedMaterial);
		}
	}

	public Vector2 GetRelativePosition(PointerEventData eventData)
	{
		Rect squaredRect = GetSquaredRect();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint);
		return new Vector2(InverseLerpUnclamped(squaredRect.xMin, squaredRect.xMax, localPoint.x), InverseLerpUnclamped(squaredRect.yMin, squaredRect.yMax, localPoint.y)) - Vector2.one * 0.5f;
	}

	public Rect GetSquaredRect()
	{
		Rect rect = _rectTransform.rect;
		float num = Mathf.Min(rect.width, rect.height);
		return new Rect(rect.center - Vector2.one * num * 0.5f, Vector2.one * num);
	}

	public float InverseLerpUnclamped(float min, float max, float value)
	{
		return (value - min) / (max - min);
	}
}
