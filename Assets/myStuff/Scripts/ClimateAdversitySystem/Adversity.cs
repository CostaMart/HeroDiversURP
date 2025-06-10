using UnityEngine;
using UnityEngine.Rendering;

public abstract class Adversity : ScriptableObject
{
    public bool isDisabling = false;

    public abstract void SetupAdversity(GameObject origin, GameObject playerPos, EffectsDispatcher playerDispacher, Volume volume, AudioSource audioSource);
    public abstract void DoEffect();
    public abstract void Disable();

}
