using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PostProcessor : InteractiveObject
{

    public GameObject LostScreen;
    public GameObject deathText;
    private static PostProcessor _instance;
    public static PostProcessor Instance
    {
        get
        {
            return _instance;
        }
    }
    public Image damageImage;

    protected override void Awake()
    {
        if (_instance == null)
            _instance = this;

        base.Awake();

        RegisterAction(ActionRegistry.ENABLE_LOST_SCREEN, (_) => Lost());
        RegisterAction(ActionRegistry.PLAY_SOUND, EmitGenericSoundEffect);
        RegisterAction(ActionRegistry.ENABLE_WIN_SCREEN, (_) => Win());
    }
    public void ShowDamageEffect(float duration, float alpha)
    {

        StartCoroutine(ShowDamageEffectCoroutine(duration, alpha));
    }

    public void Lost()
    {
        LostScreen.SetActive(true);
        damageImage.gameObject.SetActive(true);
        damageImage.color = new Color(1f, 1f, 1f, 0.8f);
        deathText.SetActive(true);
    }

    [SerializeField] GameObject winScreen;
    public void Win()
    {
        winScreen.SetActive(true);
    }

    private IEnumerator ShowDamageEffectCoroutine(float duration, float alpha)
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
    public void EmitGenericSoundEffect(object[] args)
    {
        if (args.Length > 0 && args[0] is AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}