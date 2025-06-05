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

        float linearValue = Mathf.InverseLerp(-80f, 20f, valueInDb);

        slider.value = linearValue;
        sliderText.text = valueInDb.ToString("0.0") + " dB";


        // Aggiunge listener
        slider.onValueChanged.AddListener(OnSliderChange);
    }

    public void OnSliderChange(float value)
    {
        float linearValue = slider.value; // tra 0 e 1
        float dB = Mathf.Lerp(-80f, 20f, linearValue);

        mixer.SetFloat(audioToManage, dB);

        // Aggiorna UI
        sliderText.text = dB.ToString("0.0") + " dB";

        // Salva valore (lineare) nei PlayerPrefs
        PlayerPrefs.SetFloat(prefsToUse, dB);
        PlayerPrefs.Save();
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderChange);
    }
}