using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ArrowProjectileController : MonoBehaviour
{
    [Header("Arrow Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float lifeTime = 4f;

    [Header("Arrow Damage")]
    [Tooltip("플레이어 현재 공격력에 곱해질 배율(예: 100=100%)")]
    [SerializeField] private float damageMultiplier = 100f;

    private Rigidbody2D rb;
    private MonsterController target;
    private Vector2 moveDirection = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(MonsterController initialTarget, float configuredSpeed, float configuredDamageMultiplier)
    {
        target = initialTarget;
        speed = Mathf.Max(0f, configuredSpeed);
        damageMultiplier = Mathf.Max(0f, configuredDamageMultiplier);

        if (target != null && !target.IsDead)
        {
            Vector2 targetDir = target.transform.position - transform.position;
            if (targetDir.sqrMagnitude > 0f)
            {
                moveDirection = targetDir.normalized;
            }
        }

        ApplyRotation(moveDirection);
    }

    private void Update()
    {
        UpdateDirection();
        ApplyRotation(moveDirection);
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = moveDirection * speed;
    }

    private void UpdateDirection()
    {
        if (target == null || target.IsDead)
        {
            target = null;
            return;
        }

        Vector2 toTarget = target.transform.position - transform.position;
        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector2 desired = toTarget.normalized;
        float maxRadians = turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
        moveDirection = Vector2.RotateTowards(moveDirection, desired, maxRadians, 0f).normalized;
    }

    private void ApplyRotation(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitMonster(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitMonster(collision.gameObject);
    }

    private void TryHitMonster(GameObject hitObject)
    {
        MonsterController monster = hitObject.GetComponent<MonsterController>() ?? hitObject.GetComponentInParent<MonsterController>();
        if (monster == null)
        {
            return;
        }

        float finalDamage = ResolvePlayerAttack() * (damageMultiplier / 100f);
        monster.TakeDamage(finalDamage, false);
        Destroy(gameObject);
    }

    private static float ResolvePlayerAttack()
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        return status != null ? status.CurrentAttack : 1f;
    }
}
