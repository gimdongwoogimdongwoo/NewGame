using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float attackMultiplier = 1f;

    [Header("Health")]
    [SerializeField] private float baseMaxHP = 100f;
    [SerializeField] private float currentMaxHP;
    [SerializeField] private float currentHP;
    [SerializeField] private float hpUpMultiplier = 1f;

    [Header("Move")]
    [SerializeField] private float baseMoveSpeed = 3.5f;

    [Header("Attack Speed")]
    [SerializeField] private float baseAttackInterval = 0.25f;

    [Header("Critical")]
    [SerializeField] private float criticalChancePercent = 20f;
    [SerializeField] private float criticalDamagePercent = 100f;

    [Header("Revival")]
    [SerializeField] private int baseRevivalCount;
    [SerializeField] private int remainingRevivalCount;

    [Header("Pickup")]
    [SerializeField] private float basePickupRadius = 3f;
    [SerializeField] private float currentPickupRadius;
    [SerializeField] private float pickupRadiusMultiplier = 1f;

    public float BaseAttack => Mathf.Max(0f, baseAttack);
    public float CurrentAttack => BaseAttack * Mathf.Max(0f, attackMultiplier);
    public float AttackMultiplier => Mathf.Max(0f, attackMultiplier);

    public float BaseMaxHP => Mathf.Max(0f, baseMaxHP);
    public float CurrentMaxHP => Mathf.Max(0f, currentMaxHP);
    public float CurrentHP => Mathf.Clamp(currentHP, 0f, CurrentMaxHP);
    public float HpUpMultiplier => Mathf.Max(0f, hpUpMultiplier);

    public float BaseMoveSpeed => Mathf.Max(0f, baseMoveSpeed);
    public float CurrentMoveSpeed => BaseMoveSpeed;

    public float BaseAttackInterval => Mathf.Max(0.02f, baseAttackInterval);
    public float CurrentAttackInterval => BaseAttackInterval;

    public float CriticalChancePercent => Mathf.Clamp(criticalChancePercent, 0f, 100f);
    public float CriticalDamagePercent => Mathf.Max(100f, criticalDamagePercent);
    public float CriticalDamageMultiplier => CriticalDamagePercent / 100f;

    public int BaseRevivalCount => Mathf.Max(0, baseRevivalCount);
    public int RemainingRevivalCount => Mathf.Max(0, remainingRevivalCount);

    public float BasePickupRadius => Mathf.Max(0f, basePickupRadius);
    public float CurrentPickupRadius => Mathf.Max(0f, currentPickupRadius);
    public float PickupRadiusMultiplier => Mathf.Max(0f, pickupRadiusMultiplier);

    private void OnValidate()
    {
        baseAttack = Mathf.Max(0f, baseAttack);
        attackMultiplier = Mathf.Max(0f, attackMultiplier);

        baseMaxHP = Mathf.Max(0f, baseMaxHP);
        hpUpMultiplier = Mathf.Max(0f, hpUpMultiplier);

        baseMoveSpeed = Mathf.Max(0f, baseMoveSpeed);
        baseAttackInterval = Mathf.Max(0.02f, baseAttackInterval);
        criticalChancePercent = Mathf.Clamp(criticalChancePercent, 0f, 100f);
        criticalDamagePercent = Mathf.Max(100f, criticalDamagePercent);
        baseRevivalCount = Mathf.Max(0, baseRevivalCount);

        basePickupRadius = Mathf.Max(0f, basePickupRadius);
        pickupRadiusMultiplier = Mathf.Max(0f, pickupRadiusMultiplier);

        if (!Application.isPlaying)
        {
            currentMaxHP = baseMaxHP;
            currentHP = currentMaxHP;
            currentPickupRadius = basePickupRadius;
            remainingRevivalCount = baseRevivalCount;
            return;
        }

        currentMaxHP = Mathf.Max(0f, currentMaxHP);
        currentHP = Mathf.Clamp(currentHP, 0f, currentMaxHP);
        currentPickupRadius = Mathf.Max(0f, basePickupRadius * pickupRadiusMultiplier);
        remainingRevivalCount = Mathf.Max(0, remainingRevivalCount);
    }

    private void Awake()
    {
        InitializeBattleHealth();
    }

    public void SetBaseAttack(float value) => baseAttack = Mathf.Max(0f, value);

    public void SetBaseMaxHP(float value)
    {
        baseMaxHP = Mathf.Max(0f, value);
        InitializeBattleHealth();
    }

    public void SetBaseMoveSpeed(float value) => baseMoveSpeed = Mathf.Max(0f, value);
    public void SetBaseAttackInterval(float value) => baseAttackInterval = Mathf.Max(0.02f, value);
    public void SetCriticalChancePercent(float value) => criticalChancePercent = Mathf.Clamp(value, 0f, 100f);
    public void SetCriticalDamagePercent(float value) => criticalDamagePercent = Mathf.Max(100f, value);

    public void SetBaseRevivalCount(int count)
    {
        baseRevivalCount = Mathf.Max(0, count);
        remainingRevivalCount = baseRevivalCount;
    }


    public float ApplyCriticalDamage(float baseDamage)
    {
        if (baseDamage <= 0f)
        {
            return 0f;
        }

        float chance = CriticalChancePercent / 100f;
        bool isCritical = UnityEngine.Random.value <= chance;
        return isCritical ? baseDamage * CriticalDamageMultiplier : baseDamage;
    }

    public bool TryConsumeRevival()
    {
        if (remainingRevivalCount <= 0)
        {
            return false;
        }

        remainingRevivalCount = Mathf.Max(0, remainingRevivalCount - 1);
        return true;
    }

    public void RestoreFullHealth()
    {
        currentHP = CurrentMaxHP;
    }

    public void SetBasePickupRadius(float value)
    {
        basePickupRadius = Mathf.Max(0f, value);
        currentPickupRadius = BasePickupRadius * PickupRadiusMultiplier;
    }

    public void InitializeBattleHealth()
    {
        hpUpMultiplier = 1f;
        currentMaxHP = BaseMaxHP;
        currentHP = currentMaxHP;

        pickupRadiusMultiplier = 1f;
        currentPickupRadius = BasePickupRadius;
        remainingRevivalCount = BaseRevivalCount;
    }

    public void ApplyAttackUpPercent(int percent)
    {
        if (percent <= 0) return;
        float ratio = percent / 100f;
        attackMultiplier *= ratio;
    }

    public void ApplyHpUpPercent(int percent)
    {
        if (percent <= 0) return;
        float ratio = percent / 100f;
        hpUpMultiplier *= ratio;

        currentMaxHP = Mathf.Max(0f, currentMaxHP * ratio);
        currentHP = Mathf.Min(currentMaxHP, currentHP * ratio);
    }

    public void ApplyMagnetPercent(int percent)
    {
        if (percent <= 0) return;
        float ratio = percent / 100f;
        pickupRadiusMultiplier *= ratio;
        currentPickupRadius = BasePickupRadius * pickupRadiusMultiplier;
    }

    public void AddMaxHPFlat(float amount, bool fillAddedHealth)
    {
        if (amount <= 0f) return;

        currentMaxHP = Mathf.Max(0f, currentMaxHP + amount);

        if (fillAddedHealth)
        {
            currentHP = Mathf.Min(currentMaxHP, currentHP + amount);
            return;
        }

        currentHP = Mathf.Min(currentHP, currentMaxHP);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHP <= 0f) return;
        currentHP = Mathf.Min(currentMaxHP, currentHP + amount);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHP <= 0f) return;
        currentHP = Mathf.Max(0f, currentHP - amount);
    }
}
