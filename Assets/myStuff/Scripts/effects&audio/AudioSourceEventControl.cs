using UnityEngine;

using UnityEngine;
using System.Collections;

public class AudioSourceEventControl : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] EventChannels eventChannels;
    [SerializeField] string startEvent = "AudioStart";
    [SerializeField] string stopEvent;
    [SerializeField] bool temporized = false;
    [SerializeField] float playDuration = 2.0f;

    private Coroutine activeAudioCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void Start()
    {
        if (eventChannels != null)
        {
            eventChannels.Subscribe(startEvent, PlayAudio);

            if (!string.IsNullOrEmpty(stopEvent))
                eventChannels.Subscribe(stopEvent, StopAudio);
        }
    }

    public void PlayAudio()
    {
        if (audioSource == null) return;

        audioSource.Play();

        if (temporized)
        {
            if (activeAudioCoroutine != null)
                StopCoroutine(activeAudioCoroutine);

            activeAudioCoroutine = StartCoroutine(StopAfterDelay());
        }
    }

    private IEnumerator StopAfterDelay()
    {
        yield return new WaitForSeconds(playDuration);
        StopAudio();
    }

    public void StopAudio()
    {
        if (audioSource == null) return;

        audioSource.Stop();

        if (activeAudioCoroutine != null)
        {
            StopCoroutine(activeAudioCoroutine);
            activeAudioCoroutine = null;
        }
    }
}