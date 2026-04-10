using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultManager : MonoBehaviour
{
    [Header("Result UI")]
    [SerializeField] private GameObject hudResult;
    [SerializeField] private Image success;
    [SerializeField] private Image fail;
    [SerializeField] private Button btnHome;

    [Header("Scene")]
    [SerializeField] private string homeSceneName = "MainScene";

    private bool isResultConfirmed;

    private void Awake()
    {
        if (btnHome != null)
        {
            btnHome.onClick.RemoveListener(HandleHomeClicked);
            btnHome.onClick.AddListener(HandleHomeClicked);
        }
    }

    private void Start()
    {
        SetResultHudVisible(false);
        GameplayPauseController.ClearGameResultPause();
    }

    private void OnDestroy()
    {
        if (btnHome != null)
        {
            btnHome.onClick.RemoveListener(HandleHomeClicked);
        }
    }

    public void HandleStageSuccess()
    {
        if (isResultConfirmed)
        {
            return;
        }

        isResultConfirmed = true;
        GameplayPauseController.PauseForGameResult();
        SetResultHudVisible(true);
        SetResultImage(successVisible: true);
    }

    public void HandleStageFail()
    {
        if (isResultConfirmed)
        {
            return;
        }

        isResultConfirmed = true;
        GameplayPauseController.PauseForGameResult();
        SetResultHudVisible(true);
        SetResultImage(successVisible: false);
    }

    private void SetResultHudVisible(bool visible)
    {
        if (hudResult != null)
        {
            hudResult.SetActive(visible);
        }

        if (btnHome != null)
        {
            btnHome.gameObject.SetActive(visible);
            btnHome.interactable = visible;
        }
    }

    private void SetResultImage(bool successVisible)
    {
        if (success != null)
        {
            success.gameObject.SetActive(successVisible);
        }

        if (fail != null)
        {
            fail.gameObject.SetActive(!successVisible);
        }
    }

    private void HandleHomeClicked()
    {
        SetResultHudVisible(false);
        GameplayPauseController.ClearGameResultPause();
        SceneManager.LoadScene(homeSceneName);
    }
}
