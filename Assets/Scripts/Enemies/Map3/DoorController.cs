using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

/// <summary>
/// This script controls a door that opens when a specific enemy is defeated.
/// It can find the enemy dynamically by name or tag at the start of the scene.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The NAME of the enemy GameObject that must be defeated. This is case-sensitive.")]
    [SerializeField] private string trackedEnemyName;

    // --- Private Component References ---
    private GameObject trackedEnemy; // Now we track a GameObject
    private Tilemap doorTilemap;
    private Collider2D doorCollider;
    private bool isOpen = false;

    /// <summary>
    /// Finds the tracked enemy in the scene and gets components.
    /// </summary>
    void Start()
    {
        doorTilemap = GetComponent<Tilemap>();
        doorCollider = GetComponent<TilemapCollider2D>();

        // TÌM KIẾM TỰ ĐỘNG KHI BẮT ĐẦU
        if (!string.IsNullOrEmpty(trackedEnemyName))
        {
            trackedEnemy = GameObject.Find(trackedEnemyName);

            if (trackedEnemy == null)
            {
                Debug.LogError($"DoorController on '{gameObject.name}' could not find the enemy named '{trackedEnemyName}' in the scene!", this);
            }
            else
            {
                Debug.Log($"DoorController is now tracking enemy: '{trackedEnemy.name}'");
            }
        }
        else
        {
            Debug.LogWarning("Tracked Enemy Name is not set on the DoorController!", this);
        }
    }

    /// <summary>
    /// Checks if the tracked enemy has been destroyed.
    /// </summary>
    void Update()
    {
        // Nếu cửa đã mở, không làm gì cả
        if (isOpen) return;

        // Nếu 'trackedEnemy' là null, có nghĩa là nó đã bị Destroy().
        // Biến này có thể bắt đầu là null nếu không tìm thấy, hoặc trở thành null sau khi bị phá hủy.
        if (trackedEnemy == null)
        {
            // Kiểm tra xem chúng ta đã mở cửa chưa để tránh gọi coroutine nhiều lần
            if (!isOpen)
            {
                isOpen = true; // Đánh dấu đã mở
                Debug.Log("Tracked enemy has been defeated! Opening the door.");
                StartCoroutine(OpenSequence());
            }
        }
    }

    /// <summary>
    /// The sequence for opening the door, which includes fading and destroying it.
    /// </summary>
    private IEnumerator OpenSequence()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
        if (doorTilemap != null)
        {
            float fadeDuration = 2.0f;
            float elapsedTime = 0f;
            Color originalColor = doorTilemap.color;
            while (elapsedTime < fadeDuration)
            {
                float newAlpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeDuration);
                doorTilemap.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        Destroy(gameObject);
    }
}