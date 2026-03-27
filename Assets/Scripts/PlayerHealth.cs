using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float invincibleTime = 1f;

    [Header("UI")]
    [SerializeField] private Image hpImage;

    [Header("Hit Feedback")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private float blinkInterval = 0.1f;

    private float currentHP;
    private bool isInvincible;
    private Coroutine invincibleRoutine;

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (hpImage == null)
        {
            var hpObject = GameObject.Find("HPImage");
            if (hpObject != null)
            {
                hpImage = hpObject.GetComponent<Image>();
            }
        }

        currentHP = Mathf.Clamp(maxHP, 0f, maxHP);
        UpdateHpUI();
    }

    public bool TakeDamage(float damage)
    {
        if (GameplayPauseController.IsGameplayPaused || damage <= 0f || isInvincible || currentHP <= 0f)
        {
            return false;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        UpdateHpUI();

        if (currentHP <= 0f)
        {
            HandleDeath();
            return true;
        }

        StartInvincible();
        return true;
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        UpdateHpUI();
    }

    public void IncreaseMaxHP(float amount, bool fillAddedHealth = true)
    {
        if (amount <= 0f)
        {
            return;
        }

        maxHP += amount;

        if (fillAddedHealth)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
        }
        else
        {
            currentHP = Mathf.Min(currentHP, maxHP);
        }

        UpdateHpUI();
    }

    private void StartInvincible()
    {
        if (invincibleRoutine != null)
        {
            StopCoroutine(invincibleRoutine);
        }

        invincibleRoutine = StartCoroutine(InvincibleRoutine());
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        if (targetRenderer == null)
        {
            yield return new WaitForSeconds(invincibleTime);
            isInvincible = false;
            yield break;
        }

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibleTime)
        {
            visible = !visible;
            targetRenderer.enabled = visible;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        targetRenderer.enabled = true;
        isInvincible = false;
        invincibleRoutine = null;
    }

    private void UpdateHpUI()
    {
        if (hpImage == null)
        {
            return;
        }

        hpImage.fillAmount = maxHP > 0f ? currentHP / maxHP : 0f;
    }

    private void HandleDeath()
    {
        Debug.Log("Player is dead. Game Over.");
        // TODO: GameManager 등의 게임 오버 처리 연결.
    }
}
