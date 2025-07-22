using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the AI logic for the Slime enemy using a Dynamic Rigidbody for physics-based bouncing.
/// This script requires an EnemyBehaviour4 component to handle health and death states.
/// </summary>
public class SlimeAI : MonoBehaviour, IStatefulEnemy
{
    [Header("References")]
    [Tooltip("The player character the slime will target.")]
    public Transform player;
    [Tooltip("An optional particle effect to spawn during the aura attack.")]
    public GameObject auraVFX;
    [Tooltip("An empty child object representing the center of the aura blast.")]
    public Transform attackPoint;
    private Animator anim;
    private Rigidbody2D rb;
    [Tooltip("Reference to the component that manages health and death states.")]
    [SerializeField] private EnemyBehaviour4 enemyBehaviour;

    [Header("AI Behavior")]
    [Tooltip("The range at which the slime starts to detect and bounce towards the player.")]
    public float detectionRange = 12f;
    [Tooltip("The range at which the slime stops moving and initiates its aura attack.")]
    public float attackRange = 3f;

    [Header("Dynamic Bounce Movement")]
    [Tooltip("The vertical force applied for each bounce.")]
    public float bounceForce = 8f;
    [Tooltip("The horizontal speed when bouncing towards the player.")]
    public float bounceForwardSpeed = 4f;
    [Tooltip("The cooldown time in seconds between bounces.")]
    public float bounceInterval = 1f;

    [Header("Aura Blast Attack")]
    [Tooltip("The cooldown in seconds between aura blast attacks.")]
    public float attackInterval = 4f;
    [Tooltip("The radius of the damage-dealing aura.")]
    public float explosionRadius = 3f;
    [Tooltip("The amount of damage dealt by the aura blast.")]
    public float explosionDamage = 10f;

    // --- Private State Variables ---
    private float lastBounceTime = -99f;
    private float lastAttackTime = -99f;
    private bool isAttacking = false;
    // 'isDead' and 'currentHealth' are now managed by EnemyBehaviour4.

    /// <summary>
    /// Initializes components and sets the starting state.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        enemyBehaviour = GetComponent<EnemyBehaviour4>();
        if (enemyBehaviour == null)
        {
            Debug.LogError("SlimeAI requires an EnemyBehaviour4 component to function.", this);
            this.enabled = false;
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("SlimeAI: Player not found!", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// The main AI logic loop, called every frame to decide between attacking, bouncing, or idling.
    /// </summary>
    void Update()
    {
        // Check the 'body' script's state to stop all actions when dead.
        if (enemyBehaviour == null || !enemyBehaviour.enabled || isAttacking || player == null)
        {
            // If unable to act, stop animating and halt horizontal movement.
            StopBouncing();
            return;
        }

        FacePlayer();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackInterval)
        {
            AttackDecision();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            BounceTowardsPlayer();
        }
        else
        {
            StopBouncing();
        }

        // The bouncing animation is now driven by the Rigidbody's vertical velocity.
        anim.SetBool("isBouncing", Mathf.Abs(rb.linearVelocity.y) > 0.1f);
    }

    /// <summary>
    /// Checks if the slime is on the ground. Required for the dynamic bounce.
    /// </summary>
    private bool IsGrounded()
    {
        // For production, adding a LayerMask here is highly recommended.
        return Physics2D.Raycast(transform.position, Vector2.down, GetComponent<Collider2D>().bounds.extents.y + 0.1f);
    }

    /// <summary>
    /// Initiates a physics-based bounce towards the player by applying forces to the Rigidbody.
    /// </summary>
    void BounceTowardsPlayer()
    {
        // Only allow bouncing if on the ground and the cooldown has passed.
        if (IsGrounded() && Time.time >= lastBounceTime + bounceInterval)
        {
            lastBounceTime = Time.time;
            float direction = (player.position.x > transform.position.x) ? 1f : -1f;

            // Apply an upward and forward velocity to the Dynamic Rigidbody.
            rb.linearVelocity = new Vector2(direction * bounceForwardSpeed, bounceForce);
        }
    }

    /// <summary>
    /// Halts the slime's horizontal movement, typically when idling.
    /// </summary>
    void StopBouncing()
    {
        // Only reduce horizontal velocity if on the ground, allowing for arcs in the air.
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Initiates the attack sequence.
    /// </summary>
    void AttackDecision()
    {
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero; // Halt all movement before attacking.
        anim.SetTrigger("isAttacking");
    }

    /// <summary>
    // This function is called by an Animation Event on the 'attack' animation clip.
    /// It creates the visual effect and deals damage within the aura's radius.
    /// </summary>
    public void AuraBlastEvent()
    {
        if (auraVFX != null)
        {
            Instantiate(auraVFX, attackPoint.position, Quaternion.identity);
        }

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, explosionRadius);
        foreach (Collider2D hitCollider in hitObjects)
        {
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();

            if (playerHealth != null && hitCollider.CompareTag("Player"))
            {
                playerHealth.TakeDamage(explosionDamage);
            }
        }
    }

    // TakeDamage and Die methods are no longer needed here.

    /// <summary>
    /// Allows an external script (like AttackStateBehaviour) to control the 'isAttacking' state.
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    /// <summary>
    /// Flips the enemy's sprite to face the player.
    /// </summary>
    void FacePlayer()
    {
        if (isAttacking) return;

        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Draws visualization gizmos for the AI's ranges in the Unity Editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, explosionRadius);
        }
    }
}