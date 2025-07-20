using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the AI logic for the Bat enemy. Handles patrolling, chasing, and initiating attacks.
/// This script works in tandem with an EnemyBehaviour4 component, which handles health and death states.
/// </summary>
public class BatAIMap3 : MonoBehaviour, IStatefulEnemy // No longer needs IDamageable
{
    [Header("References")]
    [Tooltip("The player the bat will target.")]
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    [Tooltip("Reference to the component that manages this enemy's health and death state.")]
    [SerializeField] private EnemyBehaviour4 enemyBehaviour;

    [Header("Stats")]
    [Tooltip("General movement speed for patrolling and chasing.")]
    public float moveSpeed = 2f;

    [Header("AI Behavior")]
    [Tooltip("The larger range at which the bat starts chasing the player.")]
    public float detectionRange = 10f;
    [Tooltip("The inner range at which the bat can perform its lunge attack.")]
    public float attackRange = 4f;

    [Header("Lunge Attack")]
    [Tooltip("How fast the bat lunges towards the player.")]
    public float lungeSpeed = 8f;
    [Tooltip("Time in seconds between attacks.")]
    public float attackInterval = 3f;
    [Tooltip("The amount of damage this bat's attack deals.")]
    public float attackDamage = 10f; // Damage is now managed here

    [Header("Patrol Behavior")]
    [Tooltip("How far the bat will patrol left and right from its start position.")]
    public float patrolDistance = 3f;

    // --- Private State Variables ---
    private Vector3 startingPoint;
    private Vector3 patrolTarget;
    private float lastAttackTime = -99f;
    private bool isAttacking = false;
    // 'isDead' is now handled by the EnemyBehaviour4 script.

    /// <summary>
    /// Initializes components and starting values.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Get the 'body' component for this 'brain'
        enemyBehaviour = GetComponent<EnemyBehaviour4>();
        if (enemyBehaviour == null)
        {
            Debug.LogError("BatAIMap3 requires an EnemyBehaviour4 component to function.", this);
            this.enabled = false; // Disable self if the 'body' is missing
            return;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero; // Use velocity, not linearVelocity
        }

        // Ensure the main collider is a trigger for damage detection
        GetComponent<Collider2D>().isTrigger = true;

        startingPoint = transform.position;
        SetNewPatrolTarget();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("BatAI: Player not found!", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// Contains the main AI logic for deciding between attacking, chasing, or patrolling.
    /// </summary>
    void Update()
    {
        if (!enemyBehaviour.enabled || isAttacking || player == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            FaceTarget(player.position);

            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackInterval)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// Handles the patrol movement when the player is out of range.
    /// </summary>
    void Patrol()
    {
        FaceTarget(patrolTarget);
        Vector2 newPosition = Vector2.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f)
        {
            SetNewPatrolTarget();
        }
    }

    /// <summary>
    /// Handles the logic for chasing the player.
    /// </summary>
    void ChasePlayer()
    {
        Vector2 newPosition = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Sets a new random target for patrolling.
    /// </summary>
    void SetNewPatrolTarget()
    {
        float randomX = Random.Range(startingPoint.x - patrolDistance, startingPoint.x + patrolDistance);
        patrolTarget = new Vector3(randomX, startingPoint.y, startingPoint.z);
    }

    /// <summary>
    /// Executes the lunge attack coroutine.
    /// </summary>
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        anim.SetTrigger("isAttacking");

        Vector3 lungeTargetPosition = player.position;
        float lungeDuration = Vector2.Distance(transform.position, lungeTargetPosition) / lungeSpeed;
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < lungeDuration)
        {
            transform.position = Vector3.Lerp(startPosition, lungeTargetPosition, elapsedTime / lungeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        isAttacking = false;
    }

    /// <summary>
    /// Detects trigger collisions to deal damage to the player during an attack.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking || !other.CompareTag("Player"))
        {
            return;
        }

        // Directly find the PlayerHealth component and deal damage
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    // --- DELEGATED METHODS ---
    // TakeDamage and Die are now removed, as EnemyBehaviour4 handles them.

    /// <summary>
    /// Allows the AttackStateBehaviour to set the attacking flag.
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    /// <summary>
    /// Flips the sprite to face the specified target.
    /// </summary>
    void FaceTarget(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Draws visualization gizmos in the Unity Editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}