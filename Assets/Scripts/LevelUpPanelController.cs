using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LevelUpPanelController : MonoBehaviour
{
    public static LevelUpPanelController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject cardContainer;
    [SerializeField] private Button[] cardButtons = new Button[3];

    private readonly Queue<int> pendingLevelUps = new();
    private readonly UnityAction[] buttonHandlers = new UnityAction[3];
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

        SetPanelVisible(true);
        GameplayPauseController.PauseForLevelUp();
    }

    private void HandleCardSelected(int cardIndex)
    {
        if (!isPanelOpen)
        {
            return;
        }

        Debug.Log($"LevelUpPanelController: Level {currentLevelRequest} card {cardIndex + 1} selected.");

        isPanelOpen = false;
        currentLevelRequest = 0;
        SetPanelVisible(false);
        GameplayPauseController.ResumeFromLevelUp();

        StartCoroutine(OpenNextPanelNextFrame());
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

    private void SetPanelVisible(bool isVisible)
    {
        if (cardContainer == null)
        {
            return;
        }

        cardContainer.SetActive(isVisible);
    }
}
