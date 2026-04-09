using System.Collections.Generic;
using UnityEngine;

public class TreasureBoxController : MonoBehaviour
{
    [SerializeField] private float maxHP = 50f;
    [SerializeField] private List<DropEntry> drops = new();

    private float currentHP;

    public bool IsDead => currentHP <= 0f;

    private void Awake()
    {
        currentHP = Mathf.Max(1f, maxHP);
    }

    public bool TakeDamage(float damage)
    {
        if (GameplayPauseController.IsGameplayPaused || damage <= 0f || IsDead)
        {
            return false;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        if (currentHP <= 0f)
        {
            Die();
            return true;
        }

        return false;
    }

    private void Die()
    {
        SpawnDrop();
        Destroy(gameObject);
    }

    private void SpawnDrop()
    {
        if (drops == null || drops.Count == 0)
        {
            return;
        }

        float totalChance = 0f;
        for (int i = 0; i < drops.Count; i++)
        {
            if (drops[i].Prefab == null)
            {
                continue;
            }

            totalChance += Mathf.Max(0f, drops[i].Chance);
        }

        if (totalChance <= 0f)
        {
            return;
        }

        float roll = Random.Range(0f, totalChance);
        float cursor = 0f;

        for (int i = 0; i < drops.Count; i++)
        {
            DropEntry entry = drops[i];
            if (entry.Prefab == null)
            {
                continue;
            }

            cursor += Mathf.Max(0f, entry.Chance);
            if (roll <= cursor)
            {
                Instantiate(entry.Prefab, transform.position, Quaternion.identity);
                return;
            }
        }
    }

    [System.Serializable]
    public struct DropEntry
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private float chance;

        public GameObject Prefab => prefab;
        public float Chance => chance;
    }
}
