using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Importante per MotionBlur

public class CinemachineImpluseController : MonoBehaviour
{
    [SerializeField] EventChannels eventChannels;
    [SerializeField] CinemachineImpulseSource impulseSource;
    [SerializeField] string startEvent = "VFXEvent";
    [SerializeField] string stopEvent = "BurstOff";
    [SerializeField] Volume volume;

    private MotionBlur motionBlur;
    private float intensity;

    void Start()
    {
        eventChannels.Subscribe(startEvent, GenerateImpulse);
        eventChannels.Subscribe(stopEvent, RemoveBlur);

        // Clona il profilo se serve evitare modifiche globali
        volume.profile = Instantiate(volume.profile);

        // Recupera Motion Blur dal profilo
        if (!volume.profile.TryGet(out motionBlur))
        {
            Debug.LogWarning("Motion Blur non trovato nel profilo Volume.");
        }

        intensity = PlayerPrefs.GetFloat("strafeBlur", 0.6f);
    }

    public void GenerateImpulse()
    {
        impulseSource.GenerateImpulse();
        ApplyMotionBlur(); // Imposta intensity a 0.6
    }

    public void ApplyMotionBlur()
    {
        if (motionBlur == null) return;

        motionBlur.active = true;
        motionBlur.intensity.overrideState = true;
        motionBlur.intensity.value = intensity;

        // Puoi anche impostare il mode (es. Camera o Object)
        motionBlur.mode.overrideState = true;
        motionBlur.mode.value = MotionBlurMode.CameraOnly;
    }

    public void RemoveBlur()
    {
        motionBlur.active = false;
    }
}