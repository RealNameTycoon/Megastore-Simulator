using DG.Tweening;
using UnityEngine;

public class TrailerCamera : MonoBehaviour
{
	[SerializeField]
	private DOTweenPath trailerPath;

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private Transform targetCam;

	private void Play()
	{
		trailerPath.DORestart();
	}

	private void RotateCam()
	{
		cam.transform.DOLocalRotate(targetCam.localEulerAngles, 40f).SetEase(Ease.Linear);
	}

	private void MoveCam()
	{
		cam.transform.DOMove(targetCam.position, 8f).SetEase(Ease.Linear);
	}

	private void SetTimeScale(int scale)
	{
		Time.timeScale = scale;
	}

	private void Update()
	{
	}
}
