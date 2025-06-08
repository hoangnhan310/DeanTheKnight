using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class HeroKnight : MonoBehaviour
{
	#region Serialized Fields
	[Header("Movement")]
	[SerializeField] private float moveSpeed = 4.0f;
	[SerializeField] private float wallSlideSpeed = 1.0f;
	[SerializeField] private float jumpForce = 7.5f;
	[SerializeField] private float rollForce = 6.0f;

	[Header("Visual Effects")]
	[SerializeField] private bool noBlood = false;
	[SerializeField] private GameObject slideDust;
	#endregion

	#region Component References
	private Animator animator;
	private Rigidbody2D body2d;
	private Sensor_HeroKnight groundSensor;
	private Sensor_HeroKnight wallSensorR1;
	private Sensor_HeroKnight wallSensorR2;
	#endregion

	#region State Variables
	private bool isWallSliding = false;
	private bool isGrounded = false;
	private bool isRolling = false;
	private bool canDoubleJump = false;

	private int facingDirection = 1;
	private int currentAttack = 0;

	private float timeSinceAttack = 0.0f;
	private float delayToIdle = 0.0f;
	private readonly float rollDuration = 8.0f / 14.0f;
	private float rollCurrentTime;
	private float inputX;
	#endregion

	#region Unity Lifecycle Methods
	private void Start()
	{
		animator = GetComponent<Animator>();
		body2d = GetComponent<Rigidbody2D>();
		groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
		wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
		wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
	}

	private void Update()
	{
		ProcessAttack();
		ProcessRoll();
		ProcessGroundCheck();
		ProcessMove();
		ProcessAnimations();
		ProcessFacingDirection();
	}
	#endregion

	#region Movement Processing
	private void ProcessMove()
	{
		if (!isRolling)
		{
			body2d.linearVelocity = new Vector2(inputX * moveSpeed, body2d.linearVelocityY);
		}
	}

	private void ProcessGroundCheck()
	{
		// Check if character just landed on the ground
		if (!isGrounded && groundSensor.State())
		{
			isGrounded = true;
			animator.SetBool("Grounded", isGrounded);
			canDoubleJump = true; // Reset double jump when landing
		}

		// Check if character just started falling
		if (isGrounded && !groundSensor.State())
		{
			isGrounded = false;
			animator.SetBool("Grounded", isGrounded);
		}
	}

	private void ProcessWallSlide()
	{
		isWallSliding = wallSensorR1.State() && wallSensorR2.State();
		animator.SetBool("WallSlide", isWallSliding);

		if (isWallSliding && !isGrounded && body2d.linearVelocityY < 0f)
		{
			body2d.linearVelocityY = Mathf.Max(body2d.linearVelocityY, -wallSlideSpeed);
		}
	}

	private void ProcessFacingDirection()
	{
		// Swap direction of sprite depending on walk direction
		if (inputX > 0)
		{
			transform.rotation = Quaternion.identity; // Reset rotation
			facingDirection = 1;
		}
		else if (inputX < 0)
		{
			transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Flip sprite
			facingDirection = -1;
		}
	}
	#endregion

	#region Combat Processing
	private void ProcessAttack()
	{
		// Increase timer that controls attack combo
		timeSinceAttack += Time.deltaTime;
	}

	private void ProcessRoll()
	{
		// Increase timer that checks roll duration
		if (isRolling)
		{
			rollCurrentTime += Time.deltaTime;
		}

		// Disable rolling if timer extends duration
		if (rollCurrentTime > rollDuration)
		{
			isRolling = false;
			rollCurrentTime = 0.0f;
		}
	}
	#endregion

	#region Animation Processing
	private void ProcessAnimations()
	{
		// Set AirSpeed in animator
		animator.SetFloat("AirSpeedY", body2d.linearVelocityY);

		// Process wall sliding
		ProcessWallSlide();

		// Run animation
		if (Mathf.Abs(inputX) > Mathf.Epsilon)
		{
			// Reset timer
			delayToIdle = 0.05f;
			animator.SetInteger("AnimState", 1);
		}
		// Idle animation
		else
		{
			// Prevents flickering transitions to idle
			delayToIdle -= Time.deltaTime;
			if (delayToIdle < 0)
				animator.SetInteger("AnimState", 0);
		}
	}
	#endregion

	#region Input Actions
	public void OnMove(InputAction.CallbackContext context)
	{
		inputX = context.ReadValue<Vector2>().x;
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.performed && !isRolling)
		{
			if (isGrounded)
			{
				PerformJump();
			}
			else if (canDoubleJump)
			{
				PerformDoubleJump();
			}
		}
	}

	private void PerformJump()
	{
		animator.SetTrigger("Jump");
		isGrounded = false;
		animator.SetBool("Grounded", isGrounded);
		body2d.linearVelocity = new Vector2(body2d.linearVelocityX, jumpForce);
		groundSensor.Disable(0.2f);
		canDoubleJump = true;
	}

	private void PerformDoubleJump()
	{
		animator.SetTrigger("Roll");
		body2d.linearVelocity = new Vector2(body2d.linearVelocityX, jumpForce);
		canDoubleJump = false;
	}

	public void OnAttack(InputAction.CallbackContext context)
	{
		if (context.performed && timeSinceAttack > 0.25f && !isRolling)
		{
			currentAttack++;

			// Loop back to one after third attack
			if (currentAttack > 3)
				currentAttack = 1;

			// Reset Attack combo if time since last attack is too large
			if (timeSinceAttack > 1.0f)
				currentAttack = 1;

			// Call one of three attack animations "Attack1", "Attack2", "Attack3"
			animator.SetTrigger($"Attack{currentAttack}");

			// Reset timer
			timeSinceAttack = 0.0f;
		}
	}

	public void OnBlock(InputAction.CallbackContext context)
	{
		if (context.performed && !isRolling)
		{
			animator.SetTrigger("Block");
			animator.SetBool("IdleBlock", true);
		}
		else if (context.canceled)
		{
			animator.SetBool("IdleBlock", false);
		}
	}

	public void OnRoll(InputAction.CallbackContext context)
	{
		if (context.performed && !isRolling && !isWallSliding)
		{
			isRolling = true;
			animator.SetTrigger("Roll");
			body2d.linearVelocity = new Vector2(facingDirection * rollForce, body2d.linearVelocityY);
		}
	}
	#endregion

	#region Animation Events
	// Called in slide animation
	private void AE_SlideDust()
	{
		if (slideDust == null)
			return;

		Vector3 spawnPosition = wallSensorR2.transform.position;

		// Set correct dust spawn position
		GameObject dust = Instantiate(slideDust, spawnPosition, Quaternion.identity);
		// Turn dust in correct direction
		dust.transform.localScale = new Vector3(facingDirection, 1, 1);
	}
	#endregion
}
