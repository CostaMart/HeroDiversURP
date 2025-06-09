using System;
using TMPro;
using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject unlockPanel;
    [SerializeField] private TMP_Text creditsText;

    public void Start()
    {
        creditsText.text = PlayerPrefs.GetInt("astroCredits", 0).ToString();

    }
    public void OnButtonPressed()
    {
        if (!settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(true);
            startButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            this.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
        else
        {
            settingsMenu.SetActive(false);
            startButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
            this.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
    }



    public void OnSavePress()
    {
        settingsMenu.SetActive(false);
        startButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        this.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
    }



}
