using UnityEngine;
using UnityEngine.UI;
using TMPro; // Nếu dùng TextMeshPro

public class UpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text currentHealthText;
    public TMP_Text currentDamageText;
    public TMP_Text totalGoldText;
    public Button healthUpgradeButton;
    public Button damageUpgradeButton;

    [Header("Upgrade Settings")]
    public int maxUpgrades = 3;
    public int healthUpgradeCost = 20;
    public int damageUpgradeCost = 25;
    public int healthPerUpgrade = 20;
    public int damagePerUpgrade = 5;

    [Header("Runtime Values (editable if useSavedData = false)")]
    [SerializeField] private int currentHealthLevel = 0;
    [SerializeField] private int currentDamageLevel = 0;

    [Header("Testing Options")]
    public bool useSavedData = true;

    public GameObject upgradeCanvas;
    public GameObject stageClearedCanvas;

    public void OnBackButtonPressed()
    {
        upgradeCanvas.SetActive(false);         
        stageClearedCanvas.SetActive(true);   
    }

    void Start()
    {
        if (useSavedData)
        {
            currentHealthLevel = PlayerPrefs.GetInt("HealthUpgrades", 0);
            currentDamageLevel = PlayerPrefs.GetInt("DamageUpgrades", 0);
        }

        CoinManager.instance.onCoinChanged.AddListener(UpdateUI); // 👈 Theo dõi coin thay đổi
        UpdateUI();
    }

    public void UpgradeHealth()
    {
        if (currentHealthLevel >= maxUpgrades) return;

        if (CoinManager.instance.totalCoins < healthUpgradeCost)
        {
            Debug.Log("Không đủ xu để nâng cấp máu!");
            return;
        }

        // Trừ xu
        CoinManager.instance.AddCoin(-healthUpgradeCost);

        // Cập nhật cấp
        currentHealthLevel++;

        if (useSavedData)
        {
            PlayerPrefs.SetInt("HealthUpgrades", currentHealthLevel);
        }

        UpdateUI();
    }

    public void UpgradeDamage()
    {
        if (currentDamageLevel >= maxUpgrades) return;

        if (CoinManager.instance.totalCoins < damageUpgradeCost)
        {
            Debug.Log("Không đủ xu để nâng cấp sát thương!");
            return;
        }

        // Trừ xu
        CoinManager.instance.AddCoin(-damageUpgradeCost);

        // Cập nhật cấp
        currentDamageLevel++;

        if (useSavedData)
        {
            PlayerPrefs.SetInt("DamageUpgrades", currentDamageLevel);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        currentHealthText.text = $"{currentHealthLevel}/{maxUpgrades}";
        currentDamageText.text = $"{currentDamageLevel}/{maxUpgrades}";
        totalGoldText.text = "Total coins: " + CoinManager.instance.totalCoins + "g";

        healthUpgradeButton.interactable = currentHealthLevel < maxUpgrades &&
                                           CoinManager.instance.totalCoins >= healthUpgradeCost;

        damageUpgradeButton.interactable = currentDamageLevel < maxUpgrades &&
                                           CoinManager.instance.totalCoins >= damageUpgradeCost;
    }

    [ContextMenu("Reset All Upgrades (for Testing)")]
    public void ResetUpgrades()
    {
        currentHealthLevel = 0;
        currentDamageLevel = 0;

        if (useSavedData)
        {
            PlayerPrefs.DeleteKey("HealthUpgrades");
            PlayerPrefs.DeleteKey("DamageUpgrades");
        }

        // Reset xu (chỉ nếu test)
        CoinManager.instance.AddCoin(-CoinManager.instance.totalCoins);
        CoinManager.instance.AddCoin(100); // Gán lại 100 xu test

        UpdateUI();
    }
}
