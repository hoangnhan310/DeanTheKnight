using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the AI logic for the Snapper enemy using a Dynamic Rigidbody for physics-based movement.
/// This script requires an EnemyBehaviour4 component to handle health and death states.
/// </summary>
public class SnapperAI : MonoBehaviour, IStatefulEnemy
{
    [Header("References")]
    [Tooltip("The player target.")]
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    [Tooltip("The component that manages health and dying for this enemy.")]
    [SerializeField] private EnemyBehaviour4 enemyBehaviour;

    [Header("Stats")]
    [Tooltip("The horizontal movement speed.")]
    public float moveSpeed = 5f; // Dynamic bodies often need higher speed values

    [Header("AI Behavior")]
    [Tooltip("The range at which the Snapper starts moving towards the player.")]
    public float detectionRange = 12f;

    [Header("Attack")]
    [Tooltip("An empty child object representing the center of the attack hitbox.")]
    public Transform attackPoint;
    [Tooltip("The radius from the Attack Point to check for the player.")]
    public float attackRadius = 1.5f;
    [Tooltip("The amount of damage the attack deals.")]
    public float attackDamage = 15f;
    [Tooltip("The cooldown in seconds between attacks.")]
    public float attackInterval = 2.5f;

    [Header("Platform Edge Detection")]
    [Tooltip("A child object placed at the enemy's feet to check for ledges.")]
    [SerializeField] private Transform edgeCheckPoint;
    [Tooltip("The LayerMask representing walkable ground.")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.5f;

    // --- Private State Variables ---
    private float lastAttackTime = -99f;
    private bool isAttacking = false;

    /// <summary>
    /// Initializes components and sets initial state.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        enemyBehaviour = GetComponent<EnemyBehaviour4>();
        if (enemyBehaviour == null)
        {
            Debug.LogError("SnapperAI requires an EnemyBehaviour4 component.", this);
            this.enabled = false;
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("SnapperAI: Player not found!", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// Contains the main AI logic to decide actions based on player proximity.
    /// </summary>
    void Update()
    {
        if (enemyBehaviour == null || !enemyBehaviour.enabled || isAttacking || player == null)
        {
            // If unable to act, ensure horizontal velocity is zero.
            StopMovement();
            return;
        }

        FacePlayer();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            float attackDistance = Vector2.Distance(attackPoint.position, player.position);

            if (attackDistance <= attackRadius && Time.time >= lastAttackTime + attackInterval)
            {
                AttackDecision();
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            StopMovement();
        }
    }

    /// <summary>
    /// Checks if there is ground ahead to prevent falling off ledges.
    /// </summary>
    /// <returns>True if ground is detected in front, otherwise false.</returns>
    private bool IsGroundAhead()
    {
        if (edgeCheckPoint == null) return true; // If no sensor, assume it's safe to move

        // Draw a line in the scene view to visualize the raycast
        Debug.DrawRay(edgeCheckPoint.position, Vector2.down * groundCheckDistance, Color.cyan);

        return Physics2D.Raycast(edgeCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
    }

    /// <summary>
    /// Sets the horizontal velocity of the Rigidbody to move towards the player.
    /// </summary>
    void MoveTowardsPlayer()
    {
        if (Vector2.Distance(attackPoint.position, player.position) <= attackRadius)
        {
            StopMovement();
            return;
        }

        // Only move if there is ground ahead
        if (IsGroundAhead())
        {
            anim.SetBool("isWalking", true);
            float moveDirection = (player.position.x > transform.position.x) ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // No ground ahead, so stop at the edge.
            StopMovement();
        }
    }

    /// <summary>
    /// Stops horizontal movement by setting the x-velocity of the Rigidbody to zero.
    /// </summary>
    void StopMovement()
    {
        anim.SetBool("isWalking", false);
        // Retains vertical velocity (gravity) while stopping horizontal movement.
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    /// <summary>
    /// Sets the attacking flag, controlled by a StateMachineBehaviour.
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
        // Stop movement when the attack starts
        if (state)
        {
            StopMovement();
        }
    }

    /// <summary>
    /// Initiates the attack sequence.
    /// </summary>
    void AttackDecision()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("isAttacking");
    }

    /// <summary>
    /// Called by an Animation Event to deal damage to the player.
    /// </summary>
    public void DealDamageEvent()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerCollider.CompareTag("Player"))
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// Flips the enemy's sprite to face the player.
    /// </summary>
    void FacePlayer()
    {
        // Prevent flipping while attacking to lock direction
        if (isAttacking) return;

        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Draws visualization gizmos for ranges in the Unity Editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}