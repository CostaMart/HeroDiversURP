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


    public static EffectsDispatcher playerDispatcher;


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
    public static Dictionary<int, DropPool> dropPools = new();
    private static Dictionary<int, Dictionary<int, Feature>> featuresSets = new();

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
        dropPools = ComputeDropPools();
        featuresSets = LoadFeatures();
        Debug.Log("Maro");

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
                itemInCreation.grabOnWalkIn = itemFromData.grabOnWalkIn;
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
    public static List<EnrichedModifier> DropFromPool(int id)
    {
        List<EnrichedModifier> itemsToDrop = new List<EnrichedModifier>();
        Random rand = new Random();
        var numberOfDrops = rand.Next(dropPools[id].minDrops, dropPools[id].maxDrops + 1);
        var items = dropPools[id].indexes.ToArray(); // get the items from the pool
        var rarities = (int[])dropPools[id].raritiesVal.Clone();

        for (int i = 0; i < numberOfDrops; i++)
        {


            var totalRarity = rarities.Sum();
            var normalizeRarities = rarities.Select(r => (float)r / totalRarity).ToArray();



            var selected = rand.NextDouble();
            var lowerBound = 0f;
            var upperbound = 0f;
            int index = 0;

            foreach (var prob in normalizeRarities)
            {
                upperbound = upperbound + prob;

                if (selected <= upperbound && selected >= lowerBound)
                {

                    if (items[index] == -1) break;

                    itemsToDrop.Add(globalItemPool[items[index]]);

                    if (dropPools[id].withRipetition == false)
                    {
                        var momList = items.ToList();
                        momList.RemoveAt(index);
                        items = momList.ToArray(); // set the item to -1 to avoid duplicates
                        var momRarities = rarities.ToList();
                        momRarities.RemoveAt(index);
                        rarities = momRarities.ToArray(); // remove the rarity of the item from the pool to avoid duplicates
                    }

                    break;
                }

                index++;
                lowerBound += prob;
            }
        }

        return itemsToDrop;
    }

    /// <summary>
    ///  returns a deep copy of a features set by hash.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static Dictionary<int, Feature> GetFeaturesByHash(int hash)
    {
        Dictionary<int, Feature> newFeatures = new();
        Dictionary<int, Feature> old = featuresSets[hash];
        foreach (var feature in old)
        {
            newFeatures.Add(feature.Key, feature.Value.Clone());
        }

        return newFeatures;
    }
    public Dictionary<int, Dictionary<int, Feature>> LoadFeatures()
    {
        string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/gameConfig/Features.txt");
        bool found = false;
        bool hasBeenFound = false;
        Dictionary<int, Dictionary<int, Feature>> featuresDict = new();
        Dictionary<int, Feature> features = new();
        int hash = 0;

        foreach (var thisLine in lines)
        {
            var line = thisLine.Trim();
            line = line.Split("//")[0];

            if (found && line.Contains("##"))
            {
                featuresDict.Add(hash, features);
                features = new();
                found = false;
            }

            if (found)
            {
                try
                {
                    string[] parts = line.Split("=");
                    FeatureType featureType = (FeatureType)Enum.Parse(typeof(FeatureType), parts[0]);

                    if (featureType == FeatureType.money || featureType == FeatureType.keys)
                    {
                        Debug.LogWarning("AbstractStatus: money and keys are special features, not customizable");
                        continue;
                    }

                    parts = parts[1].Split("ID:");
                    string value = parts[0].Trim();
                    int ID = int.Parse(parts[1].Split("range:")[0].Trim());

                    string rangeString = (parts[1].Split("range:").Length > 1) ? parts[1].Split("range:")[1].Trim() : null;
                    string maxVal = null, minVal = null;

                    if (rangeString != null)
                    {
                        maxVal = rangeString.Split("-")[1].Trim();
                        minVal = rangeString.Split("-")[0].Trim();
                    }

                    if (int.TryParse(value, out _))
                    {
                        Feature f = new Feature(featureType, int.Parse(value), typeof(int));
                        f.SetValue(int.Parse(value));
                        if (maxVal != null)
                        {
                            f.maxVal = int.Parse(maxVal);
                            f.minVal = int.Parse(minVal);
                        }
                        features.Add(ID, f);
                    }
                    else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                    {
                        Feature f = new Feature(featureType, float.Parse(value, System.Globalization.CultureInfo.InvariantCulture), typeof(float));
                        f.SetValue(float.Parse(value.Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture));
                        if (maxVal != null)
                        {
                            f.maxVal = float.Parse(maxVal, System.Globalization.CultureInfo.InvariantCulture);
                            f.minVal = float.Parse(minVal, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        features.Add(ID, f);
                    }
                    else
                    {
                        Feature f = new Feature(featureType, bool.Parse(value), typeof(bool));
                        f.SetValue(bool.Parse(value));
                        features.Add(ID, f);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Abstract Status: Error in parsing line: " + line + " " + e.Message);
                }
            }

            if (line.Contains("#>"))
            {
                hash = line.Split('>')[1].Trim().ToLower().GetHashCode();
                found = true;
                hasBeenFound = true;
            }
        }

        return featuresDict;
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

    public static Dictionary<int, DropPool> ComputeDropPools()
    {
        string lines = File.ReadAllText(Application.streamingAssetsPath + "/gameConfig/ItemPools.json");
        Dictionary<int, DropPool> dropPools = JsonConvert.DeserializeObject<Dictionary<int, DropPool>>(lines);
        return dropPools;
    }
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
        public bool grabOnWalkIn;
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
        public bool grabOnWalkIn = false;
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

public class DropPool
{
    public int[] indexes;
    public int[] raritiesVal;

    public int minDrops;
    public int maxDrops;
    public bool withRipetition = false; // if true, the same item can be dropped multiple times
}