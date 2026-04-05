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
    private bool lastHitFromExplosion;
    private Coroutine knockbackRoutine;

    public bool IsDead => currentHP <= 0f;
    public bool LastHitFromExplosion => lastHitFromExplosion;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, false);
    }

    public bool TakeDamage(float damage, bool fromExplosion)
    {
        if (GameplayPauseController.IsGameplayPaused || damage <= 0f || currentHP <= 0f)
        {
            return false;
        }

        lastHitFromExplosion = fromExplosion;
        currentHP = Mathf.Max(0f, currentHP - damage);
        if (currentHP <= 0f)
        {
            Die();
            return true;
        }

        return false;
    }


    public void ApplyKnockback(Vector2 direction, float distance)
    {
        if (distance <= 0f || IsDead)
        {
            return;
        }

        Vector2 knockDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.zero;
        if (knockDirection == Vector2.zero)
        {
            return;
        }

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        knockbackRoutine = StartCoroutine(KnockbackRoutine(knockDirection, distance));
    }

    private System.Collections.IEnumerator KnockbackRoutine(Vector2 direction, float distance)
    {
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(direction * distance);
        float duration = 0.12f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        knockbackRoutine = null;
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
        if (GameplayPauseController.IsGameplayPaused)
        {
            return;
        }

        // FireRing 오브젝트(자식 포함)와의 충돌은 플레이어 피격으로 취급하지 않는다.
        if (other.GetComponent<FireOrbDamageDealer>() != null ||
            other.GetComponentInParent<FireOrbDamageDealer>() != null ||
            other.GetComponent<FireRingController>() != null ||
            other.GetComponentInParent<FireRingController>() != null)
        {
            return;
        }

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
