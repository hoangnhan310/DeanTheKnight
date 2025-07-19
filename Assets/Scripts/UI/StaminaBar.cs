using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 20f; // lượng stamina hồi mỗi giây
    [SerializeField] private float currentStamina = 100f;

    private void Start()
    {
        currentStamina = maxStamina;
        UpdateStaminaBar();
    }

    private void Update()
    {
        RegenerateStamina();
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaBar();
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            UpdateStaminaBar();
        }
    }

    private void UpdateStaminaBar()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentStamina / maxStamina;
        }
    }

    // Optional: cập nhật trên Editor khi chỉnh các giá trị
    private void OnValidate()
    {
        UpdateStaminaBar();
    }
}
