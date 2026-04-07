using UnityEngine;

public class MagnetBoostController : MonoBehaviour
{
    [Header("Magnet Boost")]
    [SerializeField] private float boostSpeedMultiplier = 3f;
    [SerializeField] private float boostDuration = 4f;

    private static MagnetBoostController activeController;
    private float boostEndTime;
    private bool isBoostActive;
    private float currentSpeedMultiplier = 1f;
    private float currentAbsorbDistance;

    public static bool IsBoostActive => activeController != null && activeController.isBoostActive;
    public static float CurrentSpeedMultiplier => IsBoostActive ? activeController.currentSpeedMultiplier : 1f;
    public static float CurrentAbsorbDistance => IsBoostActive ? activeController.currentAbsorbDistance : 0f;

    public float BoostSpeedMultiplier
    {
        get => boostSpeedMultiplier;
        set => boostSpeedMultiplier = Mathf.Max(1f, value);
    }

    public float BoostDuration
    {
        get => boostDuration;
        set => boostDuration = Mathf.Max(0.1f, value);
    }

    private void Awake()
    {
        if (activeController == null)
        {
            activeController = this;
        }
        else if (activeController != this)
        {
            Destroy(this);
            return;
        }

        boostSpeedMultiplier = Mathf.Max(1f, boostSpeedMultiplier);
        boostDuration = Mathf.Max(0.1f, boostDuration);
    }

    private void OnDestroy()
    {
        if (activeController == this)
        {
            activeController = null;
        }
    }

    private void Update()
    {
        if (!isBoostActive)
        {
            return;
        }

        if (Time.time >= boostEndTime)
        {
            isBoostActive = false;
            currentSpeedMultiplier = 1f;
            currentAbsorbDistance = 0f;
        }
    }

    public void MagnetBoost()
    {
        MagnetBoost(boostSpeedMultiplier, 0f);
    }

    public void MagnetBoost(float overrideSpeedMultiplier, float overrideAbsorbDistance)
    {
        isBoostActive = true;
        boostEndTime = Time.time + boostDuration;
        currentSpeedMultiplier = Mathf.Max(1f, overrideSpeedMultiplier);
        currentAbsorbDistance = Mathf.Max(0f, overrideAbsorbDistance);

        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("MagnetCollectible");
        for (int i = 0; i < collectibles.Length; i++)
        {
            _ = collectibles[i];
        }
    }
}
