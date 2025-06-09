using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UnlockPanelManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    List<(int, string, string, float)> itemlist = new();
    [SerializeField] TMP_Text creditsText;
    public Transform container;
    public TMP_Text credits;
    void OnEnable()
    {
        credits.text = PlayerPrefs.GetInt("astroCredits", 0).ToString();

        ItemManager.globalItemPool.Where(x => x.Value.locked)
            .ToList()
            .ForEach(x => itemlist.Add((x.Key, x.Value.name, x.Value.description, x.Value.astroCreditPrice)));

        foreach (var elem in ItemManager.globalItemPool.Values)
        {
            Debug.Log("item name " + elem.name);
        }

        foreach (var elem in itemlist)
        {
            Debug.Log("locked: name " + elem.Item2);
        }

        foreach (var elem in itemlist)
        {
            var unlockElem = Instantiate(Resources.Load<GameObject>("guiPrefabs/UnlockElem"), container);
            unlockElem.transform.GetChild(3).GetComponent<UnlockItemElem>().id = elem.Item1;
            unlockElem.transform.GetChild(3).GetComponent<UnlockItemElem>().price = (int)elem.Item4;
            unlockElem.transform.GetChild(3).GetComponent<UnlockItemElem>().creditsText = creditsText;

            unlockElem.transform.GetChild(0).GetComponent<TMP_Text>().text = elem.Item2;
            unlockElem.transform.GetChild(1).GetComponent<TMP_Text>().text = elem.Item3;
            unlockElem.transform.GetChild(2).GetComponent<TMP_Text>().text = elem.Item4.ToString();

        }
    }




}
