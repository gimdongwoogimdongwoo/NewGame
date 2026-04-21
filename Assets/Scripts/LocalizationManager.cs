using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public enum LanguageCode
{
    KOR,
    ENG
}

public static class LocalizationManager
{
    private const string StringTableResourcePath = "StringTable";
    private static readonly Dictionary<string, LocalizedStringRow> table = new(StringComparer.Ordinal);

    public static event Action<LanguageCode> LanguageChanged;

    public static LanguageCode CurrentLanguage { get; private set; } = LanguageCode.KOR;

    public static void SetLanguage(LanguageCode language)
    {
        EnsureLoaded();
        if (CurrentLanguage == language)
        {
            return;
        }

        CurrentLanguage = language;
        LanguageChanged?.Invoke(CurrentLanguage);
    }

    public static bool TrySetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        if (!Enum.TryParse(languageCode.Trim(), true, out LanguageCode parsed))
        {
            return false;
        }

        SetLanguage(parsed);
        return true;
    }

    public static string GetText(string stringKey)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(stringKey))
        {
            return string.Empty;
        }

        if (!table.TryGetValue(stringKey.Trim(), out LocalizedStringRow row))
        {
            return stringKey;
        }

        string localized = CurrentLanguage == LanguageCode.ENG ? row.ENG : row.KOR;
        if (!string.IsNullOrWhiteSpace(localized))
        {
            return localized;
        }

        string fallback = CurrentLanguage == LanguageCode.ENG ? row.KOR : row.ENG;
        return string.IsNullOrWhiteSpace(fallback) ? stringKey : fallback;
    }

    public static string GetTextFormat(string stringKey, params object[] args)
    {
        string format = GetText(stringKey);
        if (args == null || args.Length == 0)
        {
            return format;
        }

        return string.Format(CultureInfo.InvariantCulture, format, args);
    }

    private static void EnsureLoaded()
    {
        if (table.Count > 0)
        {
            return;
        }

        TextAsset csv = Resources.Load<TextAsset>(StringTableResourcePath);
        if (csv == null)
        {
            Debug.LogWarning($"[LocalizationManager] Resources/{StringTableResourcePath}.csv not found.");
            return;
        }

        string[] lines = csv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            return;
        }

        Dictionary<string, int> header = BuildHeader(lines[0]);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }

            string[] cols = line.Split(',');
            string key = Read(cols, header, "StringKey", 0);
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            table[key] = new LocalizedStringRow
            {
                KOR = Read(cols, header, "KOR", 1),
                ENG = Read(cols, header, "ENG", 2)
            };
        }
    }

    private static Dictionary<string, int> BuildHeader(string line)
    {
        Dictionary<string, int> map = new(StringComparer.OrdinalIgnoreCase);
        string[] cols = line.Split(',');
        for (int i = 0; i < cols.Length; i++)
        {
            string key = cols[i].Trim();
            if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
            {
                map.Add(key, i);
            }
        }

        return map;
    }

    private static string Read(string[] cols, Dictionary<string, int> header, string key, int fallback)
    {
        int index = header != null && header.TryGetValue(key, out int found) ? found : fallback;
        return index >= 0 && index < cols.Length ? cols[index].Trim() : string.Empty;
    }

    private struct LocalizedStringRow
    {
        public string KOR;
        public string ENG;
    }
}
