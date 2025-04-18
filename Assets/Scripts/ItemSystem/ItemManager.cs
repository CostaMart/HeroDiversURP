using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// This class converts the JSON file into a list of items
/// </summary>
public class ItemManager
{
    public int id;
    public List<AbstractEffect> effects;
    public static Dictionary<string, int> statClassToIdRegistry;
    public bool added = false;
    ItemIconsList list;


    public static Dictionary<int, Modifier> globalItemPool = new Dictionary<int, Modifier>(); /// this contains all the items created by the game from the JSON file
    public static Dictionary<int, Modifier> bulletPool = new Dictionary<int, Modifier>(); /// this contains all the items created by the game from the JSON file 

    static ItemManager()
    {
        // Initialize the statClass dictionary with some values
        statClassToIdRegistry = new Dictionary<string, int>
        {
            { "CharStats", 0},
            { "testUpdate", 1 },
            { "Ragdoller",  2 },
            {"PrimaryWeaponStats", 3},
            {"SecondaryWeaponStats", 4},
            {"BulletPoolStatsPrimary",5},
            {"BulletPoolStatsSecondary", 6},
            {"PhysicsStats", 7},
        };

        globalItemPool = ComputeAllItems(Application.streamingAssetsPath + "/gameConfig/ModList.json", false);

        Debug.Log("items compiled");

        bulletPool = ComputeAllItems(Application.streamingAssetsPath + "/gameConfig/Bullets.json", true);
    }

    /// <summary>
    /// This method is called at the start of the game to create the item pool reading items form the JSON file
    /// 
    /// TODO: in questa fase viene chiamato dal dispatcher per prova
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Dictionary<int, Modifier> ComputeAllItems(string path, bool isbullet)
    {
        Debug.Log("ComputeAnItem called");

        // Leggi il JSON dal file
        // TODO: cambiare il path in modo che sia relativo al progetto
        string text = File.ReadAllText(path);

        // Deserializza il JSON in ItemJson, che contiene la proprietà item
        ModifierJson data = JsonConvert.DeserializeObject<ModifierJson>(text);

        // stadio molto prototipale, hardcoded la crezione di questo specifico tipo di effetto
        // ma i parametri sono presi dinamicaente dal file JSON
        // Accesso ai dati
        Modifier i = null;
        Dictionary<int, Modifier> items = new Dictionary<int, Modifier>();


        try
        {
            foreach (var item in data.mods)  // per ogni item json
            {
                i = new Modifier
                {
                    effects = new List<AbstractEffect>()
                };

                i.name = item.name;
                i.id = item.id;
                i.gameIconId = item.gameIconId;
                i.inGamePrice = item.inGamePrice;
                i.description = item.description;
                i.bullet = isbullet;
                int effectID = 0;

                foreach (var effect in item.effects) // per ogni effetto nella lista
                {
                    var type = effect["effectType"].ToString();
                    AbstractEffect e = null;

                    switch (type)
                    {
                        case "sa":
                            e = new SingleActivationEffect(effect, item.id, effectID, isbullet);
                            break;

                        case "ot":
                            e = new OverTimeEffect(effect, item.id, effectID, isbullet);
                            break;

                        case "area":
                            e = new PermanentAreaEffect(effect, item.id, effectID, isbullet);
                            break;

                        default:
                            throw new Exception("Effect type object type: '" + type +
                             "' not recognized for item: " + item.id);
                    }

                    if (e == null)
                        continue;

                    i.effects.Add(e);
                    effectID++;
                    Debug.Log("ItemManager: effect added to item: " + i.id + " with id: " + effectID + "and name: " + i.name);
                }

                if (items.ContainsKey(i.id))
                {
                    throw new Exception("Item with ID " + i.id +
                    " already exists in the global pool. Skipping creation.");
                }
                else
                {
                    items.Add(i.id, i);
                }
            }
        }

        catch (KeyNotFoundException e)
        {
            Debug.LogError("Error in Item manager unable to create an item with error: " + e.Message
            + " check the JSON item definition file" + e.StackTrace);

        }

        // create special items for system usages

        // mag consumption primary
        var utilitymod = new Modifier();
        utilitymod.effects = new List<AbstractEffect>();
        utilitymod.name = "magConsumptionPrimary";


        utilitymod.effects.Add(new SingleActivationEffect(
                new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "effectName", "ammoConsumption" },
                    { "description", "consumes 1 ammo" },
                    { "inGamePrice", "0" },
                    { "gameIconId", "0" },
                    { "target","@PrimaryWeaponStats.1"},
                    {"expr","@PrimaryWeaponStats.1 - 1"}
                }, 0, 0, false));

        items.Add(-1, utilitymod);

        // mag consumption primarySecondary
        utilitymod = new Modifier();
        utilitymod.effects = new List<AbstractEffect>();
        utilitymod.name = "magConsumptionSecondary";

        utilitymod.effects.Add(new SingleActivationEffect(
                new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "effectName", "ammoConsumption" },
                    { "description", "consumes 1 ammo" },
                    { "inGamePrice", "0" },
                    { "gameIconId", "0" },
                    { "target","@SecondaryWeaponStats.1"},
                    {"expr","@SecondaryWeaponStats.1 - 1"}

                }, 0, 0, false));
        items.Add(-2, utilitymod);
        return items;
    }

    /// <summary>
    /// given a pool of indexes, it returns a random item from the pool
    /// </summary>
    /// <param name="poolIndexes"></param>
    /// <returns></returns>
    public static Modifier DropFromPool(int[] indexes, int[] raritiesVals)
    {
        Random rand = new Random();
        var total = raritiesVals.Sum();
        var choosen = rand.Next(0, total);
        var rarityRange = 0;

        for (int i = 0; i < raritiesVals.Length; i++)
        {
            rarityRange += raritiesVals[i];
            if (choosen >= rarityRange)
                continue;

            return globalItemPool[indexes[i]];
        }

        if (indexes == null)
            throw new Exception("ItemManager: Unable to select an item to drop, indexes are null");

        if (raritiesVals == null)
            throw new Exception("ItemManager: Unable to select an item to drop, rarities are null");

        throw new Exception("ItemManager: Unable to select an item to drop");
    }


    private class ModifierJson
    {
        public List<ModifierIncomplete> mods;
    }

    private class ModifierIncomplete
    {
        public int id;
        public string name;
        public int inGamePrice;
        public string description;
        public int gameIconId;
        public List<Dictionary<string, string>> effects;
    }
    public class Modifier
    {
        public int gameIconId;
        public int inGamePrice;
        public string description;
        public bool bullet;
        public string name;
        public int id;
        public List<AbstractEffect> effects;

        public override string ToString()
        {
            string s = "Modifier: \n";
            foreach (var effect in effects)
            {
                s += effect.ToString() + "\n";
            }
            return s;
        }
    }

}

