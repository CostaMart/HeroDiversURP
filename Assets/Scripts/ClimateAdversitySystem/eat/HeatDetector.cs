using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using static ItemManager;

public class HeatDetector : MonoBehaviour
{
    RaycastHit[] hitl = new RaycastHit[1];
    RaycastHit[] hitr = new RaycastHit[1];
    RaycastHit[] hitc = new RaycastHit[1];
    public bool isExposedToSun = false;
    [SerializeField] Transform heatsorce;

    // Offset fields for the rays
    [SerializeField] Vector3 leftRayOffset = new Vector3(-0.5f, 0, 0);
    [SerializeField] Vector3 rightRayOffset = new Vector3(0.5f, 0, 0);
    [SerializeField] Vector3 centerRayOffset = new Vector3(0, 0, 0);
    [SerializeField] EffectsDispatcher dispatcher;
    private Modifier heat;
    private Modifier cooling;



    void Start()
    {
        // create an utility modifier to change temperature
        heat = new Modifier();
        heat.effects = new List<AbstractEffect>();
        heat.name = "magConsumptionSecondary";

        heat.effects.Add(new SingleActivationEffect(
                new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "effectName", "armor heating" },
                    { "description", "heats player armor" },
                    { "inGamePrice", "0" },
                    { "gameIconId", "0" },
                    { "target","@CharStats.8"},
                    {"expr","@CharStats.8 + 0.5"}

                }, 0, 0, false));

        cooling = new Modifier();
        cooling.effects = new List<AbstractEffect>();
        cooling.name = "magConsumptionSecondary";

        cooling.effects.Add(new SingleActivationEffect(
                new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "effectName", "armor cooling" },
                    { "description", "cools player's armor" },
                    { "inGamePrice", "0" },
                    { "gameIconId", "0" },
                    { "target","@CharStats.8"},
                    {"expr","@CharStats.8 - @CharStats.11"}

                }, 0, 0, false));
    }

    float dispatchTimer = 0f; // Aggiungilo come variabile fuori da Update

    void Update()
    {
        // Timer per il Dispatch

        float distance = Vector3.Distance(transform.position, heatsorce.position);

        // Left ray with offset
        Vector3 leftOrigin = transform.position + leftRayOffset;
        Vector3 leftDirection = (heatsorce.position - leftOrigin).normalized;
        Ray rl = new Ray(leftOrigin, leftDirection);
        Physics.RaycastNonAlloc(rl, hitl, distance);

        Debug.DrawRay(leftOrigin, leftDirection * distance, Color.green); // Disegno ray sinistro

        // Right ray with offset
        Vector3 rightOrigin = transform.position + rightRayOffset;
        Vector3 rightDirection = (heatsorce.position - rightOrigin).normalized;
        Ray rr = new Ray(rightOrigin, rightDirection);
        Physics.RaycastNonAlloc(rr, hitr, distance);

        Debug.DrawRay(rightOrigin, rightDirection * distance, Color.blue); // Disegno ray destro

        if (hitl[0].collider != null && hitl[0].collider.CompareTag("Sun"))
        {
            Debug.Log("it's hot here!");
            isExposedToSun = true;
            HeatPlayer();
            return;
        }
        if (hitr[0].collider != null && hitr[0].collider.CompareTag("Sun"))
        {
            Debug.Log("it's hot here!");
            isExposedToSun = true;
            HeatPlayer();
            return;
        }

        // Nessun raggio ha colpito il sole
        HeatPlayer();
        isExposedToSun = false;
    }
    private void HeatPlayer()
    {

        dispatchTimer += Time.deltaTime;
        if (dispatchTimer >= 1f)
        {
            dispatcher.ItemDispatch(isExposedToSun ? heat : cooling);
            dispatchTimer = 0f; // Resetto il timer
        }
    }


}