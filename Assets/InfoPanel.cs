using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    bool panelVisible = false;
    public string name;
    public string message;
    public GameObject panel;
    Vector3 paneOriginalPos;
    public TMPro.TMP_Text panelName;
    public TMPro.TMP_Text panelDescription;
    public float distance;
    bool panelActive = false;
    bool coroutine = false;
    Queue<(string, string)> messages = new Queue<(string, string)>();
    void Start()
    {
        panel.SetActive(false);
        paneOriginalPos = panel.transform.localPosition;
    }
    void Update()
    {
        while (messages.Count > 0)
        {

            if (panelActive)
            {
                panelName.text = messages.Peek().Item1;
                panelDescription.text = messages.Dequeue().Item2;

                panel.SetActive(true);
                panel.transform.localPosition = paneOriginalPos;


                // Calculate target position (10 units to the right from original position)
                Vector3 targetPosition = paneOriginalPos - Vector3.right * distance;

                // Smoothly move the panel toward the target position
                float moveSpeed = 5f * Time.deltaTime; // Adjust speed as needed
                panel.transform.localPosition = Vector3.Lerp(panel.transform.localPosition, targetPosition, moveSpeed);

                if (panel.transform.localPosition == targetPosition)
                {
                    StartCoroutine("DisapearPanel");
                    coroutine = true;
                }
            }
            else
            {

                panelName.text = name;
                panelDescription.text = message;


                // Smoothly move the panel toward the target position
                float moveSpeed = 5f * Time.deltaTime; // Adjust speed as needed
                panel.transform.localPosition = Vector3.Lerp(panel.transform.localPosition, paneOriginalPos, moveSpeed);

                if (panel.transform.localPosition == paneOriginalPos)
                {
                    panel.SetActive(false);
                    panelActive = true;
                }

            }

        }

    }

    public void AppearPanel(string name, string message)
    {
        messages.Enqueue((name, message));

        Debug.Log("infoPanel: " + name + " " + message);
        Debug.Log("infoPanel: " + messages.Count);
        panelActive = true;
    }

    public IEnumerator DisapearPanel()
    {
        yield return new WaitForSeconds(2f);
        panelActive = false;
        coroutine = false;
    }

}
