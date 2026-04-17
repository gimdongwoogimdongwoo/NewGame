using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StageCardSceneLoader : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string sceneName;

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
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}
