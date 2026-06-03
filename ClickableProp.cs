using UnityEngine;
using UnityEngine.Events;

public class ClickableProp : MonoBehaviour
{
	[SerializeField]
	private Outline outline;

	[SerializeField]
	private UnityEvent onClickAction;
}
