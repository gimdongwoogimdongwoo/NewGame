using TMPro;
using UnityEngine;

public class PopupUpgradeTotalCoinView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCoinText;
    [SerializeField] private string labelFormat = "Total Coin: {0}";

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

        totalCoinText.text = string.Format(labelFormat, Mathf.Max(0, amount));
    }
}
