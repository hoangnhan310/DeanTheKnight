using UnityEngine;

public class EnemyBehaviour4 : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} initialized with {maxHealth} health.");
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("ok");

        currentHealth -= damage;
        animator.SetBool("Hit", true); 
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetTrigger("Die"); // Kích hoạt animation "Die" trực tiếp
        GetComponent<Collider2D>().enabled = false;
        enabled = false;
        Destroy(gameObject, 2f);
    }

    void OnAnimationFinish()
    {
        animator.SetBool("Hit", false);
    }
}