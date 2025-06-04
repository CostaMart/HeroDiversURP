using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class DialogueBoxMan : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    List<string> diaogueList = new();
    [SerializeField] private TMP_Text text;
    private bool encountered = false;
    void Start()
    {
        dialogueBox.SetActive(false);

        // read dialogues from dialogue file
        FileInfo file = new FileInfo(Application.streamingAssetsPath + $"/gameConfig/dialogues/{gameObject.name}.txt");
        string[] lines = File.ReadAllLines(file.FullName);
        foreach (string line in lines)
        {
            diaogueList.Add(line);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (encountered)
                return;

            encountered = true;
            text.text = diaogueList[Random.Range(0, diaogueList.Count)];
            dialogueBox.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            encountered = false;
            dialogueBox.SetActive(false);
        }
    }

}
