using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class StageSurvivalTimerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hudTimeText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThresholdSeconds = 10f;

    [Header("Events")]
    [SerializeField] private UnityEvent onStageSuccess;
    [SerializeField] private UnityEvent onStageFail;

    private float remainingTime;
    private bool isFinished;
    private PlayerHealth playerHealth;

    private void Start()
    {
        int stageId = StageCsvLoader.ResolveCurrentStageId();
        remainingTime = stageId > 0 ? StageCsvLoader.LoadStageTimeSeconds(stageId) : 0f;

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        UpdateHud();

        if (remainingTime <= 0f)
        {
            TriggerSuccess();
        }
    }

    private void Update()
    {
        if (isFinished)
        {
            return;
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth != null && playerHealth.CurrentHP <= 0f && remainingTime > 0f)
        {
            TriggerFail();
            return;
        }

        if (GameplayPauseController.IsGameplayPaused)
        {
            UpdateHud();
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
        UpdateHud();

        if (remainingTime <= 0f)
        {
            TriggerSuccess();
        }
    }

    private void UpdateHud()
    {
        if (hudTimeText == null)
        {
            return;
        }

        int seconds = Mathf.Max(0, Mathf.CeilToInt(remainingTime));
        int mm = seconds / 60;
        int ss = seconds % 60;

        hudTimeText.text = $"{mm:00}:{ss:00}";
        hudTimeText.color = remainingTime <= warningThresholdSeconds ? warningColor : normalColor;
    }

    private void TriggerSuccess()
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        onStageSuccess?.Invoke();
        Debug.Log("Stage Success");
    }

    private void TriggerFail()
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        onStageFail?.Invoke();
        Debug.Log("Stage Fail");
    }
}
