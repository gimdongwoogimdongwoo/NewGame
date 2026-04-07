using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PotionController : MonoBehaviour
{
    [Header("Potion")]
    [SerializeField] private int healValue = 30;

    public int HealValue
    {
        get => healValue;
        set => healValue = Mathf.Max(0, value);
    }

    private void OnValidate()
    {
        healValue = Mathf.Max(0, healValue);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollect(collision.gameObject);
    }

    private void TryCollect(GameObject other)
    {
        if (GameplayPauseController.IsGameplayPaused)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        if (healValue > 0)
        {
            playerHealth.Heal(healValue);
        }

        Destroy(gameObject);
    }
}
