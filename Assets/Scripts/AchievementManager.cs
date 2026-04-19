using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AchievementCondition
{
    CLEAR,
    LEVEL,
    CARD,
    COIN,
    UPGRADE
}

[Serializable]
public sealed class AchievementCsvRow
{
    public string Id;
    public string Title;
    public AchievementCondition Condition;
    public int Value;
    public int Count;
}

[Serializable]
public sealed class AchievementProgressItem
{
    public string id;
    public bool completed;
    public int progress;
}

[Serializable]
public sealed class AchievementProgressSave
{
    public List<AchievementProgressItem> items = new();
}

public class AchievementManager : MonoBehaviour
{
    private const string CsvResourcePath = "Achievement";
    private const string SaveKey = "save.achievement_progress.v1";

    public static AchievementManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AchievementManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(AchievementManager));
                    instance = go.AddComponent<AchievementManager>();
                }
            }

            return instance;
        }
    }

    public static event Action<AchievementCompletedEvent> AchievementCompleted;
    public static event Action AchievementProgressReset;
    public event Action ProgressChanged;

    private static AchievementManager instance;

    private readonly List<AchievementCsvRow> definitions = new();
    private readonly Dictionary<string, AchievementRuntimeState> statesById = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<AchievementRowUiRef> rowUiRefs = new();

    private RectTransform popupRoot;
    private TMP_Text txtRate;
    private Image barFill;
    private RectTransform content;
    private GameObject rowTemplate;
    private long completionSerial;

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

        SubscribeGameplayEvents();
    }

    private void Start()
    {
        RefreshAchievementUi();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnsubscribeGameplayEvents();
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }

    public void SaveProgress()
    {
        AchievementProgressSave save = new AchievementProgressSave();

        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementCsvRow def = definitions[i];
            AchievementRuntimeState state = GetOrCreateState(def.Id);

            save.items.Add(new AchievementProgressItem
            {
                id = def.Id,
                completed = state.Completed,
                progress = Mathf.Max(0, state.Progress)
            });
        }

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void ResetAllProgress()
    {
        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementRuntimeState state = GetOrCreateState(definitions[i].Id);
            state.Completed = false;
            state.Progress = 0;
        }

        SaveProgress();
        RefreshAchievementUi();
        completionSerial = 0;
        AchievementProgressReset?.Invoke();
        ProgressChanged?.Invoke();
    }

    public int GetCompletedCount()
    {
        int completed = 0;
        for (int i = 0; i < definitions.Count; i++)
        {
            if (GetOrCreateState(definitions[i].Id).Completed)
            {
                completed++;
            }
        }

        return completed;
    }

    public int GetTotalCount() => definitions.Count;

    public IReadOnlyList<AchievementCsvRow> GetDefinitions() => definitions;

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        RefreshAchievementUi();
    }

    private void SubscribeGameplayEvents()
    {
        PlayerExperience.LevelReached -= HandleLevelReached;
        PlayerExperience.LevelReached += HandleLevelReached;

        LevelUpPanelController.CardSelected -= HandleCardSelected;
        LevelUpPanelController.CardSelected += HandleCardSelected;

        CoinManager.CoinsAdded -= HandleCoinAdded;
        CoinManager.CoinsAdded += HandleCoinAdded;

        UpgradeSystem.UpgradePurchased -= HandleUpgradePurchased;
        UpgradeSystem.UpgradePurchased += HandleUpgradePurchased;
    }

    private void UnsubscribeGameplayEvents()
    {
        PlayerExperience.LevelReached -= HandleLevelReached;
        LevelUpPanelController.CardSelected -= HandleCardSelected;
        CoinManager.CoinsAdded -= HandleCoinAdded;
        UpgradeSystem.UpgradePurchased -= HandleUpgradePurchased;
    }

    private void HandleStageCleared(int stageId)
    {
        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementCsvRow def = definitions[i];
            if (def.Condition != AchievementCondition.CLEAR || def.Value != stageId)
            {
                continue;
            }

            CompleteOnce(i, def);
        }
    }

    private void HandleLevelReached(int level)
    {
        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementCsvRow def = definitions[i];
            if (def.Condition != AchievementCondition.LEVEL)
            {
                continue;
            }

            if (level >= Mathf.Max(1, def.Value))
            {
                CompleteOnce(i, def);
            }
        }
    }

    private void HandleCardSelected(int cardId)
    {
        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementCsvRow def = definitions[i];
            if (def.Condition != AchievementCondition.CARD || def.Value != cardId)
            {
                continue;
            }

            CompleteOnce(i, def);
        }
    }

    private void HandleCoinAdded(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementCsvRow def = definitions[i];
            if (def.Condition != AchievementCondition.COIN)
            {
                continue;
            }

            IncreaseProgress(i, def, amount, Mathf.Max(1, def.Count));
        }
    }

    private void HandleUpgradePurchased(UpgradeStatType _, int __)
    {
        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementCsvRow def = definitions[i];
            if (def.Condition != AchievementCondition.UPGRADE)
            {
                continue;
            }

            IncreaseProgress(i, def, 1, Mathf.Max(1, def.Count));
        }
    }

    private void IncreaseProgress(int index, AchievementCsvRow def, int delta, int target)
    {
        AchievementRuntimeState state = GetOrCreateState(def.Id);
        if (state.Completed)
        {
            return;
        }

        state.Progress = Mathf.Max(0, state.Progress + Mathf.Max(0, delta));
        if (state.Progress >= target)
        {
            state.Progress = target;
            state.Completed = true;
            EmitCompleted(index, def);
        }

        SaveProgress();
        RefreshAchievementUi();
        ProgressChanged?.Invoke();
    }

    private void CompleteOnce(int index, AchievementCsvRow def)
    {
        AchievementRuntimeState state = GetOrCreateState(def.Id);
        if (state.Completed)
        {
            return;
        }

        state.Completed = true;
        state.Progress = Mathf.Max(1, state.Progress);
        EmitCompleted(index, def);

        SaveProgress();
        RefreshAchievementUi();
        ProgressChanged?.Invoke();
    }

    private void LoadDefinitions()
    {
        definitions.Clear();

        TextAsset csv = Resources.Load<TextAsset>(CsvResourcePath);
        if (csv == null)
        {
            Debug.LogWarning($"[AchievementManager] {CsvResourcePath}.csv not found in Resources.");
            return;
        }

        string[] lines = csv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            return;
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
            if (!TryParseDefinition(cols, header, out AchievementCsvRow row))
            {
                Debug.LogWarning($"[AchievementManager] Invalid row: {line}");
                continue;
            }

            definitions.Add(row);
        }
    }

    private bool TryParseDefinition(string[] cols, Dictionary<string, int> header, out AchievementCsvRow row)
    {
        row = null;

        string id = Read(cols, header, "ID", 0);
        string title = Read(cols, header, "Title", 1);
        string conditionRaw = Read(cols, header, "Condition", 2);

        if (string.IsNullOrWhiteSpace(id) || !Enum.TryParse(conditionRaw, true, out AchievementCondition condition))
        {
            return false;
        }

        int.TryParse(Read(cols, header, "Value", 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value);
        int.TryParse(Read(cols, header, "Count", 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out int count);

        row = new AchievementCsvRow
        {
            Id = id,
            Title = string.IsNullOrWhiteSpace(title) ? id : title,
            Condition = condition,
            Value = value,
            Count = count
        };

        return true;
    }

    private void LoadProgress()
    {
        statesById.Clear();

        string raw = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        try
        {
            AchievementProgressSave save = JsonUtility.FromJson<AchievementProgressSave>(raw);
            if (save?.items == null)
            {
                return;
            }

            for (int i = 0; i < save.items.Count; i++)
            {
                AchievementProgressItem item = save.items[i];
                if (string.IsNullOrWhiteSpace(item.id))
                {
                    continue;
                }

                statesById[item.id] = new AchievementRuntimeState
                {
                    Completed = item.completed,
                    Progress = Mathf.Max(0, item.progress)
                };
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AchievementManager] Failed to load progress: {ex.Message}");
            statesById.Clear();
        }
    }

    private void RefreshAchievementUi()
    {
        EnsurePopupRefs();
        if (popupRoot == null || content == null || rowTemplate == null)
        {
            return;
        }

        EnsureRowsBuilt();

        int total = Mathf.Max(0, definitions.Count);
        int completed = GetCompletedCount();
        float ratio = total <= 0 ? 0f : Mathf.Clamp01((float)completed / total);
        int roundedPercent = Mathf.RoundToInt(ratio * 100f);

        if (txtRate != null)
        {
            txtRate.text = $"{roundedPercent}%({completed}/{total})";
        }

        if (barFill != null)
        {
            bool showFill = ratio > 0f;
            if (barFill.gameObject.activeSelf != showFill)
            {
                barFill.gameObject.SetActive(showFill);
            }

            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillAmount = ratio;
        }

        for (int i = 0; i < rowUiRefs.Count && i < definitions.Count; i++)
        {
            AchievementCsvRow def = definitions[i];
            AchievementRuntimeState state = GetOrCreateState(def.Id);
            rowUiRefs[i].Apply(def.Title, state.Completed);
        }
    }

    private void EnsureRowsBuilt()
    {
        if (rowUiRefs.Count == definitions.Count && rowUiRefs.Count > 0)
        {
            return;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Transform child = content.GetChild(i);
            if (child == rowTemplate.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        rowUiRefs.Clear();

        rowTemplate.SetActive(false);

        for (int i = 0; i < definitions.Count; i++)
        {
            GameObject rowGo = Instantiate(rowTemplate, content);
            rowGo.name = $"Row_Achievement_{i + 1}";
            rowGo.SetActive(true);

            AchievementRowUiRef uiRef = AchievementRowUiRef.Build(rowGo.transform);
            rowUiRefs.Add(uiRef);
        }
    }

    private void EnsurePopupRefs()
    {
        if (popupRoot == null)
        {
            GameObject popupGo = GameObject.Find("Popup_Achivement");
            if (popupGo == null)
            {
                popupGo = GameObject.Find("Popup_Achievement");
            }

            popupRoot = popupGo != null ? popupGo.GetComponent<RectTransform>() : null;
        }

        if (popupRoot == null)
        {
            return;
        }

        if (txtRate == null)
        {
            Transform t = popupRoot.Find("Progress/TXT_Rate");
            txtRate = t != null ? t.GetComponent<TMP_Text>() : null;
        }

        if (barFill == null)
        {
            Transform t = popupRoot.Find("Progress/Bar_Fill");
            if (t == null)
            {
                t = popupRoot.Find("Progress/Bar");
            }

            barFill = t != null ? t.GetComponent<Image>() : null;
        }

        if (content == null)
        {
            Transform t = popupRoot.Find("Scroll View/Viewport/Content");
            content = t != null ? t.GetComponent<RectTransform>() : null;
        }

        if (rowTemplate == null && content != null)
        {
            Transform row = content.Find("Row_Achievement");
            if (row != null)
            {
                rowTemplate = row.gameObject;
            }
            else if (content.childCount > 0)
            {
                rowTemplate = content.GetChild(0).gameObject;
            }
        }
    }

    private AchievementRuntimeState GetOrCreateState(string id)
    {
        if (!statesById.TryGetValue(id, out AchievementRuntimeState state))
        {
            state = new AchievementRuntimeState();
            statesById[id] = state;
        }

        return state;
    }

    private static Dictionary<string, int> BuildHeader(string headerLine)
    {
        Dictionary<string, int> map = new(StringComparer.OrdinalIgnoreCase);
        string[] cols = headerLine.Split(',');
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
        int index = header != null && header.TryGetValue(key, out int found) ? found : fallback;
        return index >= 0 && index < cols.Length ? cols[index].Trim() : string.Empty;
    }

    public static void NotifyStageClear(int stageId)
    {
        Instance.HandleStageCleared(stageId);
    }

    private void EmitCompleted(int index, AchievementCsvRow def)
    {
        completionSerial++;
        AchievementCompleted?.Invoke(new AchievementCompletedEvent
        {
            Id = def.Id,
            Title = def.Title,
            RowIndex = Mathf.Max(0, index),
            CompletionSerial = completionSerial
        });
    }

    private sealed class AchievementRuntimeState
    {
        public bool Completed;
        public int Progress;
    }

    private sealed class AchievementRowUiRef
    {
        private readonly TMP_Text label;
        private readonly GameObject check0;
        private readonly GameObject check1;

        private AchievementRowUiRef(TMP_Text label, GameObject check0, GameObject check1)
        {
            this.label = label;
            this.check0 = check0;
            this.check1 = check1;
        }

        public static AchievementRowUiRef Build(Transform root)
        {
            TMP_Text label = null;
            Transform labelTr = root.Find("TXT_Label");
            if (labelTr != null)
            {
                label = labelTr.GetComponent<TMP_Text>();
            }

            if (label == null)
            {
                label = root.GetComponentInChildren<TMP_Text>(true);
            }

            Transform check0Tr = root.Find("Check_0");
            Transform check1Tr = root.Find("Check_1");

            return new AchievementRowUiRef(
                label,
                check0Tr != null ? check0Tr.gameObject : null,
                check1Tr != null ? check1Tr.gameObject : null);
        }

        public void Apply(string title, bool completed)
        {
            if (label != null)
            {
                label.text = title;
            }

            if (check0 != null)
            {
                check0.SetActive(!completed);
            }

            if (check1 != null)
            {
                check1.SetActive(completed);
            }
        }
    }
}

public sealed class AchievementCompletedEvent
{
    public string Id;
    public string Title;
    public int RowIndex;
    public long CompletionSerial;
}
