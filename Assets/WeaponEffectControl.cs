using System.Collections.Generic;
using UnityEngine;

public class WeaponEffectControl : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public enum Direction
    {
        Positive,
        Negative
    }

    [Header("Kick Settings")]
    public float kickDuration = 0.1f;

    [Header("Random Offset")]
    [Tooltip("Intensità della variazione casuale sugli assi secondari")]
    public float randomOffsetAmount = 0.01f;

    [Header("Target Transforms")]
    [SerializeField] private List<TargetKickSettings> kickTargets;

    private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> activeOffsets = new Dictionary<Transform, Vector3>();
    private bool isKicking = false;
    private float kickTimer = 0f;

    // Classe per memorizzare le impostazioni di ciascun target
    [System.Serializable]
    public class TargetKickSettings
    {
        public Transform target;          // Transform da muovere
        public Axis kickAxis = Axis.Z;   // Asse di movimento per il target
        public Direction kickDirection = Direction.Positive; // Direzione della spinta (positiva o negativa)
        public float kickDistance = 0.1f; // Kick distance personalizzata per ogni target
    }

    void Start()
    {
        foreach (var t in kickTargets)
        {
            if (t.target != null && !initialPositions.ContainsKey(t.target))
            {
                initialPositions[t.target] = t.target.localPosition;
            }
        }
    }

    void FixedUpdate()
    {
        if (!isKicking) return;

        kickTimer += Time.deltaTime;
        float halfDuration = kickDuration / 2f;

        foreach (var t in kickTargets)
        {
            Vector3 startPos = initialPositions[t.target];
            Vector3 offset = activeOffsets.ContainsKey(t.target) ? activeOffsets[t.target] : Vector3.zero;

            if (kickTimer <= halfDuration)
            {
                float tFactor = Mathf.SmoothStep(0f, 1f, kickTimer / halfDuration);
                t.target.localPosition = startPos + offset * tFactor;
            }
            else if (kickTimer <= kickDuration)
            {
                float tFactor = Mathf.SmoothStep(0f, 1f, (kickTimer - halfDuration) / halfDuration);
                t.target.localPosition = startPos + offset * (1f - tFactor);
            }
            else
            {
                t.target.localPosition = startPos;
            }
        }

        if (kickTimer > kickDuration)
        {
            isKicking = false;
            kickTimer = 0f;
        }
    }


    public void PlayShootEffect()
    {
        if (isKicking) return;

        isKicking = true;
        kickTimer = 0f;
        activeOffsets.Clear();

        foreach (var t in kickTargets)
        {
            Vector3 dir = GetDirectionVector(t.target, t.kickAxis, t.kickDirection);

            Vector3 random = new Vector3(
                Random.Range(-randomOffsetAmount, randomOffsetAmount),
                Random.Range(-randomOffsetAmount, randomOffsetAmount),
                Random.Range(-randomOffsetAmount, randomOffsetAmount)
            );

            Vector3 mainAxis = dir.normalized;
            Vector3 randomOffset = random - Vector3.Project(random, mainAxis);

            Vector3 fullOffset = (dir * t.kickDistance) + randomOffset; // Usa kickDistance personalizzata

            activeOffsets[t.target] = fullOffset;
        }
    }

    private Vector3 GetDirectionVector(Transform target, Axis axis, Direction direction)
    {
        Vector3 dir = axis switch
        {
            Axis.X => Vector3.right,
            Axis.Y => Vector3.up,
            Axis.Z => Vector3.forward,
            _ => Vector3.forward // Default in caso di errore
        };

        // Modifica la direzione in base alla selezione dell'enum
        if (direction == Direction.Negative)
        {
            dir = -dir;
        }

        return dir;
    }
}
