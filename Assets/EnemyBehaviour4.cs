using UnityEngine;

public class EnemyBehaviour4 : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    public float currentHealth;
    private Rigidbody2D rb;

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
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetBool("Hit", true);
            animator.SetTrigger("isHurt");
        }
    }

    private void Die()
    {
        animator.SetTrigger("isDie");
        animator.SetTrigger("Die"); // Kích hoạt animation "Die" trực tiếp
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Dừng chuyển động
            rb.gravityScale = 0; // Tắt gravity
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation; // Khóa Y và xoay
        }

        // Tắt collider để không va chạm nữa
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Chờ animation kết thúc rồi destroy
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animationLength > 0 ? animationLength : 7f);
    }

    void OnAnimationFinish()
    {
        animator.SetBool("Hit", false);
    }
}