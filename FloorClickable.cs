using DG.Tweening;
using ToolBox.Serialization;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FloorClickable : Interactable
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private int id;

	[SerializeField]
	private int defaultDecorationIndex;

	[SerializeField]
	private DecorationUI.DecorationType decorationType;

	private static float INTERACTION_DISTANCE = 6f;

	private static string LAST_USED_DECORATION_INDEX_FLOOR_KEY = "LFK_";

	private static string LAST_USED_DECORATION_INDEX_WALL_KEY = "LFW_";

	private const float PROGRESS_MIN = -0.12f;

	private const float PROGRESS_MAX_PAINT = 0.9f;

	private const float PROGRESS_MAX = 1.12f;

	private MaterialPropertyBlock materialPropertyBlock;

	private Tween paintTween;

	private int currentDecorationIndex = -1;

	public static readonly int BaseColorID = Shader.PropertyToID("_BaseColorA");

	public static readonly int BaseColorID2 = Shader.PropertyToID("_BaseColorB");

	private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");

	private static readonly int PaintMapID = Shader.PropertyToID("_PaintMap");

	private static readonly int BaseNormalID = Shader.PropertyToID("_BaseNormal");

	private static readonly int PaintNormalID = Shader.PropertyToID("_PaintNormal");

	private static readonly int PaintProgressID = Shader.PropertyToID("_PaintProgress");

	private static readonly int HoverID = Shader.PropertyToID("_Hover");

	private float currentProgress;

	public int CurrentDecorationIndex => currentDecorationIndex;

	public int DefaultDecorationIndex => defaultDecorationIndex;

	public int ID => id;

	public DecorationUI.DecorationType DecorationType => decorationType;

	private string LAST_USED_DECORATION_INDEX_KEY
	{
		get
		{
			if (decorationType != DecorationUI.DecorationType.FLOOR)
			{
				return LAST_USED_DECORATION_INDEX_WALL_KEY;
			}
			return LAST_USED_DECORATION_INDEX_FLOOR_KEY;
		}
	}

	private void Start()
	{
		materialPropertyBlock = new MaterialPropertyBlock();
		if (DataSerializer.HasKey(LAST_USED_DECORATION_INDEX_KEY + id))
		{
			int materialInstant = DataSerializer.Load<int>(LAST_USED_DECORATION_INDEX_KEY + id);
			SetMaterialInstant(materialInstant);
		}
		else
		{
			SetMaterialInstant(defaultDecorationIndex);
		}
	}

	private void SetMaterialInstant(int decorationIndex)
	{
		currentDecorationIndex = decorationIndex;
		Texture texture = SingletonBehaviour<DecorationManager>.Instance.GetTexture(decorationType, decorationIndex);
		Texture normalTexture = SingletonBehaviour<DecorationManager>.Instance.GetNormalTexture(decorationType, decorationIndex);
		Color color = SingletonBehaviour<DecorationManager>.Instance.GetColor(decorationType, decorationIndex);
		materialPropertyBlock.SetTexture(BaseMapID, texture);
		materialPropertyBlock.SetColor(BaseColorID, color);
		if (normalTexture != null)
		{
			materialPropertyBlock.SetTexture(BaseNormalID, normalTexture);
		}
		materialPropertyBlock.SetFloat(PaintProgressID, -0.12f);
		meshRenderer.sharedMaterial = SingletonBehaviour<DecorationManager>.Instance.GetMaterial(decorationType, decorationIndex);
		meshRenderer.SetPropertyBlock(null);
	}

	public override void OnMouseHoverStarted()
	{
		base.OnMouseHoverStarted();
		if (decorationType == DecorationUI.DecorationType.FLOOR)
		{
			meshRenderer.sharedMaterial = SingletonBehaviour<DecorationManager>.Instance.DefaultFloorMaterial;
		}
		else if (decorationType == DecorationUI.DecorationType.WALL)
		{
			meshRenderer.sharedMaterial = SingletonBehaviour<DecorationManager>.Instance.DefaultWallMaterial;
		}
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
		SetHover(isHovering: true);
		EventManager.NotifyEvent(DecorationEvents.FLOOR_HOVER_STARTED, this);
	}

	public override void OnMouseHoverEnded()
	{
		base.OnMouseHoverEnded();
		SetHover(isHovering: false);
		if (paintTween != null)
		{
			paintTween.Kill();
		}
		SetMaterialInstant(currentDecorationIndex);
		EventManager.NotifyEvent(DecorationEvents.FLOOR_HOVER_ENDED);
	}

	public void SetHover(bool isHovering)
	{
		materialPropertyBlock.SetFloat(HoverID, isHovering ? 1 : 0);
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}

	private void SetProgress(float value)
	{
		currentProgress = value;
		materialPropertyBlock.SetFloat(PaintProgressID, value);
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		Texture texture = SingletonBehaviour<DecorationManager>.Instance.GetTexture(decorationType, SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex);
		Texture normalTexture = SingletonBehaviour<DecorationManager>.Instance.GetNormalTexture(decorationType, SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex);
		Color color = SingletonBehaviour<DecorationManager>.Instance.GetColor(decorationType, SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex);
		materialPropertyBlock.SetTexture(PaintMapID, texture);
		materialPropertyBlock.SetColor(BaseColorID2, color);
		if (normalTexture != null)
		{
			materialPropertyBlock.SetTexture(PaintNormalID, normalTexture);
		}
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
		paintTween = DOTween.To(() => materialPropertyBlock.GetFloat(PaintProgressID), delegate(float x)
		{
			SetProgress(x);
		}, 1.12f, 0.9f).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			currentDecorationIndex = SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex;
			DataSerializer.Save(LAST_USED_DECORATION_INDEX_KEY + id, SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex);
		});
	}

	public override void OnMouseButtonUp()
	{
		base.OnMouseButtonUp();
		if (paintTween != null)
		{
			if (currentProgress > 0.9f)
			{
				currentDecorationIndex = SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex;
				DataSerializer.Save(LAST_USED_DECORATION_INDEX_KEY + id, SingletonBehaviour<DecorationManager>.Instance.lastUsedDecorationIndex);
			}
			else
			{
				paintTween.Kill();
				SetProgress(-0.12f);
			}
		}
	}

	public override float GetInteractionDistance()
	{
		return INTERACTION_DISTANCE;
	}
}
