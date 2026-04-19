using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementToastController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text txtMessage;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float holdDuration = 2f;

    private Coroutine playRoutine;

    public bool IsPlaying => playRoutine != null;

    private void Awake()
    {
        ResolveRefs();
        SetVisible(false, 0f);
    }

    public void PlayToast(string message, Action onComplete)
    {
        ResolveRefs();

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        playRoutine = StartCoroutine(PlayRoutine(message, onComplete));
    }

    private IEnumerator PlayRoutine(string message, Action onComplete)
    {
        if (txtMessage != null)
        {
            txtMessage.text = message;
        }

        SetVisible(true, 0f);
        yield return FadeTo(1f, fadeDuration);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, holdDuration));
        yield return FadeTo(0f, fadeDuration);

        SetVisible(false, 0f);
        playRoutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float start = canvasGroup.alpha;
        float time = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (time < safeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(time / safeDuration));
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private void ResolveRefs()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (txtMessage == null)
        {
            Transform txt = transform.Find("TXT_Message");
            if (txt != null)
            {
                txtMessage = txt.GetComponent<TMP_Text>();
            }

            if (txtMessage == null)
            {
                txtMessage = GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    private void SetVisible(bool visible, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        gameObject.SetActive(visible);
    }

    public static AchievementToastController EnsureOnMainSceneCanvas()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            return null;
        }

        AchievementToastController existing = FindFirstObjectByType<AchievementToastController>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(false);
            return existing;
        }

        GameObject existingToastObject = GameObject.Find("AchievementToast");
        if (existingToastObject != null)
        {
            AchievementToastController attached = existingToastObject.GetComponent<AchievementToastController>();
            if (attached == null)
            {
                attached = existingToastObject.AddComponent<AchievementToastController>();
            }

            existingToastObject.SetActive(false);
            return attached;
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            return null;
        }

        GameObject toast = new GameObject("AchievementToast", typeof(RectTransform), typeof(CanvasGroup));
        toast.transform.SetParent(canvas.transform, false);

        RectTransform toastRt = toast.GetComponent<RectTransform>();
        toastRt.anchorMin = new Vector2(0.5f, 0f);
        toastRt.anchorMax = new Vector2(0.5f, 0f);
        toastRt.pivot = new Vector2(0.5f, 0f);
        toastRt.anchoredPosition = new Vector2(0f, 150f);
        toastRt.sizeDelta = new Vector2(600f, 110f);

        GameObject textGo = new GameObject("TXT_Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(toast.transform, false);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(24f, 16f);
        textRt.offsetMax = new Vector2(-24f, -16f);

        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 36;
        text.text = string.Empty;
        text.color = Color.white;

        AchievementToastController controller = toast.AddComponent<AchievementToastController>();
        controller.canvasGroup = toast.GetComponent<CanvasGroup>();
        controller.txtMessage = text;
        toast.SetActive(false);
        return controller;
    }
}
