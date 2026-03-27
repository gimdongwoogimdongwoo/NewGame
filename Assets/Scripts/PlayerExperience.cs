
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Player Experience")]
    [SerializeField] private int totalExp;

    [SerializeField] private int currentLevel = 1;

    [SerializeField] private int level = 1;

    [SerializeField] private int currentLevelExp;
    [SerializeField] private int needXpToNextLevel;

    public int TotalExp => totalExp;

    public int CurrentLevel => currentLevel;
    public int Level => currentLevel;

   

    public int CurrentLevelExp => currentLevelExp;
    public int NeedXpToNextLevel => needXpToNextLevel;

    public void SetExperienceState(int newTotalExp, int newLevel, int newCurrentLevelExp, int newNeedXpToNextLevel)
    {
        totalExp = Mathf.Max(0, newTotalExp);

        currentLevel = Mathf.Max(1, newLevel);
        currentLevelExp = Mathf.Max(0, newCurrentLevelExp);
        needXpToNextLevel = Mathf.Max(0, newNeedXpToNextLevel);

        level = Mathf.Max(1, newLevel);
    }
   



    
    struct LevelXpEntry
    {
        public int Level;
        public int NeedXp;
    }

    [Header("Experience")]
   
    [SerializeField] private int currentExp;
    [SerializeField] private int needExp = 10;
    [SerializeField] private float levelUpFullHoldSeconds = 0.12f;

    [Header("UI")]
    [SerializeField] private ExpBarController expBarController;
    [SerializeField] private TMP_Text hudLevelTMP;
    [SerializeField] private Text hudLevelText;

    private readonly List<LevelXpEntry> levelXpTable = new();
    private Coroutine gainRoutine;


    public int CurrentExp => currentExp;
    public int NeedExp => needExp;

    private void Awake()
    {
        ResolveUiReferences();
        LoadLevelXpTable();

        currentLevel = Mathf.Max(1, currentLevel);
        currentExp = Mathf.Max(0, currentExp);
        needExp = Mathf.Max(1, ResolveNeedExp(currentLevel));

        RefreshLevelText();
        UpdateExpBarImmediate();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (gainRoutine != null)
        {
            StopCoroutine(gainRoutine);
        }

        gainRoutine = StartCoroutine(AddExperienceRoutine(amount));
    }

    private IEnumerator AddExperienceRoutine(int amount)
    {
        currentExp += amount;

        while (needExp > 0 && currentExp >= needExp)
        {
            yield return expBarController != null
                ? expBarController.AnimateToRatio(1f)
                : null;

            if (levelUpFullHoldSeconds > 0f)
            {
                yield return new WaitForSeconds(levelUpFullHoldSeconds);
            }

            currentExp -= needExp;
            currentLevel += 1;
            needExp = ResolveNeedExp(currentLevel);

            RefreshLevelText();

            if (expBarController != null)
            {
                expBarController.SetRatioImmediate(0f);
            }

            if (needExp <= 0)
            {
                currentExp = 0;
                if (expBarController != null)
                {
                    expBarController.SetRatioImmediate(1f);
                }

                gainRoutine = null;
                yield break;
            }

            if (currentExp > 0 && expBarController != null)
            {
                float carryRatio = Mathf.Clamp01((float)currentExp / needExp);
                expBarController.SetRatioImmediate(carryRatio);
            }
        }

        if (needExp > 0 && expBarController != null)
        {
            float xpRatio = Mathf.Clamp01((float)currentExp / needExp);
            yield return expBarController.AnimateToRatio(xpRatio);
        }

        gainRoutine = null;
    }

    private void RefreshLevelText()
    {
        string label = $"Level: {currentLevel}";

        if (hudLevelTMP != null)
        {
            hudLevelTMP.text = label;
        }

        if (hudLevelText != null)
        {
            hudLevelText.text = label;
        }
    }

    private void UpdateExpBarImmediate()
    {
        if (expBarController == null)
        {
            return;
        }

        float xpRatio = needExp > 0 ? Mathf.Clamp01((float)currentExp / needExp) : 1f;
        expBarController.SetRatioImmediate(xpRatio);
    }

    private void ResolveUiReferences()
    {
        if (expBarController == null)
        {
            GameObject expBarObject = GameObject.Find("ExpBar_Fill");
            if (expBarObject != null)
            {
                expBarController = expBarObject.GetComponent<ExpBarController>();
            }
        }

        if (hudLevelTMP == null || hudLevelText == null)
        {
            GameObject levelObject = GameObject.Find("HUD_LevelText");
            if (levelObject != null)
            {
                if (hudLevelTMP == null)
                {
                    hudLevelTMP = levelObject.GetComponent<TMP_Text>();
                }

                if (hudLevelText == null)
                {
                    hudLevelText = levelObject.GetComponent<Text>();
                }
            }
        }
    }

    private int ResolveNeedExp(int level)
    {
        for (int i = 0; i < levelXpTable.Count; i++)
        {
            if (levelXpTable[i].Level == level)
            {
                return levelXpTable[i].NeedXp;
            }
        }

        return 0;
    }

    private void LoadLevelXpTable()
    {
        levelXpTable.Clear();

        TextAsset csvAsset = Resources.Load<TextAsset>("LevelXP");
        if (csvAsset == null)
        {
            Debug.LogWarning("PlayerExperience: Resources/LevelXP.csv를 찾지 못해 기본값(10)을 사용합니다.");
            levelXpTable.Add(new LevelXpEntry { Level = 1, NeedXp = 10 });
            return;
        }

        string[] lines = csvAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(columns[0].Trim(), out int level))
            {
                continue;
            }

            if (!int.TryParse(columns[1].Trim(), out int requiredXp))
            {
                continue;
            }

            if (level <= 0 || requiredXp <= 0)
            {
                continue;
            }

            levelXpTable.Add(new LevelXpEntry
            {
                Level = level,
                NeedXp = requiredXp
            });
        }

        levelXpTable.Sort((a, b) => a.Level.CompareTo(b.Level));

        if (levelXpTable.Count == 0)
        {
            levelXpTable.Add(new LevelXpEntry { Level = 1, NeedXp = 10 });
        }



    }
}
