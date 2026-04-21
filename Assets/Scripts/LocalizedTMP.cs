using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedTMP : MonoBehaviour
{
    [SerializeField] private string stringKey;
    [SerializeField] private TMP_Text targetText;

    public string StringKey
    {
        get => stringKey;
        set
        {
            stringKey = value;
            Refresh();
        }
    }

    private void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged -= HandleLanguageChanged;
        LocalizationManager.LanguageChanged += HandleLanguageChanged;
        Refresh();
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(LanguageCode _)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }

        if (targetText == null)
        {
            return;
        }

        targetText.text = LocalizationManager.GetText(stringKey);
    }
}
