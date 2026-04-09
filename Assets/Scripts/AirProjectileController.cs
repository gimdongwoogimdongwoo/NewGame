using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class AirProjectileController : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float maxTravelDistance = 12f;
    [SerializeField] private float knockbackDistance = 1.5f;

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.right;
    private Vector3 spawnPosition;

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
        spawnPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Vector2 direction, float configuredSpeed, float configuredKnockbackDistance)
    {
        moveDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        speed = Mathf.Max(0f, configuredSpeed);
        knockbackDistance = Mathf.Max(0f, configuredKnockbackDistance);

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        if ((transform.position - spawnPosition).sqrMagnitude >= maxTravelDistance * maxTravelDistance)
        {
            Destroy(gameObject);
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

        monster.ApplyKnockback(moveDirection, knockbackDistance);
        Destroy(gameObject);
    }
}
