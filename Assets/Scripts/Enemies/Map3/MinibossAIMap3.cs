using System.Collections;
using UnityEngine;

public class MinibossAIMap3 : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public LayerMask playerLayer;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float maxHealth = 200f;
    private float currentHealth;
    public float moveSpeed = 1.5f;
    [Tooltip("How long the enemy is immune to damage after being hit.")]
    public float invincibilityDuration = 0.5f;

    [Header("AI Behavior")]
    public float detectionRange = 10f;
    [Tooltip("How long it takes for the boss to react and turn around.")]
    public float turnDelay = 0.5f;
    // --- The old 'attackRange' variable is now completely REMOVED ---

    [Header("Unified Attack Zone")]
    public Transform attackPoint;
    public float attackRadius = 0.8f;
    public float normalAttackDamage = 15f;
    public float attackInterval = 2f;

    [Header("Special Attack Combo")]
    public int attacksUntilSpecial = 3;
    public float specialAttackDamage = 10f;
    public float attackComboResetTime = 5.0f;

    [System.NonSerialized]
    private int normalAttackCounter = 0;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool isChasing = false;
    private bool isDead = false;
    private Vector3 initialScale;
    private bool isSpecialAttack = false;
    private Coroutine resetAttackComboCoroutine;
    private bool isInvincible = false;
    private bool isTurning = false;


    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        initialScale = transform.localScale;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("MinibossAI: Player not found!", this);
                this.enabled = false;
            }
        }
    }

    void Update()
    {
        // --- Gatekeeper Section ---
        if (isDead || player == null || isAttacking || isTurning)
        {
            return;
        }

        // --- Decision-Making Section ---
        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- CHASE STATE LOGIC ---
        // Condition to START chasing
        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }
        // Condition to STOP chasing
        else if (distanceToPlayer > detectionRange * 1.5f)
        {
            isChasing = false;
        }


        // --- ACTION LOGIC ---
        if (isChasing)
        {
            // Player is a target. Decide to ATTACK or CHASE.
            float attackDistance = Vector2.Distance(attackPoint.position, player.position);

            // A1. Can we attack? (In range AND ready)
            if (attackDistance <= attackRadius && Time.time >= lastAttackTime + attackInterval)
            {
                // ---- STATE: ATTACK ----
                StopMovement();
                AttackDecision();
            }
            // A2. Not ready to attack, so keep chasing.
            else
            {
                // ---- STATE: CHASE ----
                MoveTowardsPlayer();
            }
        }
        else
        {
            StopMovement();
        }
    }

    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    void AttackDecision()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("isAttacking");

        if (resetAttackComboCoroutine != null)
        {
            StopCoroutine(resetAttackComboCoroutine);
        }
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

    IEnumerator AttackComboResetRoutine()
    {
        yield return new WaitForSeconds(attackComboResetTime);

        Debug.Log("Attack combo reset due to inactivity.");
        normalAttackCounter = 0;

        resetAttackComboCoroutine = null;
    }

    public void DealDamageEvent()
    {
        float damageToDeal;
        if (isSpecialAttack)
        {
            damageToDeal = specialAttackDamage;
        }
        else
        {
            damageToDeal = normalAttackDamage;
        }

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToDeal);
            }
        }

        isSpecialAttack = false;
    }


    void MoveTowardsPlayer()
    {
        anim.SetBool("isWalking", true);
        float moveDirection = (player.position.x > transform.position.x) ? 1 : -1;

        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

    }

    void StopMovement()
    {
        anim.SetBool("isWalking", false);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void TakeDamage(float damage)
    {
        // --- NEW "POWER ARMOR" LOGIC ---
        // If the Golem is dead OR in the middle of an attack animation, ignore the hit.
        if (isDead || isAttacking)
        {
            Debug.Log("Attack ignored! Golem is currently attacking.");
            return;
        }

        // --- STANDARD HURT LOGIC ---
        // If the Golem is NOT attacking, it is vulnerable.

        currentHealth -= damage;
        Debug.Log($"Golem took {damage} damage. Current Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Still alive, so play the standard hurt animation.
            anim.SetTrigger("isHurt");
        }
    }

    // The master Die() function. Its only job is to set the flag and start the coroutine ONCE.
    void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("isDie"); // The trigger is set here.
        StartCoroutine(DieSequenceRoutine());
    }


    // --- THE FIX IS HERE ---
    IEnumerator DieSequenceRoutine()
    {
        Debug.Log("DieSequence has started! This should only appear once.");

        // REMOVE THIS LINE: The trigger is already set in the Die() function.
        // anim.SetTrigger("isDie"); 

        // The rest of the routine is for cleanup and destruction.
        if (resetAttackComboCoroutine != null)
        {
            StopCoroutine(resetAttackComboCoroutine);
        }

        GetComponent<Collider2D>().enabled = false;
        if (rb != null)
        {
            // This is a more stable way to freeze a Rigidbody on death
            rb.bodyType = RigidbodyType2D.Static;
        }

        yield return new WaitForSeconds(3.0f);

        Destroy(gameObject);
    }

    void FacePlayer()
    {
        if (isTurning)
        {
            return;
        }

        // Determine which way the AI *should* be facing.
        float playerDirection = Mathf.Sign(player.position.x - transform.position.x);

        // Determine which way the AI *is* currently facing.
        float currentFacingDirection = Mathf.Sign(transform.localScale.x);

        // If the directions do not match, a turn is needed.
        if (playerDirection != currentFacingDirection)
        {
            // Start the delayed turn coroutine instead of turning instantly.
            StartCoroutine(DelayedTurnRoutine());
        }
    }

    private IEnumerator DelayedTurnRoutine()
    {
        // 1. Set the flag to prevent this coroutine from running multiple times.
        isTurning = true;

        // 2. Wait for the specified "reaction time".
        yield return new WaitForSeconds(turnDelay);

        // 3. After the delay, execute the turn by flipping the local scale.
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        // 4. Reset the flag so the Golem can turn again later.
        isTurning = false;
    }

    // --- UPDATED GIZMOS ---
    // This now only draws the circles the AI actually uses.
    void OnDrawGizmosSelected()
    {
        // Draw the Detection Range (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw the unified Attack Zone (Purple)
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}