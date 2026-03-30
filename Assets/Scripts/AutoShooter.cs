using UnityEngine;

[RequireComponent(typeof(PlayerStatus))]
public class AutoShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform baseRotationReference;

    [Header("Firing")]
    [SerializeField] private float fireInterval = 0.25f;
    [SerializeField] private float minTurnCooldown = 0.1f;

    [Header("Projectile Runtime Stats")]
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float scale = 0.75f;

    private Vector2 lastInputDirection = Vector2.down;
    private Vector2 previousAimDirection = Vector2.down;
    private float nextFireTime;
    private float nextTurnAllowedTime;

    public void MultiplyDamageMultiplier(float multiplier)
    {
        if (multiplier <= 0f)
        {
            return;
        }

        damageMultiplier *= multiplier;
    }

    public void ImproveFireRateByPercent(float percent)
    {
        if (percent <= 0f)
        {
            return;
        }

        float multiplier = Mathf.Clamp(1f - (percent / 100f), 0.05f, 1f);
        fireInterval = Mathf.Max(0.02f, fireInterval * multiplier);
    }



    public void MultiplyDamageMultiplier(float multiplier)
    {
        if (multiplier <= 0f)

    public void AddDamage(float amount)
    {
        if (amount <= 0f)

        {
            return;
        }


        damageMultiplier *= multiplier;

        damage += amount;

    }

    public void ImproveFireRateByPercent(float percent)
    {
        if (percent <= 0f)
        {
            return;
        }

        float multiplier = Mathf.Clamp01(1f - (percent / 100f));
        fireInterval = Mathf.Max(0.02f, fireInterval * multiplier);
    }

    private void Reset()
    {
        spawnPoint = transform;
    }

    private void OnValidate()
    {
        fireInterval = Mathf.Max(0.02f, fireInterval);
        minTurnCooldown = Mathf.Max(0f, minTurnCooldown);
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        speed = Mathf.Max(0f, speed);
        lifeTime = Mathf.Max(0.05f, lifeTime);
        scale = Mathf.Max(0.01f, scale);
    }

    private void Update()
    {
        if (GameplayPauseController.IsGameplayPaused)
        {
            return;
        }

        UpdateAimDirection();
        TryAutoFire();
    }

    private void UpdateAimDirection()
    {
        Vector2 inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }

        if (inputDirection.sqrMagnitude > 0f)
        {
            lastInputDirection = inputDirection;
        }
    }

    private void TryAutoFire()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        Vector2 currentAimDirection = lastInputDirection.sqrMagnitude > 0f ? lastInputDirection.normalized : Vector2.down;

        if (Vector2.Dot(currentAimDirection, previousAimDirection) < 0.999f)
        {
            nextTurnAllowedTime = Time.time + minTurnCooldown;
            previousAimDirection = currentAimDirection;
        }

        if (Time.time < nextFireTime || Time.time < nextTurnAllowedTime)
        {
            return;
        }

        Fire(currentAimDirection);
        nextFireTime = Time.time + fireInterval;
    }

    private void Fire(Vector2 direction)
    {
        Quaternion baseRotation = baseRotationReference != null ? baseRotationReference.rotation : Quaternion.identity;
        Quaternion projectileRotation = baseRotation * Quaternion.Euler(0f, 0f, 180f);


        ProjectileController projectile = Instantiate(projectilePrefab, spawnPoint.position, projectileRotation);
        projectile.Initialize(direction, damageMultiplier, speed, lifeTime, scale);

        // 2D에서는 Z축 기준으로 180도 회전
        Quaternion flippedRotation = baseRotation * Quaternion.Euler(0f, 0f, 180f);

        // 발사체 생성
        ProjectileController projectile = Instantiate(projectilePrefab, spawnPoint.position, flippedRotation);

        // 이동 방향도 반대로 초기화
        projectile.Initialize(flippedDirection, damageMultiplier, speed, lifeTime, scale);

    }
}
