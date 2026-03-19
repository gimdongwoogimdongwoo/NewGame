using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;

public class MonsterSpawner : MonoBehaviour
{

    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform player;
    [FormerlySerializedAs("spawnPadding")]
    [SerializeField] private float spawnRadiusPadding = 1f;
    [SerializeField] private float spawnOuterPadding = 3f;

    [SerializeField] private List<GameObject> monsterPrefabs = new List<GameObject>();

    private readonly List<SpawnRuntimeState> runtimeStates = new List<SpawnRuntimeState>();


    [SerializeField] private List<MonsterPrefabEntry> monsterPrefabs = new();


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

        if (player == null)
        {
            Debug.LogError("MonsterSpawner could not find PlayerMovement2D target. Assign player Transform in Inspector.");
            return;
        }


        int activeStageId = overrideStageId > 0 ? overrideStageId : StageCsvLoader.ResolveCurrentStageId();
        List<StageMonsterSpawnRule> stageRules = StageCsvLoader.LoadStageMonsterRules(activeStageId);


        int resolvedStageId = stageId > 0 ? stageId : StageCsvLoader.ResolveCurrentStageId();
        List<StageMonsterSpawnRule> stageRules = StageCsvLoader.LoadStageMonsterRules(resolvedStageId);


        foreach (StageMonsterSpawnRule rule in stageRules)
        {
            GameObject prefab = FindPrefab(rule.MonsterId);



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


            SpawnRuntimeState runtimeState = new SpawnRuntimeState(rule, prefab);
            runtimeStates.Add(runtimeState);
            StartCoroutine(RunSpawnLoop(runtimeState));
        }
    }

    private IEnumerator RunSpawnLoop(SpawnRuntimeState runtimeState)
    {
        yield return new WaitForSeconds(runtimeState.Rule.SpawnStartSec);

        while (runtimeState.SpawnedTotal < runtimeState.Rule.TotalBudget)
        {
            int waveSize = Mathf.Min(
                runtimeState.Rule.WaveSizeStart + (runtimeState.WaveIndex * runtimeState.Rule.WaveSizeGrowth),
                runtimeState.Rule.WaveSizeMax);

            int remainingBudget = runtimeState.Rule.TotalBudget - runtimeState.SpawnedTotal;
            int aliveCapacity = runtimeState.Rule.MaxAliveCap - runtimeState.AliveCount;

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

                SpawnMonster(runtimeState);
            }

            runtimeState.WaveIndex++;
            yield return new WaitForSeconds(runtimeState.Rule.WaveIntervalSec);
        }
    }

    private void SpawnMonster(SpawnRuntimeState runtimeState)
    {
        Vector2 spawnPosition = ResolveSpawnPosition();
        GameObject monster = Instantiate(runtimeState.Prefab, spawnPosition, Quaternion.identity);

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


        lifetime.Initialize(() => runtimeState.AliveCount = Mathf.Max(0, runtimeState.AliveCount - 1));

        runtimeState.SpawnedTotal++;
        runtimeState.AliveCount++;

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

        float minRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight)) + spawnRadiusPadding;


        float minRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight)) + spawnRadiusPadding;



        float minRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight)) + spawnRadiusPadding;

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


        PlayerMovement2D playerMovement = FindObjectOfType<PlayerMovement2D>();


        PlayerMovement2D playerMovement = FindObjectOfType<PlayerMovement2D>();

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




    private GameObject FindPrefab(string monsterId)
    {
        foreach (GameObject prefabEntry in monsterPrefabs)
        {
            if (prefabEntry != null && prefabEntry.name == monsterId)
            {
                return prefabEntry;

            }
        }

        GameObject loadedPrefab = Resources.Load<GameObject>(monsterId);
        if (loadedPrefab == null)
        {
            loadedPrefab = Resources.Load<GameObject>($"Prefabs/{monsterId}");
        }

        return loadedPrefab;
    }




    private GameObject FindPrefab(string monsterId)
    {
        foreach (MonsterPrefabEntry entry in monsterPrefabs)
        {
            if (!string.IsNullOrWhiteSpace(entry.MonsterId) &&
                entry.Prefab != null &&
                entry.MonsterId == monsterId)
            {
                return entry.Prefab;

            }
        }

        GameObject prefab = Resources.Load<GameObject>(monsterId);
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>($"Prefabs/{monsterId}");
        }

        return prefab;
    }


    [System.Serializable]
    private struct MonsterPrefabEntry
    {
        public string MonsterId;
        public GameObject Prefab;
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
