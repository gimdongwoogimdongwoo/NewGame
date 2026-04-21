using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScenePopupController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject popupStage;
    [SerializeField] private GameObject popupUpgrade;
    [SerializeField] private GameObject popupAchievement;
    [SerializeField] private GameObject popupLanguage;

    [Header("Buttons")]
    [SerializeField] private Button btnGameStart;
    [SerializeField] private Button btnUpgrade;
    [SerializeField] private Button btnAchievement;
    [SerializeField] private Button btnLanguage;
    [SerializeField] private Button btnExit;
    [SerializeField] private Button btnLanguageEng;
    [SerializeField] private Button btnLanguageKor;
    [SerializeField] private Button backdropButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainScene")
        {
            return;
        }

        if (FindFirstObjectByType<MainScenePopupController>() != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(MainScenePopupController));
        go.AddComponent<MainScenePopupController>();
    }

    private void Awake()
    {
        ResolveReferences();
        BindMainButtons();
        BindCloseButtonsFromPopup();
    }

    private void Start()
    {
        GameplayPauseController.ClearGameResultPause();
        GameplayPauseController.ResumeFromLevelUp();
        _ = TotalCoinPersistence.Instance;
        EnsureUpgradeCoinViewBinding();
        EnsureUpgradePopupBinding();
        CloseAllPopups();
    }

    private void OnDestroy()
    {
        UnbindMainButtons();
    }

    private void ResolveReferences()
    {
        popupPanel = popupPanel != null ? popupPanel : FindByAnyName("Popup_Panel");
        popupStage = popupStage != null ? popupStage : FindByAnyName("Popup_Stage", "PopupStage");
        popupUpgrade = popupUpgrade != null ? popupUpgrade : FindByAnyName("Popup_Upgrade", "PopupUpgrade");
        popupAchievement = popupAchievement != null ? popupAchievement : FindByAnyName("Popup_Achivement", "Popup_Achievement", "PopupAchievement");
        popupLanguage = popupLanguage != null ? popupLanguage : FindByAnyName("Popup_Language", "PopupLanguage");

        btnGameStart = btnGameStart != null ? btnGameStart : FindButtonAny("BTN_GameStart", "Btn_GameStart", "GameStart");
        btnUpgrade = btnUpgrade != null ? btnUpgrade : FindButtonAny("BTN_Upgrade", "Btn_Upgrade", "Upgrade");
        btnAchievement = btnAchievement != null ? btnAchievement : FindButtonAny("BTN_Achivement", "BTN_Achievement", "Btn_Achievement", "Achievement");
        btnLanguage = btnLanguage != null ? btnLanguage : FindButtonAny("Button_Language", "BTN_Language", "Btn_Language");
        btnLanguageEng = btnLanguageEng != null ? btnLanguageEng : FindButtonAny("Language_ENG", "BTN_Language_ENG");
        btnLanguageKor = btnLanguageKor != null ? btnLanguageKor : FindButtonAny("Language_KOR", "BTN_Language_KOR");
        btnExit = btnExit != null ? btnExit : FindButtonAny("BTN_Exit", "Btn_Exit", "Exit");
    }

    private void BindMainButtons()
    {
        if (btnGameStart != null)
        {
            btnGameStart.onClick.RemoveListener(OpenStagePopup);
            btnGameStart.onClick.AddListener(OpenStagePopup);
        }

        if (btnUpgrade != null)
        {
            btnUpgrade.onClick.RemoveListener(OpenUpgradePopup);
            btnUpgrade.onClick.AddListener(OpenUpgradePopup);
        }

        if (btnAchievement != null)
        {
            btnAchievement.onClick.RemoveListener(OpenAchievementPopup);
            btnAchievement.onClick.AddListener(OpenAchievementPopup);
        }

        if (btnLanguage != null)
        {
            btnLanguage.onClick.RemoveListener(OpenLanguagePopup);
            btnLanguage.onClick.AddListener(OpenLanguagePopup);
        }

        if (btnLanguageEng != null)
        {
            btnLanguageEng.onClick.RemoveListener(HandleLanguageEngClicked);
            btnLanguageEng.onClick.AddListener(HandleLanguageEngClicked);
        }

        if (btnLanguageKor != null)
        {
            btnLanguageKor.onClick.RemoveListener(HandleLanguageKorClicked);
            btnLanguageKor.onClick.AddListener(HandleLanguageKorClicked);
        }

        if (btnExit != null)
        {
            btnExit.onClick.RemoveListener(HandleExitClicked);
            btnExit.onClick.AddListener(HandleExitClicked);
        }

        if (backdropButton != null)
        {
            backdropButton.onClick.AddListener(CloseAllPopups);
        }
    }

    private void UnbindMainButtons()
    {
        if (btnGameStart != null)
        {
            btnGameStart.onClick.RemoveListener(OpenStagePopup);
        }

        if (btnUpgrade != null)
        {
            btnUpgrade.onClick.RemoveListener(OpenUpgradePopup);
        }

        if (btnAchievement != null)
        {
            btnAchievement.onClick.RemoveListener(OpenAchievementPopup);
        }

        if (btnLanguage != null)
        {
            btnLanguage.onClick.RemoveListener(OpenLanguagePopup);
        }

        if (btnLanguageEng != null)
        {
            btnLanguageEng.onClick.RemoveListener(HandleLanguageEngClicked);
        }

        if (btnLanguageKor != null)
        {
            btnLanguageKor.onClick.RemoveListener(HandleLanguageKorClicked);
        }

        if (btnExit != null)
        {
            btnExit.onClick.RemoveListener(HandleExitClicked);
        }

        if (backdropButton != null)
        {
            backdropButton.onClick.RemoveListener(CloseAllPopups);
        }
    }


    private void EnsureUpgradeCoinViewBinding()
    {
        if (popupUpgrade == null)
        {
            return;
        }

        PopupUpgradeTotalCoinView existing = popupUpgrade.GetComponentInChildren<PopupUpgradeTotalCoinView>(true);
        if (existing != null)
        {
            return;
        }

        TextMeshProUGUI[] texts = popupUpgrade.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
            {
                continue;
            }

            string name = text.gameObject.name;
            if (name.IndexOf("coin", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                text.gameObject.AddComponent<PopupUpgradeTotalCoinView>();
                return;
            }
        }

        // Popup 구조에 코인 텍스트가 없는 경우도 허용 (경고 로그 과다 방지).
    }


    private void EnsureUpgradePopupBinding()
    {
        if (popupUpgrade == null)
        {
            return;
        }

        UpgradePopupUI popupUi = popupUpgrade.GetComponent<UpgradePopupUI>();
        if (popupUi == null)
        {
            popupUi = popupUpgrade.AddComponent<UpgradePopupUI>();
        }

        Transform[] children = popupUpgrade.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child == null || child == popupUpgrade.transform)
            {
                continue;
            }

            if (child.name.IndexOf("stat", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                child.name.IndexOf("row", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (child.GetComponent<UpgradeStatRowUI>() == null)
                {
                    child.gameObject.AddComponent<UpgradeStatRowUI>();
                }
            }
        }
    }

    private void BindCloseButtonsFromPopup()
    {
        BindCloseButtonsIn(popupStage);
        BindCloseButtonsIn(popupUpgrade);
        BindCloseButtonsIn(popupAchievement);
        BindCloseButtonsIn(popupLanguage);
    }

    private void BindCloseButtonsIn(GameObject popup)
    {
        if (popup == null)
        {
            return;
        }

        Button[] buttons = popup.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button != null && button.name.Contains("Xbutton"))
            {
                button.onClick.RemoveListener(CloseAllPopups);
                button.onClick.AddListener(CloseAllPopups);
            }
        }
    }

    private void OpenStagePopup()
    {
        OpenOnly(popupStage);
    }

    private void OpenUpgradePopup()
    {
        OpenOnly(popupUpgrade);
    }

    private void OpenAchievementPopup()
    {
        OpenOnly(popupAchievement);
    }

    private void OpenLanguagePopup()
    {
        OpenOnly(popupLanguage);
    }

    private void HandleLanguageEngClicked()
    {
        LocalizationManager.SetLanguage(LanguageCode.ENG);
    }

    private void HandleLanguageKorClicked()
    {
        LocalizationManager.SetLanguage(LanguageCode.KOR);
    }

    private void OpenOnly(GameObject targetPopup)
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        if (popupStage != null) popupStage.SetActive(popupStage == targetPopup);
        if (popupUpgrade != null) popupUpgrade.SetActive(popupUpgrade == targetPopup);
        if (popupAchievement != null) popupAchievement.SetActive(popupAchievement == targetPopup);
        if (popupLanguage != null) popupLanguage.SetActive(popupLanguage == targetPopup);
    }

    private void CloseAllPopups()
    {
        if (popupStage != null) popupStage.SetActive(false);
        if (popupUpgrade != null) popupUpgrade.SetActive(false);
        if (popupAchievement != null) popupAchievement.SetActive(false);
        if (popupLanguage != null) popupLanguage.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    private void HandleExitClicked()
    {
#if UNITY_EDITOR
        Debug.Log("BTN_Exit clicked (Editor): 종료 동작은 빌드 환경에서만 수행됩니다.");
#else
        Application.Quit();
#endif
    }

    private static GameObject FindByName(string objectName)
    {
        return string.IsNullOrWhiteSpace(objectName) ? null : GameObject.Find(objectName);
    }

    private static GameObject FindByAnyName(params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            GameObject found = FindByName(names[i]);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Button FindButton(string objectName)
    {
        GameObject go = FindByName(objectName);
        return go != null ? go.GetComponent<Button>() : null;
    }

    private static Button FindButtonAny(params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            Button found = FindButton(names[i]);
            if (found != null)
            {
                return found;
            }
        }

        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button candidate = allButtons[i];
            if (candidate == null)
            {
                continue;
            }

            string name = candidate.gameObject.name;
            for (int j = 0; j < names.Length; j++)
            {
                if (!string.IsNullOrWhiteSpace(names[j]) &&
                    name.IndexOf(names[j], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return candidate;
                }
            }
        }

        return null;
    }
}
