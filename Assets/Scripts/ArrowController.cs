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

    private int arrowsPerShot;
    private float nextFireTime;

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
    }

    private void Update()
    {
        if (GameplayPauseController.IsGameplayPaused || arrowsPerShot <= 0 || arrowPrefab == null)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        FireArrows();
        nextFireTime = Time.time + coolTime;
    }

    public void Activate(int count)
    {
        int validCount = Mathf.Max(1, count);
        arrowsPerShot = Mathf.Max(arrowsPerShot, validCount);
        nextFireTime = Time.time;
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
