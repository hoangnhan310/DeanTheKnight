using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Gây sát thương nếu player có component TakeDamage
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Enemy hit player for {damage} damage");
            }
        }
    }
}
