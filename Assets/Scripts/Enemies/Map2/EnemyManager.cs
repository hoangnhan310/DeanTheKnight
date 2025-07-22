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
        if (enemyCount == 1)
        {
            cameraSwitcher.SummonFocus();
            StartCoroutine(DelayForSummonFalling());
        }
    }

    private IEnumerator DelayForSummonFalling()
    {
        yield return new WaitForSeconds(timeSummonFPDelay);
        interactiveObject.SetActive(true);

        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        if (interactiveObject == null)
        {
            yield break;
        }

        float fadeDuration = 2f;
        float elapsedTime = 0f;

        // Get all SpriteRenderers in child objects
        SpriteRenderer[] spriteRenderers = interactiveObject.GetComponentsInChildren<SpriteRenderer>();

        // Store original colors and set initial alpha to 0
        Color[] originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
            Color transparent = new Color(originalColors[i].r, originalColors[i].g, originalColors[i].b, 0f);
            spriteRenderers[i].color = transparent;
        }

        // Fade in all SpriteRenderers
        while (elapsedTime < fadeDuration)
        {
            float newAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Color col = originalColors[i];
                spriteRenderers[i].color = new Color(col.r, col.g, col.b, newAlpha);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure alpha is fully restored at the end
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = originalColors[i];
        }
    }
}

