using TMPro;
using UnityEngine;

public class PopupUpgradeTotalCoinView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCoinText;
    [SerializeField] private string labelFormatStringKey = "UI_UPGRADE_TOTAL_COIN_FORMAT";

    private void Awake()
    {
        if (totalCoinText == null)
        {
            totalCoinText = GetComponent<TextMeshProUGUI>();
        }

        Refresh();
    }

    private void OnEnable()
    {
        TotalCoinPersistence.Instance.TotalCoinsChanged += HandleTotalCoinsChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (TotalCoinPersistence.Instance != null)
        {
            TotalCoinPersistence.Instance.TotalCoinsChanged -= HandleTotalCoinsChanged;
        }
    }

    private void HandleTotalCoinsChanged(int totalCoins, string _)
    {
        SetText(totalCoins);
    }

    private void Refresh()
    {
        SetText(TotalCoinPersistence.Instance.TotalCoins);
    }

    private void SetText(int amount)
    {
        if (totalCoinText == null)
        {
            return;
        }

        totalCoinText.text = LocalizationManager.GetTextFormat(labelFormatStringKey, Mathf.Max(0, amount));
    }
}
