using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMuzzleManager : MonoBehaviour
{

    public AudioClip[] firingAudioClips;
    public AudioClip[] emptyMagAudioClips;
    public AudioClip[] chargeAudioClips;
    public AudioMixerGroup shooting;

    private AudioSource fireAudioSource;
    private AudioSource chargeAudioSource;


    public void OnEnable()
    {
        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.outputAudioMixerGroup = shooting;
            chargeAudioSource = gameObject.AddComponent<AudioSource>();
        }

    }

    public void EmitFireSound()
    {
        if (firingAudioClips.Length > 0)
        {
            AudioClip clip = firingAudioClips[Random.Range(0, firingAudioClips.Length)];
            fireAudioSource.pitch = Random.Range(0.8f, 1.2f);
            fireAudioSource.PlayOneShot(clip);
        }
    }

    private Coroutine chargeSoundCoroutine;

    public void EmitChargeSound(float durationInSeconds, float targetPitch = 2.0f)
    {
        if (chargeSoundCoroutine != null)
        {
            StopCoroutine(chargeSoundCoroutine);
        }

        AudioClip clip = chargeAudioClips[Random.Range(0, chargeAudioClips.Length)];
        chargeAudioSource.clip = clip;
        chargeAudioSource.loop = true;
        chargeAudioSource.pitch = 1.0f;
        chargeAudioSource.Play();

        chargeSoundCoroutine = StartCoroutine(UpdatePitchOverTime(durationInSeconds, 1.0f, targetPitch));
    }

    private IEnumerator UpdatePitchOverTime(float duration, float startPitch, float endPitch)
    {
        while (true)
        {
            chargeAudioSource.pitch = Mathf.Lerp(chargeAudioSource.pitch, endPitch, 0.01f);
            yield return null;
        }
    }

    public void StopChargeSound()
    {
        if (chargeSoundCoroutine != null) StopCoroutine(chargeSoundCoroutine);
        chargeAudioSource.Stop();
    }

    public void StopFireSound()
    {
        if (chargeSoundCoroutine != null) StopCoroutine(chargeSoundCoroutine);
        fireAudioSource.Stop();
    }
    public bool isPlaying()
    {
        return fireAudioSource.isPlaying;
    }

    public void EmitEmptyMagSound()
    {
        if (emptyMagAudioClips.Length > 0)
        {
            AudioClip clip = emptyMagAudioClips[Random.Range(0, emptyMagAudioClips.Length)];
            fireAudioSource.PlayOneShot(clip);
        }
    }
}
