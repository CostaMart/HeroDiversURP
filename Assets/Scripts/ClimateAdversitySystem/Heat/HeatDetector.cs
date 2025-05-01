using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemManager;

public class HeatStats : AbstractStatus
{
    public bool isExposedToSun = false;

    Transform heatsorce;
    [SerializeField] Transform rayCastRSource;
    [SerializeField] Transform rayCastLSource;

    [SerializeField] Vector3 leftRayOffset = new Vector3(-0.5f, 0, 0);
    [SerializeField] Vector3 rightRayOffset = new Vector3(0.5f, 0, 0);
    [SerializeField] EffectsDispatcher dispatcher;

    [Header("Raycast Settings")]
    public float raycastInterval = 0.2f; // Fisso: 5 volte al secondo

    [Header("Effect Timings")]

    private Modifier heat;
    private Modifier cooling;

    private float raycastTimer = 0f;
    private float timer = 0f;

    void Start()
    {
        heatsorce = GameObject.Find("heatSource").transform;

        heat = new Modifier
        {
            effects = new List<AbstractEffect>
            {
                new SingleActivationEffect(new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "target","@CharStats.8"},
                    {"expr","@CharStats.8 + @HeatStats.0"}
                }, 0, 0, false)
            }
        };

        cooling = new Modifier
        {
            effects = new List<AbstractEffect>
            {
                new SingleActivationEffect(new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "target","@CharStats.8"},
                    {"expr","@CharStats.8 - @CharStats.11"}
                }, 0, 0, false)
            }
        };
    }

    private void Update()
    {
        base.Update();
        var heatInterval = 1 / dispatcher.GetAllFeatureByType<float>(FeatureType.heatingRate).Sum();
        var coolingInterval = 1 / dispatcher.GetAllFeatureByType<float>(FeatureType.coolingRate).Sum();

        // Raycast 5 volte al secondo per aggiornare esposizione
        raycastTimer += Time.deltaTime;
        if (raycastTimer >= raycastInterval)
        {
            raycastTimer = 0f;
            CheckExposure();
        }

        timer += Time.deltaTime;
        if (isExposedToSun)
        {
            if (timer >= heatInterval)
            {
                dispatcher.modifierDispatch(heat);
                timer = 0f;
            }
        }
        else
        {
            if (timer >= coolingInterval)
            {
                dispatcher.modifierDispatch(cooling);
                timer = 0f;
            }
        }
    }

    private void CheckExposure()
    {
        isExposedToSun = false;
        float distance = Vector3.Distance(transform.position, heatsorce.position);

        Vector3 leftOrigin = rayCastLSource.position;
        Vector3 leftDir = (heatsorce.position - (transform.position + leftRayOffset)).normalized;
        if (Physics.Raycast(leftOrigin, leftDir, out RaycastHit hitLeft, distance))
        {
            Debug.DrawRay(leftOrigin, leftDir * distance, Color.green, raycastInterval);
            if (hitLeft.collider.CompareTag("Sun"))
            {
                isExposedToSun = true;
                return;
            }
        }

        Vector3 rightOrigin = rayCastRSource.position;
        Vector3 rightDir = (heatsorce.position - (transform.position + rightRayOffset)).normalized;
        if (Physics.Raycast(rightOrigin, rightDir, out RaycastHit hitRight, distance))
        {
            Debug.DrawRay(rightOrigin, rightDir * distance, Color.blue, raycastInterval);
            if (hitRight.collider.CompareTag("Sun"))
            {
                isExposedToSun = true;
                return;
            }
        }
    }

    protected override int ComputeID()
    {
        return ItemManager.statClassToIdRegistry["HeatStats"];
    }
}