using UnityEngine;

public static class PlayerLocator
{
    public static Transform FindPlayerTransform()
    {
        PlayerStatus status = Object.FindFirstObjectByType<PlayerStatus>();
        if (status != null)
        {
            return status.transform;
        }

        PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            return health.transform;
        }

        PlayerMovement2D movement = Object.FindFirstObjectByType<PlayerMovement2D>();
        if (movement != null)
        {
            return movement.transform;
        }

        return null;
    }
}
