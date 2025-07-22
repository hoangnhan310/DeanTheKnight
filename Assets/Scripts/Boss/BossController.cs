using UnityEngine;

public class BossController : EnemyBehaviour4
{
    [Header("Boss Settings")]
    public float moveSpeed = 3f;
    public float attackCooldown = 2f;
    public GameObject summonPrefab;
    public Transform[] summonPoint;
    public int phase2HealthThreshold = 50;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRadius = 1.2f;
    public LayerMask playerLayer;
    public float attackDamage = 10f;

    public BossHealthBar healthBar;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private float lastAttackTime;
    public bool isFighting = false;
    private int phase = 1;
    private bool hasSummoned = false;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        if (!isFighting || player == null) return;
        FlipToPlayer();
        TryChaseAndAttackPlayer();
        if (phase == 1)
        {
            if (currentHealth <= phase2HealthThreshold)
            {
                phase = 2;
            }
        }
        else if (phase == 2)
        {
            if (!hasSummoned)
            {
                // Summon minions
                foreach (var point in summonPoint)
                {
                    if (point != null)
                    {
                        SummonMinion(point);
                    }
                }
                hasSummoned = true;
            }
        }
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetFloat("xVelocity", 0);
    }

    private void TryChaseAndAttackPlayer()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            if (attackPoint != null)
            {
                Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
                if (hit != null && hit.transform == player)
                {
                    StopMoving();
                    animator.SetTrigger("Attack");
                    lastAttackTime = Time.time;
                    hit.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
                }
                else
                {
                    ChasePlayer();
                }
            }

        }
    }
    // Vẽ gizmo cho attack point trong editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }

    private void SummonMinion(Transform summonPoint)
    {
        if (summonPrefab != null && summonPoint != null)
        {
            GameObject minion = Instantiate(summonPrefab, summonPoint.position, Quaternion.identity);
            var patrolEnemy = minion.GetComponent<PatrolEnemy>();
            if (patrolEnemy != null && player != null)
            {
                patrolEnemy.player = player;
            }
        }
    }

    public void FlipToPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    protected override void Die()
    {
        base.Die();
        BossAudioManager.Instance.StopBossMusic();
        var summoned = FindObjectsByType<PatrolEnemy>(FindObjectsSortMode.None);
        foreach (var minion in summoned)
        {
            Destroy(minion.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFighting)
        {
            isFighting = true;
        }
    }
    private void OnDestroy()
    {
        if (CrongratUI.Instance != null)
        {
            CrongratUI.Instance.TriggerWin();
        }
    }
}
