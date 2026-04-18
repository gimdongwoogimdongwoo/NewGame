using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileController : MonoBehaviour
{
    private static int cachedMonsterLayer = int.MinValue;

    [Header("Projectile Stats")]
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float scale = 0.75f;

    [Header("Visual Settings")]
    [SerializeField] private float rotationOffset = 180f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.down;
    private bool isInitialized;
    private bool canPierce;
    private readonly HashSet<int> hitTargetIds = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        transform.localScale = Vector3.one * scale;

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        if (!isInitialized)
        {
            SetDirection(Vector2.down);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = moveDirection * speed;
    }

    public void Initialize(Vector2 direction, float configuredDamageMultiplier, float configuredSpeed, float configuredLifeTime, float configuredScale, bool pierceEnabled)
    {
        damageMultiplier = Mathf.Max(0f, configuredDamageMultiplier);
        speed = configuredSpeed;
        lifeTime = configuredLifeTime;
        scale = configuredScale;
        canPierce = pierceEnabled;
        hitTargetIds.Clear();

        transform.localScale = Vector3.one * scale;
        Destroy(gameObject, lifeTime);

        SetDirection(direction);
        isInitialized = true;
    }

    private void SetDirection(Vector2 direction)
    {
        moveDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.down;
        ApplySpriteOrientation(moveDirection);
    }

    private void ApplySpriteOrientation(Vector2 direction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        spriteRenderer.flipX = false;
    }
    private static float ApplyCritical(float baseDamage)
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.ApplyCriticalDamage(baseDamage) : baseDamage;
    }


    private static float ResolveCriticalMultiplier()
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.CriticalDamageMultiplier : 1f;
    }

    private static float ResolvePlayerAttack()
    {
        PlayerStatus status = UnityEngine.Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.CurrentAttack : 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject target)
    {
        if (target.GetComponent<ProjectileController>() != null ||
            target.GetComponentInParent<ProjectileController>() != null)
        {
            return;
        }

        if (target.GetComponent<PlayerHealth>() != null ||
            target.GetComponentInParent<PlayerHealth>() != null)
        {
            return;
        }

        MonsterController monster = target.GetComponent<MonsterController>() ?? target.GetComponentInParent<MonsterController>();
        TreasureBoxController treasureBox = target.GetComponent<TreasureBoxController>() ?? target.GetComponentInParent<TreasureBoxController>();
        if (monster == null && treasureBox == null)
        {
            return;
        }

        float currentAttack = ResolvePlayerAttack();
        float finalDamage = ApplyCritical(currentAttack * Mathf.Max(0f, damageMultiplier));

        int targetId = monster != null ? monster.GetInstanceID() : treasureBox.GetInstanceID();
        if (hitTargetIds.Contains(targetId))
        {
            return;
        }

        hitTargetIds.Add(targetId);

        if (monster != null)
        {
            if (IsMonsterLayerHit(monster.gameObject))
            {
                SkillSfxPlayer.PlayProjectileHit();
            }

            Vector3 hitPosition = monster.transform.position;
            bool killed = monster.TakeDamage(finalDamage, false);
            if (killed)
            {
                ExplosionController explosionController = ExplosionController.FindForPlayer();
                if (explosionController != null)
                {
                    explosionController.TryTriggerFromProjectileKill(hitPosition, monster.LastHitFromExplosion);
                }
            }
        }
        else
        {
            treasureBox.TakeDamage(finalDamage);
        }

        if (!canPierce)
        {
            Destroy(gameObject);
        }
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
