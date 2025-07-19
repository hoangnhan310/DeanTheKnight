using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinibossAIMap3 : MonoBehaviour, IDamageable, IStatefulEnemy
{
    [Header("References")]
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float maxHealth = 200f;
    private float currentHealth;
    public float moveSpeed = 1.5f;

    [Header("AI Behavior")]
    public float detectionRange = 10f;

    [Header("Unified Attack Zone")]
    public Transform attackPoint; // This now holds the trigger collider
    public float attackRadius = 0.8f;
    public float normalAttackDamage = 15f;
    public float attackInterval = 2f;

    [Header("Special Attack Combo")]
    public int attacksUntilSpecial = 3;
    public float specialAttackDamage = 10f;
    public float attackComboResetTime = 5.0f;

    [Header("Door Control")]
    public GameObject associatedDoor;

    // --- Private State Variables ---
    [System.NonSerialized] private int normalAttackCounter = 0;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool isChasing = false;
    private bool isDead = false;
    private Vector3 initialScale;
    private bool isSpecialAttack = false;
    private Coroutine resetAttackComboCoroutine;


    /// <summary>
    /// Called once when the script instance is being loaded to initialize components and values.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        initialScale = transform.localScale;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        attackPoint.GetComponent<Collider2D>().enabled = false;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("MinibossAI: Player not found!", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// Called every frame, contains the main AI logic to decide between attacking, chasing, or standing still.
    /// </summary>
    void Update()
    {
        if (isDead || player == null || isAttacking) return;
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
    /// Handles the physical movement towards the player and sets the walking animation.
    /// </summary>
    void MoveTowardsPlayer()
    {
        anim.SetBool("isWalking", true);
        Vector2 targetPosition = new Vector2(player.position.x, rb.position.y);
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Stops the enemy's movement by setting the walking animation to false.
    /// </summary>
    void StopMovement()
    {
        anim.SetBool("isWalking", false);
    }

    /// <summary>
    /// A public method called by an animation event to activate the attack hitbox.
    /// </summary>
    public void DealDamageEvent()
    {
        StartCoroutine(DamageWindowRoutine());
    }

    /// <summary>
    /// A coroutine that enables the attack collider for a short "active" window.
    /// </summary>
    IEnumerator DamageWindowRoutine()
    {
        attackPoint.GetComponent<Collider2D>().enabled = true;
        yield return new WaitForSeconds(0.2f);
        attackPoint.GetComponent<Collider2D>().enabled = false;
    }

    /// <summary>
    /// Called when the attack trigger collider hits another object, dealing damage to the player if appropriate.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isAttacking && other.CompareTag("Player"))
        {
            IDamageable playerHealth = other.GetComponent<IDamageable>();
            if (playerHealth != null)
            {
                float damageToDeal = isSpecialAttack ? specialAttackDamage : normalAttackDamage;
                playerHealth.TakeDamage(damageToDeal);
            }
        }
    }

    /// <summary>
    /// Handles taking damage, reduces health, and triggers the hurt or death sequence.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead || isAttacking) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
        else anim.SetTrigger("isHurt");
    }

    /// <summary>
    /// A coroutine that handles the visual fading of the associated door and its eventual destruction.
    /// </summary>
    private IEnumerator FadeOutAndDestroyDoor()
    {
        Tilemap doorTilemap = associatedDoor.GetComponent<Tilemap>();
        Collider2D doorCollider = associatedDoor.GetComponent<Collider2D>();

        if (doorTilemap == null)
        {
            Destroy(associatedDoor);
            yield break;
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        float fadeDuration = 2f;
        float elapsed = 0f;
        Color initialColor = doorTilemap.color;

        while (elapsed < fadeDuration)
        {
            float newAlpha = Mathf.Lerp(initialColor.a, 0f, elapsed / fadeDuration);
            doorTilemap.color = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(associatedDoor);
    }

    /// <summary>
    /// Initiates the death sequence, triggering an animation and opening the associated door.
    /// </summary>
    void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("isDie");

        if (associatedDoor != null)
        {
            StartCoroutine(FadeOutAndDestroyDoor());
        }

        StartCoroutine(DieSequenceRoutine());
    }

    /// <summary>
    /// A coroutine that cleans up the enemy GameObject after the death animation has played.
    /// </summary>
    IEnumerator DieSequenceRoutine()
    {
        if (resetAttackComboCoroutine != null) StopCoroutine(resetAttackComboCoroutine);
        GetComponent<Collider2D>().enabled = false;
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Allows setting the attacking state from outside (typically for Animation Events).
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    /// <summary>
    /// Contains the logic for deciding which attack to use (normal or special).
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
    /// A coroutine to reset the special attack combo counter after a period of time.
    /// </summary>
    IEnumerator AttackComboResetRoutine()
    {
        yield return new WaitForSeconds(attackComboResetTime);
        normalAttackCounter = 0;
        resetAttackComboCoroutine = null;
    }

    /// <summary>
    /// Flips the enemy's sprite to face the player.
    /// </summary>
    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
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