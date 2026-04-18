using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePopupUI : MonoBehaviour
{
    [SerializeField] private List<UpgradeStatRowUI> statRows = new();

    private void Awake()
    {
        if (statRows.Count == 0)
        {
            statRows.AddRange(GetComponentsInChildren<UpgradeStatRowUI>(true));
        }
    }

    private void OnEnable()
    {
        UpgradeSystem.Instance.UpgradesChanged += RefreshAll;
        TotalCoinPersistence.Instance.TotalCoinsChanged += HandleCoinChanged;

        BindRows();
        RefreshAll();
    }

    private void OnDisable()
    {
        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.UpgradesChanged -= RefreshAll;
        }

        if (TotalCoinPersistence.Instance != null)
        {
            TotalCoinPersistence.Instance.TotalCoinsChanged -= HandleCoinChanged;
        }

        for (int i = 0; i < statRows.Count; i++)
        {
            statRows[i]?.Unbind();
        }
    }

    public void RefreshAll()
    {
        for (int i = 0; i < statRows.Count; i++)
        {
            statRows[i]?.Refresh();
        }
    }

    private void HandleCoinChanged(int _, string __)
    {
        RefreshAll();
    }

    private void BindRows()
    {
        for (int i = 0; i < statRows.Count; i++)
        {
            statRows[i]?.Bind();
        }
    }
}

public class UpgradeStatRowUI : MonoBehaviour
{
    [SerializeField] private UpgradeStatType statType;
    [SerializeField] private TextMeshProUGUI textStatusName;
    [SerializeField] private TextMeshProUGUI textPrice;
    [SerializeField] private Button btnPrice;
    [SerializeField] private GameObject tagMax;
    [SerializeField] private Image[] dots = Array.Empty<Image>();
    [SerializeField] private Sprite dot0Sprite;
    [SerializeField] private Sprite dot1Sprite;

    private bool isBound;
    private bool autoResolvedStat;

    public void Bind()
    {
        ResolveRefs();
        if (btnPrice != null && !isBound)
        {
            btnPrice.onClick.RemoveListener(HandlePriceClicked);
            btnPrice.onClick.AddListener(HandlePriceClicked);
            isBound = true;
        }
    }

    public void Unbind()
    {
        if (btnPrice != null)
        {
            btnPrice.onClick.RemoveListener(HandlePriceClicked);
        }

        isBound = false;
    }

    public void Refresh()
    {
        ResolveRefs();

        UpgradeSystem upgrade = UpgradeSystem.Instance;
        int level = upgrade.GetCurrentLevel(statType);
        int maxLevel = upgrade.GetMaxLevel(statType);
        bool isMax = level >= maxLevel;

        if (textStatusName != null)
        {
            textStatusName.text = upgrade.GetStatName(statType);
        }

        if (textPrice != null)
        {
            textPrice.text = isMax ? "MAX" : upgrade.GetNextCoinCost(statType).ToString();
        }

        if (btnPrice != null)
        {
            int nextCost = upgrade.GetNextCoinCost(statType);
            btnPrice.interactable = !isMax && TotalCoinPersistence.Instance.TotalCoins >= nextCost;
        }

        if (tagMax != null)
        {
            tagMax.SetActive(isMax);
        }

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null)
            {
                continue;
            }

            if (dot0Sprite != null && dot1Sprite != null)
            {
                dots[i].sprite = i < level ? dot1Sprite : dot0Sprite;
            }
        }
    }

    private void HandlePriceClicked()
    {
        if (!UpgradeSystem.Instance.TryUpgrade(statType))
        {
            Refresh();
            return;
        }

        Refresh();
    }

    private void ResolveRefs()
    {
        if (!autoResolvedStat)
        {
            autoResolvedStat = TryInferStatTypeFromName();
        }

        if (textStatusName == null)
        {
            textStatusName = FindTextByName("Text_StatusName", "TXT_StatusName", "StatusName");
        }

        if (textPrice == null)
        {
            textPrice = FindTextByName("TXT_Price", "Text_Price", "Price");
        }

        if (btnPrice == null)
        {
            btnPrice = FindButtonByName("BTN_Price", "Btn_Price", "Price");
        }

        if (tagMax == null)
        {
            Transform maxTr = transform.Find("Tag_Max");
            tagMax = maxTr != null ? maxTr.gameObject : null;
        }

        if (dots == null || dots.Length == 0)
        {
            List<Image> foundDots = new();
            for (int i = 1; i <= 5; i++)
            {
                Transform dot = transform.Find($"Dot{i}");
                if (dot != null && dot.TryGetComponent(out Image image))
                {
                    foundDots.Add(image);
                }
            }

            dots = foundDots.ToArray();
        }
    }


    private bool TryInferStatTypeFromName()
    {
        string n = gameObject.name;
        if (n.IndexOf("MoveSpeed", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Move", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            statType = UpgradeStatType.MoveSpeed;
            return true;
        }

        if (n.IndexOf("ATKSpeed", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("AttackSpeed", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            statType = UpgradeStatType.ATKSpeed;
            return true;
        }

        if (n.IndexOf("Revival", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            statType = UpgradeStatType.Revival;
            return true;
        }

        if (n.IndexOf("CRI", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Crit", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            statType = UpgradeStatType.CRI;
            return true;
        }

        if (n.IndexOf("HP", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            statType = UpgradeStatType.HP;
            return true;
        }

        if (n.IndexOf("ATK", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Attack", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            statType = UpgradeStatType.ATK;
            return true;
        }

        return false;
    }

    private TextMeshProUGUI FindTextByName(params string[] names)
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            for (int j = 0; j < names.Length; j++)
            {
                if (string.Equals(texts[i].gameObject.name, names[j], StringComparison.OrdinalIgnoreCase))
                {
                    return texts[i];
                }
            }
        }

        return null;
    }

    private Button FindButtonByName(params string[] names)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            for (int j = 0; j < names.Length; j++)
            {
                if (string.Equals(buttons[i].gameObject.name, names[j], StringComparison.OrdinalIgnoreCase))
                {
                    return buttons[i];
                }
            }
        }

        return null;
    }
}
