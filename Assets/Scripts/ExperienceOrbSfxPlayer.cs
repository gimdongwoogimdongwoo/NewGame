using System.Collections.Generic;
using UnityEngine;

public class ExperienceOrbSfxPlayer : MonoBehaviour
{
    private const string PopResourcePath = "SFX/pop";
    private const int InitialVoiceCount = 12;
    private const int MaxVoiceCount = 24;
    private const float VoiceVolume = 0.8f;

    private static ExperienceOrbSfxPlayer instance;

    private readonly List<AudioSource> voices = new();
    private AudioClip popClip;
    private int nextVoiceIndex;
    private bool missingClipLogged;

    public static void PlayPop()
    {
        EnsureInstance();
        if (instance == null)
        {
            return;
        }

        instance.PlayPopInternal();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindFirstObjectByType<ExperienceOrbSfxPlayer>();
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(ExperienceOrbSfxPlayer));
        instance = go.AddComponent<ExperienceOrbSfxPlayer>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        popClip = Resources.Load<AudioClip>(PopResourcePath);
        PrewarmVoices(InitialVoiceCount);
    }

    private void PlayPopInternal()
    {
        if (popClip == null)
        {
            if (!missingClipLogged)
            {
                Debug.LogWarning($"ExperienceOrbSfxPlayer: '{PopResourcePath}' 오디오를 찾지 못했습니다.");
                missingClipLogged = true;
            }

            return;
        }

        AudioSource voice = AcquireVoice();
        voice.pitch = 1f;
        voice.clip = popClip;
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
