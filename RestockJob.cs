using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RestockJob
{
	[SerializeField]
	private string shelfID;

	[SerializeField]
	private Restockable shelf;

	[SerializeField]
	private ProductType productType;

	[SerializeField]
	private int neededAmount;

	[SerializeField]
	private Employee assignedEmployee;

	[SerializeField]
	private List<Box> reservedBoxes = new List<Box>();

	[SerializeField]
	private Box reservedBox;

	[SerializeField]
	private TrayShelf reservedTrayShelf;

	private Tray reservedTray;

	public string ShelfID => shelfID;

	public Restockable Shelf => shelf;

	public ProductType ProductType => productType;

	public int NeededAmount => neededAmount;

	public bool HasProductLabel => shelf.HasProductLabel();

	public Tray ReservedTray => reservedTray;

	public RestockJob(string shelfID, Restockable shelf, ProductType productType, int neededAmount)
	{
		this.shelfID = shelfID;
		this.shelf = shelf;
		this.productType = productType;
		this.neededAmount = neededAmount;
	}

	public void UpdateNeededAmount(int newAmount)
	{
		neededAmount = Mathf.Max(0, newAmount);
	}

	public void UpdateProductType(ProductType newProductType)
	{
		productType = newProductType;
	}

	public void AssignToEmployee(Employee employee)
	{
		assignedEmployee = employee;
		if (shelf != null && !shelf.IsReservedToStaff())
		{
			shelf.SetReservedToStaff(value: true);
		}
	}

	public Employee GetAssignedEmployee()
	{
		return assignedEmployee;
	}

	public bool IsAssigned()
	{
		return assignedEmployee != null;
	}

	public bool IsPlayerReserved()
	{
		return shelf.IsPlayerReserved();
	}

	public void Unassign()
	{
		assignedEmployee = null;
		if (shelf != null && shelf.IsReservedToStaff())
		{
			shelf.ClearReservedToStaff();
		}
		if (reservedTrayShelf != null && reservedTrayShelf.IsReservedToStaff())
		{
			reservedTrayShelf.ClearReservedToStaff();
			ClearTrayShelfReservation();
		}
	}

	public void ReserveBox(Box box, int employeeID)
	{
		reservedBox = box;
		box.SetReservedForRestocking(employeeID);
	}

	public void AddBox(Box box, int employeeID)
	{
		reservedBoxes.Add(box);
		box.SetReservedForRestocking(employeeID);
	}

	public void ReserveBoxes(List<Box> boxes, int employeeID)
	{
		reservedBoxes = boxes;
		foreach (Box box in boxes)
		{
			box.SetReservedForRestocking(employeeID);
		}
	}

	public void ReserveTrayShelf(TrayShelf trayShelf, int employeeID)
	{
		reservedTrayShelf = trayShelf;
		reservedTrayShelf.SetReservedToStaff(value: true);
		reservedTray = trayShelf.ContainedTray;
	}

	public TrayShelf GetReservedTrayShelf()
	{
		return reservedTrayShelf;
	}

	public void ClearTrayShelfReservation()
	{
		reservedTrayShelf = null;
		reservedTray = null;
	}

	public Box GetReservedBox()
	{
		return reservedBox;
	}

	public List<Box> GetReservedBoxes()
	{
		return reservedBoxes;
	}

	public bool HasReservedBox()
	{
		return reservedBox != null;
	}

	public void ClearBoxReservation()
	{
		reservedBox = null;
	}

	public void ClearBoxesReservation()
	{
		foreach (Box reservedBox in reservedBoxes)
		{
			reservedBox.SetReservedForRestocking(-1);
		}
		reservedBoxes.Clear();
	}

	public bool IsReservedBoxAvailable()
	{
		if (reservedBox == null)
		{
			return false;
		}
		return SingletonBehaviour<BoxManager>.Instance.GetPickedBox() != reservedBox;
	}
}
