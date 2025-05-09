using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource walking;
    [SerializeField] AudioClip[] walkingClips;

    [SerializeField] AudioSource thrustersExplosion;
    [SerializeField] AudioSource fireSource;
    [SerializeField] AudioClip[] thrusterExplosions;
    [SerializeField] AudioClip[] fireSounds;



    public void EmitWalkingSound()
    {
        int index = Random.Range(0, walkingClips.Length);
        if (walking != null)
        {
            walking.clip = walkingClips[index];
            walking.Play();
        }
    }

    public void EmitThrusterExplosion()
    {
        int index = Random.Range(0, thrusterExplosions.Length);
        if (thrustersExplosion != null)
        {
            thrustersExplosion.clip = thrusterExplosions[index];
            thrustersExplosion.Play();
        }
    }

    public void EmitFireSound()
    {
        int index = Random.Range(0, fireSounds.Length);
        if (fireSource != null)
        {
            Debug.Log("Playing fire sound: " + fireSounds[index].name);
            fireSource.clip = fireSounds[index];
            fireSource.Play();
        }
    }

    public void StopFireSound()
    {
        if (fireSource != null)
        {
            Debug.Log("Stopping fire sound");
            fireSource.Stop();
        }
    }
}
