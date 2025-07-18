using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    private Animator animator;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Remaining health: {currentHealth}");

        // Gọi animation bị trúng đòn
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");
        animator.SetTrigger("Death");
        GetComponent<PlayerInput>().enabled = false;
        var combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.enabled = false;
        }
        // Ngắt di chuyển, điều khiển, v.v.
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        gameObject.tag = "Untagged"; // Đặt tag thành Untagged
        gameObject.layer = 0; // Đ
    }

   
}
