using System.Collections.Generic;
using UnityEngine;

public class GameResultSfxPlayer : MonoBehaviour
{
    private const string WinResourcePath = "SFX/win";
    private const string FailResourcePath = "SFX/fail";
    private const int InitialVoiceCount = 4;
    private const int MaxVoiceCount = 8;
    private const float VoiceVolume = 1f;

    private static GameResultSfxPlayer instance;

    private readonly List<AudioSource> voices = new();
    private AudioClip winClip;
    private AudioClip failClip;
    private int nextVoiceIndex;
    private bool missingWinLogged;
    private bool missingFailLogged;

    public static void PlayWin()
    {
        EnsureInstance();
        instance?.PlayClip(instance.winClip, WinResourcePath, ref instance.missingWinLogged);
    }

    public static void PlayFail()
    {
        EnsureInstance();
        instance?.PlayClip(instance.failClip, FailResourcePath, ref instance.missingFailLogged);
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindFirstObjectByType<GameResultSfxPlayer>();
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(GameResultSfxPlayer));
        instance = go.AddComponent<GameResultSfxPlayer>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        winClip = Resources.Load<AudioClip>(WinResourcePath);
        failClip = Resources.Load<AudioClip>(FailResourcePath);
        PrewarmVoices(InitialVoiceCount);
    }

    private void PlayClip(AudioClip clip, string resourcePath, ref bool missingLogged)
    {
        if (clip == null)
        {
            if (!missingLogged)
            {
                Debug.LogWarning($"GameResultSfxPlayer: '{resourcePath}' 오디오를 찾지 못했습니다.");
                missingLogged = true;
            }

            return;
        }

        AudioSource voice = AcquireVoice();
        voice.pitch = 1f;
        voice.clip = clip;
        voice.time = 0f;
        voice.Play();
    }

    private AudioSource AcquireVoice()
    {
        for (int i = 0; i < voices.Count; i++)
        {
            int idx = (nextVoiceIndex + i) % voices.Count;
            AudioSource candidate = voices[idx];
            if (!candidate.isPlaying)
            {
                nextVoiceIndex = (idx + 1) % voices.Count;
                return candidate;
            }
        }

        if (voices.Count < MaxVoiceCount)
        {
            AudioSource newVoice = CreateVoice();
            voices.Add(newVoice);
            nextVoiceIndex = voices.Count > 0 ? voices.Count - 1 : 0;
            return newVoice;
        }

        AudioSource fallback = voices[nextVoiceIndex];
        nextVoiceIndex = (nextVoiceIndex + 1) % voices.Count;
        return fallback;
    }

    private void PrewarmVoices(int count)
    {
        int safeCount = Mathf.Clamp(count, 1, MaxVoiceCount);
        for (int i = 0; i < safeCount; i++)
        {
            voices.Add(CreateVoice());
        }
    }

    private AudioSource CreateVoice()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.volume = VoiceVolume;
        return source;
    }
}
