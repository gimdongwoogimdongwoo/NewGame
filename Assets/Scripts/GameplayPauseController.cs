using UnityEngine;

public class GameplayPauseController : MonoBehaviour
{
    private static bool isPausedByLevelUp;
    private static bool isPausedByGameResult;

    public static bool IsGameplayPaused => isPausedByLevelUp || isPausedByGameResult;

    public static void PauseForLevelUp()
    {
        isPausedByLevelUp = true;
        RefreshTimeScale();
    }

    public static void ResumeFromLevelUp()
    {
        isPausedByLevelUp = false;
        RefreshTimeScale();
    }

    public static void PauseForGameResult()
    {
        isPausedByGameResult = true;
        RefreshTimeScale();
    }

    public static void ClearGameResultPause()
    {
        isPausedByGameResult = false;
        RefreshTimeScale();
    }

    private static void RefreshTimeScale()
    {
        Time.timeScale = IsGameplayPaused ? 0f : 1f;
    }
}
