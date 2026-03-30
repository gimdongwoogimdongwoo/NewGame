using UnityEngine;

public class GameplayPauseController : MonoBehaviour
{
    private static bool isPausedByLevelUp;

    public static bool IsGameplayPaused => isPausedByLevelUp;

    public static void PauseForLevelUp()
    {
        if (isPausedByLevelUp)
        {
            return;
        }

        isPausedByLevelUp = true;
        Time.timeScale = 0f;
    }

    public static void ResumeFromLevelUp()
    {
        if (!isPausedByLevelUp)
        {
            return;
        }

        isPausedByLevelUp = false;
        Time.timeScale = 1f;
    }
}
