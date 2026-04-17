using System.Collections.Generic;
using UnityEngine;

public class SkillSfxPlayer : MonoBehaviour
{
    private const string ShootResourcePath = "SFX/shoot";
    private const string FireResourcePath = "SFX/fire";
    private const string BoomResourcePath = "SFX/boom";
    private const int InitialVoiceCount = 10;
    private const int MaxVoiceCount = 28;

    private static SkillSfxPlayer instance;

    private readonly List<AudioSource> voices = new();
    private AudioClip shootClip;
    private AudioClip fireClip;
    private AudioClip boomClip;
    private int nextVoiceIndex;

    private bool missingShootLogged;
    private bool missingFireLogged;
    private bool missingBoomLogged;

    public static void PlayProjectileHit()
    {
        EnsureInstance();
        instance?.Play(instance.shootClip, ShootResourcePath, 0.58f, ref instance.missingShootLogged);
    }

    public static void PlayFireRingHit()
    {
        EnsureInstance();
        instance?.Play(instance.fireClip, FireResourcePath, 0.5f, ref instance.missingFireLogged);
    }

    public static void PlayExplosionBoom()
    {
        EnsureInstance();
        instance?.Play(instance.boomClip, BoomResourcePath, 0.82f, ref instance.missingBoomLogged);
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindFirstObjectByType<SkillSfxPlayer>();
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(SkillSfxPlayer));
        instance = go.AddComponent<SkillSfxPlayer>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        shootClip = Resources.Load<AudioClip>(ShootResourcePath);
        fireClip = Resources.Load<AudioClip>(FireResourcePath);
        boomClip = Resources.Load<AudioClip>(BoomResourcePath);
        PrewarmVoices(InitialVoiceCount);
    }

    private void Play(AudioClip clip, string resourcePath, float volume, ref bool missingLogged)
    {
        if (clip == null)
        {
            if (!missingLogged)
            {
                Debug.LogWarning($"SkillSfxPlayer: '{resourcePath}' 오디오를 찾지 못했습니다.");
                missingLogged = true;
            }

            return;
        }

        AudioSource source = AcquireVoice();
        source.pitch = 1f;
        source.volume = Mathf.Clamp01(volume);
        source.clip = clip;
        source.time = 0f;
        source.Play();
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
        source.priority = 40;
        return source;
    }
}
