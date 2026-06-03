using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public static class TransformExtensions
{
	public static TweenerCore<Vector3, Path, PathOptions> DoCurvedMove(this Transform t, Vector3 to, float duration, Vector3 topPosition)
	{
		Vector3 position = t.position;
		float num = Mathf.Abs(position.x - to.x);
		Mathf.Abs(position.y - to.y);
		float num2 = Mathf.Abs(position.z - to.z);
		float x = ((position.x > to.x) ? (to.x - num / 4f) : (to.x + num / 4f));
		float x2 = Mathf.Min(t.position.x, to.x) + num / 2f;
		float y = topPosition.y;
		float z = Mathf.Max(t.localPosition.z, to.z) + num2 / 2f;
		Vector3 vector = new Vector3(x2, y, z);
		Vector3 vector2 = new Vector3(x, y, z);
		Vector3[] path = new Vector3[3] { to, vector, vector2 };
		return t.DOPath(path, duration, DG.Tweening.PathType.CubicBezier);
	}

	public static TweenerCore<Vector3, Path, PathOptions> DoCurvedLocalMove(this Transform t, Vector3 to, float duration, float heightMultiplier)
	{
		float num = Mathf.Abs(t.localPosition.x - to.x);
		float y = Mathf.Max(t.localPosition.y, to.y) + Mathf.Lerp(0.25f, 0.5f, num / 4f);
		Vector2 vector = new Vector2(t.localPosition.x, y);
		Vector2 vector2 = new Vector2(to.x, y);
		Vector3[] path = new Vector3[3] { to, vector, vector2 };
		return t.DOLocalPath(path, duration, DG.Tweening.PathType.CubicBezier);
	}

	public static TweenerCore<Vector3, Path, PathOptions> DoCurvedLocalMove3D(this Transform t, Vector3 to, float duration)
	{
		Vector3 localPosition = t.localPosition;
		float num = Mathf.Abs(localPosition.x - to.x);
		float num2 = Mathf.Abs(localPosition.y - to.y);
		float num3 = Mathf.Abs(localPosition.z - to.z);
		float x = ((localPosition.x > to.x) ? (to.x - num / 4f) : (to.x + num / 4f));
		float x2 = Mathf.Min(t.localPosition.x, to.x) + num / 2f;
		float y = Mathf.Max(t.localPosition.y, to.y) + num2 / 2f;
		float z = Mathf.Max(t.localPosition.z, to.z) + num3 / 2f;
		Vector3 vector = new Vector3(x2, y, z);
		Vector3 vector2 = new Vector3(x, y, z);
		Vector3[] path = new Vector3[3] { to, vector, vector2 };
		return t.DOLocalPath(path, duration, DG.Tweening.PathType.CubicBezier);
	}

	public static TweenerCore<Vector3, Path, PathOptions> DoCurvedLocalMove3D(this Transform t, Vector3 to, float duration, float heightAddition)
	{
		Vector3 localPosition = t.localPosition;
		Vector3 normalized = (to - localPosition).normalized;
		float num = Vector3.Distance(localPosition, to);
		Vector3 vector = localPosition + normalized * (num / 4f) + Vector3.up * heightAddition;
		Vector3 vector2 = localPosition - normalized * (num / 2f) + Vector3.up * heightAddition;
		Vector3[] path = new Vector3[3] { to, vector, vector2 };
		return t.DOLocalPath(path, duration, DG.Tweening.PathType.CubicBezier);
	}

	public static TweenerCore<Vector3, Path, PathOptions> DoCurvedMove3D(this Transform t, Vector3 to, float duration)
	{
		Vector3 position = t.position;
		float num = Mathf.Abs(position.x - to.x);
		float num2 = Mathf.Abs(position.y - to.y);
		float num3 = Mathf.Abs(position.z - to.z);
		float num4 = Vector3.Distance(position, to);
		float y = to.y + num4 / 6f;
		Mathf.Min(t.position.x, to.x);
		_ = num / 2f;
		Mathf.Min(t.position.y, to.y);
		_ = num2 / 2f;
		Mathf.Min(t.position.z, to.z);
		_ = num3 / 2f;
		Vector3 vector = new Vector3(position.x, y, position.z);
		Vector3 vector2 = new Vector3(to.x, y, to.z);
		Vector3[] path = new Vector3[3] { to, vector, vector2 };
		return t.DOPath(path, duration, DG.Tweening.PathType.CubicBezier);
	}

	public static void Bounce(this Transform t, Vector3 initialScale, float duration)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(t.DOScale(1.2f * initialScale, duration / 3f).SetSpeedBased(isSpeedBased: true));
		sequence.Append(t.DOScale(0.85f * initialScale, duration / 3f).SetSpeedBased(isSpeedBased: true));
		sequence.Append(t.DOScale(1f * initialScale, duration / 3f).SetSpeedBased(isSpeedBased: true));
		sequence.Play();
	}
}
