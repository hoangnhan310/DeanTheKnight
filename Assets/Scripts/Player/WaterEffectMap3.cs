using UnityEngine;

public class WaterEffectMap3 : MonoBehaviour
{
    public ParticleSystem waterSplashEffect; // Particle System cho hiệu ứng bọt nước
    private bool inWater = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (waterSplashEffect != null)
        {
            waterSplashEffect.Stop(); // Tắt hiệu ứng lúc đầu
            Debug.Log("WaterEffect initialized, Particle System assigned");
        }
        else
        {
            Debug.LogWarning("WaterSplashEffect is not assigned in Inspector");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water_Map3"))
        {
            inWater = true;
            Debug.Log("WaterEffect: Entered Water, Trigger detected");
            StartWaterEffect();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water_Map3"))
        {
            inWater = false;
            Debug.Log("WaterEffect: Exited Water");
            StopWaterEffect();
        }
    }

    void Update()
    {
        if (inWater)
        {
            UpdateWaterEffect();
        }
    }

    void StartWaterEffect()
    {
        if (waterSplashEffect != null)
        {
            waterSplashEffect.Play();
        }
    }

    void StopWaterEffect()
    {
        if (waterSplashEffect != null)
        {
            waterSplashEffect.Stop();
        }
    }

    void UpdateWaterEffect()
    {
        if (waterSplashEffect != null)
        {
            // Điều chỉnh số lượng hạt dựa trên tốc độ di chuyển
            var emission = waterSplashEffect.emission;
            float speed = rb.linearVelocity.magnitude;
            emission.rateOverTime = Mathf.Clamp(speed * 10f, 5f, 50f); // Tốc độ hạt từ 5 đến 50
            Debug.Log($"WaterEffect: Speed={speed}, Emission Rate={emission.rateOverTime}");
        }
    }
}
