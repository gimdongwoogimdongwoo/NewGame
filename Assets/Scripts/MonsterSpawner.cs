using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인스펙터에서 등록한 프리팹들을 개별 설정값에 따라 플레이어 주변에 자동으로 스폰하는 클래스.
/// CSV는 사용하지 않습니다.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnRadiusPadding = 1f;
    [SerializeField] private float spawnOuterPadding = 3f;

    // 인스펙터에서 프리팹과 개별 설정을 등록하는 리스트
    [SerializeField] private List<MonsterSpawnEntry> monsterEntries = new();

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        ResolvePlayerReference();

        if (player == null)
        {
            Debug.LogError("MonsterSpawner could not find Player target. Assign player Transform in Inspector.");
            return;
        }

        // 각 프리팹마다 개별 스폰 루프 시작
        foreach (var entry in monsterEntries)
        {
            if (entry.prefab != null)
            {
                StartCoroutine(RunSpawnLoop(entry));
            }
        }
    }

    private IEnumerator RunSpawnLoop(MonsterSpawnEntry entry)
    {
        yield return WaitForSecondsExcludingTimeStop(entry.spawnStartDelay);

        int spawnedTotal = 0;
        int waveIndex = 0;
        int aliveCount = 0;

        while (spawnedTotal < entry.totalBudget)
        {
            int waveSize = Mathf.Min(
                entry.waveSizeStart + (waveIndex * entry.waveSizeGrowth),
                entry.waveSizeMax);

            int remainingBudget = entry.totalBudget - spawnedTotal;
            int aliveCapacity = entry.maxAliveCap - aliveCount;

            int spawnCount = Mathf.Min(waveSize, remainingBudget, aliveCapacity);

            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 spawnPosition = ResolveSpawnPosition();
                GameObject monster = Instantiate(entry.prefab, spawnPosition, Quaternion.identity);

                // 플레이어 추적 컴포넌트 연결
                MonsterChasePlayer chase = monster.GetComponent<MonsterChasePlayer>();
                if (chase == null) chase = monster.AddComponent<MonsterChasePlayer>();
                chase.SetTarget(player);

                // 생명주기 관리 컴포넌트 연결
                SpawnedMonsterLifetime lifetime = monster.GetComponent<SpawnedMonsterLifetime>();
                if (lifetime == null) lifetime = monster.AddComponent<SpawnedMonsterLifetime>();
                lifetime.Initialize(() => aliveCount = Mathf.Max(0, aliveCount - 1));

                spawnedTotal++;
                aliveCount++;
            }

            waveIndex++;
            yield return WaitForSecondsExcludingTimeStop(entry.waveIntervalSec);
        }
    }


    private static IEnumerator WaitForSecondsExcludingTimeStop(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (!TimeStopController.IsTimeStopped)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }
    }

    private Vector2 ResolveSpawnPosition()
    {
        if (player == null)
        {
            ResolvePlayerReference();
            if (player == null) return Vector2.zero;
        }

        if (targetCamera == null) return player.position;

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        float minRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight)) + spawnRadiusPadding;

        for (int i = 0; i < 8; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir == Vector2.zero) dir = Vector2.right;

            float distance = Random.Range(minRadius, minRadius + spawnOuterPadding);
            Vector2 candidate = (Vector2)player.position + (dir * distance);

            if (MapBoundaryController.Instance != null)
                candidate = MapBoundaryController.Instance.ClampPosition(candidate);

            if (Vector2.Distance(player.position, candidate) >= minRadius * 0.85f)
                return candidate;
        }

        Vector2 fallback = (Vector2)player.position + (Vector2.right * minRadius);
        if (MapBoundaryController.Instance != null)
            fallback = MapBoundaryController.Instance.ClampPosition(fallback);

        return fallback;
    }

    private void ResolvePlayerReference()
    {
        if (player != null) return;

        Transform foundPlayer = PlayerLocator.FindPlayerTransform();
        if (foundPlayer != null)
        {
            player = foundPlayer;
        }
    }

    [System.Serializable]
    private struct MonsterSpawnEntry
    {
        public GameObject prefab;       // 소환할 프리팹
        public float spawnStartDelay;   // 첫 스폰 시작 지연 시간
        public float waveIntervalSec;   // 웨이브 간격
        public int waveSizeStart;       // 시작 웨이브 크기
        public int waveSizeGrowth;      // 웨이브마다 증가량
        public int waveSizeMax;         // 웨이브 최대 크기
        public int totalBudget;         // 총 소환 수
        public int maxAliveCap;         // 동시에 살아있는 최대 수
    }
}