using System;
using UnityEngine;

public static class LevelUpEffectService
{
    public static void Apply(string effect, int? value)
    {
        string effectKey = effect.Trim().ToUpperInvariant();

        PlayerHealth playerHealth = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        AutoShooter autoShooter = UnityEngine.Object.FindFirstObjectByType<AutoShooter>();
        PlayerMovement2D movement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement2D>();

        PlayerStatus playerStatus = UnityEngine.Object.FindFirstObjectByType<PlayerStatus>();



        int amount = value ?? 0;

        switch (effectKey)
        {
            case "HEAL":
                if (playerHealth != null)
                {
                    playerHealth.Heal(amount);
                }
                break;

            case "MAX_HP_UP":
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHP(amount, true);
                }
                break;

            case "HPUP":
                if (playerStatus != null)
                {
                    playerStatus.ApplyHpUpPercent(amount);
                    if (playerHealth != null)
                    {
                        playerHealth.SyncFromStatus();
                    }
                }
                break;

            case "MAGNET":
                if (playerStatus != null)
                {
                    playerStatus.ApplyMagnetPercent(amount);
                }
                break;

            case "MOVE_SPEED_UP":
                if (movement != null)
                {
                    movement.AddMoveSpeed(amount);
                }
                break;


            case "ATKUP":
                if (playerStatus != null)
                {
                    playerStatus.ApplyAttackUpPercent(amount);
                }
                break;

            case "DAMAGE_MULT_UP":
                if (autoShooter != null)
                {
                    autoShooter.MultiplyDamageMultiplier(amount / 100f);
                }
                break;

            case "DAMAGE_UP":
                if (autoShooter != null)
                {
                    autoShooter.AddDamage(amount);


                }
                break;

            case "FIRE_RATE_UP":
                if (autoShooter != null)
                {
                    autoShooter.ImproveFireRateByPercent(amount);
                }
                break;

            default:
                Debug.LogWarning($"LevelUpEffectService: 알 수 없는 Effect '{effect}'. 적용을 건너뜁니다.");
                break;
        }
    }
}
