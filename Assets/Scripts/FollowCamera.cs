using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private string targetTag = "player";
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float fixedZ = -10f;

    private Transform target;
    private Vector3 currentVelocity;

    private void Start()
    {
        FindTargetByTag();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindTargetByTag();
            if (target == null)
            {
                return;
            }
        }

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, fixedZ);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }

    private void FindTargetByTag()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(targetTag);

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }
}
