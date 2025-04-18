using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    private enum PanelState { Idle, Entering, Showing, Exiting }

    private PanelState currentState = PanelState.Idle;

    public string defaultName;
    public string defaultMessage;
    public GameObject panel;
    private Vector3 panelOriginalPos;
    public TMPro.TMP_Text panelName;
    public TMPro.TMP_Text panelDescription;
    public float distance = 300f;
    public float moveSpeed = 5f;
    public float displayTime = 2f;

    private Queue<(string, string)> messages = new Queue<(string, string)>();
    private Vector3 targetPosition;
    private float showTimer = 0f;

    void Start()
    {
        panel.SetActive(false);
        panelOriginalPos = panel.transform.localPosition;
        targetPosition = panelOriginalPos;
    }

    void Update()
    {
        switch (currentState)
        {
            case PanelState.Idle:
                if (messages.Count > 0)
                {
                    // Pronto per entrare
                    (string title, string desc) = messages.Dequeue();
                    panelName.text = title;
                    panelDescription.text = desc;
                    panel.SetActive(true);
                    targetPosition = panelOriginalPos - Vector3.right * distance;
                    currentState = PanelState.Entering;
                }
                break;

            case PanelState.Entering:
                panel.transform.localPosition = Vector3.Lerp(panel.transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(panel.transform.localPosition, targetPosition) < 0.1f)
                {
                    panel.transform.localPosition = targetPosition; // Snap
                    showTimer = displayTime;
                    currentState = PanelState.Showing;
                }
                break;

            case PanelState.Showing:
                showTimer -= Time.deltaTime;
                if (showTimer <= 0f)
                {
                    targetPosition = panelOriginalPos;
                    currentState = PanelState.Exiting;
                }
                break;

            case PanelState.Exiting:
                panel.transform.localPosition = Vector3.Lerp(panel.transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(panel.transform.localPosition, targetPosition) < 0.1f)
                {
                    panel.transform.localPosition = targetPosition;
                    panel.SetActive(false);
                    currentState = PanelState.Idle;
                }
                break;
        }
    }

    public void AppearPanel(string name, string message)
    {
        messages.Enqueue((name, message));
        Debug.Log("Requesting panel: " + messages.Count);
    }
}
