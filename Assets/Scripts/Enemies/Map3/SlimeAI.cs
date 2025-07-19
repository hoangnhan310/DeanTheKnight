using System.Collections;
using UnityEngine;

public class SlimeAI : MonoBehaviour, IDamageable, IStatefulEnemy
{
    [Header("References")]
    public Transform player;
    public GameObject auraVFX;
    public Transform attackPoint;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float maxHealth = 60f;
    private float currentHealth;

    [Header("AI Behavior")]
    public float detectionRange = 12f;
    public float attackRange = 3f;

    [Header("Kinematic Bounce Movement")]
    public float bounceHeight = 1.2f;
    public float bounceDuration = 0.7f;
    public float bounceDistance = 2.0f;
    [Tooltip("Time between bounces after landing.")]
    public float bounceInterval = 0.5f;

    [Header("Aura Blast Attack")]
    [Tooltip("Time between aura blast attacks.")]
    public float attackInterval = 4f;
    public float explosionRadius = 3f;
    public float explosionDamage = 10f;

    // --- Private State Variables ---
    private float lastBounceTime = -99f;
    private float lastAttackTime = -99f;
    private bool isBouncing = false;
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
            else { Debug.LogError("SlimeAI: Player not found!", this); this.enabled = false; }
        }
    }

    /// <summary>
    /// Called every frame, contains the main AI logic to decide between attacking or bouncing.
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
    /// Checks if the slime is on the ground using a short raycast.
    /// </summary>
    private bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
    }

    /// <summary>
    /// A coroutine that performs a single arced bounce movement towards the player.
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
            currentPos.y += bounceHeight * Mathf.Sin(Mathf.PI * t);
            rb.MovePosition(currentPos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.down, 1.0f);
        if (hit.collider != null) { endPos.y = hit.point.y; }
        rb.MovePosition(endPos);
        isBouncing = false;
        anim.SetBool("isBouncing", false);
    }

    /// <summary>
    /// Initiates the attack sequence by setting state flags and triggering the attack animation.
    /// </summary>
    void AttackDecision()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        anim.SetTrigger("isAttacking");
    }

    /// <summary>
    /// An animation event function that creates the visual effect and deals damage for the aura blast.
    /// </summary>
    public void AuraBlastEvent()
    {
        if (auraVFX != null)
        {
            Instantiate(auraVFX, attackPoint.position, Quaternion.identity);
        }

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, explosionRadius);
        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(explosionDamage);
                }
            }
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
        StopAllCoroutines();
        anim.SetTrigger("isDie");
        GetComponent<Collider2D>().enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        this.enabled = false;
        Destroy(gameObject, 2.0f);
    }

    /// <summary>
    /// Allows setting the attacking state from outside (typically for Animation Events or StateMachineBehaviours).
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
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Draws Gizmos in the Editor to visualize the detection, attack, and explosion ranges.
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