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
        EnsureRows();
    }

    private void OnEnable()
    {
        EnsureRows();
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

    private void EnsureRows()
    {
        if (statRows.Count == 0)
        {
            statRows.AddRange(GetComponentsInChildren<UpgradeStatRowUI>(true));
        }

        if (statRows.Count > 0)
        {
            return;
        }

        CreateGeneratedRows();
    }

    private void CreateGeneratedRows()
    {
        GameObject containerGo = new GameObject("GeneratedUpgradeRows", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        containerGo.transform.SetParent(transform, false);

        RectTransform container = containerGo.GetComponent<RectTransform>();
        container.anchorMin = new Vector2(0.1f, 0.15f);
        container.anchorMax = new Vector2(0.9f, 0.8f);
        container.offsetMin = Vector2.zero;
        container.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = containerGo.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 10f;

        ContentSizeFitter fitter = containerGo.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (UpgradeStatType stat in UpgradeSystem.Instance.GetAllStats())
        {
            UpgradeStatRowUI row = BuildRow(container, stat);
            statRows.Add(row);
        }
    }

    private static UpgradeStatRowUI BuildRow(Transform parent, UpgradeStatType stat)
    {
        GameObject rowGo = new GameObject($"StatRow_{stat}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        rowGo.transform.SetParent(parent, false);
        rowGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

        RectTransform rowRt = rowGo.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(0f, 70f);

        HorizontalLayoutGroup rowLayout = rowGo.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 8f;
        rowLayout.padding = new RectOffset(8, 8, 8, 8);
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childForceExpandHeight = true;

        TextMeshProUGUI statusName = CreateText(rowGo.transform, "Text_StatusName", 22, TextAlignmentOptions.Left);
        statusName.rectTransform.sizeDelta = new Vector2(180f, 54f);

        GameObject dotsWrap = new GameObject("Dots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        dotsWrap.transform.SetParent(rowGo.transform, false);
        HorizontalLayoutGroup dotsLayout = dotsWrap.GetComponent<HorizontalLayoutGroup>();
        dotsLayout.spacing = 4f;
        dotsLayout.childControlHeight = true;
        dotsLayout.childControlWidth = true;
        dotsLayout.childForceExpandWidth = false;
        dotsLayout.childForceExpandHeight = false;

        Image[] dots = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject dotGo = new GameObject($"Dot{i + 1}", typeof(RectTransform), typeof(Image));
            dotGo.transform.SetParent(dotsWrap.transform, false);
            Image image = dotGo.GetComponent<Image>();
            image.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            dots[i] = image;

            RectTransform dotRt = dotGo.GetComponent<RectTransform>();
            dotRt.sizeDelta = new Vector2(10f, 10f);
        }

        TextMeshProUGUI priceText = CreateText(rowGo.transform, "TXT_Price", 20, TextAlignmentOptions.Center);
        priceText.rectTransform.sizeDelta = new Vector2(90f, 54f);

        GameObject buttonGo = new GameObject("BTN_Price", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(rowGo.transform, false);
        buttonGo.GetComponent<Image>().color = new Color(0.16f, 0.58f, 0.22f, 0.95f);
        RectTransform btnRt = buttonGo.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(110f, 54f);
        CreateText(buttonGo.transform, "Label", 18, TextAlignmentOptions.Center).text = "UP";
        Button btn = buttonGo.GetComponent<Button>();

        GameObject tagMax = new GameObject("Tag_Max", typeof(RectTransform));
        tagMax.transform.SetParent(rowGo.transform, false);
        TextMeshProUGUI maxText = CreateText(tagMax.transform, "Text", 18, TextAlignmentOptions.Center);
        maxText.text = "MAX";
        tagMax.SetActive(false);

        UpgradeStatRowUI row = rowGo.AddComponent<UpgradeStatRowUI>();
        row.ConfigureGenerated(stat, statusName, priceText, btn, tagMax, dots);
        return row;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, float size, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = align;
        text.color = Color.white;
        text.text = string.Empty;
        return text;
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

    public void ConfigureGenerated(
        UpgradeStatType type,
        TextMeshProUGUI statusName,
        TextMeshProUGUI price,
        Button button,
        GameObject maxTag,
        Image[] generatedDots)
    {
        statType = type;
        textStatusName = statusName;
        textPrice = price;
        btnPrice = button;
        tagMax = maxTag;
        dots = generatedDots ?? Array.Empty<Image>();
        autoResolvedStat = true;
    }

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

            bool completed = i < level;
            if (dot0Sprite != null && dot1Sprite != null)
            {
                dots[i].sprite = completed ? dot1Sprite : dot0Sprite;
            }
            else
            {
                dots[i].color = completed ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.45f, 0.45f, 0.45f, 1f);
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
