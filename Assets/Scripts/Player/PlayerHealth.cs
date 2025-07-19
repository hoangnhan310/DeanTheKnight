using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [Header("Component References")]
    private Animator animator;
    private PlayerState playerState;
    private HeroKnight heroKnight; // To disable movement
    private PlayerCombat playerCombat; // To disable combat

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        playerState = GetComponent<PlayerState>();
        heroKnight = GetComponent<HeroKnight>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Player's "Roll" provides invincibility frames
        if (playerState.IsRolling)
        {
            Debug.Log("Player dodged the attack!");
            return;
        }

        currentHealth -= damage;
        Debug.Log("Player took " + damage + " damage. Current Health: " + currentHealth);

        if (currentHealth > 0)
        {
            // Trigger the hurt animation
            animator.SetTrigger("Hurt");
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;
        Debug.Log("Player has been defeated.");

        // Trigger death animation
        animator.SetBool("noBlood", true); // Your animator uses this, let's trigger it.
        animator.SetTrigger("Death");

        // Disable player control scripts
        if (heroKnight != null) heroKnight.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;

        // You might want to show a 'Game Over' screen here
    }
}
