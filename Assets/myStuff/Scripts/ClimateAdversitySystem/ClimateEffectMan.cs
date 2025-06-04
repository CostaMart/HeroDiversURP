using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class SandStormDispatcher : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private float MaximpulseInterval = 0.1f; // Intervallo tra gli impulsi
    [SerializeField] private float MinimpulseInterval = 0.05f; // Intervallo tra gli impulsi
    [SerializeField] private float impulseDuration = 0.5f; // Durata dell'impulso
    [SerializeField] private float impulseForce = 1.0f; // Forza dell'impulso
    private bool isInTrigger = false;

    private ClimateEffectController climateEffectOrigin;
    [SerializeField] private SandStorm sandstorm;

    // Iniziamo a generare impulsi quando il giocatore entra nel trigger

    private void Start()
    {
        climateEffectOrigin = GameObject.Find("ClimateRef").GetComponent<ClimateEffectController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //            isInTrigger = true;

            //           impulseSource.ImpulseDefinition.ImpulseDuration = impulseDuration;
            //          impulseSource.ImpulseDefinition.AmplitudeGain = impulseForce;
            //         StartCoroutine(GenerateImpulses()); // Avvia la coroutine che invia gli impulsi

            climateEffectOrigin.InjectAdversity(sandstorm);


        }
    }

    // Continuo invio di impulsi finché il giocatore è nel trigger
    private IEnumerator GenerateImpulses()
    {
        while (isInTrigger)
        {
            var impulserinterval = UnityEngine.Random.Range(MinimpulseInterval, MaximpulseInterval); // Intervallo casuale tra gli impulsi
            impulseSource.GenerateImpulse(); // Genera l'impulso
            yield return new WaitForSeconds(impulserinterval); // Attendi prima di inviare un altro impulso
        }
    }

    // Fermiamo l'invio degli impulsi quando il giocatore esce dal trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = false;
            Debug.Log("Impulse Stopped (Player left trigger)");
        }
    }
}
