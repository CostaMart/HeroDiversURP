using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class DialogueBoxMan : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    void Start()
    {
        dialogueBox.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueBox.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueBox.SetActive(false);
        }
    }

}
