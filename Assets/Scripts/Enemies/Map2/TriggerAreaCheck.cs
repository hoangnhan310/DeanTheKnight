using UnityEngine;

public class TriggerAreaCheck : MonoBehaviour
{
    private EnemyBehaviour enemyParent;

    private void Awake()
    {
        enemyParent = GetComponentInParent<EnemyBehaviour>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameObject.SetActive(false); 
            enemyParent.target = collision.transform; 
            enemyParent.inRange = true; 
            enemyParent.hotZone.SetActive(true);
            
        }
    }
}

