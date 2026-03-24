using UnityEngine;

public class ExpOrbController : MonoBehaviour
{
    [Header("Exp")]
    [Tooltip("이 구슬이 플레이어에게 지급할 경험치")]
    [SerializeField] private int expValue = 1;

    [Header("Absorb")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float collectDistance = 0.1f;

    public int ExpValue => expValue;

    private bool isCollected;

    private void OnValidate()
    {
        expValue = Mathf.Max(1, expValue);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        collectDistance = Mathf.Max(0.01f, collectDistance);
    }

    private void Update()
    {
        ExpDropManager manager = ExpDropManager.Instance;
        if (manager == null)
        {
            return;
        }

        Transform player = manager.Player;
        if (player == null)
        {
            manager.ResolvePlayerReference();
            player = manager.Player;
            if (player == null)
            {
                return;
            }
        }

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > manager.MagnetRanage)
        {
            return;
        }

        float speed = Mathf.Max(moveSpeed, manager.MagnetSpeed);
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        float collectThreshold = Mathf.Max(collectDistance, manager.AbsorbDistance);
        if (Vector2.Distance(transform.position, player.position) <= collectThreshold)
        {
            Collect(manager);
        }
    }

    private void Collect(ExpDropManager manager)
    {
        if (isCollected)
        {
            return;
        }

        isCollected = true;
        manager.AddExp(expValue);
        Destroy(gameObject);
    }
}
