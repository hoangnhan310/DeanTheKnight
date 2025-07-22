using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This component simulates water physics for any Rigidbody2D that enters its trigger volume.
/// It applies buoyancy, drag, and bobbing effects, and can also generate splash particles.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WaterController : MonoBehaviour
{
    [Header("Water Physics Settings")]
    [Tooltip("The maximum upward force applied to objects to counteract gravity.")]
    public float buoyancyForce = 35f;

    [Tooltip("Movement resistance in the water. Higher values result in less bouncing and quicker stabilization.")]
    public float waterDrag = 3f;

    [Tooltip("Adjusts how high an object floats. A value of 0.5 means half-submerged.")]
    [Range(0.0f, 1.0f)]
    public float floatHeight = 0.5f;

    [Header("Bobbing Effect")]
    [Tooltip("Enables or disables a gentle up-and-down bobbing motion.")]
    public bool enableBobbing = true;

    [Tooltip("The strength of the bobbing motion.")]
    public float bobbingForce = 0.5f;

    [Tooltip("The speed of the bobbing motion.")]
    public float bobbingSpeed = 1.0f;

    [Header("Water Effects")]
    [Tooltip("A particle effect prefab to instantiate when an object enters the water.")]
    public GameObject splashEffectPrefab;

    [Tooltip("The vertical velocity threshold (must be negative) required to trigger the splash effect.")]
    public float splashVelocityThreshold = -3f;

    // --- Private Variables ---
    private List<Rigidbody2D> bodiesInWater = new List<Rigidbody2D>();
    private Dictionary<Rigidbody2D, float> originalDrags = new Dictionary<Rigidbody2D, float>();
    private const string PLAYER_TAG = "Player";
    private Collider2D waterCollider;

    /// <summary>
    /// Initializes the component by getting a reference to its own collider.
    /// </summary>
    void Start()
    {
        waterCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Called every fixed framerate frame, applies buoyancy forces to all objects within the water volume.
    /// </summary>
    void FixedUpdate()
    {
        foreach (var body in bodiesInWater)
        {
            ApplyBuoyancy(body);
        }
    }

    /// <summary>
    /// Calculates and applies buoyancy and bobbing forces to a given Rigidbody2D.
    /// </summary>
    /// <param name="body">The Rigidbody2D to apply forces to.</param>
    private void ApplyBuoyancy(Rigidbody2D body)
    {
        Collider2D objectCollider = body.GetComponent<Collider2D>();
        if (objectCollider == null) return;

        float waterSurfaceY = waterCollider.bounds.max.y;
        float objectHeight = objectCollider.bounds.size.y;
        float floatPointY = waterSurfaceY - (objectHeight * (1.0f - floatHeight));
        float objectBottomY = objectCollider.bounds.min.y;
        float depthDifference = floatPointY - objectBottomY;

        float calculatedBuoyancy = Mathf.Max(0f, depthDifference * buoyancyForce);
        body.AddForce(new Vector2(0f, calculatedBuoyancy));

        if (enableBobbing)
        {
            float bobbingValue = Mathf.Sin(Time.time * bobbingSpeed) * bobbingForce;
            body.AddForce(new Vector2(0f, bobbingValue));
        }
    }

    /// <summary>
    /// Called when another collider enters this object's trigger.
    /// Handles adding objects to the water, applying water drag, and triggering effects.
    /// </summary>
    /// <param name="other">The Collider2D that has entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && !bodiesInWater.Contains(rb))
            {
                originalDrags[rb] = rb.linearDamping;
                rb.linearDamping = waterDrag;
                bodiesInWater.Add(rb);

                if (splashEffectPrefab != null && rb.linearVelocity.y < splashVelocityThreshold)
                {
                    float waterSurfaceY = waterCollider.bounds.max.y;
                    Vector3 splashPosition = new Vector3(other.transform.position.x, waterSurfaceY, 0);
                    Instantiate(splashEffectPrefab, splashPosition, Quaternion.identity);
                }
            }
        }
    }

    /// <summary>
    /// Called every physics frame for any Collider2D that is touching this object's trigger.
    /// This will continuously allow the player to jump as long as they are in the water.
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        // Chỉ tác động lên người chơi
        if (other.CompareTag(PLAYER_TAG))
        {
            PlayerState playerState = other.GetComponent<PlayerState>();
            if (playerState != null)
            {
                // Nếu người chơi không thể nhảy đôi, hãy cho phép họ.
                if (!playerState.CanDoubleJump)
                {
                    playerState.CanDoubleJump = true;
                }
            }
        }
    }

    /// <summary>
    /// Called when a collider exits this object's trigger.
    /// Handles removing objects from the water and restoring their original drag.
    /// </summary>
    /// <param name="other">The Collider2D that has exited the trigger.</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && bodiesInWater.Contains(rb))
            {
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