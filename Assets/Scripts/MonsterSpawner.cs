using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnPadding = 1f;
    [SerializeField] private float spawnOuterPadding = 3f;

    private readonly List<SpawnRuntimeState> runtimeStates = new();

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        ResolvePlayerReference();

        int stageId = StageCsvLoader.ResolveCurrentStageId();
        List<StageMonsterSpawnRule> stageRules = StageCsvLoader.LoadStageMonsterRules(stageId);

        foreach (StageMonsterSpawnRule rule in stageRules)
        {
            GameObject prefab = Resources.Load<GameObject>(rule.MonsterId);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>($"Prefabs/{rule.MonsterId}");
            }

            if (prefab == null)
            {
                Debug.LogWarning($"Monster prefab '{rule.MonsterId}' was not found under Resources. Skipping this rule.");
                continue;
            }

            SpawnRuntimeState state = new SpawnRuntimeState(rule, prefab);
            runtimeStates.Add(state);
            StartCoroutine(RunSpawnLoop(state));
        }
    }

    private IEnumerator RunSpawnLoop(SpawnRuntimeState state)
    {
        yield return new WaitForSeconds(state.Rule.SpawnStartSec);

        while (state.SpawnedTotal < state.Rule.TotalBudget)
        {
            int waveSize = Mathf.Min(
                state.Rule.WaveSizeStart + (state.WaveIndex * state.Rule.WaveSizeGrowth),
                state.Rule.WaveSizeMax);

            int remainingBudget = state.Rule.TotalBudget - state.SpawnedTotal;
            int aliveCapacity = state.Rule.MaxAliveCap - state.AliveCount;
            int spawnCount = Mathf.Min(waveSize, remainingBudget, aliveCapacity);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnMonster(state);
            }

            state.WaveIndex++;
            yield return new WaitForSeconds(state.Rule.WaveIntervalSec);
        }
    }

    private void SpawnMonster(SpawnRuntimeState state)
    {
        Vector2 spawnPosition = ResolveSpawnPosition();
        GameObject monster = Instantiate(state.Prefab, spawnPosition, Quaternion.identity);

        MonsterChasePlayer chase = monster.GetComponent<MonsterChasePlayer>();
        if (chase == null)
        {
            chase = monster.AddComponent<MonsterChasePlayer>();
        }

        chase.SetTarget(player);

        SpawnedMonsterLifetime lifetime = monster.GetComponent<SpawnedMonsterLifetime>();
        if (lifetime == null)
        {
            lifetime = monster.AddComponent<SpawnedMonsterLifetime>();
        }

        lifetime.Initialize(() => state.AliveCount = Mathf.Max(0, state.AliveCount - 1));

        state.SpawnedTotal++;
        state.AliveCount++;
    }

    private Vector2 ResolveSpawnPosition()
    {
        if (player == null)
        {
            ResolvePlayerReference();
            if (player == null)
            {
                return Vector2.zero;
            }
        }

        if (targetCamera == null)
        {
            return player.position;
        }

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        float minRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight)) + spawnPadding;

        for (int i = 0; i < 8; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir == Vector2.zero)
            {
                dir = Vector2.right;
            }

            float distance = Random.Range(minRadius, minRadius + spawnOuterPadding);
            Vector2 candidate = (Vector2)player.position + (dir * distance);

            if (MapBoundaryController.Instance != null)
            {
                candidate = MapBoundaryController.Instance.ClampPosition(candidate);
            }

            if (Vector2.Distance(player.position, candidate) >= minRadius * 0.85f)
            {
                return candidate;
            }
        }

        Vector2 fallback = (Vector2)player.position + (Vector2.right * minRadius);
        return MapBoundaryController.Instance != null
            ? MapBoundaryController.Instance.ClampPosition(fallback)
            : fallback;
    }

    private void ResolvePlayerReference()
    {
        if (player != null)
        {
            return;
        }

        GameObject tagged = GameObject.FindGameObjectWithTag("player");
        if (tagged != null)
        {
            player = tagged.transform;
            return;
        }

        PlayerMovement2D playerMovement = FindFirstObjectByType<PlayerMovement2D>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }
    }

    private sealed class SpawnRuntimeState
    {
        public StageMonsterSpawnRule Rule { get; }
        public GameObject Prefab { get; }
        public int WaveIndex { get; set; }
        public int SpawnedTotal { get; set; }
        public int AliveCount { get; set; }

        public SpawnRuntimeState(StageMonsterSpawnRule rule, GameObject prefab)
        {
            Rule = rule;
            Prefab = prefab;
            WaveIndex = 0;
            SpawnedTotal = 0;
            AliveCount = 0;
        }
    }
}
