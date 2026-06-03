using DG.Tweening;
using UnityEngine;

public class ForkController : MonoBehaviour
{
	public Transform fork;

	public Transform mast;

	public float speedTranslate;

	public Transform forkTopPosition;

	public Transform forkBottomPosition;

	public Transform mastTopPosition;

	public Transform mastBottomPosition;

	public Transform handle;

	private const float HANDLE_TARGET_X = 15f;

	private bool mastMoveTrue = true;

	private Vector3 handleForwardEuler;

	private Vector3 handleBackwardEuler;

	private void Awake()
	{
		handleForwardEuler = new Vector3(15f, handle.localEulerAngles.y, handle.localEulerAngles.z);
		handleBackwardEuler = new Vector3(345f, handle.localEulerAngles.y, handle.localEulerAngles.z);
	}

	public void Update()
	{
		if (fork.transform.localPosition.y >= mastTopPosition.localPosition.y && fork.transform.localPosition.y < forkTopPosition.localPosition.y)
		{
			mastMoveTrue = true;
		}
		else
		{
			mastMoveTrue = false;
		}
		if (fork.transform.localPosition.y <= mastTopPosition.localPosition.y)
		{
			mastMoveTrue = false;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition, forkTopPosition.localPosition, speedTranslate * Time.deltaTime);
			if (mastMoveTrue)
			{
				mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition, mastTopPosition.localPosition, speedTranslate * Time.deltaTime);
			}
		}
		if (Input.GetKey(KeyCode.LeftControl))
		{
			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition, forkBottomPosition.localPosition, speedTranslate * Time.deltaTime);
			if (mastMoveTrue)
			{
				mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition, mastBottomPosition.localPosition, speedTranslate * Time.deltaTime);
			}
		}
		if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftShift))
		{
			Debug.Log("go zero");
			handle.DOKill();
			handle.DOLocalRotate(Vector3.zero, 45f).SetSpeedBased(isSpeedBased: true);
		}
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			Debug.Log("go backward");
			handle.DOKill();
			handle.DOLocalRotate(handleBackwardEuler, 45f).SetSpeedBased(isSpeedBased: true);
		}
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			Debug.Log("go forward");
			handle.DOKill();
			handle.DOLocalRotate(handleForwardEuler, 45f).SetSpeedBased(isSpeedBased: true);
		}
	}
}
