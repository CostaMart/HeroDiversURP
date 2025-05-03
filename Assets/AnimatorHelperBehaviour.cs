using UnityEngine;

public class AnimatorHelperBehaviour : MonoBehaviour
{
    public bool isReloading = false;

    public void Reloading()
    {
        isReloading = true;
    }

    public void ReloadComplete()
    {
        isReloading = false;
    }
}
