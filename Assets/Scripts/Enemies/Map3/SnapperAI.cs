using UnityEngine;
using System.Collections;

public class SnapperAI : MonoBehaviour, IDamageable, IStatefulEnemy
{
    [Header("References")]
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float maxHealth = 50f;
    private float currentHealth;
    public float moveSpeed = 2.5f;

    [Header("AI Behavior")]
    [Tooltip("The range at which the Snapper starts moving towards the player.")]
    public float detectionRange = 12f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 1.0f;
    public float attackDamage = 15f;
    public float attackInterval = 2.5f;

    // --- Private State Variables ---
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

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("SnapperAI: Player not found!", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// Called every frame, contains the main AI logic to decide between attacking, chasing, or being idle.
    /// </summary>
    void Update()
    {
        if (isDead || isAttacking || player == null)
        {
            return;
        }

        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
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
    /// Allows setting the attacking state from outside (typically for Animation Events or StateMachineBehaviours).
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    /// <summary>
    /// Initiates the attack sequence by setting state flags and triggering the attack animation.
    /// </summary>
    void AttackDecision()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("isAttacking");
    }

    /// <summary>
    /// An animation event function that detects and deals damage to the player within the attack range.
    /// </summary>
    public void DealDamageEvent()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            IDamageable damageable = playerCollider.GetComponent<IDamageable>();
            if (damageable != null && playerCollider.CompareTag("Player"))
            {
                damageable.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// Handles the physical movement towards the player, stopping when within attack range.
    /// </summary>
    void MoveTowardsPlayer()
    {
        float distanceToPlayer = Vector2.Distance(attackPoint.position, player.position);

        if (distanceToPlayer <= attackRadius)
        {
            StopMovement();
            return;
        }

        anim.SetBool("isWalking", true);
        Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
        Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Halts the enemy's movement by setting the walking animation to false.
    /// </summary>
    void StopMovement()
    {
        anim.SetBool("isWalking", false);
    }

    /// <summary>
    /// Handles taking damage, reduces health, and calls the Die() method if necessary.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Initiates the death sequence by triggering the animation and starting a cleanup routine.
    /// </summary>
    void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("isDie");
        StartCoroutine(DieSequenceRoutine());
    }

    /// <summary>
    /// A coroutine that disables components and destroys the GameObject after the death animation.
    /// </summary>
    IEnumerator DieSequenceRoutine()
    {
        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;

        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Flips the enemy's sprite to face the player.
    /// </summary>
    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Draws Gizmos in the Editor to visualize the detection and attack ranges.
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