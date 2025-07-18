using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class HeroKnight : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float wallSlideSpeed = 1.0f;
    [SerializeField] private float jumpForce = 7.5f;

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
    private PlayerState playerState;
    #endregion

    #region State Variables
    private float delayToIdle = 0.0f;

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
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        ProcessGroundCheck();
        ProcessMove();
        ProcessAnimations();
        ProcessFacingDirection();
    }
    #endregion

    #region Movement Processing
    private void ProcessMove()
    {
        if (!playerState.IsRolling)
        {
            body2d.linearVelocity = new Vector2(inputX * moveSpeed, body2d.linearVelocityY);
        }
    }

    private void ProcessGroundCheck()
    {
        // Check if character just landed on the ground
        if (!playerState.IsGrounded && groundSensor.State())
        {
            playerState.IsGrounded = true;
            animator.SetBool("Grounded", playerState.IsGrounded);
            playerState.CanDoubleJump = true; // Reset double jump when landing
        }

        // Check if character just started falling
        if (playerState.IsGrounded && !groundSensor.State())
        {
            playerState.IsGrounded = false;
            animator.SetBool("Grounded", playerState.IsGrounded);
        }
    }

    private void ProcessWallSlide()
    {
        playerState.IsWallSliding = wallSensorR1.State() && wallSensorR2.State();
        animator.SetBool("WallSlide", playerState.IsWallSliding);

        if (playerState.IsWallSliding && !playerState.IsGrounded && body2d.linearVelocityY < 0f)
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
            playerState.FacingDirection = 1;
        }
        else if (inputX < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Flip sprite
            playerState.FacingDirection = -1;
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
        if (context.performed && !playerState.IsRolling)
        {
            if (playerState.IsGrounded)
            {
                PerformJump();
            }
            else if (playerState.CanDoubleJump)
            {
                PerformDoubleJump();
            }
        }
    }

    private void PerformJump()
    {
        animator.SetTrigger("Jump");
        playerState.IsGrounded = false;
        animator.SetBool("Grounded", playerState.IsGrounded);
        body2d.linearVelocity = new Vector2(body2d.linearVelocityX, jumpForce);
        groundSensor.Disable(0.2f);
        playerState.CanDoubleJump = true;
    }

    private void PerformDoubleJump()
    {
        animator.SetTrigger("Roll");
        body2d.linearVelocity = new Vector2(body2d.linearVelocityX, jumpForce);
        playerState.CanDoubleJump = false;
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
        dust.transform.localScale = new Vector3(playerState.FacingDirection, 1, 1);
    }
    #endregion
}
