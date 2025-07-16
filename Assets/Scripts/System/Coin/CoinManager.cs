using UnityEngine;
using UnityEngine.Events;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    public int totalCoins = 0;

    public UnityEvent onCoinChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

            if (onCoinChanged == null)
                onCoinChanged = new UnityEvent();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoin(int amount)
    {
        totalCoins += amount;

        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log("Xu hiện tại: " + totalCoins);

        onCoinChanged.Invoke(); // 🔥 Gọi sự kiện
    }
}
