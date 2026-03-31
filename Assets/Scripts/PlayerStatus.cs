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

    public float BaseAttack => Mathf.Max(0f, baseAttack);
    public float CurrentAttack => BaseAttack * Mathf.Max(0f, attackMultiplier);
    public float AttackMultiplier => Mathf.Max(0f, attackMultiplier);

    public float BaseMaxHP => Mathf.Max(0f, baseMaxHP);
    public float CurrentMaxHP => Mathf.Max(0f, currentMaxHP);
    public float CurrentHP => Mathf.Clamp(currentHP, 0f, CurrentMaxHP);
    public float HpUpMultiplier => Mathf.Max(0f, hpUpMultiplier);

    private void Awake()
    {
        InitializeBattleHealth();
    }

    public void SetBaseAttack(float value)
    {
        baseAttack = Mathf.Max(0f, value);
    }

    public void SetBaseMaxHP(float value)
    {
        baseMaxHP = Mathf.Max(0f, value);
        InitializeBattleHealth();
    }

    public void InitializeBattleHealth()
    {
        hpUpMultiplier = 1f;
        currentMaxHP = BaseMaxHP;
        currentHP = currentMaxHP;
    }

    public void ApplyAttackUpPercent(int percent)
    {
        if (percent <= 0)
        {
            return;
        }

        float ratio = percent / 100f;
        attackMultiplier *= ratio;
    }

    public void ApplyHpUpPercent(int percent)
    {
        if (percent <= 0)
        {
            return;
        }

        float ratio = percent / 100f;
        hpUpMultiplier *= ratio;

        currentMaxHP = Mathf.Max(0f, currentMaxHP * ratio);
        currentHP = Mathf.Min(currentMaxHP, currentHP * ratio);
    }

    public void AddMaxHPFlat(float amount, bool fillAddedHealth)
    {
        if (amount <= 0f)
        {
            return;
        }

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
        if (amount <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Min(currentMaxHP, currentHP + amount);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Max(0f, currentHP - amount);
    }
}
