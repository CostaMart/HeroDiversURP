using CartoonFX;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class ParticleSystemController : MonoBehaviour
{
    ParticleSystem vfx;
    [SerializeField] EventChannels eventChannels;
    [SerializeField] CFXR_Effect cFXR_Effect;
    [SerializeField] string startEvent = "VFXEvent";
    [SerializeField] string stopEvent;
    [SerializeField] bool temporized = false;
    [SerializeField] float effectDuration = 2.0f;

    private Coroutine activeEffectCoroutine;

    public void Awake()
    {
        vfx = GetComponent<ParticleSystem>();
        vfx.Stop();
    }

    private void Start()
    {
        eventChannels.Subscribe(startEvent, PlayVFX);
        if (!string.IsNullOrEmpty(stopEvent))
            eventChannels.Subscribe(stopEvent, StopVFX);
    }

    public void PlayVFX()
    {
        if (vfx == null) return;

        vfx.Play();

        if (cFXR_Effect != null)
        {
            cFXR_Effect.Animate(0.2f);
        }

        if (temporized)
        {
            if (activeEffectCoroutine != null)
            {
                StopCoroutine(activeEffectCoroutine);
            }
            activeEffectCoroutine = StartCoroutine(StopAfterDelay());
        }
    }

    private IEnumerator StopAfterDelay()
    {
        yield return new WaitForSeconds(effectDuration);
        StopVFX();
    }

    public void StopVFX()
    {
        if (vfx == null) return;

        vfx.Stop();

        if (activeEffectCoroutine != null)
        {
            StopCoroutine(activeEffectCoroutine);
            activeEffectCoroutine = null;
        }
    }
}
