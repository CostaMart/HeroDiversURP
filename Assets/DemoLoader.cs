using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DemoLoader : MonoBehaviour
{
    public GameObject spaceship;
    public float delayBeforeSceneActivation = 5f;
    public Image p; // immagine UI nera
    public Camera mainCamera;

    public float fadeDuration = 1f;

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 0.1f;

    private AsyncOperation loadingOperation;
    private float timer = 0f;
    private bool isLoading = false;
    private bool fadeStarted = false;
    public ParticleSystem launcheffect;

    private Vector3 originalCameraPosition;
    private bool loading = false;

    public void LoadDemo()
    {
        if (!loading)
        {
            loading = true;
            loadingOperation = SceneManager.LoadSceneAsync("DesertLevelHeroDivers");
            loadingOperation.allowSceneActivation = false;

            DeployShip();
            isLoading = true;
            timer = 0f;
            fadeStarted = false;

            Color c = p.color;
            c.a = 0f;
            p.color = c;
            p.gameObject.SetActive(true);

            if (mainCamera != null)
                StartCoroutine(ShakeCamera());
        }
    }

    void Update()
    {
        if (isLoading)
        {
            timer += Time.deltaTime;

            if (loadingOperation.progress >= 0.9f && timer >= delayBeforeSceneActivation)
            {
                if (!fadeStarted)
                {
                    fadeStarted = true;
                    StartCoroutine(FadeOutAndActivate());
                }
            }
        }
    }

    IEnumerator FadeOutAndActivate()
    {
        float t = 0f;
        Color c = p.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            p.color = c;
            yield return null;
        }

        loadingOperation.allowSceneActivation = true;
        isLoading = false;
    }

    public void DeployShip()
    {
        spaceship.SetActive(true);
        launcheffect.Play();
    }

    IEnumerator ShakeCamera()
    {
        originalCameraPosition = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null; // aspetta il frame successivo
        }

        mainCamera.transform.localPosition = originalCameraPosition;
    }
}