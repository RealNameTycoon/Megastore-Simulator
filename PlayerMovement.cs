using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	public float moveSpeed;

	public float groundDrag;

	public float jumpForce;

	public float jumpCooldown;

	public float airMultiplier;

	private bool readyToJump;

	[HideInInspector]
	public float walkSpeed;

	[HideInInspector]
	public float sprintSpeed;

	[Header("Keybinds")]
	public KeyCode jumpKey = KeyCode.Space;

	[Header("Ground Check")]
	public float playerHeight;

	public LayerMask whatIsGround;

	private bool grounded;

	public Transform orientation;

	private float horizontalInput;

	private float verticalInput;

	private Vector3 moveDirection;

	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		readyToJump = true;
	}

	private void Update()
	{
		grounded = Physics.Raycast(base.transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
		MyInput();
		SpeedControl();
		if (grounded)
		{
			rb.linearDamping = groundDrag;
		}
		else
		{
			rb.linearDamping = 0f;
		}
	}

	private void FixedUpdate()
	{
		MovePlayer();
	}

	private void MyInput()
	{
		if (SingletonBehaviour<InputManager>.Instance.WasActionTriggeredThisFrame(SingletonBehaviour<InputManager>.Instance.JumpActionRef) && readyToJump && grounded)
		{
			readyToJump = false;
			Jump();
			Invoke("ResetJump", jumpCooldown);
		}
	}

	private void MovePlayer()
	{
		moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
		if (grounded)
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
		}
		else if (!grounded)
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
		}
	}

	private void SpeedControl()
	{
		Vector3 vector = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
		if (vector.magnitude > moveSpeed)
		{
			Vector3 vector2 = vector.normalized * moveSpeed;
			rb.linearVelocity = new Vector3(vector2.x, rb.linearVelocity.y, vector2.z);
		}
	}

	private void Jump()
	{
		rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
		rb.AddForce(base.transform.up * jumpForce, ForceMode.Impulse);
	}

	private void ResetJump()
	{
		readyToJump = true;
	}
}
