using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.ParticleSystem;
[CreateAssetMenu(fileName = "ClimateSys", menuName = "ClimateSys/SandStorm")]
public class SandStorm : Adversity
{
    Vector3 direction;
    [SerializeField] GameObject sandstorm;
    ParticleSystem particleSystem;
    private GameObject sandstormInstance;
    private GameObject player;
    private float offsetx;
    private float offsetz;
    [SerializeField] private float offsety = 10f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 15f;
    private Vector3 offsetFromPlayer;
    [SerializeField] public float targetRate = 200f;
    [SerializeField] public float growTime = 5f;    // tempo in secondi per crescere
    private float currentTime = 0f;
    private EmissionModule emission;
    [SerializeField] private float fogtarget = 0.1f;
    private float oldFog;
    private float oldIllumination;
    private Vignette vignette;
    private Volume volume;
    [SerializeField] private float vignettetarget = 0.5f;

    public override void DoEffect()
    {
        if (currentTime < growTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / growTime;
            emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, targetRate, t);
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, fogtarget, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, fogtarget, t);
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, vignettetarget, t);
        }

        sandstormInstance.transform.position = player.transform.position + new Vector3(offsetFromPlayer.x, offsety, offsetFromPlayer.z);
    }



    public override void SetupAdversity(GameObject origin, GameObject playerPos, EffectsDispatcher playerDispatcher, Volume volume)
    {
        oldIllumination = RenderSettings.ambientIntensity;
        oldFog = RenderSettings.fogDensity;
        currentTime = 0f;
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0; // lavoriamo solo sul piano XZ
        randomDirection.Normalize();

        float randomDistance = Random.Range(minDistance, maxDistance);

        Vector3 spawnPosition = playerPos.transform.position + randomDirection * randomDistance;

        Vector3 lookDirection = playerPos.transform.position - spawnPosition;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(0, -90f, 0);

        sandstormInstance = Instantiate(sandstorm, spawnPosition, lookRotation);


        particleSystem = sandstormInstance.GetComponent<ParticleSystem>();

        emission = particleSystem.emission;

        var targetPlayer = playerPos.transform;
        volume.profile.TryGet(out vignette);

        // Parti con vignetta disattivata
        vignette.intensity.value = 0f; offsetFromPlayer = sandstormInstance.transform.position - targetPlayer.position;
    }
    public override void Disable()
    {
        sandstormInstance.GetComponent<ParticleSystem>().Stop();
        Destroy(sandstormInstance);
    }

}
