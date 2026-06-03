using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectDeadZoneExtensions
{
	public static void KeepInDeadZone(this ScrollRect sr, RectTransform target, float topPaddingPx = 70f, float bottomPaddingPx = 70f, float leftPaddingPx = 0f, float rightPaddingPx = 0f, bool clampToBounds = true, bool forceRebuild = true)
	{
		if (sr == null || sr.content == null || target == null)
		{
			return;
		}
		RectTransform content = sr.content;
		RectTransform rectTransform = ((sr.viewport != null) ? sr.viewport : ((RectTransform)sr.transform));
		if (forceRebuild)
		{
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(content);
		}
		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform, content);
		Rect rect = rectTransform.rect;
		if (sr.vertical && bounds.size.y <= rect.height + 0.5f)
		{
			sr.vertical = false;
		}
		if (sr.horizontal && bounds.size.x <= rect.width + 0.5f)
		{
			sr.horizontal = false;
		}
		if (!sr.vertical && !sr.horizontal)
		{
			return;
		}
		sr.StopMovement();
		sr.velocity = Vector2.zero;
		Rect rectInSpace = GetRectInSpace(target, rectTransform);
		float num = rect.yMax - topPaddingPx;
		float num2 = rect.yMin + bottomPaddingPx;
		float num3 = rect.xMin + leftPaddingPx;
		float num4 = rect.xMax - rightPaddingPx;
		Vector2 zero = Vector2.zero;
		if (sr.vertical)
		{
			if (rectInSpace.yMax > num)
			{
				zero.y -= rectInSpace.yMax - num;
			}
			else if (rectInSpace.yMin < num2)
			{
				zero.y += num2 - rectInSpace.yMin;
			}
		}
		if (sr.horizontal)
		{
			if (rectInSpace.xMin < num3)
			{
				zero.x += num3 - rectInSpace.xMin;
			}
			else if (rectInSpace.xMax > num4)
			{
				zero.x -= rectInSpace.xMax - num4;
			}
		}
		if (Mathf.Abs(zero.x) < 0.5f)
		{
			zero.x = 0f;
		}
		if (Mathf.Abs(zero.y) < 0.5f)
		{
			zero.y = 0f;
		}
		if (!(zero == Vector2.zero))
		{
			Vector2 vector = content.anchoredPosition + zero;
			if (clampToBounds)
			{
				vector = ClampContentToViewport(sr, vector);
			}
			vector.x = Mathf.Round(vector.x);
			vector.y = Mathf.Round(vector.y);
			if (!((vector - content.anchoredPosition).sqrMagnitude < 0.25f))
			{
				content.anchoredPosition = vector;
			}
		}
	}

	private static Rect GetRectInSpace(RectTransform rt, RectTransform space)
	{
		Vector3[] array = new Vector3[4];
		rt.GetWorldCorners(array);
		for (int i = 0; i < 4; i++)
		{
			array[i] = space.InverseTransformPoint(array[i]);
		}
		float xmin = Mathf.Min(array[0].x, array[1].x, array[2].x, array[3].x);
		float xmax = Mathf.Max(array[0].x, array[1].x, array[2].x, array[3].x);
		float ymin = Mathf.Min(array[0].y, array[1].y, array[2].y, array[3].y);
		float ymax = Mathf.Max(array[0].y, array[1].y, array[2].y, array[3].y);
		return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
	}

	private static Vector2 ClampContentToViewport(ScrollRect sr, Vector2 anchoredPos)
	{
		RectTransform content = sr.content;
		RectTransform rectTransform = ((sr.viewport != null) ? sr.viewport : ((RectTransform)sr.transform));
		Vector3[] array = new Vector3[4];
		content.GetWorldCorners(array);
		Vector2 vector = anchoredPos - content.anchoredPosition;
		Vector3 vector2 = content.TransformVector(new Vector3(vector.x, vector.y, 0f));
		Vector3[] array2 = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			array2[i] = rectTransform.InverseTransformPoint(array[i] + vector2);
		}
		Rect rect = rectTransform.rect;
		float num = Mathf.Min(array2[0].x, array2[1].x, array2[2].x, array2[3].x);
		float num2 = Mathf.Max(array2[0].x, array2[1].x, array2[2].x, array2[3].x);
		float num3 = Mathf.Min(array2[0].y, array2[1].y, array2[2].y, array2[3].y);
		float num4 = Mathf.Max(array2[0].y, array2[1].y, array2[2].y, array2[3].y);
		Vector2 result = anchoredPos;
		if (sr.horizontal && num2 - num > rect.width)
		{
			if (num > rect.xMin)
			{
				result.x -= num - rect.xMin;
			}
			if (num2 < rect.xMax)
			{
				result.x += rect.xMax - num2;
			}
		}
		if (sr.vertical && num4 - num3 > rect.height)
		{
			if (num4 < rect.yMax)
			{
				result.y += rect.yMax - num4;
			}
			if (num3 > rect.yMin)
			{
				result.y -= num3 - rect.yMin;
			}
		}
		return result;
	}
}
