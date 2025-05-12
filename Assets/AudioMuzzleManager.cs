using UnityEngine;

public class AudioMuzzleManager : MonoBehaviour
{

    public AudioClip[] firingAudioClips;
    public AudioClip[] emptyMagAudioClips;

    private AudioSource audioSource;


    public void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void EmitFireSound()
    {
        if (firingAudioClips.Length > 0)
        {
            AudioClip clip = firingAudioClips[Random.Range(0, firingAudioClips.Length)];
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }

    public void StopFireSound()
    {
        if (firingAudioClips.Length > 0)
        {
            AudioClip clip = firingAudioClips[Random.Range(0, firingAudioClips.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
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
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
