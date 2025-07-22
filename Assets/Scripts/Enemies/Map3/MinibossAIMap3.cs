using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Controls the AI logic for the Miniboss Golem using a Dynamic Rigidbody for physics-based movement.
/// This script requires an EnemyBehaviour4 component on the same GameObject to manage its health and death state.
/// </summary>
public class MinibossAIMap3 : MonoBehaviour, IStatefulEnemy
{
    [Header("References")]
    [Tooltip("The player character the miniboss will target.")]
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    [Tooltip("Reference to the component that manages this enemy's health and death.")]
    [SerializeField] private EnemyBehaviour4 enemyBehaviour;

    [Header("Stats")]
    [Tooltip("The horizontal movement speed.")]
    public float moveSpeed = 3f; // Dynamic bodies might need different speed values

    [Header("AI Behavior")]
    [Tooltip("The range at which the miniboss will start chasing the player.")]
    public float detectionRange = 10f;

    [Header("Platform Edge Detection")]
    [Tooltip("A child object placed at the enemy's feet to check for ledges.")]
    [SerializeField] private Transform edgeCheckPoint;
    [Tooltip("The LayerMask representing walkable ground.")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.5f;

    // --- (Attack, Combo, and Door Headers are unchanged) ---
    [Header("Unified Attack Zone")]
    public Transform attackPoint;
    public float attackRadius = 0.8f;
    public float normalAttackDamage = 15f;
    public float attackInterval = 2f;
    [Header("Special Attack Combo")]
    public int attacksUntilSpecial = 3;
    public float specialAttackDamage = 25f;
    public float attackComboResetTime = 5.0f;
    [Header("Door Control")]
    public GameObject associatedDoor;


    // --- Private State Variables ---
    [System.NonSerialized] private int normalAttackCounter = 0;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool isChasing = false;
    private Vector3 initialScale;
    private bool isSpecialAttack = false;
    private Coroutine resetAttackComboCoroutine;
    private int originalLayer;
    private int invulnerableLayer;

    /// <summary>
    /// Initializes components and sets starting values.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;

        enemyBehaviour = GetComponent<EnemyBehaviour4>();
        if (enemyBehaviour == null)
        {
            Debug.LogError("MinibossAIMap3 requires an EnemyBehaviour4 component to function.", this);
            this.enabled = false;
            return;
        }

        // The Rigidbody Type is now set to Dynamic in the Inspector.

        if (attackPoint != null && attackPoint.GetComponent<Collider2D>() != null)
            attackPoint.GetComponent<Collider2D>().enabled = false;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("MinibossAI: Player not found!", this); this.enabled = false; }
        }

        originalLayer = gameObject.layer;
        invulnerableLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (invulnerableLayer == -1) { invulnerableLayer = LayerMask.NameToLayer("Water"); }
    }

    /// <summary>
    /// The main AI logic loop, called every frame to decide between attacking, chasing, or standing still.
    /// </summary>
    void Update()
    {
        if (enemyBehaviour == null || !enemyBehaviour.enabled || isAttacking || player == null) return;

        FacePlayer();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange) { isChasing = true; }
        else if (distanceToPlayer > detectionRange * 1.5f) { isChasing = false; }

        if (isChasing)
        {
            float attackDistance = Vector2.Distance(attackPoint.position, player.position);
            if (attackDistance <= attackRadius && Time.time >= lastAttackTime + attackInterval)
            {
                StopMovement();
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
    /// Checks if there is ground ahead of the miniboss to prevent it from walking off ledges.
    /// </summary>
    private bool IsGroundAhead()
    {
        if (edgeCheckPoint == null) return true; // Failsafe if not assigned

        Debug.DrawRay(edgeCheckPoint.position, Vector2.down * groundCheckDistance, Color.cyan);
        return Physics2D.Raycast(edgeCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
    }

    /// <summary>
    /// Moves the Dynamic Rigidbody towards the player by setting its horizontal velocity.
    /// </summary>
    void MoveTowardsPlayer()
    {
        // Only move forward if ground is detected ahead.
        if (IsGroundAhead())
        {
            anim.SetBool("isWalking", true);
            float moveDirection = player.position.x > transform.position.x ? 1f : -1f;

            // Set the horizontal velocity, but leave the vertical velocity for gravity to control.
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // If there's no ground ahead, stop at the edge.
            StopMovement();
        }
    }

    /// <summary>
    /// Stops the enemy's horizontal movement by setting its x-velocity to zero.
    /// </summary>
    void StopMovement()
    {
        anim.SetBool("isWalking", false);
        // Retains vertical velocity (like falling) while stopping horizontal movement.
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    /// <summary>
    /// An Animation Event function that activates the attack hitbox for a short duration.
    /// </summary>
    public void DealDamageEvent()
    {
        StartCoroutine(DamageWindowRoutine());
    }

    /// <summary>
    /// A coroutine that enables the attack trigger collider for a brief "active" window during an attack.
    /// </summary>
    IEnumerator DamageWindowRoutine()
    {
        Collider2D attackCollider = attackPoint.GetComponent<Collider2D>();
        if (attackCollider != null) attackCollider.enabled = true;
        yield return new WaitForSeconds(0.2f);
        if (attackCollider != null) attackCollider.enabled = false;
    }

    /// <summary>
    /// Called when the AttackPoint's trigger collider hits another object. Deals damage to the player if appropriate.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isAttacking && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float damageToDeal = isSpecialAttack ? specialAttackDamage : normalAttackDamage;
                playerHealth.TakeDamage(damageToDeal);
            }
        }
    }

    /// <summary>
    /// Allows setting the attacking state from an external script (like AttackStateBehaviour).
    /// This also controls the layer switching to grant invincibility.
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;

        if (state == true)
        {
            gameObject.layer = invulnerableLayer;
            StopMovement(); // Ensure movement stops when an attack begins.
        }
        else
        {
            gameObject.layer = originalLayer;
        }
    }

    /// <summary>
    /// Determines whether to use a normal or special attack and resets combo timers.
    /// </summary>
    void AttackDecision()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("isAttacking");
        if (resetAttackComboCoroutine != null) StopCoroutine(resetAttackComboCoroutine);
        resetAttackComboCoroutine = StartCoroutine(AttackComboResetRoutine());
        if (normalAttackCounter >= attacksUntilSpecial - 1)
        {
            isSpecialAttack = true; normalAttackCounter = 0;
        }
        else
        {
            isSpecialAttack = false; normalAttackCounter++;
        }
    }

    /// <summary>
    /// A coroutine that resets the special attack combo after a period of inactivity.
    /// </summary>
    IEnumerator AttackComboResetRoutine()
    {
        yield return new WaitForSeconds(attackComboResetTime);
        normalAttackCounter = 0;
        resetAttackComboCoroutine = null;
    }

    /// <summary>
    /// Flips the enemy's sprite to face the player's horizontal position.
    /// </summary>
    void FacePlayer()
    {
        if (isAttacking) return;

        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
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
        // Draw gizmo for edge check sensor
        if (edgeCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(edgeCheckPoint.position, edgeCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }
}