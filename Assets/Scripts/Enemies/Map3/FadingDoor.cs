using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class FadingDoor : MonoBehaviour
{
    private Tilemap doorTilemap;
    private Collider2D doorCollider;

    /// <summary>
    /// Called when the script instance is being loaded to get necessary components.
    /// </summary>
    void Awake()
    {
        doorTilemap = GetComponent<Tilemap>();
        doorCollider = GetComponent<TilemapCollider2D>();
    }

    /// <summary>
    /// A public method that can be called by other scripts to start the door opening sequence.
    /// </summary>
    public void Open()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        StartCoroutine(FadeOutAndDestroyRoutine());
    }

    /// <summary>
    /// A coroutine that handles the visual fading of the door and its eventual destruction.
    /// </summary>
    private IEnumerator FadeOutAndDestroyRoutine()
    {
        if (doorTilemap == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float fadeDuration = 2f;
        float elapsedTime = 0f;
        Color originalColor = doorTilemap.color;

        while (elapsedTime < fadeDuration)
        {
            float newAlpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeDuration);
            doorTilemap.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}