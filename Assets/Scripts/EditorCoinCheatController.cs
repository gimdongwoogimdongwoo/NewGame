#if UNITY_EDITOR
using UnityEngine;

public class EditorCoinCheatController : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<EditorCoinCheatController>() != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(EditorCoinCheatController));
        DontDestroyOnLoad(go);
        go.AddComponent<EditorCoinCheatController>();
    }

    [ContextMenu("Cheat/Reset Total Coin To 0")]
    private void ResetTotalCoins()
    {
        TotalCoinPersistence.Instance.ResetToDefault();
        Debug.Log("[EditorCoinCheat] Total coin reset to 0.");
    }


    [ContextMenu("Cheat/Reset All Upgrade Levels To 0")]
    private void ResetAllUpgradeLevels()
    {
        UpgradeSystem.Instance.ResetAllLevels();

        UpgradePopupUI popupUi = FindFirstObjectByType<UpgradePopupUI>(FindObjectsInactive.Include);
        if (popupUi != null)
        {
            popupUi.RefreshAll();
        }

        Debug.Log("[EditorCoinCheat] All upgrade levels reset to 0.");
    }

    [ContextMenu("Cheat/Reset All Achievements")]
    private void ResetAllAchievements()
    {
        AchievementManager.Instance.ResetAllAchievements();
        AchievementPopupUI popupUi = FindFirstObjectByType<AchievementPopupUI>(FindObjectsInactive.Include);
        if (popupUi != null)
        {
            popupUi.Refresh();
        }
    }

    [ContextMenu("Cheat/Add +100 Total Coin")]
    private void AddHundredCoins()
    {
        TotalCoinPersistence.Instance.AddCheatCoins(100);
        Debug.Log($"[EditorCoinCheat] Added 100. Total={TotalCoinPersistence.Instance.TotalCoins}");
    }
}
#endif
