using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MapBoundaryController : MonoBehaviour
{
    public static MapBoundaryController Instance { get; private set; }

    [SerializeField] private BoxCollider2D boundaryCollider;

    public Bounds WorldBounds => boundaryCollider.bounds;

    private void Reset()
    {
        boundaryCollider = GetComponent<BoxCollider2D>();
        boundaryCollider.isTrigger = true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple MapBoundaryController instances found. Destroying duplicate.", this);
            Destroy(this);
            return;
        }

        Instance = this;

        if (boundaryCollider == null)
        {
            boundaryCollider = GetComponent<BoxCollider2D>();
        }

        if (boundaryCollider == null)
        {
            Debug.LogError("MapBoundaryController requires a BoxCollider2D.", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Vector2 ClampPosition(Vector2 worldPosition)
    {
        Bounds bounds = WorldBounds;

        float clampedX = Mathf.Clamp(worldPosition.x, bounds.min.x, bounds.max.x);
        float clampedY = Mathf.Clamp(worldPosition.y, bounds.min.y, bounds.max.y);

        return new Vector2(clampedX, clampedY);
    }

    public Vector3 ClampCameraCenter(Vector3 worldPosition, Camera targetCamera)
    {
        if (targetCamera == null)
        {
            return worldPosition;
        }

        Bounds bounds = WorldBounds;

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        float clampedX = minX <= maxX
            ? Mathf.Clamp(worldPosition.x, minX, maxX)
            : bounds.center.x;

        float clampedY = minY <= maxY
            ? Mathf.Clamp(worldPosition.y, minY, maxY)
            : bounds.center.y;

        return new Vector3(clampedX, clampedY, worldPosition.z);
    }
}
