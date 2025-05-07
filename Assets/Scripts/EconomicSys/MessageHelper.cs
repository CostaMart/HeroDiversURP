using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MessageHelper : MonoBehaviour
{
    // mantains a reference to the message text object
    TMP_Text messagetext;
    PlayerInput playerInput;
    public EffectsDispatcher dispatcher;

    void Start()
    {
        messagetext = GameObject.Find("MessageText").GetComponent<TMP_Text>();
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
        dispatcher = GameObject.Find("Player").GetComponent<EffectsDispatcher>();


        if (messagetext == null)
        {
            Debug.LogError("MessageText not found in the scene.");
        }

        messagetext.transform.parent.gameObject.SetActive(false);
    }

    public void PostMessage(string message)
    {
        messagetext.text = message;
        messagetext.transform.parent.gameObject.SetActive(true);
    }

    public void HideMessage()
    {
        messagetext.transform.parent.gameObject.SetActive(false);
    }

}
