using UnityEngine;

public class CoinWallet : MonoBehaviour
{
    [SerializeField] private int coins;

    public int Coins => Mathf.Max(0, coins);

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coins = Mathf.Max(0, coins + amount);
    }
}
