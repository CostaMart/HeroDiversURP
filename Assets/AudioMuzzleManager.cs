using UnityEngine;

public class AudioMuzzleManager : MonoBehaviour
{

    public AudioClip[] firingAudioClips;
    public AudioClip[] emptyMagAudioClips;
    public AudioClip[] chargeAudioClips;

    private AudioSource audioSource;


    public void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = PlayerPrefs.GetFloat("shootingVolume", 0.5f);
        Debug.Log("AudioMuzzleManager initialized with volume: " + audioSource.volume);
    }

    public void EmitFireSound()
    {
        if (firingAudioClips.Length > 0)
        {
            AudioClip clip = firingAudioClips[Random.Range(0, firingAudioClips.Length)];
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(clip);
        }
    }

    public void EmitChargeSound()
    {
        if (firingAudioClips.Length > 0)
        {
            AudioClip clip = firingAudioClips[Random.Range(0, chargeAudioClips.Length)];
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopChargeSound()
    {
        if (firingAudioClips.Length > 0)
        {
            AudioClip clip = firingAudioClips[Random.Range(0, chargeAudioClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopFireSound()
    {
        if (firingAudioClips.Length > 0)
        {
            AudioClip clip = firingAudioClips[Random.Range(0, firingAudioClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
    public bool isPlaying()
    {
        return audioSource.isPlaying;
    }

    public void EmitEmptyMagSound()
    {
        if (emptyMagAudioClips.Length > 0)
        {
            AudioClip clip = emptyMagAudioClips[Random.Range(0, emptyMagAudioClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}
