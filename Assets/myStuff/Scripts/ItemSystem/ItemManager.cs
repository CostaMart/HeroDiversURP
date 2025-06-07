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
[DefaultExecutionOrder(-100)]
public class ItemManager : MonoBehaviour
{


    private static ItemManager instance = null;
    public static ItemManager Instance
    {
        get
        {
            return Instance;
        }
    }

    public static Dictionary<string, int> statClassToIdRegistry;
    public static Dictionary<int, EnrichedModifier> globalItemPool = new Dictionary<int, EnrichedModifier>(); /// this contains all the items created by the game from the JSON file
    public static Dictionary<int, Modifier> bulletPool = new Dictionary<int, Modifier>();
    /// this contains all the items created by the game from the JSON file 

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

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
        bulletPool = ComputeAllBullets(Application.streamingAssetsPath + "/gameConfig/Bullets.json");

    }

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// This method is called at the start of the game to create the item pool reading items form the JSON file
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Dictionary<int, EnrichedModifier> ComputeAllItems(string path, bool isbullet)
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
        Dictionary<int, EnrichedModifier> itemsCollection = new Dictionary<int, EnrichedModifier>();


        try
        {
            foreach (var itemFromData in data.mods)  // per ogni item json
            {
                EnrichedModifier itemInCreation = new EnrichedModifier();
                var modifier = new Modifier
                {
                    effects = new List<AbstractEffect>()
                };

                itemInCreation.name = itemFromData.name;
                itemInCreation.id = itemFromData.id;
                itemInCreation.gameIconId = itemFromData.gameIconId;
                itemInCreation.inGamePrice = itemFromData.inGamePrice;
                itemInCreation.astroCreditPrice = itemFromData.astroCreditPrice;
                itemInCreation.description = itemFromData.description;
                modifier.bullet = isbullet;
                int effectID = 0;


                //check if this item is free or unlockable 
                if (itemFromData.requiresUnlocking) // item designed as locked, check if it has been unlocked by the player, 1 means unlocked
                    itemInCreation.locked = PlayerPrefs.GetInt($"unlocked: {itemInCreation.id}", 0) == 0 ? true : false;
                else itemInCreation.locked = false; // item designed as free, so it is not locked

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
    /// This method is called at the start of the game to create the item pool reading items form the JSON file
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Dictionary<int, Modifier> ComputeAllBullets(string path)
    {
        Debug.Log("ComputeAnItem called");

        // Leggi il JSON dal file
        // TODO: cambiare il path in modo che sia relativo al progetto
        string text = File.ReadAllText(path);

        // Deserializza il JSON in ItemJson, che contiene la proprietà item
        BulletJson data = JsonConvert.DeserializeObject<BulletJson>(text);

        // stadio molto prototipale, hardcoded la crezione di questo specifico tipo di effetto
        // ma i parametri sono presi dinamicaente dal file JSON
        // Accesso ai dati
        Dictionary<int, Modifier> bulletCollection = new Dictionary<int, Modifier>();


        try
        {
            foreach (var bulletFromData in data.buls)  // per ogni item json
            {
                BulletJson bulletInCreation = new();
                var modifier = new Modifier
                {
                    effects = new List<AbstractEffect>()
                };

                int effectID = 0;

                foreach (var effect in bulletFromData.effects) // per ogni effetto nella lista
                {
                    var type = effect["effectType"].ToString();
                    AbstractEffect newEffect = null;

                    switch (type)
                    {
                        case "sa":
                            newEffect = new SingleActivationEffect(effect, bulletFromData.id, effectID, true);
                            break;

                        case "ot":
                            newEffect = new OverTimeEffect(effect, bulletFromData.id, effectID, true);
                            break;
                    }

                    if (newEffect == null)
                        continue;

                    modifier.effects.Add(newEffect);
                    effectID++;
                }

                bulletCollection.Add(bulletFromData.id, modifier);
            }
        }

        catch (KeyNotFoundException e)
        {
            Debug.LogError("Error in Item manager unable to create an item with error: " + e.Message
            + " check the JSON item definition file" + e.StackTrace);

        }

        // create special items for system usages


        // mag consumption primarySecondary
        return bulletCollection;
    }


    /// <summary>
    /// given a pool index, it returns the item if the drop is successful, null otherwise.
    /// </summary>
    /// <param name="poolIndexes"></param>
    /// <returns></returns>
    public static EnrichedModifier DropFromPool(int[] indexes, int[] raritiesVals)
    {
        Random rand = new Random();
        var total = raritiesVals.Sum();
        var choosen = rand.Next(0, total);
        var rarityRange = 0;

        // la selezione del drop viene fatta solo sugli item non bloccati
        if (globalItemPool[indexes[0]].locked) return null;

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

    public static void UnlockItem(int itemId)
    {
        if (globalItemPool.ContainsKey(itemId))
        {
            globalItemPool[itemId].locked = false;
            PlayerPrefs.SetInt($"unlocked: {itemId}", 1); // set the item as unlocked
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError($"Item with ID {itemId} does not exist in the global item pool.");
        }
    }


    /// <summary>
    /// used just to deserializing the JSON file
    /// </summary>
    private class ItemJson
    {
        public List<ItemDeserializing> mods;
    }

    private class BulletJson
    {
        public List<BulletDeserializing> buls;
    }
    private class BulletDeserializing
    {
        public int id;
        public List<Dictionary<string, string>> effects;
    }

    /// <summary>
    /// used just to deserializing the JSON file
    /// </summary>
    private class ItemDeserializing
    {
        public int id;
        public string name;
        public float astroCreditPrice;
        public bool requiresUnlocking; // if true, the item is locked and must be unlocked by the player
        public int inGamePrice;
        public string description;
        public int gameIconId;
        public List<Dictionary<string, string>> effects;
    }

    /// <summary>
    /// this represents a modifer with supplementary information that can be incapsulted in an item.
    /// </summary>
    public class EnrichedModifier
    {
        public int id;
        public string name;
        public float astroCreditPrice;
        public bool locked;
        public int inGamePrice;
        public string description;
        public int gameIconId;
        public Modifier modifier;
    }


}


/// <summary>
/// represents a modifier, which is a collection of effects
/// can be applied to a target.
/// This class is used for pure modifers that will not be used as items.
/// e.g. climate change modifier
/// </summary>
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