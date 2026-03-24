using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Stats")]
    [SerializeField] private float maxHP = 30f;
    [SerializeField] private float attackDamage = 10f;

    [Header("Exp Orb Drops")]
    [Tooltip("몬스터 사망 시 드롭할 구슬 종류/확률/수량")]
    [SerializeField] private List<ExpOrbDropEntry> expOrbDrops = new();

    private float currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void TryDamagePlayer(GameObject other)
    {
        if (!other.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null || playerHealth.IsInvincible)
        {
            return;
        }

        playerHealth.TakeDamage(attackDamage);
    }

    private void Die()
    {
        if (ExpDropManager.Instance != null)
        {
            ExpDropManager.Instance.DropOrbs(transform.position, expOrbDrops);
        }

        Destroy(gameObject);
    }

    public enum ExpOrbType
    {
        Bronze,
        Silver,
        Gold
    }

    [Serializable]
    public struct ExpOrbDropEntry
    {
        [SerializeField] private ExpOrbType orbType;
        [SerializeField] private GameObject overrideOrbPrefab;
        [SerializeField, Range(0f, 1f)] private float dropChance;
        [SerializeField] private int dropCount;

        public ExpOrbType OrbType => orbType;
        public GameObject OverrideOrbPrefab => overrideOrbPrefab;
        public float DropChance => Mathf.Clamp01(dropChance);
        public int DropCount => Mathf.Max(1, dropCount);
    }
}
