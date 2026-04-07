using UnityEngine;

public class MagnetBoostController : MonoBehaviour
{
    [Header("Magnet Boost")]
    [SerializeField] private float boostSpeedMultiplier = 3f;
    [SerializeField] private float boostDuration = 4f;

    private static MagnetBoostController activeController;
    private float boostEndTime;
    private bool isBoostActive;

    public static bool IsBoostActive => activeController != null && activeController.isBoostActive;
    public static float CurrentSpeedMultiplier => IsBoostActive ? activeController.boostSpeedMultiplier : 1f;

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
        }
    }

    public void MagnetBoost()
    {
        isBoostActive = true;
        boostEndTime = Time.time + boostDuration;

        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("MagnetCollectible");
        for (int i = 0; i < collectibles.Length; i++)
        {
            // 발동 시점의 대상 탐색 보장을 위한 루프 (실제 이동은 각 오브젝트 Update에서 처리).
            _ = collectibles[i];
        }
    }
}
