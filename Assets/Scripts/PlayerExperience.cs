using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Player Experience")]
    [SerializeField] private int totalExp;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentLevelExp;
    [SerializeField] private int needXpToNextLevel;

    public int TotalExp => totalExp;
    public int CurrentLevel => currentLevel;
    public int Level => currentLevel;
    public int CurrentLevelExp => currentLevelExp;
    public int NeedXpToNextLevel => needXpToNextLevel;

    public void SetExperienceState(int newTotalExp, int newLevel, int newCurrentLevelExp, int newNeedXpToNextLevel)
    {
        totalExp = Mathf.Max(0, newTotalExp);
        currentLevel = Mathf.Max(1, newLevel);
        currentLevelExp = Mathf.Max(0, newCurrentLevelExp);
        needXpToNextLevel = Mathf.Max(0, newNeedXpToNextLevel);
    }
}
