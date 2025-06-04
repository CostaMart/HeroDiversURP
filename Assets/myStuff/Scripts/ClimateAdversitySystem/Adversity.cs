using UnityEngine;
using UnityEngine.Rendering;

public abstract class Adversity : ScriptableObject
{

    public abstract void SetupAdversity(GameObject origin, GameObject playerPos, EffectsDispatcher playerDispacher, Volume volume);
    public abstract void DoEffect();
    public abstract void Disable();

}
