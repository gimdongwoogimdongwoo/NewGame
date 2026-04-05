using UnityEngine;

public static class LevelUpEffectService
{
    public static void Apply(string effect, int? value)
    {
        if (string.IsNullOrWhiteSpace(effect))
        {
            Debug.LogWarning("LevelUpEffectService: Effect가 비어 있어 적용을 건너뜁니다.");
            return;
        }

        string effectKey = effect.Trim().ToUpperInvariant();
        int amount = value ?? 0;

        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        AutoShooter autoShooter = Object.FindFirstObjectByType<AutoShooter>();
        PlayerMovement2D movement = Object.FindFirstObjectByType<PlayerMovement2D>();
        PlayerStatus playerStatus = Object.FindFirstObjectByType<PlayerStatus>();
        FireRingController fireRingController = Object.FindFirstObjectByType<FireRingController>();
        ExplosionController explosionController = Object.FindFirstObjectByType<ExplosionController>();
        ArrowController arrowController = Object.FindFirstObjectByType<ArrowController>();
        AirController airController = Object.FindFirstObjectByType<AirController>();

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

            case "PLUS1":
                if (autoShooter != null)
                {
                    int projectileCount = amount > 0 ? amount : 1;
                    autoShooter.AddOppositeProjectileCount(projectileCount);
                }
                break;

            case "FIRERING":
                if (fireRingController != null)
                {
                    int orbCount = amount > 0 ? amount : 1;
                    fireRingController.Activate(orbCount);
                }
                break;

            case "EXPLOSION":
                if (explosionController != null)
                {
                    int chancePercent = Mathf.Clamp(amount, 0, 100);
                    explosionController.Activate(chancePercent);
                }
                break;

            case "ARROW":
                if (arrowController != null)
                {
                    int arrowCount = amount > 0 ? amount : 1;
                    arrowController.Activate(arrowCount);
                }
                break;

            case "AIR":
                if (airController != null)
                {
                    int airCount = amount > 0 ? amount : 1;
                    airController.Activate(airCount);
                }
                break;

            case "PIERCE":
                if (autoShooter != null)
                {
                    autoShooter.EnablePierce();
                }
                break;

            default:
                Debug.LogWarning($"LevelUpEffectService: 알 수 없는 Effect '{effect}'. 적용을 건너뜁니다.");
                break;
        }
    }
}
