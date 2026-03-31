using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private string targetTag = "player";
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float fixedZ = -10f;

    private Transform target;
    private Vector3 currentVelocity;
    private Camera targetCamera;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        FindTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            if (target == null)
            {
                return;
            }
        }

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, fixedZ);

        if (MapBoundaryController.Instance != null)
        {
            targetPosition = MapBoundaryController.Instance.ClampCameraCenter(targetPosition, targetCamera);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }

    private void FindTarget()
    {
        Transform resolved = PlayerLocator.FindPlayerTransform();
        if (resolved != null)
        {
            target = resolved;
            return;
        }

        if (string.IsNullOrWhiteSpace(targetTag))
        {
            return;
        }

        target = null;
    }
}
