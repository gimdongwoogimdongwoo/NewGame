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
    }

    private void Start()
    {
        int stageId = StageCsvLoader.ResolveCurrentStageId();
        if (stageId <= 0)
        {
            return;
        }

        string bgmName = StageCsvLoader.LoadStageBgmName(stageId);
        if (string.IsNullOrWhiteSpace(bgmName))
        {
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>($"{BgmResourceRoot}/{bgmName}");
        if (clip == null)
        {
            Debug.LogWarning($"BGM clip not found at Resources/{BgmResourceRoot}/{bgmName}");
            return;
        }

        audioSource.clip = clip;
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
