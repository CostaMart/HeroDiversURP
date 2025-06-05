using UnityEngine;
using UnityEngine.UI;

public class OnSliderEnabled : MonoBehaviour
{
    [SerializeField] string dataToUse;
    [SerializeField] float defaultValue = -6f; // decibel di default (es. -6 dB)

    void Start()
    {
        Slider slider = GetComponent<Slider>();
        float dB = PlayerPrefs.GetFloat(dataToUse, defaultValue);

        // Converti da decibel a valore lineare per lo slider
        float linearValue = Mathf.Pow(10f, dB / 20f);

        // Clamp tra 0 e 1 per sicurezza
        linearValue = Mathf.Clamp01(linearValue);

        slider.value = linearValue;

        Debug.Log($"Decibel: {dB} dB | Valore slider: {linearValue}");
    }
}