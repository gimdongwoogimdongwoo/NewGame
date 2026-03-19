using System;
using UnityEngine;

public class SpawnedMonsterLifetime : MonoBehaviour
{
    private Action onDestroyed;
    private bool hasNotified;

    public void Initialize(Action onDestroyedCallback)
    {
        onDestroyed = onDestroyedCallback;
        hasNotified = false;
    }

    private void OnDestroy()
    {
        if (hasNotified)
        {
            return;
        }

        hasNotified = true;
        onDestroyed?.Invoke();
    }
}
