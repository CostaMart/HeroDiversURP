using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static ItemManager;

public class HeatStats : AbstractStatsClass
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
    private Modifier overHeatMalus;
    private Modifier overHeatMalusCounter;

    private float raycastTimer = 0f;
    private float timer = 0f;
    private int layermask;

    void Start()
    {
        layermask = LayerMask.GetMask("Sun", "Player", "Default", "Terrain");

        string path = Path.Combine(Application.streamingAssetsPath, "gameConfig/HeatSystem.json");
        var mod = JsonConvert.DeserializeObject<Dictionary<string, ModifierLoader>>(File.ReadAllText(path));

        heatsorce = GameObject.Find("heatSource").transform;
        var effects = new List<AbstractEffect>();

        foreach (var effect in mod["heatModifier"].effects)
        {
            effects.Add(new SingleActivationEffect(new Dictionary<string, string>
            {
                { "effectType", effect.effectType },
                { "target", effect.target },
                { "expr", effect.expr }
            }, 0, 0, false));
        }

        heat = new Modifier
        {
            effects = effects
        };

        // Inizializza il modificatore di raffreddamento
        effects = new List<AbstractEffect>();

        foreach (var effect in mod["coolingModifier"].effects)
        {
            effects.Add(new SingleActivationEffect(new Dictionary<string, string>
            {
                { "effectType", effect.effectType },
                { "target", effect.target },
                { "expr", effect.expr }
            }, 0, 0, false));
        }

        cooling = new Modifier
        {
            effects = effects
        };

        effects = new List<AbstractEffect>();

        foreach (var effect in mod["overHeatMalus"].effects)
        {
            effects.Add(new SingleActivationEffect(new Dictionary<string, string>
            {
                { "effectType", effect.effectType },
                { "target", effect.target },
                { "expr", effect.expr }
            }, 0, 0, false));
        }

        overHeatMalus = new Modifier
        {
            effects = effects
        };


        effects = new List<AbstractEffect>();

        foreach (var effect in mod["overHeatMalusCounter"].effects)
        {
            effects.Add(new SingleActivationEffect(new Dictionary<string, string>
            {
                { "effectType", effect.effectType },
                { "target", effect.target },
                { "expr", effect.expr }
            }, 0, 0, false));
        }

        overHeatMalusCounter = new Modifier
        {
            effects = effects
        };

    }

    bool overHeat = false;
    private void Update()
    {
        base.Update();
        var heatInterval = 1 / dispatcher.GetFeatureByType<float>(FeatureType.heatingRate).Sum();
        var coolingInterval = 1 / dispatcher.GetFeatureByType<float>(FeatureType.coolingRate).Sum();
        var currentHeatLvl = dispatcher.GetFeatureByType<float>(FeatureType.heat).Sum();
        var overHeatLimit = dispatcher.GetFeatureByType<float>(FeatureType.overHeatLimit).Sum();



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

        if (currentHeatLvl >= overHeatLimit)
        {
            dispatcher.modifierDispatch(overHeatMalus);
            overHeat = true;
            return;
        }

        if (overHeat)
        {
            // Reset overheat state
            overHeat = false;
            dispatcher.modifierDispatch(overHeatMalusCounter);
        }

    }

    private void CheckExposure()
    {
        isExposedToSun = false;
        float distance = Vector3.Distance(transform.position, heatsorce.position);

        Vector3 leftOrigin = rayCastLSource.position;
        Vector3 leftDir = (heatsorce.position - (transform.position + leftRayOffset)).normalized;
        if (Physics.Raycast(leftOrigin, leftDir, out RaycastHit hitLeft, distance, layermask))
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
        if (Physics.Raycast(rightOrigin, rightDir, out RaycastHit hitRight, distance, layermask))
        {
            Debug.DrawRay(rightOrigin, rightDir * distance, Color.blue, raycastInterval);
            if (hitRight.collider.CompareTag("Sun"))
            {
                isExposedToSun = true;
                return;
            }
        }
    }

    [System.Serializable]
    public class ModifierLoader
    {
        public int id;
        public string name;
        public List<Effect> effects;
    }
    public class Effect
    {
        public string effectType;
        public string target;
        public string expr;
    }
}