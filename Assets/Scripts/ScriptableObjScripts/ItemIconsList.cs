using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemIconsList", menuName = "ItemSys/ItemIconsList")]
public class ItemIconsList : ScriptableObject

{
    [SerializeField] public List<GameObject> itemsList = new();
}
