using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class AudioSliderControl : MonoBehaviour
{
    Slider slider;
    TMP_Text sliderText;
    public AudioMixer mixer;
    public string audioToManage; // es: "MasterVolume"
    public string prefsToUse;    // es: "Volume_Master"

    void Awake()
    {
        slider = GetComponent<Slider>();
        sliderText = transform.GetChild(0).GetComponent<TMP_Text>();
    }

    void OnEnable()
    {

        // Recupera valore lineare da PlayerPrefs (default = 1.0)
        float valueInDb = PlayerPrefs.GetFloat(prefsToUse, 1f);
        slider.value = valueInDb;
        sliderText.text = valueInDb.ToString("0.0") + " dB";
        // Aggiunge listener
        slider.onValueChanged.AddListener(OnSliderChange);
    }

    public void OnSliderChange(float value)
    {
        mixer.SetFloat(audioToManage, value);

        // Aggiorna UI
        sliderText.text = value.ToString("0.0") + " dB";

        // Salva valore (lineare) nei PlayerPrefs
        PlayerPrefs.SetFloat(prefsToUse, value);
        PlayerPrefs.Save();
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderChange);
    }
}