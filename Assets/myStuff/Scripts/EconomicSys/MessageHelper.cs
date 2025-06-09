using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MessageHelper : MonoBehaviour
{
    // mantains a reference to the message text object
    TMP_Text messagetext;
    PlayerInput playerInput;
    public EffectsDispatcher dispatcher;
    public bool isMessageActive = false;

    // Singleton instance
    private static MessageHelper _instance;
    public static MessageHelper Instance
    {
        get
        {
            return _instance;
        }

        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
        }
    }

    void Awake()
    {
        Instance = this.gameObject.GetComponent<MessageHelper>();
    }
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

    Coroutine coroutine;
    public void PostMessage(string message)
    {
        messagetext.transform.parent.gameObject.SetActive(true);
        messagetext.text = message;
        isMessageActive = true;
        timer = 0f;

    }

    public void HideMessage()
    {
        messagetext.transform.parent.gameObject.SetActive(false);
        messagetext.text = "";
    }

    float timer = 0f;
    public void Update()
    {
        if (timer > 0.6f)
        {
            // se nessuno rinnova l'attivazione disattiva
            timer = 0f;
            HideMessage();
            return;
        }

        if (timer > 0.5)
        {
            // avvisa che il messaggio richiede rinnovo
            isMessageActive = false;
            return;
        }


        timer += Time.deltaTime;
    }

}
