using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;
    private Rigidbody2D rb;

    [Header("Patrol & Combat Settings")]
    public float speed = 2f;
    public Transform checkPoint;
    public float distance = 1f;
    public LayerMask layerMask;
    private bool facingLeft = true;
    public bool inRange = false;
    public Transform player;
    public float attackRange = 10f;
    public float retrieveDistance = 2.5f;
    public float chaseSpeed = 3f;

    [Header("Attack Settings")]
    public Animator animator;
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask attackLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} initialized with {maxHealth} health.");
    }

    void Update()
    {
        if (currentHealth <= 0) return; // Dừng logic nếu đã chết

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        inRange = distanceToPlayer <= attackRange;

        if (inRange)
        {
            FlipTowardsPlayer();

            if (distanceToPlayer > retrieveDistance)
            {
                animator.SetBool("Attack1", false);
                transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetBool("Attack1", true);
            }
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        RaycastHit2D hit = Physics2D.Raycast(checkPoint.position, Vector2.down, distance, layerMask);

        if (!hit)
        {
            FlipDirectionPatrol();
        }
    }

    private void FlipDirectionPatrol()
    {
        if (facingLeft)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingLeft = false;
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingLeft = true;
        }
    }

    private void FlipTowardsPlayer()
    {
        if (player.position.x > transform.position.x && facingLeft)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingLeft = false;
        }
        else if (player.position.x < transform.position.x && !facingLeft)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingLeft = true;
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Enemy took damage");

        currentHealth -= damage;
        animator.SetBool("Hit", true);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetTrigger("Die");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        float animTime = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animTime > 0 ? animTime : 7f);
    }

    public void OnAnimationFinish()
    {
        animator.SetBool("Hit", false);
    }

    public void Attack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, attackLayer);

        foreach (Collider2D col in colliders)
        {
            Debug.Log("Hit: " + col.name);
            // Nếu muốn gây damage cho player: col.GetComponent<Player>().TakeDamage(x);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (checkPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(checkPoint.position, Vector2.down * distance);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
