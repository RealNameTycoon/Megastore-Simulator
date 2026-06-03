using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour
{
	private bool trig;

	private bool open;

	public float smooth = 2f;

	public float DoorOpenAngle = 85f;

	private Vector3 defaulRot;

	private Vector3 openRot;

	public Text txt;

	private void Start()
	{
		defaulRot = base.transform.eulerAngles;
		openRot = new Vector3(defaulRot.x, defaulRot.y + DoorOpenAngle, defaulRot.z);
	}

	private void Update()
	{
		if (open)
		{
			base.transform.eulerAngles = Vector3.Slerp(base.transform.eulerAngles, openRot, Time.deltaTime * smooth);
		}
		else
		{
			base.transform.eulerAngles = Vector3.Slerp(base.transform.eulerAngles, defaulRot, Time.deltaTime * smooth);
		}
		if (Input.GetKeyDown(KeyCode.E) && trig)
		{
			open = !open;
		}
		if (trig)
		{
			if (open)
			{
				txt.text = "Close E";
			}
			else
			{
				txt.text = "Open E";
			}
		}
	}

	private void OnTriggerEnter(Collider coll)
	{
		if (coll.tag == "Player")
		{
			if (!open)
			{
				txt.text = "Close E ";
			}
			else
			{
				txt.text = "Open E";
			}
			trig = true;
		}
	}

	private void OnTriggerExit(Collider coll)
	{
		if (coll.tag == "Player")
		{
			txt.text = " ";
			trig = false;
		}
	}
}
