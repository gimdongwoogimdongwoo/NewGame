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

        int stageId = StageCsvLoader.ResolveCurrentStageId();
        if (stageId <= 0)
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
        int stageId = StageCsvLoader.ResolveCurrentStageId();
        if (stageId <= 0)
        {
            return;
        }

        string bgmName = StageCsvLoader.LoadStageBgmName(stageId);
        AudioClip clip = string.IsNullOrWhiteSpace(bgmName)
            ? null
            : Resources.Load<AudioClip>($"{BgmResourceRoot}/{bgmName}");
        if (clip == null)
        {
            AudioClip[] candidates = Resources.LoadAll<AudioClip>(BgmResourceRoot);
            if (candidates != null && candidates.Length > 0)
            {
                clip = candidates[0];
                Debug.LogWarning($"BGM '{bgmName}' 을(를) 찾지 못해 '{clip.name}' 을(를) 대체 재생합니다.");
            }
            else
            {
                Debug.LogWarning($"BGM clip not found at Resources/{BgmResourceRoot}/{bgmName}");
                return;
            }
        }

        audioSource.clip = clip;
        wasStoppedByResult = false;
        audioSource.Play();
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
