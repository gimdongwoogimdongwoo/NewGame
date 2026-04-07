using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private ArrowProjectileController arrowPrefab;
    [Tooltip("플레이어 현재 공격력에 곱해질 배율(예: 100=100%)")]
    [SerializeField] private float damageMultiplier = 100f;
    [SerializeField] private float coolTime = 1.5f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private int initialArrowCount;

    [Header("Air Settings")]
    [SerializeField] private AirProjectileController airPrefab;
    [SerializeField] private float knockbackDistance = 1.5f;
    [SerializeField] private float airCoolTime = 1.5f;
    [SerializeField] private float airSpeed = 8f;
    [SerializeField] private int initialAirCount;

    private int arrowsPerShot;
    private int airProjectilesPerShot;
    private float nextArrowFireTime;
    private float nextAirFireTime;

    public ArrowProjectileController ArrowPrefab
    {
        get => arrowPrefab;
        set => arrowPrefab = value;
    }

    public float DamageMultiplier
    {
        get => damageMultiplier;
        set => damageMultiplier = Mathf.Max(0f, value);
    }

    public float CoolTime
    {
        get => coolTime;
        set => coolTime = Mathf.Max(0.01f, value);
    }

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0f, value);
    }

    private void Start()
    {
        if (initialArrowCount > 0)
        {
            Activate(initialArrowCount);
        }

        if (initialAirCount > 0)
        {
            ActivateAir(initialAirCount);
        }
    }

    private void Update()
    {
        if (GameplayPauseController.IsGameplayPaused)
        {
            return;
        }

        TryFireArrows();
        TryFireAirProjectiles();
    }

    public void Activate(int count)
    {
        int validCount = Mathf.Max(1, count);
        arrowsPerShot = Mathf.Max(arrowsPerShot, validCount);
        nextArrowFireTime = Time.time;
    }

    public void ActivateAir(int count)
    {
        int validCount = Mathf.Clamp(count, 1, 4);
        airProjectilesPerShot = Mathf.Max(airProjectilesPerShot, validCount);
        nextAirFireTime = Time.time;
    }

    private void TryFireArrows()
    {
        if (arrowsPerShot <= 0 || arrowPrefab == null)
        {
            return;
        }

        if (Time.time < nextArrowFireTime)
        {
            return;
        }

        FireArrows();
        nextArrowFireTime = Time.time + coolTime;
    }

    private void FireArrows()
    {
        List<MonsterController> targets = FindNearestMonsters(transform.position, arrowsPerShot);

        for (int i = 0; i < arrowsPerShot; i++)
        {
            MonsterController target = i < targets.Count ? targets[i] : null;
            ArrowProjectileController arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            arrow.Initialize(target, speed, damageMultiplier);
        }
    }

    private void TryFireAirProjectiles()
    {
        if (airProjectilesPerShot <= 0 || airPrefab == null)
        {
            return;
        }

        if (Time.time < nextAirFireTime)
        {
            return;
        }

        for (int i = 0; i < airProjectilesPerShot; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            AirProjectileController projectile = Instantiate(airPrefab, transform.position, Quaternion.identity);
            projectile.Initialize(direction, airSpeed, knockbackDistance);
        }

        nextAirFireTime = Time.time + airCoolTime;
    }

    private static List<MonsterController> FindNearestMonsters(Vector3 origin, int count)
    {
        MonsterController[] all = Object.FindObjectsByType<MonsterController>(FindObjectsSortMode.None);
        List<MonsterController> monsters = new(all.Length);

        for (int i = 0; i < all.Length; i++)
        {
            MonsterController monster = all[i];
            if (monster == null || monster.IsDead)
            {
                continue;
            }

            monsters.Add(monster);
        }

        monsters.Sort((a, b) =>
        {
            float da = (a.transform.position - origin).sqrMagnitude;
            float db = (b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        });

        if (monsters.Count > count)
        {
            monsters.RemoveRange(count, monsters.Count - count);
        }

        return monsters;
    }
}
