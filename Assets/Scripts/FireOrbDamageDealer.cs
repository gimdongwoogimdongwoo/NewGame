using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FireOrbDamageDealer : MonoBehaviour
{
    private static int cachedMonsterLayer = int.MinValue;

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
        TreasureBoxController treasureBox = target.GetComponent<TreasureBoxController>() ?? target.GetComponentInParent<TreasureBoxController>();
        if (monster == null && treasureBox == null)
        {
            return;
        }

        int targetId = monster != null ? monster.GetInstanceID() : treasureBox.GetInstanceID();
        float now = Time.time;
        if (nextDamageTimeByTarget.TryGetValue(targetId, out float nextTime) && now < nextTime)
        {
            return;
        }

        float finalDamage = ResolvePlayerAttack() * damageMultiplier * ResolveCriticalMultiplier();
        if (finalDamage <= 0f)
        {
            return;
        }

        if (monster != null)
        {
            monster.TakeDamage(finalDamage);

            if (IsMonsterLayerHit(monster.gameObject))
            {
                SkillSfxPlayer.PlayFireRingHit();
            }
        }
        else
        {
            treasureBox.TakeDamage(finalDamage);
        }

        nextDamageTimeByTarget[targetId] = now + hitCooldown;
    }

    private static float ResolveCriticalMultiplier()
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.CriticalDamageMultiplier : 1f;
    }

    private static float ResolvePlayerAttack()
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.CurrentAttack : 1f;
    }

    private static bool IsMonsterLayerHit(GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        if (cachedMonsterLayer == int.MinValue)
        {
            cachedMonsterLayer = LayerMask.NameToLayer("Monster");
        }

        return cachedMonsterLayer >= 0 ? target.layer == cachedMonsterLayer : true;
    }
}
