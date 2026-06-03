using UnityEngine;
using UnityEngine.UI;

public class SelectableUI : MonoBehaviour
{
	public virtual Selectable GetSelectable()
	{
		return null;
	}

	public virtual Selectable RefreshNavigation(Selectable up, Selectable down, Selectable left, Selectable right)
	{
		if (GetSelectable() == null)
		{
			return null;
		}
		Navigation navigation = GetSelectable().navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnUp = up;
		navigation.selectOnDown = down;
		navigation.selectOnLeft = left;
		navigation.selectOnRight = right;
		GetSelectable().navigation = navigation;
		return GetSelectable();
	}

	public virtual Selectable GetAvailableSelectable()
	{
		Navigation navigation = GetSelectable().navigation;
		if (navigation.selectOnRight != null)
		{
			return navigation.selectOnRight;
		}
		if (navigation.selectOnUp != null)
		{
			return navigation.selectOnUp;
		}
		if (navigation.selectOnLeft != null)
		{
			return navigation.selectOnLeft;
		}
		if (navigation.selectOnDown != null)
		{
			return navigation.selectOnDown;
		}
		return null;
	}
}
