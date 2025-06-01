using System;
using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject startButton;
    public void Start()
    {
        // set once each volume
        PlayerPrefs.SetFloat("strafeBlur", 0.3f);
        PlayerPrefs.SetFloat("musicVolume", 0.8f);
        PlayerPrefs.SetFloat("sfxVolume", 0.6f);
        PlayerPrefs.SetFloat("shootingVolume", 0.2f);
        PlayerPrefs.Save();
    }
    public void OnButtonPressed()
    {
        settingsMenu.SetActive(true);
        startButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        this.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = false;
    }

    public void OnBlurSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("strafeBlur", value);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save();
    }
    public void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("sfxVolume", value);
        Debug.Log("SFX Volume set to: " + value);
        PlayerPrefs.Save();
    }
    public void OnShotVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("shootingVolume", value);
        Debug.Log("SFX Volume set to: " + value);
        PlayerPrefs.Save();
    }

    public void OnCloseSettings()
    {
        settingsMenu.SetActive(false);
        startButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        this.gameObject.GetComponent<UnityEngine.UI.Button>().interactable = true;
    }
}
