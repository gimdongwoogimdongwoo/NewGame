using UnityEngine;

public interface IMagnetCollectible
{
    void Collect(Transform player);
}

public abstract class MagnetCollectible : MonoBehaviour, IMagnetCollectible
{
    private const string MagnetCollectibleTag = "MagnetCollectible";

    [Header("Magnet Collectible")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float collectDistance = 0.1f;

    private Transform cachedPlayer;
    private PlayerStatus cachedPlayerStatus;
    private bool isCollected;

    protected virtual void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        collectDistance = Mathf.Max(0.01f, collectDistance);
    }

    protected virtual void Update()
    {
        if (isCollected || GameplayPauseController.IsGameplayPaused)
        {
            return;
        }

        if (!TryResolvePlayer(out Transform player, out PlayerStatus status))
        {
            return;
        }

        float pickupRadius = status != null ? status.CurrentPickupRadius : 0f;
        if (pickupRadius <= 0f)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > pickupRadius)
        {
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, player.position) <= collectDistance)
        {
            isCollected = true;
            Collect(player);
        }
    }

    private bool TryResolvePlayer(out Transform player, out PlayerStatus status)
    {
        player = cachedPlayer;
        status = cachedPlayerStatus;

        if (player == null)
        {

            Transform foundPlayer = PlayerLocator.FindPlayerTransform();
            if (foundPlayer != null)
            {
                player = foundPlayer;

            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("player");
            if (taggedPlayer != null)
            {
                player = taggedPlayer.transform;

                cachedPlayer = player;
            }
        }

        if (player == null)
        {
            status = null;
            return false;
        }

        if (status == null)
        {
            status = player.GetComponent<PlayerStatus>();
            if (status == null)
            {
                status = player.GetComponentInParent<PlayerStatus>();
            }

            cachedPlayerStatus = status;
        }

        return true;
    }

    public void Collect(Transform player)
    {
        OnCollected(player);
    }

    protected abstract void OnCollected(Transform player);

    protected bool IsMagnetCollectibleTagValid()
    {
        return gameObject.tag == MagnetCollectibleTag;
    }
}
