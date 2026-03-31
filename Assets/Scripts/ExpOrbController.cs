using UnityEngine;

public class ExpOrbController : MagnetCollectible
{
    [Header("Exp")]
    [Tooltip("이 구슬이 플레이어에게 지급할 경험치")]
    [SerializeField] private int expValue = 1;

    public int ExpValue => expValue;

    protected override void OnValidate()
    {
        base.OnValidate();
        expValue = Mathf.Max(1, expValue);
    }

    private void Awake()
    {
        if (!IsMagnetCollectibleTagValid())
        {
            Debug.LogWarning($"{name}: 드롭형 아이템은 'MagnetCollectible' 태그를 권장합니다.");
        }
    }

    protected override void OnCollected(Transform player)
    {
        ExpDropManager manager = ExpDropManager.Instance;
        if (manager != null)
        {
            manager.AddExp(expValue);
        }

        Destroy(gameObject);
    }
}
