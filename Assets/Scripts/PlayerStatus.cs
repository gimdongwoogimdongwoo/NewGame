using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerStatus : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float attackMultiplier = 1f;

    public float BaseAttack => Mathf.Max(0f, baseAttack);
    public float CurrentAttack => BaseAttack * Mathf.Max(0f, attackMultiplier);
    public float AttackMultiplier => Mathf.Max(0f, attackMultiplier);

    public void SetBaseAttack(float value)
    {
        baseAttack = Mathf.Max(0f, value);
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
}
