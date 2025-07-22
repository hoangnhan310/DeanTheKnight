using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    #region Serialized Fields
    [Header("Combat Settings")]
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float attackRate = 0.25f;
    [SerializeField] private float resetAttackComboTime = 1.0f;
    [SerializeField] private float attack1Damage = 10.0f;
    [SerializeField] private float attack2Damage = 10.0f;
    [SerializeField] private float attack3Damage = 15.0f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private LayerMask enemyLayers;
    #endregion

    private Animator animator;
    private Rigidbody2D body2d;
    private PlayerState playerState;
    [SerializeField] private StaminaBar staminaBar;
    private int currentAttack = 0;
    private float timeSinceAttack = 0.0f;
    [SerializeField] private float rollStaminaCost = 10f;

    private readonly float rollDuration = 8.0f / 14.0f;
    private float rollCurrentTime;

    // Cheat code state variable
    private bool superDamageMode = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        body2d = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();

        // Get damage
        int damageLevel = PlayerPrefs.GetInt("DamageUpgrades", 0);
        float extraDamage = damageLevel * 5f; // mỗi cấp +5 sát thương

        // Increase damage
        attack1Damage += extraDamage;
        attack2Damage += extraDamage;
        attack3Damage += extraDamage;
    }


    void Update()
    {
        ProcessAttack();
        ProcessRoll();

        // Cheat Code: Press 'G' to toggle super damage mode.
        if (Input.GetKeyDown(KeyCode.G))
        {
            superDamageMode = !superDamageMode;
            if (superDamageMode)
            {
                Debug.Log("CHEAT ACTIVATED: Super Damage Mode ON (500 damage)");
            }
            else
            {
                Debug.Log("CHEAT DEACTIVATED: Super Damage Mode OFF");
            }
        }
    }

    #region Input Callbacks
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && timeSinceAttack > attackRate && !playerState.IsRolling)
        {
            if (staminaBar == null)
            {
                Debug.LogWarning("StaminaBar chưa được gán trong PlayerCombat!");
                return;
            }

            float staminaCost = 35f;

            if (staminaBar.CurrentStamina < staminaCost)
            {
                Debug.Log("Không đủ stamina để tấn công!");
                return;
            }

            staminaBar.UseStamina(staminaCost);

            currentAttack++;

            // Loop back to one after third attack
            if (currentAttack > 3)
                currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (timeSinceAttack > resetAttackComboTime)
                currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            animator.SetTrigger($"Attack{currentAttack}");

            // Check for enemies in attack range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            // Deal damage to each enemy
            float currentAttackDamage;
            // Apply cheat damage if active
            if (superDamageMode)
            {
                currentAttackDamage = 500f;
            }
            else
            {
                // Otherwise, calculate normal combo damage
                currentAttackDamage = currentAttack switch
                {
                    1 => attack1Damage,
                    2 => attack2Damage,
                    3 => attack3Damage,
                    _ => 0.0f
                };
            }

            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponentInParent<EnemyBehaviour4>().TakeDamage(currentAttackDamage);
            }

            // Reset timer
            timeSinceAttack = 0.0f;
        }
    }

    // public void OnBlock(InputAction.CallbackContext context)
    // {
    //     if (context.performed && !playerState.IsRolling)
    //     {
    //         animator.SetTrigger("Block");
    //         animator.SetBool("IdleBlock", true);
    //     }
    //     else if (context.canceled)
    //     {
    //         animator.SetBool("IdleBlock", false);
    //     }
    // }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && !playerState.IsRolling && !playerState.IsWallSliding)
        {
            staminaBar.UseStamina(rollStaminaCost);
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

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        // Draw attack range in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}