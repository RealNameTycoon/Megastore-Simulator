using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class RestockerManager : SingletonBehaviour<RestockerManager>
{
	[SerializeField]
	private SerializedDictionary<string, RestockJob> labeledJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> unlabeledJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> allJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> allBakeryJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> labeledBakeryJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> unlabeledBakeryJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> allUnloaderJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> labeledUnloaderJobs = new SerializedDictionary<string, RestockJob>();

	[SerializeField]
	private SerializedDictionary<string, RestockJob> unlabeledUnloaderJobs = new SerializedDictionary<string, RestockJob>();

	private const float LOW_STOCK_THRESHOLD_BAKERY = 0.75f;

	private const float LOW_STOCK_THRESHOLD_UNLOADER = 0.99f;

	private const float LOW_STOCK_THRESHOLD = 0.98f;

	private const float HIGH_STOCK_THRESHOLD = 0.99f;

	private bool isInitialized;

	private int restockThreshold = 75;

	private string restockThresholdKey = "RESTOCK_THRESHOLD";

	public int RestockThreshold => restockThreshold;

	public void SetRestockThreshold(int threshold)
	{
		restockThreshold = threshold;
		GenericDataSerializer.SaveInt(restockThresholdKey, threshold);
	}

	protected override void Awake()
	{
		if (GameManager.isDemo)
		{
			base.gameObject.SetActive(value: false);
		}
		base.Awake();
		Initialize();
	}

	private void Initialize()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			restockThreshold = GenericDataSerializer.LoadInt(restockThresholdKey, 75);
			EventManager.AddListener<Shelf, ProductType>(PlaceableEvents.PRODUCT_ADDED, OnProductAddedToShelf);
			EventManager.AddListener<Shelf, ProductType>(PlaceableEvents.PRODUCT_REMOVED, OnProductRemovedFromShelf);
			EventManager.AddListener<TrayShelf, ProductType>(PlaceableEvents.TRAY_PRODUCT_ADDED, OnProductAddedToShelf);
			EventManager.AddListener<TrayShelf, ProductType>(PlaceableEvents.TRAY_PRODUCT_REMOVED, OnProductRemovedFromShelf);
			EventManager.AddListener<TrayShelf>(PlaceableEvents.TRAY_ADDED, OnTrayAdded);
			EventManager.AddListener<TrayShelf>(PlaceableEvents.TRAY_REMOVED, OnTrayRemoved);
			EventManager.AddListener<Shelf>(PlaceableEvents.PRICE_TAG_REMOVED, OnPriceTagRemoved);
			EventManager.AddListener<TrayShelf>(PlaceableEvents.TRACK_LABEL_REMOVED, UpdateJobForShelf);
			EventManager.AddListener<Restockable>(PlaceableEvents.BOX_ADDED, OnBoxAdded);
			EventManager.AddListener<Restockable>(PlaceableEvents.BOX_REMOVED, OnBoxRemoved);
			EventManager.AddListener<Restockable>(PlaceableEvents.PALLET_SHELF_LABEL_REMOVED, OnPalletLabelRemoved);
			EventManager.AddListener<Restockable>(PlaceableEvents.PALLET_SHELF_PALLET_ADDED, OnPalletShelfPalletAdded);
			EventManager.AddListener<Restockable>(PlaceableEvents.PALLET_SHELF_PALLET_REMOVED, OnPalletShelfPalletRemoved);
			EventManager.AddListener<Restockable>(PlaceableEvents.RESTOCKABLE_PACKED, OnRestockablePacked);
		}
	}

	private void OnRestockablePacked(Restockable restockable)
	{
		DisposeJob(restockable.RestocableID());
	}

	private void OnPalletShelfPalletAdded(Restockable palletShelf)
	{
		UpdateJobForShelf(palletShelf);
	}

	private void OnPalletShelfPalletRemoved(Restockable palletShelf)
	{
		UpdateJobForShelf(palletShelf);
	}

	private void OnPalletLabelRemoved(Restockable shelf)
	{
		UpdateJobForShelf(shelf);
	}

	private void OnBoxAdded(Restockable shelf)
	{
		UpdateJobForShelf(shelf);
	}

	private void OnBoxRemoved(Restockable shelf)
	{
		UpdateJobForShelf(shelf);
	}

	private void OnTrayAdded(TrayShelf trayShelf)
	{
		UpdateJobForShelf(trayShelf);
	}

	private void OnTrayRemoved(TrayShelf trayShelf)
	{
		UpdateJobForShelf(trayShelf);
	}

	private void OnPriceTagRemoved(Shelf shelf)
	{
		UpdateJobForShelf(shelf);
	}

	public void RegisterJob(Restockable shelf, ProductType productType, int neededAmount)
	{
		if (shelf.IsCookedShelf())
		{
			return;
		}
		Dictionary<string, RestockJob> dictionary;
		Dictionary<string, RestockJob> dictionary2;
		Dictionary<string, RestockJob> dictionary3;
		if (shelf.IsBakeryPlaceable())
		{
			dictionary = allBakeryJobs;
			dictionary2 = labeledBakeryJobs;
			dictionary3 = unlabeledBakeryJobs;
		}
		else if (shelf.IsUnloaderPlaceable())
		{
			dictionary = allUnloaderJobs;
			dictionary2 = labeledUnloaderJobs;
			dictionary3 = unlabeledUnloaderJobs;
		}
		else
		{
			dictionary = allJobs;
			dictionary2 = labeledJobs;
			dictionary3 = unlabeledJobs;
		}
		string text = shelf.RestocableID();
		if (dictionary.ContainsKey(text))
		{
			if (shelf.RestocableID().Contains("OVEN") || productType == ProductType.NONE || dictionary[text].ProductType == productType)
			{
				dictionary[text].UpdateNeededAmount(neededAmount);
				return;
			}
			dictionary.Remove(text);
			dictionary2.Remove(text);
			dictionary3.Remove(text);
		}
		RestockJob restockJob = new RestockJob(text, shelf, productType, neededAmount);
		dictionary.Add(text, restockJob);
		if (restockJob.HasProductLabel)
		{
			dictionary2.Add(text, restockJob);
		}
		else
		{
			dictionary3.Add(text, restockJob);
		}
		EventManager.NotifyEvent(RestockEvents.JOB_REGISTERED);
	}

	public void DisposeJob(string shelfID)
	{
		if (allJobs.ContainsKey(shelfID))
		{
			RestockJob restockJob = allJobs[shelfID];
			if (restockJob.IsAssigned())
			{
				restockJob.GetAssignedEmployee().OnJobDisposed(restockJob);
			}
			allJobs.Remove(shelfID);
			labeledJobs.Remove(shelfID);
			unlabeledJobs.Remove(shelfID);
		}
		else if (allBakeryJobs.ContainsKey(shelfID))
		{
			_ = allBakeryJobs[shelfID];
			allBakeryJobs.Remove(shelfID);
			labeledBakeryJobs.Remove(shelfID);
			unlabeledBakeryJobs.Remove(shelfID);
		}
		else if (allUnloaderJobs.ContainsKey(shelfID))
		{
			RestockJob restockJob2 = allUnloaderJobs[shelfID];
			if (restockJob2.IsAssigned())
			{
				restockJob2.GetAssignedEmployee().OnJobDisposed(restockJob2);
			}
			allUnloaderJobs.Remove(shelfID);
			labeledUnloaderJobs.Remove(shelfID);
			unlabeledUnloaderJobs.Remove(shelfID);
		}
	}

	public RestockJob RequestJobByType(Employee restocker, ProductType productType, bool labeled = true)
	{
		if (productType == ProductType.NONE)
		{
			return null;
		}
		RestockJob restockJob = null;
		if (labeled && labeledJobs.Count > 0)
		{
			new List<RestockJob>();
			float num = float.PositiveInfinity;
			foreach (KeyValuePair<string, RestockJob> labeledJob in labeledJobs)
			{
				if (!labeledJob.Value.IsAssigned() && !labeledJob.Value.IsPlayerReserved() && productType == labeledJob.Value.ProductType)
				{
					float num2 = (float)labeledJob.Value.Shelf.GetProductCount() / (float)labeledJob.Value.Shelf.GetCapacityForProduct(SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(labeledJob.Value.ProductType));
					if (!(num2 > (float)restockThreshold / 100f) && num2 < num)
					{
						restockJob = labeledJob.Value;
						num = num2;
					}
				}
			}
		}
		restockJob?.AssignToEmployee(restocker);
		return restockJob;
	}

	public (RestockJob, Restocker.IdleReason, HashSet<ProductType>) RequestJobByProductGroup(Employee restocker, ProductGroup productGroup, bool labeled = true, bool includeWarehouse = false)
	{
		RestockJob restockJob = null;
		Restocker.IdleReason item = Restocker.IdleReason.NO_BOXES;
		HashSet<ProductType> hashSet = new HashSet<ProductType>();
		bool flag = true;
		if (labeled && labeledJobs.Count > 0)
		{
			List<RestockJob> list = new List<RestockJob>();
			float num = float.PositiveInfinity;
			foreach (KeyValuePair<string, RestockJob> labeledJob in labeledJobs)
			{
				if (labeledJob.Value.IsAssigned() || labeledJob.Value.IsPlayerReserved() || (productGroup != ProductGroup.NONE && productGroup != labeledJob.Value.Shelf.GetProductGroup()))
				{
					continue;
				}
				if (labeledJob.Value.ProductType != labeledJob.Value.Shelf.ContainedProductType() && labeledJob.Value.ProductType != labeledJob.Value.Shelf.PreviousContainedProductType())
				{
					RestockJob validatedJob = GetValidatedJob(labeledJob.Value);
					list.Add(validatedJob);
				}
				else if (SingletonBehaviour<RestockZoneManager>.Instance.HasBoxWithProductType(labeledJob.Value.ProductType, includeWarehouse))
				{
					float num2 = (float)labeledJob.Value.Shelf.GetProductCount() / (float)labeledJob.Value.Shelf.GetCapacityForProduct(SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(labeledJob.Value.ProductType));
					if (!(num2 > (float)restockThreshold / 100f) && num2 < num)
					{
						num = num2;
						restockJob = labeledJob.Value;
						flag = false;
					}
				}
				else if (!((float)labeledJob.Value.Shelf.GetProductCount() / (float)labeledJob.Value.Shelf.GetCapacityForProduct(SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(labeledJob.Value.ProductType)) > (float)restockThreshold / 100f) && !hashSet.Contains(labeledJob.Value.ProductType))
				{
					hashSet.Add(labeledJob.Value.ProductType);
				}
			}
			foreach (RestockJob item2 in list)
			{
				labeledJobs.Remove(item2.ShelfID);
				allJobs.Remove(item2.ShelfID);
				if (item2.ProductType != ProductType.NONE)
				{
					labeledJobs.Add(item2.ShelfID, item2);
					allJobs.Add(item2.ShelfID, item2);
				}
			}
			if (flag && hashSet.Count == 0)
			{
				item = Restocker.IdleReason.NO_JOBS;
			}
		}
		else
		{
			item = Restocker.IdleReason.NO_JOBS;
		}
		restockJob?.AssignToEmployee(restocker);
		return (restockJob, item, hashSet);
	}

	public RestockJob RequestBakerJobByType(Baker baker, ProductType productType, bool labeled = true)
	{
		if (productType == ProductType.NONE)
		{
			return null;
		}
		RestockJob restockJob = labeledBakeryJobs.Values.FirstOrDefault((RestockJob j) => !j.IsAssigned() && j.ProductType == productType && !j.IsPlayerReserved());
		if (restockJob == null)
		{
			return null;
		}
		restockJob.AssignToEmployee(baker);
		return restockJob;
	}

	private RestockJob GetValidatedJob(RestockJob job)
	{
		ProductType productType = ProductType.NONE;
		productType = ((job.Shelf.ContainedProductType() != ProductType.NONE) ? job.Shelf.ContainedProductType() : job.Shelf.PreviousContainedProductType());
		if (productType == ProductType.NONE)
		{
			return new RestockJob(job.ShelfID, job.Shelf, ProductType.NONE, 0);
		}
		ProductData anyProductData = SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(productType);
		int productCount = job.Shelf.GetProductCount();
		int neededAmount = ((anyProductData != null) ? job.Shelf.GetCapacityForProduct(anyProductData) : 0) - productCount;
		return new RestockJob(job.ShelfID, job.Shelf, productType, neededAmount);
	}

	public (RestockJob, Restocker.IdleReason) RequestBakerJob(Baker baker, bool labeled = true)
	{
		RestockJob restockJob = null;
		Restocker.IdleReason item = Restocker.IdleReason.NO_JOBS;
		if (labeled && labeledBakeryJobs.Count > 0)
		{
			foreach (KeyValuePair<string, RestockJob> labeledBakeryJob in labeledBakeryJobs)
			{
				if (!labeledBakeryJob.Value.IsAssigned() && !labeledBakeryJob.Value.IsPlayerReserved())
				{
					if (SingletonBehaviour<BakerManager>.Instance.HasTrayShelfToReserve(labeledBakeryJob.Value.ProductType))
					{
						restockJob = labeledBakeryJob.Value;
						break;
					}
					item = Restocker.IdleReason.NO_TRAYS;
				}
			}
		}
		restockJob?.AssignToEmployee(baker);
		return (restockJob, item);
	}

	public RestockJob RequestUnloaderJob(Employee unloader, bool labeled = true)
	{
		RestockJob restockJob = null;
		if (labeled && labeledUnloaderJobs.Count > 0)
		{
			foreach (KeyValuePair<string, RestockJob> labeledUnloaderJob in labeledUnloaderJobs)
			{
				if (!labeledUnloaderJob.Value.IsAssigned() && !labeledUnloaderJob.Value.IsPlayerReserved() && SingletonBehaviour<RestockZoneManager>.Instance.HasBoxWithProductTypeOnTruck(labeledUnloaderJob.Value.ProductType))
				{
					restockJob = labeledUnloaderJob.Value;
					break;
				}
			}
		}
		restockJob?.AssignToEmployee(unloader);
		return restockJob;
	}

	public RestockJob RequestCookedBakerJob(Baker baker, bool labeled = true)
	{
		RestockJob restockJob = null;
		if (labeled && labeledBakeryJobs.Count > 0)
		{
			foreach (KeyValuePair<string, RestockJob> labeledBakeryJob in labeledBakeryJobs)
			{
				if (!labeledBakeryJob.Value.IsAssigned() && !labeledBakeryJob.Value.IsPlayerReserved() && SingletonBehaviour<BakerManager>.Instance.HasCookedTrayShelfToReserve(labeledBakeryJob.Value.ProductType))
				{
					restockJob = labeledBakeryJob.Value;
					break;
				}
			}
		}
		restockJob?.AssignToEmployee(baker);
		return restockJob;
	}

	public bool HasJob(Restockable shelf)
	{
		string key = shelf.RestocableID();
		if (shelf.IsBakeryPlaceable())
		{
			return allBakeryJobs.ContainsKey(key);
		}
		if (shelf.IsUnloaderPlaceable())
		{
			return allUnloaderJobs.ContainsKey(key);
		}
		return allJobs.ContainsKey(key);
	}

	public void UnassignJob(RestockJob job)
	{
		if (job != null && job.IsAssigned())
		{
			job.Unassign();
		}
	}

	private void OnProductAddedToShelf(Restockable shelf, ProductType productType)
	{
		UpdateJobForShelf(shelf);
	}

	private void OnProductRemovedFromShelf(Restockable shelf, ProductType productType)
	{
		UpdateJobForShelf(shelf);
	}

	private void UpdateJobForShelf(Restockable shelf)
	{
		if (shelf == null)
		{
			return;
		}
		if (shelf.IsCookedShelf())
		{
			DisposeJob(shelf.RestocableID());
			return;
		}
		Dictionary<string, RestockJob> dictionary;
		Dictionary<string, RestockJob> dictionary2;
		Dictionary<string, RestockJob> dictionary3;
		if (shelf.IsBakeryPlaceable())
		{
			dictionary = allBakeryJobs;
			dictionary2 = labeledBakeryJobs;
			dictionary3 = unlabeledBakeryJobs;
		}
		else if (shelf.IsUnloaderPlaceable())
		{
			dictionary = allUnloaderJobs;
			dictionary2 = labeledUnloaderJobs;
			dictionary3 = unlabeledUnloaderJobs;
		}
		else
		{
			dictionary = allJobs;
			dictionary2 = labeledJobs;
			dictionary3 = unlabeledJobs;
		}
		if (!shelf.IsAvailableForRestocking())
		{
			DisposeJob(shelf.RestocableID());
			return;
		}
		ProductData productData = ((shelf.ContainedProductType() != ProductType.NONE) ? SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(shelf.ContainedProductType()) : null);
		string key = shelf.RestocableID();
		int productCount = shelf.GetProductCount();
		int num = ((productData != null) ? shelf.GetCapacityForProduct(productData) : 0);
		if (dictionary.ContainsKey(key))
		{
			RestockJob restockJob = dictionary[key];
			bool flag = dictionary2.ContainsKey(key);
			if (flag && !restockJob.HasProductLabel)
			{
				dictionary2.Remove(key);
				restockJob.UpdateProductType(ProductType.NONE);
				dictionary3.Add(key, restockJob);
			}
			else if (!flag && restockJob.HasProductLabel)
			{
				dictionary3.Remove(key);
				restockJob.UpdateProductType(shelf.ContainedProductType());
				dictionary2.Add(key, restockJob);
			}
		}
		bool flag2 = shelf.ContainedProductType() == ProductType.NONE;
		if (flag2 && shelf.HasProductLabel())
		{
			int capacityForProduct = shelf.GetCapacityForProduct(SingletonBehaviour<ProductPool>.Instance.GetAnyProductData(shelf.PreviousContainedProductType()));
			RegisterJob(shelf, shelf.PreviousContainedProductType(), capacityForProduct);
			return;
		}
		if (flag2 && !shelf.HasProductLabel())
		{
			RegisterJob(shelf, ProductType.NONE, 1);
			return;
		}
		float num2 = ((num > 0) ? ((float)productCount / (float)num) : 0f);
		bool flag3 = false;
		flag3 = (shelf.IsBakeryPlaceable() ? (num2 < 0.75f) : ((!shelf.IsUnloaderPlaceable()) ? (num2 < 0.98f) : (num2 < 0.99f)));
		bool flag4 = Mathf.Approximately(num2, 1f);
		if (flag3)
		{
			int neededAmount = num - productCount;
			RegisterJob(shelf, shelf.ContainedProductType(), neededAmount);
		}
		else if (flag4 && HasJob(shelf))
		{
			DisposeJob(shelf.RestocableID());
		}
	}

	private string GenerateShelfID(Shelf shelf)
	{
		return $"{shelf.ParentPlaceableType}{shelf.ParentPlaceable.PlaceableID}|{shelf.ShelfID}";
	}

	public void CheckAndRegisterShelf(Restockable shelf)
	{
		if (shelf != null)
		{
			UpdateJobForShelf(shelf);
		}
	}

	public void AvailablitiyChanged(Restockable shelf)
	{
		if (shelf != null)
		{
			UpdateJobForShelf(shelf);
			EventManager.NotifyEvent(BakerEvents.TRAY_AVAILABLITY_CHANGED);
		}
	}
}
