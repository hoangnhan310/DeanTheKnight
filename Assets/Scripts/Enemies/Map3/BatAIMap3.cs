using System.Collections;
using UnityEngine;

public class BatAIMap3 : MonoBehaviour, IDamageable, IStatefulEnemy
{
    [Header("References")]
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float maxHealth = 30f;
    private float currentHealth;
    public float moveSpeed = 2f;

    [Header("AI Behavior")]
    [Tooltip("The larger range at which the bat starts chasing the player.")]
    public float detectionRange = 10f;
    [Tooltip("The inner range at which the bat can perform its lunge attack.")]
    public float attackRange = 4f;

    [Header("Lunge Attack")]
    public float lungeSpeed = 8f;
    public float attackInterval = 3f;
    public float attackDamage = 10f;

    [Header("Patrol Behavior")]
    [Tooltip("How far the bat will patrol left and right from its start position.")]
    public float patrolDistance = 3f;

    private Vector3 startingPoint;
    private Vector3 patrolTarget;
    private float lastAttackTime = -99f;
    private bool isAttacking = false;
    private bool isDead = false;

    /// <summary>
    /// Called once when the script instance is being loaded to initialize components and values.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

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
    /// Called every frame, contains the main AI logic to decide between attacking, chasing, or patrolling.
    /// </summary>
    void Update()
    {
        if (isDead || isAttacking || player == null)
        {
            return;
        }

        FaceTarget(player.position);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackInterval)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// Handles the patrol movement logic when the player is out of detection range.
    /// </summary>
    void Patrol()
    {
        FaceTarget(patrolTarget);

        transform.position = Vector2.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f)
        {
            SetNewPatrolTarget();
        }
    }

    /// <summary>
    /// Sets a new random patrol target within a certain distance from the starting point.
    /// </summary>
    void SetNewPatrolTarget()
    {
        float randomX = Random.Range(startingPoint.x - patrolDistance, startingPoint.x + patrolDistance);
        patrolTarget = new Vector3(randomX, startingPoint.y, startingPoint.z);
    }

    /// <summary>
    /// Executes the attack action by lunging towards the player's position.
    /// </summary>
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        Vector3 lungeTargetPosition = player.position;
        anim.SetTrigger("isAttackin");

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
    /// Detects collision with the player while attacking in order to deal damage.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        IDamageable playerHealth = other.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Handles taking damage, reduces health, and calls the Die() method if necessary.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    /// <summary>
    /// Initiates the death sequence, playing an animation and destroying the GameObject.
    /// </summary>
    void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("isDie");
        rb.bodyType = RigidbodyType2D.Static;
        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// Flips the sprite to face the target's position (left or right).
    /// </summary>
    void FaceTarget(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Allows setting the attacking state from outside (typically for Animation Events).
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    /// <summary>
    /// Draws Gizmos in the Editor to visualize the detection and attack ranges.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}