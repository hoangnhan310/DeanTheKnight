using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    public int totalCoins = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void AddCoin(int amount)
    {
        totalCoins += amount;

        // LƯU tổng số xu sau khi thay đổi
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log("Số xu hiện tại (đã lưu): " + totalCoins);
    }


}
