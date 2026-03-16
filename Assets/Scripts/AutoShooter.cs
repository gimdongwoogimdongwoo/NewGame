using UnityEngine;

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
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float scale = 0.75f;

    private Vector2 lastInputDirection = Vector2.right;
    private Vector2 previousAimDirection = Vector2.right;
    private float nextFireTime;
    private float nextTurnAllowedTime;

    private void Reset()
    {
        spawnPoint = transform;
    }

    private void Update()
    {
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

        Vector2 currentAimDirection = lastInputDirection.sqrMagnitude > 0f ? lastInputDirection.normalized : Vector2.right;

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
        ProjectileController projectile = Instantiate(projectilePrefab, spawnPoint.position, baseRotation);

        projectile.Initialize(direction, damage, speed, lifeTime, scale);
    }
}
