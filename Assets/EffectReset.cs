using System.Collections;
using UnityEngine;

public class EffectReset : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float timeToReset = 0.5f; // Time to wait before resetting the effect
    Coroutine coroutine;
    Transform origin;
    ParticleSystem sys;
    public void OnEnable()
    {
        origin = this.gameObject.transform.parent;
        coroutine = null;
    }
    public void StartResetTimer()
    {
        if (coroutine == null)
        {
            if (this.gameObject.activeSelf)
                coroutine = StartCoroutine(ResetEffect());
        }
    }

    IEnumerator ResetEffect()
    {

        Debug.Log("Coroutine launched effects about to be reset");
        yield return new WaitForSeconds(timeToReset);
        this.gameObject.transform.SetParent(origin);
        gameObject.SetActive(false);
    }

}
