using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinController : MonoBehaviour
{
    [Header("Coin Bag")]
    [SerializeField] private int coinValue = 10;

    public int CoinValue
    {
        get => coinValue;
        set => coinValue = Mathf.Max(0, value);
    }

    private void OnValidate()
    {
        coinValue = Mathf.Max(0, coinValue);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollect(collision.gameObject);
    }

    private void TryCollect(GameObject other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        if (coinValue > 0)
        {
            CoinManager.Instance.AddCoins(coinValue);
        }

        Destroy(gameObject);
    }
}
