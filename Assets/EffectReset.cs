using System.Collections;
using UnityEngine;

public class EffectReset : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Coroutine coroutine;
    public void StartResetTimer()
    {
        if (coroutine != null)
        {
            coroutine = StartCoroutine(ResetEffect());
        }
    }

    IEnumerator ResetEffect()
    {
        Debug.Log("Coroutine launched");
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

}
