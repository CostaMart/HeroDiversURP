using UnityEngine;

public class WeaponEffectControl : MonoBehaviour
{
    public enum Direction
    {
        Positive,
        Negative
    }

    [System.Serializable]
    public class KickAxisSettings
    {
        public bool enabled = false;
        public Direction direction = Direction.Positive;
        public float distance = 0.1f;
    }

    [System.Serializable]
    public class TargetKickSettings
    {
        public Transform target;

        public KickAxisSettings xAxis = new KickAxisSettings();
        public KickAxisSettings yAxis = new KickAxisSettings();
        public KickAxisSettings zAxis = new KickAxisSettings();

        // Memorizza posizione iniziale
        [HideInInspector] public Vector3 initialPosition;
    }

    [Header("Kick Settings")]
    public float kickDuration = 0.1f;

    [Header("Random Offset")]
    [Tooltip("Intensità della variazione casuale sugli assi secondari")]
    public float randomOffsetAmount = 0.01f;

    [Header("Targets Settings")]
    public TargetKickSettings containerSettings = new TargetKickSettings();
    public TargetKickSettings frontHandleSettings = new TargetKickSettings();
    public TargetKickSettings backHandleSettings = new TargetKickSettings();

    private bool isKicking = false;
    private float kickTimer = 0f;

    private readonly TargetKickSettings[] allTargets;

    private void Awake()
    {
        // Associa i target a mano se non assegnati
        if (containerSettings.target == null)
            containerSettings.target = GameObject.Find("Container")?.transform;

        if (frontHandleSettings.target == null)
            frontHandleSettings.target = GameObject.Find("frontHandle")?.transform;

        if (backHandleSettings.target == null)
            backHandleSettings.target = GameObject.Find("backHandle")?.transform;

        CacheInitialPositions();
    }

    void OnEnable()
    {
        // Associa i target a mano se non assegnati
        if (containerSettings.target == null)
            containerSettings.target = GameObject.Find("Container")?.transform;

        if (frontHandleSettings.target == null)
            frontHandleSettings.target = GameObject.Find("frontHandle")?.transform;

        if (backHandleSettings.target == null)
            backHandleSettings.target = GameObject.Find("backHandle")?.transform;
    }

    void CacheInitialPositions()
    {
        if (containerSettings.target != null)
            containerSettings.initialPosition = containerSettings.target.localPosition;
        if (frontHandleSettings.target != null)
            frontHandleSettings.initialPosition = frontHandleSettings.target.localPosition;
        if (backHandleSettings.target != null)
            backHandleSettings.initialPosition = backHandleSettings.target.localPosition;
    }

    void Update()
    {
        if (!isKicking) return;

        kickTimer += Time.deltaTime;
        float halfDuration = kickDuration / 2f;

        UpdateKickForTarget(containerSettings, kickTimer, halfDuration);
        UpdateKickForTarget(frontHandleSettings, kickTimer, halfDuration);
        UpdateKickForTarget(backHandleSettings, kickTimer, halfDuration);

        if (kickTimer > kickDuration)
        {
            isKicking = false;
            kickTimer = 0f;
        }
    }

    void UpdateKickForTarget(TargetKickSettings t, float kickTimer, float halfDuration)
    {
        if (t.target == null) return;

        Vector3 offset = CalculateOffset(t);

        if (kickTimer <= halfDuration)
        {
            float tFactor = Mathf.SmoothStep(0f, 1f, kickTimer / halfDuration);
            t.target.localPosition = t.initialPosition + offset * tFactor;
        }
        else if (kickTimer <= kickDuration)
        {
            float tFactor = Mathf.SmoothStep(0f, 1f, (kickTimer - halfDuration) / halfDuration);
            t.target.localPosition = t.initialPosition + offset * (1f - tFactor);
        }
        else
        {
            t.target.localPosition = t.initialPosition;
        }
    }

    Vector3 CalculateOffset(TargetKickSettings t)
    {
        Vector3 offset = Vector3.zero;

        if (t.xAxis.enabled)
        {
            offset += (t.xAxis.direction == Direction.Positive ? Vector3.right : Vector3.left) * t.xAxis.distance;
        }

        if (t.yAxis.enabled)
        {
            offset += (t.yAxis.direction == Direction.Positive ? Vector3.up : Vector3.down) * t.yAxis.distance;
        }

        if (t.zAxis.enabled)
        {
            offset += (t.zAxis.direction == Direction.Positive ? Vector3.forward : Vector3.back) * t.zAxis.distance;
        }

        // Random offset per naturalezza, ortogonale all'offset principale
        Vector3 random = new Vector3(
            Random.Range(-randomOffsetAmount, randomOffsetAmount),
            Random.Range(-randomOffsetAmount, randomOffsetAmount),
            Random.Range(-randomOffsetAmount, randomOffsetAmount)
        );

        Vector3 mainAxis = offset.normalized;
        Vector3 randomOffset = random - Vector3.Project(random, mainAxis);

        return offset + randomOffset;
    }

    public void PlayShootEffect()
    {
        if (isKicking) return;

        isKicking = true;
        kickTimer = 0f;
    }
}