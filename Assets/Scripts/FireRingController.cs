using System.Collections.Generic;
using UnityEngine;

public class FireRingController : MonoBehaviour
{
    [Header("Fire Ring Settings")]
    [Tooltip("궤도 지름(월드 단위). 실제 회전 반경은 OrbitRadius * 0.5 입니다.")]
    [SerializeField] private float orbitRadius = 2f;
    [Tooltip("초당 회전 속도(도/초)")]
    [SerializeField] private float orbitSpeed = 90f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private GameObject fireOrbPrefab;
    [SerializeField] private int initialOrbCount;

    private readonly List<Transform> spawnedOrbs = new();
    private float currentAngleOffset;
    private int activeOrbCount;

    public float OrbitRadius
    {
        get => orbitRadius;
        set => orbitRadius = Mathf.Max(0f, value);
    }

    public float OrbitSpeed
    {
        get => orbitSpeed;
        set => orbitSpeed = value;
    }

    public float DamageMultiplier
    {
        get => damageMultiplier;
        set
        {
            damageMultiplier = Mathf.Max(0f, value);
            ApplyDamageMultiplierToAllOrbs();
        }
    }

    public GameObject FireOrbPrefab
    {
        get => fireOrbPrefab;
        set
        {
            if (fireOrbPrefab == value)
            {
                return;
            }

            fireOrbPrefab = value;
            RebuildOrbs();
        }
    }

    private void Start()
    {
        if (initialOrbCount > 0)
        {
            Activate(initialOrbCount);
        }
    }

    private void Update()
    {
        if (spawnedOrbs.Count == 0)
        {
            return;
        }

        currentAngleOffset += orbitSpeed * Time.deltaTime;
        UpdateOrbTransforms();
    }

    public void Activate(int requestedOrbCount)
    {
        int targetCount = Mathf.Max(1, requestedOrbCount);
        activeOrbCount = Mathf.Max(activeOrbCount, targetCount);

        if (fireOrbPrefab == null)
        {
            Debug.LogWarning("FireRingController: FireOrbPrefab이 비어 있어 불덩이를 생성할 수 없습니다.");
            return;
        }

        EnsureOrbCount(activeOrbCount);
        UpdateOrbTransforms();
    }

    public void SetOrbCount(int orbCount)
    {
        activeOrbCount = Mathf.Max(0, orbCount);
        EnsureOrbCount(activeOrbCount);
        UpdateOrbTransforms();
    }

    private void RebuildOrbs()
    {
        EnsureOrbCount(0);

        if (activeOrbCount > 0)
        {
            EnsureOrbCount(activeOrbCount);
            UpdateOrbTransforms();
        }
    }

    private void EnsureOrbCount(int targetCount)
    {
        targetCount = Mathf.Max(0, targetCount);

        while (spawnedOrbs.Count > targetCount)
        {
            int lastIndex = spawnedOrbs.Count - 1;
            Transform orb = spawnedOrbs[lastIndex];
            spawnedOrbs.RemoveAt(lastIndex);

            if (orb != null)
            {
                Destroy(orb.gameObject);
            }
        }

        while (spawnedOrbs.Count < targetCount)
        {
            if (fireOrbPrefab == null)
            {
                break;
            }

            GameObject orbObject = Instantiate(fireOrbPrefab, transform.position, Quaternion.identity, transform);
            FireOrbDamageDealer dealer = orbObject.GetComponent<FireOrbDamageDealer>();
            if (dealer == null)
            {
                dealer = orbObject.AddComponent<FireOrbDamageDealer>();
            }

            dealer.DamageMultiplier = damageMultiplier;
            spawnedOrbs.Add(orbObject.transform);
        }

        ApplyDamageMultiplierToAllOrbs();
    }

    private void ApplyDamageMultiplierToAllOrbs()
    {
        for (int i = 0; i < spawnedOrbs.Count; i++)
        {
            Transform orb = spawnedOrbs[i];
            if (orb == null)
            {
                continue;
            }

            FireOrbDamageDealer dealer = orb.GetComponent<FireOrbDamageDealer>();
            if (dealer != null)
            {
                dealer.DamageMultiplier = damageMultiplier;
            }
        }
    }

    private void UpdateOrbTransforms()
    {
        int count = spawnedOrbs.Count;
        if (count == 0)
        {
            return;
        }

        float ringRadius = orbitRadius * 0.5f;
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            Transform orb = spawnedOrbs[i];
            if (orb == null)
            {
                continue;
            }

            float angle = currentAngleOffset + step * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 localOffset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * ringRadius;

            orb.position = transform.position + localOffset;
        }
    }
}
