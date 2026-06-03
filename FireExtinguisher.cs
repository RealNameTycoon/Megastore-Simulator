using System;
using System.Collections;
using System.Collections.Generic;
using DFTGames.Localization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireExtinguisher : Clickable
{
	[SerializeField]
	private Transform hangPosition;

	[SerializeField]
	private Transform extinguisherParent;

	[SerializeField]
	private MeshRenderer body;

	[SerializeField]
	private MeshRenderer pipeHanging;

	[SerializeField]
	private Transform pipeUsingStart;

	[SerializeField]
	private Transform pipeUsingTarget;

	[SerializeField]
	private Collider collider;

	[SerializeField]
	private ParticleSystem water;

	[SerializeField]
	private TextMeshProUGUI ownedText;

	[SerializeField]
	private Button purchaseButton;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private ShoppingWindow shoppingWindow;

	private const int EXTINGUISHER_PRICE = 20;

	private const string extinguisherCountKey = "extinguisherCountKey";

	private bool isPicked;

	private float PICKUP_DURATION = 0.5f;

	private bool isUsing;

	private int extinguisherCount = -1;

	public bool IsPicked => isPicked;

	private Transform ExtinguisherParent => extinguisherParent;

	public static FireExtinguisher Instance { get; protected set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		extinguisherCount = GenericDataSerializer.LoadInt("extinguisherCountKey", 5);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		LocalizeBase.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		ownedText.text = Locale.GetWord("owned").Replace("{0}", extinguisherCount.ToString());
	}

	public void PickUp()
	{
		isPicked = true;
		base.transform.SetParent(ExtinguisherParent);
		collider.enabled = false;
		base.transform.DOLocalMove(Vector3.zero, PICKUP_DURATION);
		base.transform.DOLocalRotate(Vector3.zero, PICKUP_DURATION).OnComplete(delegate
		{
			UpdateMenu();
			EventManager.NotifyEvent(GameEvents.EXTINGUISHER_PICKED);
		});
		pipeHanging.DOKill();
		pipeHanging.transform.DOLocalRotate(pipeUsingTarget.localEulerAngles, PICKUP_DURATION);
	}

	public void UpdateMenu()
	{
		if (IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)> { 
			{
				KeyCode.R,
				("finish", delegate
				{
					Release();
				})
			} });
		}
	}

	public void UpdateMenuWithExtinguish()
	{
		if (IsPicked)
		{
			SingletonBehaviour<ButtonsWindow>.Instance.RepaintWithKeyCodes(new Dictionary<KeyCode, (string, Action)>
			{
				{
					KeyCode.Mouse0,
					("use", delegate
					{
						Use();
					})
				},
				{
					KeyCode.R,
					("finish", delegate
					{
						Release();
					})
				}
			});
		}
	}

	private void Release()
	{
		EventManager.NotifyEvent(GameEvents.EXTINGUISHER_RELEASED);
		base.transform.SetParent(hangPosition);
		base.transform.localPosition = Vector3.zero;
		base.transform.localEulerAngles = Vector3.zero;
		collider.enabled = true;
		pipeHanging.DOKill();
		pipeHanging.transform.DOLocalRotate(pipeUsingStart.localEulerAngles, PICKUP_DURATION);
		isPicked = false;
		if (isUsing)
		{
			water.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			isUsing = false;
		}
		SingletonBehaviour<ButtonsWindow>.Instance.Close();
		SingletonBehaviour<RayShooter>.Instance.ImitateHover();
	}

	public void Use()
	{
		if (!isUsing)
		{
			extinguisherCount--;
			GenericDataSerializer.SaveInt("extinguisherCountKey", extinguisherCount);
			ownedText.text = Locale.GetWord("owned").Replace("{0}", extinguisherCount.ToString());
			isUsing = true;
			StartCoroutine(UseAndStop());
		}
	}

	private IEnumerator UseAndStop()
	{
		water.gameObject.SetActive(value: true);
		water.Play(withChildren: true);
		SingletonBehaviour<FireManager>.Instance.StopFireEmitting();
		yield return new WaitForSeconds(3f);
		HapticController.Vibrate(PresetType.LightImpact);
		StopUse();
		if (extinguisherCount == 0)
		{
			SingletonBehaviour<MiddleTooltipUI>.Instance.Open("extinguisher_empty");
			Release();
		}
	}

	private void StopUse()
	{
		water.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		water.gameObject.SetActive(value: false);
		SingletonBehaviour<FireManager>.Instance.StopFire();
		isUsing = false;
		UpdateMenu();
	}

	protected override string GetToolTip()
	{
		if (extinguisherCount == 0)
		{
			return "purchase_extinguisher";
		}
		return base.GetToolTip();
	}

	public override void OnMouseButtonDown()
	{
		base.OnMouseButtonDown();
		if (extinguisherCount == 0)
		{
			shoppingWindow.OnItemsClicked();
		}
		else
		{
			PickUp();
		}
	}
}
