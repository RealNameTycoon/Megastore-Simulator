using System.Collections;
using UnityEngine;

public class ConnectionController : SingletonBehaviour<ConnectionController>
{
	[SerializeField]
	private Canvas offlinePopup;

	private bool connectedToNetwork;

	private WaitForSeconds networkWaiter = new WaitForSeconds(1f);

	private void Start()
	{
	}

	public void OnRemoveAdsPurchased()
	{
		StopAllCoroutines();
		offlinePopup.enabled = false;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!offlinePopup.enabled);
	}

	private IEnumerator CheckNetworkRoutine()
	{
		while (true)
		{
			if (connectedToNetwork != (Application.internetReachability != NetworkReachability.NotReachable))
			{
				connectedToNetwork = Application.internetReachability != NetworkReachability.NotReachable;
				if (!connectedToNetwork)
				{
					offlinePopup.enabled = true;
					SingletonBehaviour<PlayerLook>.Instance.LockCursor(!offlinePopup.enabled);
				}
				else
				{
					offlinePopup.enabled = false;
					SingletonBehaviour<PlayerLook>.Instance.LockCursor(!offlinePopup.enabled);
				}
			}
			yield return networkWaiter;
		}
	}

	public void OnRetry()
	{
		offlinePopup.enabled = Application.internetReachability == NetworkReachability.NotReachable;
		SingletonBehaviour<PlayerLook>.Instance.LockCursor(!offlinePopup.enabled);
	}
}
