using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;


using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;


using UnityEngine.Events;



public class ExpDropManager : MonoBehaviour
{
    [System.Serializable]
    public class ExperienceChangedEvent : UnityEvent<float> { }



    [System.Serializable]
    public struct OrbPrefabBinding
    {
        [SerializeField] private string orbId;
        [SerializeField] private ExpOrbController orbPrefab;

        public string OrbId => orbId;
        public ExpOrbController OrbPrefab => orbPrefab;

        public OrbPrefabBinding(string orbId, ExpOrbController orbPrefab)
        {
            this.orbId = orbId;
            this.orbPrefab = orbPrefab;
        }
    }



    public static ExpDropManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Magnet")]
    [SerializeField] private float magnetRanage = 3f;
    [SerializeField] private float absorbDistance = 0.1f;
    [SerializeField] private float magnetSpeed = 8f;


    [Header("On Experience Changed")]
    [SerializeField] private ExperienceChangedEvent onExperienceChanged = new();



    [Header("Orb Prefab Library")]
    [SerializeField] private List<OrbPrefabBinding> orbPrefabBindings = new();

    [SerializeField] private DefaultAsset orbPrefabFolder;

    [Header("On Experience Changed")]
    [SerializeField] private ExperienceChangedEvent onExperienceChanged = new();


    [Header("On Experience Changed")]
    [SerializeField] private ExperienceChangedEvent onExperienceChanged = new();


public class ExpDropManager : MonoBehaviour
{
    public static ExpDropManager Instance { get; private set; }

    [Header("Magnet")]
    [SerializeField] private float magnetRanage = 3f;
    [SerializeField] private Transform player;




    [Header("Debug")]
    [SerializeField] private int totalExp;

    public float MagnetRanage => magnetRanage;

    public float AbsorbDistance => absorbDistance;
    public float MagnetSpeed => magnetSpeed;


    public float AbsorbDistance => absorbDistance;
    public float MagnetSpeed => magnetSpeed;


    public float AbsorbDistance => absorbDistance;
    public float MagnetSpeed => magnetSpeed;



    public Transform Player => player;
    public int TotalExp => totalExp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolvePlayerReference();
    }

    private void OnValidate()
    {
        magnetRanage = Mathf.Max(0f, magnetRanage);

        absorbDistance = Mathf.Max(0.01f, absorbDistance);
        magnetSpeed = Mathf.Max(0f, magnetSpeed);


        absorbDistance = Mathf.Max(0.01f, absorbDistance);
        magnetSpeed = Mathf.Max(0f, magnetSpeed);


        absorbDistance = Mathf.Max(0.01f, absorbDistance);
        magnetSpeed = Mathf.Max(0f, magnetSpeed);




    }

    public void DropOrbs(Vector2 position, IReadOnlyList<MonsterController.ExpOrbDropEntry> dropEntries)
    {
        if (dropEntries == null || dropEntries.Count == 0)
        {
            return;
        }

        for (int i = 0; i < dropEntries.Count; i++)
        {
            MonsterController.ExpOrbDropEntry entry = dropEntries[i];

            if (entry.OrbPrefab == null)


            ExpOrbController resolvedPrefab = ResolveOrbPrefab(entry);
            if (resolvedPrefab == null)

            if (entry.OrbPrefab == null)


            {
                continue;
            }

            if (Random.value > entry.DropChance)
            {
                continue;
            }

            int spawnCount = Mathf.Max(1, entry.DropCount);
            for (int spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.35f;

                GameObject orbInstance = Instantiate(entry.OrbPrefab, position + offset, Quaternion.identity);

                if (!orbInstance.TryGetComponent(out ExpOrbController orbController))
                {
                    Debug.LogWarning($"ExpDropManager: '{entry.OrbPrefab.name}' prefab has no ExpOrbController component.");
                    Destroy(orbInstance);
                }


                Instantiate(resolvedPrefab, position + offset, Quaternion.identity);

                Instantiate(entry.OrbPrefab, position + offset, Quaternion.identity);


            }
        }
    }

    public void AddExp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        totalExp += amount;

        onExperienceChanged?.Invoke(totalExp);

        onExperienceChanged?.Invoke(totalExp);


        onExperienceChanged?.Invoke(totalExp);




        Debug.Log($"EXP +{amount} (Total: {totalExp})");
    }

    public void ResolvePlayerReference()
    {
        if (player != null)
        {
            return;
        }

        GameObject tagged = GameObject.FindGameObjectWithTag("player");
        if (tagged != null)
        {
            player = tagged.transform;
            return;
        }

        PlayerMovement2D playerMovement = FindFirstObjectByType<PlayerMovement2D>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }
    }



    private ExpOrbController ResolveOrbPrefab(MonsterController.ExpOrbDropEntry entry)
    {
        if (entry.OrbPrefab != null)
        {
            return entry.OrbPrefab;
        }

        if (string.IsNullOrWhiteSpace(entry.OrbId))
        {
            return null;
        }

        for (int i = 0; i < orbPrefabBindings.Count; i++)
        {
            OrbPrefabBinding binding = orbPrefabBindings[i];
            if (binding.OrbPrefab == null)
            {
                continue;
            }

            if (binding.OrbId == entry.OrbId)
            {
                return binding.OrbPrefab;
            }
        }

        return null;
    }


    [ContextMenu("Auto Populate Orb Prefab Library From Folder")]
    private void AutoPopulateOrbPrefabLibraryFromFolder()
    {
        orbPrefabBindings.Clear();

        if (orbPrefabFolder == null)
        {
            Debug.LogWarning("ExpDropManager: orbPrefabFolder is not assigned.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(orbPrefabFolder);
        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                continue;
            }

            ExpOrbController controller = prefab.GetComponent<ExpOrbController>();
            if (controller == null)
            {
                continue;
            }

            orbPrefabBindings.Add(new OrbPrefabBinding(prefab.name, controller));
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"ExpDropManager: registered {orbPrefabBindings.Count} orb prefabs from {folderPath}");
    }


}
