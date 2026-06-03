using UnityEngine;

public class BoxReturner : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == BoxManager.BOX_LAYER)
		{
			Box component = other.GetComponent<Box>();
			if (component != null && !component.Stored)
			{
				component.RigidBody.linearVelocity = Vector3.zero;
				component.RigidBody.angularVelocity = Vector3.zero;
				Vector3 instantSpawnPosition = SingletonBehaviour<BoxManager>.Instance.GetInstantSpawnPosition(component);
				component.transform.position = instantSpawnPosition;
				component.transform.rotation = SingletonBehaviour<BoxManager>.Instance.TutorialBoxTransform.rotation;
				component.SaveLocation();
			}
		}
	}
}
