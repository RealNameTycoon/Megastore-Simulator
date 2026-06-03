using UnityEngine;

public class EscalatorManager : SingletonBehaviour<EscalatorManager>
{
	[SerializeField]
	private Escalator upwardEscalator;

	[SerializeField]
	private Escalator downwardEscalator;

	public Escalator UpwardEscalator => upwardEscalator;

	public Escalator DownwardEscalator => downwardEscalator;
}
