using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExpDropManager : MonoBehaviour
{
    [Serializable]
    public class ExperienceChangedEvent : UnityEvent<float> { }


    [System.Serializable]
    public class LevelChangedEvent : UnityEvent<int, int, int> { }

    [System.Serializable]








    public class LevelXpEntry
    {
        public int Level;
        public int NeedXP;
    }

    public static ExpDropManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Object playerExperienceReference;
    [SerializeField] private PlayerExperience playerExperience;

    [SerializeField] private GameObject playerExperienceObject;

    [SerializeField] private PlayerExperience playerExperience;

    [Header("Magnet")]
    [Tooltip("구슬이 플레이어를 향해 이동하기 시작하는 거리")]
    [SerializeField] private float magnetRanage = 3f;
    [SerializeField] private float absorbDistance = 0.1f;
    [SerializeField] private float magnetSpeed = 8f;

    [Header("Experience")]

    [SerializeField] private ExperienceChangedEvent onExperienceChanged = new ExperienceChangedEvent();
    [SerializeField] private LevelChangedEvent onLevelChanged = new LevelChangedEvent();



    [SerializeField] private ExperienceChangedEvent onExperienceChanged = new ExperienceChangedEvent();
    [SerializeField] private LevelChangedEvent onLevelChanged = new LevelChangedEvent();






    [Header("Debug")]
    [SerializeField] private int totalExp;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentLevelExp;

    [SerializeField] private List<LevelXpEntry> levelXpTable = new List<LevelXpEntry>();

    [SerializeField] private List<LevelXpEntry> levelXpTable = new List<LevelXpEntry>();





    [Header("Orb Prefabs")]
    [SerializeField] private GameObject xpOrbBronze;
    [SerializeField] private GameObject xpOrbSilver;
    [SerializeField] private GameObject xpOrbGold;

    public float MagnetRanage => magnetRanage;
    public float AbsorbDistance => absorbDistance;
    public float MagnetSpeed => magnetSpeed;
    public Transform Player => player;

    public Object PlayerExperienceReference => playerExperienceReference;


    public GameObject PlayerExperienceObject => playerExperienceObject;


    public PlayerExperience PlayerExperience => playerExperience;
    public int TotalExp => totalExp;
    public int CurrentLevel => currentLevel;
    public int CurrentLevelExp => currentLevelExp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolvePlayerReference();
        LoadLevelXpTable();
        RecalculateLevelState();

        SyncPlayerExperience();


   





    }

    private void OnValidate()
    {
        magnetRanage = Mathf.Max(0f, magnetRanage);
        absorbDistance = Mathf.Max(0.01f, absorbDistance);
        magnetSpeed = Mathf.Max(0f, magnetSpeed);
        ResolvePlayerExperienceReference();
    }

    public void DropOrbs(Vector2 position, List<MonsterController.ExpOrbDropEntry> dropEntries)
    {
        if (dropEntries == null || dropEntries.Count == 0)
        {
            return;
        }

        for (int i = 0; i < dropEntries.Count; i++)
        {
            MonsterController.ExpOrbDropEntry entry = dropEntries[i];
            if (Random.value > entry.DropChance)
            {
                continue;
            }

            GameObject resolvedPrefab = ResolveOrbPrefab(entry);
            if (resolvedPrefab == null)
            {
                continue;
            }

            int spawnCount = Mathf.Max(1, entry.DropCount);
            for (int spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.35f;
                Instantiate(resolvedPrefab, position + offset, Quaternion.identity);
            }
        }
    }

    public void AddExp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        PlayerExperience playerExperience = null;
        if (player != null)
        {
            playerExperience = player.GetComponent<PlayerExperience>();
        }

        if (playerExperience == null)
        {
            ResolvePlayerReference();
            if (player != null)
            {
                playerExperience = player.GetComponent<PlayerExperience>();
            }
        }

        if (playerExperience == null && player != null)
        {
            playerExperience = player.gameObject.AddComponent<PlayerExperience>();
        }

        if (playerExperience != null)
        {
            playerExperience.AddExperience(amount);
            return;
        }

        totalExp += amount;
        RecalculateLevelState();
        onExperienceChanged?.Invoke(totalExp);

        SyncPlayerExperience();

        Debug.Log($"EXP +{amount} (Total: {totalExp}, Lv: {currentLevel}, LvEXP: {currentLevelExp}/{GetNeedXpForLevel(currentLevel)})");
    }

    public int GetNeedXpForLevel(int level)
    {
        LevelXpEntry entry = levelXpTable.Find(data => data.Level == level);
        if (entry == null)
        {
            return 0;
        }

        return entry.NeedXP;
    }

    public void ResolvePlayerReference()
    {
        if (player != null)
        {
            ResolvePlayerExperienceReference();
            return;
        }

        GameObject tagged = GameObject.FindGameObjectWithTag("player");
        if (tagged != null)
        {
            player = tagged.transform;
            ResolvePlayerExperienceReference();
            return;
        }

        PlayerMovement2D playerMovement = FindObjectOfType<PlayerMovement2D>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
            ResolvePlayerExperienceReference();
        }
    }

    public void ResolvePlayerExperienceReference()
    {

        if (playerExperienceReference != null)
        {
            PlayerExperience resolvedByComponent = playerExperienceReference as PlayerExperience;
            if (resolvedByComponent != null)
            {
                playerExperience = resolvedByComponent;
                playerExperienceReference = resolvedByComponent;
                return;
            }

            GameObject resolvedGameObject = playerExperienceReference as GameObject;
            if (resolvedGameObject == null)
            {
                Component resolvedComponent = playerExperienceReference as Component;
                if (resolvedComponent != null)
                {
                    resolvedGameObject = resolvedComponent.gameObject;
                }
            }

            if (resolvedGameObject != null)
            {
                playerExperience = resolvedGameObject.GetComponent<PlayerExperience>();
                if (playerExperience != null)
                {
                    playerExperienceReference = playerExperience;
                    return;
                }
            }

        if (playerExperienceObject != null)
        {
            playerExperience = playerExperienceObject.GetComponent<PlayerExperience>();

        }

        if (playerExperience != null)
        {

            playerExperienceReference = playerExperience;

            playerExperienceObject = playerExperience.gameObject;

        if (playerExperience != null)
        {


            return;
        }

        if (player == null)
        {
            return;
        }

        playerExperience = player.GetComponent<PlayerExperience>();

        if (playerExperience != null)
        {
            playerExperienceReference = playerExperience;
        }
    }



        if (playerExperience != null)
        {
            playerExperienceObject = playerExperience.gameObject;
        }
    }


    }

    private void RecalculateLevelState()
    {
        if (levelXpTable.Count == 0)
        {
            currentLevel = 1;
            currentLevelExp = totalExp;
            return;
        }

        int previousLevel = currentLevel;
        int remaining = totalExp;
        int resolvedLevel = 1;

        for (int i = 0; i < levelXpTable.Count; i++)
        {
            LevelXpEntry entry = levelXpTable[i];
            if (entry.NeedXP <= 0)
            {
                continue;
            }

            resolvedLevel = entry.Level;
            if (remaining < entry.NeedXP)
            {
                currentLevel = resolvedLevel;
                currentLevelExp = remaining;
                if (currentLevel != previousLevel)
                {
                    onLevelChanged?.Invoke(currentLevel, currentLevelExp, entry.NeedXP);
                }

                return;
            }

            remaining -= entry.NeedXP;
            currentLevel = resolvedLevel + 1;
            currentLevelExp = remaining;
        }

        if (currentLevel != previousLevel)
        {
            onLevelChanged?.Invoke(currentLevel, currentLevelExp, 0);
        }
    }

    private void LoadLevelXpTable()
    {
        levelXpTable.Clear();

        HashSet<int> loadedLevels = new HashSet<int>();


        TextAsset csvAsset = Resources.Load<TextAsset>("LevelXP");
        if (csvAsset == null)
        {
            Debug.LogWarning("ExpDropManager: Resources/LevelXP.csv 파일을 찾을 수 없어 기본 레벨 경험치 표를 사용합니다.");
            levelXpTable.AddRange(GetDefaultLevelXpTable());
            return;
        }

        string[] lines = csvAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
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

            if (!int.TryParse(columns[1].Trim(), out int needXp))
            {
                continue;
            }

            if (level <= 0 || needXp <= 0)
            {
                continue;
            }


            if (!loadedLevels.Add(level))
            {
                Debug.LogWarning($"ExpDropManager: LevelXP.csv에 중복된 Level 식별자({level})가 있어 첫 번째 값만 사용합니다.");

            levelXpTable.Add(new LevelXpEntry
            {
                Level = level,
                NeedXP = needXp
            });
        }

        levelXpTable.Sort((a, b) => a.Level.CompareTo(b.Level));

        if (levelXpTable.Count == 0)
        {
            Debug.LogWarning("ExpDropManager: LevelXP.csv 파싱 결과가 비어 있어 기본 레벨 경험치 표를 사용합니다.");
            levelXpTable.AddRange(GetDefaultLevelXpTable());
        }
    }

    private static List<LevelXpEntry> GetDefaultLevelXpTable()
    {
        return new List<LevelXpEntry>
        {
            new LevelXpEntry { Level = 1, NeedXP = 10 },
            new LevelXpEntry { Level = 2, NeedXP = 20 },
            new LevelXpEntry { Level = 3, NeedXP = 35 },
            new LevelXpEntry { Level = 4, NeedXP = 55 },
            new LevelXpEntry { Level = 5, NeedXP = 80 }
        };
    }


    private void RecalculateLevelState()
    {
        if (levelXpTable.Count == 0)
        {
            currentLevel = 1;
            currentLevelExp = totalExp;
            return;
        }

        int previousLevel = currentLevel;
        int remaining = totalExp;
        int resolvedLevel = 1;

        for (int i = 0; i < levelXpTable.Count; i++)
        {
            LevelXpEntry entry = levelXpTable[i];
            if (entry.NeedXP <= 0)
            {
                continue;
            }

            resolvedLevel = entry.Level;
            if (remaining < entry.NeedXP)
            {
                currentLevel = resolvedLevel;
                currentLevelExp = remaining;
                if (currentLevel != previousLevel)
                {
                    onLevelChanged?.Invoke(currentLevel, currentLevelExp, entry.NeedXP);
                }

                return;
            }

            remaining -= entry.NeedXP;
            currentLevel = resolvedLevel + 1;
            currentLevelExp = remaining;
        }

        if (currentLevel != previousLevel)
        {
            onLevelChanged?.Invoke(currentLevel, currentLevelExp, 0);
        }
    }

    private void LoadLevelXpTable()
    {
        levelXpTable.Clear();

        TextAsset csvAsset = Resources.Load<TextAsset>("LevelXP");
        if (csvAsset == null)
        {
            Debug.LogWarning("ExpDropManager: Resources/LevelXP.csv 파일을 찾을 수 없어 기본 레벨 경험치 표를 사용합니다.");
            levelXpTable.AddRange(GetDefaultLevelXpTable());
            return;
        }


        string[] lines = csvAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

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

            if (!int.TryParse(columns[1].Trim(), out int needXp))
            {
                continue;
            }

            if (level <= 0 || needXp <= 0)
            {

                continue;
            }

            levelXpTable.Add(new LevelXpEntry
            {
                Level = level,
                NeedXP = needXp
            });
        }

        levelXpTable.Sort((a, b) => a.Level.CompareTo(b.Level));

        if (levelXpTable.Count == 0)
        {
            Debug.LogWarning("ExpDropManager: LevelXP.csv 파싱 결과가 비어 있어 기본 레벨 경험치 표를 사용합니다.");
            levelXpTable.AddRange(GetDefaultLevelXpTable());
        }
    }


    private static List<LevelXpEntry> GetDefaultLevelXpTable()
    {
        return new List<LevelXpEntry>
        {
            new LevelXpEntry { Level = 1, NeedXP = 10 },
            new LevelXpEntry { Level = 2, NeedXP = 20 },
            new LevelXpEntry { Level = 3, NeedXP = 35 },
            new LevelXpEntry { Level = 4, NeedXP = 55 },
            new LevelXpEntry { Level = 5, NeedXP = 80 }
        };
    }


    private GameObject ResolveOrbPrefab(MonsterController.ExpOrbDropEntry entry)
    {
        if (entry.OverrideOrbPrefab != null)
        {
            return IsValidOrbPrefab(entry.OverrideOrbPrefab) ? entry.OverrideOrbPrefab : null;
        }

        GameObject selectedPrefab = null;
        switch (entry.OrbType)
        {
            case MonsterController.ExpOrbType.Bronze:
                selectedPrefab = xpOrbBronze;
                break;
            case MonsterController.ExpOrbType.Silver:
                selectedPrefab = xpOrbSilver;
                break;
            case MonsterController.ExpOrbType.Gold:
                selectedPrefab = xpOrbGold;
                break;
        }

        return IsValidOrbPrefab(selectedPrefab) ? selectedPrefab : null;
    }

    private bool IsValidOrbPrefab(GameObject orbPrefab)
    {
        if (orbPrefab == null)
        {
            return false;
        }

        if (!orbPrefab.TryGetComponent(out ExpOrbController _))
        {
            Debug.LogWarning($"ExpDropManager: '{orbPrefab.name}' prefab does not include ExpOrbController.");
            return false;
        }

        return true;
    }

    private void SyncPlayerExperience()
    {
        ResolvePlayerExperienceReference();
        if (playerExperience == null)
        {
            return;
        }

        int needXp = GetNeedXpForLevel(currentLevel);
        playerExperience.SetExperienceState(totalExp, currentLevel, currentLevelExp, needXp);
    }
}
