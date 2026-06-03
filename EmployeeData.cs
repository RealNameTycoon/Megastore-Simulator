using RandomNameGen;
using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeData", menuName = "ScriptableObjects/EmployeeData", order = 2)]
public class EmployeeData : ScriptableObject
{
	[SerializeField]
	public Sprite employeeSprite;

	[SerializeField]
	public EmployeeRole role;

	[SerializeField]
	public Sex sex;

	public int employeeID;

	public void FixNameAsSpriteName()
	{
	}

	public void RenameImageAsSpriteName()
	{
	}
}
