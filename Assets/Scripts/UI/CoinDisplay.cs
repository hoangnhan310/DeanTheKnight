using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    void Start()
    {
        UpdateCoinText();
    }

    void Update()
    {
        UpdateCoinText(); // cập nhật mỗi frame (hoặc tối ưu hơn: chỉ khi có thay đổi)
    }

    void UpdateCoinText()
    {
        if (coinText != null)
        {
            int coins = CoinManager.instance.totalCoins;
            coinText.text = "Gold: " + coins.ToString() + "g";
        }
    }
}
