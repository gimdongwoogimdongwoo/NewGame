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

    public static int ResolveCurrentStageId()
    {
        TextAsset stageCsv = Resources.Load<TextAsset>(StageCsvResourcePath);
        if (stageCsv == null)
        {
            Debug.LogError($"{StageCsvResourcePath}.csv was not found in Resources.");
            return -1;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        foreach (string line in EnumerateDataLines(stageCsv.text))
        {
            string[] cols = line.Split(',');
            if (cols.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(cols[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int stageId))
            {
                continue;
            }

            if (string.Equals(cols[1].Trim(), sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return stageId;
            }
        }

        Debug.LogWarning($"No StageId mapping found for scene '{sceneName}' in {StageCsvResourcePath}.csv");
        return -1;
    }


    public static float LoadStageTimeSeconds(int stageId)
    {
        TextAsset stageCsv = Resources.Load<TextAsset>(StageCsvResourcePath);
        if (stageCsv == null)
        {
            Debug.LogError($"{StageCsvResourcePath}.csv was not found in Resources.");
            return 0f;
        }

        foreach (string line in EnumerateDataLines(stageCsv.text))
        {
            string[] cols = line.Split(',');
            if (cols.Length < 4)
            {
                continue;
            }

            if (!int.TryParse(cols[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int rowStageId))
            {
                continue;
            }

            if (rowStageId != stageId)
            {
                continue;
            }

            if (!float.TryParse(cols[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float timeSec))
            {
                return 0f;
            }

            return Mathf.Max(0f, timeSec);
        }

        return 0f;
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
