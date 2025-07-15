using UnityEngine;

public class Coin : MonoBehaviour
{
    public string coinID; // Gán ID khác nhau cho mỗi coin trong inspector

    void Start()
    {
        if (PlayerPrefs.GetInt("CoinCollected_" + coinID, 0) == 1)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.instance.AddCoin(1);
            PlayerPrefs.SetInt("CoinCollected_" + coinID, 1);
            PlayerPrefs.Save();
            Destroy(gameObject);
        }
    }
}
