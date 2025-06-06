using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OnSliderEnabled : MonoBehaviour
{
    [SerializeField] string dataToUse;
    [SerializeField] float defaultValue = -6f; // decibel di default (es. -6 dB)
    [SerializeField] TMP_Text label;
    [SerializeField] string thingToChange = "sfxVolume"; // il nome della chiave PlayerPrefs da usare
    [SerializeField] AudioMixer audio;
    [SerializeField] string volumeTochange = "sfxVolume"; // il nome del parametro dell'AudioMixer da cambiare
    [SerializeField] AudioClip testClip;

    void OnEnable()
    {

        Slider slider = GetComponent<Slider>();
        float dB = PlayerPrefs.GetFloat(dataToUse, defaultValue);



        // visualizza il valore in decibel (massimo due cigre decimali)
        slider.value = dB;
        label.text = $"{dB.ToString("0.00")} dB"; // aggiorna il testo del label con il nuovo valore in decibel
    }
    public void onSliderChanged(float value)
    {
        PlayerPrefs.SetFloat(thingToChange, value);
        label.text = $"{value.ToString("0.00")} dB"; // aggiorna il testo del label con il nuovo valore in decibel
        audio.SetFloat(volumeTochange, value); // aggiorna l'audio mixer con il nuovo valore
        if (testClip != null)
            AudioSource.PlayClipAtPoint(testClip, Camera.main.transform.position, value); // riproduce un suono di clic quando il valore del cursore cambia
        PlayerPrefs.Save();
    }
}