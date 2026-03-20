using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Stats")]
    [SerializeField] private float maxHP = 30f;
    [SerializeField] private float attackDamage = 10f;

    private float currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void TryDamagePlayer(GameObject other)
    {
        if (!other.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null || playerHealth.IsInvincible)
        {
            return;
        }

        playerHealth.TakeDamage(attackDamage);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
