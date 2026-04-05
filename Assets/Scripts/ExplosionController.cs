using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private LayerMask affectsLayers = ~0;
    [Tooltip("플레이어 현재 공격력에 곱해질 배율(예: 30 => 30%)")]
    [SerializeField] private float damageMultiplier = 30f;
    [SerializeField] private bool chainPrevention = true;
    [SerializeField] private GameObject effectPrefab;

    [Header("Runtime")]
    [SerializeField, Range(0f, 100f)] private float explosionChancePercent;

    private static ExplosionController cachedController;

    public float ExplosionRadius
    {
        get => explosionRadius;
        set => explosionRadius = Mathf.Max(0f, value);
    }

    public LayerMask AffectsLayers
    {
        get => affectsLayers;
        set => affectsLayers = value;
    }

    public float DamageMultiplier
    {
        get => damageMultiplier;
        set => damageMultiplier = Mathf.Max(0f, value);
    }

    public bool ChainPrevention
    {
        get => chainPrevention;
        set => chainPrevention = value;
    }

    public GameObject EffectPrefab
    {
        get => effectPrefab;
        set => effectPrefab = value;
    }

    public static ExplosionController FindForPlayer()
    {
        if (cachedController != null)
        {
            return cachedController;
        }

        cachedController = Object.FindFirstObjectByType<ExplosionController>();
        return cachedController;
    }

    public void Activate(int chancePercent)
    {
        float normalized = Mathf.Clamp(chancePercent, 0, 100);
        explosionChancePercent = Mathf.Max(explosionChancePercent, normalized);
    }

    public bool TryTriggerFromProjectileKill(Vector3 origin, bool killedByExplosionFlag)
    {
        if (explosionChancePercent <= 0f)
        {
            return false;
        }

        if (chainPrevention && killedByExplosionFlag)
        {
            return false;
        }


        float chance = explosionChancePercent / 100f;
        if (Random.value > chance)


       


    

        float roll = Random.Range(0f, 100f);
        if (roll > explosionChancePercent)



        {
            return false;
        }

        TriggerExplosion(origin);
        return true;
    }

    private void TriggerExplosion(Vector3 origin)
    {

        Vector3 effectOrigin = new Vector3(origin.x, origin.y, 0f);
        ShowEffect(effectOrigin);


        Vector3 effectOrigin = new Vector3(origin.x, origin.y, 0f);
        ShowEffect(effectOrigin);


        Vector3 effectOrigin = new Vector3(origin.x, origin.y, 0f);
        ShowEffect(effectOrigin);

        ShowEffect(origin);




        float finalDamage = ResolvePlayerAttack() * (damageMultiplier / 100f);
        if (finalDamage <= 0f)
        {
            return;
        }


        Collider2D[] hits = Physics2D.OverlapCircleAll(effectOrigin, explosionRadius, affectsLayers);




        HashSet<int> damagedMonsterIds = new();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            MonsterController monster = hit.GetComponent<MonsterController>() ?? hit.GetComponentInParent<MonsterController>();
            if (monster == null)
            {
                continue;
            }

            int id = monster.GetInstanceID();
            if (damagedMonsterIds.Contains(id))
            {
                continue;
            }

            damagedMonsterIds.Add(id);
            monster.TakeDamage(finalDamage, chainPrevention);
        }
    }

    private void ShowEffect(Vector3 origin)
    {
        if (effectPrefab == null)
        {
            return;
        }

        GameObject spawned = Instantiate(effectPrefab, origin, Quaternion.identity);
        ExplosionEffectController effect = spawned.GetComponent<ExplosionEffectController>();
        if (effect == null)
        {
            effect = spawned.AddComponent<ExplosionEffectController>();
        }

        effect.Play(origin);
    }

    private static float ResolvePlayerAttack()
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.CurrentAttack : 1f;
    }
}
