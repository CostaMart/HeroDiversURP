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


    public static Dictionary<int, Item> globalItemPool = new Dictionary<int, Item>(); /// this contains all the items created by the game from the JSON file
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
            {"HeatStats", 8}
        };

        globalItemPool = ComputeAllItems(Application.streamingAssetsPath + "/gameConfig/ModList.json", false);

        Debug.Log("items compiled");

    }

    /// <summary>
    /// This method is called at the start of the game to create the item pool reading items form the JSON file
    /// 
    /// TODO: in questa fase viene chiamato dal dispatcher per prova
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Dictionary<int, Item> ComputeAllItems(string path, bool isbullet)
    {
        Debug.Log("ComputeAnItem called");

        // Leggi il JSON dal file
        // TODO: cambiare il path in modo che sia relativo al progetto
        string text = File.ReadAllText(path);

        // Deserializza il JSON in ItemJson, che contiene la proprietà item
        ItemJson data = JsonConvert.DeserializeObject<ItemJson>(text);

        // stadio molto prototipale, hardcoded la crezione di questo specifico tipo di effetto
        // ma i parametri sono presi dinamicaente dal file JSON
        // Accesso ai dati
        Dictionary<int, Item> itemsCollection = new Dictionary<int, Item>();


        try
        {
            foreach (var itemFromData in data.mods)  // per ogni item json
            {
                Item itemInCreation = new Item();
                var modifier = new Modifier
                {
                    effects = new List<AbstractEffect>()
                };

                itemInCreation.name = itemFromData.name;
                itemInCreation.id = itemFromData.id;
                itemInCreation.gameIconId = itemFromData.gameIconId;
                itemInCreation.inGamePrice = itemFromData.inGamePrice;
                itemInCreation.description = itemFromData.description;
                modifier.bullet = isbullet;
                int effectID = 0;

                foreach (var effect in itemFromData.effects) // per ogni effetto nella lista
                {
                    var type = effect["effectType"].ToString();
                    AbstractEffect newEffect = null;

                    switch (type)
                    {
                        case "sa":
                            newEffect = new SingleActivationEffect(effect, itemFromData.id, effectID, isbullet);
                            break;

                        case "ot":
                            newEffect = new OverTimeEffect(effect, itemFromData.id, effectID, isbullet);
                            break;

                        case "area":
                            newEffect = new PermanentAreaEffect(effect, itemFromData.id, effectID, isbullet);
                            break;

                        default:
                            throw new Exception("Effect type object type: '" + type +
                             "' not recognized for item: " + itemFromData.id);
                    }

                    if (newEffect == null)
                        continue;

                    modifier.effects.Add(newEffect);
                    effectID++;
                }

                itemInCreation.modifier = modifier;

                if (itemsCollection.ContainsKey(itemInCreation.id))
                {
                    throw new Exception("Item with ID " + itemInCreation.id +
                    " already exists in the global pool. Skipping creation.");
                }
                else
                {
                    itemsCollection.Add(itemInCreation.id, itemInCreation);
                }
            }
        }

        catch (KeyNotFoundException e)
        {
            Debug.LogError("Error in Item manager unable to create an item with error: " + e.Message
            + " check the JSON item definition file" + e.StackTrace);

        }

        // create special items for system usages


        // mag consumption primarySecondary
        return itemsCollection;
    }

    /// <summary>
    /// given a pool of indexes, it returns a random item from the pool
    /// </summary>
    /// <param name="poolIndexes"></param>
    /// <returns></returns>
    public static Item DropFromPool(int[] indexes, int[] raritiesVals)
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


    private class ItemJson
    {
        public List<ItemDeserializing> mods;
    }

    private class ItemDeserializing
    {
        public int id;
        public string name;
        public bool isBullet;
        public int inGamePrice;
        public string description;
        public int gameIconId;
        public List<Dictionary<string, string>> effects;
    }

    public class Item
    {
        public int id;
        public string name;
        public int inGamePrice;
        public string description;
        public int gameIconId;
        public Modifier modifier;
    }

    public class Modifier
    {
        public bool bullet;
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

