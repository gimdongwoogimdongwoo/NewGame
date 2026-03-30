using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelUpPanelController : MonoBehaviour
{
    public static LevelUpPanelController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject cardContainer;
    [SerializeField] private Button[] cardButtons = new Button[3];

    [Header("Card UI Child Names")]
    [SerializeField] private string iconObjectName = "Image_Icon";
    [SerializeField] private string descriptionObjectName = "Text_Description";

    [Header("Data")]
    [SerializeField] private string levelUpCardResourcePath = "LevelUpCard";

    private readonly Queue<int> pendingLevelUps = new();
    private readonly UnityAction[] buttonHandlers = new UnityAction[3];
    private readonly HashSet<int> selectedCardIds = new();
    private readonly HashSet<int> selectedHistoryIds = new();
    private readonly List<LevelUpCardRow> allCards = new();

    private readonly CardSlotUI[] cardSlotUis = new CardSlotUI[3];
    private readonly LevelUpCardRow[] currentDrawCards = new LevelUpCardRow[3];

    private bool isPanelOpen;
    private int currentLevelRequest;

    public bool IsSelectionOpen => isPanelOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        LevelUpPanelController existing = FindFirstObjectByType<LevelUpPanelController>();
        if (existing != null)
        {
            Instance = existing;
            return;
        }

        GameObject bootstrap = new GameObject("LevelUpPanelController");
        bootstrap.AddComponent<LevelUpPanelController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveReferences();
        BindCardButtons();
        ResolveCardSlotUis();
        LoadCardTable();
        SetPanelVisible(false);
    }

    private void Update()
    {
        if (!isPanelOpen || cardContainer == null)
        {
            return;
        }

        if (!cardContainer.activeSelf)
        {
            cardContainer.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        UnbindCardButtons();
        GameplayPauseController.ResumeFromLevelUp();
    }

    public void EnqueueLevelUpSelection(int level)
    {
        pendingLevelUps.Enqueue(Mathf.Max(1, level));
        TryOpenNextPanel();
    }

    private void TryOpenNextPanel()
    {
        if (isPanelOpen || pendingLevelUps.Count == 0)
        {
            return;
        }

        currentLevelRequest = pendingLevelUps.Dequeue();
        isPanelOpen = true;

        DrawCardsForCurrentPanel();
        SetPanelVisible(true);
        GameplayPauseController.PauseForLevelUp();
    }

    private void DrawCardsForCurrentPanel()
    {
        for (int i = 0; i < currentDrawCards.Length; i++)
        {
            currentDrawCards[i] = null;
        }

        List<LevelUpCardRow> candidates = BuildCandidates();
        if (candidates.Count == 0)
        {
            Debug.LogWarning("LevelUpPanelController: 조건을 만족하는 레벨업 카드가 없습니다.");
            ApplyFallbackCardUi();
            return;
        }

        List<LevelUpCardRow> drawPool = new(candidates);
        int drawCount = Mathf.Min(cardButtons.Length, drawPool.Count);

        for (int i = 0; i < drawCount; i++)
        {
            LevelUpCardRow picked = WeightedPickAndRemove(drawPool);
            currentDrawCards[i] = picked;
            BindCardToSlot(i, picked, true);
        }

        for (int i = drawCount; i < cardButtons.Length; i++)
        {
            BindCardToSlot(i, null, false);
        }
    }

    private List<LevelUpCardRow> BuildCandidates()
    {
        List<LevelUpCardRow> candidates = new();

        for (int i = 0; i < allCards.Count; i++)
        {
            LevelUpCardRow row = allCards[i];
            if (selectedCardIds.Contains(row.Id))
            {
                continue;
            }

            if (row.RequireId.HasValue && !selectedHistoryIds.Contains(row.RequireId.Value))
            {
                continue;
            }

            candidates.Add(row);
        }

        return candidates;
    }

    private static LevelUpCardRow WeightedPickAndRemove(List<LevelUpCardRow> pool)
    {
        int totalWeight = 0;
        for (int i = 0; i < pool.Count; i++)
        {
            totalWeight += Mathf.Max(1, pool[i].Ratio);
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int acc = 0;
        for (int i = 0; i < pool.Count; i++)
        {
            acc += Mathf.Max(1, pool[i].Ratio);
            if (roll < acc)
            {
                LevelUpCardRow result = pool[i];
                pool.RemoveAt(i);
                return result;
            }
        }

        LevelUpCardRow fallback = pool[pool.Count - 1];
        pool.RemoveAt(pool.Count - 1);
        return fallback;
    }

    private void HandleCardSelected(int cardIndex)
    {
        if (!isPanelOpen || cardIndex < 0 || cardIndex >= currentDrawCards.Length)
        {
            return;
        }

        LevelUpCardRow selectedRow = currentDrawCards[cardIndex];
        if (selectedRow == null)
        {
            return;
        }

        selectedCardIds.Add(selectedRow.Id);
        selectedHistoryIds.Add(selectedRow.Id);

        ApplyCardEffect(selectedRow);

        Debug.Log($"LevelUpPanelController: Level {currentLevelRequest} card selected. ID={selectedRow.Id}, Effect={selectedRow.Effect}, Value={selectedRow.ValueToString}");

        isPanelOpen = false;
        currentLevelRequest = 0;
        SetPanelVisible(false);
        GameplayPauseController.ResumeFromLevelUp();

        StartCoroutine(OpenNextPanelNextFrame());
    }

    private void ApplyCardEffect(LevelUpCardRow card)
    {
        if (string.IsNullOrWhiteSpace(card.Effect))
        {
            Debug.LogWarning($"LevelUpPanelController: ID={card.Id} 카드에 Effect가 비어있어 적용을 건너뜁니다.");
            return;
        }

        LevelUpEffectService.Apply(card.Effect, card.Value);
    }

    private IEnumerator OpenNextPanelNextFrame()
    {
        yield return null;
        TryOpenNextPanel();
    }

    private void ResolveReferences()
    {
        if (cardContainer == null)
        {
            GameObject foundContainer = GameObject.Find("CardContainer");
            if (foundContainer != null)
            {
                cardContainer = foundContainer;
            }
        }

        if (cardButtons != null && cardButtons.Length >= 3 &&
            cardButtons[0] != null && cardButtons[1] != null && cardButtons[2] != null)
        {
            return;
        }

        List<Button> resolvedButtons = new();
        if (cardContainer != null)
        {
            resolvedButtons.AddRange(cardContainer.GetComponentsInChildren<Button>(true));
        }
        else
        {
            resolvedButtons.AddRange(GetComponentsInChildren<Button>(true));
        }

        if (resolvedButtons.Count >= 3)
        {
            cardButtons = new[] { resolvedButtons[0], resolvedButtons[1], resolvedButtons[2] };
        }
    }

    private void ResolveCardSlotUis()
    {
        for (int i = 0; i < cardSlotUis.Length && i < cardButtons.Length; i++)
        {
            cardSlotUis[i] = CardSlotUI.FromButton(cardButtons[i], iconObjectName, descriptionObjectName);
        }
    }

    private void BindCardButtons()
    {
        if (cardButtons == null || cardButtons.Length == 0)
        {
            return;
        }

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (cardButtons[i] == null)
            {
                continue;
            }

            int captured = i;
            buttonHandlers[i] = () => HandleCardSelected(captured);
            cardButtons[i].onClick.AddListener(buttonHandlers[i]);
        }
    }

    private void UnbindCardButtons()
    {
        if (cardButtons == null)
        {
            return;
        }

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (cardButtons[i] == null)
            {
                continue;
            }

            if (buttonHandlers[i] != null)
            {
                cardButtons[i].onClick.RemoveListener(buttonHandlers[i]);
                buttonHandlers[i] = null;
            }
        }
    }

    private void BindCardToSlot(int slotIndex, LevelUpCardRow card, bool interactable)
    {
        if (slotIndex < 0 || slotIndex >= cardButtons.Length)
        {
            return;
        }

        Button button = cardButtons[slotIndex];
        if (button != null)
        {
            button.interactable = interactable;
            button.gameObject.SetActive(true);
        }

        CardSlotUI slotUi = cardSlotUis[slotIndex];
        if (slotUi == null)
        {
            return;
        }

        if (card == null)
        {
            slotUi.SetIcon(null);
            slotUi.SetDescription(string.Empty);
            return;
        }

        slotUi.SetIcon(card.LoadIconSprite());
        slotUi.SetDescription(card.Desc);
    }

    private void ApplyFallbackCardUi()
    {
        for (int i = 0; i < cardButtons.Length; i++)
        {
            BindCardToSlot(i, null, false);
            if (cardSlotUis[i] != null)
            {
                cardSlotUis[i].SetDescription("카드 데이터가 없습니다");
            }
        }
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (cardContainer == null)
        {
            return;
        }

        cardContainer.SetActive(isVisible);
    }

    private void LoadCardTable()
    {
        allCards.Clear();

        TextAsset csvAsset = Resources.Load<TextAsset>(levelUpCardResourcePath);
        if (csvAsset == null)
        {
            Debug.LogWarning($"LevelUpPanelController: Resources/{levelUpCardResourcePath}.csv 파일을 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }

            string[] cols = line.Split(',');
            if (cols.Length < 7)
            {
                Debug.LogWarning($"LevelUpPanelController: 잘못된 LevelUpCard 행을 무시합니다. line={line}");
                continue;
            }

            if (!int.TryParse(cols[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            {
                continue;
            }

            if (!int.TryParse(cols[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int ratio))
            {
                ratio = 1;
            }

            int? requireId = null;
            string requireRaw = cols[2].Trim();
            if (!string.IsNullOrWhiteSpace(requireRaw) && int.TryParse(requireRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedRequire))
            {
                requireId = parsedRequire;
            }

            string icon = cols[3].Trim();
            string desc = cols[4].Trim();
            string effect = cols[5].Trim();

            int? value = null;
            string valueRaw = cols[6].Trim();
            if (!string.IsNullOrWhiteSpace(valueRaw) && int.TryParse(valueRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue))
            {
                value = parsedValue;
            }

            allCards.Add(new LevelUpCardRow(id, Mathf.Max(1, ratio), requireId, icon, desc, effect, value));
        }

        if (allCards.Count == 0)
        {
            Debug.LogWarning("LevelUpPanelController: LevelUpCard 데이터가 비어 있습니다.");
        }
    }

    private sealed class CardSlotUI
    {
        private readonly Image iconImage;
        private readonly TMP_Text descriptionTmp;
        private readonly Text descriptionText;

        private CardSlotUI(Image iconImage, TMP_Text descriptionTmp, Text descriptionText)
        {
            this.iconImage = iconImage;
            this.descriptionTmp = descriptionTmp;
            this.descriptionText = descriptionText;
        }

        public static CardSlotUI FromButton(Button button, string iconName, string descriptionName)
        {
            if (button == null)
            {
                return null;
            }

            Transform iconTransform = button.transform.Find(iconName);
            Transform descTransform = button.transform.Find(descriptionName);

            Image icon = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
            TMP_Text tmp = descTransform != null ? descTransform.GetComponent<TMP_Text>() : null;
            Text txt = descTransform != null ? descTransform.GetComponent<Text>() : null;

            return new CardSlotUI(icon, tmp, txt);
        }

        public void SetIcon(Sprite sprite)
        {
            if (iconImage == null)
            {
                return;
            }

            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        public void SetDescription(string value)
        {
            if (descriptionTmp != null)
            {
                descriptionTmp.text = value;
            }

            if (descriptionText != null)
            {
                descriptionText.text = value;
            }
        }
    }

    private sealed class LevelUpCardRow
    {
        public int Id { get; }
        public int Ratio { get; }
        public int? RequireId { get; }
        public string Icon { get; }
        public string Desc { get; }
        public string Effect { get; }
        public int? Value { get; }
        public string ValueToString => Value.HasValue ? Value.Value.ToString(CultureInfo.InvariantCulture) : "";

        public LevelUpCardRow(int id, int ratio, int? requireId, string icon, string desc, string effect, int? value)
        {
            Id = id;
            Ratio = ratio;
            RequireId = requireId;
            Icon = icon;
            Desc = desc;
            Effect = effect;
            Value = value;
        }

        public Sprite LoadIconSprite()
        {
            if (string.IsNullOrWhiteSpace(Icon))
            {
                return null;
            }

            string iconPath = Icon;
            const string spritePrefix = "Sprite/";
            if (!iconPath.StartsWith(spritePrefix, StringComparison.OrdinalIgnoreCase))
            {
                iconPath = spritePrefix + iconPath;
            }

            int extensionIndex = iconPath.LastIndexOf('.');
            if (extensionIndex > 0)
            {
                iconPath = iconPath.Substring(0, extensionIndex);
            }

            return Resources.Load<Sprite>(iconPath);
        }
    }
}
