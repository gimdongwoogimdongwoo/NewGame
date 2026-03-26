using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExpBarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image expBarFill;

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 6f;

    private Coroutine fillRoutine;

    private void Awake()
    {
        if (expBarFill == null)
        {
            expBarFill = GetComponent<Image>();
        }

        if (expBarFill == null)
        {
            Debug.LogWarning("ExpBarController: Image 컴포넌트를 찾을 수 없습니다.");
        }
    }

    public void SetRatioImmediate(float ratio)
    {
        if (fillRoutine != null)
        {
            StopCoroutine(fillRoutine);
            fillRoutine = null;
        }

        if (expBarFill == null)
        {
            return;
        }

        expBarFill.fillAmount = Mathf.Clamp01(ratio);
    }

    public IEnumerator AnimateToRatio(float targetRatio)
    {
        if (fillRoutine != null)
        {
            StopCoroutine(fillRoutine);
        }

        fillRoutine = StartCoroutine(AnimateFillRoutine(Mathf.Clamp01(targetRatio)));
        yield return fillRoutine;
        fillRoutine = null;
    }

    private IEnumerator AnimateFillRoutine(float target)
    {
        if (expBarFill == null)
        {
            yield break;
        }

        float current = expBarFill.fillAmount;
        while (!Mathf.Approximately(current, target))
        {
            current = Mathf.MoveTowards(current, target, fillSpeed * Time.deltaTime);
            expBarFill.fillAmount = current;
            yield return null;
        }

        expBarFill.fillAmount = target;
    }
}
