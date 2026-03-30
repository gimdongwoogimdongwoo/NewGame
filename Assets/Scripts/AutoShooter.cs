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

    public void AddDamage(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        // damageMultiplier를 단순히 증가시키는 방식으로 수정
        damageMultiplier += amount;
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
        Quaternion projectileRotation = baseRotation;

        ProjectileController projectile = Instantiate(projectilePrefab, spawnPoint.position, projectileRotation);
        projectile.Initialize(direction, damageMultiplier, speed, lifeTime, scale);
    }
}