using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private BossController bossController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !bossController.isFighting)
        {
            bossController.isFighting = true;
            BossAudioManager.Instance.PlayBossMusic();
            Debug.Log("Boss fight started with " + collision.gameObject.name);
        }
    }
}