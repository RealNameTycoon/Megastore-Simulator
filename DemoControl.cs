using UnityEngine;

public class DemoControl : MonoBehaviour
{
	public float time;

	public GameObject ForDeleteCardboardBox_20x10x7_Animated;

	public GameObject ForDeleteCardboardBox_20x20x10_Animated;

	public GameObject ForDeleteCardboardBox_20x20x20_Animated;

	public GameObject ForDeleteCardboardBox_30x20x20_Animated;

	public GameObject StuffCan;

	public GameObject StuffAmmunition;

	public GameObject StuffJar;

	public GameObject StuffMoney;

	public GameObject CardboardBox_20x10x7_Animated;

	public GameObject CardboardBox_20x20x10_Animated;

	public GameObject CardboardBox_20x20x20_Animated;

	public GameObject CardboardBox_30x20x20_Animated;

	public GameObject CardboardBox_20x10x7_Fragmented;

	public GameObject CardboardBox_20x20x10_Fragmented;

	public GameObject CardboardBox_20x20x20_Fragmented;

	public GameObject CardboardBox_30x20x20_Fragmented;

	private bool instantiate;

	private bool instantiate1;

	private GameObject Obj_StuffCan;

	private GameObject Obj_StuffAmmunition;

	private GameObject Obj_StuffJar;

	private GameObject Obj_StuffMoney;

	private GameObject Obj_CardboardBox_20x10x7_Animated;

	private GameObject Obj_CardboardBox_20x20x10_Animated;

	private GameObject Obj_CardboardBox_20x20x20_Animated;

	private GameObject Obj_CardboardBox_30x20x20_Animated;

	private GameObject Obj_CardboardBox_20x10x7_Fragmented;

	private GameObject Obj_CardboardBox_20x20x10_Fragmented;

	private GameObject Obj_CardboardBox_20x20x20_Fragmented;

	private GameObject Obj_CardboardBox_30x20x20_Fragmented;

	private void Start()
	{
		Object.Destroy(ForDeleteCardboardBox_20x10x7_Animated);
		Object.Destroy(ForDeleteCardboardBox_20x20x10_Animated);
		Object.Destroy(ForDeleteCardboardBox_20x20x20_Animated);
		Object.Destroy(ForDeleteCardboardBox_30x20x20_Animated);
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (time >= 0f && time <= 0.1f)
		{
			StuffCan.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 0f);
			StuffAmmunition.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 0f);
			StuffJar.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 0f);
			StuffMoney.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 0f);
			if (!instantiate1)
			{
				Object.Destroy(Obj_CardboardBox_20x10x7_Fragmented);
				Object.Destroy(Obj_CardboardBox_20x20x10_Fragmented);
				Object.Destroy(Obj_CardboardBox_20x20x20_Fragmented);
				Object.Destroy(Obj_CardboardBox_30x20x20_Fragmented);
				Object.Destroy(Obj_StuffCan);
				Object.Destroy(Obj_StuffAmmunition);
				Object.Destroy(Obj_StuffJar);
				Object.Destroy(Obj_StuffMoney);
				Obj_CardboardBox_20x10x7_Animated = Object.Instantiate(CardboardBox_20x10x7_Animated);
				Obj_CardboardBox_20x20x10_Animated = Object.Instantiate(CardboardBox_20x20x10_Animated);
				Obj_CardboardBox_20x20x20_Animated = Object.Instantiate(CardboardBox_20x20x20_Animated);
				Obj_CardboardBox_30x20x20_Animated = Object.Instantiate(CardboardBox_30x20x20_Animated);
				Obj_StuffCan = Object.Instantiate(StuffCan);
				Obj_StuffAmmunition = Object.Instantiate(StuffAmmunition);
				Obj_StuffJar = Object.Instantiate(StuffJar);
				Obj_StuffMoney = Object.Instantiate(StuffMoney);
				instantiate1 = true;
			}
		}
		if (time >= 1f && time <= 1.1f)
		{
			Obj_CardboardBox_20x10x7_Animated.GetComponent<Animation>().Play("Open");
			Obj_CardboardBox_20x20x10_Animated.GetComponent<Animation>().Play("Open");
			Obj_CardboardBox_20x20x20_Animated.GetComponent<Animation>().Play("Open");
			Obj_CardboardBox_30x20x20_Animated.GetComponent<Animation>().Play("Open");
		}
		if (time >= 1.4f && time <= 1.5f)
		{
			Obj_StuffCan.GetComponent<Move>().MoveUp();
			Obj_StuffAmmunition.GetComponent<Move>().MoveUp();
			Obj_StuffJar.GetComponent<Move>().MoveUp();
			Obj_StuffMoney.GetComponent<Move>().MoveUp();
		}
		if (time >= 1.8f && time <= 1.9f)
		{
			Obj_StuffCan.GetComponent<Rotate>().SetRotate(rotate: true);
			Obj_StuffAmmunition.GetComponent<Rotate>().SetRotate(rotate: true);
			Obj_StuffJar.GetComponent<Rotate>().SetRotate(rotate: true);
			Obj_StuffMoney.GetComponent<Rotate>().SetRotate(rotate: true);
		}
		if (time >= 2.8f && time <= 2.9f)
		{
			Obj_StuffCan.GetComponent<Rotate>().SetRotate(rotate: false);
			Obj_StuffCan.GetComponent<Move>().MoveDown();
			Obj_StuffAmmunition.GetComponent<Rotate>().SetRotate(rotate: false);
			Obj_StuffAmmunition.GetComponent<Move>().MoveDown();
			Obj_StuffJar.GetComponent<Rotate>().SetRotate(rotate: false);
			Obj_StuffJar.GetComponent<Move>().MoveDown();
			Obj_StuffMoney.GetComponent<Rotate>().SetRotate(rotate: false);
			Obj_StuffMoney.GetComponent<Move>().MoveDown();
			Obj_CardboardBox_20x10x7_Animated.GetComponent<Animation>().Play("Close");
			Obj_CardboardBox_20x20x10_Animated.GetComponent<Animation>().Play("Close");
			Obj_CardboardBox_20x20x20_Animated.GetComponent<Animation>().Play("Close");
			Obj_CardboardBox_30x20x20_Animated.GetComponent<Animation>().Play("Close");
		}
		if (time >= 5f && time <= 5.1f && !instantiate)
		{
			Object.Destroy(Obj_CardboardBox_20x10x7_Animated);
			Object.Destroy(Obj_CardboardBox_20x20x10_Animated);
			Object.Destroy(Obj_CardboardBox_20x20x20_Animated);
			Object.Destroy(Obj_CardboardBox_30x20x20_Animated);
			Obj_CardboardBox_20x10x7_Fragmented = Object.Instantiate(CardboardBox_20x10x7_Fragmented);
			Obj_CardboardBox_20x20x10_Fragmented = Object.Instantiate(CardboardBox_20x20x10_Fragmented);
			Obj_CardboardBox_20x20x20_Fragmented = Object.Instantiate(CardboardBox_20x20x20_Fragmented);
			Obj_CardboardBox_30x20x20_Fragmented = Object.Instantiate(CardboardBox_30x20x20_Fragmented);
			instantiate = true;
		}
		if (time >= 8f && time <= 8.1f)
		{
			time = 0f;
			instantiate1 = false;
			instantiate = false;
		}
	}
}
