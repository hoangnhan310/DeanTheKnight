using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public int enemyCount;

    public float timeSummonFPDelay = 2f;

    public GameObject interactiveObject;

    public CameraSwitcher cameraSwitcher;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        Debug.Log("Enemy in map: " + enemyCount);
    }

    public void NotifyEnemyKilled()
    {
        enemyCount--;
        //enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (enemyCount <= 0)
        {
            cameraSwitcher.SummonFocus();
            StartCoroutine(DelayForSummonFalling());
        }
    }

    private IEnumerator DelayForSummonFalling()
    {
        yield return new WaitForSeconds(timeSummonFPDelay);
        interactiveObject.SetActive(true);
    }
}
