using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StormTrigger : MonoBehaviour
{
    [SerializeField] Adversity storm;
    stormSettings settings;
    private float timer;


    public void Start()
    {
        string file = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "gameConfig/stormSettings.json"));
        settings = JsonUtility.FromJson<stormSettings>(file);
    }



    private void OnTriggerStay(Collider other)
    {

        if (ClimateEffectController.Instance.active) return;

        if (other.CompareTag("Player"))
        {


            if (timer >= settings.activationCheckInterval)
            {
                Debug.Log("Storm activation check " + timer + " >= " + settings.activationCheckInterval);
                int choosen = Random.Range(0, 100);

                if (choosen <= settings.chance)
                {
                    Debug.Log("Storm activated");
                    ClimateEffectController.Instance.InjectAdversity(storm, settings.minDuration, settings.maxDuration);
                }

                timer = 0;
                return;
            }
            else
            {

                timer += Time.deltaTime;
            }

        }
    }


    private class stormSettings
    {
        public int chance = 50;

        // rate at which the storm activation will be checked when player is inside the trigger
        public float activationCheckInterval = 0.5f;
        public float minDuration = 10f;
        public float maxDuration = 20f;
    }

}
