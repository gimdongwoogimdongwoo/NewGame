using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TimeStopPickupController : MonoBehaviour
{
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

        TimeStopController.TriggerTimeStop();
        Destroy(gameObject);
    }
}
