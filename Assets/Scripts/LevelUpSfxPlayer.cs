using System.Collections.Generic;
using UnityEngine;

public class LevelUpSfxPlayer : MonoBehaviour
{
    private const string LevelUpResourcePath = "SFX/levelup";
    private const int InitialVoiceCount = 6;
    private const int MaxVoiceCount = 16;
    private const float VoiceVolume = 0.9f;

    private static LevelUpSfxPlayer instance;

    private readonly List<AudioSource> voices = new();
    private AudioClip levelUpClip;
    private int nextVoiceIndex;
    private bool missingClipLogged;

    public static void PlayPopup()
    {
        EnsureInstance();
        if (instance == null)
        {
            return;
        }

        instance.PlayInternal();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindFirstObjectByType<LevelUpSfxPlayer>();
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(LevelUpSfxPlayer));
        instance = go.AddComponent<LevelUpSfxPlayer>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        levelUpClip = Resources.Load<AudioClip>(LevelUpResourcePath);
        PrewarmVoices(InitialVoiceCount);
    }

    private void PlayInternal()
    {
        if (levelUpClip == null)
        {
            if (!missingClipLogged)
            {
                Debug.LogWarning($"LevelUpSfxPlayer: '{LevelUpResourcePath}' 오디오를 찾지 못했습니다.");
                missingClipLogged = true;
            }

            return;
        }

        AudioSource voice = AcquireVoice();
        voice.pitch = 1f;
        voice.clip = levelUpClip;
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
