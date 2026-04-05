using UnityEngine;

public class AirController : MonoBehaviour
{
    [Header("Air Skill Settings")]
    [SerializeField] private AirProjectileController airPrefab;
    [SerializeField] private float knockbackDistance = 1.5f;
    [SerializeField] private float coolTime = 1.5f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private int initialProjectileCount;

    private int projectileCount;
    private float nextFireTime;

    public AirProjectileController AirPrefab
    {
        get => airPrefab;
        set => airPrefab = value;
    }

    public float KnockbackDistance
    {
        get => knockbackDistance;
        set => knockbackDistance = Mathf.Max(0f, value);
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
        if (initialProjectileCount > 0)
        {
            Activate(initialProjectileCount);
        }
    }

    private void Update()
    {
        if (GameplayPauseController.IsGameplayPaused || projectileCount <= 0 || airPrefab == null)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        FireProjectiles();
        nextFireTime = Time.time + coolTime;
    }

    public void Activate(int count)
    {
        int valid = Mathf.Clamp(count, 1, 4);
        projectileCount = Mathf.Max(projectileCount, valid);
        nextFireTime = Time.time;
    }

    private void FireProjectiles()
    {
        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir.sqrMagnitude <= 0.0001f)
            {
                dir = Vector2.right;
            }

            AirProjectileController projectile = Instantiate(airPrefab, transform.position, Quaternion.identity);
            projectile.Initialize(dir, speed, knockbackDistance);
        }
    }
}
