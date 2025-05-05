using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MappingEntry
{
    public string eventKey;
    public string targetObjectName;
    public string actionName;
}

public class CsvEventActionMap
{

    public static Dictionary<string, string> LoadFromFile(string csvFilePath)
    {
        // Load the CSV file from the Resources folder
        TextAsset csvFile = Resources.Load<TextAsset>(csvFilePath);
        List<MappingEntry> entries = ParseCsv(csvFile.text);
        Debug.Log($"Parsed {entries.Count} entries from CSV file.");
        var eventTable = new Dictionary<string, string>();
        foreach (var entry in entries)
        {
            // var targetObject = GameObject.Find(entry.targetObjectName);
            // if (targetObject == null)
            // {
            //     Debug.LogWarning($"Target object '{entry.targetObjectName}' not found.");
            //     continue;
            // }

            // var actionMethod = targetObject.GetType().GetMethod(entry.actionName);
            // if (actionMethod == null)
            // {
            //     Debug.LogWarning($"Action '{entry.actionName}' not found on {entry.targetObjectName}.");
            //     continue;
            // }

            // void action(object[] parameters) => actionMethod.Invoke(targetObject, parameters);
            // eventTable[entry.eventKey] = $"{entry.targetObjectName}.{entry.actionName}";
            eventTable[entry.eventKey] = entry.actionName; // Assuming action is a string for simplicity
        }
        return eventTable;
    }

    static List<MappingEntry> ParseCsv(string text)
    {
        var lines = text.Split('\n');
        var list = new List<MappingEntry>();
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 3) continue;
            list.Add(new MappingEntry {
                eventKey = cols[0],
                targetObjectName = cols[1],
                actionName = cols[2]
            });
        }
        return list;
    }
}
