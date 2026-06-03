using UnityEngine;

public class NewCarController : MonoBehaviour
{
	[SerializeField]
	private int colWheels;

	[SerializeField]
	private CarSteerType m_CarSteerType;

	[SerializeField]
	private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;

	[SerializeField]
	private WheelCollider[] m_WheelColliders = new WheelCollider[4];

	[SerializeField]
	private GameObject[] m_WheelMeshes = new GameObject[4];

	[SerializeField]
	private Vector3 m_CentreOfMassOffset;

	[SerializeField]
	private float m_MaximumSteerAngle;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_SteerHelper;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_TractionControl;

	[SerializeField]
	private float m_FullTorqueOverAllWheels;

	[SerializeField]
	private float m_ReverseTorque;

	[SerializeField]
	private float m_MaxHandbrakeTorque;

	[SerializeField]
	private float m_Downforce = 100f;

	[SerializeField]
	private SpeedType m_SpeedType;

	[SerializeField]
	private float m_Topspeed = 200f;

	[SerializeField]
	private static int NoOfGears = 5;

	[SerializeField]
	private float m_RevRangeBoundary = 1f;

	[SerializeField]
	private float m_SlipLimit;

	[SerializeField]
	private float m_BrakeTorque;

	[SerializeField]
	private Transform steeringWheel;

	[SerializeField]
	private float m_BrakeFactor = 0.95f;

	[SerializeField]
	private Rigidbody m_Rigidbody;

	private Quaternion[] m_WheelMeshLocalRotations;

	private Vector3 m_Prevpos;

	private Vector3 m_Pos;

	private float m_SteerAngle;

	private int m_GearNum;

	private float m_GearFactor;

	private float m_OldRotation;

	private float m_CurrentTorque;

	private const float k_ReversingThreshold = 0.01f;

	private float steerAngularSpeed;

	public bool Skidding { get; private set; }

	public float BrakeInput { get; private set; }

	public float CurrentSteerAngle => m_SteerAngle;

	public float CurrentSpeed => m_Rigidbody.linearVelocity.magnitude * 2.2369363f;

	public Vector3 LinearVelocity => m_Rigidbody.linearVelocity;

	public float MaxSpeed => m_Topspeed;

	public float Revs { get; private set; }

	public float AccelInput { get; private set; }

	private void Start()
	{
		m_WheelMeshLocalRotations = new Quaternion[colWheels];
		for (int i = 0; i < colWheels; i++)
		{
			m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
		}
		m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;
		m_MaxHandbrakeTorque = float.MaxValue;
		m_CurrentTorque = m_FullTorqueOverAllWheels - m_TractionControl * m_FullTorqueOverAllWheels;
	}

	private void GearChanging()
	{
		float num = Mathf.Abs(CurrentSpeed / MaxSpeed);
		float num2 = 1f / (float)NoOfGears * (float)(m_GearNum + 1);
		float num3 = 1f / (float)NoOfGears * (float)m_GearNum;
		if (m_GearNum > 0 && num < num3)
		{
			m_GearNum--;
		}
		if (num > num2 && m_GearNum < NoOfGears - 1)
		{
			m_GearNum++;
		}
	}

	private static float CurveFactor(float factor)
	{
		return 1f - (1f - factor) * (1f - factor);
	}

	private static float ULerp(float from, float to, float value)
	{
		return (1f - value) * from + value * to;
	}

	private void CalculateGearFactor()
	{
		float num = 1f / (float)NoOfGears;
		float b = Mathf.InverseLerp(num * (float)m_GearNum, num * (float)(m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
		m_GearFactor = Mathf.Lerp(m_GearFactor, b, Time.deltaTime * 5f);
	}

	private void CalculateRevs()
	{
		CalculateGearFactor();
		float num = (float)m_GearNum / (float)NoOfGears;
		float num2 = ULerp(0f, m_RevRangeBoundary, CurveFactor(num));
		float to = ULerp(m_RevRangeBoundary, 1f, num);
		Revs = ULerp(num2, to, m_GearFactor);
	}

	public void Move(float steering, float accel, float footbrake, float handbrake)
	{
		for (int i = 0; i < colWheels; i++)
		{
			m_WheelColliders[i].GetWorldPose(out var _, out var quat);
			m_WheelMeshes[i].transform.rotation = quat;
		}
		steering = Mathf.Clamp(steering, -1f, 1f);
		AccelInput = (accel = Mathf.Clamp(accel, -1f, 1f));
		BrakeInput = (footbrake = -1f * Mathf.Clamp(footbrake, -1f, 0f));
		handbrake = Mathf.Clamp(handbrake, 0f, 1f);
		m_SteerAngle = steering * m_MaximumSteerAngle;
		switch (m_CarSteerType)
		{
		case CarSteerType.FrontSteerWheels:
			m_WheelColliders[0].steerAngle = m_SteerAngle;
			m_WheelColliders[1].steerAngle = m_SteerAngle;
			break;
		case CarSteerType.RearSteerWheels:
			m_WheelColliders[2].steerAngle = 0f - m_SteerAngle;
			m_WheelColliders[3].steerAngle = 0f - m_SteerAngle;
			break;
		case CarSteerType.FourSteerWheels:
			m_WheelColliders[0].steerAngle = m_SteerAngle;
			m_WheelColliders[1].steerAngle = m_SteerAngle;
			m_WheelColliders[2].steerAngle = m_SteerAngle;
			m_WheelColliders[3].steerAngle = m_SteerAngle;
			break;
		}
		steeringWheel.localEulerAngles = new Vector3(steeringWheel.localEulerAngles.x, m_SteerAngle * 9f, steeringWheel.localEulerAngles.z);
		SteerHelper();
		ApplyDrive(accel, footbrake);
		CapSpeed();
		if (handbrake > 0f)
		{
			float brakeTorque = handbrake * m_MaxHandbrakeTorque;
			m_WheelColliders[0].brakeTorque = brakeTorque;
			m_WheelColliders[1].brakeTorque = brakeTorque;
			m_WheelColliders[2].brakeTorque = brakeTorque;
			m_WheelColliders[3].brakeTorque = brakeTorque;
			m_Rigidbody.linearVelocity *= m_BrakeFactor;
		}
		else
		{
			m_WheelColliders[0].brakeTorque = 0f;
			m_WheelColliders[1].brakeTorque = 0f;
			m_WheelColliders[2].brakeTorque = 0f;
			m_WheelColliders[3].brakeTorque = 0f;
		}
		CalculateRevs();
		GearChanging();
		AddDownForce();
		TractionControl();
	}

	private void CapSpeed()
	{
		float magnitude = m_Rigidbody.linearVelocity.magnitude;
		switch (m_SpeedType)
		{
		case SpeedType.MPH:
			magnitude *= 2.2369363f;
			if (magnitude > m_Topspeed)
			{
				m_Rigidbody.linearVelocity = m_Topspeed / 2.2369363f * m_Rigidbody.linearVelocity.normalized;
			}
			break;
		case SpeedType.KPH:
			magnitude *= 3.6f;
			if (magnitude > m_Topspeed)
			{
				m_Rigidbody.linearVelocity = m_Topspeed / 3.6f * m_Rigidbody.linearVelocity.normalized;
			}
			break;
		}
	}

	private void ApplyDrive(float accel, float footbrake)
	{
		switch (m_CarDriveType)
		{
		case CarDriveType.FourWheelDrive:
		{
			float num = accel * (m_CurrentTorque / 4f);
			for (int i = 0; i < colWheels; i++)
			{
				m_WheelColliders[i].motorTorque = num;
			}
			break;
		}
		case CarDriveType.FrontWheelDrive:
		{
			float num = accel * (m_CurrentTorque / 2f);
			WheelCollider obj2 = m_WheelColliders[0];
			float motorTorque = (m_WheelColliders[1].motorTorque = num);
			obj2.motorTorque = motorTorque;
			break;
		}
		case CarDriveType.RearWheelDrive:
		{
			float num = accel * (m_CurrentTorque / 2f);
			WheelCollider obj = m_WheelColliders[2];
			float motorTorque = (m_WheelColliders[3].motorTorque = num);
			obj.motorTorque = motorTorque;
			break;
		}
		}
		for (int j = 0; j < colWheels; j++)
		{
			if (NeedBreak(accel))
			{
				m_WheelColliders[j].brakeTorque = m_MaxHandbrakeTorque;
			}
		}
		if (NeedBreak(accel) && m_Rigidbody.linearVelocity.magnitude > 0.5f)
		{
			m_Rigidbody.linearVelocity *= m_BrakeFactor;
		}
	}

	private bool NeedBreak(float accel)
	{
		if (accel > 0f && !GoingForward())
		{
			return true;
		}
		if (accel < 0f && GoingForward())
		{
			return true;
		}
		return false;
	}

	private void SteerHelper()
	{
		for (int i = 0; i < 4; i++)
		{
			m_WheelColliders[i].GetGroundHit(out var hit);
			if (hit.normal == Vector3.zero)
			{
				return;
			}
		}
		if (Mathf.Abs(m_OldRotation - base.transform.eulerAngles.y) < 10f)
		{
			Quaternion quaternion = Quaternion.AngleAxis((base.transform.eulerAngles.y - m_OldRotation) * m_SteerHelper, Vector3.up);
			m_Rigidbody.linearVelocity = quaternion * m_Rigidbody.linearVelocity;
		}
		m_OldRotation = base.transform.eulerAngles.y;
	}

	private void AddDownForce()
	{
		m_WheelColliders[0].attachedRigidbody.AddForce(-base.transform.up * m_Downforce * m_WheelColliders[0].attachedRigidbody.linearVelocity.magnitude);
	}

	private void TractionControl()
	{
		WheelHit hit;
		switch (m_CarDriveType)
		{
		case CarDriveType.FourWheelDrive:
		{
			for (int i = 0; i < 4; i++)
			{
				m_WheelColliders[i].GetGroundHit(out hit);
				AdjustTorque(hit.forwardSlip);
			}
			break;
		}
		case CarDriveType.RearWheelDrive:
			m_WheelColliders[2].GetGroundHit(out hit);
			AdjustTorque(hit.forwardSlip);
			m_WheelColliders[3].GetGroundHit(out hit);
			AdjustTorque(hit.forwardSlip);
			break;
		case CarDriveType.FrontWheelDrive:
			m_WheelColliders[0].GetGroundHit(out hit);
			AdjustTorque(hit.forwardSlip);
			m_WheelColliders[1].GetGroundHit(out hit);
			AdjustTorque(hit.forwardSlip);
			break;
		}
	}

	private void AdjustTorque(float forwardSlip)
	{
		if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0f)
		{
			m_CurrentTorque -= 10f * m_TractionControl;
			return;
		}
		m_CurrentTorque += 10f * m_TractionControl;
		if (m_CurrentTorque > m_FullTorqueOverAllWheels)
		{
			m_CurrentTorque = m_FullTorqueOverAllWheels;
		}
	}

	public bool GoingForward()
	{
		return Vector3.Angle(LinearVelocity, base.transform.forward) < 45f;
	}

	public void DisableMovement()
	{
		m_Rigidbody.linearVelocity = Vector3.zero;
		m_Rigidbody.angularVelocity = Vector3.zero;
		m_Rigidbody.interpolation = RigidbodyInterpolation.None;
		m_Rigidbody.isKinematic = true;
	}

	public void EnableMovement()
	{
		m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		m_Rigidbody.isKinematic = false;
	}
}
