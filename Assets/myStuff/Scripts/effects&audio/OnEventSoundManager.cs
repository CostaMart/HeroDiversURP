using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class OnEventSoundManager : MonoBehaviour
{
    [SerializeField] EventChannels eventChannels;
    [SerializeField] AudioSource soundSource;
    [SerializeField] AudioClip[] soundClips;
    [SerializeField] string startEvent = "VFXEvent";
    [SerializeField] string stopEvent;
    [SerializeField] bool temporized = false;
    [SerializeField] float soundDuration = 2.0f;

    private Coroutine activeSoundCoroutine;
    private float volume;

    void Start()
    {
        eventChannels.Subscribe(startEvent, EmitSound);
        if (!string.IsNullOrEmpty(stopEvent))
            eventChannels.Subscribe(stopEvent, StopSound);
    }

    public void EmitSound()
    {
        if (soundClips.Length == 0 || soundSource == null) return;

        soundSource.PlayOneShot(soundClips[Random.Range(0, soundClips.Length)]);

        if (temporized)
        {
            if (activeSoundCoroutine != null)
            {
                StopCoroutine(activeSoundCoroutine);
            }
            activeSoundCoroutine = StartCoroutine(StopAfterDelay());
        }
    }

    private IEnumerator StopAfterDelay()
    {
        yield return new WaitForSeconds(soundDuration);
        StopSound();
    }

    public void StopSound()
    {
        if (soundSource == null) return;

        soundSource.Stop();

        if (activeSoundCoroutine != null)
        {
            StopCoroutine(activeSoundCoroutine);
            activeSoundCoroutine = null;
        }
    }
}