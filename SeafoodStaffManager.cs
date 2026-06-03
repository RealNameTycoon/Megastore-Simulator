using UnityEngine;

public class SeafoodStaffManager : SingletonBehaviour<SeafoodStaffManager>
{
	public ChoppingStandPlaceable GetAvailableFishStand(SeafoodStaff seafoodStaff)
	{
		float num = float.MaxValue;
		ChoppingStandPlaceable result = null;
		for (int i = 0; i < SingletonBehaviour<SpawnManager>.Instance.FishStandPlaceables.Count; i++)
		{
			ChoppingStandPlaceable choppingStandPlaceable = SingletonBehaviour<SpawnManager>.Instance.FishStandPlaceables[i];
			if (!choppingStandPlaceable.IsOccupiedByStaff() && !choppingStandPlaceable.PlayerSitting)
			{
				float num2 = Vector3.Distance(seafoodStaff.transform.position, choppingStandPlaceable.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = SingletonBehaviour<SpawnManager>.Instance.FishStandPlaceables[i];
				}
			}
		}
		return result;
	}

	public ChoppingStandPlaceable GetServeNeedingFishStand(SeafoodStaff seafoodStaff)
	{
		float num = float.MaxValue;
		ChoppingStandPlaceable result = null;
		for (int i = 0; i < SingletonBehaviour<SpawnManager>.Instance.FishStandPlaceables.Count; i++)
		{
			ChoppingStandPlaceable choppingStandPlaceable = SingletonBehaviour<SpawnManager>.Instance.FishStandPlaceables[i];
			if (!choppingStandPlaceable.IsOccupiedByStaff() && !choppingStandPlaceable.PlayerSitting && choppingStandPlaceable.HasCustomer())
			{
				float num2 = Vector3.Distance(seafoodStaff.transform.position, SingletonBehaviour<SpawnManager>.Instance.FishStandPlaceables[i].transform.position);
				if (num2 < num)
				{
					num = num2;
					result = SingletonBehaviour<SpawnManager>.Instance.FishStandPlaceables[i];
				}
			}
		}
		return result;
	}
}
