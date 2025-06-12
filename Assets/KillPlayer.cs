using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;


public class KillPlayer : MonoBehaviour
{
    Modifier killModifier;
    void Start()
    {
        var mod = JsonConvert.DeserializeObject<ModLoader>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "gameConfig/OutOfMap.json")));
        var effects = new List<AbstractEffect>();

        foreach (var effect in mod.effects)
        {
            effects.Add(new SingleActivationEffect(new Dictionary<string, string>
            {
                { "effectType", effect.effectType },
                { "target", effect.target },
                { "expr", effect.expr }
            }, 0, 0, false));
        }

        killModifier = new Modifier
        {
            effects = effects
        };

    }



    float time = 0f;
    bool active = false;
    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            time += Time.deltaTime;
            if (time > 0.5f)
            {
                time = 0f;
                ItemManager.playerDispatcher.modifierDispatch(killModifier);
                if (active) return;
                active = true;
                MessageHelper.Instance.PostAlarm("Are you trying to escape your mission?", 5);
                Debug.Log("Player killed by " + gameObject.name);

            }
        }
        else
        {
            time = 0f;
        }

    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            time = 0f;
            active = false;
        }
    }
    public class ModLoader
    {
        public List<Effects> effects;

    };
    public class Effects
    {
        public string effectType;
        public string target;
        public string expr;
    }

}
