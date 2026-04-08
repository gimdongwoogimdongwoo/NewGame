using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeStopController : MonoBehaviour
{
    [Header("Time Stop")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private LayerMask affectedLayers = ~0;

    [Header("Screen Tint")]
    [SerializeField] private Image tintPanel;
    [SerializeField] private Color tintColor = new Color(0.4f, 0.5f, 0.7f, 0.35f);

    private static TimeStopController instance;
    private static bool isTimeStopped;

    private readonly Dictionary<Rigidbody2D, RigidbodyState> frozenBodies = new();
    private readonly Dictionary<Animator, float> frozenAnimators = new();

    private float stopEndTime;
    private float nextScanTime;

    public static bool IsTimeStopped => isTimeStopped;

    public float Duration
    {
        get => duration;
        set => duration = Mathf.Max(0.1f, value);
    }

    private void Awake()
    {
        instance = this;
        duration = Mathf.Max(0.1f, duration);

        ApplyTint(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            RestoreAll();
            instance = null;
            isTimeStopped = false;
        }
    }

    private void Update()
    {
        if (!isTimeStopped)
        {
            return;
        }

        if (Time.time >= stopEndTime)
        {
            EndTimeStop();
            return;
        }

        if (Time.time >= nextScanTime)
        {
            FreezeNewTargets();
            nextScanTime = Time.time + 0.1f;
        }
    }

    public static void TriggerTimeStop()
    {
        if (instance == null)
        {
            instance = Object.FindFirstObjectByType<TimeStopController>();
        }

        if (instance != null)
        {
            instance.StartOrRefresh();
        }
    }

    private void StartOrRefresh()
    {
        stopEndTime = Time.time + duration;

        if (!isTimeStopped)
        {
            isTimeStopped = true;
            FreezeNewTargets();
            ApplyTint(true);
        }
        else
        {
            // 다중 발동 시 지속시간 갱신
            FreezeNewTargets();
            ApplyTint(true);
        }
    }

    private void EndTimeStop()
    {
        isTimeStopped = false;
        RestoreAll();
        ApplyTint(false);
    }

    private void FreezeNewTargets()
    {
        Rigidbody2D[] bodies = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        for (int i = 0; i < bodies.Length; i++)
        {
            Rigidbody2D body = bodies[i];
            if (body == null || frozenBodies.ContainsKey(body))
            {
                continue;
            }

            if (!IsLayerAffected(body.gameObject.layer))
            {
                continue;
            }

            frozenBodies[body] = new RigidbodyState(body.simulated, body.linearVelocity, body.angularVelocity);
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        Animator[] animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null || frozenAnimators.ContainsKey(animator))
            {
                continue;
            }

            if (!IsLayerAffected(animator.gameObject.layer))
            {
                continue;
            }

            frozenAnimators[animator] = animator.speed;
            animator.speed = 0f;
        }
    }

    private void RestoreAll()
    {
        foreach (KeyValuePair<Rigidbody2D, RigidbodyState> pair in frozenBodies)
        {
            Rigidbody2D body = pair.Key;
            if (body == null)
            {
                continue;
            }

            body.simulated = pair.Value.Simulated;
            body.linearVelocity = pair.Value.LinearVelocity;
            body.angularVelocity = pair.Value.AngularVelocity;
        }

        frozenBodies.Clear();

        foreach (KeyValuePair<Animator, float> pair in frozenAnimators)
        {
            Animator animator = pair.Key;
            if (animator == null)
            {
                continue;
            }

            animator.speed = pair.Value;
        }

        frozenAnimators.Clear();
    }

    private bool IsLayerAffected(int layer)
    {
        return (affectedLayers.value & (1 << layer)) != 0;
    }

    private void ApplyTint(bool isVisible)
    {
        if (tintPanel == null)
        {
            return;
        }

        tintPanel.color = tintColor;
        tintPanel.gameObject.SetActive(isVisible);
    }

    private readonly struct RigidbodyState
    {
        public readonly bool Simulated;
        public readonly Vector2 LinearVelocity;
        public readonly float AngularVelocity;

        public RigidbodyState(bool simulated, Vector2 linearVelocity, float angularVelocity)
        {
            Simulated = simulated;
            LinearVelocity = linearVelocity;
            AngularVelocity = angularVelocity;
        }
    }
}
