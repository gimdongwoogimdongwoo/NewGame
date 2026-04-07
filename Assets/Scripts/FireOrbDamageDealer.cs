using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FireOrbDamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageMultiplier = 1f;
    [Tooltip("동일 적에게 다시 피해를 줄 수 있는 최소 간격(초)")]
    [SerializeField] private float hitCooldown = 0.2f;

    private readonly Dictionary<int, float> nextDamageTimeByTarget = new();

    public float DamageMultiplier
    {
        get => damageMultiplier;
        set => damageMultiplier = Mathf.Max(0f, value);
    }

    public float HitCooldown
    {
        get => hitCooldown;
        set => hitCooldown = Mathf.Max(0f, value);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision.gameObject);
    }

    private void TryDealDamage(GameObject target)
    {
        if (GameplayPauseController.IsGameplayPaused)
        {
            return;
        }

        MonsterController monster = target.GetComponent<MonsterController>() ?? target.GetComponentInParent<MonsterController>();
        if (monster == null)
        {
            return;
        }

        int targetId = monster.GetInstanceID();
        float now = Time.time;
        if (nextDamageTimeByTarget.TryGetValue(targetId, out float nextTime) && now < nextTime)
        {
            return;
        }

        float finalDamage = ResolvePlayerAttack() * damageMultiplier;
        if (finalDamage <= 0f)
        {
            return;
        }

        monster.TakeDamage(finalDamage);
        nextDamageTimeByTarget[targetId] = now + hitCooldown;
    }

    private static float ResolvePlayerAttack()
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.CurrentAttack : 1f;
    }
}
