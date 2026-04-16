using System.Collections;
using UnityEngine;

public class ExplosionEffectController : MonoBehaviour
{
    [SerializeField] private float effectSize = 2f;
    [SerializeField] private float effectLifeTime = 0.35f;

    private SpriteRenderer[] spriteRenderers;
    private Color[] initialColors;

    public float EffectSize
    {
        get => effectSize;
        set => effectSize = Mathf.Max(0.01f, value);
    }

    public float EffectLifeTime
    {
        get => effectLifeTime;
        set => effectLifeTime = Mathf.Max(0.01f, value);
    }

    private void Awake()
    {
        CacheSpriteRenderers();
    }

    public void Play(Vector3 worldPosition)
    {
        gameObject.SetActive(true);
        transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        SkillSfxPlayer.PlayExplosionBoom();

        CacheSpriteRenderers();

        transform.localScale = Vector3.one * 0.1f;
        ApplyAlpha(1f);

        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        CacheSpriteRenderers();
        float elapsed = 0f;

        while (elapsed < effectLifeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / effectLifeTime);

            float size = Mathf.Lerp(0.1f, effectSize, t);
            transform.localScale = Vector3.one * size;
            ApplyAlpha(1f - t);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void CacheSpriteRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        initialColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            initialColors[i] = spriteRenderers[i].color;
        }
    }

    private void ApplyAlpha(float alpha)
    {
        if (spriteRenderers == null || initialColors == null)
        {
            return;
        }

        int count = Mathf.Min(spriteRenderers.Length, initialColors.Length);
        for (int i = 0; i < count; i++)
        {
            SpriteRenderer sr = spriteRenderers[i];
            if (sr == null)
            {
                continue;
            }

            Color baseColor = initialColors[i];
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha);
        }
    }
}
