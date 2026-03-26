using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Player Experience")]
    [SerializeField] private int totalExp;
    [SerializeField] private int level = 1;
    [SerializeField] private int currentLevelExp;
    [SerializeField] private int needXpToNextLevel;

    public int TotalExp => totalExp;
    public int Level => level;
    public int CurrentLevelExp => currentLevelExp;
    public int NeedXpToNextLevel => needXpToNextLevel;

    public void SetExperienceState(int newTotalExp, int newLevel, int newCurrentLevelExp, int newNeedXpToNextLevel)
    {
        totalExp = Mathf.Max(0, newTotalExp);
        level = Mathf.Max(1, newLevel);
        currentLevelExp = Mathf.Max(0, newCurrentLevelExp);
        needXpToNextLevel = Mathf.Max(0, newNeedXpToNextLevel);
    }
}
