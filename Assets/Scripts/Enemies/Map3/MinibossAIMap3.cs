using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Controls the AI logic for the Miniboss Golem, handling chasing, combat sequences, and special attacks.
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
    public float moveSpeed = 1.5f;

    [Header("AI Behavior")]
    [Tooltip("The range at which the miniboss will start chasing the player.")]
    public float detectionRange = 10f;

    [Header("Unified Attack Zone")]
    [Tooltip("An empty child object representing the attack hitbox's center.")]
    public Transform attackPoint;
    [Tooltip("The radius from the Attack Point used for attack decisions and dealing damage.")]
    public float attackRadius = 0.8f;
    [Tooltip("Damage dealt by a normal attack.")]
    public float normalAttackDamage = 15f;
    [Tooltip("The cooldown in seconds between attacks.")]
    public float attackInterval = 2f;

    [Header("Special Attack Combo")]
    [Tooltip("Number of normal attacks to perform before a special attack.")]
    public int attacksUntilSpecial = 3;
    [Tooltip("Damage dealt by the special attack.")]
    public float specialAttackDamage = 25f;
    [Tooltip("Time to wait after inactivity before resetting the attack combo.")]
    public float attackComboResetTime = 5.0f;

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

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero; // Use .velocity
        }

        if (attackPoint != null && attackPoint.GetComponent<Collider2D>() != null)
            attackPoint.GetComponent<Collider2D>().enabled = false;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("MinibossAI: Player not found!", this); this.enabled = false; }
        }

        // Store the original layer and find a layer to switch to for invincibility
        originalLayer = gameObject.layer;
        invulnerableLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (invulnerableLayer == -1) // Fallback if the layer doesn't exist
        {
            invulnerableLayer = LayerMask.NameToLayer("Water");
            if (invulnerableLayer == -1) Debug.LogWarning("Could not find 'Ignore Raycast' or 'Water' layer for invincibility frames.", this);
        }
    }

    /// <summary>
    /// The main AI logic loop, called every frame to decide between attacking, chasing, or standing still.
    /// </summary>
    void Update()
    {
        if (enemyBehaviour == null || !enemyBehaviour.enabled || player == null || isAttacking) return;

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
    /// Moves the Kinematic Rigidbody towards the player horizontally.
    /// </summary>
    void MoveTowardsPlayer()
    {
        anim.SetBool("isWalking", true);
        Vector2 targetPosition = new Vector2(player.position.x, rb.position.y);
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Stops the enemy's movement by setting the animation state.
    /// </summary>
    void StopMovement()
    {
        anim.SetBool("isWalking", false);
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
            // When attacking, switch to the invulnerable layer.
            gameObject.layer = invulnerableLayer;
        }
        else
        {
            // When finished attacking, switch back to the original layer.
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
            isSpecialAttack = true;
            normalAttackCounter = 0;
        }
        else
        {
            isSpecialAttack = false;
            normalAttackCounter++;
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
    }
}