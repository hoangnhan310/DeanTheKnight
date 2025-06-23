using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFloatOnWaterMap3 : MonoBehaviour
{
    [Header("Water Physics Settings")]
    [Tooltip("The maximum upward force to counteract gravity.")]
    public float buoyancyForce = 35f;

    [Tooltip("Water drag. Lower values will bounce more, higher values will be more stable.")]
    public float waterDrag = 3f;

    [Tooltip("Adjusts the player's float height. 0.5 means half-submerged.")]
    [Range(0.0f, 1.0f)]
    public float floatHeight = 0.5f;

    [Header("Bobbing Effect")]
    [Tooltip("Enable/disable the gentle bobbing effect.")]
    public bool enableBobbing = true;

    [Tooltip("The strength of the bobbing. Should be a small value.")]
    public float bobbingForce = 0.5f;

    [Tooltip("The speed of the bobbing motion.")]
    public float bobbingSpeed = 1.0f;

    // ---- UPDATED: Fields for water effects ----
    [Header("Water Effects")]
    [Tooltip("The Prefab for the splash effect when entering water.")]
    public GameObject splashEffectPrefab;

    [Tooltip("The vertical velocity threshold to trigger a splash. Must be negative.")]
    public float splashVelocityThreshold = -3f;


    // --- Private Variables ---
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float originalDrag;
    private bool isInWater = false;
    private Collider2D waterCollider;

    private const string WATER_TAG = "Water_Map3";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        // CORRECTED: Rigidbody2D uses 'drag', not 'linearDamping'.
        originalDrag = rb.linearDamping;
    }

    void FixedUpdate()
    {
        if (!isInWater || waterCollider == null) return;

        // --- Buoyancy Calculation ---
        float waterSurfaceY = waterCollider.bounds.max.y;
        float playerHeight = playerCollider.bounds.size.y;
        float floatPointY = waterSurfaceY - (playerHeight * (1.0f - floatHeight));
        float playerBottomY = playerCollider.bounds.min.y;
        float depthDifference = floatPointY - playerBottomY;
        float calculatedBuoyancy = Mathf.Max(0f, depthDifference * buoyancyForce);
        rb.AddForce(new Vector2(0f, calculatedBuoyancy));

        // --- Apply Bobbing Force ---
        if (enableBobbing)
        {
            float bobbingValue = Mathf.Sin(Time.time * bobbingSpeed) * bobbingForce;
            rb.AddForce(new Vector2(0f, bobbingValue));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(WATER_TAG))
        {
            isInWater = true;
            waterCollider = other;
            // CORRECTED: Apply to 'drag' property for Rigidbody2D.
            rb.linearDamping = waterDrag;

            // --- UPDATED: Splash Effect Logic ---
            // Check if a prefab is assigned and the player is falling fast enough
            if (splashEffectPrefab != null && rb.linearVelocity.y < splashVelocityThreshold)
            {
                // Calculate the splash position at the water's surface
                float waterSurfaceY = other.bounds.max.y;
                Vector3 splashPosition = new Vector3(transform.position.x, waterSurfaceY, 0);

                // Create an instance of the splash effect
                Instantiate(splashEffectPrefab, splashPosition, Quaternion.identity);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(WATER_TAG))
        {
            isInWater = false;
            waterCollider = null;
            // CORRECTED: Restore the 'drag' property.
            rb.linearDamping = originalDrag;
        }
    }
}
