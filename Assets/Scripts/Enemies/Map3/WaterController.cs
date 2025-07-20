using UnityEngine;
using System.Collections.Generic;

// Yêu cầu Tilemap phải có một Collider2D để hoạt động như một trigger
[RequireComponent(typeof(Collider2D))]
public class WaterController : MonoBehaviour
{
    [Header("Water Physics Settings")]
    [Tooltip("Lực đẩy nổi tối đa để chống lại trọng lực.")]
    public float buoyancyForce = 35f;

    [Tooltip("Lực cản của nước. Giá trị thấp sẽ nảy nhiều, giá trị cao sẽ ổn định hơn.")]
    public float waterDrag = 3f;

    [Tooltip("Điều chỉnh độ cao nổi của đối tượng. 0.5 nghĩa là chìm một nửa.")]
    [Range(0.0f, 1.0f)]
    public float floatHeight = 0.5f;

    [Header("Bobbing Effect")]
    [Tooltip("Bật/tắt hiệu ứng bập bênh nhẹ nhàng.")]
    public bool enableBobbing = true;

    [Tooltip("Độ mạnh của hiệu ứng bập bênh.")]
    public float bobbingForce = 0.5f;

    [Tooltip("Tốc độ của hiệu ứng bập bênh.")]
    public float bobbingSpeed = 1.0f;

    [Header("Water Effects")]
    [Tooltip("Prefab cho hiệu ứng tóe nước khi đi vào.")]
    public GameObject splashEffectPrefab;

    [Tooltip("Ngưỡng vận tốc theo chiều dọc để kích hoạt hiệu ứng tóe nước (phải là số âm).")]
    public float splashVelocityThreshold = -3f;

    // Danh sách để lưu trữ tất cả các Rigidbody đang ở trong nước
    private List<Rigidbody2D> bodiesInWater = new List<Rigidbody2D>();
    // Dictionary để lưu trữ lực cản ban đầu của mỗi đối tượng
    private Dictionary<Rigidbody2D, float> originalDrags = new Dictionary<Rigidbody2D, float>();

    private const string PLAYER_TAG = "Player"; // Tag của người chơi
    private Collider2D waterCollider;

    void Start()
    {
        waterCollider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        // Lặp qua tất cả các đối tượng trong nước và áp dụng lực
        foreach (var body in bodiesInWater)
        {
            ApplyBuoyancy(body);
        }
    }

    private void ApplyBuoyancy(Rigidbody2D body)
    {
        Collider2D objectCollider = body.GetComponent<Collider2D>();
        if (objectCollider == null) return;

        // --- Tính toán lực đẩy nổi ---
        float waterSurfaceY = waterCollider.bounds.max.y;
        float objectHeight = objectCollider.bounds.size.y;
        float floatPointY = waterSurfaceY - (objectHeight * (1.0f - floatHeight));
        float objectBottomY = objectCollider.bounds.min.y;
        float depthDifference = floatPointY - objectBottomY;
        float calculatedBuoyancy = Mathf.Max(0f, depthDifference * buoyancyForce);
        body.AddForce(new Vector2(0f, calculatedBuoyancy));

        // --- Áp dụng hiệu ứng bập bênh ---
        if (enableBobbing)
        {
            float bobbingValue = Mathf.Sin(Time.time * bobbingSpeed) * bobbingForce;
            body.AddForce(new Vector2(0f, bobbingValue));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ tác động lên đối tượng có tag "Player" và có Rigidbody2D
        if (other.CompareTag(PLAYER_TAG))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && !bodiesInWater.Contains(rb))
            {
                // Lưu lại lực cản ban đầu và áp dụng lực cản của nước
                originalDrags[rb] = rb.linearDamping;
                rb.linearDamping = waterDrag;
                bodiesInWater.Add(rb);

                // --- Hiệu ứng tóe nước ---
                if (splashEffectPrefab != null && rb.linearVelocity.y < splashVelocityThreshold)
                {
                    float waterSurfaceY = waterCollider.bounds.max.y;
                    Vector3 splashPosition = new Vector3(other.transform.position.x, waterSurfaceY, 0);
                    Instantiate(splashEffectPrefab, splashPosition, Quaternion.identity);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && bodiesInWater.Contains(rb))
            {
                // Khôi phục lại lực cản ban đầu và xóa khỏi danh sách
                if (originalDrags.ContainsKey(rb))
                {
                    rb.linearDamping = originalDrags[rb];
                    originalDrags.Remove(rb);
                }
                bodiesInWater.Remove(rb);
            }
        }
    }
}