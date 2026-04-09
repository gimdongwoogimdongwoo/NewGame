using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SuperMagnetController : MonoBehaviour
{
    [Header("Super Magnet Defaults")]
    [SerializeField] private float defaultMagnetSpeed = 3f;
    [SerializeField] private float defaultAbsorbDistance = 20f;

    private void OnValidate()
    {
        defaultMagnetSpeed = Mathf.Max(1f, defaultMagnetSpeed);
        defaultAbsorbDistance = Mathf.Max(0.1f, defaultAbsorbDistance);
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
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        MagnetBoostController boostController = Object.FindFirstObjectByType<MagnetBoostController>();
        if (boostController != null)
        {
            boostController.MagnetBoost(defaultMagnetSpeed, defaultAbsorbDistance);
        }

        Destroy(gameObject);
    }
}
