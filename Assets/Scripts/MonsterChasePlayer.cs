using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterChasePlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private Transform target;
    private Rigidbody2D rb;

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
