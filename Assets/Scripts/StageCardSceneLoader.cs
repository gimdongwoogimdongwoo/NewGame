using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StageCardSceneLoader : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string sceneName;
    private static bool isSceneLoading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetLoadGuard()
    {
        isSceneLoading = false;
    }

    public void SetSceneName(string nextSceneName)
    {
        sceneName = nextSceneName;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LoadSceneSafe(sceneName);
    }

    public static void LoadSceneSafe(string targetSceneName)
    {
        if (isSceneLoading || string.IsNullOrWhiteSpace(targetSceneName))
        {
            return;
        }

        isSceneLoading = true;
        SceneManager.LoadScene(targetSceneName);
    }
}
