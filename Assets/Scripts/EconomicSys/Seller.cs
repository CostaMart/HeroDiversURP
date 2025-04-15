using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static ItemManager;
using static Unity.Cinemachine.CinemachineSplineDollyLookAtTargets;

public class Seller : MonoBehaviour
{
    List<int> itIds;
    List<int> rarities;
    (int, int) itemNumRange = (1, 1);
    Transform transform;
    [SerializeField] private EconomyManager economyManager;
    float offset = 0f;
    int rigtOrLeft = 1;
    List<Modifier> it;
    void Start()
    {
        ReadSellingPool();
        var itemNum = Random.Range(itemNumRange.Item1, itemNumRange.Item2);
        it = new List<Modifier>();

        for (var x = 0; x < itemNum; x++)
        {
            it.Add(ItemManager.DropFromPool(itIds.ToArray(), rarities.ToArray()));
            Place(it[x]);
        }

    }
    private void Place(Modifier it)
    {
        Vector3 myOffset = new(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        myOffset.x += this.offset;



        GameObject prefab = Resources.Load("prefabs/ItemContainer") as GameObject;
        GameObject container = Instantiate(prefab, myOffset, Quaternion.identity);

        Grabbable g = container.transform.GetChild(0).GetComponent<Grabbable>();
        g.item = it;
        g.selling = true;
        g.economyManager = economyManager;

        this.offset += 3f * rigtOrLeft;
        rigtOrLeft = rigtOrLeft * -1;

    }
    private void ReadSellingPool()
    {
        string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/gameConfig/ItemPools.txt");
        string name = gameObject.name;

        bool found = false;
        List<int> fixedDropPool = new();
        List<int> myPool = new();
        List<int> ratrities = new();

        foreach (var line in lines)
        {
            /// found pool start
            if (line.Contains(name))
            {
                found = true;
                var rangeS = line.Split(' ')[1];
                itemNumRange.Item1 = int.Parse(rangeS.Split('-')[0]);
                itemNumRange.Item2 = int.Parse(rangeS.Split('-')[1]);
                continue;
            }

            /// pool completely loaded
            if (line.Contains("##") && found)
                break;

            if (found)
            {
                var item = (0, 0);

                //Debug.Log("Dropper: itempool item with ID: " +
                //int.Parse(line.Split("rarity: ")[0].Split(' ')[0] + " added in object " + name));
                if (line.Contains("fixed"))
                {
                    fixedDropPool.Add(int.Parse(line.Split("rarity: ")[0].Split(' ')[0]));
                    continue;
                }

                myPool.Add(int.Parse(line.Split("rarity: ")[0].Split(' ')[0]));
                ratrities.Add(int.Parse(line.Split("rarity: ")[1]));
            }

        }

        itIds = myPool.ToList();
        rarities = ratrities.ToList();
    }
}
