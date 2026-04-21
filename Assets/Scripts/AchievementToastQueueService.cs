using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementToastQueueService : MonoBehaviour
{
    private static AchievementToastQueueService instance;
    private static readonly List<QueuedToast> pending = new();
    private static readonly HashSet<string> queuedOrShownIds = new(StringComparer.OrdinalIgnoreCase);

    private AchievementToastController toastController;
    private bool waitingForPlaybackEnd;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(AchievementToastQueueService));
        instance = go.AddComponent<AchievementToastQueueService>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        AchievementManager.AchievementCompleted -= HandleAchievementCompleted;
        AchievementManager.AchievementCompleted += HandleAchievementCompleted;
        AchievementManager.AchievementProgressReset -= HandleAchievementProgressReset;
        AchievementManager.AchievementProgressReset += HandleAchievementProgressReset;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        ResolveToastController();
        TryPlayNext();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        AchievementManager.AchievementCompleted -= HandleAchievementCompleted;
        AchievementManager.AchievementProgressReset -= HandleAchievementProgressReset;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        ResolveToastController();
        TryPlayNext();
    }

    private void HandleAchievementCompleted(AchievementCompletedEvent evt)
    {
        if (evt == null || string.IsNullOrWhiteSpace(evt.Id) || queuedOrShownIds.Contains(evt.Id))
        {
            return;
        }

        queuedOrShownIds.Add(evt.Id);
        pending.Add(new QueuedToast
        {
            Id = evt.Id,
            Message = LocalizationManager.GetTextFormat("UI_ACHIEVEMENT_COMPLETED_TOAST", LocalizationManager.GetText(evt.TitleKey)),
            RowIndex = evt.RowIndex,
            CompletionSerial = evt.CompletionSerial
        });

        pending.Sort((a, b) =>
        {
            int serialCompare = a.CompletionSerial.CompareTo(b.CompletionSerial);
            return serialCompare != 0 ? serialCompare : a.RowIndex.CompareTo(b.RowIndex);
        });

        TryPlayNext();
    }

    private void HandleAchievementProgressReset()
    {
        pending.Clear();
        queuedOrShownIds.Clear();
        waitingForPlaybackEnd = false;
        if (toastController != null)
        {
            toastController.gameObject.SetActive(false);
        }
    }

    private void ResolveToastController()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            toastController = null;
            return;
        }

        toastController = AchievementToastController.EnsureOnMainSceneCanvas();
    }

    private void TryPlayNext()
    {
        if (waitingForPlaybackEnd)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            return;
        }

        if (pending.Count == 0)
        {
            return;
        }

        if (toastController == null)
        {
            ResolveToastController();
        }

        if (toastController == null)
        {
            return;
        }

        QueuedToast next = pending[0];
        pending.RemoveAt(0);

        waitingForPlaybackEnd = true;
        toastController.PlayToast(next.Message, NotifyPlaybackFinished);
    }

    private void NotifyPlaybackFinished()
    {
        waitingForPlaybackEnd = false;
        TryPlayNext();
    }

    private sealed class QueuedToast
    {
        public string Id;
        public string Message;
        public int RowIndex;
        public long CompletionSerial;
    }
}
