using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "ClimateSys", menuName = "ClimateSys/SandStorm")]
public class SandStorm : Adversity
{
    [Header("VFX Graph")]
    [SerializeField] private GameObject sandstormPrefab;
    private VisualEffect visualEffect;
    private GameObject sandstormInstance;

    [Header("Spawn Rate")]
    [SerializeField] public float targetRate = 200f;
    [SerializeField] public float growTime = 5f;
    private float currentTime = 0f;
    private float initialRate;

    [Header("Fog & Lighting")]
    [SerializeField] private float fogTarget = 0.1f;
    private float oldFog;
    private float oldIllumination;

    [Header("Vignette")]
    [SerializeField] private float vignetteTarget = 0.5f;
    private Volume volume;
    private Vignette vignette;

    [Header("Positioning")]
    [SerializeField] private float offsetY = 10f;
    private GameObject player;
    private Vector3 offsetFromPlayer;

    [Header("Audio")]
    [SerializeField] AudioClip audio;
    AudioSource audiosource;

    // Nome esatto della proprietà Float esposta nel VFX Graph
    private const string k_SpawnRateProperty = "SpawnRate";

    public override void SetupAdversity(GameObject origin, GameObject playerPos, EffectsDispatcher playerDispatcher, Volume vol, AudioSource source)
    {
        // Riferimenti
        player = playerPos;
        volume = vol;
        volume.profile.TryGet(out vignette);

        // Salva valori originali e resetti
        oldFog = RenderSettings.fogDensity;
        oldIllumination = RenderSettings.ambientIntensity;
        vignette.intensity.value = 0f;
        currentTime = 0f;

        // Imposta spawn direttamente sopra il player
        Vector3 spawnPos = player.transform.position + new Vector3(0, offsetY, 0);
        Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (sandstormInstance == null)
            sandstormInstance = Instantiate(sandstormPrefab, spawnPos, spawnRot);

        visualEffect = sandstormInstance.GetComponent<VisualEffect>();

        // Leggi il valore iniziale di spawn rate dal VFX Graph
        initialRate = visualEffect.GetFloat(k_SpawnRateProperty);

        // Mantieni l'offset costante per seguire il player
        offsetFromPlayer = new Vector3(0, offsetY, 0);
        visualEffect.Play();

        this.audiosource = source;
        this.isDisabling = false;

    }

    public float fadeOutTime = 5f; // Aggiungi questa variabile per controllare la durata del fade out
    private float disableTimer = 0f;
    private float currentSpawnRate;
    private float currentFogDensity;
    private float currentAmbientIntensity;
    private float currentVignette;

    public override void DoEffect()
    {
        if (!sandstormInstance.activeSelf)
            sandstormInstance.SetActive(true);

        // Se siamo in fase di disattivazione graduale
        if (isDisabling)
        {
            disableTimer += Time.deltaTime;
            float t = disableTimer / fadeOutTime;

            // Decrementa progressivamente i parametri
            float newRate = Mathf.Lerp(currentSpawnRate, 0f, t);
            visualEffect.SetFloat(k_SpawnRateProperty, newRate);

            RenderSettings.fogDensity = Mathf.Lerp(currentFogDensity, oldFog, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(currentAmbientIntensity, oldIllumination, t);
            vignette.intensity.value = Mathf.Lerp(currentVignette, 0f, t);

            // Quando il fade out è completato
            if (disableTimer >= fadeOutTime)
            {
                CompleteDisable();
            }
        }
        // Se la tempesta è attiva normalmente
        else if (currentTime < growTime)
        {
            if (!audiosource.isPlaying) audiosource.PlayOneShot(audio);

            // Incrementa progressivamente i parametri
            if (currentTime < growTime)
            {
                currentTime += Time.deltaTime;
                float t = currentTime / growTime;

                // Spawn rate nel VFX Graph
                float newRate = Mathf.Lerp(initialRate, targetRate, t);
                visualEffect.SetFloat(k_SpawnRateProperty, newRate);

                // Fog e ambient lighting
                RenderSettings.fogDensity = Mathf.Lerp(oldFog, fogTarget, t);
                RenderSettings.ambientIntensity = Mathf.Lerp(oldIllumination, fogTarget, t);
                vignette.intensity.value = Mathf.Lerp(0f, vignetteTarget, t);
            }
        }

        // Mantieni centrata sul player (anche durante il fade out)
        Vector3 pos = player.transform.position + offsetFromPlayer;
        sandstormInstance.transform.position = pos;
    }

    public void StartDisable()
    {
        if (isDisabling || sandstormInstance == null) return;

        isDisabling = true;
        disableTimer = 0f;

        // Salva i valori correnti per il fade out
        currentSpawnRate = visualEffect.GetFloat(k_SpawnRateProperty);
        currentFogDensity = RenderSettings.fogDensity;
        currentAmbientIntensity = RenderSettings.ambientIntensity;
        currentVignette = vignette.intensity.value;
    }

    private void CompleteDisable()
    {
        if (sandstormInstance != null)
        {
            sandstormInstance.gameObject.SetActive(false);
            visualEffect.Stop();
        }

        // Ripristina valori originali
        RenderSettings.fogDensity = oldFog;
        RenderSettings.ambientIntensity = oldIllumination;
        vignette.intensity.value = 0f;

        // Ferma l'audio
        if (audiosource.isPlaying) audiosource.Stop();
    }
    public override void Disable()
    {
        StartDisable();
    }
}
