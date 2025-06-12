using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CinemachineImpluseController : MonoBehaviour
{
    [SerializeField] private EventChannels eventChannels;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private string startEvent = "VFXEvent";
    [SerializeField] private string stopEvent = "BurstOff";
    [SerializeField] private Volume volume;

    private MotionBlur motionBlur;
    private float intensity;

    void Start()
    {
        // Subscribe agli eventi
        eventChannels.Subscribe(startEvent, GenerateImpulse);
        eventChannels.Subscribe(stopEvent, RemoveBlur);

        if (volume == null || volume.profile == null)
        {
            Debug.LogError("Volume o profilo non assegnato!");
            return;
        }

        // Ottieni Motion Blur dal profilo attivo (runtime)
        if (!volume.profile.TryGet(out motionBlur))
        {
            Debug.LogError("Motion Blur non trovato nel volume.profile!");
            return;
        }

        // Attiva l'override per sicurezza
        motionBlur.active = true;

        intensity = PlayerPrefs.GetFloat("strafeBlur", 0.6f);

        Debug.Log("Motion Blur trovato. Override attivo? " + motionBlur.active);
    }

    public void GenerateImpulse()
    {
        impulseSource.GenerateImpulse();
        ApplyMotionBlur();
    }

    public void ApplyMotionBlur()
    {
        if (motionBlur == null) return;

        motionBlur.active = true;
        motionBlur.intensity.value = intensity;

        Debug.Log($"ApplyMotionBlur: intensity = {motionBlur.intensity.value}");
    }

    public void RemoveBlur()
    {
        if (motionBlur == null) return;

        motionBlur.intensity.value = 0f;

        Debug.Log("RemoveBlur: intensity = 0");
    }
}