using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum UpgradeStatType
{
    ATK,
    HP,
    MoveSpeed,
    ATKSpeed,
    CRI,
    Revival
}

[Serializable]
public struct UpgradeStatusRow
{
    public string Id;
    public UpgradeStatType Stat;
    public string StatName;
    public int Level;
    public float StatValue;
    public int CoinValue;
}

public static class UpgradeStatusCsvLoader
{
    private const string ResourcePath = "UpgradeStatus";

    public static List<UpgradeStatusRow> LoadRows()
    {
        List<UpgradeStatusRow> result = new();
        TextAsset csv = Resources.Load<TextAsset>(ResourcePath);
        if (csv == null)
        {
            Debug.LogWarning($"[UpgradeStatusCsvLoader] {ResourcePath}.csv not found in Resources.");
            return result;
        }

        string[] lines = csv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            return result;
        }

        Dictionary<string, int> header = BuildHeader(lines[0]);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }

            string[] cols = line.Split(',');
            if (!TryParseRow(cols, header, out UpgradeStatusRow row))
            {
                Debug.LogWarning($"[UpgradeStatusCsvLoader] Invalid row: {line}");
                continue;
            }

            result.Add(row);
        }

        return result;
    }

    private static bool TryParseRow(string[] cols, Dictionary<string, int> header, out UpgradeStatusRow row)
    {
        row = default;

        string id = Read(cols, header, "ID", 0);
        string statRaw = Read(cols, header, "Stat", 1);
        string statName = Read(cols, header, "StatName", 2);

        if (!Enum.TryParse(statRaw, true, out UpgradeStatType stat))
        {
            return false;
        }

        // 1) Preferred format: ID,Stat,StatName,Level,StatValue,CoinValue
        // 2) Legacy format support: ID,Stat,StatName,StatValue,CoinValue[, ...]
        bool parsedLevel = int.TryParse(Read(cols, header, "Level", 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out int level);
        bool parsedStatValue = float.TryParse(Read(cols, header, "StatValue", parsedLevel ? 4 : 3), NumberStyles.Float, CultureInfo.InvariantCulture, out float statValue);
        bool parsedCoinValue = int.TryParse(Read(cols, header, "CoinValue", parsedLevel ? 5 : 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out int coinValue);

        if (!parsedStatValue || !parsedCoinValue)
        {
            return false;
        }

        if (!parsedLevel)
        {
            level = InferLevelFromId(id);
        }

        row = new UpgradeStatusRow
        {
            Id = id,
            Stat = stat,
            StatName = string.IsNullOrWhiteSpace(statName) ? stat.ToString() : statName,
            Level = Mathf.Clamp(level, 0, 5),
            StatValue = statValue,
            CoinValue = Mathf.Max(0, coinValue)
        };

        return true;
    }

    private static int InferLevelFromId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return 0;
        }

        int underscore = id.LastIndexOf('_');
        if (underscore < 0 || underscore >= id.Length - 1)
        {
            return 0;
        }

        string suffix = id.Substring(underscore + 1);
        return int.TryParse(suffix, NumberStyles.Integer, CultureInfo.InvariantCulture, out int level)
            ? Mathf.Clamp(level, 0, 5)
            : 0;
    }

    private static Dictionary<string, int> BuildHeader(string line)
    {
        Dictionary<string, int> map = new(StringComparer.OrdinalIgnoreCase);
        string[] cols = line.Split(',');
        for (int i = 0; i < cols.Length; i++)
        {
            string key = cols[i].Trim();
            if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
            {
                map.Add(key, i);
            }
        }

        return map;
    }

    private static string Read(string[] cols, Dictionary<string, int> header, string key, int fallback)
    {
        int index = header != null && header.TryGetValue(key, out int idx) ? idx : fallback;
        return index >= 0 && index < cols.Length ? cols[index].Trim() : string.Empty;
    }
}

[Serializable]
public class UpgradeProgressSave
{
    public List<UpgradeStatLevelSave> levels = new();
}

[Serializable]
public class UpgradeStatLevelSave
{
    public string stat;
    public int level;
}

public class UpgradeSystem : MonoBehaviour
{
    private const string SaveKey = "save.upgrade_progress.v1";

    public static UpgradeSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UpgradeSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(UpgradeSystem));
                    instance = go.AddComponent<UpgradeSystem>();
                }
            }

            return instance;
        }
    }

    public event Action UpgradesChanged;
    public event Action<UpgradeStatType, int> UpgradePurchased;

    private static UpgradeSystem instance;

    private readonly Dictionary<UpgradeStatType, List<UpgradeStatusRow>> rowsByStat = new();
    private readonly Dictionary<UpgradeStatType, int> currentLevels = new();

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

        LoadDefinitions();
        LoadProgress();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public IEnumerable<UpgradeStatType> GetAllStats()
    {
        return rowsByStat.Keys;
    }

    public string GetStatName(UpgradeStatType stat)
    {
        if (!rowsByStat.TryGetValue(stat, out List<UpgradeStatusRow> rows) || rows.Count == 0)
        {
            return stat.ToString();
        }

        return rows[0].StatName;
    }

    public int GetCurrentLevel(UpgradeStatType stat)
    {
        return currentLevels.TryGetValue(stat, out int level) ? level : 0;
    }

    public int GetMaxLevel(UpgradeStatType stat)
    {
        if (!rowsByStat.TryGetValue(stat, out List<UpgradeStatusRow> rows) || rows.Count == 0)
        {
            return 0;
        }

        return rows[rows.Count - 1].Level;
    }

    public float GetCurrentStatValue(UpgradeStatType stat)
    {
        return GetRow(stat, GetCurrentLevel(stat)).StatValue;
    }

    public int GetNextCoinCost(UpgradeStatType stat)
    {
        int nextLevel = GetCurrentLevel(stat) + 1;
        UpgradeStatusRow next = GetRow(stat, nextLevel);
        return next.Level == nextLevel ? next.CoinValue : 0;
    }

    public bool IsMaxLevel(UpgradeStatType stat)
    {
        return GetCurrentLevel(stat) >= GetMaxLevel(stat);
    }

    public bool TryUpgrade(UpgradeStatType stat)
    {
        if (IsMaxLevel(stat))
        {
            return false;
        }

        int cost = GetNextCoinCost(stat);
        if (!TotalCoinPersistence.Instance.TrySpendCoins(cost))
        {
            return false;
        }

        currentLevels[stat] = Mathf.Clamp(GetCurrentLevel(stat) + 1, 0, GetMaxLevel(stat));
        UpgradePurchased?.Invoke(stat, currentLevels[stat]);
        SaveProgress();
        ApplyToCurrentPlayer();
        UpgradesChanged?.Invoke();
        return true;
    }

    public void ResetAllLevels()
    {
        List<UpgradeStatType> keys = currentLevels.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            currentLevels[keys[i]] = 0;
        }

        SaveProgress();
        ApplyToCurrentPlayer();
        UpgradesChanged?.Invoke();
    }

    public void ApplyToCurrentPlayer()
    {
        PlayerStatus status = FindFirstObjectByType<PlayerStatus>();
        if (status == null)
        {
            return;
        }

        status.SetBaseAttack(GetCurrentStatValue(UpgradeStatType.ATK));
        status.SetBaseMaxHP(GetCurrentStatValue(UpgradeStatType.HP));
        status.SetBaseMoveSpeed(GetCurrentStatValue(UpgradeStatType.MoveSpeed));
        status.SetBaseAttackInterval(GetCurrentStatValue(UpgradeStatType.ATKSpeed));
        status.SetCriticalDamagePercent(GetCurrentStatValue(UpgradeStatType.CRI));
        status.SetBaseRevivalCount(Mathf.RoundToInt(GetCurrentStatValue(UpgradeStatType.Revival)));

        PlayerMovement2D movement = status.GetComponent<PlayerMovement2D>();
        if (movement != null)
        {
            movement.SetMoveSpeed(status.CurrentMoveSpeed);
        }

        AutoShooter shooter = status.GetComponent<AutoShooter>();
        if (shooter != null)
        {
            shooter.SetFireInterval(status.CurrentAttackInterval);
        }

        PlayerHealth health = status.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.SyncFromStatus();
        }
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        ApplyToCurrentPlayer();
    }

    private UpgradeStatusRow GetRow(UpgradeStatType stat, int level)
    {
        if (!rowsByStat.TryGetValue(stat, out List<UpgradeStatusRow> rows) || rows.Count == 0)
        {
            return default;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].Level == level)
            {
                return rows[i];
            }
        }

        return rows[rows.Count - 1];
    }

    private void LoadDefinitions()
    {
        rowsByStat.Clear();
        List<UpgradeStatusRow> rows = UpgradeStatusCsvLoader.LoadRows();

        for (int i = 0; i < rows.Count; i++)
        {
            UpgradeStatusRow row = rows[i];
            if (!rowsByStat.TryGetValue(row.Stat, out List<UpgradeStatusRow> list))
            {
                list = new List<UpgradeStatusRow>();
                rowsByStat[row.Stat] = list;
            }

            list.Add(row);
        }

        foreach (UpgradeStatType stat in Enum.GetValues(typeof(UpgradeStatType)))
        {
            if (!rowsByStat.ContainsKey(stat))
            {
                rowsByStat[stat] = new List<UpgradeStatusRow>
                {
                    new UpgradeStatusRow
                    {
                        Id = stat.ToString(),
                        Stat = stat,
                        StatName = stat.ToString(),
                        Level = 0,
                        StatValue = 0f,
                        CoinValue = 0
                    }
                };
            }

            rowsByStat[stat].Sort((a, b) => a.Level.CompareTo(b.Level));
            currentLevels[stat] = 0;
        }
    }

    private void LoadProgress()
    {
        string raw = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        try
        {
            UpgradeProgressSave save = JsonUtility.FromJson<UpgradeProgressSave>(raw);
            if (save?.levels == null)
            {
                return;
            }

            for (int i = 0; i < save.levels.Count; i++)
            {
                UpgradeStatLevelSave item = save.levels[i];
                if (!Enum.TryParse(item.stat, true, out UpgradeStatType stat))
                {
                    continue;
                }

                currentLevels[stat] = Mathf.Clamp(item.level, 0, GetMaxLevel(stat));
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UpgradeSystem] Failed to load save. Reset to default. {ex.Message}");
            ResetAllLevels();
        }
    }

    private void SaveProgress()
    {
        UpgradeProgressSave save = new UpgradeProgressSave();
        foreach (KeyValuePair<UpgradeStatType, int> pair in currentLevels)
        {
            save.levels.Add(new UpgradeStatLevelSave
            {
                stat = pair.Key.ToString(),
                level = Mathf.Clamp(pair.Value, 0, GetMaxLevel(pair.Key))
            });
        }

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }
}
