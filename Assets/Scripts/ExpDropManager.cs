using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExpDropManager : MonoBehaviour
{
    [System.Serializable]
    public class ExperienceChangedEvent : UnityEvent<float> { }

    [System.Serializable]
    public struct OrbPrefabBinding
    {
        [SerializeField] private string orbId;
        [SerializeField] private GameObject orbPrefab;

        public string OrbId => orbId;
        public GameObject OrbPrefab => orbPrefab;
    }

    public static ExpDropManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Magnet")]
    [Tooltip("구슬이 플레이어를 향해 이동하기 시작하는 거리")]
    [SerializeField] private float magnetRanage = 3f;
    [SerializeField] private float absorbDistance = 0.1f;
    [SerializeField] private float magnetSpeed = 8f;

    [Header("Experience")]
    [SerializeField] private ExperienceChangedEvent onExperienceChanged = new();

    [Header("Orb Prefab Library")]
    [Tooltip("MonsterController에서 orbId만 지정했을 때 사용할 프리팹 매핑")]
    [SerializeField] private List<OrbPrefabBinding> orbPrefabBindings = new();

    [Header("Debug")]
    [SerializeField] private int totalExp;

    public float MagnetRanage => magnetRanage;
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
            GameObject resolvedPrefab = ResolveOrbPrefab(entry);

            if (resolvedPrefab == null)
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

        totalExp += amount;
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

        PlayerMovement2D playerMovement = FindObjectOfType<PlayerMovement2D>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }
    }

    private GameObject ResolveOrbPrefab(MonsterController.ExpOrbDropEntry entry)
    {
        if (entry.OrbPrefab != null)
        {
            return IsValidOrbPrefab(entry.OrbPrefab) ? entry.OrbPrefab : null;
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
                return IsValidOrbPrefab(binding.OrbPrefab) ? binding.OrbPrefab : null;
            }
        }

        return null;
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
}
