using System;
using UnityEngine;

public interface ITotalCoinStorage
{
    bool TryLoad(out TotalCoinSaveData data);
    bool TrySave(TotalCoinSaveData data);
}

[Serializable]
public struct TotalCoinSaveData
{
    public int totalCoins;
    public string lastUpdatedUtc;
}

public sealed class PlayerPrefsTotalCoinStorage : ITotalCoinStorage
{
    private const string KeyCoins = "save.total_coins";
    private const string KeyLastUpdated = "save.total_coins_last_updated_utc";

    public bool TryLoad(out TotalCoinSaveData data)
    {
        data = default;

        try
        {
            if (!PlayerPrefs.HasKey(KeyCoins))
            {
                return false;
            }

            data.totalCoins = PlayerPrefs.GetInt(KeyCoins, 0);
            data.lastUpdatedUtc = PlayerPrefs.GetString(KeyLastUpdated, string.Empty);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TotalCoinStorage] Load failed: {ex.Message}");
            data = default;
            return false;
        }
    }

    public bool TrySave(TotalCoinSaveData data)
    {
        try
        {
            PlayerPrefs.SetInt(KeyCoins, data.totalCoins);
            PlayerPrefs.SetString(KeyLastUpdated, data.lastUpdatedUtc ?? string.Empty);
            PlayerPrefs.Save();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TotalCoinStorage] Save failed: {ex.Message}");
            return false;
        }
    }
}

public class TotalCoinPersistence : MonoBehaviour
{
    public const int MaxTotalCoins = 1_000_000_000;

    public static TotalCoinPersistence Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<TotalCoinPersistence>();
                if (instance == null)
                {
                    GameObject bootstrap = new GameObject(nameof(TotalCoinPersistence));
                    instance = bootstrap.AddComponent<TotalCoinPersistence>();
                }
            }

            return instance;
        }
    }

    public event Action<int, string> TotalCoinsChanged;

    private static TotalCoinPersistence instance;

    private ITotalCoinStorage storage;
    private TotalCoinSaveData current;

    public int TotalCoins => current.totalCoins;
    public string LastUpdatedUtc => current.lastUpdatedUtc;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        _ = Instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        storage = new PlayerPrefsTotalCoinStorage();
        LoadOrRecover();
    }

    public void CommitSessionCoins(int sessionCoins)
    {
        if (sessionCoins <= 0)
        {
            return;
        }

        int safeSessionCoins = ValidateCoinValue(sessionCoins);
        long merged = (long)current.totalCoins + safeSessionCoins;

        current.totalCoins = ValidateCoinValue(merged > int.MaxValue ? int.MaxValue : (int)merged);
        current.lastUpdatedUtc = DateTime.UtcNow.ToString("O");

        if (!storage.TrySave(current))
        {
            Debug.LogWarning("[TotalCoinPersistence] Commit save failed. Data kept in memory for this session.");
        }

        TotalCoinsChanged?.Invoke(current.totalCoins, current.lastUpdatedUtc);
    }


    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (current.totalCoins < amount)
        {
            return false;
        }

        current.totalCoins = ValidateCoinValue(current.totalCoins - amount);
        current.lastUpdatedUtc = DateTime.UtcNow.ToString("O");

        if (!storage.TrySave(current))
        {
            Debug.LogWarning("[TotalCoinPersistence] Spend save failed. Data kept in memory for this session.");
        }

        TotalCoinsChanged?.Invoke(current.totalCoins, current.lastUpdatedUtc);
        return true;
    }

    public void ResetToDefault()
    {
        current = DefaultData();

        if (!storage.TrySave(current))
        {
            Debug.LogWarning("[TotalCoinPersistence] Failed to persist default coin state.");
        }

        TotalCoinsChanged?.Invoke(current.totalCoins, current.lastUpdatedUtc);
    }

    public void AddCheatCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CommitSessionCoins(amount);
    }

    private void LoadOrRecover()
    {
        if (!storage.TryLoad(out TotalCoinSaveData loaded))
        {
            current = DefaultData();
            Debug.LogWarning("[TotalCoinPersistence] Save missing or unreadable. Recovered to default (0). ");
            return;
        }

        current.totalCoins = ValidateCoinValue(loaded.totalCoins);
        current.lastUpdatedUtc = string.IsNullOrWhiteSpace(loaded.lastUpdatedUtc)
            ? DateTime.UtcNow.ToString("O")
            : loaded.lastUpdatedUtc;

        if (!storage.TrySave(current))
        {
            Debug.LogWarning("[TotalCoinPersistence] Failed to rewrite normalized save data.");
        }

        TotalCoinsChanged?.Invoke(current.totalCoins, current.lastUpdatedUtc);
    }

    private static int ValidateCoinValue(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        return Mathf.Min(value, MaxTotalCoins);
    }

    private static TotalCoinSaveData DefaultData()
    {
        return new TotalCoinSaveData
        {
            totalCoins = 0,
            lastUpdatedUtc = DateTime.UtcNow.ToString("O")
        };
    }
}
