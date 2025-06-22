using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    #region Serialized Fields
    [Header("Combat Settings")]
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float attackRate = 0.25f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 20;
    #endregion

    private Animator animator;
    private Rigidbody2D body2d;
    private PlayerState playerState;

    private int currentAttack = 0;
    private float timeSinceAttack = 0.0f;

    private readonly float rollDuration = 8.0f / 14.0f;
    private float rollCurrentTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        body2d = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
    }

    void Update()
    {
        ProcessAttack();
        ProcessRoll();
    }

    #region Input Callbacks
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && timeSinceAttack > attackRate && !playerState.IsRolling)
        {
            currentAttack++;

            // Loop back to one after third attack
            if (currentAttack > 3)
                currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (timeSinceAttack > 1.0f)
                currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            animator.SetTrigger($"Attack{currentAttack}");

            // Reset timer
            timeSinceAttack = 0.0f;
        }
    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        if (context.performed && !playerState.IsRolling)
        {
            animator.SetTrigger("Block");
            animator.SetBool("IdleBlock", true);
        }
        else if (context.canceled)
        {
            animator.SetBool("IdleBlock", false);
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && !playerState.IsRolling && !playerState.IsWallSliding)
        {
            playerState.IsRolling = true;
            animator.SetTrigger("Roll");
            body2d.linearVelocity = new Vector2(playerState.FacingDirection * rollForce, body2d.linearVelocityY);
        }
    }
    #endregion

    #region Combat Processing
    private void ProcessAttack()
    {
        // Increase timer that controls attack combo
        timeSinceAttack += Time.deltaTime;
    }

    private void ProcessRoll()
    {
        // Increase timer that checks roll duration
        if (playerState.IsRolling)
        {
            rollCurrentTime += Time.deltaTime;
        }

        // Disable rolling if timer extends duration
        if (rollCurrentTime > rollDuration)
        {
            playerState.IsRolling = false;
            rollCurrentTime = 0.0f;
        }
    }
    #endregion

    // Trigger attack in the animation event
    private void TriggerAttack()
    {
        // Detect enemies in attack range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        //Attach hitEnemies
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyBehaviour enemyScript = enemy.GetComponentInParent<EnemyBehaviour>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
                Debug.Log($"Hit {enemy.name} with attack {currentAttack}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Draw attack range in editor 
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}