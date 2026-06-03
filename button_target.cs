using UnityEngine;
using UnityEngine.UI;

public class button_target : MonoBehaviour
{
	public GameObject MY_target;

	private GameObject temp_target;

	private GameObject[] ALL_target;

	private void Start()
	{
		base.transform.GetChild(0).GetComponent<Text>().text = MY_target.name;
		if (ALL_target == null)
		{
			ALL_target = GameObject.FindGameObjectsWithTag("TAZOFX");
		}
		GameObject[] aLL_target = ALL_target;
		for (int i = 0; i < aLL_target.Length; i++)
		{
			aLL_target[i].SetActive(value: false);
		}
	}

	private void Update()
	{
	}

	public void ShowTarget()
	{
		ALL_target = GameObject.FindGameObjectsWithTag("TAZOFX");
		GameObject[] aLL_target = ALL_target;
		for (int i = 0; i < aLL_target.Length; i++)
		{
			aLL_target[i].SetActive(value: false);
		}
		MY_target.SetActive(value: true);
	}
}
