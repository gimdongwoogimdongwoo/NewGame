using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneStageCardListController : MonoBehaviour
{
    [Header("Stage Card UI")]
    [SerializeField] private Transform stageCardPanel;
    [SerializeField] private GameObject stageCardPrefab;
    [SerializeField] private string stageImageObjectName = "StageImage";
    [SerializeField] private string stageTitleObjectName = "StageTitle";
    [SerializeField] private bool clearExistingChildren = true;
    [SerializeField] private string templateCardName = "StageCard";
    private readonly List<BoundStageCard> boundCards = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            return;
        }

        if (FindFirstObjectByType<MainSceneStageCardListController>() != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(MainSceneStageCardListController));
        go.AddComponent<MainSceneStageCardListController>();
    }

    private void Start()
    {
        ResolveReferences();
        BuildStageCards();
    }

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged -= HandleLanguageChanged;
        LocalizationManager.LanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= HandleLanguageChanged;
    }

    private void ResolveReferences()
    {
        if (stageCardPanel == null)
        {
            GameObject found = GameObject.Find("StageCardPanel");
            if (found != null)
            {
                stageCardPanel = found.transform;
            }
        }

        if (stageCardPrefab == null && stageCardPanel != null)
        {
            Transform template = stageCardPanel.Find(templateCardName);
            if (template == null && stageCardPanel.childCount > 0)
            {
                template = stageCardPanel.GetChild(0);
            }

            if (template != null)
            {
                stageCardPrefab = template.gameObject;
            }
        }
    }

    private void BuildStageCards()
    {
        if (stageCardPanel == null || stageCardPrefab == null)
        {
            return;
        }

        EnsureScrollableLayout(stageCardPanel.gameObject);

        Transform templateCard = stageCardPrefab != null ? stageCardPrefab.transform : null;
        bool templateFromPanel = templateCard != null && templateCard.parent == stageCardPanel;

        if (clearExistingChildren)
        {
            boundCards.Clear();
            for (int i = stageCardPanel.childCount - 1; i >= 0; i--)
            {
                Transform child = stageCardPanel.GetChild(i);
                if (templateFromPanel && child == templateCard)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }

        var stages = StageCsvLoader.LoadAllStages();
        for (int i = 0; i < stages.Count; i++)
        {
            StageRow stage = stages[i];
            GameObject card = Instantiate(stageCardPrefab, stageCardPanel);
            card.SetActive(true);
            BindStageCard(card, stage);
        }

        if (templateFromPanel)
        {
            templateCard.gameObject.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(stageCardPanel as RectTransform);
    }

    private void BindStageCard(GameObject card, StageRow stage)
    {
        if (card == null)
        {
            return;
        }

        Transform imageNode = card.transform.Find(stageImageObjectName);
        if (imageNode != null)
        {
            Image image = imageNode.GetComponent<Image>();
            if (image != null && !string.IsNullOrWhiteSpace(stage.StageImage))
            {
                Sprite sprite = Resources.Load<Sprite>($"Sprite/{stage.StageImage}");
                if (sprite != null)
                {
                    image.sprite = sprite;
                }
            }
        }

        Transform titleNode = card.transform.Find(stageTitleObjectName);
        if (titleNode != null)
        {
            TMP_Text tmp = titleNode.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = LocalizationManager.GetText(stage.StageName);
                boundCards.Add(new BoundStageCard(stage.StageName, tmp, null));
            }
            else
            {
                Text text = titleNode.GetComponent<Text>();
                if (text != null)
                {
                    text.text = LocalizationManager.GetText(stage.StageName);
                    boundCards.Add(new BoundStageCard(stage.StageName, null, text));
                }
            }
        }

        StageCardSceneLoader sceneLoader = card.GetComponent<StageCardSceneLoader>();
        if (sceneLoader == null)
        {
            sceneLoader = card.AddComponent<StageCardSceneLoader>();
        }

        sceneLoader.SetSceneName(stage.SceneName);

        Button[] buttons = card.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            string sceneName = stage.SceneName;
            button.onClick.AddListener(() => StageCardSceneLoader.LoadSceneSafe(sceneName));
        }
    }

    private static void EnsureScrollableLayout(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        VerticalLayoutGroup vertical = panel.GetComponent<VerticalLayoutGroup>();
        if (vertical == null)
        {
            vertical = panel.AddComponent<VerticalLayoutGroup>();
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;
            vertical.spacing = 10f;
        }

        ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = panel.AddComponent<ContentSizeFitter>();
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void HandleLanguageChanged(LanguageCode _)
    {
        for (int i = 0; i < boundCards.Count; i++)
        {
            boundCards[i].Apply();
        }
    }

    private readonly struct BoundStageCard
    {
        private readonly string stageNameKey;
        private readonly TMP_Text tmpText;
        private readonly Text legacyText;

        public BoundStageCard(string stageNameKey, TMP_Text tmpText, Text legacyText)
        {
            this.stageNameKey = stageNameKey;
            this.tmpText = tmpText;
            this.legacyText = legacyText;
        }

        public void Apply()
        {
            string value = LocalizationManager.GetText(stageNameKey);
            if (tmpText != null)
            {
                tmpText.text = value;
            }

            if (legacyText != null)
            {
                legacyText.text = value;
            }
        }
    }
}
