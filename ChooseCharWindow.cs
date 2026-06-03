using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCharWindow : SingletonBehaviour<ChooseCharWindow>
{
	[SerializeField]
	private List<Image> femaleSelectedImages;

	[SerializeField]
	private List<Image> maleSelectedImages;

	[SerializeField]
	private Button femaleButton;

	[SerializeField]
	private Button maleButton;

	[SerializeField]
	private GameObject femaleParent;

	[SerializeField]
	private GameObject maleParent;

	[SerializeField]
	private Animator femaleAnimator;

	[SerializeField]
	private Animator maleAnimator;

	[SerializeField]
	private Transform femaleBoxParent;

	[SerializeField]
	private Transform maleBoxParent;

	[SerializeField]
	private Transform femaleExtinguisherParent;

	[SerializeField]
	private Transform maleExtinguisherParent;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Button okButton;

	[SerializeField]
	private StartWindow startWindow;

	private string MALE_SELECTED_KEY = "MALE_SELECTED_KEY";

	private bool maleSelected;

	public Animator Animator
	{
		get
		{
			if (maleSelected)
			{
				return maleAnimator;
			}
			return femaleAnimator;
		}
	}

	public Transform CharParent
	{
		get
		{
			if (maleSelected)
			{
				return maleParent.transform;
			}
			return femaleParent.transform;
		}
	}

	public Transform BoxParent
	{
		get
		{
			if (maleSelected)
			{
				return maleBoxParent;
			}
			return femaleBoxParent;
		}
	}

	public Transform ExtinguisherParent
	{
		get
		{
			if (maleSelected)
			{
				return maleExtinguisherParent;
			}
			return femaleExtinguisherParent;
		}
	}

	private void SelectMale(bool state)
	{
		for (int i = 0; i < femaleSelectedImages.Count; i++)
		{
			femaleSelectedImages[i].enabled = !state;
		}
		for (int j = 0; j < maleSelectedImages.Count; j++)
		{
			maleSelectedImages[j].enabled = state;
		}
		maleSelected = state;
	}

	private void Start()
	{
		maleSelected = GenericDataSerializer.LoadBool(MALE_SELECTED_KEY);
		femaleButton.onClick.AddListener(delegate
		{
			SelectMale(state: false);
		});
		maleButton.onClick.AddListener(delegate
		{
			SelectMale(state: true);
		});
		okButton.onClick.AddListener(delegate
		{
			if (maleSelected)
			{
				EventLogger.LogEvent("c_william_selected");
				GenericDataSerializer.SaveBool(MALE_SELECTED_KEY, value: true);
			}
			else
			{
				EventLogger.LogEvent("c_emma_selected");
				GenericDataSerializer.SaveBool(MALE_SELECTED_KEY, value: false);
			}
			canvas.enabled = false;
		});
		SelectMale(state: false);
	}

	public void Initialized()
	{
		maleSelected = GenericDataSerializer.LoadBool(MALE_SELECTED_KEY);
		femaleButton.onClick.AddListener(delegate
		{
			SelectMale(state: false);
		});
		maleButton.onClick.AddListener(delegate
		{
			SelectMale(state: true);
		});
		SelectMale(state: false);
		if (!GenericDataSerializer.HasKey(MALE_SELECTED_KEY))
		{
			canvas.enabled = true;
		}
	}

	private void Update()
	{
	}
}
