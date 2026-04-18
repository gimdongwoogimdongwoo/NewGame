using UnityEngine;

public class StageBgmController : MonoBehaviour
{
    private const string BgmResourceRoot = "BGM";

    private AudioSource audioSource;
    private bool wasStoppedByResult;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<StageBgmController>() != null)
        {
            return;
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!StageCsvLoader.TryGetStageBySceneName(sceneName, out _, logWhenMissing: false))
        {
            return;
        }

        GameObject go = new GameObject(nameof(StageBgmController));
        go.AddComponent<StageBgmController>();
    }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
        audioSource.priority = 20;
    }

    private void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!StageCsvLoader.TryGetStageBySceneName(sceneName, out StageRow stage, logWhenMissing: false))
        {
            return;
        }

        string bgmName = stage.Bgm;
        AudioClip clip = ResolveClipByName(bgmName);
        if (clip == null)
        {
            Debug.LogWarning($"BGM clip not found at Resources/{BgmResourceRoot}/{bgmName}");
            return;
        }

        audioSource.clip = clip;
        wasStoppedByResult = false;
        audioSource.Play();
    }

    private static AudioClip ResolveClipByName(string bgmName)
    {
        if (string.IsNullOrWhiteSpace(bgmName))
        {
            return null;
        }

        AudioClip direct = Resources.Load<AudioClip>($"{BgmResourceRoot}/{bgmName}");
        if (direct != null)
        {
            return direct;
        }

        AudioClip[] candidates = Resources.LoadAll<AudioClip>(BgmResourceRoot);
        if (candidates == null || candidates.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < candidates.Length; i++)
        {
            AudioClip candidate = candidates[i];
            if (candidate != null &&
                string.Equals(candidate.name, bgmName, System.StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return candidates[0];
    }

    private void Update()
    {
        if (wasStoppedByResult || !GameplayPauseController.IsGameResultPaused)
        {
            return;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        wasStoppedByResult = true;
    }
}
