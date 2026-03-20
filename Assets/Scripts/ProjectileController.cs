using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float scale = 0.75f;

    [Header("Visual Settings")]
    [SerializeField] private float rotationOffset = 180f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.down;
    private bool isInitialized;

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

    public void Initialize(Vector2 direction, float configuredDamage, float configuredSpeed, float configuredLifeTime, float configuredScale)
    {
        projectileDamage = configuredDamage;
        speed = configuredSpeed;
        lifeTime = configuredLifeTime;
        scale = configuredScale;

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
        if (monster == null)
        {
            return;
        }

        monster.TakeDamage(projectileDamage);
        Destroy(gameObject);
    }
}
