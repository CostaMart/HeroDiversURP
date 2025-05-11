using CartoonFX;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class FOotDustControl : MonoBehaviour
{
    [SerializeField] ParticleSystem vfx;
    [SerializeField] EventChannels eventChannels;
    GameObject container;
    [SerializeField] string startEvent = "VFXEvent";
    [SerializeField] string stopEvent;
    [SerializeField] bool temporized = false;
    [SerializeField] float effectDuration = 2.0f;

    private Coroutine activeEffectCoroutine;

    public void Awake()
    {
        vfx.Stop();
    }

    private void Start()
    {
        eventChannels.Subscribe(startEvent, PlayVFX);
        if (!string.IsNullOrEmpty(stopEvent))
            eventChannels.Subscribe(stopEvent, StopVFX);
        container = vfx.gameObject;
    }

    public void PlayVFX()
    {
        if (vfx == null) return;


        container.transform.position = transform.position;
        vfx.Play();

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
