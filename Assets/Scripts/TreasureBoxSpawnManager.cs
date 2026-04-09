using UnityEngine;

public class TreasureBoxSpawnManager : MonoBehaviour
{
    [SerializeField] private TreasureBoxController treasureBoxPrefab;
    [SerializeField] private float spawnCooldown = 20f;
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private int maxActiveBoxes = 3;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnRadiusPadding = 1f;
    [SerializeField] private float spawnOuterPadding = 4f;

    private float nextSpawnTime;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (player == null)
        {
            Transform found = PlayerLocator.FindPlayerTransform();
            if (found != null)
            {
                player = found;
            }
        }

        nextSpawnTime = Time.time + Mathf.Max(0f, initialDelay);
    }

    private void Update()
    {
        if (GameplayPauseController.IsGameplayPaused || treasureBoxPrefab == null || player == null)
        {
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        if (GetActiveBoxCount() < Mathf.Max(0, maxActiveBoxes))
        {
            Vector2 spawnPos = ResolveSpawnPosition();
            Instantiate(treasureBoxPrefab, spawnPos, Quaternion.identity);
        }

        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnCooldown);
    }

    private int GetActiveBoxCount()
    {
        TreasureBoxController[] boxes = Object.FindObjectsByType<TreasureBoxController>(FindObjectsSortMode.None);
        int count = 0;
        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i] != null && !boxes[i].IsDead)
            {
                count++;
            }
        }

        return count;
    }

    private Vector2 ResolveSpawnPosition()
    {
        if (targetCamera == null)
        {
            return player.position;
        }

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        float minRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight)) + spawnRadiusPadding;

        for (int i = 0; i < 8; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float distance = Random.Range(minRadius, minRadius + spawnOuterPadding);
            Vector2 candidate = (Vector2)player.position + dir * distance;

            if (MapBoundaryController.Instance != null)
            {
                candidate = MapBoundaryController.Instance.ClampPosition(candidate);
            }

            if (Vector2.Distance(player.position, candidate) >= minRadius * 0.85f)
            {
                return candidate;
            }
        }

        return (Vector2)player.position + Vector2.right * minRadius;
    }
}
