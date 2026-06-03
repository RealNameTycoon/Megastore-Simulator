using System;
using System.Collections.Generic;
using UnityEngine;

public class TrayStation : Furniture
{
	[SerializeField]
	private List<TrayShelf> trayShelf;

	[SerializeField]
	private List<TrayShelf> storageTrayShelf;

	public override void InitializeOldFurniture(int id, bool isPacked = false)
	{
		base.InitializeOldFurniture(id, isPacked);
		for (int i = 0; i < trayShelf.Count; i++)
		{
			trayShelf[i].InitializeOld();
		}
		for (int j = 0; j < storageTrayShelf.Count; j++)
		{
			storageTrayShelf[j].InitializeOld();
		}
	}

	public override void InitializeNewFurniture(int id)
	{
		base.InitializeNewFurniture(id);
		for (int i = 0; i < trayShelf.Count; i++)
		{
			trayShelf[i].InitializeNew();
		}
		for (int j = 0; j < storageTrayShelf.Count; j++)
		{
			storageTrayShelf[j].InitializeNew();
		}
	}

	public override List<(KeyCode, (string, Action))> GetExtraButtonActions()
	{
		if (SingletonBehaviour<BoxManager>.Instance.NoContainerPicked())
		{
			return new List<(KeyCode, (string, Action))> { (KeyCode.F, ("pack", delegate
			{
				Pack();
			})) };
		}
		return null;
	}

	public new void Pack()
	{
		if (CanPack())
		{
			for (int i = 0; i < trayShelf.Count; i++)
			{
				trayShelf[i].OnPacked();
			}
			for (int j = 0; j < storageTrayShelf.Count; j++)
			{
				storageTrayShelf[j].OnPacked();
			}
			SingletonBehaviour<SpawnManager>.Instance.PackFurniture(this);
		}
	}

	public override bool CanPack()
	{
		if (ReservedToStaff())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("tray_station_reserved_staff_error", base.transform);
			return false;
		}
		if (!AllTraysEmpty())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_oven_not_empty", base.transform);
			return false;
		}
		if (HasLabels())
		{
			SingletonBehaviour<TooltipUI>.Instance.ShowTimedError("error_pack_product_label", base.transform);
			return false;
		}
		return true;
	}

	private bool ReservedToStaff()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].IsReservedToStaff())
			{
				return true;
			}
		}
		return false;
	}

	private bool AllTraysEmpty()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (!trayShelf[i].IsTrayEmpty())
			{
				return false;
			}
		}
		return true;
	}

	private bool HasLabels()
	{
		for (int i = 0; i < trayShelf.Count; i++)
		{
			if (trayShelf[i].HasLabel())
			{
				return true;
			}
		}
		return false;
	}
}
