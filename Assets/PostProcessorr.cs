using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PostProcessor : MonoBehaviour
{

    private static PostProcessor _instance;
    public static PostProcessor instance
    {
        get
        {
            return _instance;
        }
    }
    public Image damageImage;

    public void Awake()
    {
        if (_instance == null)
            _instance = this;

        DontDestroyOnLoad(gameObject);
    }
    public void ShowDamageEffect(float duration, float alpha)
    {

        StartCoroutine(ShowDamageEffectCoroutine(duration, alpha));
    }

    public IEnumerator ShowDamageEffectCoroutine(float duration, float alpha)
    {

        damageImage.gameObject.SetActive(true);
        damageImage.color = new Color(1f, 1f, 1f, alpha);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(alpha, 0f, elapsedTime / duration);
            damageImage.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        {
            damageImage.gameObject.SetActive(false);
        }


    }

    public AudioSource audioSource;
    public void EmitGenericSoundEffect(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

}