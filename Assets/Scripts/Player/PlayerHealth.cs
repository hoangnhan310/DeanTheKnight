using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField]
    private float currentHealth;
    [SerializeField] private GameObject defeatedUI;
    [SerializeField] private HealthBar healthBar; // Gán trong Inspector

    private Animator animator;
    private bool isDead = false;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Reset máu về tối đa mỗi lần bắt đầu
        currentHealth = maxHealth;

        healthBar?.SetHealth(currentHealth, maxHealth);
    }


    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {amount} damage. Remaining health: {currentHealth}");

        // Gọi animation trúng đòn
        animator.SetTrigger("Hurt");

        // Cập nhật UI thanh máu
        healthBar?.SetHealth(currentHealth, maxHealth);

        // Lưu máu vào PlayerPrefs
        PlayerPrefs.SetFloat("PlayerCurrentHealth", currentHealth);
        PlayerPrefs.Save();

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
            combat.enabled = false;

        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        gameObject.tag = "Untagged";
        gameObject.layer = 0;
        StartCoroutine(ShowDefeatAndPause());
    }
    private IEnumerator ShowDefeatAndPause()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây

        if (defeatedUI != null)
            defeatedUI.SetActive(true);

        Time.timeScale = 0f; // Tạm dừng toàn bộ game
    }
}
