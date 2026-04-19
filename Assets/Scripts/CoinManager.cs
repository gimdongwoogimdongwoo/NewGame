using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CoinManager>();
                if (instance == null)
                {
                    GameObject bootstrap = new GameObject("CoinManager");
                    instance = bootstrap.AddComponent<CoinManager>();
                }
            }

            return instance;
        }
    }

    public event Action<int> CoinChanged;
    public event Action<int> CoinsAdded;

    [SerializeField] private int currentCoins;

    private static CoinManager instance;

    public int CurrentCoins => Mathf.Max(0, currentCoins);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentCoins = Mathf.Max(0, currentCoins + amount);
        CoinsAdded?.Invoke(amount);
        CoinChanged?.Invoke(CurrentCoins);
    }

    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Max(0, amount);
        CoinChanged?.Invoke(CurrentCoins);
    }
}
