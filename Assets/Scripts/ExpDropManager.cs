using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class ExpDropManager : MonoBehaviour
{
    [System.Serializable]
    public class ExperienceChangedEvent : UnityEvent<float> { }

    public static ExpDropManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Magnet")]
    [SerializeField] private float magnetRanage = 3f;
    [SerializeField] private float absorbDistance = 0.1f;
    [SerializeField] private float magnetSpeed = 8f;

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
}
