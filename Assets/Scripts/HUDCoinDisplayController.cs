using TMPro;
using UnityEngine;

public class HUDCoinDisplayController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private CoinManager coinManager;

    private void Awake()
    {
        ResolveReferences();
        RefreshText(coinManager != null ? coinManager.CurrentCoins : 0);
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (coinManager != null)
        {
            coinManager.CoinChanged += RefreshText;
            RefreshText(coinManager.CurrentCoins);
        }
    }

    private void OnDisable()
    {
        if (coinManager != null)
        {
            coinManager.CoinChanged -= RefreshText;
        }
    }

    private void ResolveReferences()
    {
        if (coinManager == null)
        {
            coinManager = CoinManager.Instance;
        }

        if (coinText == null)
        {
            coinText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void RefreshText(int amount)
    {
        if (coinText == null)
        {
            return;
        }

        coinText.text = LocalizationManager.GetTextFormat("UI_HUD_COIN_FORMAT", Mathf.Max(0, amount));
    }
}
