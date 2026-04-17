using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StageCsvLoader
{
    private const string StageCsvResourcePath = "Stage";
    private const string StageMonsterCsvResourcePath = "StageMonster";

    public static List<StageRow> LoadAllStages()
    {
        List<StageRow> stages = new();

        TextAsset stageCsv = Resources.Load<TextAsset>(StageCsvResourcePath);
        if (stageCsv == null)
        {
            Debug.LogError($"{StageCsvResourcePath}.csv was not found in Resources.");
            return stages;
        }

        string[] rawLines = stageCsv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (rawLines.Length <= 1)
        {
            return stages;
        }

        Dictionary<string, int> header = BuildHeaderMap(rawLines[0]);

        for (int i = 1; i < rawLines.Length; i++)
        {
            string line = rawLines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }

            string[] cols = line.Split(',');

            if (!TryReadInt(cols, header, "StageId", fallbackIndex: 0, out int stageId))
            {
                continue;
            }

            string sceneName = ReadString(cols, header, "SceneName", fallbackIndex: 1);
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                continue;
            }

            stages.Add(new StageRow
            {
                StageId = stageId,
                SceneName = sceneName,
                StageName = ReadString(cols, header, "StageName", fallbackIndex: 2),
                Time = TryReadFloat(cols, header, "Time", fallbackIndex: 3, out float timeSec)
                    ? Mathf.Max(0f, timeSec)
                    : 0f,
                Bgm = ReadString(cols, header, "BGM", fallbackIndex: 4),
                StageImage = ReadString(cols, header, "StageImage", fallbackIndex: 5)
            });
        }

        return stages;
    }

    public static int ResolveCurrentStageId()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        List<StageRow> stages = LoadAllStages();
        for (int i = 0; i < stages.Count; i++)
        {
            if (string.Equals(stages[i].SceneName, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return stages[i].StageId;
            }
        }

        Debug.LogWarning($"No StageId mapping found for scene '{sceneName}' in {StageCsvResourcePath}.csv");
        return -1;
    }


    public static float LoadStageTimeSeconds(int stageId)
    {
        List<StageRow> stages = LoadAllStages();
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].StageId == stageId)
            {
                return stages[i].Time;
            }
        }

        return 0f;
    }

    public static string LoadStageBgmName(int stageId)
    {
        List<StageRow> stages = LoadAllStages();
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].StageId == stageId)
            {
                return stages[i].Bgm;
            }
        }

        return string.Empty;
    }

    

    public static List<StageMonsterSpawnRule> LoadStageMonsterRules(int stageId)
    {
        var results = new List<StageMonsterSpawnRule>();

        TextAsset stageMonsterCsv = Resources.Load<TextAsset>(StageMonsterCsvResourcePath);
        if (stageMonsterCsv == null)
        {
            Debug.LogError($"{StageMonsterCsvResourcePath}.csv was not found in Resources.");
            return results;
        }

        foreach (string line in EnumerateDataLines(stageMonsterCsv.text))
        {
            string[] cols = line.Split(',');
            if (cols.Length < 9)
            {
                Debug.LogWarning($"Ignored malformed StageMonster row: {line}");
                continue;
            }

            if (!TryParseRule(cols, out StageMonsterSpawnRule rule))
            {
                Debug.LogWarning($"Ignored invalid StageMonster row: {line}");
                continue;
            }

            if (rule.StageId == stageId)
            {
                results.Add(rule);
            }
        }

        return results;
    }

    private static bool TryParseRule(string[] cols, out StageMonsterSpawnRule rule)
    {
        rule = default;

        if (!int.TryParse(cols[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int stageId)) return false;
        if (!float.TryParse(cols[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float spawnStartSec)) return false;
        if (!float.TryParse(cols[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float waveIntervalSec)) return false;
        if (!int.TryParse(cols[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int waveSizeStart)) return false;
        if (!int.TryParse(cols[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out int waveSizeGrowth)) return false;
        if (!int.TryParse(cols[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out int waveSizeMax)) return false;
        if (!int.TryParse(cols[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out int totalBudget)) return false;
        if (!int.TryParse(cols[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxAliveCap)) return false;

        rule = new StageMonsterSpawnRule
        {
            StageId = stageId,
            MonsterId = cols[1].Trim(),
            SpawnStartSec = Mathf.Max(0f, spawnStartSec),
            WaveIntervalSec = Mathf.Max(0.01f, waveIntervalSec),
            WaveSizeStart = Mathf.Max(0, waveSizeStart),
            WaveSizeGrowth = Mathf.Max(0, waveSizeGrowth),
            WaveSizeMax = Mathf.Max(0, waveSizeMax),
            TotalBudget = Mathf.Max(0, totalBudget),
            MaxAliveCap = Mathf.Max(0, maxAliveCap)
        };

        return !string.IsNullOrWhiteSpace(rule.MonsterId);
    }

    private static IEnumerable<string> EnumerateDataLines(string csvText)
    {
        string[] rawLines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return rawLines.Skip(1).Select(line => line.Trim()).Where(line => line.Length > 0 && !line.StartsWith("#"));
    }

    private static Dictionary<string, int> BuildHeaderMap(string headerLine)
    {
        Dictionary<string, int> map = new(StringComparer.OrdinalIgnoreCase);
        string[] cols = headerLine.Split(',');
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

    private static string ReadString(string[] cols, Dictionary<string, int> header, string key, int fallbackIndex)
    {
        int idx = ResolveColumnIndex(header, key, fallbackIndex);
        if (idx < 0 || idx >= cols.Length)
        {
            return string.Empty;
        }

        return cols[idx].Trim();
    }

    private static bool TryReadInt(string[] cols, Dictionary<string, int> header, string key, int fallbackIndex, out int value)
    {
        string str = ReadString(cols, header, key, fallbackIndex);
        return int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadFloat(string[] cols, Dictionary<string, int> header, string key, int fallbackIndex, out float value)
    {
        string str = ReadString(cols, header, key, fallbackIndex);
        return float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static int ResolveColumnIndex(Dictionary<string, int> header, string key, int fallbackIndex)
    {
        if (header != null && header.TryGetValue(key, out int idx))
        {
            return idx;
        }

        return fallbackIndex;
    }
}

[Serializable]
public struct StageMonsterSpawnRule
{
    public int StageId;
    public string MonsterId;
    public float SpawnStartSec;
    public float WaveIntervalSec;
    public int WaveSizeStart;
    public int WaveSizeGrowth;
    public int WaveSizeMax;
    public int TotalBudget;
    public int MaxAliveCap;
}

[Serializable]
public struct StageRow
{
    public int StageId;
    public string SceneName;
    public string StageName;
    public float Time;
    public string Bgm;
    public string StageImage;
}
