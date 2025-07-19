using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the AI logic for the Snapper enemy. It chases the player on the ground and performs a bite attack.
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
    public float moveSpeed = 2.5f;

    [Header("AI Behavior")]
    [Tooltip("The range at which the Snapper starts moving towards the player.")]
    public float detectionRange = 12f;

    [Header("Attack")]
    [Tooltip("An empty child object representing the center of the attack hitbox.")]
    public Transform attackPoint;
    [Tooltip("The radius from the Attack Point to check for the player.")]
    public float attackRadius = 1.0f;
    [Tooltip("The amount of damage the attack deals.")]
    public float attackDamage = 15f;
    [Tooltip("The cooldown in seconds between attacks.")]
    public float attackInterval = 2.5f;

    // --- Private State Variables ---
    private float lastAttackTime = -99f;
    private bool isAttacking = false;
    // 'isDead' is now managed by the EnemyBehaviour4 script via its IsDead property.

    /// <summary>
    /// Initializes components and starting values.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Get the required 'body' component for this 'brain'
        enemyBehaviour = GetComponent<EnemyBehaviour4>();
        if (enemyBehaviour == null)
        {
            Debug.LogError("SnapperAI requires an EnemyBehaviour4 component to function.", this);
            this.enabled = false;
            return;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero; // Use .velocity, the modern standard
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("SnapperAI: Player not found!", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// Contains the main AI logic to decide between attacking, chasing, or being idle each frame.
    /// </summary>
    void Update()
    {
        // Check the 'body' script's state to halt logic when dead, attacking, etc.
        if (enemyBehaviour == null || !enemyBehaviour.enabled || isAttacking || player == null)
        {
            return;
        }

        FacePlayer();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            float attackDistance = Vector2.Distance(attackPoint.position, player.position);

            // Check if player is in range and attack is off cooldown
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
    /// Sets the 'isAttacking' flag, controlled by the AttackStateBehaviour on the Animator.
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    /// <summary>
    /// Initiates the attack by setting a cooldown and triggering the animation.
    /// </summary>
    void AttackDecision()
    {
        lastAttackTime = Time.time;
        StartCoroutine(AttackSequence());
    }

    /// <summary>
    /// Coroutine xử lý toàn bộ chuỗi tấn công.
    /// </summary>
    IEnumerator AttackSequence()
    {
        isAttacking = true;
        anim.SetTrigger("isAttacking");

        yield return new WaitForSeconds(0.5f);

        DealDamageEvent();

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
    }

    /// <summary>
    /// Called by an Animation Event at the precise moment the attack should deal damage.
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
    /// Moves the Kinematic Rigidbody towards the player horizontally.
    /// </summary>
    void MoveTowardsPlayer()
    {
        // Stop moving if we are already inside the attack range to prevent jittering.
        if (Vector2.Distance(attackPoint.position, player.position) <= attackRadius)
        {
            StopMovement();
            return;
        }

        anim.SetBool("isWalking", true);

        // Calculate the horizontal target position
        Vector2 targetPosition = new Vector2(player.position.x, rb.position.y);
        // Move towards the target using the correct method for kinematic bodies
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Halts horizontal movement and stops the walking animation.
    /// </summary>
    void StopMovement()
    {
        anim.SetBool("isWalking", false);
        // For a Kinematic body, movement stops when we stop calling MovePosition.
    }

    /// <summary>
    /// Flips the enemy's sprite to face the player's horizontal position.
    /// </summary>
    void FacePlayer()
    {
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
        // Draw the main detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw the attack initiation range and hitbox
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}