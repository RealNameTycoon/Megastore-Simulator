using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoxContentWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private RectTransform openedPosition;

	[SerializeField]
	private RectTransform closedPosition;

	[SerializeField]
	private List<BoxContentRow> boxContentRows;

	private List<ProductType> productTypes;

	private List<int> productCounts;

	private int selectedProductIndex;

	public void Repaint(List<ProductType> productTypes, List<int> productCounts, int selectedProductIndex)
	{
		this.productTypes = productTypes;
		this.productCounts = productCounts;
		this.selectedProductIndex = selectedProductIndex;
		for (int i = 0; i < productTypes.Count; i++)
		{
			if (i >= boxContentRows.Count)
			{
				boxContentRows.Add(Object.Instantiate(boxContentRows[0]));
				boxContentRows[i].transform.SetParent(boxContentRows[0].transform.parent);
				boxContentRows[i].transform.localScale = Vector3.one;
			}
			boxContentRows[i].Repaint(productTypes[i], productCounts[i], i == selectedProductIndex, productTypes.Count);
			if (!boxContentRows[i].gameObject.activeSelf)
			{
				boxContentRows[i].gameObject.SetActive(value: true);
			}
		}
		for (int j = productTypes.Count; j < boxContentRows.Count; j++)
		{
			if (boxContentRows[j].gameObject.activeSelf)
			{
				boxContentRows[j].gameObject.SetActive(value: false);
			}
		}
	}

	public void Open()
	{
		canvas.enabled = true;
		base.transform.DOKill();
		base.transform.DOMove(openedPosition.position, 0.2f).SetEase(Ease.OutSine);
		Repaint(productTypes, productCounts, selectedProductIndex);
	}

	public void Close()
	{
		base.transform.DOKill();
		base.transform.DOMove(closedPosition.position, 0.2f).SetEase(Ease.OutSine).OnComplete(delegate
		{
			canvas.enabled = false;
		});
	}

	public bool IsOpen()
	{
		return canvas.enabled;
	}
}
