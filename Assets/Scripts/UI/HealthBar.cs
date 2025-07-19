using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private float maxHealth = 100f;
    private float currentHealth = 100f;

    // Gọi từ PlayerHealth
    public void SetHealth(float current, float max)
    {
        currentHealth = Mathf.Clamp(current, 0, max);
        maxHealth = max;
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    // Editor hỗ trợ cập nhật thanh máu khi chỉnh số
    private void OnValidate()
    {
        UpdateHealthBar();
    }
}
