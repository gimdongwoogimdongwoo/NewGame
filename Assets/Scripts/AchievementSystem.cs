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
public struct AchievementDefinition
{
    public string Id;
    public string Title;
    public AchievementCondition Condition;
    public int Value;
    public int Count;
}

[Serializable]
public class AchievementSaveEntry
{
    public string id;
    public int progress;
    public bool completed;
}

[Serializable]
public class AchievementSaveData
{
    public List<AchievementSaveEntry> entries = new();
}

public class AchievementManager : MonoBehaviour
{
    private const string SaveKey = "save.achievements.v1";

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

    public event Action AchievementChanged;

    private static AchievementManager instance;
    private readonly List<AchievementDefinition> definitions = new();
    private readonly Dictionary<string, AchievementSaveEntry> states = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() => _ = Instance;

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
        BindRuntimeEvents();
    }

    private void OnDestroy()
    {
        UnbindRuntimeEvents();
    }

    public IReadOnlyList<AchievementDefinition> Definitions => definitions;

    public int GetCompletedCount()
    {
        int count = 0;
        for (int i = 0; i < definitions.Count; i++)
        {
            if (TryGetState(definitions[i].Id, out AchievementSaveEntry state) && state.completed)
            {
                count++;
            }
        }

        return count;
    }

    public bool IsCompleted(string id) => TryGetState(id, out AchievementSaveEntry state) && state.completed;

    public int GetProgress(string id) => TryGetState(id, out AchievementSaveEntry state) ? state.progress : 0;

    public void ResetAllAchievements()
    {
        states.Clear();
        for (int i = 0; i < definitions.Count; i++)
        {
            states[definitions[i].Id] = new AchievementSaveEntry { id = definitions[i].Id, progress = 0, completed = false };
        }

        SaveProgress();
        AchievementChanged?.Invoke();
    }

    public void RecordStageClear(int stageId) => UpdateByCondition(AchievementCondition.CLEAR, stageId, 1);
    public void RecordLevelReached(int level) => UpdateByCondition(AchievementCondition.LEVEL, level, 1);
    public void RecordCardSelected(int cardId) => UpdateByCondition(AchievementCondition.CARD, cardId, 1);
    public void RecordCoinEarned(int amount) => UpdateByCondition(AchievementCondition.COIN, 0, Mathf.Max(0, amount));
    public void RecordUpgradePurchased() => UpdateByCondition(AchievementCondition.UPGRADE, 0, 1);

    private void UpdateByCondition(AchievementCondition condition, int value, int amount)
    {
        bool changed = false;

        for (int i = 0; i < definitions.Count; i++)
        {
            AchievementDefinition def = definitions[i];
            if (def.Condition != condition)
            {
                continue;
            }

            if ((condition == AchievementCondition.CLEAR || condition == AchievementCondition.LEVEL || condition == AchievementCondition.CARD) && def.Value != value)
            {
                continue;
            }

            AchievementSaveEntry state = states[def.Id];
            if (state.completed)
            {
                continue;
            }

            switch (condition)
            {
                case AchievementCondition.CLEAR:
                case AchievementCondition.LEVEL:
                case AchievementCondition.CARD:
                    state.progress = Mathf.Max(1, def.Count);
                    state.completed = true;
                    changed = true;
                    break;

                case AchievementCondition.COIN:
                case AchievementCondition.UPGRADE:
                    state.progress = Mathf.Max(0, state.progress + amount);
                    if (state.progress >= Mathf.Max(1, def.Count))
                    {
                        state.completed = true;
                    }

                    changed = true;
                    break;
            }

            states[def.Id] = state;
        }

        if (!changed)
        {
            return;
        }

        SaveProgress();
        AchievementChanged?.Invoke();
    }

    private bool TryGetState(string id, out AchievementSaveEntry state)
    {
        if (states.TryGetValue(id, out state))
        {
            return true;
        }

        state = null;
        return false;
    }

    private void LoadDefinitions()
    {
        definitions.Clear();
        TextAsset csv = Resources.Load<TextAsset>("Achievement");
        if (csv == null)
        {
            return;
        }

        string[] lines = csv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }

            string[] cols = line.Split(',');
            if (cols.Length < 5)
            {
                continue;
            }

            if (!Enum.TryParse(cols[2].Trim(), true, out AchievementCondition condition))
            {
                continue;
            }

            int.TryParse(cols[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value);
            int.TryParse(cols[4].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int count);

            AchievementDefinition def = new AchievementDefinition
            {
                Id = cols[0].Trim(),
                Title = cols[1].Trim(),
                Condition = condition,
                Value = value,
                Count = Mathf.Max(1, count)
            };

            if (!string.IsNullOrWhiteSpace(def.Id))
            {
                definitions.Add(def);
            }
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            if (!states.ContainsKey(definitions[i].Id))
            {
                states[definitions[i].Id] = new AchievementSaveEntry { id = definitions[i].Id, progress = 0, completed = false };
            }
        }
    }

    private void LoadProgress()
    {
        string raw = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            AchievementSaveData loaded = JsonUtility.FromJson<AchievementSaveData>(raw);
            if (loaded?.entries != null)
            {
                for (int i = 0; i < loaded.entries.Count; i++)
                {
                    AchievementSaveEntry entry = loaded.entries[i];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !states.ContainsKey(entry.id))
                    {
                        continue;
                    }

                    states[entry.id] = entry;
                }
            }
        }

        SaveProgress();
    }

    private void SaveProgress()
    {
        AchievementSaveData data = new AchievementSaveData();
        foreach (KeyValuePair<string, AchievementSaveEntry> pair in states)
        {
            data.entries.Add(new AchievementSaveEntry
            {
                id = pair.Key,
                progress = Mathf.Max(0, pair.Value.progress),
                completed = pair.Value.completed
            });
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void BindRuntimeEvents()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(default, LoadSceneMode.Single);
    }

    private void UnbindRuntimeEvents()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.CoinsAdded -= RecordCoinEarned;
        }

        PlayerExperience exp = FindFirstObjectByType<PlayerExperience>();
        if (exp != null)
        {
            exp.LevelReached -= RecordLevelReached;
        }

        LevelUpPanelController panel = FindFirstObjectByType<LevelUpPanelController>();
        if (panel != null)
        {
            panel.CardSelected -= RecordCardSelected;
        }

        UpgradeSystem.Instance.UpgradePurchased -= HandleUpgradePurchased;
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.CoinsAdded -= RecordCoinEarned;
            CoinManager.Instance.CoinsAdded += RecordCoinEarned;
        }

        PlayerExperience exp = FindFirstObjectByType<PlayerExperience>();
        if (exp != null)
        {
            exp.LevelReached -= RecordLevelReached;
            exp.LevelReached += RecordLevelReached;
        }

        LevelUpPanelController panel = FindFirstObjectByType<LevelUpPanelController>();
        if (panel != null)
        {
            panel.CardSelected -= RecordCardSelected;
            panel.CardSelected += RecordCardSelected;
        }

        UpgradeSystem.Instance.UpgradePurchased -= HandleUpgradePurchased;
        UpgradeSystem.Instance.UpgradePurchased += HandleUpgradePurchased;
    }

    private void HandleUpgradePurchased(UpgradeStatType _, int __)
    {
        RecordUpgradePurchased();
    }
}

public class AchievementPopupUI : MonoBehaviour
{
    private TextMeshProUGUI rateText;
    private Image barFill;
    private RectTransform content;
    private readonly List<AchievementRowUI> rows = new();

    private void Awake()
    {
        EnsureStructure();
    }

    private void OnEnable()
    {
        AchievementManager.Instance.AchievementChanged += Refresh;
        EnsureRows();
        Refresh();
    }

    private void OnDisable()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.AchievementChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        int total = Mathf.Max(1, AchievementManager.Instance.Definitions.Count);
        int done = AchievementManager.Instance.GetCompletedCount();
        float ratio = Mathf.Clamp01(done / (float)total);

        if (rateText != null)
        {
            int percent = Mathf.RoundToInt(ratio * 100f);
            rateText.text = $"{percent}% ({done}/{total})";
        }

        if (barFill != null)
        {
            barFill.gameObject.SetActive(ratio > 0f);
            barFill.fillAmount = ratio;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].Refresh();
        }
    }

    private void EnsureRows()
    {
        if (rows.Count > 0)
        {
            return;
        }

        for (int i = 0; i < AchievementManager.Instance.Definitions.Count; i++)
        {
            AchievementDefinition def = AchievementManager.Instance.Definitions[i];
            rows.Add(AchievementRowUI.Create(content, def));
        }
    }

    private void EnsureStructure()
    {
        Transform progress = transform.Find("Progress");
        if (progress == null)
        {
            GameObject progressGo = new GameObject("Progress", typeof(RectTransform));
            progressGo.transform.SetParent(transform, false);
            progress = progressGo.transform;
        }

        Transform rate = progress.Find("TXT_Rate");
        if (rate == null)
        {
            GameObject rateGo = new GameObject("TXT_Rate", typeof(RectTransform), typeof(TextMeshProUGUI));
            rateGo.transform.SetParent(progress, false);
            rate = rateGo.transform;
        }

        rateText = rate.GetComponent<TextMeshProUGUI>();

        Transform fill = progress.Find("Bar_Fill");
        if (fill == null)
        {
            GameObject fillGo = new GameObject("Bar_Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(progress, false);
            fill = fillGo.transform;
        }

        barFill = fill.GetComponent<Image>();
        barFill.type = Image.Type.Filled;
        barFill.fillMethod = Image.FillMethod.Horizontal;

        Transform scrollContent = transform.Find("Scroll View/Viewport/Content");
        if (scrollContent == null)
        {
            GameObject scroll = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect));
            scroll.transform.SetParent(transform, false);
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewport.transform, false);
            scrollContent = contentGo.transform;

            ScrollRect scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentGo.GetComponent<RectTransform>();
            scrollRect.horizontal = false;

            ContentSizeFitter fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        content = scrollContent.GetComponent<RectTransform>();
    }
}

public class AchievementRowUI
{
    private readonly AchievementDefinition definition;
    private readonly TextMeshProUGUI label;
    private readonly GameObject check0;
    private readonly GameObject check1;

    private AchievementRowUI(AchievementDefinition definition, TextMeshProUGUI label, GameObject check0, GameObject check1)
    {
        this.definition = definition;
        this.label = label;
        this.check0 = check0;
        this.check1 = check1;
    }

    public static AchievementRowUI Create(Transform parent, AchievementDefinition definition)
    {
        GameObject row = new GameObject("Row_Achievement", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);

        GameObject labelGo = new GameObject("TXT_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(row.transform, false);
        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = definition.Title;

        GameObject check0 = new GameObject("Check_0", typeof(RectTransform), typeof(Image));
        check0.transform.SetParent(row.transform, false);
        check0.GetComponent<Image>().color = Color.gray;

        GameObject check1 = new GameObject("Check_1", typeof(RectTransform), typeof(Image));
        check1.transform.SetParent(row.transform, false);
        check1.GetComponent<Image>().color = Color.green;

        AchievementRowUI ui = new AchievementRowUI(definition, label, check0, check1);
        ui.Refresh();
        return ui;
    }

    public void Refresh()
    {
        bool completed = AchievementManager.Instance.IsCompleted(definition.Id);
        if (label != null)
        {
            label.text = definition.Title;
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
