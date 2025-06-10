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
    public bool allarmActive = false;
    public AudioClip alarmSound;

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
        if (allarmActive) return; // gli allarmi hanno maggiore priorità

        messagetext.transform.parent.gameObject.SetActive(true);
        messagetext.text = message;
        messagetext.color = Color.white;
        isMessageActive = true;
        timer = 0f;
    }

    public void PostAlarm(string message, float duration)
    {
        messagetext.transform.parent.gameObject.SetActive(true);
        messagetext.text = message;
        messagetext.color = Color.red;
        isMessageActive = true;
        timer = 0f;
        PostProcessor.Instance.audioSource.PlayOneShot(alarmSound);


        if (coroutine != null)
        {
            StopCoroutine(coroutine); // alarms can overwrite previous ones
        }

        coroutine = StartCoroutine(DisableAllarm(duration));
    }

    public IEnumerator DisableAllarm(float durartion)
    {
        yield return new WaitForSeconds(durartion);
        allarmActive = false;
        HideMessage();
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
