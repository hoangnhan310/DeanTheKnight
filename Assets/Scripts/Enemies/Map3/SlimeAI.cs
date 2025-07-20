using System.Collections;
using UnityEngine;

/// <summary>
/// Controls all AI, combat, and health logic for the Slime enemy.
/// This is a self-contained script that handles bouncing, aura attacks, and its own damageable state.
/// </summary>
public class SlimeAI : MonoBehaviour, IDamageable, IStatefulEnemy
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

    [Header("Stats")]
    [Tooltip("The maximum health of the slime.")]
    public float maxHealth = 60f;
    private float currentHealth;

    [Header("AI Behavior")]
    [Tooltip("The range at which the slime starts to detect and bounce towards the player.")]
    public float detectionRange = 12f;
    [Tooltip("The range at which the slime stops moving and initiates its aura attack.")]
    public float attackRange = 3f;

    [Header("Kinematic Bounce Movement")]
    [Tooltip("The maximum height the slime reaches during its bounce arc.")]
    public float bounceHeight = 1.2f;
    [Tooltip("How long, in seconds, a single bounce takes to complete.")]
    public float bounceDuration = 0.7f;
    [Tooltip("The horizontal distance covered in a single bounce.")]
    public float bounceDistance = 2.0f;
    [Tooltip("The cooldown time in seconds between bounces after landing.")]
    public float bounceInterval = 0.5f;

    [Header("Aura Blast Attack")]
    [Tooltip("The cooldown in seconds between aura blast attacks.")]
    public float attackInterval = 4f;
    [Tooltip("The radius of the damage-dealing aura, measured from the Attack Point.")]
    public float explosionRadius = 3f;
    [Tooltip("The amount of damage dealt by the aura blast.")]
    public float explosionDamage = 10f;

    // --- Private State Variables ---
    private float lastBounceTime = -99f;
    private float lastAttackTime = -99f;
    private bool isBouncing = false;
    private bool isAttacking = false;
    private bool isDead = false;

    /// <summary>
    /// Initializes components and sets the starting state.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero; // Use .velocity instead of the obsolete .linearVelocity
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else { Debug.LogError("SlimeAI: Player not found! Ensure the player has the 'Player' tag.", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// The main AI logic loop, called every frame to decide between attacking, bouncing, or idling.
    /// </summary>
    void Update()
    {
        if (isDead || isAttacking || isBouncing || player == null)
        {
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
            if (IsGrounded() && Time.time >= lastBounceTime + bounceInterval)
            {
                StartCoroutine(BounceRoutine());
            }
        }
        else
        {
            anim.SetBool("isBouncing", false);
        }
    }

    /// <summary>
    /// Checks if the slime is on a surface by casting a short ray downwards.
    /// </summary>
    /// <returns>True if ground is detected, otherwise false.</returns>
    private bool IsGrounded()
    {
        // For production, it's recommended to add a LayerMask to this Raycast.
        return Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
    }

    /// <summary>
    /// Executes a single, arced bounce movement towards the player over a set duration.
    /// </summary>
    IEnumerator BounceRoutine()
    {
        isBouncing = true;
        lastBounceTime = Time.time;
        anim.SetBool("isBouncing", true);
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector2 directionToPlayer = new Vector2(player.position.x - startPos.x, 0).normalized;
        Vector3 endPos = startPos + (Vector3)directionToPlayer * bounceDistance;

        while (elapsedTime < bounceDuration)
        {
            float t = elapsedTime / bounceDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += bounceHeight * Mathf.Sin(Mathf.PI * t); // Creates the parabolic arc
            rb.MovePosition(currentPos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Raycast down to snap to the ground, accounting for slopes.
        RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.down, bounceHeight + 1.0f);
        if (hit.collider != null) { endPos.y = hit.point.y; }

        rb.MovePosition(endPos);
        isBouncing = false;
        anim.SetBool("isBouncing", false);
    }

    /// <summary>
    /// Initiates the attack sequence by setting state flags and triggering the animation.
    /// </summary>
    void AttackDecision()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        anim.SetTrigger("isAttacking"); // Must match the trigger name in the Animator
    }

    /// <summary>
    /// This function is called by an Animation Event on the 'attack' animation clip.
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

    /// <summary>
    /// This method is part of the IDamageable interface. It's called when the player's attack hits this enemy.
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log($"Slime took {damage} damage. Current Health: {currentHealth}/{maxHealth}");
        // A hurt animation trigger would be called here if it existed.
        if (currentHealth <= 0) Die();
    }

    /// <summary>
    /// Initiates the death sequence, disabling components and playing the death animation.
    /// </summary>
    void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        anim.SetTrigger("isDie"); // Must match the trigger name in the Animator
        GetComponent<Collider2D>().enabled = false;
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        this.enabled = false;
        Destroy(gameObject, 2.0f);
    }

    /// <summary>
    /// Allows an external script (like AttackStateBehaviour) to control the 'isAttacking' state.
    /// This is part of the IStatefulEnemy interface.
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
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